using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CatenaX.NetworkServices.Notification.Service.Tests.EnpointSetup;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Tests.Shared.Extensions;
using CatenaX.NetworkServices.Tests.Shared.IntegrationTests;
using CatenaX.NetworkServices.Tests.Shared.TestSeeds;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace CatenaX.NetworkServices.Notification.Service.Tests.IntegrationTests;

public class NotificationControllerIntegrationTests : IClassFixture<IntegrationTestFactory<Program>>
{
    private readonly IntegrationTestFactory<Program> _factory;
    private static readonly Guid CompanyUserId = new("ac1cf001-7fbc-1f2f-817f-bce058020001");

    public NotificationControllerIntegrationTests(IntegrationTestFactory<Program> factory)
    {
        _factory = factory;
        var readNotifications = new List<PortalBackend.PortalEntities.Entities.Notification>();
        var unreadNotifications = new List<PortalBackend.PortalEntities.Entities.Notification>();

        for (var i = 0; i < 3; i++)
        {
            readNotifications.Add(new PortalBackend.PortalEntities.Entities.Notification(Guid.NewGuid(),
                CompanyUserId, DateTimeOffset.UtcNow,
                i % 2 == 0 ? NotificationTypeId.ACTION : NotificationTypeId.INFO, true));
        }

        for (var i = 0; i < 2; i++)
        {
            unreadNotifications.Add(new PortalBackend.PortalEntities.Entities.Notification(Guid.NewGuid(),
                CompanyUserId, DateTimeOffset.UtcNow,
                i % 2 == 0 ? NotificationTypeId.ACTION : NotificationTypeId.INFO, false));
        }

        var notifications = readNotifications.Concat(unreadNotifications).ToList();

        _factory.SetupDbActions = new[]
        {
            SeedExtensions.SeedNotification(notifications.ToArray())
        };
    }

    [Fact]
    public async Task NotificationCount_WithTwoUnreadNotifications_ReturnsCorrectAmount()
    {
        // Arrange
        var client = _factory.CreateClient();
        var endpoint = new NotificationEndpoint(client);
        
        // Act
        var response = await endpoint.NotificationCount(false);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var count = await response.GetResultFromContent<int>();
        count.Should().Be(2);
    }
}