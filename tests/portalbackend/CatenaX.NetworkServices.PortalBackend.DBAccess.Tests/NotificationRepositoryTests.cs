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
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Tests.Shared;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="NotificationRepository"/>
/// </summary>
public class NotificationRepositoryTests
{
    private readonly IFixture _fixture;
    private readonly PortalDbContext _contextFake;
    private readonly Guid _companyUserId;
    private readonly string _iamUserId;
    private readonly ICollection<Notification> _readNotifications;
    private readonly ICollection<Notification> _unreadNotifications;
    private List<Notification> _notifications;

    public NotificationRepositoryTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _companyUserId = Guid.NewGuid();
        _iamUserId = Guid.NewGuid().ToString();
        _contextFake = A.Fake<PortalDbContext>();

        _readNotifications = new List<Notification>();
        _unreadNotifications = new List<Notification>();
        _notifications = new List<Notification>();
        SetupNotificationDb();
    }

    #region GetAllAsDetailsByUserIdUntracked

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithUnreadStatus_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        _fixture.Inject(_contextFake);

        var sut = _fixture.Create<NotificationRepository>();

        // Act
        var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(_iamUserId, false, null).ToListAsync();

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(_unreadNotifications.Count);
        results.Should().AllBeOfType<NotificationDetailData>();
        results.Should().AllSatisfy(x => _unreadNotifications.Select(notification => notification.Id).Contains(x.Id));
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatus_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        _fixture.Inject(_contextFake);

        var sut = _fixture.Create<NotificationRepository>();

        // Act
        var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(_iamUserId, true, null).ToListAsync();

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(_readNotifications.Count);
        results.Should().AllBeOfType<NotificationDetailData>();
        results.Should().AllSatisfy(x => _readNotifications.Select(notification => notification.Id).Contains(x.Id));
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatusAndInfoType_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        _fixture.Inject(_contextFake);

        var sut = _fixture.Create<NotificationRepository>();

        // Act
        var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(_iamUserId, true, NotificationTypeId.INFO).ToListAsync();

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(1);
        results.Should().AllBeOfType<NotificationDetailData>();
        results.Should().AllSatisfy(x => _readNotifications.Where(y => y.NotificationTypeId == NotificationTypeId.INFO).Select(notification => notification.Id).Contains(x.Id));
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatusAndActionType_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        _fixture.Inject(_contextFake);

        var sut = _fixture.Create<NotificationRepository>();

        // Act
        var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(_iamUserId, true, NotificationTypeId.ACTION).ToListAsync();

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(2);
        results.Should().AllBeOfType<NotificationDetailData>();
        results.Should().AllSatisfy(x => _readNotifications.Where(y => y.NotificationTypeId == NotificationTypeId.ACTION).Select(notification => notification.Id).Contains(x.Id));
    }

    #endregion

    #region GetNotificationCount

    [Fact]
    public async Task GetNotificationCountAsync_WithReadStatus_ReturnsExpectedCount()
    {
        // Arrange
        SetupCompanyUserDb();
        _fixture.Inject(_contextFake);

        var sut = _fixture.Create<NotificationRepository>();

        // Act
        var results = await sut.GetNotificationCountForIamUserAsync(_iamUserId, true);

        // Assert
        results.Count.Should().Be(_readNotifications.Count);
    }

    [Fact]
    public async Task GetNotificationCountAsync_WithoutStatus_ReturnsExpectedCount()
    {
        // Arrange
        SetupCompanyUserDb();
        _fixture.Inject(_contextFake);

        var sut = _fixture.Create<NotificationRepository>();

        // Act
        var results = await sut.GetNotificationCountForIamUserAsync(_iamUserId, null);

        // Assert
        results.Count.Should().Be(_notifications.Count);
    }

    #endregion

    private void SetupNotificationDb()
    {
        var companyUser = new CompanyUser(_companyUserId, Guid.NewGuid(), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow)
        {
            IamUser = new IamUser(_iamUserId, _companyUserId)
        };

        for (var i = 0; i < 3; i++)
        {
            _readNotifications.Add(new Notification(Guid.NewGuid(), _companyUserId, DateTimeOffset.Now, i % 2 == 0 ? NotificationTypeId.ACTION : NotificationTypeId.INFO, true)
            {
                Receiver = companyUser
            });
        }

        for (var i = 0; i < 2; i++)
        {
            _unreadNotifications.Add(new Notification(Guid.NewGuid(), _companyUserId, DateTimeOffset.Now, i % 2 == 0 ? NotificationTypeId.ACTION : NotificationTypeId.INFO, false)
            {
                Receiver = companyUser
            });
        }

        _notifications = _readNotifications.Concat(_unreadNotifications).ToList();
        A.CallTo(() => _contextFake.Notifications).Returns(_notifications.AsFakeDbSet());
    }

    private void SetupCompanyUserDb()
    {
        var companyUser = new CompanyUser(_companyUserId, Guid.NewGuid(), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow)
        {
            IamUser = new IamUser(_iamUserId, _companyUserId),
            Notifications = _notifications
        };

        A.CallTo(() => _contextFake.CompanyUsers).Returns(new List<CompanyUser>{companyUser}.AsFakeDbSet());
    }
}
