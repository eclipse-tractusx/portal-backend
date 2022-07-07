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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Notification.Service.BusinessLogic;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace CatenaX.NetworkServices.Notification.Service.Tests;

public class NotificationBusinessLogicTests
{
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

    public NotificationBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        var (companyUser, iamuser) = CreateTestUserPair();
        _companyUser = companyUser;
        _iamUser = iamuser;
        _notificationDetail = new NotificationDetailData(Guid.NewGuid(), "Test Title", "Test Message");

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
        A.CallTo(() => _userRepository.IsUserWithIdExisting(companyUser.Id))
            .ReturnsLazily(() => Task.FromResult(true));
        A.CallTo(() => _userRepository.IsUserWithIdExisting(A<Guid>.That.Not.Matches(x => x == companyUser.Id)))
            .ReturnsLazily(() => Task.FromResult(false));
        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserIdUntrackedAsync(iamuser.UserEntityId))
            .ReturnsLazily(() => Task.FromResult(companyUser.Id));
        A.CallTo(() =>
                _notificationRepository.GetByIdAndUserIdUntrackedAsync(_notificationDetail.Id, _companyUser.Id))
            .Returns(_notificationDetail);
        A.CallTo(() =>
                _notificationRepository.GetByIdAndUserIdUntrackedAsync(
                    A<Guid>.That.Not.Matches(x => x == _notificationDetail.Id), A<Guid>._))
            .Returns((NotificationDetailData?) null);

        A.CallTo(() =>
                _notificationRepository.GetAllAsDetailsByUserIdUntracked(_companyUser.Id, NotificationStatusId.UNREAD,
                    null))
            .Returns(_unreadNotificationDetails);
        A.CallTo(() =>
                _notificationRepository.GetAllAsDetailsByUserIdUntracked(_companyUser.Id, NotificationStatusId.READ,
                    null))
            .Returns(_readNotificationDetails);
        A.CallTo(() =>
                _notificationRepository.GetAllAsDetailsByUserIdUntracked(_companyUser.Id, null, null))
            .Returns(_notificationDetails);
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

    #region Create Notification

    [Fact]
    public async Task CreateNotification_WithValidData_ReturnsCorrectDetails()
    {
        // Arrange
        var notifications = new List<PortalBackend.PortalEntities.Entities.Notification>();
        A.CallTo(() => _notificationRepository.Add(A<PortalBackend.PortalEntities.Entities.Notification>._))
            .Invokes(action =>
                notifications.Add(action.GetArgument<PortalBackend.PortalEntities.Entities.Notification>("notification")!));
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();
        const string title = "That's a title";
        const string message = "Here is a message for the reader";

        // Act
        var result = await sut.CreateNotification(
            new NotificationCreationData(DateTimeOffset.Now, title, message, NotificationTypeId.INFO,
                NotificationStatusId.UNREAD), _companyUser.Id);

        // Assert
        result.Should().NotBeNull();
        notifications.Should().HaveCount(1);
        var notification = notifications.Single();
        notification.Should().NotBeNull();
        notification.Title.Should().Be(title);
        notification.Message.Should().Be(message);
    }

    [Fact]
    public async Task CreateNotification_WithInvalidNotificationType_ThrowsArgumentException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        try
        {
            await sut.CreateNotification(
                new NotificationCreationData(DateTimeOffset.Now, "That's a title", "Here is a message for the reader",
                    (NotificationTypeId) 666, NotificationStatusId.UNREAD), _companyUser.Id);
        }
        catch (ArgumentException e)
        {
            // Assert
            e.ParamName.Should().Be("notificationTypeId");
            return;
        }

        // Must not reach that code because of the exception
        false.Should().BeTrue();
    }

    [Fact]
    public async Task CreateNotification_WithInvalidNotificationStatus_ThrowsArgumentException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        try
        {
            await sut.CreateNotification(
                new NotificationCreationData(DateTimeOffset.Now, "That's a title", "Here is a message for the reader",
                    NotificationTypeId.INFO, (NotificationStatusId) 666), _companyUser.Id);
        }
        catch (ArgumentException e)
        {
            // Assert
            e.ParamName.Should().Be("notificationStatusId");
            return;
        }

        // Must not reach that code because of the exception
        false.Should().BeTrue();
    }

    [Fact]
    public async Task CreateNotification_WithNotExistingCompanyUser_ThrowsArgumentException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        try
        {
            await sut.CreateNotification(
                new NotificationCreationData(DateTimeOffset.Now, "That's a title", "Here is a message for the reader",
                    NotificationTypeId.INFO, NotificationStatusId.UNREAD), Guid.NewGuid());
        }
        catch (ArgumentException e)
        {
            // Assert
            e.ParamName.Should().Be("companyUserId");
            return;
        }

        // Must not reach that code because of the exception
        false.Should().BeTrue();
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
        var result = await sut.GetNotifications(_iamUser.UserEntityId, NotificationStatusId.UNREAD, null);

        // Assert
        result.CountAsync().Should().Be(_unreadNotificationDetails.CountAsync());
    }

    [Fact]
    public async Task GetNotifications_WithReadStatus_ReturnsList()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        var result = await sut.GetNotifications(_iamUser.UserEntityId, NotificationStatusId.READ, null);

        // Assert
        result.CountAsync().Should().Be(_readNotificationDetails.CountAsync());
    }

    [Fact]
    public async Task GetNotifications_WithoutStatus_ReturnsList()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        var result = await sut.GetNotifications(_iamUser.UserEntityId, null, null);

        // Assert
        result.CountAsync().Should().Be(_notificationDetails.CountAsync());
    }

    [Fact]
    public async Task GetNotifications_WithNotExistingCompanyUser_ThrowsForbiddenException()
    {
        // Arrange
        var iamUserId = Guid.NewGuid().ToString();
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        try
        {
            await sut.GetNotifications(iamUserId, null, null);
        }
        catch (ForbiddenException e)
        {
            // Assert
            e.Message.Should().Be($"iamUserId {iamUserId} is not assigned");
            return;
        }

        // Must not reach that code because of the exception
        false.Should().BeTrue();
    }

    #endregion

    #region Get Notification

    [Fact]
    public async Task GetNotification_WithMatchingId_ReturnsDetailData()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        var (_, title, message) = await sut.GetNotification(_notificationDetail.Id, _iamUser.UserEntityId);

        // Assert
        title.Should().Be("Test Title");
        message.Should().Be("Test Message");
    }

    [Fact]
    public async Task GetNotification_WithNotMatchingNotification_NotFoundException()
    {
        // Arrange
        var randomNotificationId = Guid.NewGuid();
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        try
        {
            await sut.GetNotification(randomNotificationId, _iamUser.UserEntityId);
        }
        catch (NotFoundException e)
        {
            // Assert
            e.Message.Should().Be("Notification does not exist.");
            return;
        }

        // Must not reach that code because of the exception
        false.Should().BeTrue();
    }

    [Fact]
    public async Task GetNotification_WithNotExistingCompanyUser_ThrowsForbiddenException()
    {
        // Arrange
        var iamUserId = Guid.NewGuid().ToString();
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<NotificationBusinessLogic>();

        // Act
        try
        {
            await sut.GetNotification(Guid.NewGuid(), iamUserId);
        }
        catch (ForbiddenException e)
        {
            // Assert
            e.Message.Should().Be($"iamUserId {iamUserId} is not assigned");
            return;
        }

        // Must not reach that code because of the exception
        false.Should().BeTrue();
    }

    #endregion
}
