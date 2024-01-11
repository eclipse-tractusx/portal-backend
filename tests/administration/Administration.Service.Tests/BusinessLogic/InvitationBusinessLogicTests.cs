/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class InvitationBusinessLogicTests
{
    private readonly IFixture _fixture;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityProviderRepository _identityProviderRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IMailingInformationRepository _mailingInformationRepository;
    private readonly IOptions<InvitationSettings> _options;
    private readonly ICompanyInvitationRepository _companyInvitationRepository;
    private readonly string _companyName;
    private readonly string _idpName;
    private readonly Guid _companyId;
    private readonly Guid _identityProviderId;
    private readonly Guid _applicationId;
    private readonly Func<UserCreationRoleDataIdpInfo, (Guid CompanyUserId, string UserName, string? Password, Exception? Error)> _processLine;
    private readonly Exception _error;
    private readonly InvitationBusinessLogic _sut;

    public InvitationBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _provisioningManager = A.Fake<IProvisioningManager>();
        _userProvisioningService = A.Fake<IUserProvisioningService>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _mailingInformationRepository = A.Fake<IMailingInformationRepository>();
        _options = A.Fake<IOptions<InvitationSettings>>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _processStepRepository = A.Fake<IProcessStepRepository>();
        _companyInvitationRepository = A.Fake<ICompanyInvitationRepository>();

        _companyName = "testCompany";
        _idpName = _fixture.Create<string>();
        _companyId = _fixture.Create<Guid>();
        _identityProviderId = _fixture.Create<Guid>();
        _applicationId = _fixture.Create<Guid>();
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyInvitationRepository>()).Returns(_companyInvitationRepository);

        _processLine = A.Fake<Func<UserCreationRoleDataIdpInfo, (Guid CompanyUserId, string UserName, string? Password, Exception? Error)>>();

        _error = _fixture.Create<TestException>();

        _sut = new InvitationBusinessLogic(
            _provisioningManager,
            _userProvisioningService,
            _portalRepositories,
            _options);
    }

    #region ExecuteInvitation

    [Fact]
    public async Task TestExecuteInvitationSuccess()
    {
        var processes = new List<Process>();
        var processSteps = new List<ProcessStep>();
        var invitations = new List<CompanyInvitation>();
        SetupFakesForInvite(processes, processSteps, invitations);

        var invitationData = _fixture.Build<CompanyInvitationData>()
            .With(x => x.OrganisationName, _companyName)
            .WithNamePattern(x => x.FirstName)
            .WithNamePattern(x => x.LastName)
            .WithEmailPattern(x => x.Email)
            .Create();

        var sut = new InvitationBusinessLogic(
            _provisioningManager,
            _userProvisioningService,
            _portalRepositories,
            _options);

        await sut.ExecuteInvitation(invitationData).ConfigureAwait(false);

        A.CallTo(() => _provisioningManager.GetNextCentralIdentityProviderNameAsync()).MustHaveHappened();
        // TODO PS - A.CallTo(() => _provisioningManager.SetupSharedIdpAsync(A<string>.That.IsEqualTo(_idpName), A<string>.That.IsEqualTo(invitationData.OrganisationName), A<string?>._)).MustHaveHappened();

        A.CallTo(() => _companyRepository.CreateCompany(A<string>.That.IsEqualTo(invitationData.OrganisationName), null)).MustHaveHappened();
        A.CallTo(() => _identityProviderRepository.CreateIdentityProvider(IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.SHARED, A<Guid>._, A<Action<IdentityProvider>>._)).MustHaveHappened();
        A.CallTo(() => _identityProviderRepository.CreateIamIdentityProvider(A<Guid>._, _idpName)).MustHaveHappened();
        A.CallTo(() => _applicationRepository.CreateCompanyApplication(_companyId, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.INTERNAL, A<Action<CompanyApplication>>._)).MustHaveHappened();

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(
            A<CompanyNameIdpAliasData>.That.Matches(d => d.CompanyId == _companyId),
            A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._,
            A<CancellationToken>._)).MustHaveHappened();

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Matches(u => u.FirstName == invitationData.FirstName))).MustHaveHappened();
        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Not.Matches(u => u.FirstName == invitationData.FirstName))).MustNotHaveHappened();

        A.CallTo(() => _applicationRepository.CreateInvitation(A<Guid>.That.IsEqualTo(_applicationId), A<Guid>._)).MustHaveHappened();

        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedTwiceExactly();
        A.CallTo(() => _mailingInformationRepository.CreateMailingInformation(A<Guid>._, A<string>._, A<string>._, A<Dictionary<string, string>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task TestExecuteInvitationNoEmailThrows()
    {
        var processes = new List<Process>();
        var processSteps = new List<ProcessStep>();
        var invitations = new List<CompanyInvitation>();
        SetupFakesForInvite(processes, processSteps, invitations);

        var invitationData = _fixture.Build<CompanyInvitationData>()
            .WithNamePattern(x => x.FirstName)
            .WithNamePattern(x => x.LastName)
            .With(x => x.Email, "")
            .Create();

        var sut = new InvitationBusinessLogic(
            _provisioningManager,
            _userProvisioningService,
            _portalRepositories,
            _options);

        Task Act() => sut.ExecuteInvitation(invitationData);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be("email must not be empty (Parameter 'email')");

        A.CallTo(() => _provisioningManager.GetNextCentralIdentityProviderNameAsync()).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingInformationRepository.CreateMailingInformation(A<Guid>._, A<string>._, A<string>._, A<Dictionary<string, string>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task TestExecuteInvitationNoOrganisationNameThrows()
    {
        var processes = new List<Process>();
        var processSteps = new List<ProcessStep>();
        var invitations = new List<CompanyInvitation>();
        SetupFakesForInvite(processes, processSteps, invitations);

        var invitationData = _fixture.Build<CompanyInvitationData>()
            .With(x => x.OrganisationName, "")
            .WithNamePattern(x => x.FirstName)
            .WithNamePattern(x => x.LastName)
            .WithEmailPattern(x => x.Email)
            .Create();

        var sut = new InvitationBusinessLogic(
            _provisioningManager,
            _userProvisioningService,
            _portalRepositories,
            _options);

        Task Act() => sut.ExecuteInvitation(invitationData);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be("organisationName must not be empty (Parameter 'organisationName')");

        A.CallTo(() => _provisioningManager.GetNextCentralIdentityProviderNameAsync()).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingInformationRepository.CreateMailingInformation(A<Guid>._, A<string>._, A<string>._, A<Dictionary<string, string>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task TestExecuteInvitationWrongPatternOrganisationNameThrows()
    {
        var processes = new List<Process>();
        var processSteps = new List<ProcessStep>();
        var invitations = new List<CompanyInvitation>();
        SetupFakesForInvite(processes, processSteps, invitations);

        var invitationData = _fixture.Build<CompanyInvitationData>()
            .With(x => x.OrganisationName, "*Catena")
            .WithNamePattern(x => x.FirstName)
            .WithNamePattern(x => x.LastName)
            .WithEmailPattern(x => x.Email)
            .Create();

        var sut = new InvitationBusinessLogic(
            _provisioningManager,
            _userProvisioningService,
            _portalRepositories,
            _options);

        Task Act() => sut.ExecuteInvitation(invitationData);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be("OrganisationName length must be 3-40 characters and *+=#%\\s not used as one of the first three characters in the Organisation name (Parameter 'organisationName')");

        A.CallTo(() => _provisioningManager.GetNextCentralIdentityProviderNameAsync()).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingInformationRepository.CreateMailingInformation(A<Guid>._, A<string>._, A<string>._, A<Dictionary<string, string>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task TestExecuteInvitationCreateUserErrorThrows()
    {
        var processes = new List<Process>();
        var processSteps = new List<ProcessStep>();
        var invitations = new List<CompanyInvitation>();
        SetupFakesForInvite(processes, processSteps, invitations);

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>._)).ReturnsLazily(
            (UserCreationRoleDataIdpInfo creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                .With(x => x.UserName, creationInfo.UserName)
                .With(x => x.Error, _error)
                .Create());

        var invitationData = _fixture.Build<CompanyInvitationData>()
            .With(x => x.OrganisationName, _companyName)
            .WithNamePattern(x => x.FirstName)
            .WithNamePattern(x => x.LastName)
            .WithEmailPattern(x => x.Email)
            .Create();

        var sut = new InvitationBusinessLogic(
            _provisioningManager,
            _userProvisioningService,
            _portalRepositories,
            _options);

        Task Act() => sut.ExecuteInvitation(invitationData);

        var error = await Assert.ThrowsAsync<TestException>(Act).ConfigureAwait(false);
        error.Message.Should().Be(_error.Message);

        A.CallTo(() => _provisioningManager.GetNextCentralIdentityProviderNameAsync()).MustHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened();
        A.CallTo(() => _mailingInformationRepository.CreateMailingInformation(A<Guid>._, A<string>._, A<string>._, A<Dictionary<string, string>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task TestExecuteInvitationCreateUserThrowsThrows()
    {
        var processes = new List<Process>();
        var processSteps = new List<ProcessStep>();
        var invitations = new List<CompanyInvitation>();
        SetupFakesForInvite(processes, processSteps, invitations);

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>._)).Throws(_error);

        var invitationData = _fixture.Build<CompanyInvitationData>()
            .With(x => x.OrganisationName, _companyName)
            .WithNamePattern(x => x.FirstName)
            .WithNamePattern(x => x.LastName)
            .WithEmailPattern(x => x.Email)
            .Create();

        var sut = new InvitationBusinessLogic(
            _provisioningManager,
            _userProvisioningService,
            _portalRepositories,
            _options);

        Task Act() => sut.ExecuteInvitation(invitationData);

        processes.Should().ContainSingle().And.Satisfy(x => x.ProcessTypeId == ProcessTypeId.INVITATION);
        processSteps.Should().ContainSingle().And.Satisfy(x => x.ProcessStepTypeId == ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP && x.ProcessStepStatusId == ProcessStepStatusId.TODO);
        invitations.Should().ContainSingle().And.Satisfy(x => x.ProcessId == processes.Single().Id && x.UserName == "testUserName");
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
        var process = _fixture.Build<Process>().With(x => x.LockExpiryDate, (DateTimeOffset?)null).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerCreateCentralIdp(process.Id).ConfigureAwait(false);

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
        async Task Act() => await _sut.RetriggerCreateCentralIdp(process.Id).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

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
        var process = _fixture.Build<Process>().With(x => x.LockExpiryDate, (DateTimeOffset?)null).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerCreateSharedIdpServiceAccount(process.Id).ConfigureAwait(false);

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
        async Task Act() => await _sut.RetriggerCreateDatabaseIdp(process.Id).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

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
        var process = _fixture.Build<Process>().With(x => x.LockExpiryDate, (DateTimeOffset?)null).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerUpdateCentralIdpUrls(process.Id).ConfigureAwait(false);

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
        async Task Act() => await _sut.RetriggerCreateDatabaseIdp(process.Id).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

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
        var process = _fixture.Build<Process>().With(x => x.LockExpiryDate, (DateTimeOffset?)null).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerCreateCentralIdpOrgMapper(process.Id).ConfigureAwait(false);

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
        async Task Act() => await _sut.RetriggerCreateCentralIdpOrgMapper(process.Id).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

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
        var process = _fixture.Build<Process>().With(x => x.LockExpiryDate, (DateTimeOffset?)null).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerCreateSharedRealmIdpClient(process.Id).ConfigureAwait(false);

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
        async Task Act() => await _sut.RetriggerCreateSharedRealmIdpClient(process.Id).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

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
        var process = _fixture.Build<Process>().With(x => x.LockExpiryDate, (DateTimeOffset?)null).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerEnableCentralIdp(process.Id).ConfigureAwait(false);

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
        async Task Act() => await _sut.RetriggerEnableCentralIdp(process.Id).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

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
        var process = _fixture.Build<Process>().With(x => x.LockExpiryDate, (DateTimeOffset?)null).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerCreateDatabaseIdp(process.Id).ConfigureAwait(false);

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
        async Task Act() => await _sut.RetriggerCreateDatabaseIdp(process.Id).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

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
        var process = _fixture.Build<Process>().With(x => x.LockExpiryDate, (DateTimeOffset?)null).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerInvitationCreateUser(process.Id).ConfigureAwait(false);

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
        async Task Act() => await _sut.RetriggerInvitationCreateUser(process.Id).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be($"process {process.Id} does not exist");
    }

    #endregion

    #region RetriggerInvitationSendMail

    [Fact]
    public async Task RetriggerInvitationSendMail_CallsExpected()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_SEND_MAIL;
        var processStepTypeId = ProcessStepTypeId.INVITATION_SEND_MAIL;
        var processSteps = new List<ProcessStep>();
        var process = _fixture.Build<Process>().With(x => x.LockExpiryDate, (DateTimeOffset?)null).Create();
        var processStepId = Guid.NewGuid();
        SetupFakesForRetrigger(processSteps);
        var verifyProcessData = new VerifyProcessData(process, Enumerable.Repeat(new ProcessStep(processStepId, stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow), 1));
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerInvitationSendMail(process.Id).ConfigureAwait(false);

        // Assert
        processSteps.Should().ContainSingle().And.Satisfy(x => x.ProcessStepTypeId == processStepTypeId);
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerInvitationSendMail_WithNotExistingProcess_ThrowsException()
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.RETRIGGER_INVITATION_SEND_MAIL;
        var process = _fixture.Create<Process>();
        A.CallTo(() => _processStepRepository.IsValidProcess(process.Id, ProcessTypeId.INVITATION, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .Returns((false, _fixture.Create<VerifyProcessData>()));
        async Task Act() => await _sut.RetriggerInvitationSendMail(process.Id).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be($"process {process.Id} does not exist");
    }

    #endregion

    #region Setup

    private void SetupFakesForInvite(List<Process> processes, List<ProcessStep> processSteps, List<CompanyInvitation> invitations)
    {
        var createdProcessId = Guid.NewGuid();
        // A.CallTo(() => _options.Value).Returns(_fixture.Build<InvitationSettings>()
        //     .With(x => x.InvitedUserInitialRoles, new[]
        //     {
        //         new UserRoleConfig(_fixture.Create<string>(), _fixture.CreateMany<string>())
        //     });
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP, ProcessStepStatusId.TODO, createdProcessId))
            .Invokes((ProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid processId) =>
            {
                var processStep = new ProcessStep(Guid.NewGuid(), processStepTypeId, processStepStatusId, processId,
                    DateTimeOffset.UtcNow);
                processSteps.Add(processStep);
            });

        A.CallTo(() => _portalRepositories.GetInstance<IMailingInformationRepository>()).Returns(_mailingInformationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);

        A.CallTo(() => _companyRepository.CreateCompany(A<string>._, A<Action<Company>?>._)).ReturnsLazily((string organisationName, Action<Company>? _) =>
            new Company(_companyId, organisationName, CompanyStatusId.PENDING, _fixture.Create<DateTimeOffset>()));

        A.CallTo(() => _identityProviderRepository.CreateIdentityProvider(A<IdentityProviderCategoryId>._, A<IdentityProviderTypeId>._, A<Guid>._, A<Action<IdentityProvider>?>._))
            .ReturnsLazily((IdentityProviderCategoryId categoryId, IdentityProviderTypeId typeId, Guid owner, Action<IdentityProvider>? setOptionalFields) =>
            {
                var idp = new IdentityProvider(_identityProviderId, categoryId, typeId, owner, _fixture.Create<DateTimeOffset>());
                setOptionalFields?.Invoke(idp);
                return idp;
            });

        A.CallTo(() => _applicationRepository.CreateCompanyApplication(A<Guid>._, A<CompanyApplicationStatusId>._, A<CompanyApplicationTypeId>._, A<Action<CompanyApplication>?>._))
            .ReturnsLazily((Guid companyId, CompanyApplicationStatusId applicationStatusId, CompanyApplicationTypeId typeId, Action<CompanyApplication>? _) => new CompanyApplication(_applicationId, companyId, applicationStatusId, typeId, _fixture.Create<DateTimeOffset>()));

        A.CallTo(() => _provisioningManager.GetNextCentralIdentityProviderNameAsync()).Returns(_idpName);

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<CancellationToken>._))
            .ReturnsLazily((CompanyNameIdpAliasData _, IAsyncEnumerable<UserCreationRoleDataIdpInfo> userCreationInfos, CancellationToken _) =>
                userCreationInfos.Select(userCreationInfo => _processLine(userCreationInfo)));

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>._)).ReturnsLazily(
            (UserCreationRoleDataIdpInfo creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                .With(x => x.UserName, creationInfo.UserName)
                .With(x => x.Error, default(Exception?))
                .Create());
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

    [Serializable]
    public class TestException : Exception
    {
        public TestException() { }
        public TestException(string message) : base(message) { }
        public TestException(string message, Exception inner) : base(message, inner) { }

        protected TestException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
