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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class StaticDataBusinessLogicTest
{
    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly ICompanyRepository _companyRepository;
    private readonly IStaticDataRepository _staticDataRepository;

    public StaticDataBusinessLogicTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _companyRepository = A.Fake<ICompanyRepository>();
        _staticDataRepository = A.Fake<IStaticDataRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IStaticDataRepository>()).Returns(_staticDataRepository);
    }

    [Fact]
    public async Task GetAllUseCase_ReturnExpectedResult()
    {
        // Arrange
        var data = _fixture.Build<UseCaseData>()
                        .With(x => x.Name, "test")
                        .With(x => x.ShortName, "t")
                        .CreateMany(1)
                        .ToAsyncEnumerable();

        A.CallTo(() => _staticDataRepository.GetAllUseCase())
            .Returns(data);
        var sut = new StaticDataBusinessLogic(_portalRepositories);

        // Act
        var result = await sut.GetAllUseCase().ToListAsync().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _staticDataRepository.GetAllUseCase())
            .MustHaveHappenedOnceExactly();
        result.Should().ContainSingle()
            .Which.Should().BeOfType<UseCaseData>()
            .Which.Should().Match<UseCaseData>(x => x.Name == "test" && x.ShortName == "t");
    }

    [Fact]
    public async Task GetAllLicenseType_ReturnExpectedResult()
    {
        // Arrange
        var data = _fixture.Build<LicenseTypeData>()
                        .With(x => x.LicenseTypeId, 1)
                        .With(x => x.Name, LicenseTypeId.COTS.ToString())
                        .CreateMany()
                        .ToAsyncEnumerable();

        A.CallTo(() => _staticDataRepository.GetLicenseTypeData())
            .Returns(data);
        var sut = new StaticDataBusinessLogic(_portalRepositories);

        // Act
        var result = await sut.GetAllLicenseType().ToListAsync().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _staticDataRepository.GetLicenseTypeData())
            .MustHaveHappenedOnceExactly();
        result.Should().NotBeEmpty()
            .And.AllBeOfType<LicenseTypeData>()
            .Which.First().Should().Match<LicenseTypeData>(x => x.LicenseTypeId == 1 && x.Name == LicenseTypeId.COTS.ToString());
    }

    [Fact]
    public async Task GetAllLanguages_ReturnsExpectedResult()
    {
        // Arrange
        var data = _fixture.CreateMany<LanguageData>(3).ToImmutableArray();

        A.CallTo(() => _staticDataRepository.GetAllLanguage())
            .Returns(data.ToAsyncEnumerable());

        var sut = new StaticDataBusinessLogic(_portalRepositories);

        // Act
        var result = await sut.GetAllLanguage().ToListAsync().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _staticDataRepository.GetAllLanguage())
            .MustHaveHappenedOnceExactly();
        result.Should().HaveCount(3).And.ContainInOrder(data);
    }

    [Fact]
    public async Task GetOperatorBpns_ReturnsExpectedResult()
    {
        // Arrange
        var data = _fixture.CreateMany<OperatorBpnData>(3).ToImmutableArray();

        A.CallTo(() => _companyRepository.GetOperatorBpns())
            .Returns(data.ToAsyncEnumerable());

        var sut = new StaticDataBusinessLogic(_portalRepositories);

        // Act
        var result = await sut.GetOperatorBpns().ToListAsync().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companyRepository.GetOperatorBpns())
            .MustHaveHappenedOnceExactly();
        result.Should().HaveCount(3).And.ContainInOrder(data);
    }
}
