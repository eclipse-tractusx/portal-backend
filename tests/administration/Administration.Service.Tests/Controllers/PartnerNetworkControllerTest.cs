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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class PartnerNetworkControllerTest
{

    private readonly IPartnerNetworkBusinessLogic _logic;
    private readonly PartnerNetworkController _controller;
    private readonly IFixture _fixture;

    public PartnerNetworkControllerTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
        .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _logic = A.Fake<IPartnerNetworkBusinessLogic>();
        _controller = new PartnerNetworkController(_logic);
    }

    [Theory]
#pragma warning disable xUnit1012
    [InlineData(null)]
#pragma warning restore xUnit1012
    [InlineData("BPNL00000003LLHB", "CAXLBOSCHZZ")]
    public async Task GetAllMemberCompaniesBPN_Test(params string[]? bpnIds)
    {
        //Arrange
        var bpn = bpnIds == null ? null : bpnIds.ToList();
        var data = _fixture.CreateMany<string>(5);
        A.CallTo(() => _logic.GetAllMemberCompaniesBPNAsync(A<IEnumerable<string>?>._)).Returns(data.ToAsyncEnumerable());

        //Act
        var result = await _controller.GetAllMemberCompaniesBPNAsync(bpn).ToListAsync();

        //Assert
        A.CallTo(() => _logic.GetAllMemberCompaniesBPNAsync(A<IEnumerable<string>?>.That.IsSameAs(bpn))).MustHaveHappenedOnceExactly();

        result.Should().HaveSameCount(data).And.ContainInOrder(data);
    }
}
