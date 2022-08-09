using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CatenaX.NetworkServices.Notification.Service.Tests.EnpointSetup;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Tests.Shared.IntegrationTests;
using FluentAssertions;
using Xunit;

namespace CatenaX.NetworkServices.Notification.Service.Tests.Controllers;

public class NotificationControllerTests : IClassFixture<IntegrationTestFactory<Program, PortalDbContext>>
{
    private readonly IntegrationTestFactory<Program, PortalDbContext> _factory;
    private static readonly Guid CompanyUserId = new("BF3B6533-3C36-4681-BA96-90CFB587ED7D");
    private static readonly string IamUserId = new Guid("217C7900-BC49-4882-83D9-C3C98083C584").ToString();

    public NotificationControllerTests(IntegrationTestFactory<Program, PortalDbContext> factory)
    {
        _factory = factory;
        _factory.SetupDbContext = dbContext =>
        {
            var company = new Company(Guid.NewGuid(), "Umberella Corporation", CompanyStatusId.ACTIVE, DateTime.UtcNow);
            var iamUser = new IamUser(IamUserId, CompanyUserId);
            var companyUser = new CompanyUser(CompanyUserId, company.Id, CompanyUserStatusId.ACTIVE, DateTime.UtcNow);
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

            dbContext.Companies.Add(company);
            dbContext.CompanyUsers.Add(companyUser);
            dbContext.IamUsers.Add(iamUser);
            dbContext.Notifications.AddRange(notifications);
            dbContext.SaveChanges();
        };
        // _factory.SeedDatabase(dbContext =>
        // {
        //     var company = new Company(Guid.NewGuid(), "Umberella Corporation", CompanyStatusId.ACTIVE, DateTime.UtcNow);
        //     var iamUser = new IamUser(IamUserId, CompanyUserId);
        //     var companyUser = new CompanyUser(CompanyUserId, company.Id, CompanyUserStatusId.ACTIVE, DateTime.UtcNow);
        //     var readNotifications = new List<PortalBackend.PortalEntities.Entities.Notification>();
        //     var unreadNotifications = new List<PortalBackend.PortalEntities.Entities.Notification>();
        //
        //     for (var i = 0; i < 3; i++)
        //     {
        //         readNotifications.Add(new PortalBackend.PortalEntities.Entities.Notification(Guid.NewGuid(), CompanyUserId, DateTimeOffset.UtcNow, i % 2 == 0 ? NotificationTypeId.ACTION : NotificationTypeId.INFO, true));
        //     }
        //
        //     for (var i = 0; i < 2; i++)
        //     {
        //         unreadNotifications.Add(new PortalBackend.PortalEntities.Entities.Notification(Guid.NewGuid(), CompanyUserId, DateTimeOffset.UtcNow, i % 2 == 0 ? NotificationTypeId.ACTION : NotificationTypeId.INFO, false));
        //     }
        //
        //     var notifications = readNotifications.Concat(unreadNotifications).ToList();
        //
        //     dbContext.Companies.Add(company);
        //     dbContext.CompanyUsers.Add(companyUser);
        //     dbContext.IamUsers.Add(iamUser);
        //     dbContext.Notifications.AddRange(notifications);
        //     dbContext.SaveChanges();
        // });
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
    }
}