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
        var result = await this._controller.UpdateOwnCompanyIdentityProvider(id, new IdentityProviderEditableDetails("test"), cancellationToken);

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
        await this._controller.DeleteOwnCompanyIdentityProvider(id);

        //Assert
        A.CallTo(() => _logic.DeleteCompanyIdentityProviderAsync(id)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetOwnIdentityProviderWithConnectedCompanies_WithValidData_ReturnsOk()
    {
        //Arrange
        var id = Guid.NewGuid();

        //Act
        await this._controller.GetOwnIdentityProviderWithConnectedCompanies(id);

        //Assert
        A.CallTo(() => _logic.GetOwnIdentityProviderWithConnectedCompanies(id)).MustHaveHappenedOnceExactly();
    }
}
