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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers.V1;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.EnpointSetup;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.IntegrationTests.Seeding;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.IntegrationTests;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.IntegrationTests;

public class ConnectorsControllerIntegrationTests : IClassFixture<IntegrationTestFactory<ConnectorsController, BaseSeeding>>
{
    private readonly IntegrationTestFactory<ConnectorsController, BaseSeeding> _factory;

    public ConnectorsControllerIntegrationTests(IntegrationTestFactory<ConnectorsController, BaseSeeding> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCompanyConnectorsForCurrentUserAsync_WithTwoConnectors_ReturnsCorrectAmount()
    {
        // Arrange
        var client = _factory.CreateClient();
        var endpoint = new ConnectorsEndpoints(client);

        // Act
        var response = await endpoint.GetCompanyConnectorsForCurrentUserAsync().ConfigureAwait(false);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagination = await response.GetResultFromContent<Pagination.Response<ConnectorData>>();
        pagination.Content.Should().HaveCount(2);
    }
}
