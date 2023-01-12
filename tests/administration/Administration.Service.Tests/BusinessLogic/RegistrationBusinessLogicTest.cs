/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Custodian;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class RegistrationBusinessLogicTest
{
    private static readonly Guid Id = new("d90995fe-1241-4b8d-9f5c-f3909acc6383");
    private static readonly Guid IdWithoutBpn = new("d90995fe-1241-4b8d-9f5c-f3909acc6399");
    private static readonly string AccessToken = "THISISTHEACCESSTOKEN";
    private static readonly string IamUserId = new Guid("4C1A6851-D4E7-4E10-A011-3732CD045E8A").ToString();
    private static readonly Guid ApplicationId = new("6084d6e0-0e01-413c-850d-9f944a6c494c");
    private static readonly Guid CompanyUserId1 = new("857b93b1-8fcb-4141-81b0-ae81950d489e");
    private static readonly Guid CompanyUserId2 = new("857b93b1-8fcb-4141-81b0-ae81950d489f");
    private static readonly Guid CompanyUserId3 = new("857b93b1-8fcb-4141-81b0-ae81950d48af");
    private static readonly Guid CompanyUserRoleId = new("607818be-4978-41f4-bf63-fa8d2de51154");
    private static readonly Guid CentralUserId1 = new("6bc51706-9a30-4eb9-9e60-77fdd6d9cd6f");
    private static readonly Guid CentralUserId2 = new("6bc51706-9a30-4eb9-9e60-77fdd6d9cd70");
    private static readonly Guid CentralUserId3 = new("6bc51706-9a30-4eb9-9e60-77fdd6d9cd71");
    private static readonly Guid UserRoleId = new("607818be-4978-41f4-bf63-fa8d2de51154");
    private const string BusinessPartnerNumber = "CAXLSHAREDIDPZZ";
    private const string CompanyName = "Shared Idp Test";
    private const string ClientId = "catenax-portal";

    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUserBusinessPartnerRepository _businessPartnerRepository;
    private readonly IUserRolesRepository _rolesRepository;
    private readonly ICustodianService _custodianService;
    private readonly IFixture _fixture;
    private readonly RegistrationBusinessLogic _logic;
    private readonly RegistrationSettings _settings;
    private readonly List<Notification> _notifications = new();
    private readonly INotificationService _notificationService;
    private readonly ISdFactoryService _sdFactory;
    private readonly ICompanyRepository _companyRepository;
    private readonly IBpdmService _bpdmService;
    private readonly IMailingService _mailingService;

    public RegistrationBusinessLogicTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());  

        _provisioningManager = A.Fake<IProvisioningManager>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _businessPartnerRepository = A.Fake<IUserBusinessPartnerRepository>();
        _rolesRepository = A.Fake<IUserRolesRepository>();
        _custodianService = A.Fake<ICustodianService>();
        _settings = A.Fake<RegistrationSettings>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _bpdmService = A.Fake<IBpdmService>();

        var userRepository = A.Fake<IUserRepository>();
        var options = A.Fake<IOptions<RegistrationSettings>>();
        _mailingService = A.Fake<IMailingService>();
        _notificationService = A.Fake<INotificationService>();
        _sdFactory = A.Fake<ISdFactoryService>();
        
        _settings.WelcomeNotificationTypeIds = new List<NotificationTypeId>
        {
            NotificationTypeId.WELCOME,
            NotificationTypeId.WELCOME_USE_CASES,
            NotificationTypeId.WELCOME_APP_MARKETPLACE,
            NotificationTypeId.WELCOME_SERVICE_PROVIDER,
            NotificationTypeId.WELCOME_CONNECTOR_REGISTRATION
        };
        _settings.ApplicationsMaxPageSize = 15;

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserBusinessPartnerRepository>()).Returns(_businessPartnerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_rolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => options.Value).Returns(_settings);

        A.CallTo(() => userRepository.GetCompanyUserIdForIamUserUntrackedAsync(IamUserId))
            .ReturnsLazily(Guid.NewGuid);

        _logic = new RegistrationBusinessLogic(_portalRepositories, options, _provisioningManager, _custodianService, _mailingService, _notificationService, _sdFactory, _bpdmService);
    }
    
    #region ApprovePartnerRequest

    [Fact]
    public async Task ApprovePartnerRequest_WithCompanyAdminUser_ApprovesRequestAndCreatesNotifications()
    {
        //Arrange
        var roles = new List<string> { "Company Admin" };
        var clientRoleNames = new Dictionary<string, IEnumerable<string>>
        {
            { ClientId, roles.AsEnumerable() }
        };
        var userRoleData = new List<UserRoleData>() { new(UserRoleId, ClientId, "Company Admin") };

        var companyUserAssignedRole = _fixture.Create<CompanyUserAssignedRole>();
        var companyUserAssignedBusinessPartner = _fixture.Create<CompanyUserAssignedBusinessPartner>();

        SetupFakes(clientRoleNames, userRoleData, companyUserAssignedRole, companyUserAssignedBusinessPartner);

        //Act
        var result = await _logic.ApprovePartnerRequest(IamUserId, AccessToken, Id, CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForSubmittedApplicationAsync(Id)).MustHaveHappened(1, Times.Exactly);
        A.CallTo(() => _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(Id)).MustHaveHappened(1, Times.Exactly);
        A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(CompanyUserId1, UserRoleId)).MustHaveHappened(1, Times.Exactly);
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId1, BusinessPartnerNumber)).MustHaveHappened(1, Times.Exactly);
        A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(CompanyUserId2, UserRoleId)).MustNotHaveHappened();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId2, BusinessPartnerNumber)).MustNotHaveHappened();
        A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(CompanyUserId3, UserRoleId)).MustHaveHappened(1, Times.Exactly);
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId3, BusinessPartnerNumber)).MustHaveHappened(1, Times.Exactly);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened(1, Times.OrMore);
        A.CallTo(() => _custodianService.CreateWalletAsync(BusinessPartnerNumber, CompanyName, A<CancellationToken>._)).MustHaveHappened(1, Times.OrMore);
        Assert.IsType<bool>(result);
        Assert.True(result);
        _notifications.Should().HaveCount(5);
    }

    [Fact]
    public async Task ApprovePartnerRequest_WithFailingWalletCreation_ChangesAreSavedInDatabaseAndMailGetsSend()
    {
        //Arrange
        var roles = new List<string> { "Company Admin" };
        var clientRoleNames = new Dictionary<string, IEnumerable<string>>
        {
            { ClientId, roles.AsEnumerable() }
        };
        var userRoleData = new List<UserRoleData> { new(UserRoleId, ClientId, "Company Admin") };
        
        var companyUserAssignedRole = _fixture.Create<CompanyUserAssignedRole>();
        var companyUserAssignedBusinessPartner = _fixture.Create<CompanyUserAssignedBusinessPartner>();

        SetupFakes(clientRoleNames, userRoleData, companyUserAssignedRole, companyUserAssignedBusinessPartner);
        A.CallTo(() => _custodianService.CreateWalletAsync(BusinessPartnerNumber, CompanyName, A<CancellationToken>._))
            .Throws(new ServiceException("error"));

        //Act
        var result = await _logic.ApprovePartnerRequest(IamUserId, AccessToken, Id, CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForSubmittedApplicationAsync(Id)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(Id)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(CompanyUserId1, UserRoleId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId1, BusinessPartnerNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(CompanyUserId2, UserRoleId)).MustNotHaveHappened();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId2, BusinessPartnerNumber)).MustNotHaveHappened();
        A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(CompanyUserId3, UserRoleId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId3, BusinessPartnerNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened(1, Times.OrMore);
        A.CallTo(() => _custodianService.CreateWalletAsync(BusinessPartnerNumber, CompanyName, A<CancellationToken>._)).MustHaveHappened(1, Times.OrMore);
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>._)).MustHaveHappened(3, Times.Exactly);
        Assert.IsType<bool>(result);
        Assert.True(result);
        _notifications.Should().HaveCount(5);
    }

    [Fact]
    public async Task ApprovePartnerRequest_WithDefaultApplicationId_ThrowsArgumentNullException()
    {
        //Act
        async Task Action() => await _logic.ApprovePartnerRequest(IamUserId, AccessToken, Guid.Empty, CancellationToken.None).ConfigureAwait(false);
        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(Action);
        ex.ParamName.Should().Be("applicationId");
    }

    [Fact]
    public async Task ApprovePartnerRequest_WithNotAssignedUser_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var iamUserId = Guid.NewGuid().ToString();

        //Act
        async Task Action() => await _logic.ApprovePartnerRequest(iamUserId, AccessToken, Id, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Action);
        ex.Message.Should().Be($"user {iamUserId} is not associated with a companyuser");
    }

    [Fact]
    public async Task ApprovePartnerRequest_WithoutCompanyApplication_ThrowsNotFoundException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForSubmittedApplicationAsync(applicationId))
            .ReturnsLazily(() => new ValueTuple<Guid, string, string?, string>());

        //Act
        async Task Action() => await _logic.ApprovePartnerRequest(IamUserId, AccessToken, applicationId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"CompanyApplication {applicationId} is not in status SUBMITTED");
    }

    [Fact]
    public async Task ApprovePartnerRequest_WithCompanyWithoutBPN_ThrowsArgumentException()
    {
        //Act
        async Task Action() => await _logic.ApprovePartnerRequest(IamUserId, AccessToken, IdWithoutBpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.Message.Should().Be($"BusinessPartnerNumber (bpn) for CompanyApplications {IdWithoutBpn} company {Guid.Empty} is empty (Parameter 'bpn')");
        ex.ParamName.Should().Be($"bpn");
    }

    #endregion

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
        var result = await _logic.GetCompanyApplicationDetailsAsync(0, 5,null,null).ConfigureAwait(false);
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
        var result = await _logic.GetCompanyApplicationDetailsAsync(0, 5,CompanyApplicationStatusFilter.InReview,null).ConfigureAwait(false);
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
        var result = await _logic.GetCompanyApplicationDetailsAsync(0, 5,CompanyApplicationStatusFilter.Closed,null).ConfigureAwait(false);
        // Assert
        A.CallTo(() => _applicationRepository.GetCompanyApplicationsFilteredQuery(null, A<IEnumerable<CompanyApplicationStatusId>>.That.Matches(x => x.Count() == 2 && x.All(y => companyAppStatus.Contains(y))))).MustHaveHappenedOnceExactly();
        Assert.IsType<Pagination.Response<CompanyApplicationDetails>>(result);
        result.Content.Should().HaveCount(5);       
    }

    #endregion

    #region GetCompanyWithAddressAsync

    [Fact]
    public async Task GetCompanyWithAddressAsync_WithDefaultRequest_GetsExpectedResult()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var data = _fixture.Build<CompanyUserRoleWithAddress>()
            .With(x => x.AgreementsData, _fixture.CreateMany<AgreementsData>(20))
            .Create();
        A.CallTo(() => _applicationRepository.GetCompanyUserRoleWithAdressUntrackedAsync(applicationId))
            .Returns(data);

        // Act
        var result = await _logic.GetCompanyWithAddressAsync(applicationId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationRepository.GetCompanyUserRoleWithAdressUntrackedAsync(applicationId)).MustHaveHappenedOnceExactly();
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
            r.ZipCode == data.Zipcode &&
            r.CountryDe == data.CountryDe
        );
        result.AgreementsRoleData.Should().HaveSameCount(data.AgreementsData.DistinctBy(ad => ad.CompanyRoleId));
        result.InvitedUserData.Should().HaveSameCount(data.InvitedCompanyUserData);
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
        A.CallTo(() => _applicationRepository.GetCompanyUserRoleWithAdressUntrackedAsync(applicationId))
            .Returns(data);

        // Act
        var result = await _logic.GetCompanyWithAddressAsync(applicationId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationRepository.GetCompanyUserRoleWithAdressUntrackedAsync(applicationId)).MustHaveHappenedOnceExactly();
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
            r.ZipCode == "" &&
            r.CountryDe == ""
        );
        result.InvitedUserData.Should().HaveSameCount(data.InvitedCompanyUserData);
        result.InvitedUserData.Should().AllSatisfy(u => u.Should().Match<InvitedUserData>(u => u.FirstName == "" && u.LastName == "" && u.Email == ""));
    }

    #endregion

    #region Trigger bpn data push

    [Fact]
    public async Task TriggerBpnDataPush_WithValidData_CallsService()
    {
        // Act
        var data = _fixture
            .Build<BpdmData>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .With(x => x.IsUserInCompany, true)
            .Create();
        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(IamUserId, ApplicationId))
            .ReturnsLazily(() => data);

        await _logic.TriggerBpnDataPushAsync(IamUserId, ApplicationId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(A<BpdmTransferData>._, CancellationToken.None)).MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task TriggerBpnDataPush_WithNotExistingApplication_ThrowsNotFoundException()
    {
        // Arrange
        var notExistingApplicationId = Guid.NewGuid();
        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(IamUserId, notExistingApplicationId))
            .ReturnsLazily(() => (BpdmData?)null);

        // Act
        async Task Act() => await _logic.TriggerBpnDataPushAsync(IamUserId, notExistingApplicationId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Application {notExistingApplicationId} does not exists.");
    }

    [Fact]
    public async Task TriggerBpnDataPush_WithNotSubmittedApplication_ThrowsArgumentException()
    {
        // Arrange
        var createdApplicationId = Guid.NewGuid();
        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(IamUserId, createdApplicationId))
            .ReturnsLazily(() => new BpdmData(CompanyApplicationStatusId.CREATED, null!, null!, null!, null!, null!, true));

        // Act
        async Task Act() => await _logic.TriggerBpnDataPushAsync(IamUserId, createdApplicationId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
        ex.ParamName.Should().Be("applicationId");
    }

    [Fact]
    public async Task TriggerBpnDataPush_WithNotExistingUser_ThrowsArgumentException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var wrongUserId = Guid.NewGuid().ToString();
        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(wrongUserId, applicationId))
            .ReturnsLazily(() => new BpdmData(CompanyApplicationStatusId.SUBMITTED, null!, null!, null!, null!, null!, false));

        // Act
        async Task Act() => await _logic.TriggerBpnDataPushAsync(wrongUserId, applicationId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.ParamName.Should().Be("iamUserId");
    }

    [Fact]
    public async Task TriggerBpnDataPush_WithEmptyZipCode_ThrowsConflictException()
    {
        // Arrange
        var createdApplicationId = _fixture.Create<Guid>();
        var data = _fixture.Build<BpdmData>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .With(x => x.IsUserInCompany, true)
            .With(x => x.ZipCode, (string?)null)
            .Create();
        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(IamUserId, createdApplicationId))
            .ReturnsLazily(() => data);

        // Act
        async Task Act() => await _logic.TriggerBpnDataPushAsync(IamUserId, createdApplicationId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("ZipCode must not be empty");
    }

    #endregion
    
    #region Setup

    private void SetupFakes(
        IDictionary<string, IEnumerable<string>> clientRoleNames,
        IEnumerable<UserRoleData> userRoleData,
        CompanyUserAssignedRole companyUserAssignedRole,
        CompanyUserAssignedBusinessPartner companyUserAssignedBusinessPartner)
    {
        var company = _fixture.Build<Company>()
            .With(u => u.BusinessPartnerNumber, BusinessPartnerNumber)
            .With(u => u.Name, CompanyName)
            .Create();

        var companyInvitedUsers = new List<CompanyInvitedUserData>
        {
            new(CompanyUserId1, CentralUserId1.ToString(), Enumerable.Empty<string>(), Enumerable.Empty<Guid>()),
            new(CompanyUserId2, CentralUserId2.ToString(), Enumerable.Repeat(BusinessPartnerNumber, 1), Enumerable.Repeat(UserRoleId, 1)),
            new(CompanyUserId3, CentralUserId3.ToString(), Enumerable.Empty<string>(), Enumerable.Empty<Guid>())
        }.ToAsyncEnumerable();
        var businessPartnerNumbers = new List<string> { BusinessPartnerNumber }.AsEnumerable();

        _settings.ApplicationApprovalInitialRoles = clientRoleNames;
        _settings.CompanyAdminRoles = new Dictionary<string, IEnumerable<string>>
        {
            { ClientId, new List<string> { "Company Admin" }.AsEnumerable() }
        };

        
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForSubmittedApplicationAsync(A<Guid>.That.Matches(x => x == Id)))
            .ReturnsLazily(() => new ValueTuple<Guid, string, string?, string>(company.Id, company.Name, company.BusinessPartnerNumber!, "de"));
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForSubmittedApplicationAsync(A<Guid>.That.Matches(x => x == IdWithoutBpn)))
            .ReturnsLazily(() => new ValueTuple<Guid, string, string?, string>(IdWithoutBpn, company.Name, null, "de"));

        var welcomeEmailData = new List<WelcomeEmailData>();
        welcomeEmailData.AddRange(new WelcomeEmailData[]
        {
            new (CompanyUserId1, "Stan", "Lee", "stan@lee.com", company.Name),
            new (CompanyUserId2, "Tony", "Stark", "tony@stark.com", company.Name),
            new (CompanyUserId3, "Peter", "Parker", "peter@parker.com", company.Name)
        });
        A.CallTo(() => _applicationRepository.GetWelcomeEmailDataUntrackedAsync(Id, A<IEnumerable<Guid>>._))
            .Returns(welcomeEmailData.ToAsyncEnumerable());
        A.CallTo(() => _applicationRepository.GetWelcomeEmailDataUntrackedAsync(A<Guid>.That.Not.Matches(x => x == Id), A<IEnumerable<Guid>>._))
            .Returns(new List<WelcomeEmailData>().ToAsyncEnumerable());

        A.CallTo(() => _rolesRepository.GetUserRoleDataUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>.That.Matches(x => x[ClientId].First() == clientRoleNames[ClientId].First())))
            .Returns(userRoleData.ToAsyncEnumerable());

        A.CallTo(() => _rolesRepository.GetUserRoleDataUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>.That.Matches(x => x[ClientId].First() == _settings.CompanyAdminRoles[ClientId].First())))
            .Returns(new List<UserRoleData>() { new(UserRoleId, ClientId, "Company Admin") }.ToAsyncEnumerable());

        A.CallTo(() => _applicationRepository.GetInvitedUsersDataByApplicationIdUntrackedAsync(Id))
            .Returns(companyInvitedUsers);

        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(CentralUserId1.ToString(), clientRoleNames))
            .Returns(clientRoleNames.Select(x => (Client: x.Key, Roles: x.Value)).ToAsyncEnumerable());
        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(CentralUserId2.ToString(), clientRoleNames))
            .Returns(clientRoleNames.Select(x => (Client: x.Key, Roles: x.Value)).ToAsyncEnumerable());
        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(CentralUserId3.ToString(), clientRoleNames))
            .Returns(clientRoleNames.Select(x => (Client: x.Key, Roles: x.Value)).ToAsyncEnumerable());

        A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(CentralUserId1.ToString(), businessPartnerNumbers))
            .Returns(Task.CompletedTask);
        A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(CentralUserId2.ToString(), businessPartnerNumbers))
            .Returns(Task.CompletedTask);
        A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(CentralUserId3.ToString(), businessPartnerNumbers))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(CompanyUserId1, CompanyUserRoleId))
            .Returns(companyUserAssignedRole);
        A.CallTo(() => _rolesRepository.CreateCompanyUserAssignedRole(CompanyUserId3, CompanyUserRoleId))
            .Returns(companyUserAssignedRole);

        A.CallTo(() =>
                _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId1, BusinessPartnerNumber))
            .Returns(companyUserAssignedBusinessPartner);
        A.CallTo(() =>
                _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(CompanyUserId3, BusinessPartnerNumber))
            .Returns(companyUserAssignedBusinessPartner);

        A.CallTo(() => _portalRepositories.SaveAsync())
            .Returns(1);

        A.CallTo(() => _custodianService.CreateWalletAsync(BusinessPartnerNumber, CompanyName, A<CancellationToken>._))
            .Returns(Task.CompletedTask);
            
        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, A<Guid>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._))
            .Invokes((
                IDictionary<string,IEnumerable<string>> _, 
                Guid? creatorId, 
                IEnumerable<(string? content, NotificationTypeId notificationTypeId)> notifications, 
                Guid _) =>
            {
                foreach (var notificationData in notifications)
                {
                    var notification = new Notification(Guid.NewGuid(), Guid.NewGuid(),
                        DateTimeOffset.UtcNow, notificationData.notificationTypeId, false)
                    {
                        CreatorUserId = creatorId,
                        Content = notificationData.content
                    };
                    _notifications.Add(notification);
                }
            });
    }

    #endregion
}
