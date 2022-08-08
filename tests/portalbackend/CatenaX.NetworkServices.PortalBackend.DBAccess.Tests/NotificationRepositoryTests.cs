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
using FluentAssertions;
using Xunit;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="NotificationRepository"/>
/// </summary>
public class NotificationRepositoryTests : IClassFixture<TestDbFactory>
{
    private readonly IFixture _fixture;
    private readonly PortalDbContext _context;
    private readonly Guid _companyUserId;
    private readonly string _iamUserId;
    private readonly ICollection<Notification> _readNotifications;
    private readonly ICollection<Notification> _unreadNotifications;
    private List<Notification> _notifications;

    public NotificationRepositoryTests(TestDbFactory factory)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _companyUserId = Guid.NewGuid();
        _iamUserId = Guid.NewGuid().ToString();

        _readNotifications = new List<Notification>();
        _unreadNotifications = new List<Notification>();
        _notifications = new List<Notification>();

        _context = factory.GetPortalDbContext();
        SeedDb();
    }

    #region GetAllAsDetailsByUserIdUntracked

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithUnreadStatus_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        _fixture.Inject(_context);

        var sut = _fixture.Create<NotificationRepository>();

        // Act
        var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(_iamUserId, false, null).ToListAsync();

        // Assert
        var unreadNotificationIds = _unreadNotifications.Select(notification => notification.Id).ToList();
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(_unreadNotifications.Count);
        results.Should().AllBeOfType<NotificationDetailData>();
        results.Select(x => x.Id).Should().BeEquivalentTo(unreadNotificationIds);
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatus_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        _fixture.Inject(_context);

        var sut = _fixture.Create<NotificationRepository>();

        // Act
        var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(_iamUserId, true, null).ToListAsync();

        var readNotificationIds = _readNotifications.Select(notification => notification.Id).ToList();
        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(_readNotifications.Count);
        results.Should().AllBeOfType<NotificationDetailData>();
        results.Select(x => x.Id).Should().BeEquivalentTo(readNotificationIds);
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatusAndInfoType_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        _fixture.Inject(_context);

        var sut = _fixture.Create<NotificationRepository>();

        // Act
        var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(_iamUserId, true, NotificationTypeId.INFO).ToListAsync();

        // Assert
        var readNotificationIds = _readNotifications
            .Where(x => x.NotificationTypeId == NotificationTypeId.INFO)
            .Select(x => x.Id)
            .ToList();
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(1);
        results.Should().AllBeOfType<NotificationDetailData>();
        results.Select(x => x.Id).Should().BeEquivalentTo(readNotificationIds);
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatusAndActionType_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        _fixture.Inject(_context);

        var sut = _fixture.Create<NotificationRepository>();

        // Act
        var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(_iamUserId, true, NotificationTypeId.ACTION).ToListAsync();

        // Assert
        var readNotificationIds = _readNotifications
            .Where(x => x.NotificationTypeId == NotificationTypeId.ACTION)
            .Select(x => x.Id).ToList();
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(2);
        results.Should().AllBeOfType<NotificationDetailData>();
        results.Select(x => x.Id).Should().BeEquivalentTo(readNotificationIds);
    }

    #endregion

    #region GetNotificationCount

    [Fact]
    public async Task GetNotificationCountAsync_WithReadStatus_ReturnsExpectedCount()
    {
        // Arrange
        _fixture.Inject(_context);

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
        _fixture.Inject(_context);

        var sut = _fixture.Create<NotificationRepository>();

        // Act
        var results = await sut.GetNotificationCountForIamUserAsync(_iamUserId, null);

        // Assert
        results.Count.Should().Be(_notifications.Count);
    }

    #endregion

    private void SeedDb()
    {
        var company = new Company(Guid.NewGuid(), "Umberella Corporation", CompanyStatusId.ACTIVE, DateTime.UtcNow);
        var iamUser = new IamUser(_iamUserId, _companyUserId);
        var companyUser = new CompanyUser(_companyUserId, company.Id, CompanyUserStatusId.ACTIVE, DateTime.UtcNow);

        for (var i = 0; i < 3; i++)
        {
            _readNotifications.Add(new Notification(Guid.NewGuid(), _companyUserId, DateTimeOffset.UtcNow, i % 2 == 0 ? NotificationTypeId.ACTION : NotificationTypeId.INFO, true));
        }

        for (var i = 0; i < 2; i++)
        {
            _unreadNotifications.Add(new Notification(Guid.NewGuid(), _companyUserId, DateTimeOffset.UtcNow, i % 2 == 0 ? NotificationTypeId.ACTION : NotificationTypeId.INFO, false));
        }

        _notifications = _readNotifications.Concat(_unreadNotifications).ToList();

        _context.Companies.Add(company);
        _context.CompanyUsers.Add(companyUser);
        _context.IamUsers.Add(iamUser);
        _context.Notifications.AddRange(_notifications);
        _context.SaveChanges();
    }
}
