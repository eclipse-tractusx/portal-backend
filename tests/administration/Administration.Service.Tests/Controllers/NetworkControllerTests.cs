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

public class NetworkControllerTests
{
    private readonly INetworkBusinessLogic _logic;
    private readonly NetworkController _controller;
    private readonly Fixture _fixture;

    public NetworkControllerTests()
    {
        _fixture = new Fixture();
        _logic = A.Fake<INetworkBusinessLogic>();
        this._controller = new NetworkController(_logic);
    }

    [Fact]
    public async Task PartnerRegister_ReturnsExpected()
    {
        // Arrange
        var data = _fixture.Create<PartnerRegistrationData>();

        // Act
        var result = await this._controller.PartnerRegister(data).ConfigureAwait(false);

        // Assert
        result.StatusCode.Should().Be(200);
        A.CallTo(() => _logic.HandlePartnerRegistration(data)).MustHaveHappenedOnceExactly();
    }

}
