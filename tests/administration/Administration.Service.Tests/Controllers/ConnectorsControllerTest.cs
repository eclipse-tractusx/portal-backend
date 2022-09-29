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
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Controllers;
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared.Extensions;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.Tests.Controllers;

public class ConnectorsControllerTest
{
    private const string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private const string AccessToken = "superSafeToken";
    private readonly IConnectorsBusinessLogic _logic;
    private readonly ConnectorsController _controller;

    public ConnectorsControllerTest()
    {
        _logic = A.Fake<IConnectorsBusinessLogic>();
        this._controller = new ConnectorsController(_logic);
        _controller.AddControllerContextWithClaimAndBearerTokenX(IamUserId, AccessToken);
    }

    [Fact]
    public async Task CreateConnectorAsync_WithValidData_ReturnsExpectedResult()
    {
        //Arrange
        var connectorInputModel = new ConnectorInputModel(
            "New Connector", 
            "https://connec-tor.com",
            ConnectorTypeId.CONNECTOR_AS_A_SERVICE,
            ConnectorStatusId.ACTIVE,
            "the location",
            Guid.NewGuid(),
            Guid.NewGuid());
        var connectorResult = new ConnectorData("New Connector", "the location")
        {
            Id = Guid.NewGuid(),
            Status = ConnectorStatusId.ACTIVE,
            Type = ConnectorTypeId.CONNECTOR_AS_A_SERVICE
        };
        A.CallTo(() => _logic.CreateConnectorAsync(connectorInputModel, AccessToken, IamUserId, false))
            .ReturnsLazily(() => connectorResult);

        //Act
        var result = await this._controller.CreateConnectorAsync(connectorInputModel).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.CreateConnectorAsync(connectorInputModel, AccessToken, IamUserId, false)).MustHaveHappenedOnceExactly();
        Assert.IsType<CreatedAtRouteResult>(result);
        result.Value.Should().Be(connectorResult);
    }
}