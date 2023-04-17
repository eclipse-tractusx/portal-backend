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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Collections.Immutable;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic.Tests;

public class AppChangeBusinessLogicTest
{
    private const string ClientId = "catenax-portal";
    private readonly Guid _companyUserId = Guid.NewGuid();
    private readonly string _iamUserId = Guid.NewGuid().ToString();

    private readonly IFixture _fixture;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferRepository _offerRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly INotificationService _notificationService;
    private readonly AppChangeBusinessLogic _sut;

    public AppChangeBusinessLogicTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _provisioningManager = A.Fake<IProvisioningManager>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerRepository = A.Fake<IOfferRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _notificationService = A.Fake<INotificationService>();

        var settings = new AppsSettings
        {
            ActiveAppNotificationTypeIds = new []
            {
                NotificationTypeId.APP_ROLE_ADDED
            },
            ActiveAppCompanyAdminRoles = new Dictionary<string, IEnumerable<string>>
            {
                { ClientId, new [] { "Company Admin" } }
            }
        };
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        _sut = new AppChangeBusinessLogic(_portalRepositories, _notificationService, _provisioningManager, Options.Create(settings));
    }

    #region  AddActiveAppUserRole

    [Fact]
    public async Task AddActiveAppUserRoleAsync_ExecutesSuccessfully()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var companyId = _fixture.Create<Guid>();
        var appName = _fixture.Create<string>();
        var appAssignedRoleDesc = _fixture.CreateMany<string>(3).Select(role => new AppUserRole(role, _fixture.CreateMany<AppUserRoleDescription>(2).ToImmutableArray())).ToImmutableArray();
        var clientIds = new[] {"client"};

        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>().GetInsertActiveAppUserRoleDataAsync(appId, _iamUserId, OfferTypeId.APP))
            .ReturnsLazily(() => (true, appName, _companyUserId, companyId, clientIds));

        IEnumerable<UserRole>? userRoles = null;
        A.CallTo(() => _userRolesRepository.CreateAppUserRoles(A<IEnumerable<(Guid,string)>>._))
            .ReturnsLazily((IEnumerable<(Guid AppId, string Role)> appRoles) =>
            {
                userRoles = appRoles.Select(x => new UserRole(Guid.NewGuid(), x.Role, x.AppId)).ToImmutableArray();
                return userRoles;
            });

        var userRoleDescriptions = new List<IEnumerable<UserRoleDescription>>();
        A.CallTo(() => _userRolesRepository.CreateAppUserRoleDescriptions(A<IEnumerable<(Guid,string,string)>>._))
            .ReturnsLazily((IEnumerable<(Guid RoleId, string LanguageCode, string Description)> roleLanguageDescriptions) =>
            {
                var createdUserRoleDescriptions = roleLanguageDescriptions.Select(x => new UserRoleDescription(x.RoleId, x.LanguageCode, x.Description)).ToImmutableArray();
                userRoleDescriptions.Add(createdUserRoleDescriptions);
                return createdUserRoleDescriptions;
            });

        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, A<Guid>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._))
            .Returns(_fixture.CreateMany<Guid>(4).AsFakeIAsyncEnumerable(out var createNotificationsResultAsyncEnumerator));

        //Act
        var result = await _sut.AddActiveAppUserRoleAsync(appId, appAssignedRoleDesc, _iamUserId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _offerRepository.GetInsertActiveAppUserRoleDataAsync(appId, _iamUserId, OfferTypeId.APP)).MustHaveHappened();

        A.CallTo(() => _userRolesRepository.CreateAppUserRoles(A<IEnumerable<(Guid,string)>>._)).MustHaveHappenedOnceExactly();
        userRoles.Should().NotBeNull()
            .And.HaveSameCount(appAssignedRoleDesc)
            .And.AllSatisfy(x =>
            {
                x.Id.Should().NotBeEmpty();
                x.OfferId.Should().Be(appId);
            })
            .And.Satisfy(
                x => x.UserRoleText == appAssignedRoleDesc[0].Role,
                x => x.UserRoleText == appAssignedRoleDesc[1].Role,
                x => x.UserRoleText == appAssignedRoleDesc[2].Role
            );

        A.CallTo(() => _userRolesRepository.CreateAppUserRoleDescriptions(A<IEnumerable<(Guid,string,string)>>._)).MustHaveHappened(appAssignedRoleDesc.Length, Times.Exactly);
        userRoleDescriptions.Should()
            .HaveSameCount(appAssignedRoleDesc)
            .And.SatisfyRespectively(
                x => x.Should().HaveCount(2).And.Satisfy(
                    x => x.UserRoleId == userRoles!.ElementAt(0).Id && x.LanguageShortName == appAssignedRoleDesc[0].Descriptions.ElementAt(0).LanguageCode && x.Description == appAssignedRoleDesc[0].Descriptions.ElementAt(0).Description,
                    x => x.UserRoleId == userRoles!.ElementAt(0).Id && x.LanguageShortName == appAssignedRoleDesc[0].Descriptions.ElementAt(1).LanguageCode && x.Description == appAssignedRoleDesc[0].Descriptions.ElementAt(1).Description),
                x => x.Should().HaveCount(2).And.Satisfy(
                    x => x.UserRoleId == userRoles!.ElementAt(1).Id && x.LanguageShortName == appAssignedRoleDesc[1].Descriptions.ElementAt(0).LanguageCode && x.Description == appAssignedRoleDesc[1].Descriptions.ElementAt(0).Description,
                    x => x.UserRoleId == userRoles!.ElementAt(1).Id && x.LanguageShortName == appAssignedRoleDesc[1].Descriptions.ElementAt(1).LanguageCode && x.Description == appAssignedRoleDesc[1].Descriptions.ElementAt(1).Description),
                x => x.Should().HaveCount(2).And.Satisfy(
                    x => x.UserRoleId == userRoles!.ElementAt(2).Id && x.LanguageShortName == appAssignedRoleDesc[2].Descriptions.ElementAt(0).LanguageCode && x.Description == appAssignedRoleDesc[2].Descriptions.ElementAt(0).Description,
                    x => x.UserRoleId == userRoles!.ElementAt(2).Id && x.LanguageShortName == appAssignedRoleDesc[2].Descriptions.ElementAt(1).LanguageCode && x.Description == appAssignedRoleDesc[2].Descriptions.ElementAt(1).Description));

        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, A<Guid>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => createNotificationsResultAsyncEnumerator.MoveNextAsync())
            .MustHaveHappened(5, Times.Exactly);
        A.CallTo(() => _provisioningManager.AddRolesToClientAsync("client", A<IEnumerable<string>>.That.IsSameSequenceAs(appAssignedRoleDesc.Select(x => x.Role))))
            .MustHaveHappenedOnceExactly();

        result.Should().NotBeNull()
            .And.HaveSameCount(appAssignedRoleDesc)
            .And.Satisfy(
                x => x.RoleId == userRoles!.ElementAt(0).Id && x.RoleName == appAssignedRoleDesc[0].Role,
                x => x.RoleId == userRoles!.ElementAt(1).Id && x.RoleName == appAssignedRoleDesc[1].Role,
                x => x.RoleId == userRoles!.ElementAt(2).Id && x.RoleName == appAssignedRoleDesc[2].Role
            );
    }

    [Fact]
    public async Task AddActiveAppUserRoleAsync_WithCompanyUserIdNotSet_ThrowsForbiddenException()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var appName = _fixture.Create<string>();

        var appUserRoleDescription = new AppUserRoleDescription[] {
            new("de","this is test1"),
            new("en","this is test2"),
        };
        var appAssignedRoleDesc = new AppUserRole[] { new("Legal Admin", appUserRoleDescription) };
        var clientIds = new[] {"client"};
        A.CallTo(() => _offerRepository.GetInsertActiveAppUserRoleDataAsync(appId, _iamUserId, OfferTypeId.APP))
            .ReturnsLazily(() => (true, appName, Guid.Empty, null, clientIds));

        //Act
        async Task Act() => await _sut.AddActiveAppUserRoleAsync(appId, appAssignedRoleDesc, _iamUserId).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"user {_iamUserId} is not a member of the provider company of app {appId}");
    }

    [Fact]
    public async Task AddActiveAppUserRoleAsync_WithProviderCompanyNotSet_ThrowsConflictException()
    {
        //Arrange
        const string appName = "app name";
        var appId = _fixture.Create<Guid>();

        var appUserRoleDescription = new AppUserRoleDescription[] {
            new("de","this is test1"),
            new("en","this is test2"),
        };
        var appAssignedRoleDesc = new AppUserRole[] { new("Legal Admin", appUserRoleDescription) };
        var clientIds = new[] {"client"};
        A.CallTo(() => _offerRepository.GetInsertActiveAppUserRoleDataAsync(appId, _iamUserId, OfferTypeId.APP))
            .ReturnsLazily(() => (true, appName, _companyUserId, null, clientIds));

        //Act
        async Task Act() => await _sut.AddActiveAppUserRoleAsync(appId, appAssignedRoleDesc, _iamUserId).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"App {appId} providing company is not yet set.");
    }

    #endregion

    #region  AppDescription

   [Fact]
    public async Task GetAppUpdateDescritionByIdAsync_ReturnsExpected()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var offerDescription = _fixture.CreateMany<LocalizedDescription>(3);
        var appDescriptionData = (IsStatusActive: true, IsProviderCompanyUser: true, OfferDescriptionDatas: offerDescription);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .ReturnsLazily(() => appDescriptionData);
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), null!);

        // Act
        var result = await _sut.GetAppUpdateDescriptionByIdAsync(appId, _iamUserId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Select(x => x.LanguageCode).Should().Contain(offerDescription.Select(od => od.LanguageCode));
    }

    [Fact]    
    public async Task GetAppUpdateDescritionByIdAsync_ThrowsNotFoundException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .ReturnsLazily(() => ((bool IsStatusActive, bool IsProviderCompanyUser, IEnumerable<LocalizedDescription> OfferDescriptionDatas))default);
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), null!);

        // Act
        async Task Act() => await _sut.GetAppUpdateDescriptionByIdAsync(appId, _iamUserId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"App {appId} does not exist.");  
    }
    
    [Fact]    
    public async Task GetAppUpdateDescritionByIdAsync_ThrowsConflictException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var offerDescription = _fixture.CreateMany<LocalizedDescription>(3);
        var appDescriptionData = (IsStatusActive: false, IsProviderCompanyUser: true, OfferDescriptionDatas: offerDescription);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .ReturnsLazily(() => appDescriptionData);
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), null!);

        // Act
        async Task Act() => await _sut.GetAppUpdateDescriptionByIdAsync(appId, _iamUserId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"App {appId} is in InCorrect Status");  
    }

    [Fact]    
    public async Task GetAppUpdateDescritionByIdAsync_ThrowsForbiddenException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var offerDescription = _fixture.CreateMany<LocalizedDescription>(3);
        var appDescriptionData = (IsStatusActive: true, IsProviderCompanyUser: false, OfferDescriptionDatas: offerDescription);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .ReturnsLazily(() => appDescriptionData);
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), null!);

        // Act
        async Task Act() => await _sut.GetAppUpdateDescriptionByIdAsync(appId, _iamUserId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"user {_iamUserId} is not a member of the providercompany of App {appId}");  
    }

    [Fact]
    public async Task GetAppUpdateDescritionByIdAsync_withNullDescriptionData_ThowsUnexpectedConditionException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var offerDescription = _fixture.CreateMany<LocalizedDescription>(3);
        var appDescriptionData = (IsStatusActive: true, IsProviderCompanyUser: true, OfferDescriptionDatas: (IEnumerable<LocalizedDescription>?)null);
        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .ReturnsLazily(() => appDescriptionData);

        // Act
        var Act = () => _sut.GetAppUpdateDescriptionByIdAsync(appId, _iamUserId);

        // Assert
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);
        result.Message.Should().Be("offerDescriptionDatas should never be null here");

    }

    [Fact]
    public async Task CreateOrUpdateAppDescriptionByIdAsync_withEmptyExistingDescriptionData_ReturnExpectedResult()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var updateDescriptionData =  new []{
            new LocalizedDescription("en", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("de", _fixture.Create<string>(), _fixture.Create<string>())
            };
        var appDescriptionData = (IsStatusActive: true, IsProviderCompanyUser: true, OfferDescriptionDatas: (IEnumerable<LocalizedDescription>)updateDescriptionData);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .ReturnsLazily(() => appDescriptionData);
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), null!);

        // Act
        await _sut.CreateOrUpdateAppDescriptionByIdAsync(appId, _iamUserId, updateDescriptionData).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerRepository.CreateUpdateDeleteOfferDescriptions(appId, A<IEnumerable<LocalizedDescription>>._, A<IEnumerable<(string,string,string)>>.That.IsSameSequenceAs(updateDescriptionData.Select(x => new ValueTuple<string,string,string>(x.LanguageCode, x.LongDescription, x.ShortDescription)))))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateOrUpdateAppDescriptionByIdAsync_withNullDescriptionData_ThowsUnexpectedConditionException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var updateDescriptionData =  new []{
            new LocalizedDescription("en", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("de", _fixture.Create<string>(), _fixture.Create<string>())
            };
        var appDescriptionData = (IsStatusActive: true, IsProviderCompanyUser: true, OfferDescriptionDatas: (IEnumerable<LocalizedDescription>)null!);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .ReturnsLazily(() => appDescriptionData);

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), null!);

        // Act
        var Act = () => _sut.CreateOrUpdateAppDescriptionByIdAsync(appId, _iamUserId, updateDescriptionData);

        // Assert
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);
        result.Message.Should().Be("offerDescriptionDatas should never be null here");

    }

    [Fact]
    public async Task CreateOrUpdateAppDescriptionByIdAsync_ThrowsForbiddenException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var updateDescriptionData =  new []{
            new LocalizedDescription("en", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("de", _fixture.Create<string>(), _fixture.Create<string>())
            };
        var appDescriptionData = (IsStatusActive: true, IsProviderCompanyUser: false, OfferDescriptionDatas: (IEnumerable<LocalizedDescription>)null!);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .ReturnsLazily(() => appDescriptionData);

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), null!);

        // Act
        var Act = () => _sut.CreateOrUpdateAppDescriptionByIdAsync(appId, _iamUserId, updateDescriptionData);

        // Assert
        var result = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"user {_iamUserId} is not a member of the providercompany of App {appId}");
        
    }

    [Fact]
    public async Task CreateOrUpdateAppDescriptionByIdAsync_ThrowsConflictException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var updateDescriptionData =  new []{
            new LocalizedDescription("en", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("de", _fixture.Create<string>(), _fixture.Create<string>())
            };
        var appDescriptionData = (IsStatusActive: false, IsProviderCompanyUser: true, OfferDescriptionDatas: (IEnumerable<LocalizedDescription>)null!);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .ReturnsLazily(() => appDescriptionData);

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), null!);

        // Act
        var Act = () => _sut.CreateOrUpdateAppDescriptionByIdAsync(appId, _iamUserId, updateDescriptionData);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"App {appId} is in InCorrect Status");
        
    }

     [Fact]
    public async Task CreateOrUpdateAppDescriptionByIdAsync_ThrowsNotFoundException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var updateDescriptionData =  new []{
            new LocalizedDescription("en", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("de", _fixture.Create<string>(), _fixture.Create<string>())
            };

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
             .ReturnsLazily(() => ((bool IsStatusActive, bool IsProviderCompanyUser, IEnumerable<LocalizedDescription> OfferDescriptionDatas))default);

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), null!);

        // Act
        var Act = () => _sut.CreateOrUpdateAppDescriptionByIdAsync(appId, _iamUserId, updateDescriptionData);

        // Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"App {appId} does not exist.");
        
    }

    #endregion
}
