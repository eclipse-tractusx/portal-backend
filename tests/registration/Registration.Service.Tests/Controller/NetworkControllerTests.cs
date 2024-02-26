/********************************************************************************
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

using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests.Controller;

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
    public async Task Submit_ReturnsExpected()
    {
        // Arrange
        var data = _fixture.Create<PartnerSubmitData>();

        // Act
        var result = await this._controller.Submit(data);

        // Assert
        result.StatusCode.Should().Be(204);
        A.CallTo(() => _logic.Submit(A<PartnerSubmitData>._))
            .MustHaveHappenedOnceExactly();
    }
}
