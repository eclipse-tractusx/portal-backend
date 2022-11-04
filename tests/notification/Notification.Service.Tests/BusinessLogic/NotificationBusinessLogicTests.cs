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
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Notification.Service.BusinessLogic;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using FakeItEasy;
using FluentAssertions;
using Notification.Service.Models;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Notification.Service.Tests;

public class NotificationBusinessLogicTests
{
    private readonly CompanyUser _companyUser;
    private readonly IFixture _fixture;
    private readonly IamUser _iamUser;
    private readonly NotificationDetailData _notificationDetail;
    private readonly IEnumerable<NotificationDetailData> _notificationDetails;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IEnumerable<NotificationDetailData> _readNotificationDetails;
    private readonly IEnumerable<NotificationDetailData> _unreadNotificationDetails;
    private readonly IUserRepository _userRepository;

    public NotificationBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var (companyUser, iamUser) = CreateTestUserPair();
        _companyUser = companyUser;
        _iamUser = iamUser;
        _notificationDetail = new NotificationDetailData(Guid.NewGuid(),  DateTime.UtcNow, NotificationTypeId.INFO, NotificationTopicId.INFO, false, "Test Message", null);

        _portalRepositories = A.Fake<IPortalRepositories>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _userRepository = A.Fake<IUserRepository>();

        _readNotificationDetails = _fixture.Build<NotificationDetailData>()
            .CreateMany(1);
        _unreadNotificationDetails = _fixture.Build<NotificationDetailData>()
            .CreateMany(3);
        _notificationDetails = _readNotificationDetails.Concat(_unreadNotificationDetails);
        SetupRepositories(companyUser, iamUser);
    }

    #region Create Notification

    [Fact]
    public async Task CreateNotification_WithValidData_ReturnsCorrectDetails()
    {
        // Arrange
        var notifications = new List<PortalBackend.PortalEntities.Entities.Notification>();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._,
                A<Action<PortalBackend.PortalEntities.Entities.Notification?>>._))
            .Invokes(x =>
            {
                var receiverId = x.Arguments.Get<Guid>("receiverUserId");
                var notificationTypeId = x.Arguments.Get<NotificationTypeId>("notificationTypeId");
                var isRead = x.Arguments.Get<bool>("isRead");
                var action = x.Arguments.Get<Action<PortalBackend.PortalEntities.Entities.Notification?>>("setOptionalParameter");

                var notification = new PortalBackend.PortalEntities.Entities.Notification(Guid.NewGuid(), receiverId,
                    DateTimeOffset.UtcNow, notificationTypeId, NotificationTopicId.INFO, isRead);
                action?.Invoke(notification);
                notifications.Add(notification);
            });
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();
        const string content = "That's a title";

        // Act
        var result = await sut.CreateNotificationAsync(_iamUser.UserEntityId,
            new NotificationCreationData(
                content,
                NotificationTypeId.INFO,
                false), _companyUser.Id)
            .ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        notifications.Should().HaveCount(1);
        var notification = notifications.Single();
        notification.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateNotification_WithNotExistingCompanyUser_ThrowsArgumentException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        async Task Action() => await sut.CreateNotificationAsync(_iamUser.UserEntityId,
                new NotificationCreationData("That's a title",
                    NotificationTypeId.INFO, false), Guid.NewGuid())
            .ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Action);
        ex.ParamName.Should().Be("receiverId");
    }

    #endregion

    #region Get Notifications

    [Fact]
    public async Task GetNotifications_WithUnreadStatus_ReturnsList()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        var result = await sut.GetNotificationsAsync(0, 15, _iamUser.UserEntityId, false, null, NotificationSorting.DateDesc).ConfigureAwait(false);

        // Assert
        result.Content.Should().HaveCount(_unreadNotificationDetails.Count());
    }

    [Fact]
    public async Task GetNotifications_WithReadStatus_ReturnsList()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        var result = await sut.GetNotificationsAsync(0, 15, _iamUser.UserEntityId, true, null, NotificationSorting.DateDesc).ConfigureAwait(false);

        // Assert
        result.Content.Should().HaveCount(_readNotificationDetails.Count());
    }

    [Fact]
    public async Task GetNotifications_WithoutStatus_ReturnsList()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        var result = await sut.GetNotificationsAsync(0, 15, _iamUser.UserEntityId, null, null, NotificationSorting.DateDesc).ConfigureAwait(false);

        // Assert
        result.Content.Should().HaveCount(_notificationDetails.Count());
    }

    #endregion

    #region Get Notification Details

    [Fact]
    public async Task GetNotificationDetailDataAsync_WithIdAndUser_ReturnsCorrectResult()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        var result = await sut.GetNotificationDetailDataAsync(_iamUser.UserEntityId, _notificationDetail.Id);

        // Assert
        var notificationDetailData = _unreadNotificationDetails.First();
        result.Should().Be(notificationDetailData);
    }

    [Fact]
    public async Task GetNotificationDetailDataAsync_WithNotMatchingUser_ThrowsForbiddenException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        var iamUserId = Guid.NewGuid().ToString();
        try
        {
            await sut.GetNotificationDetailDataAsync(iamUserId, _notificationDetail.Id);
        }
        catch (ForbiddenException ex)
        {
            ex.Message.Should().Be($"iamUserId {iamUserId} is not the receiver of the notification");
            return;
        }

        // Assert
        false.Should().BeTrue(); // Must not be hit, because we test the exception here
    }

    [Fact]
    public async Task GetNotificationDetailDataAsync_WithNotMatchingNotificationId_ThrowsNotFoundException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        var notificationId = Guid.NewGuid();
        try
        {
            await sut.GetNotificationDetailDataAsync(_iamUser.UserEntityId, notificationId);
        }
        catch (NotFoundException ex)
        {
            ex.Message.Should().Be($"Notification {notificationId} does not exist.");
            return;
        }

        // Assert
        false.Should().BeTrue(); // Must not be hit, because we test the exception here
    }

    #endregion

    #region Get Notification Count

    [Fact]
    public async Task GetNotificationCountAsync_WithIdAndUser_ReturnsCorrectResult()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        var result = await sut.GetNotificationCountAsync(_iamUser.UserEntityId, false);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public async Task GetNotificationCountAsync_WithNotMatchingUser_ThrowsForbiddenException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        var iamUserId = Guid.NewGuid().ToString();
        try
        {
            await sut.GetNotificationCountAsync(iamUserId, false);
        }
        catch (ForbiddenException ex)
        {
            ex.Message.Should().Be($"iamUserId {iamUserId} is not assigned");
            return;
        }

        // Assert
        false.Should().BeTrue(); // Must not be hit, because we test the exception here
    }

    #endregion

    #region Set Notification To Read

    [Fact]
    public async Task SetNotificationToRead_WithMatchingId_ReturnsDetailData()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        await sut.SetNotificationStatusAsync(_iamUser.UserEntityId, _notificationDetail.Id, true);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SetNotificationToRead_WithNotMatchingNotification_NotFoundException()
    {
        // Arrange
        var randomNotificationId = Guid.NewGuid();
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();
        var notExistingUserId = Guid.NewGuid().ToString();

        // Act
        try
        {
            await sut.SetNotificationStatusAsync(notExistingUserId, randomNotificationId, true);
        }
        catch (NotFoundException e)
        {
            // Assert
            e.Message.Should().Be($"Notification {randomNotificationId} does not exist.");
            return;
        }

        // Must not reach that code because of the exception
        false.Should().BeTrue();
    }

    [Fact]
    public async Task SetNotificationToRead_WithNotExistingCompanyUser_ThrowsForbiddenException()
    {
        // Arrange
        var iamUserId = Guid.NewGuid().ToString();
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        try
        {
            await sut.SetNotificationStatusAsync(iamUserId, _notificationDetail.Id, true);
        }
        catch (ForbiddenException e)
        {
            // Assert
            e.Message.Should().Be($"iamUserId {iamUserId} is not the receiver of the notification");
            return;
        }

        // Must not reach that code because of the exception
        false.Should().BeTrue();
    }

    #endregion

    #region Delete Notifications

    [Fact]
    public async Task DeleteNotification_WithValidData_ExecutesSuccessfully()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        await sut.DeleteNotificationAsync(_iamUser.UserEntityId, _notificationDetail.Id);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteNotification_WithNotExistingCompanyUser_ThrowsForbiddenException()
    {
        // Arrange
        var iamUserId = Guid.NewGuid().ToString();
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        try
        {
            await sut.DeleteNotificationAsync(iamUserId, _notificationDetail.Id);
        }
        catch (ForbiddenException e)
        {
            // Assert
            e.Message.Should().Be($"iamUserId {iamUserId} is not the receiver of the notification");
            return;
        }

        // Must not reach that code because of the exception
        false.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteNotification_WithNotExistingNotification_ThrowsNotFoundException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();
        var randomNotificationId = Guid.NewGuid();

        // Act
        try
        {
            await sut.DeleteNotificationAsync(_iamUser.UserEntityId, randomNotificationId);
        }
        catch (NotFoundException e)
        {
            // Assert
            e.Message.Should().Be($"Notification {randomNotificationId} does not exist.");
            return;
        }

        // Must not reach that code because of the exception
        false.Should().BeTrue();
    }

    #endregion

    #region Setup

    private void SetupRepositories(CompanyUser companyUser, IamUser iamUser)
    {
        var unreadNotificationDetails = new AsyncEnumerableStub<NotificationDetailData>(_unreadNotificationDetails);
        var readNotificationDetails = new AsyncEnumerableStub<NotificationDetailData>(_readNotificationDetails);
        var notificationDetails = new AsyncEnumerableStub<NotificationDetailData>(_notificationDetails);
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheck(iamUser.UserEntityId, companyUser.Id))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool iamUser)>{new (_companyUser.Id, true), new (_companyUser.Id, false)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheck(A<string>.That.Not.Matches(x => x == iamUser.UserEntityId), A<Guid>.That.Not.Matches(x => x == companyUser.Id)))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool iamUser)>().ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyIdForIamUserUntrackedAsync(iamUser.UserEntityId))
            .ReturnsLazily(() => Task.FromResult(companyUser.Id));
        A.CallTo(() =>
                _notificationRepository.GetNotificationByIdAndIamUserIdUntrackedAsync(_notificationDetail.Id, _iamUser.UserEntityId))
            .Returns((true, _notificationDetail));
        A.CallTo(() =>
                _notificationRepository.GetNotificationByIdAndIamUserIdUntrackedAsync(
                    A<Guid>.That.Not.Matches(x => x == _notificationDetail.Id), A<string>._))
            .Returns(((bool, NotificationDetailData)) default);

        A.CallTo(() =>
                _notificationRepository.GetAllNotificationDetailsByIamUserIdUntracked(_iamUser.UserEntityId, false,
                    null))
            .Returns(unreadNotificationDetails.AsQueryable());
        A.CallTo(() =>
                _notificationRepository.GetAllNotificationDetailsByIamUserIdUntracked(_iamUser.UserEntityId, true,
                    null))
            .Returns(readNotificationDetails.AsQueryable());
        A.CallTo(() =>
                _notificationRepository.GetAllNotificationDetailsByIamUserIdUntracked(_iamUser.UserEntityId, null, null))
            .Returns(notificationDetails.AsQueryable());

        A.CallTo(() =>
                _notificationRepository.CheckNotificationExistsByIdAndIamUserIdAsync(_notificationDetail.Id, _iamUser.UserEntityId))
            .ReturnsLazily(() => (true, true));
        A.CallTo(() =>
                _notificationRepository.CheckNotificationExistsByIdAndIamUserIdAsync(
                    A<Guid>.That.Not.Matches(x => x == _notificationDetail.Id), A<string>._))
            .ReturnsLazily(() => (false, false));
        A.CallTo(() =>
                _notificationRepository.CheckNotificationExistsByIdAndIamUserIdAsync(_notificationDetail.Id,
                    A<string>.That.Not.Matches(x => x == _iamUser.UserEntityId)))
            .ReturnsLazily(() => (false, true));
        A.CallTo(() => _notificationRepository.GetNotificationByIdAndIamUserIdUntrackedAsync(_notificationDetail.Id, _iamUser.UserEntityId))
            .ReturnsLazily(() => (true, _unreadNotificationDetails.First()));
        A.CallTo(() => _notificationRepository.GetNotificationByIdAndIamUserIdUntrackedAsync(_notificationDetail.Id, A<string>.That.Not.Matches(x => x == _iamUser.UserEntityId)))
            .ReturnsLazily(() => (false, _unreadNotificationDetails.First()));
        A.CallTo(() => _notificationRepository.GetNotificationByIdAndIamUserIdUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _notificationDetail.Id), _iamUser.UserEntityId))
            .ReturnsLazily(() => default((bool IsUserReceiver, NotificationDetailData NotificationDetailData)));

        A.CallTo(() => _notificationRepository.GetNotificationCountForIamUserAsync(_iamUser.UserEntityId, false))
            .ReturnsLazily(() => (true, 5));
        A.CallTo(() => _notificationRepository.GetNotificationCountForIamUserAsync(A<string>.That.Not.Matches(x => x == _iamUser.UserEntityId), false))
            .ReturnsLazily(() => default((bool IsUserExisting, int Count)));

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
    }

    private (CompanyUser, IamUser) CreateTestUserPair()
    {
        var companyUser = _fixture.Build<CompanyUser>()
            .Without(u => u.IamUser)
            .Create();
        var iamUser = _fixture.Build<IamUser>()
            .With(u => u.CompanyUser, companyUser)
            .Create();
        companyUser.IamUser = iamUser;
        return (companyUser, iamUser);
    }

    #endregion
}
