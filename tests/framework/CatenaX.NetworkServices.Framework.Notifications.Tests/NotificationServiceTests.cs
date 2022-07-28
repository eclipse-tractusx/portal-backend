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
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace CatenaX.NetworkServices.Framework.Notifications.Tests;

public class NotificationServiceTests
{
    private static readonly Guid NoExistingAdminCompanyId = Guid.NewGuid();
    private static readonly Guid CatenaXAdminId = Guid.NewGuid();
    private readonly CompanyUser _companyUser;
    private readonly IFixture _fixture;
    private readonly IamUser _iamUser;
    private readonly NotificationDetailData _notificationDetail;
    private readonly IAsyncEnumerable<NotificationDetailData> _notificationDetails;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IAsyncEnumerable<NotificationDetailData> _readNotificationDetails;
    private readonly IAsyncEnumerable<NotificationDetailData> _unreadNotificationDetails;
    private readonly IUserRepository _userRepository;

    public NotificationServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var (companyUser, iamUser) = CreateTestUserPair();
        _companyUser = companyUser;
        _iamUser = iamUser;
        _notificationDetail = new NotificationDetailData(Guid.NewGuid(),  DateTime.UtcNow, NotificationTypeId.INFO, false,"Test Message", null);

        _portalRepositories = A.Fake<IPortalRepositories>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _userRepository = A.Fake<IUserRepository>();

        _readNotificationDetails = _fixture.Build<NotificationDetailData>()
            .CreateMany(1)
            .ToAsyncEnumerable();
        _unreadNotificationDetails = _fixture.Build<NotificationDetailData>()
            .CreateMany(3)
            .ToAsyncEnumerable();
        _notificationDetails = _readNotificationDetails.Concat(_unreadNotificationDetails).AsAsyncEnumerable();
        SetupRepositories(companyUser, iamUser);
    }

    #region Create Welcome Notification

    [Fact]
    public async Task CreateWelcomeNotifications_WithValidData_ReturnsCorrectDetails()
    {
        // Arrange
        var notifications = new List<Notification>();
        A.CallTo(() =>
                _notificationRepository.Create(A<Guid>._, A<NotificationTypeId>._, A<bool>._,
                    A<Action<Notification>>._))
            .Invokes(x =>
                notifications.Add(new Notification(Guid.NewGuid(), x.Arguments.Get<Guid>("receiverUserId"), DateTimeOffset.UtcNow, x.Arguments.Get<NotificationTypeId>("notificationTypeId"), x.Arguments.Get<bool>("isRead"))));
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationService>();

        // Act
        await sut.CreateWelcomeNotificationsForCompanyAsync(_companyUser.CompanyId);

        // Assert
        notifications.Should().HaveCount(5);
        notifications.Should().AllSatisfy(x => x.ReceiverUserId.Should().Be(_companyUser.Id));
        notifications.Where(x => x.NotificationTypeId == NotificationTypeId.WELCOME).Should().HaveCount(1);
        notifications.Where(x => x.NotificationTypeId == NotificationTypeId.WELCOME_USE_CASES).Should().HaveCount(1);
        notifications.Where(x => x.NotificationTypeId == NotificationTypeId.WELCOME_SERVICE_PROVIDER).Should().HaveCount(1);
        notifications.Where(x => x.NotificationTypeId == NotificationTypeId.WELCOME_CONNECTOR_REGISTRATION).Should().HaveCount(1);
        notifications.Where(x => x.NotificationTypeId == NotificationTypeId.WELCOME_APP_MARKETPLACE).Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateWelcomeNotification_WithoutCatenaXAdmin_NotificationsDontGetCreated()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationService>();

        // Act
        await sut.CreateWelcomeNotificationsForCompanyAsync(Guid.NewGuid());

        // Must not reach that code because of the exception
        A.CallTo(() => _notificationRepository.Create(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>?>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateWelcomeNotification_WithNoCompanyAdmin_NotificationsDontGetCreated()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationService>();

        // Act
        await sut.CreateWelcomeNotificationsForCompanyAsync(NoExistingAdminCompanyId);

        A.CallTo(() => _notificationRepository.Create(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>?>._)).MustNotHaveHappened();
    }

    #endregion

    #region Create Notification

    [Fact]
    public async Task CreateNotification_WithValidData_ReturnsCorrectDetails()
    {
        // Arrange
        var notifications = new List<Notification>();
        A.CallTo(() => _notificationRepository.Create(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._))
            .Invokes(_ =>
                notifications.Add(A.Fake<Notification>()));
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationService>();
        const string content = "That's a title";

        // Act
        var result = await sut.CreateNotificationAsync(_iamUser.UserEntityId,
            new NotificationCreationData(content, NotificationTypeId.INFO,
                false), _companyUser.Id);

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
        var sut = _fixture.Create<NotificationService>();

        // Act
        try
        {
            await sut.CreateNotificationAsync(_iamUser.UserEntityId,
                new NotificationCreationData("That's a title",
                    NotificationTypeId.INFO, false), Guid.NewGuid());
        }
        catch (ArgumentException e)
        {
            // Assert
            e.ParamName.Should().Be("receiverId");
            return;
        }

        // Must not reach that code because of the exception
        false.Should().BeTrue();
    }

    #endregion

    #region Setup

    private void SetupRepositories(CompanyUser companyUser, IamUser iamUser)
    {
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheck(iamUser.UserEntityId, companyUser.Id))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool iamUser)>{new (_companyUser.Id, true), new (_companyUser.Id, false)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheck(A<string>.That.Not.Matches(x => x == iamUser.UserEntityId), A<Guid>.That.Not.Matches(x => x == companyUser.Id)))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool iamUser)>().ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyIdForIamUserUntrackedAsync(iamUser.UserEntityId))
            .ReturnsLazily(() => Task.FromResult(companyUser.Id));
        A.CallTo(() => _userRepository.GetCatenaAndCompanyAdminIdAsync(NoExistingAdminCompanyId))
            .Returns(new List<(Guid CompanyUserId, bool IsCatenaXAdmin, bool IsCompanyAdmin)> { new (CatenaXAdminId, true, false) }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCatenaAndCompanyAdminIdAsync(companyUser.CompanyId))
            .Returns(new List<(Guid CompanyUserId, bool IsCatenaXAdmin, bool IsCompanyAdmin)> { new (CatenaXAdminId, true, false), new (companyUser.Id, false, true) }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCatenaAndCompanyAdminIdAsync(A<Guid>.That.Not.Matches(x => x == NoExistingAdminCompanyId || x == companyUser.CompanyId)))
            .Returns(new List<(Guid CompanyUserId, bool IsCatenaXAdmin, bool IsCompanyAdmin)>().ToAsyncEnumerable());
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
            .Returns(_unreadNotificationDetails);
        A.CallTo(() =>
                _notificationRepository.GetAllNotificationDetailsByIamUserIdUntracked(_iamUser.UserEntityId, true,
                    null))
            .Returns(_readNotificationDetails);
        A.CallTo(() =>
                _notificationRepository.GetAllNotificationDetailsByIamUserIdUntracked(_iamUser.UserEntityId, null, null))
            .Returns(_notificationDetails);

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
