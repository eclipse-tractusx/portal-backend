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

using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.Tests.EnpointSetup;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.IntegrationTests;
using System.Net;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.Tests.IntegrationTests;

public class NotificationControllerIntegrationTests : IClassFixture<IntegrationTestFactory<NotificationController>>
{
    private readonly IntegrationTestFactory<NotificationController> _factory;

    public NotificationControllerIntegrationTests(IntegrationTestFactory<NotificationController> factory)
    {
        _factory = factory;
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
        count.Should().Be(3);
    }
}