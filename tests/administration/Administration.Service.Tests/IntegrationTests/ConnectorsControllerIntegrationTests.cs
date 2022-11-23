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

using System.Net;
using FluentAssertions;
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Controllers;
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Tests.EnpointSetup;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared.Extensions;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared.IntegrationTests;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.Tests.IntegrationTests;

public class ConnectorsControllerIntegrationTests : IClassFixture<IntegrationTestFactory<ConnectorsController>>
{
    private readonly IntegrationTestFactory<ConnectorsController> _factory;

    public ConnectorsControllerIntegrationTests(IntegrationTestFactory<ConnectorsController> factory)
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