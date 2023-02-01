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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Microsoft.Extensions.Options;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.Tests.BusinessLogic;

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
        var notifications = new List<Notification>();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._,
                A<Action<Notification?>>._))
            .Invokes((Guid receiverUserId, NotificationTypeId notificationTypeId, bool isRead, Action<Notification>? setOptionalParameters) =>
            {
                var notification = new Notification(Guid.NewGuid(), receiverUserId,
                    DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                notifications.Add(notification);
            });
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));
        const string content = "That's a title";

        // Act
        var result = await sut.CreateNotificationAsync(_iamUser.UserEntityId,
            new NotificationCreationData(
                content,
                NotificationTypeId.INFO,
                false), _companyUser.Id)
            .ConfigureAwait(false);

        // Assert
        result.Should().BeEmpty();
        notifications.Should().HaveCount(1);
        var notification = notifications.Single();
        notification.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateNotification_WithNotExistingCompanyUser_ThrowsArgumentException()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetNotifications_WithStatus_ReturnsList(bool status)
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        var result = await sut.GetNotificationsAsync(0, 15, _iamUser.UserEntityId, status).ConfigureAwait(false);

        // Assert
        var expectedCount = status ? 
            _readNotificationDetails.Count() :
            _unreadNotificationDetails.Count();
        result.Content.Should().HaveCount(expectedCount);
    }

    [Theory]
    [InlineData(NotificationSorting.DateAsc)]
    [InlineData(NotificationSorting.DateDesc)]
    [InlineData(NotificationSorting.ReadStatusAsc)]
    [InlineData(NotificationSorting.ReadStatusDesc)]
    public async Task GetNotifications_WithoutStatus_ReturnsList(NotificationSorting sorting)
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        var result = await sut.GetNotificationsAsync(0, 15, _iamUser.UserEntityId, sorting: sorting).ConfigureAwait(false);

        // Assert
        result.Meta.NumberOfElements.Should().Be(_notificationDetails.Count());
        result.Meta.NumberOfPages.Should().Be(1);
        result.Meta.Page.Should().Be(0);
        result.Meta.PageSize.Should().Be(_notificationDetails.Count());
        result.Content.Should().HaveCount(_notificationDetails.Count());
    }

    [Fact]
    public async Task GetNotifications_SecondPage_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        var results = await sut.GetNotificationsAsync(1, 3, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results.Meta.NumberOfElements.Should().Be(_notificationDetails.Count());
        results.Meta.NumberOfPages.Should().Be(2);
        results.Meta.Page.Should().Be(1);
        results.Meta.PageSize.Should().Be(1);
        results.Content.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetNotifications_ExceedMaxPageSize_Throws()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        var Act = () => sut.GetNotificationsAsync(0, 20, _iamUser.UserEntityId);

        // Act & Assert
        await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
    }

    [Fact]
    public async Task GetNotifications_NegativePage_Throws()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        var Act = () => sut.GetNotificationsAsync(-1, 15, _iamUser.UserEntityId);

        // Act & Assert
        await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
    }

    #endregion

    #region Get Notification Details

    [Fact]
    public async Task GetNotificationDetailDataAsync_WithIdAndUser_ReturnsCorrectResult()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        var result = await sut.GetNotificationDetailDataAsync(_iamUser.UserEntityId, _notificationDetail.Id).ConfigureAwait(false);

        // Assert
        var notificationDetailData = _unreadNotificationDetails.First();
        result.Should().Be(notificationDetailData);
    }

    [Fact]
    public async Task GetNotificationDetailDataAsync_WithNotMatchingUser_ThrowsForbiddenException()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        var iamUserId = Guid.NewGuid().ToString();
        async Task Act() => await sut.GetNotificationDetailDataAsync(iamUserId, _notificationDetail.Id).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"iamUserId {iamUserId} is not the receiver of the notification");
    }

    [Fact]
    public async Task GetNotificationDetailDataAsync_WithNotMatchingNotificationId_ThrowsNotFoundException()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        var notificationId = Guid.NewGuid();
        async Task Act() => await sut.GetNotificationDetailDataAsync(_iamUser.UserEntityId, notificationId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Notification {notificationId} does not exist.");
    }

    #endregion

    #region Get Notification Count

    [Fact]
    public async Task GetNotificationCountAsync_WithIdAndUser_ReturnsCorrectResult()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        var result = await sut.GetNotificationCountAsync(_iamUser.UserEntityId, false).ConfigureAwait(false);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public async Task GetNotificationCountAsync_WithNotMatchingUser_ThrowsForbiddenException()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        var iamUserId = Guid.NewGuid().ToString();
        async Task Act() => await sut.GetNotificationCountAsync(iamUserId, false).ConfigureAwait(false);
       
        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"iamUserId {iamUserId} is not assigned");
    }

    #endregion

    #region Get Notification Count Detail

    [Fact]
    public async Task GetNotificationCountDetailsAsync()
    {
        // Arrange
        var data = new AsyncEnumerableStub<(bool IsRead, NotificationTopicId NotificationTopicId, int Count)>(new List<(bool IsRead, NotificationTopicId NotificationTopicId, int Count)>
        {
            new (true, NotificationTopicId.INFO, 2),
            new (false, NotificationTopicId.INFO, 3),
            new (true, NotificationTopicId.OFFER, 6),
            new (false, NotificationTopicId.OFFER, 4),
            new (true, NotificationTopicId.ACTION, 1),
            new (false, NotificationTopicId.ACTION, 5),
        });
        A.CallTo(() => _notificationRepository.GetCountDetailsForUserAsync(_iamUser.UserEntityId)).Returns(data.AsAsyncEnumerable());
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));
        
        // Act
        var result = await sut.GetNotificationCountDetailsAsync(_iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        result.Read.Should().Be(9);
        result.Unread.Should().Be(12);
        result.InfoUnread.Should().Be(3);
        result.OfferUnread.Should().Be(4);
        result.ActionRequired.Should().Be(5);
    }

    #endregion
    
    #region Set Notification To Read

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SetNotificationStatus_WithMatchingId_ReturnsDetailData(bool isRead)
    {
        // Arrange
        var notification = new Notification(_notificationDetail.Id, Guid.NewGuid(), DateTimeOffset.Now, NotificationTypeId.INFO, !isRead);
        A.CallTo(() => _notificationRepository.AttachAndModifyNotification(_notificationDetail.Id, A<Action<Notification>>._))
            .Invokes((Guid _, Action<Notification> setOptionalParameters) =>
            {
                setOptionalParameters.Invoke(notification);
            });
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        await sut.SetNotificationStatusAsync(_iamUser.UserEntityId, _notificationDetail.Id, isRead).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        notification.IsRead.Should().Be(isRead);
    }

    [Fact]
    public async Task SetNotificationToRead_WithNotMatchingNotification_NotFoundException()
    {
        // Arrange
        var randomNotificationId = Guid.NewGuid();
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));
        var notExistingUserId = Guid.NewGuid().ToString();

        // Act
        async Task Act() => await sut.SetNotificationStatusAsync(notExistingUserId, randomNotificationId, true).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Notification {randomNotificationId} does not exist.");
    }

    [Fact]
    public async Task SetNotificationToRead_WithNotExistingCompanyUser_ThrowsForbiddenException()
    {
        // Arrange
        var iamUserId = Guid.NewGuid().ToString();
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        async Task Act() => await sut.SetNotificationStatusAsync(iamUserId, _notificationDetail.Id, true).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"iamUserId {iamUserId} is not the receiver of the notification");
    }

    #endregion

    #region Delete Notifications

    [Fact]
    public async Task DeleteNotification_WithValidData_ExecutesSuccessfully()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        await sut.DeleteNotificationAsync(_iamUser.UserEntityId, _notificationDetail.Id).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteNotification_WithNotExistingCompanyUser_ThrowsForbiddenException()
    {
        // Arrange
        var iamUserId = Guid.NewGuid().ToString();
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        async Task Act() => await sut.DeleteNotificationAsync(iamUserId, _notificationDetail.Id).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"iamUserId {iamUserId} is not the receiver of the notification");
    }

    [Fact]
    public async Task DeleteNotification_WithNotExistingNotification_ThrowsNotFoundException()
    {
        // Arrange
        var randomNotificationId = Guid.NewGuid();
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        async Task Act() => await sut.DeleteNotificationAsync(_iamUser.UserEntityId, randomNotificationId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Notification {randomNotificationId} does not exist.");
    }

    #endregion

    #region Setup

    private void SetupRepositories(CompanyUser companyUser, IamUser iamUser)
    {
        SetupNotifications();

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

    private void SetupNotifications()
    {
        var unreadPaging = (int skip, int take) => Task.FromResult(new Pagination.Source<NotificationDetailData>(_unreadNotificationDetails.Count(), _unreadNotificationDetails.Skip(skip).Take(take)));
        var readPaging = (int skip, int take) => Task.FromResult(new Pagination.Source<NotificationDetailData>(_readNotificationDetails.Count(), _readNotificationDetails.Skip(skip).Take(take)));
        var notificationsPaging = (int skip, int take) => Task.FromResult(new Pagination.Source<NotificationDetailData>(_notificationDetails.Count(), _notificationDetails.Skip(skip).Take(take)));
        
        A.CallTo(() => _notificationRepository.GetAllNotificationDetailsByIamUserIdUntracked(_iamUser.UserEntityId, false, null, null, A<NotificationSorting>._))
            .Returns(unreadPaging);
        A.CallTo(() => _notificationRepository.GetAllNotificationDetailsByIamUserIdUntracked(_iamUser.UserEntityId, true, null, null, A<NotificationSorting>._))
            .Returns(readPaging);
        A.CallTo(() => _notificationRepository.GetAllNotificationDetailsByIamUserIdUntracked(_iamUser.UserEntityId, null, null, null, A<NotificationSorting>._))
            .Returns(notificationsPaging);
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
