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

using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class ConnectorsControllerTests
{
    private const string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private const string AccessToken = "superSafeToken";
    private readonly IConnectorsBusinessLogic _logic;
    private readonly ConnectorsController _controller;
    private readonly Fixture _fixture;

    public ConnectorsControllerTests()
    {
        _fixture = new Fixture();
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
            "the location",
            null);
        var connectorId = _fixture.Create<Guid>();
        A.CallTo(() => _logic.CreateConnectorAsync(connectorInputModel, IamUserId, A<CancellationToken>._))
            .ReturnsLazily(() => connectorId);

        //Act
        var result = await this._controller.CreateConnectorAsync(connectorInputModel, CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.CreateConnectorAsync(connectorInputModel, IamUserId, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        Assert.IsType<CreatedAtRouteResult>(result);
        result.Value.Should().Be(connectorId);
    }
    
    [Fact]
    public async Task CreateManagedConnectorAsync_WithValidData_ReturnsExpectedResult()
    {
        //Arrange
        var connectorInputModel = new ManagedConnectorInputModel(
            "New Connector", 
            "https://connec-tor.com",
            "the location",
            "VALIDBPN1234",
            null);
        var connectorId = _fixture.Create<Guid>();
        A.CallTo(() => _logic.CreateManagedConnectorAsync(connectorInputModel, IamUserId, A<CancellationToken>._))
            .ReturnsLazily(() => connectorId);

        //Act
        var result = await this._controller.CreateManagedConnectorAsync(connectorInputModel, CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.CreateManagedConnectorAsync(connectorInputModel, IamUserId, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        Assert.IsType<CreatedAtRouteResult>(result);
        result.Value.Should().Be(connectorId);
    }
    
    [Fact]
    public async Task TriggerDaps_WithValidData_ReturnsExpectedResult()
    {
        //Arrange
        var connectorId = Guid.NewGuid();
        var file = FormFileHelper.GetFormFile("this is just random content", "cert.pem", "application/x-pem-file");
        A.CallTo(() => _logic.TriggerDapsAsync(connectorId, file, AccessToken, IamUserId, A<CancellationToken>._))
            .ReturnsLazily(() => true);

        //Act
        var result = await this._controller.TriggerDapsAuth(connectorId, file, CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.TriggerDapsAsync(connectorId, file, AccessToken, IamUserId, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task GetCompanyConnectorsForCurrentUser_WithValidData_ReturnsExpectedResult()
    {
        //Arrange
        var paginationResponse = new Pagination.Response<ConnectorData>(new Pagination.Metadata(15, 1, 1, 15), _fixture.CreateMany<ConnectorData>(5));
        A.CallTo(() => _logic.GetAllCompanyConnectorDatasForIamUserAsyncEnum(IamUserId, 0, 15))
            .Returns(paginationResponse);

        //Act
        var result = await this._controller.GetCompanyConnectorsForCurrentUserAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetAllCompanyConnectorDatasForIamUserAsyncEnum(IamUserId, 0, 15)).MustHaveHappenedOnceExactly();
        result.Content.Should().HaveCount(5);
    }
    
    [Fact]
    public async Task GetCompanyConnectorByIdForCurrentUser_WithValidData_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.Create<ConnectorData>();
        var connectorId = Guid.NewGuid();
        A.CallTo(() => _logic.GetCompanyConnectorDataForIdIamUserAsync(connectorId, IamUserId))
            .ReturnsLazily(() => data);

        //Act
        var result = await this._controller.GetCompanyConnectorByIdForCurrentUserAsync(connectorId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetCompanyConnectorDataForIdIamUserAsync(connectorId, IamUserId)).MustHaveHappenedOnceExactly();
        result.Should().Be(data);
    }
    
    [Fact]
    public async Task DeleteConnector_WithValidData_ReturnsExpectedResult()
    {
        //Arrange
        var connectorId = Guid.NewGuid();
        A.CallTo(() => _logic.DeleteConnectorAsync(connectorId))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        await this._controller.DeleteConnectorAsync(connectorId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.DeleteConnectorAsync(connectorId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompanyConnectorEndPoint_WithValidData_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.CreateMany<ConnectorEndPointData>(5);
        var bpns = new[]
        {
            "1",
            "2"
        };
        A.CallTo(() => _logic.GetCompanyConnectorEndPointAsync(bpns))
            .Returns(data.ToAsyncEnumerable());

        //Act
        var result = await this._controller.GetCompanyConnectorEndPointAsync(bpns).ToListAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetCompanyConnectorEndPointAsync(bpns)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

}
