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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.Tests.BusinessLogic;

public class NotificationBusinessLogicTests
{
    private const string IamUserId = "3e8343f7-4fe5-4296-8312-f33aa6dbde5d";
    private readonly IdentityData _identity = new(IamUserId, Guid.NewGuid(), IdentityTypeId.COMPANY_USER, Guid.NewGuid());

    private readonly IFixture _fixture;
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

        _notificationDetail = new NotificationDetailData(Guid.NewGuid(), DateTime.UtcNow, NotificationTypeId.INFO, NotificationTopicId.INFO, false, "Test Message", null, false);

        _portalRepositories = A.Fake<IPortalRepositories>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _userRepository = A.Fake<IUserRepository>();

        _readNotificationDetails = _fixture.Build<NotificationDetailData>()
            .CreateMany(1);
        _unreadNotificationDetails = _fixture.Build<NotificationDetailData>()
            .CreateMany(3);
        _notificationDetails = _readNotificationDetails.Concat(_unreadNotificationDetails);
        SetupRepositories();
    }

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
        var result = await sut.GetNotificationsAsync(0, 15, _identity.UserId, new NotificationFilters(status, null, null, false, null, null)).ConfigureAwait(false);

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
        var result = await sut.GetNotificationsAsync(0, 15, _identity.UserId, new NotificationFilters(null, null, null, false, sorting, null)).ConfigureAwait(false);

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
        var results = await sut.GetNotificationsAsync(1, 3, _identity.UserId, new NotificationFilters(null, null, null, false, null, null)).ConfigureAwait(false);

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

        var Act = () => sut.GetNotificationsAsync(0, 20, _identity.UserId, new NotificationFilters(null, null, null, false, null, null));

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

        var Act = () => sut.GetNotificationsAsync(-1, 15, _identity.UserId, new NotificationFilters(null, null, null, false, null, null));

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
        var result = await sut.GetNotificationDetailDataAsync(_identity.UserId, _notificationDetail.Id).ConfigureAwait(false);

        // Assert
        var notificationDetailData = _unreadNotificationDetails.First();
        result.Should().Be(notificationDetailData);
    }

    [Fact]
    public async Task GetNotificationDetailDataAsync_WithNotMatchingUser_ThrowsForbiddenException()
    {
        // Arrange
        var identity = _fixture.Create<IdentityData>();
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        async Task Act() => await sut.GetNotificationDetailDataAsync(identity.UserId, _notificationDetail.Id).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be("The user is not the receiver of the notification");
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
        async Task Act() => await sut.GetNotificationDetailDataAsync(_identity.UserId, notificationId).ConfigureAwait(false);

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
        var result = await sut.GetNotificationCountAsync(_identity.UserId, false).ConfigureAwait(false);

        // Assert
        result.Should().Be(5);
    }

    #endregion

    #region Get Notification Count Detail

    [Fact]
    public async Task GetNotificationCountDetailsAsync()
    {
        // Arrange
        var data = new AsyncEnumerableStub<(bool IsRead, bool? Done, NotificationTopicId NotificationTopicId, int Count)>(new List<(bool IsRead, bool? Done, NotificationTopicId NotificationTopicId, int Count)>
        {
            new (true, null, NotificationTopicId.INFO, 2),
            new (false, null, NotificationTopicId.INFO, 3),
            new (true, null, NotificationTopicId.OFFER, 6),
            new (false, null, NotificationTopicId.OFFER, 4),
            new (true, null, NotificationTopicId.ACTION, 1),
            new (false, null, NotificationTopicId.ACTION, 5),
            new (false, true, NotificationTopicId.ACTION, 3),
            new (false, false, NotificationTopicId.ACTION, 2),
        });
        A.CallTo(() => _notificationRepository.GetCountDetailsForUserAsync(_identity.UserId)).Returns(data.AsAsyncEnumerable());
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        var result = await sut.GetNotificationCountDetailsAsync(_identity.UserId).ConfigureAwait(false);

        // Assert
        result.Read.Should().Be(9);
        result.Unread.Should().Be(17);
        result.InfoUnread.Should().Be(3);
        result.OfferUnread.Should().Be(4);
        result.ActionRequired.Should().Be(8);
        result.UnreadActionRequired.Should().Be(10);
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
        await sut.SetNotificationStatusAsync(_identity.UserId, _notificationDetail.Id, isRead).ConfigureAwait(false);

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
        var identity = _fixture.Create<IdentityData>();

        // Act
        async Task Act() => await sut.SetNotificationStatusAsync(identity.UserId, randomNotificationId, true).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Notification {randomNotificationId} does not exist.");
    }

    [Fact]
    public async Task SetNotificationToRead_WithNotExistingCompanyUser_ThrowsForbiddenException()
    {
        // Arrange
        var identity = _fixture.Create<IdentityData>();
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        async Task Act() => await sut.SetNotificationStatusAsync(identity.UserId, _notificationDetail.Id, true).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be("The user is not the receiver of the notification");
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
        await sut.DeleteNotificationAsync(_identity.UserId, _notificationDetail.Id).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteNotification_WithNotExistingCompanyUser_ThrowsForbiddenException()
    {
        // Arrange
        var identity = _fixture.Create<IdentityData>();
        var sut = new NotificationBusinessLogic(_portalRepositories, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        async Task Act() => await sut.DeleteNotificationAsync(identity.UserId, _notificationDetail.Id).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be("The user is not the receiver of the notification");
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
        async Task Act() => await sut.DeleteNotificationAsync(_identity.UserId, randomNotificationId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Notification {randomNotificationId} does not exist.");
    }

    #endregion

    #region Setup

    private void SetupRepositories()
    {
        SetupNotifications();

        A.CallTo(() => _notificationRepository.GetNotificationByIdAndValidateReceiverAsync(_notificationDetail.Id, _identity.UserId))
            .Returns((true, _notificationDetail));
        A.CallTo(() =>
                _notificationRepository.GetNotificationByIdAndValidateReceiverAsync(
                    A<Guid>.That.Not.Matches(x => x == _notificationDetail.Id), A<Guid>._))
            .Returns(((bool, NotificationDetailData))default);

        A.CallTo(() =>
                _notificationRepository.CheckNotificationExistsByIdAndValidateReceiverAsync(_notificationDetail.Id, _identity.UserId))
            .ReturnsLazily(() => (true, true));
        A.CallTo(() =>
                _notificationRepository.CheckNotificationExistsByIdAndValidateReceiverAsync(
                    A<Guid>.That.Not.Matches(x => x == _notificationDetail.Id), A<Guid>._))
            .Returns((false, false));
        A.CallTo(() =>
                _notificationRepository.CheckNotificationExistsByIdAndValidateReceiverAsync(_notificationDetail.Id,
                    A<Guid>.That.Not.Matches(x => x == _identity.UserId)))
            .Returns((false, true));
        A.CallTo(() => _notificationRepository.GetNotificationByIdAndValidateReceiverAsync(_notificationDetail.Id, _identity.UserId))
            .Returns((true, _unreadNotificationDetails.First()));
        A.CallTo(() => _notificationRepository.GetNotificationByIdAndValidateReceiverAsync(_notificationDetail.Id, A<Guid>.That.Not.Matches(x => x == _identity.UserId)))
            .Returns((false, _unreadNotificationDetails.First()));
        A.CallTo(() => _notificationRepository.GetNotificationByIdAndValidateReceiverAsync(A<Guid>.That.Not.Matches(x => x == _notificationDetail.Id), _identity.UserId))
            .Returns(default((bool IsUserReceiver, NotificationDetailData NotificationDetailData)));

        A.CallTo(() => _notificationRepository.GetNotificationCountForUserAsync(_identity.UserId, false))
            .Returns(5);

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
    }

    private void SetupNotifications()
    {
        var unreadPaging = (int skip, int take) => Task.FromResult(new Pagination.Source<NotificationDetailData>(_unreadNotificationDetails.Count(), _unreadNotificationDetails.Skip(skip).Take(take)));
        var readPaging = (int skip, int take) => Task.FromResult(new Pagination.Source<NotificationDetailData>(_readNotificationDetails.Count(), _readNotificationDetails.Skip(skip).Take(take)));
        var notificationsPaging = (int skip, int take) => Task.FromResult(new Pagination.Source<NotificationDetailData>(_notificationDetails.Count(), _notificationDetails.Skip(skip).Take(take)));

        A.CallTo(() => _notificationRepository.GetAllNotificationDetailsByReceiver(_identity.UserId, false, null, null, false, A<NotificationSorting>._, null))
            .Returns(unreadPaging);
        A.CallTo(() => _notificationRepository.GetAllNotificationDetailsByReceiver(_identity.UserId, true, null, null, false, A<NotificationSorting>._, null))
            .Returns(readPaging);
        A.CallTo(() => _notificationRepository.GetAllNotificationDetailsByReceiver(_identity.UserId, null, null, null, false, A<NotificationSorting>._, null))
            .Returns(notificationsPaging);
    }

    #endregion
}
