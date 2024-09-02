/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class InvitationBusinessLogicTests
{
    private static readonly Guid ApplicationId = Guid.NewGuid();
    private static readonly Guid CompanyId = Guid.NewGuid();
    private readonly IFixture _fixture;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly ICompanyInvitationRepository _companyInvitationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly InvitationBusinessLogic _sut;

    public InvitationBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.ConfigureFixture();

        _portalRepositories = A.Fake<IPortalRepositories>();
        _processStepRepository = A.Fake<IProcessStepRepository>();
        _companyInvitationRepository = A.Fake<ICompanyInvitationRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _applicationRepository = A.Fake<IApplicationRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyInvitationRepository>()).Returns(_companyInvitationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);

        _sut = new InvitationBusinessLogic(_portalRepositories);
    }

    #region ExecuteInvitation

    [Fact]
    public async Task ExecuteInvitation_WithoutEmail_ThrowsControllerArgumentException()
    {
        var invitationData = _fixture.Build<CompanyInvitationData>()
            .With(x => x.OrganisationName, _fixture.Create<string>())
            .WithNamePattern(x => x.FirstName)
            .WithNamePattern(x => x.LastName)
            .With(x => x.Email, (string?)null)
            .Create();

        async Task Act() => await _sut.ExecuteInvitation(invitationData);

        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("email must not be empty (Parameter 'email')");
        ex.ParamName.Should().Be("email");
    }

    [Fact]
    public async Task ExecuteInvitation_WithValidData_CreatesExpected()
    {
        var processes = new List<Process>();
        var processSteps = new List<ProcessStep>();
        var invitations = new List<CompanyInvitation>();

        SetupFakesForInvite(processes, processSteps, invitations);

        var invitationData = _fixture.Build<CompanyInvitationData>()
            .WithOrgNamePattern(x => x.OrganisationName)
            .With(x => x.UserName, "testUserName")
            .WithNamePattern(x => x.FirstName)
            .WithNamePattern(x => x.LastName)
            .WithEmailPattern(x => x.Email)
            .Create();

        var result = await _sut.ExecuteInvitation(invitationData);

        processes.Should().ContainSingle().And.Satisfy(x => x.ProcessTypeId == ProcessTypeId.INVITATION);
        processSteps.Should().ContainSingle().And.Satisfy(x => x.ProcessStepTypeId == ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP && x.ProcessStepStatusId == ProcessStepStatusId.TODO);
        invitations.Should().ContainSingle().And.Satisfy(x => x.ProcessId == processes.Single().Id && x.UserName == "testUserName" && x.ApplicationId == ApplicationId);
        A.CallTo(() => _companyRepository.CreateCompany(A<string>._, null)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationRepository.CreateCompanyApplication(CompanyId, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.INTERNAL, null)).MustHaveHappenedOnceExactly();
        result.Should().Be(new CompanyInvitationResponse(ApplicationId, CompanyId));
    }

    [Theory]
    [InlineData("ValidOrganisationName123")]
    [InlineData("Organisation Name")]
    [InlineData("Organisation$Name")]
    [InlineData("Organisation\\Name")]
    [InlineData("Organisation/Name")]
    [InlineData("Organisation<Name>")]
    [InlineData("Organisation Name!")]
    [InlineData("7-ELEVEN INTERNATIONAL LLC")]
    [InlineData("C")]
    [InlineData("+SEN Inc.")]
    [InlineData("Double \"Quote\" Company S.A.")]
    [InlineData("Special Characters ^&%#@*/_-\\")]
    [InlineData("German: ÄÖÜß")]
    [InlineData("Icelandic: ÆÐÞ")]
    public async Task ExecuteInvitation_WithValidOrganisationName_DoesNotThrowException(string validName)
    {
        var invitationData = _fixture.Build<CompanyInvitationData>()
            .With(x => x.OrganisationName, validName)
            .Create();

        Func<Task> Act = async () => await _sut.ExecuteInvitation(invitationData);

        await Act.Should().NotThrowAsync<ControllerArgumentException>();
    }

    [Theory]
    [InlineData(null)] // null value
    [InlineData("Organisation Name ")] // Ends with whitespace
    [InlineData("1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWX")] // Exceeds 160 characters
    public async Task ExecuteInvitation_WithInvalidOrganisationName_ThrowsControllerArgumentException(string? invalidName)
    {
        var invitationData = _fixture.Build<CompanyInvitationData>()
            .With(x => x.OrganisationName, invalidName)
            .Create();

        async Task Act() => await _sut.ExecuteInvitation(invitationData);

        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(ValidationExpressionErrors.INCORRECT_COMPANY_NAME.ToString());
    }

    #endregion

    #region RetriggerCreateCentralIdp

    [Fact]
    public async Task RetriggerCreateCentralIdp_CallsExpected()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_CENTRAL_IDP;
        var processStepTypeId = ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP;
        var processSteps = new List<ProcessStep>();
        var process = _fixture.Build<Process>().With(x => x.LockExpiryDate, default(DateTimeOffset?)).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerCreateCentralIdp(process.Id);

        // Assert
        processSteps.Should().ContainSingle().And.Satisfy(x => x.ProcessStepTypeId == processStepTypeId);
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCreateCentralIdp_WithNotExistingProcess_ThrowsException()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_USER;
        var process = _fixture.Create<Process>();
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((false, _fixture.Create<VerifyProcessData>()));
        async Task Act() => await _sut.RetriggerCreateCentralIdp(process.Id);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be($"process {process.Id} does not exist");
    }

    #endregion

    #region RetriggerCreateSharedIdpServiceAccount

    [Fact]
    public async Task RetriggerCreateSharedIdpServiceAccount_CallsExpected()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT;
        var processStepTypeId = ProcessStepTypeId.INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT;
        var processSteps = new List<ProcessStep>();
        var process = _fixture.Build<Process>().With(x => x.LockExpiryDate, default(DateTimeOffset?)).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerCreateSharedIdpServiceAccount(process.Id);

        // Assert
        processSteps.Should().ContainSingle().And.Satisfy(x => x.ProcessStepTypeId == processStepTypeId);
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCreateSharedIdpServiceAccount_WithNotExistingProcess_ThrowsException()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT;
        var process = _fixture.Create<Process>();
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((false, _fixture.Create<VerifyProcessData>()));
        async Task Act() => await _sut.RetriggerCreateDatabaseIdp(process.Id);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be($"process {process.Id} does not exist");
    }

    #endregion

    #region RetriggerUpdateCentralIdpUrls

    [Fact]
    public async Task RetriggerUpdateCentralIdpUrls_CallsExpected()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_UPDATE_CENTRAL_IDP_URLS;
        var processStepTypeId = ProcessStepTypeId.INVITATION_UPDATE_CENTRAL_IDP_URLS;
        var processSteps = new List<ProcessStep>();
        var process = _fixture.Build<Process>().With(x => x.LockExpiryDate, default(DateTimeOffset?)).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerUpdateCentralIdpUrls(process.Id);

        // Assert
        processSteps.Should().ContainSingle().And.Satisfy(x => x.ProcessStepTypeId == processStepTypeId);
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerUpdateCentralIdpUrls_WithNotExistingProcess_ThrowsException()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_UPDATE_CENTRAL_IDP_URLS;
        var process = _fixture.Create<Process>();
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((false, _fixture.Create<VerifyProcessData>()));
        async Task Act() => await _sut.RetriggerCreateDatabaseIdp(process.Id);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be($"process {process.Id} does not exist");
    }

    #endregion

    #region RetriggerCreateCentralIdpOrgMapper

    [Fact]
    public async Task RetriggerCreateCentralIdpOrgMapper_CallsExpected()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER;
        var processStepTypeId = ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER;
        var processSteps = new List<ProcessStep>();
        var process = _fixture.Build<Process>().With(x => x.LockExpiryDate, default(DateTimeOffset?)).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerCreateCentralIdpOrgMapper(process.Id);

        // Assert
        processSteps.Should().ContainSingle().And.Satisfy(x => x.ProcessStepTypeId == processStepTypeId);
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCreateCentralIdpOrgMapper_WithNotExistingProcess_ThrowsException()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER;
        var process = _fixture.Create<Process>();
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((false, _fixture.Create<VerifyProcessData>()));
        async Task Act() => await _sut.RetriggerCreateCentralIdpOrgMapper(process.Id);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be($"process {process.Id} does not exist");
    }

    #endregion

    #region RetriggerCreateCentralIdpOrgMapper

    [Fact]
    public async Task RetriggerCreateSharedRealmIdpClient_CallsExpected()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_SHARED_REALM;
        var processStepTypeId = ProcessStepTypeId.INVITATION_CREATE_SHARED_REALM;
        var processSteps = new List<ProcessStep>();
        var process = _fixture.Build<Process>().With(x => x.LockExpiryDate, default(DateTimeOffset?)).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerCreateSharedRealmIdpClient(process.Id);

        // Assert
        processSteps.Should().ContainSingle().And.Satisfy(x => x.ProcessStepTypeId == processStepTypeId);
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCreateSharedRealmIdpClient_WithNotExistingProcess_ThrowsException()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_SHARED_REALM;
        var process = _fixture.Create<Process>();
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((false, _fixture.Create<VerifyProcessData>()));
        async Task Act() => await _sut.RetriggerCreateSharedRealmIdpClient(process.Id);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be($"process {process.Id} does not exist");
    }

    #endregion

    #region RetriggerCreateCentralIdpOrgMapper

    [Fact]
    public async Task RetriggerEnableCentralIdp_CallsExpected()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_ENABLE_CENTRAL_IDP;
        var processStepTypeId = ProcessStepTypeId.INVITATION_ENABLE_CENTRAL_IDP;
        var processSteps = new List<ProcessStep>();
        var process = _fixture.Build<Process>().With(x => x.LockExpiryDate, default(DateTimeOffset?)).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerEnableCentralIdp(process.Id);

        // Assert
        processSteps.Should().ContainSingle().And.Satisfy(x => x.ProcessStepTypeId == processStepTypeId);
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerEnableCentralIdp_WithNotExistingProcess_ThrowsException()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_ENABLE_CENTRAL_IDP;
        var process = _fixture.Create<Process>();
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((false, _fixture.Create<VerifyProcessData>()));
        async Task Act() => await _sut.RetriggerEnableCentralIdp(process.Id);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be($"process {process.Id} does not exist");
    }

    #endregion

    #region RetriggerCreateDatabaseIdp

    [Fact]
    public async Task RetriggerCreateDatabaseIdp_CallsExpected()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_DATABASE_IDP;
        var processStepTypeId = ProcessStepTypeId.INVITATION_CREATE_DATABASE_IDP;
        var processSteps = new List<ProcessStep>();
        var process = _fixture.Build<Process>().With(x => x.LockExpiryDate, default(DateTimeOffset?)).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerCreateDatabaseIdp(process.Id);

        // Assert
        processSteps.Should().ContainSingle().And.Satisfy(x => x.ProcessStepTypeId == processStepTypeId);
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCreateDatabaseIdp_WithNotExistingProcess_ThrowsException()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_DATABASE_IDP;
        var process = _fixture.Create<Process>();
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((false, _fixture.Create<VerifyProcessData>()));
        async Task Act() => await _sut.RetriggerCreateDatabaseIdp(process.Id);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be($"process {process.Id} does not exist");
    }

    #endregion

    #region RetriggerInvitationCreateUser

    [Fact]
    public async Task RetriggerInvitationCreateUser_CallsExpected()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_USER;
        var processStepTypeId = ProcessStepTypeId.INVITATION_CREATE_USER;
        var processSteps = new List<ProcessStep>();
        var process = _fixture.Build<Process>().With<DateTimeOffset?>(x => x.LockExpiryDate, default(DateTimeOffset?)).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerInvitationCreateUser(process.Id);

        // Assert
        processSteps.Should().ContainSingle().And.Satisfy(x => x.ProcessStepTypeId == processStepTypeId);
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerInvitationCreateUser_WithNotExistingProcess_ThrowsException()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_USER;
        var process = _fixture.Create<Process>();
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((false, _fixture.Create<VerifyProcessData>()));
        async Task Act() => await _sut.RetriggerInvitationCreateUser(process.Id);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be($"process {process.Id} does not exist");
    }

    #endregion

    #region Setup

    private void SetupFakesForInvite(List<Process> processes, List<ProcessStep> processSteps, List<CompanyInvitation> invitations)
    {
        var company = new Company(CompanyId, "testOrg", CompanyStatusId.PENDING, DateTimeOffset.UtcNow);
        var createdProcessId = Guid.NewGuid();
        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.INVITATION))
            .Invokes((ProcessTypeId processTypeId) =>
            {
                var process = new Process(createdProcessId, processTypeId, Guid.NewGuid());
                processes.Add(process);
            })
            .Returns(new Process(createdProcessId, ProcessTypeId.INVITATION, Guid.NewGuid()));
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP, ProcessStepStatusId.TODO, createdProcessId))
            .Invokes((ProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid processId) =>
            {
                var processStep = new ProcessStep(Guid.NewGuid(), processStepTypeId, processStepStatusId, processId,
                    DateTimeOffset.UtcNow);
                processSteps.Add(processStep);
            });

        A.CallTo(() => _companyInvitationRepository.CreateCompanyInvitation(A<string>._, A<string>._, A<string>._, A<string>._, createdProcessId, A<Action<CompanyInvitation>>._))
            .Invokes((string firstName, string lastName, string email, string organisationName, Guid processId, Action<CompanyInvitation>? setOptionalFields) =>
            {
                var entity = new CompanyInvitation(Guid.NewGuid(), firstName, lastName, email, organisationName, processId);
                setOptionalFields?.Invoke(entity);
                invitations.Add(entity);
            });
        A.CallTo(() => _companyRepository.CreateCompany(A<string>._, A<Action<Company>>._)).Returns(company);
        A.CallTo(() => _applicationRepository.CreateCompanyApplication(company.Id, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.INTERNAL, null))
            .Returns(new CompanyApplication(ApplicationId, company.Id, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.INTERNAL, DateTimeOffset.UtcNow));
    }

    private void SetupFakesForRetrigger(List<ProcessStep> processSteps)
    {
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)>>._))
            .Invokes((IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> processStepTypeStatus) =>
                {
                    processSteps.AddRange(processStepTypeStatus.Select(x => new ProcessStep(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)).ToList());
                });
    }

    #endregion
}
