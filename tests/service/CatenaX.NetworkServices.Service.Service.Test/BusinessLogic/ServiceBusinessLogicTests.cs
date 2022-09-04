/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Service.Service.BusinessLogic;
using CatenaX.NetworkServices.Tests.Shared;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace CatenaX.NetworkServices.Service.Service.Test.BusinessLogic;

public class ServiceBusinessLogicTests
{
    private readonly Guid _companyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");
    private readonly string _notAssignedCompanyIdUser = "395f955b-f11b-4a74-ab51-92a526c1973c";
    private readonly Guid _existingServiceId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
    private readonly CompanyUser _companyUser;
    private readonly IFixture _fixture;
    private readonly IamUser _iamUser;
    private readonly IAgreementRepository _agreementRepository;
    private readonly IAppRepository _appRepository;
    private readonly ICompanyAssignedAppsRepository _companyAssignedAppsRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IUserRepository _userRepository;

    public ServiceBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var (companyUser, iamUser) = CreateTestUserPair();
        _companyUser = companyUser;
        _iamUser = iamUser;

        _portalRepositories = A.Fake<IPortalRepositories>();
        _agreementRepository = A.Fake<IAgreementRepository>();
        _appRepository = A.Fake<IAppRepository>();
        _companyAssignedAppsRepository = A.Fake<ICompanyAssignedAppsRepository>();
        _languageRepository = A.Fake<ILanguageRepository>();
        _userRepository = A.Fake<IUserRepository>();

        SetupRepositories(companyUser, iamUser);
    }

    #region Create Service

    [Fact]
    public async Task CreateServiceOffering_WithValidDataAndEmptyDescriptions_ReturnsCorrectDetails()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        var apps = new List<App>();
        A.CallTo(() => _appRepository.CreateApp(A<string>._, A<AppTypeId>._, A<Action<App?>>._))
            .Invokes(x =>
            {
                var provider = x.Arguments.Get<string>("provider");
                var appTypeId = x.Arguments.Get<AppTypeId>("appType");
                var action = x.Arguments.Get<Action<App?>>("setOptionalParameters");

                var app = new App(serviceId, provider!, DateTimeOffset.UtcNow, appTypeId);
                action?.Invoke(app);
                apps.Add(app);
            })
            .Returns(new App(serviceId)
            {
                AppTypeId = AppTypeId.SERVICE 
            });
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.CreateServiceOffering(new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<ServiceDescription>()), _iamUser.UserEntityId);

        // Assert
        result.Should().Be(serviceId);
        apps.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateServiceOffering_WithValidDataAndDescription_ReturnsCorrectDetails()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        var apps = new List<App>();
        A.CallTo(() => _appRepository.CreateApp(A<string>._, A<AppTypeId>._, A<Action<App?>>._))
            .Invokes(x =>
            {
                var provider = x.Arguments.Get<string>("provider");
                var appTypeId = x.Arguments.Get<AppTypeId>("appType");
                var action = x.Arguments.Get<Action<App?>>("setOptionalParameters");

                var app = new App(serviceId, provider!, DateTimeOffset.UtcNow, appTypeId);
                action?.Invoke(app);
                apps.Add(app);
            })
            .Returns(new App(serviceId)
            {
                AppTypeId = AppTypeId.SERVICE 
            });
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var serviceOfferingData = new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<ServiceDescription>
        {
            new ("en", "That's a description with a valid language code")
        });
        var result = await sut.CreateServiceOffering(serviceOfferingData, _iamUser.UserEntityId);

        // Assert
        result.Should().Be(serviceId);
        apps.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task CreateServiceOffering_WithWrongIamUser_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.CreateServiceOffering(new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<ServiceDescription>()), Guid.NewGuid().ToString());
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    [Fact]
    public async Task CreateServiceOffering_WithInvalidLanguage_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var serviceOfferingData = new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<ServiceDescription>
        {
            new ("gg", "That's a description with incorrect language short code")
        });
        async Task Action() => await sut.CreateServiceOffering(serviceOfferingData, _iamUser.UserEntityId);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("languageCodes");
    }

    [Fact]
    public async Task CreateServiceOffering_WithoutCompanyUser_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.CreateServiceOffering(new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", Guid.NewGuid(), new List<ServiceDescription>()), _iamUser.UserEntityId);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("SalesManager");
    }

    #endregion

    #region Get Active Services

    [Fact]
    public async Task GetAllActiveServicesAsync_WithDefaultRequest_GetsExpectedEntries()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetAllActiveServicesAsync(0, 15);

        // Assert
        result.Content.Should().HaveCount(5);
    }

    
    [Fact]
    public async Task GetAllActiveServicesAsync_WithSmallSize_GetsExpectedEntries()
    {
        // Arrange
        const int expectedCount = 3;
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetAllActiveServicesAsync(0, expectedCount);

        // Assert
        result.Content.Should().HaveCount(expectedCount);
    }

    #endregion

    #region Add Service Subscription

    [Fact]
    public async Task AddServiceSubscription_WithExistingId_CreatesServiceSubscription()
    {
        // Arrange
        var companyAssignedAppId = Guid.NewGuid(); 
        var companyAssignedApps = new List<CompanyAssignedApp>();
        A.CallTo(() => _companyAssignedAppsRepository.CreateCompanyAssignedApp(A<Guid>._, A<Guid>._, A<AppSubscriptionStatusId>._, A<Guid>._, A<Guid>._))
            .Invokes(x =>
            {
                var appId = x.Arguments.Get<Guid>("appId");
                var companyId = x.Arguments.Get<Guid>("companyId");
                var appSubscriptionStatusId = x.Arguments.Get<AppSubscriptionStatusId>("appSubscriptionStatusId");
                var requesterId = x.Arguments.Get<Guid>("requesterId");
                var creatorId = x.Arguments.Get<Guid>("creatorId");

                var companyAssignedApp = new CompanyAssignedApp(companyAssignedAppId, appId, companyId, appSubscriptionStatusId, requesterId, creatorId);
                companyAssignedApps.Add(companyAssignedApp);
            });
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        await sut.AddServiceSubscription(_existingServiceId, _iamUser.UserEntityId);

        // Assert
        companyAssignedApps.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task AddServiceSubscription_WithNotExistingId_ThrowsException()
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.AddServiceSubscription(notExistingServiceId, _iamUser.UserEntityId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Service {notExistingServiceId} does not exist");
    }
    
    [Fact]
    public async Task AddServiceSubscription_NotAssignedCompany_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.AddServiceSubscription(_existingServiceId, _notAssignedCompanyIdUser);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    [Fact]
    public async Task AddServiceSubscription_NotAssignedCompanyUser_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.AddServiceSubscription(_existingServiceId, Guid.NewGuid().ToString());

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    #endregion

    #region Get Service Detail Data

    [Fact]
    public async Task GetServiceDetailsAsync_WithExistingServiceAndLanguageCode_ReturnsServiceDetailData()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetServiceDetailsAsync(_existingServiceId, "en");

        // Assert
        result.Id.Should().Be(_existingServiceId);
    }

    [Fact]
    public async Task GetServiceDetailsAsync_WithoutExistingService_ThrowsException()
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.GetServiceDetailsAsync(notExistingServiceId, "en");

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Service {notExistingServiceId} does not exist");
    }

    #endregion

    #region Get Service Agreement

    [Fact]
    public async Task GetServiceAgreement_WithUserId_ReturnsServiceDetailData()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetServiceAgreement(_iamUser.UserEntityId).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().ContainSingle();
    }

    [Fact]
    public async Task GetServiceAgreement_WithoutExistingService_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var agreementData = await sut.GetServiceAgreement(Guid.NewGuid().ToString()).ToListAsync().ConfigureAwait(false);

        // Assert
        agreementData.Should().BeEmpty();
    }

    #endregion

    #region Setup

    private void SetupRepositories(CompanyUser companyUser, IamUser iamUser)
    {
        var serviceDetailData = new AsyncEnumerableStub<ValueTuple<Guid, string?, string, string?, string?, string?>>(_fixture.CreateMany<ValueTuple<Guid, string?, string, string?, string?, string?>>(5));
        var serviceDetail = _fixture.Build<ServiceDetailData>()
            .With(x => x.Id, _existingServiceId)
            .Create();
        
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(iamUser.UserEntityId, companyUser.Id))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (_companyUser.Id, true, "COMPANYBPN", _companyUserCompanyId), new (_companyUser.Id, false, "OTHERCOMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(iamUser.UserEntityId, A<Guid>.That.Not.Matches(x => x == companyUser.Id)))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (_companyUser.Id, true, "COMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(A<string>.That.Not.Matches(x => x == iamUser.UserEntityId), companyUser.Id))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (_companyUser.Id, false, "OTHERCOMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(A<string>.That.Not.Matches(x => x == iamUser.UserEntityId), A<Guid>.That.Not.Matches(x => x == companyUser.Id)))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>().ToAsyncEnumerable());

        A.CallTo(() => _userRepository.GetOwnCompanAndCompanyUseryId(iamUser.UserEntityId))
            .ReturnsLazily(() => (_companyUser.Id, _companyUser.CompanyId));
        A.CallTo(() => _userRepository.GetOwnCompanAndCompanyUseryId(_notAssignedCompanyIdUser))
            .ReturnsLazily(() => (_companyUser.Id, Guid.Empty));
        A.CallTo(() => _userRepository.GetOwnCompanAndCompanyUseryId(A<string>.That.Not.Matches(x => x == iamUser.UserEntityId || x == _notAssignedCompanyIdUser)))
            .ReturnsLazily(() => (Guid.Empty, _companyUser.CompanyId));
        
        A.CallTo(() => _appRepository.GetActiveServices())
            .Returns(serviceDetailData.AsQueryable());
        
        A.CallTo(() => _appRepository.GetServiceDetailByIdUntrackedAsync(_existingServiceId, A<string>.That.Matches(x => x == "en")))
            .ReturnsLazily(() => serviceDetail);
        A.CallTo(() => _appRepository.GetServiceDetailByIdUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<string>._))
            .ReturnsLazily(() => default(ServiceDetailData));
        
        A.CallTo(() => _languageRepository.GetLanguageCodesUntrackedAsync(A<ICollection<string>>.That.Matches(x => x.Count == 1 && x.All(y => y == "en"))))
            .Returns(new List<string> { "en" }.ToAsyncEnumerable());
        A.CallTo(() => _languageRepository.GetLanguageCodesUntrackedAsync(A<ICollection<string>>.That.Matches(x => x.Count == 1 && x.All(y => y == "gg"))))
            .Returns(new List<string>().ToAsyncEnumerable());
        
        A.CallTo(() => _appRepository.CheckAppExistsById(_existingServiceId))
            .Returns(true);
        A.CallTo(() => _appRepository.CheckAppExistsById(A<Guid>.That.Not.Matches(x => x == _existingServiceId)))
            .Returns(false);

        var agreementData = _fixture.CreateMany<AgreementData>(1);
        A.CallTo(() => _agreementRepository.GetAgreementDataWithAppIdSet(A<string>.That.Matches(x => x == iamUser.UserEntityId)))
            .Returns(agreementData.ToAsyncEnumerable());
        A.CallTo(() => _agreementRepository.GetAgreementDataWithAppIdSet(A<string>.That.Not.Matches(x => x == iamUser.UserEntityId)))
            .Returns(new List<AgreementData>().ToAsyncEnumerable());

        A.CallTo(() => _portalRepositories.GetInstance<IAppRepository>()).Returns(_appRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IAgreementRepository>()).Returns(_agreementRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyAssignedAppsRepository>()).Returns(_companyAssignedAppsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ILanguageRepository>()).Returns(_languageRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
    }

    private (CompanyUser, IamUser) CreateTestUserPair()
    {
        var companyUser = _fixture.Build<CompanyUser>()
            .Without(u => u.IamUser)
            .With(u => u.CompanyId, _companyUserCompanyId)
            .Create();
        var iamUser = _fixture.Build<IamUser>()
            .With(u => u.CompanyUser, companyUser)
            .Create();
        companyUser.IamUser = iamUser;
        return (companyUser, iamUser);
    }

    #endregion
}
