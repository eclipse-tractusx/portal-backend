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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

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
    public async Task GetUseCases_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.CreateMany<UseCaseData>(5).ToAsyncEnumerable();
        A.CallTo(() => _logic.GetAllUseCase())
            .Returns(data);

        //Act
        var result = await _controller.GetUseCases().ToListAsync().ConfigureAwait(false);

        // Assert 
        A.CallTo(() => _logic.GetAllUseCase()).MustHaveHappenedOnceExactly();
        Assert.IsType<List<UseCaseData>>(result);
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetLanguages_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.CreateMany<LanguageData>(5).ToAsyncEnumerable();
        A.CallTo(() => _logic.GetAllLanguage())
            .Returns(data);

        //Act
        var result = await _controller.GetLanguages().ToListAsync().ConfigureAwait(false);

        // Assert 
        A.CallTo(() => _logic.GetAllLanguage()).MustHaveHappenedOnceExactly();
        Assert.IsType<List<LanguageData>>(result);
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetLicenseTypes_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.CreateMany<LicenseTypeData>(5).ToAsyncEnumerable();
        A.CallTo(() => _logic.GetAllLicenseType())
            .Returns(data);

        //Act
        var result = await _controller.GetLicenseTypes().ToListAsync().ConfigureAwait(false);

        // Assert 
        A.CallTo(() => _logic.GetAllLicenseType()).MustHaveHappenedOnceExactly();
        Assert.IsType<List<LicenseTypeData>>(result);
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetOperatorBpns_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.CreateMany<OperatorBpnData>(5).ToAsyncEnumerable();
        A.CallTo(() => _logic.GetOperatorBpns())
            .Returns(data);

        //Act
        var result = await _controller.GetOperatorBpns().ToListAsync().ConfigureAwait(false);

        // Assert 
        A.CallTo(() => _logic.GetOperatorBpns()).MustHaveHappenedOnceExactly();
        Assert.IsType<List<OperatorBpnData>>(result);
        result.Should().HaveCount(5);
    }
}
