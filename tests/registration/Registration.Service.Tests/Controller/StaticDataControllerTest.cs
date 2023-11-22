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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests;

public class StaticDataControllerTest
{
    private readonly IStaticDataBusinessLogic _logic;
    private readonly StaticDataController _controller;
    private readonly Fixture _fixture;
    public StaticDataControllerTest()
    {
        _fixture = new Fixture();
        _logic = A.Fake<IStaticDataBusinessLogic>();
        this._controller = new StaticDataController(_logic);
    }

    [Fact]
    public async Task GetCountries_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.CreateMany<CountriesLongNamesData>(5).ToAsyncEnumerable();
        A.CallTo(() => _logic.GetAllCountries())
            .Returns(data);

        //Act
        var result = await _controller.GetCountries().ToListAsync().ConfigureAwait(false);

        // Assert 
        A.CallTo(() => _logic.GetAllCountries()).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5).And.AllBeOfType<CountriesLongNamesData>();
    }
}
