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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic.Tests;

public class PartnerNetworkBusinessLogicTests
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly ICompanyRepository _companyRepository;
    private readonly IPartnerNetworkBusinessLogic _sut;
    private readonly IFixture _fixture;

    public PartnerNetworkBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _companyRepository = A.Fake<ICompanyRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);

        _sut = new PartnerNetworkBusinessLogic(_portalRepositories);
    }

    [Fact]
    public async Task GetAllMemberCompaniesBPNAsync_ReturnsExpected()
    {
        //Arrange
        var bpnIds = _fixture.CreateMany<string>(3);
        A.CallTo(() => _companyRepository.GetAllMemberCompaniesBPNAsync(A<IEnumerable<string>>._))
            .Returns(_fixture.CreateMany<string>(2).ToAsyncEnumerable());

        // Act
        var result = await _sut.GetAllMemberCompaniesBPNAsync(bpnIds).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull().And.HaveCount(2);
        A.CallTo(() => _companyRepository.GetAllMemberCompaniesBPNAsync(A<IEnumerable<string>>.That.IsSameAs(bpnIds))).MustHaveHappenedOnceExactly();

    }
}
