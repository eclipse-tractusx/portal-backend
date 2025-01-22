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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.ErrorHandling;
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
    private readonly Guid _identityId = Guid.NewGuid();
    private readonly IIdentityData _identity;

    private readonly IFixture _fixture;
    private readonly NotificationDetailData _notificationDetail;
    private readonly IEnumerable<NotificationDetailData> _notificationDetails;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IEnumerable<NotificationDetailData> _readNotificationDetails;
    private readonly IEnumerable<NotificationDetailData> _unreadNotificationDetails;
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;

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

        _identity = A.Fake<IIdentityData>();
        _identityService = A.Fake<IIdentityService>();
        A.CallTo(() => _identity.IdentityId).Returns(_identityId);
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

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
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        var result = await sut.GetNotificationsAsync(0, 15, new NotificationFilters(status, null, null, false, null, null, Enumerable.Empty<NotificationTypeId>(), null), SearchSemanticTypeId.AND);

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
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        var result = await sut.GetNotificationsAsync(0, 15, new NotificationFilters(null, null, null, false, sorting, null, Enumerable.Empty<NotificationTypeId>(), null), SearchSemanticTypeId.AND);

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
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        var results = await sut.GetNotificationsAsync(1, 3, new NotificationFilters(null, null, null, false, null, null, Enumerable.Empty<NotificationTypeId>(), null), SearchSemanticTypeId.AND);

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
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        var Act = () => sut.GetNotificationsAsync(0, 20, new NotificationFilters(null, null, null, false, null, null, Enumerable.Empty<NotificationTypeId>(), null), SearchSemanticTypeId.AND);

        // Act & Assert
        await Assert.ThrowsAsync<ControllerArgumentException>(Act);
    }

    [Fact]
    public async Task GetNotifications_NegativePage_Throws()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        var Act = () => sut.GetNotificationsAsync(-1, 15, new NotificationFilters(null, null, null, false, null, null, Enumerable.Empty<NotificationTypeId>(), null), SearchSemanticTypeId.AND);

        // Act & Assert
        await Assert.ThrowsAsync<ControllerArgumentException>(Act);
    }

    [Fact]
    public async Task GetNotifications_WithFilters_CallsExpected()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 20
        }));

        var userId = Guid.NewGuid();
        A.CallTo(() => _identity.IdentityId).Returns(userId);
        var filter = _fixture.Create<NotificationFilters>();

        A.CallTo(() => _notificationRepository.GetAllNotificationDetailsByReceiver(A<Guid>._, A<SearchSemanticTypeId>._, A<bool>._, A<NotificationTypeId>._, A<NotificationTopicId>._, A<bool>._, A<NotificationSorting>._, A<bool>._, A<IEnumerable<NotificationTypeId>>._, A<string?>._))
            .Returns((int skip, int take) => Task.FromResult<Pagination.Source<NotificationDetailData>?>(new(100, _fixture.CreateMany<NotificationDetailData>(take))));

        // Act
        var result = await sut.GetNotificationsAsync(2, 10, filter, SearchSemanticTypeId.AND);

        // Assert
        A.CallTo(() => _notificationRepository.GetAllNotificationDetailsByReceiver(userId, SearchSemanticTypeId.AND, filter.IsRead, filter.TypeId, filter.TopicId, filter.OnlyDueDate, filter.Sorting, filter.DoneState, A<IEnumerable<NotificationTypeId>>.That.Matches(x => x.Count() == filter.SearchTypeIds.Count()), filter.SearchQuery))
            .MustHaveHappenedOnceExactly();
        result.Should().NotBeNull()
            .And.Match<Pagination.Response<NotificationDetailData>>(x =>
                x.Meta.NumberOfElements == 100 &&
                x.Meta.NumberOfPages == 10 &&
                x.Meta.Page == 2 &&
                x.Meta.PageSize == 10 &&
                x.Content.Count() == 10);
    }

    #endregion

    #region Get Notification Details

    [Fact]
    public async Task GetNotificationDetailDataAsync_WithIdAndUser_ReturnsCorrectResult()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        var result = await sut.GetNotificationDetailDataAsync(_notificationDetail.Id);

        // Assert
        var notificationDetailData = _unreadNotificationDetails.First();
        result.Should().Be(notificationDetailData);
    }

    [Fact]
    public async Task GetNotificationDetailDataAsync_WithNotMatchingUser_ThrowsForbiddenException()
    {
        // Arrange
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());

        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        async Task Act() => await sut.GetNotificationDetailDataAsync(_notificationDetail.Id);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(NotificationErrors.USER_NOT_RECEIVER.ToString());
    }

    [Fact]
    public async Task GetNotificationDetailDataAsync_WithNotMatchingNotificationId_ThrowsNotFoundException()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        var notificationId = Guid.NewGuid();
        async Task Act() => await sut.GetNotificationDetailDataAsync(notificationId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(NotificationErrors.NOTIFICATION_NOT_FOUND.ToString());
    }

    #endregion

    #region Get Notification Count

    [Fact]
    public async Task GetNotificationCountAsync_WithIdAndUser_ReturnsCorrectResult()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        var result = await sut.GetNotificationCountAsync(false);

        // Assert
        result.Should().Be(5);
    }

    #endregion

    #region Get Notification Count Detail

    [Fact]
    public async Task GetNotificationCountDetailsAsync()
    {
        // Arrange
        var data = new AsyncEnumerableStub<(bool IsRead, bool? Done, NotificationTopicId? NotificationTopicId, int Count)>(new List<(bool IsRead, bool? Done, NotificationTopicId? NotificationTopicId, int Count)>
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
        A.CallTo(() => _notificationRepository.GetCountDetailsForUserAsync(_identityId)).Returns(data.AsAsyncEnumerable());
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        var result = await sut.GetNotificationCountDetailsAsync();

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
        A.CallTo(() => _notificationRepository.AttachAndModifyNotification(_notificationDetail.Id, A<Action<Notification>>._, A<Action<Notification>>._))
            .Invokes((Guid _, Action<Notification> initializers, Action<Notification> dbupdatedfield) =>
            {
                initializers.Invoke(notification);
                dbupdatedfield.Invoke(notification);
            });
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        await sut.SetNotificationStatusAsync(_notificationDetail.Id, isRead);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        notification.IsRead.Should().Be(isRead);
    }

    [Fact]
    public async Task SetNotificationToRead_WithNotMatchingNotification_NotFoundException()
    {
        // Arrange
        var randomNotificationId = Guid.NewGuid();
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());

        // Act
        async Task Act() => await sut.SetNotificationStatusAsync(randomNotificationId, true);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(NotificationErrors.NOTIFICATION_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task SetNotificationToRead_WithNotExistingCompanyUser_ThrowsForbiddenException()
    {
        // Arrange
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());

        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        async Task Act() => await sut.SetNotificationStatusAsync(_notificationDetail.Id, true);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(NotificationErrors.USER_NOT_RECEIVER.ToString());
    }

    #endregion

    #region Delete Notifications

    [Fact]
    public async Task DeleteNotification_WithValidData_ExecutesSuccessfully()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        await sut.DeleteNotificationAsync(_notificationDetail.Id);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteNotification_WithNotExistingCompanyUser_ThrowsForbiddenException()
    {
        // Arrange
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());

        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        async Task Act() => await sut.DeleteNotificationAsync(_notificationDetail.Id);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(NotificationErrors.USER_NOT_RECEIVER.ToString());
    }

    [Fact]
    public async Task DeleteNotification_WithNotExistingNotification_ThrowsNotFoundException()
    {
        // Arrange
        var randomNotificationId = Guid.NewGuid();
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        async Task Act() => await sut.DeleteNotificationAsync(randomNotificationId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(NotificationErrors.NOTIFICATION_NOT_FOUND.ToString());
    }

    #endregion

    #region Create Notifications

    [Fact]
    public async Task CreateNotification_WithNotExistingCompanyUser_ThrowsForbiddenException()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));
        var userId = Guid.NewGuid();
        var data = _fixture.Build<NotificationRequest>()
            .With(x => x.Receiver, userId)
            .With(x => x.NotificationTypeId, NotificationTypeId.CREDENTIAL_APPROVAL)
            .Create();
        A.CallTo(() => _userRepository.CheckUserExists(userId)).Returns(false);
        async Task Act() => await sut.CreateNotification(data).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be(NotificationErrors.USER_NOT_FOUND.ToString());
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateNotification_WithInvalidNotificationType_ThrowsForbiddenException()
    {
        // Arrange
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));
        var userId = Guid.NewGuid();
        var data = _fixture.Build<NotificationRequest>().With(x => x.Receiver, userId).With(x => x.NotificationTypeId, NotificationTypeId.INFO).Create();
        async Task Act() => await sut.CreateNotification(data).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be(NotificationErrors.INVALID_NOTIFICATION_TYPE.ToString());
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(NotificationTypeId.CREDENTIAL_APPROVAL)]
    [InlineData(NotificationTypeId.CREDENTIAL_REJECTED)]
    [InlineData(NotificationTypeId.CREDENTIAL_EXPIRY)]
    public async Task CreateNotification_WithValidData_ExecutesSuccessfully(NotificationTypeId notificationTypeId)
    {
        // Arrange
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        var userId = Guid.NewGuid();
        var notifications = new List<Notification>();
        var data = _fixture.Build<NotificationRequest>()
            .With(x => x.Receiver, userId)
            .With(x => x.Content, "test")
            .With(x => x.NotificationTypeId, notificationTypeId)
            .Create();
        A.CallTo(() => _userRepository.CheckUserExists(userId)).Returns(true);
        A.CallTo(() => _notificationRepository.CreateNotification(userId, A<NotificationTypeId>._, false, A<Action<Notification>>._))
            .Invokes((Guid receiverUserId, NotificationTypeId notificationTypeId, bool isRead, Action<Notification>? setOptionalParameters) =>
            {
                var notification = new Notification(Guid.NewGuid(), receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                notifications.Add(notification);
            });
        var sut = new NotificationBusinessLogic(_portalRepositories, _identityService, Options.Create(new NotificationSettings
        {
            MaxPageSize = 15
        }));

        // Act
        await sut.CreateNotification(data);

        // Assert
        notifications.Should().ContainSingle();
        var notification = notifications.Single();
        notification.Content.Should().Be("test");
    }

    #endregion

    #region Setup

    private void SetupRepositories()
    {
        SetupNotifications();

        A.CallTo(() => _notificationRepository.GetNotificationByIdAndValidateReceiverAsync(_notificationDetail.Id, _identityId))
            .Returns((true, _notificationDetail));
        A.CallTo(() =>
                _notificationRepository.GetNotificationByIdAndValidateReceiverAsync(
                    A<Guid>.That.Not.Matches(x => x == _notificationDetail.Id), A<Guid>._))
            .Returns<(bool, NotificationDetailData)>(default);

        A.CallTo(() =>
                _notificationRepository.CheckNotificationExistsByIdAndValidateReceiverAsync(_notificationDetail.Id, _identityId))
            .Returns((true, true, true));
        A.CallTo(() =>
                _notificationRepository.CheckNotificationExistsByIdAndValidateReceiverAsync(
                    A<Guid>.That.Not.Matches(x => x == _notificationDetail.Id), A<Guid>._))
            .Returns((false, false, true));
        A.CallTo(() =>
                _notificationRepository.CheckNotificationExistsByIdAndValidateReceiverAsync(_notificationDetail.Id,
                    A<Guid>.That.Not.Matches(x => x == _identityId)))
            .Returns((false, true, true));
        A.CallTo(() => _notificationRepository.GetNotificationByIdAndValidateReceiverAsync(_notificationDetail.Id, _identityId))
            .Returns((true, _unreadNotificationDetails.First()));
        A.CallTo(() => _notificationRepository.GetNotificationByIdAndValidateReceiverAsync(_notificationDetail.Id, A<Guid>.That.Not.Matches(x => x == _identityId)))
            .Returns((false, _unreadNotificationDetails.First()));
        A.CallTo(() => _notificationRepository.GetNotificationByIdAndValidateReceiverAsync(A<Guid>.That.Not.Matches(x => x == _notificationDetail.Id), _identityId))
            .Returns<(bool IsUserReceiver, NotificationDetailData NotificationDetailData)>(default);

        A.CallTo(() => _notificationRepository.GetNotificationCountForUserAsync(_identityId, false))
            .Returns(5);

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
    }

    private void SetupNotifications()
    {
        var unreadPaging = (int skip, int take) => Task.FromResult(new Pagination.Source<NotificationDetailData>(_unreadNotificationDetails.Count(), _unreadNotificationDetails.Skip(skip).Take(take)));
        var readPaging = (int skip, int take) => Task.FromResult(new Pagination.Source<NotificationDetailData>(_readNotificationDetails.Count(), _readNotificationDetails.Skip(skip).Take(take)));
        var notificationsPaging = (int skip, int take) => Task.FromResult(new Pagination.Source<NotificationDetailData>(_notificationDetails.Count(), _notificationDetails.Skip(skip).Take(take)));

        A.CallTo(() => _notificationRepository.GetAllNotificationDetailsByReceiver(_identityId, SearchSemanticTypeId.AND, false, null, null, false, A<NotificationSorting>._, null, A<IEnumerable<NotificationTypeId>>._, null))
            .Returns(unreadPaging);
        A.CallTo(() => _notificationRepository.GetAllNotificationDetailsByReceiver(_identityId, SearchSemanticTypeId.AND, true, null, null, false, A<NotificationSorting>._, null, A<IEnumerable<NotificationTypeId>>._, null))
            .Returns(readPaging);
        A.CallTo(() => _notificationRepository.GetAllNotificationDetailsByReceiver(_identityId, SearchSemanticTypeId.AND, null, null, null, false, A<NotificationSorting>._, null, A<IEnumerable<NotificationTypeId>>._, null))
            .Returns(notificationsPaging);
    }

    #endregion
}
