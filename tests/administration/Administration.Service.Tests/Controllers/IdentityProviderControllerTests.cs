/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class IdentityProviderControllerTests
{
    private readonly IIdentityProviderBusinessLogic _logic;
    private readonly IdentityProviderController _controller;
    private readonly Fixture _fixture;

    public IdentityProviderControllerTests()
    {
        _fixture = new Fixture();
        _logic = A.Fake<IIdentityProviderBusinessLogic>();
        _controller = new IdentityProviderController(_logic);
    }

    [Fact]
    public async Task UpdateOwnCompanyIdentityProvider_WithValidData_ReturnsOk()
    {
        //Arrange
        var id = Guid.NewGuid();
        var data = _fixture.Create<IdentityProviderDetails>();
        A.CallTo(() => _logic.UpdateOwnCompanyIdentityProviderAsync(A<Guid>._, A<IdentityProviderEditableDetails>._, A<CancellationToken>._))
            .Returns(data);
        var cancellationToken = CancellationToken.None;

        //Act
        var result = await _controller.UpdateOwnCompanyIdentityProvider(id, new IdentityProviderEditableDetails("test"), cancellationToken);

        //Assert
        result.Should().Be(data);
        A.CallTo(() => _logic.UpdateOwnCompanyIdentityProviderAsync(id, A<IdentityProviderEditableDetails>.That.Matches(x => x.DisplayName == "test"), cancellationToken)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteOwnCompanyIdentityProvider_WithValidData_ReturnsOk()
    {
        //Arrange
        var id = Guid.NewGuid();

        //Act
        await _controller.DeleteOwnCompanyIdentityProvider(id);

        //Assert
        A.CallTo(() => _logic.DeleteCompanyIdentityProviderAsync(id)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetOwnIdentityProviderWithConnectedCompanies_WithValidData_ReturnsOk()
    {
        //Arrange
        var id = Guid.NewGuid();

        //Act
        await _controller.GetOwnIdentityProviderWithConnectedCompanies(id);

        //Assert
        A.CallTo(() => _logic.GetOwnIdentityProviderWithConnectedCompanies(id)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateSharedIdpInstanceDetails_WithValidData_CallsBusinessLogic()
    {
        // Arrange
        var request = _fixture.Create<SharedIdpInstanceRequestData>();

        // Act
        await _controller.CreateSharedIdpInstanceDetails(request);

        // Assert
        A.CallTo(() => _logic.CreateSharedIdpInstanceDetails(request)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateSharedIdpInstanceDetails_WithValidData_CallsBusinessLogicAndReturnsResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = _fixture.Create<SharedIdpInstanceUpdateRequestData>();
        var expectedResponse = _fixture.Create<SharedIdpInstanceResponseData>();

        A.CallTo(() => _logic.UpdateSharedIdpInstanceDetails(id, request))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.UpdateSharedIdpInstanceDetails(id, request);

        // Assert
        result.Should().Be(expectedResponse);
        A.CallTo(() => _logic.UpdateSharedIdpInstanceDetails(id, request)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetSharedIdpInstanceDetails_CallsBusinessLogicAndReturnsResult()
    {
        // Arrange
        var expectedList = _fixture.CreateMany<SharedIdpInstanceResponseData>(3).ToList();
        A.CallTo(() => _logic.GetSharedIdpInstanceDetails())
            .Returns(expectedList);

        // Act
        var result = await _controller.GetSharedIdpInstanceDetails();

        // Assert
        result.Should().BeEquivalentTo(expectedList);
        A.CallTo(() => _logic.GetSharedIdpInstanceDetails()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SyncSharedIdpRealmMapping_CallsBusinessLogicAndReturnsProcessId()
    {
        // Arrange
        var processId = Guid.NewGuid();
        A.CallTo(() => _logic.SyncSharedIdpRealmMapping())
            .Returns(processId);

        // Act
        var result = await _controller.SyncSharedIdpRealmMapping();

        // Assert
        result.Should().Be(processId);
        A.CallTo(() => _logic.SyncSharedIdpRealmMapping()).MustHaveHappenedOnceExactly();
    }
}
