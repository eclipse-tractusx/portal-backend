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
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;
using System.Collections.Immutable;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests;

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
    public async Task GetAllCountries_ReturnsExpectedResult()
    {
        // Arrange
        var data = _fixture.CreateMany<CountriesLongNamesData>(3).ToImmutableArray();

        A.CallTo(() => _staticDataRepository.GetAllCountries())
            .Returns(data.ToAsyncEnumerable());

        var sut = new StaticDataBusinessLogic(_portalRepositories);

        // Act
        var result = await sut.GetAllCountries().ToListAsync().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _staticDataRepository.GetAllLanguage())
            .MustHaveHappenedOnceExactly();
        result.Should().HaveCount(3).And.ContainInOrder(data);
    }
}
