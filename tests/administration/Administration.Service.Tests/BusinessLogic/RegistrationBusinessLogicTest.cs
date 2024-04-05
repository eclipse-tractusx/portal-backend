/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Common;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class RegistrationBusinessLogicTest
{
    private const string BusinessPartnerNumber = "CAXLSHAREDIDPZZ";
    private const string AlreadyTakenBpn = "BPNL123698762666";
    private const string ValidBpn = "BPNL123698762345";
    private const string CompanyName = "TestCompany";
    private const string IamAliasId = "idp1";
    private static readonly Guid IdpId = Guid.NewGuid();
    private static readonly Guid IdWithBpn = new("c244f79a-7faf-4c59-bb85-fbfdf72ce46f");
    private static readonly Guid NotExistingApplicationId = new("9f0cfd0d-c512-438e-a07e-3198bce873bf");
    private static readonly Guid ActiveApplicationCompanyId = new("045abf01-7762-468b-98fb-84a30c39b7c7");
    private static readonly Guid CompanyId = new("95c4339e-e087-4cd2-a5b8-44d385e64630");
    private static readonly Guid ExistingExternalId = Guid.NewGuid();
    private static readonly Guid IdWithoutBpn = new("d90995fe-1241-4b8d-9f5c-f3909acc6399");
    private static readonly Guid ApplicationId = new("6084d6e0-0e01-413c-850d-9f944a6c494c");
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly IPortalRepositories _portalRepositories;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IIdentityProviderRepository _identityProviderRepository;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFixture _fixture;
    private readonly IRegistrationBusinessLogic _logic;
    private readonly ICompanyRepository _companyRepository;
    private readonly IApplicationChecklistService _checklistService;
    private readonly IClearinghouseBusinessLogic _clearinghouseBusinessLogic;
    private readonly ISdFactoryBusinessLogic _sdFactoryBusinessLogic;
    private readonly IMailingService _mailingService;
    private readonly IDocumentRepository _documentRepository;
    private readonly IProvisioningManager _provisioningManager;

    public RegistrationBusinessLogicTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();

        var options = A.Fake<IOptions<RegistrationSettings>>();
        var settings = A.Fake<RegistrationSettings>();
        settings.ApplicationsMaxPageSize = 15;
        A.CallTo(() => options.Value).Returns(settings);

        _clearinghouseBusinessLogic = A.Fake<IClearinghouseBusinessLogic>();
        _sdFactoryBusinessLogic = A.Fake<ISdFactoryBusinessLogic>();
        _checklistService = A.Fake<IApplicationChecklistService>();
        _mailingService = A.Fake<IMailingService>();
        _provisioningManager = A.Fake<IProvisioningManager>();

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);

        var logger = A.Fake<ILogger<RegistrationBusinessLogic>>();

        _logic = new RegistrationBusinessLogic(_portalRepositories, options, _mailingService, _checklistService, _clearinghouseBusinessLogic, _sdFactoryBusinessLogic, _provisioningManager, logger);
    }

    #region GetCompanyApplicationDetailsAsync

    [Fact]
    public async Task GetCompanyApplicationDetailsAsync_WithDefaultRequest_GetsExpectedEntries()
    {
        // Arrange
        var companyAppStatus = new[] { CompanyApplicationStatusId.SUBMITTED, CompanyApplicationStatusId.CONFIRMED, CompanyApplicationStatusId.DECLINED };
        var companyApplicationData = new AsyncEnumerableStub<CompanyApplication>(_fixture.CreateMany<CompanyApplication>(5));
        A.CallTo(() => _applicationRepository.GetCompanyApplicationsFilteredQuery(A<string?>._, A<IEnumerable<CompanyApplicationStatusId>?>._))
            .Returns(companyApplicationData.AsQueryable());

        // Act
        var result = await _logic.GetCompanyApplicationDetailsAsync(0, 5).ConfigureAwait(false);
        // Assert
        A.CallTo(() => _applicationRepository.GetCompanyApplicationsFilteredQuery(null, A<IEnumerable<CompanyApplicationStatusId>>.That.Matches(x => x.Count() == 3 && x.All(y => companyAppStatus.Contains(y))))).MustHaveHappenedOnceExactly();
        Assert.IsType<Pagination.Response<CompanyApplicationDetails>>(result);
        result.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetCompanyApplicationDetailsAsync_WithInReviewRequest_GetsExpectedEntries()
    {
        // Arrange
        var companyAppStatus = new[] { CompanyApplicationStatusId.SUBMITTED };
        var companyApplicationData = new AsyncEnumerableStub<CompanyApplication>(_fixture.CreateMany<CompanyApplication>(5));
        A.CallTo(() => _applicationRepository.GetCompanyApplicationsFilteredQuery(A<string?>._, A<IEnumerable<CompanyApplicationStatusId>?>._))
            .Returns(companyApplicationData.AsQueryable());

        // Act
        var result = await _logic.GetCompanyApplicationDetailsAsync(0, 5, CompanyApplicationStatusFilter.InReview).ConfigureAwait(false);
        // Assert
        A.CallTo(() => _applicationRepository.GetCompanyApplicationsFilteredQuery(null, A<IEnumerable<CompanyApplicationStatusId>>.That.Matches(x => x.Count() == 1 && x.All(y => companyAppStatus.Contains(y))))).MustHaveHappenedOnceExactly();
        Assert.IsType<Pagination.Response<CompanyApplicationDetails>>(result);
        result.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetCompanyApplicationDetailsAsync_WithClosedRequest_GetsExpectedEntries()
    {
        // Arrange
        var companyAppStatus = new[] { CompanyApplicationStatusId.CONFIRMED, CompanyApplicationStatusId.DECLINED };
        var companyApplicationData = new AsyncEnumerableStub<CompanyApplication>(_fixture.CreateMany<CompanyApplication>(5));
        A.CallTo(() => _applicationRepository.GetCompanyApplicationsFilteredQuery(A<string?>._, A<IEnumerable<CompanyApplicationStatusId>?>._))
            .Returns(companyApplicationData.AsQueryable());

        // Act
        var result = await _logic.GetCompanyApplicationDetailsAsync(0, 5, CompanyApplicationStatusFilter.Closed).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationRepository.GetCompanyApplicationsFilteredQuery(null, A<IEnumerable<CompanyApplicationStatusId>>.That.Matches(x => x.Count() == 2 && x.All(y => companyAppStatus.Contains(y))))).MustHaveHappenedOnceExactly();
        Assert.IsType<Pagination.Response<CompanyApplicationDetails>>(result);
        result.Content.Should().HaveCount(5);
    }

    #endregion

    #region GetCompanyWithAddressAsync

    [Theory]
    [InlineData(UniqueIdentifierId.VAT_ID, "DE124356789")]
    [InlineData(UniqueIdentifierId.LEI_CODE, "54930084UKLVMY22DS16")]
    [InlineData(UniqueIdentifierId.EORI, "DE123456789012345")]
    [InlineData(UniqueIdentifierId.COMMERCIAL_REG_NUMBER, "HRB123456")]
    [InlineData(UniqueIdentifierId.VIES, "ATU99999999")]
    public async Task GetCompanyWithAddressAsync_WithDefaultRequest_GetsExpectedResult(UniqueIdentifierId identifierIdType, string companyUniqueIds)
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var data = _fixture.Build<CompanyUserRoleWithAddress>()
            .With(x => x.AgreementsData, _fixture.CreateMany<AgreementsData>(20))
            .With(x => x.CompanyIdentifiers, Enumerable.Repeat(new ValueTuple<UniqueIdentifierId, string>(identifierIdType, companyUniqueIds), 1))
            .Create();
        A.CallTo(() => _applicationRepository.GetCompanyUserRoleWithAddressUntrackedAsync(applicationId))
            .Returns(data);

        // Act
        var result = await _logic.GetCompanyWithAddressAsync(applicationId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationRepository.GetCompanyUserRoleWithAddressUntrackedAsync(applicationId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<CompanyWithAddressData>();
        result.Should().Match<CompanyWithAddressData>(r =>
            r.CompanyId == data.CompanyId &&
            r.Name == data.Name &&
            r.ShortName == data.Shortname &&
            r.BusinessPartnerNumber == data.BusinessPartnerNumber &&
            r.City == data.City &&
            r.StreetName == data.StreetName &&
            r.CountryAlpha2Code == data.CountryAlpha2Code &&
            r.Region == data.Region &&
            r.StreetAdditional == data.Streetadditional &&
            r.StreetNumber == data.Streetnumber &&
            r.ZipCode == data.Zipcode
        );
        result.AgreementsRoleData.Should().HaveSameCount(data.AgreementsData.DistinctBy(ad => ad.CompanyRoleId));
        result.InvitedUserData.Should().HaveSameCount(data.InvitedCompanyUserData);
        result.UniqueIds.Should().HaveSameCount(data.CompanyIdentifiers);
        result.UniqueIds.Should().AllSatisfy(u => u.Should().Match<CompanyUniqueIdData>(u => u.UniqueIdentifierId == identifierIdType && u.Value == companyUniqueIds));
    }

    [Fact]
    public async Task GetCompanyWithAddressAsync_WithDefaultRequest_GetsExpectedResult_DefaultValues()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var data = _fixture.Build<CompanyUserRoleWithAddress>()
            .With(x => x.Shortname, (string?)null)
            .With(x => x.BusinessPartnerNumber, (string?)null)
            .With(x => x.City, (string?)null)
            .With(x => x.StreetName, (string?)null)
            .With(x => x.CountryAlpha2Code, (string?)null)
            .With(x => x.Region, (string?)null)
            .With(x => x.Streetadditional, (string?)null)
            .With(x => x.Streetnumber, (string?)null)
            .With(x => x.Zipcode, (string?)null)
            .With(x => x.CountryDe, (string?)null)
            .With(x => x.InvitedCompanyUserData, _fixture.CreateMany<Guid>().Select(id => new InvitedCompanyUserData(id, null, null, null)))
            .Create();
        A.CallTo(() => _applicationRepository.GetCompanyUserRoleWithAddressUntrackedAsync(applicationId))
            .Returns(data);

        // Act
        var result = await _logic.GetCompanyWithAddressAsync(applicationId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationRepository.GetCompanyUserRoleWithAddressUntrackedAsync(applicationId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<CompanyWithAddressData>();
        result.Should().Match<CompanyWithAddressData>(r =>
            r.CompanyId == data.CompanyId &&
            r.Name == data.Name &&
            r.ShortName == "" &&
            r.BusinessPartnerNumber == "" &&
            r.City == "" &&
            r.StreetName == "" &&
            r.CountryAlpha2Code == "" &&
            r.Region == "" &&
            r.StreetAdditional == "" &&
            r.StreetNumber == "" &&
            r.ZipCode == ""
        );
        result.InvitedUserData.Should().HaveSameCount(data.InvitedCompanyUserData);
        result.InvitedUserData.Should().AllSatisfy(u => u.Should().Match<InvitedUserData>(u => u.FirstName == "" && u.LastName == "" && u.Email == ""));
    }

    #endregion

    #region UpdateCompanyBpn

    [Fact]
    public async Task UpdateCompanyBpnAsync_WithInvalidBpn_ThrowsControllerArgumentException()
    {
        // Arrange
        var bpn = "123";

        // Act
        async Task Act() => await _logic.UpdateCompanyBpn(IdWithBpn, bpn).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.ParamName.Should().Be("bpn");
        ex.Message.Should().Be("BPN must contain exactly 16 characters long. (Parameter 'bpn')");
    }

    [Fact]
    public async Task UpdateCompanyBpnAsync_WithNotExistingApplication_ThrowsNotFoundException()
    {
        // Arrange
        SetupForUpdateCompanyBpn();

        // Act
        async Task Act() => await _logic.UpdateCompanyBpn(NotExistingApplicationId, ValidBpn).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"application {NotExistingApplicationId} not found");
    }

    [Fact]
    public async Task UpdateCompanyBpnAsync_WithAlreadyTakenBpn_ThrowsConflictException()
    {
        // Arrange
        SetupForUpdateCompanyBpn();

        // Act
        async Task Act() => await _logic.UpdateCompanyBpn(IdWithoutBpn, AlreadyTakenBpn).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("BusinessPartnerNumber is already assigned to a different company");
    }

    [Fact]
    public async Task UpdateCompanyBpnAsync_WithActiveCompanyForApplication_ThrowsConflictException()
    {
        // Arrange
        SetupForUpdateCompanyBpn();

        // Act
        async Task Act() => await _logic.UpdateCompanyBpn(ActiveApplicationCompanyId, ValidBpn).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"application {ActiveApplicationCompanyId} for company {CompanyId} is not pending");
    }

    [Fact]
    public async Task UpdateCompanyBpnAsync_WithBpnAlreadySet_ThrowsConflictException()
    {
        // Arrange
        SetupForUpdateCompanyBpn();

        // Act
        async Task Act() => await _logic.UpdateCompanyBpn(IdWithBpn, ValidBpn).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"BusinessPartnerNumber of company {CompanyId} has already been set.");
    }

    [Fact]
    public async Task UpdateCompanyBpnAsync_WithValidData_CallsExpected()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithoutBpn, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        SetupForUpdateCompanyBpn(entry);

        // Act
        await _logic.UpdateCompanyBpn(IdWithoutBpn, ValidBpn).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(CompanyId, null, A<Action<Company>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
    }

    #endregion

    #region ProcessClearinghouseResponse

    [Fact]
    public async Task ProcessClearinghouseResponseAsync_WithValidData_CallsExpected()
    {
        // Arrange
        A.CallTo(() => _applicationRepository.GetSubmittedApplicationIdsByBpn(BusinessPartnerNumber))
            .Returns(Enumerable.Repeat(ApplicationId, 1).ToAsyncEnumerable());

        // Act
        var data = new ClearinghouseResponseData(BusinessPartnerNumber, ClearinghouseResponseStatus.CONFIRM, null);
        await _logic.ProcessClearinghouseResponseAsync(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _clearinghouseBusinessLogic.ProcessEndClearinghouse(ApplicationId, data, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ProcessClearinghouseResponseAsync_WithMultipleApplications_ThrowsConflictException()
    {
        // Arrange
        A.CallTo(() => _applicationRepository.GetSubmittedApplicationIdsByBpn(BusinessPartnerNumber))
            .Returns(new[] { CompanyId, Guid.NewGuid() }.ToAsyncEnumerable());

        // Act
        var data = new ClearinghouseResponseData(BusinessPartnerNumber, ClearinghouseResponseStatus.CONFIRM, null);
        async Task Act() => await _logic.ProcessClearinghouseResponseAsync(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Contain($"more than one companyApplication in status SUBMITTED found for BPN {BusinessPartnerNumber}");
    }

    [Fact]
    public async Task ProcessClearinghouseResponseAsync_WithNoApplication_ThrowsNotFoundException()
    {
        // Arrange
        A.CallTo(() => _applicationRepository.GetSubmittedApplicationIdsByBpn(BusinessPartnerNumber))
            .Returns(Enumerable.Empty<Guid>().ToAsyncEnumerable());

        // Act
        var data = new ClearinghouseResponseData(BusinessPartnerNumber, ClearinghouseResponseStatus.CONFIRM, null);
        async Task Act() => await _logic.ProcessClearinghouseResponseAsync(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Contain($"No companyApplication for BPN {BusinessPartnerNumber} is not in status SUBMITTED");
    }

    #endregion

    #region SetRegistrationVerification

    [Fact]
    public async Task SetRegistrationVerification_WithApproval_CallsExpected()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithBpn, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        SetupForApproveRegistrationVerification(entry);

        // Act
        await _logic.ApproveRegistrationVerification(IdWithBpn).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.Comment.Should().BeNull();
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>._)).MustNotHaveHappened();
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(
            A<IApplicationChecklistService.ManualChecklistProcessStepData>._,
            null,
            A<Action<ApplicationChecklistEntry>>._,
            A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count(y => y == ProcessStepTypeId.CREATE_IDENTITY_WALLET) == 1)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SetRegistrationVerification_WithBpnNotDone_CallsExpected()
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(IdWithoutBpn, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        SetupForApproveRegistrationVerification(entry);

        // Act
        await _logic.ApproveRegistrationVerification(IdWithoutBpn).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.Comment.Should().BeNull();
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>._)).MustNotHaveHappened();
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, null, A<Action<ApplicationChecklistEntry>>._, null)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(ApplicationChecklistEntryStatusId.TO_DO, IdentityProviderTypeId.SHARED)]
    [InlineData(ApplicationChecklistEntryStatusId.DONE, IdentityProviderTypeId.SHARED)]
    [InlineData(ApplicationChecklistEntryStatusId.TO_DO, IdentityProviderTypeId.OWN)]
    [InlineData(ApplicationChecklistEntryStatusId.DONE, IdentityProviderTypeId.OWN)]
    public async Task DeclineRegistrationVerification_WithDecline_StateAndCommentSetCorrectly(ApplicationChecklistEntryStatusId checklistStatusId, IdentityProviderTypeId idpTypeId)
    {
        // Arrange
        const string comment = "application rejected because of reasons.";
        var entry = new ApplicationChecklistEntry(IdWithBpn, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, checklistStatusId, DateTimeOffset.UtcNow);
        var company = new Company(CompanyId, null!, CompanyStatusId.PENDING, DateTimeOffset.UtcNow);
        var application = new CompanyApplication(ApplicationId, company.Id, CompanyApplicationStatusId.SUBMITTED, CompanyApplicationTypeId.INTERNAL, DateTimeOffset.UtcNow);
        SetupForDeclineRegistrationVerification(entry, application, company, checklistStatusId, idpTypeId);

        // Act
        await _logic.DeclineRegistrationVerification(IdWithBpn, comment, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.Comment.Should().Be(comment);
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.FAILED);
        company.CompanyStatusId.Should().Be(CompanyStatusId.REJECTED);
        application.ApplicationStatusId.Should().Be(CompanyApplicationStatusId.DECLINED);
        A.CallTo(() => _checklistService.SkipProcessSteps(
                A<IApplicationChecklistService.ManualChecklistProcessStepData>._,
                A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Single() == ProcessStepTypeId.VERIFY_REGISTRATION)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(
                A<IApplicationChecklistService.ManualChecklistProcessStepData>._,
                A<Action<ApplicationChecklistEntry>>._,
                A<Action<ApplicationChecklistEntry>>._,
                A<IEnumerable<ProcessStepTypeId>?>.That.Matches(x => x != null && x.Count() == 1 && x.Single() == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED)))
            .MustHaveHappenedOnceExactly();
        if (idpTypeId == IdentityProviderTypeId.SHARED)
        {
            A.CallTo(() => _provisioningManager.DeleteSharedIdpRealmAsync(IamAliasId))
            .MustHaveHappenedOnceExactly();
        }

        A.CallTo(() => _provisioningManager.DeleteCentralIdentityProviderAsync(IamAliasId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteCentralRealmUserAsync("user123"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeclineRegistrationVerification_WithApplicationNotFound_ThrowsArgumentException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        A.CallTo(() => _applicationRepository.GetCompanyIdNameForSubmittedApplication(applicationId))
            .Returns(default((Guid, string, Guid?, IEnumerable<(Guid, string, IdentityProviderTypeId, IEnumerable<Guid>)>, IEnumerable<Guid>)));
        async Task Act() => await _logic.DeclineRegistrationVerification(applicationId, "test", CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(Act);

        // Assert
        ex.Message.Should().Be($"CompanyApplication {applicationId} is not in status SUBMITTED (Parameter 'applicationId')");
        ex.ParamName.Should().Be("applicationId");
    }

    [Fact]
    public async Task DeclineRegistrationVerification_WithMultipleIdps_CallsExpected()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var sharedIdpId = Guid.NewGuid();
        var managedIdpId = Guid.NewGuid();
        var ownIdpId = Guid.NewGuid();
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var user3 = Guid.NewGuid();

        A.CallTo(() => _applicationRepository.GetCompanyIdNameForSubmittedApplication(applicationId))
            .Returns((
                companyId,
                "test",
                null,
                new[]
                {
                    (sharedIdpId, "idp1", IdentityProviderTypeId.SHARED, new[] { user1 }.AsEnumerable()),
                    (managedIdpId, "idp2", IdentityProviderTypeId.MANAGED, new[] { user2 }.AsEnumerable()),
                    (ownIdpId, "idp3", IdentityProviderTypeId.OWN, new[] { user3 }.AsEnumerable()),
                },
                new[]
                {
                    user1,
                    user2,
                    user3
                }));

        // Act
        await _logic.DeclineRegistrationVerification(applicationId, "test", CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _identityProviderRepository.DeleteCompanyIdentityProvider(companyId, sharedIdpId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.DeleteIamIdentityProvider("idp1")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.DeleteIdentityProvider(sharedIdpId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteSharedIdpRealmAsync("idp1")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteCentralIdentityProviderAsync("idp1")).MustHaveHappenedOnceExactly();

        A.CallTo(() => _identityProviderRepository.DeleteCompanyIdentityProvider(companyId, sharedIdpId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.DeleteIamIdentityProvider("idp2")).MustNotHaveHappened();
        A.CallTo(() => _identityProviderRepository.DeleteIdentityProvider(managedIdpId)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.DeleteSharedIdpRealmAsync("idp2")).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.DeleteCentralIdentityProviderAsync("idp2")).MustNotHaveHappened();

        A.CallTo(() => _identityProviderRepository.DeleteCompanyIdentityProvider(companyId, ownIdpId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.DeleteIamIdentityProvider("idp3")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.DeleteIdentityProvider(ownIdpId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteSharedIdpRealmAsync("idp3")).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.DeleteCentralIdentityProviderAsync("idp3")).MustHaveHappenedOnceExactly();

        A.CallTo(() => _userRepository.RemoveCompanyUserAssignedIdentityProviders(A<IEnumerable<ValueTuple<Guid, Guid>>>.That.IsSameSequenceAs(new[] { new ValueTuple<Guid, Guid>(user1, sharedIdpId) }))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.RemoveCompanyUserAssignedIdentityProviders(A<IEnumerable<ValueTuple<Guid, Guid>>>.That.IsSameSequenceAs(new[] { new ValueTuple<Guid, Guid>(user2, managedIdpId) }))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.RemoveCompanyUserAssignedIdentityProviders(A<IEnumerable<ValueTuple<Guid, Guid>>>.That.IsSameSequenceAs(new[] { new ValueTuple<Guid, Guid>(user3, ownIdpId) }))).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetChecklistForApplicationAsync

    [Fact]
    public async Task GetChecklistForApplicationAsync_WithNotExistingApplication_ThrowsNotFoundException()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        A.CallTo(() => _applicationRepository.GetApplicationChecklistData(applicationId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new ValueTuple<bool, IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId, string?)>, IEnumerable<ProcessStepTypeId>>());

        //Act
        async Task Act() => await _logic.GetChecklistForApplicationAsync(applicationId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Application {applicationId} does not exists");
    }

    [Fact]
    public async Task GetChecklistForApplicationAsync_WithValidApplication_ReturnsExpected()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var list = new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId, string?>[]
        {
            new(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE, null),
            new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE, null),
            new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.FAILED, "error occured"),
            new(ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.IN_PROGRESS, null),
            new(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.IN_PROGRESS, null),
        };
        var processSteps = new ProcessStepTypeId[]
        {
            ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET
        };
        A.CallTo(() => _applicationRepository.GetApplicationChecklistData(applicationId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new ValueTuple<bool, IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId, string?)>, IEnumerable<ProcessStepTypeId>>(true, list, processSteps));

        //Act
        var result = await _logic.GetChecklistForApplicationAsync(applicationId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(5);
        result.Where(x => x.RetriggerableProcessSteps.Any()).Should().HaveCount(1);
        result.Where(x => x.Status == ApplicationChecklistEntryStatusId.FAILED).Should().ContainSingle();
    }

    #endregion

    #region TriggerChecklistAsync

    [Fact]
    public async Task TriggerChecklistAsync_WithFailingChecklistServiceCall_ReturnsError()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();

        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(applicationId,
            ApplicationChecklistEntryTypeId.CLEARING_HOUSE,
            A<IEnumerable<ApplicationChecklistEntryStatusId>>._,
            A<ProcessStepTypeId>._,
            null,
            A<IEnumerable<ProcessStepTypeId>>._))
            .Throws(new ConflictException("Test"));

        //Act
        async Task Act() => await _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Test");
    }

    [Theory]
    [InlineData(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE, ProcessStepTypeId.START_CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO)]
    [InlineData(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET, ProcessStepTypeId.CREATE_IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO)]
    [InlineData(ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP, ProcessStepTypeId.START_SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO)]
    [InlineData(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH, ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH, ApplicationChecklistEntryStatusId.TO_DO)]
    [InlineData(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL, ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL, ApplicationChecklistEntryStatusId.IN_PROGRESS)]
    public async Task TriggerChecklistAsync_WithValidData_ReturnsExpected(ApplicationChecklistEntryTypeId typeId, ProcessStepTypeId stepId, ProcessStepTypeId nextStepId, ApplicationChecklistEntryStatusId statusId)
    {
        // Arrange
        var checklistEntry = new ApplicationChecklistEntry(Guid.NewGuid(), typeId,
            ApplicationChecklistEntryStatusId.FAILED, DateTimeOffset.UtcNow)
        {
            Comment = "test"
        };
        var applicationId = _fixture.Create<Guid>();
        var context = new IApplicationChecklistService.ManualChecklistProcessStepData(
            applicationId,
            new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid()),
            Guid.NewGuid(),
            typeId,
            new Dictionary<ApplicationChecklistEntryTypeId, ValueTuple<ApplicationChecklistEntryStatusId, string?>>()
                .ToImmutableDictionary(),
            Enumerable.Empty<ProcessStep>());
        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(applicationId,
                typeId,
                A<IEnumerable<ApplicationChecklistEntryStatusId>>._,
                stepId,
                null,
                A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(context);
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(
                A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._,
                A<IEnumerable<ProcessStepTypeId>>._))
            .Invokes((IApplicationChecklistService.ManualChecklistProcessStepData _, Action<ApplicationChecklistEntry> initial, Action<ApplicationChecklistEntry> modify, IEnumerable<ProcessStepTypeId> _) =>
            {
                initial?.Invoke(checklistEntry);
                modify.Invoke(checklistEntry);
            });

        //Act
        await _logic.TriggerChecklistAsync(applicationId, typeId, stepId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(context,
                A<Action<ApplicationChecklistEntry>>._,
                A<Action<ApplicationChecklistEntry>>._,
                A<IEnumerable<ProcessStepTypeId>>.That.IsSameSequenceAs(new[] { nextStepId })))
            .MustHaveHappenedOnceExactly();
        checklistEntry.ApplicationChecklistEntryStatusId.Should().Be(statusId);
        checklistEntry.Comment.Should().BeNull();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region ProcessClearinghouseSelfDescription

    [Fact]
    public async Task ProcessClearinghouseSelfDescription_WithValidData_CallsExpected()
    {
        // Arrange
        var data = new SelfDescriptionResponseData(ApplicationId, SelfDescriptionStatus.Confirm, null, "{ \"test\": true }");
        var companyId = Guid.NewGuid();
        A.CallTo(() => _applicationRepository.GetCompanyIdSubmissionStatusForApplication(ApplicationId))
            .Returns((true, companyId, true));

        // Act
        await _logic.ProcessClearinghouseSelfDescription(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _sdFactoryBusinessLogic.ProcessFinishSelfDescriptionLpForApplication(data, companyId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ProcessClearinghouseSelfDescription_WithNotExistingApplication_ThrowsNotFoundException()
    {
        // Arrange
        var data = new SelfDescriptionResponseData(ApplicationId, SelfDescriptionStatus.Confirm, null, "{ \"test\": true }");
        A.CallTo(() => _applicationRepository.GetCompanyIdSubmissionStatusForApplication(ApplicationId))
            .Returns(((bool, Guid, bool))default);

        // Act
        async Task Act() => await _logic.ProcessClearinghouseSelfDescription(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"companyApplication {ApplicationId} not found");
    }

    [Fact]
    public async Task ProcessClearinghouseSelfDescription_WithNotSubmittedApplication_ThrowsConflictException()
    {
        // Arrange
        var data = new SelfDescriptionResponseData(ApplicationId, SelfDescriptionStatus.Confirm, null, "{ \"test\": true }");
        A.CallTo(() => _applicationRepository.GetCompanyIdSubmissionStatusForApplication(ApplicationId))
            .Returns((true, Guid.NewGuid(), false));

        // Act
        async Task Act() => await _logic.ProcessClearinghouseSelfDescription(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"companyApplication {ApplicationId} is not in status SUBMITTED");
    }

    [Theory]
    [InlineData(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH)]
    [InlineData(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET)]
    [InlineData(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP)]
    [InlineData(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL)]
    [InlineData(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE)]
    [InlineData(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH)]
    [InlineData(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET)]
    [InlineData(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP)]
    [InlineData(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL)]
    [InlineData(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE)]
    [InlineData(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH)]
    [InlineData(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET)]
    [InlineData(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP)]
    [InlineData(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL)]
    [InlineData(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH)]
    [InlineData(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE)]
    [InlineData(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP)]
    [InlineData(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL)]
    [InlineData(ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH)]
    [InlineData(ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE)]
    [InlineData(ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL)]
    [InlineData(ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET)]
    [InlineData(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE)]
    [InlineData(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP)]
    [InlineData(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET)]
    public async Task TriggerChecklistAsync_WithWrongProcessStepForChecklist_ThrowsConflictException(ApplicationChecklistEntryTypeId typeId, ProcessStepTypeId stepId)
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();

        //Act
        async Task Act() => await _logic.TriggerChecklistAsync(applicationId, typeId, stepId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"The processStep {stepId} is not retriggerable");
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    #endregion

    #region GetDocumentAsync

    [Fact]
    public async Task GetDocumentAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var content = new byte[7];
        A.CallTo(() => _documentRepository.GetDocumentByIdAsync(documentId))
            .ReturnsLazily(() => new Document(documentId, content, content, "test.pdf", MediaTypeId.JSON, DateTimeOffset.UtcNow, DocumentStatusId.LOCKED, DocumentTypeId.APP_CONTRACT));

        // Act
        var result = await _logic.GetDocumentAsync(documentId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.fileName.Should().Be("test.pdf");
    }

    [Fact]
    public async Task GetDocumentAsync_WithNotExistingDocument_ThrowsNotFoundException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        A.CallTo(() => _documentRepository.GetDocumentByIdAsync(documentId))
            .ReturnsLazily(() => (Document?)null);

        // Act
        async Task Act() => await _logic.GetDocumentAsync(documentId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Document {documentId} does not exist");
    }

    #endregion

    #region Setup

    private void SetupForUpdateCompanyBpn(ApplicationChecklistEntry? applicationChecklistEntry = null)
    {
        if (applicationChecklistEntry != null)
        {
            A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>?>._))
                .Invokes((IApplicationChecklistService.ManualChecklistProcessStepData _, Action<ApplicationChecklistEntry> initial, Action<ApplicationChecklistEntry> action, IEnumerable<ProcessStepTypeId>? _) =>
                {
                    action.Invoke(applicationChecklistEntry);
                });
        }

        A.CallTo(() => _userRepository.GetBpnForIamUserUntrackedAsync(IdWithoutBpn, ValidBpn))
            .ReturnsLazily(() => new (bool, bool, string?, Guid)[]
            {
                new (true, true, null, CompanyId)
            }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetBpnForIamUserUntrackedAsync(NotExistingApplicationId, ValidBpn))
            .ReturnsLazily(() => new (bool, bool, string?, Guid)[]
            {
                new (false, true, ValidBpn, CompanyId)
            }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetBpnForIamUserUntrackedAsync(IdWithoutBpn, AlreadyTakenBpn))
            .ReturnsLazily(() => new (bool, bool, string?, Guid)[]
            {
                new (true, true, ValidBpn, CompanyId),
                new (false, true, AlreadyTakenBpn, Guid.NewGuid())
            }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetBpnForIamUserUntrackedAsync(ActiveApplicationCompanyId, ValidBpn))
            .ReturnsLazily(() => new (bool, bool, string?, Guid)[]
            {
                new (true, false, ValidBpn, CompanyId)
            }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetBpnForIamUserUntrackedAsync(IdWithBpn, ValidBpn))
            .ReturnsLazily(() => new (bool, bool, string?, Guid)[]
            {
                new (true, true, ValidBpn, CompanyId)
            }.ToAsyncEnumerable());

        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(IdWithoutBpn, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, A<IEnumerable<ApplicationChecklistEntryStatusId>>._, A<ProcessStepTypeId>._, A<IEnumerable<ApplicationChecklistEntryTypeId>?>._, A<IEnumerable<ProcessStepTypeId>?>._))
            .ReturnsLazily(() => new IApplicationChecklistService.ManualChecklistProcessStepData(IdWithoutBpn, new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid()), Guid.NewGuid(), ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, new Dictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, new ValueTuple<ApplicationChecklistEntryStatusId, string?>(ApplicationChecklistEntryStatusId.DONE, null) }
            }.ToImmutableDictionary(), Enumerable.Empty<ProcessStep>()));
    }

    private void SetupForApproveRegistrationVerification(ApplicationChecklistEntry applicationChecklistEntry)
    {
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>?>._))
            .Invokes((IApplicationChecklistService.ManualChecklistProcessStepData _, Action<ApplicationChecklistEntry>? initial, Action<ApplicationChecklistEntry> action, IEnumerable<ProcessStepTypeId>? _) =>
            {
                action.Invoke(applicationChecklistEntry);
            });

        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(IdWithoutBpn, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, A<IEnumerable<ApplicationChecklistEntryStatusId>>._, A<ProcessStepTypeId>._, A<IEnumerable<ApplicationChecklistEntryTypeId>?>._, A<IEnumerable<ProcessStepTypeId>?>._))
            .ReturnsLazily(() => new IApplicationChecklistService.ManualChecklistProcessStepData(IdWithoutBpn, new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid()), Guid.NewGuid(), ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, new Dictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>
            {
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, new ValueTuple<ApplicationChecklistEntryStatusId, string?>(ApplicationChecklistEntryStatusId.IN_PROGRESS, null) }
            }.ToImmutableDictionary(), Enumerable.Empty<ProcessStep>()));
        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(IdWithBpn, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, A<IEnumerable<ApplicationChecklistEntryStatusId>>._, A<ProcessStepTypeId>._, A<IEnumerable<ApplicationChecklistEntryTypeId>?>._, A<IEnumerable<ProcessStepTypeId>?>._))
            .ReturnsLazily(() => new IApplicationChecklistService.ManualChecklistProcessStepData(IdWithoutBpn, new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid()), Guid.NewGuid(), ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, new Dictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>
            {
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, new ValueTuple<ApplicationChecklistEntryStatusId, string?>(ApplicationChecklistEntryStatusId.DONE, null) }
            }.ToImmutableDictionary(), Enumerable.Empty<ProcessStep>()));
    }

    private void SetupForDeclineRegistrationVerification(ApplicationChecklistEntry applicationChecklistEntry, CompanyApplication application, Company company, ApplicationChecklistEntryStatusId checklistStatusId, IdentityProviderTypeId idpTypeId)
    {
        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>?>._))
            .Invokes((IApplicationChecklistService.ManualChecklistProcessStepData _, Action<ApplicationChecklistEntry>? _, Action<ApplicationChecklistEntry> action, IEnumerable<ProcessStepTypeId>? _) =>
            {
                action.Invoke(applicationChecklistEntry);
            });

        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(IdWithoutBpn, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, A<IEnumerable<ApplicationChecklistEntryStatusId>>._, A<ProcessStepTypeId>._, A<IEnumerable<ApplicationChecklistEntryTypeId>?>._, A<IEnumerable<ProcessStepTypeId>?>._))
            .ReturnsLazily(() => new IApplicationChecklistService.ManualChecklistProcessStepData(IdWithoutBpn, new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid()), Guid.NewGuid(), ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, new Dictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, new ValueTuple<ApplicationChecklistEntryStatusId, string?>(checklistStatusId, null) }
            }.ToImmutableDictionary(), Enumerable.Empty<ProcessStep>()));
        A.CallTo(() => _checklistService.VerifyChecklistEntryAndProcessSteps(IdWithBpn, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, A<IEnumerable<ApplicationChecklistEntryStatusId>>._, A<ProcessStepTypeId>._, A<IEnumerable<ApplicationChecklistEntryTypeId>?>._, A<IEnumerable<ProcessStepTypeId>?>._))
            .ReturnsLazily(() => new IApplicationChecklistService.ManualChecklistProcessStepData(IdWithoutBpn, new Process(Guid.NewGuid(), ProcessTypeId.APPLICATION_CHECKLIST, Guid.NewGuid()), Guid.NewGuid(), ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, new Dictionary<ApplicationChecklistEntryTypeId, (ApplicationChecklistEntryStatusId, string?)>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, new ValueTuple<ApplicationChecklistEntryStatusId, string?>(ApplicationChecklistEntryStatusId.DONE, null) }
            }.ToImmutableDictionary(), Enumerable.Empty<ProcessStep>()));

        A.CallTo(() => _applicationRepository.GetCompanyIdNameForSubmittedApplication(IdWithBpn))
            .Returns((CompanyId, CompanyName, ExistingExternalId, Enumerable.Repeat((IdpId, IamAliasId, idpTypeId, Enumerable.Repeat(UserId, 1)), 1), Enumerable.Repeat(UserId, 1)));

        A.CallTo(() => _provisioningManager.GetUserByUserName(UserId.ToString()))
            .Returns("user123");

        A.CallTo(() => _checklistService.FinalizeChecklistEntryAndProcessSteps(A<IApplicationChecklistService.ManualChecklistProcessStepData>._, A<Action<ApplicationChecklistEntry>>._, A<Action<ApplicationChecklistEntry>>._, A<IEnumerable<ProcessStepTypeId>?>._))
            .Invokes((IApplicationChecklistService.ManualChecklistProcessStepData _, Action<ApplicationChecklistEntry> _, Action<ApplicationChecklistEntry> action, IEnumerable<ProcessStepTypeId>? _) =>
            {
                action.Invoke(applicationChecklistEntry);
            });

        A.CallTo(() => _applicationRepository.AttachAndModifyCompanyApplication(A<Guid>._, A<Action<CompanyApplication>>._))
            .Invokes((Guid _, Action<CompanyApplication> modify) =>
            {
                modify.Invoke(application);
            });
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(A<Guid>._, null, A<Action<Company>>._))
            .Invokes((Guid _, Action<Company>? _, Action<Company> modify) =>
            {
                modify.Invoke(company);
            });
    }

    #endregion
}
