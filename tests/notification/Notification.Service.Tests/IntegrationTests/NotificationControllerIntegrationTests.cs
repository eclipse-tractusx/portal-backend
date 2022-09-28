/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
using System.Net;
using System.Threading.Tasks;
using Org.Eclipse.TractusX.Portal.Backend.Notification.Service.Tests.EnpointSetup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.IntegrationTests;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.TestSeeds;
using FluentAssertions;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Notification.Service.Tests.IntegrationTests;

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
        var endpoint = new NotificationEndpoints(client);
        
        // Act
        var response = await endpoint.NotificationCount(false);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var count = await response.GetResultFromContent<int>();
        count.Should().Be(2);
    }
}