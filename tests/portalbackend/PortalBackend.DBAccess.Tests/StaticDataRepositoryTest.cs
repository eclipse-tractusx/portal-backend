/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class StaticDataRepositoryTest : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    public StaticDataRepositoryTest(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetCompanyIdentifiers

    [Theory]
    [InlineData("DE", new [] { UniqueIdentifierId.COMMERCIAL_REG_NUMBER, UniqueIdentifierId.VAT_ID }, true)]
    [InlineData("PT", new UniqueIdentifierId[] {}, true)]
    [InlineData("XY", null, false)]
    public async Task GetCompanyIdentifiers_ReturnsExpectedResult(string countryCode, IEnumerable<UniqueIdentifierId>? expectedIds, bool validCountry)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetCompanyIdentifiers(countryCode).ConfigureAwait(false);

        // Assert
        result.IsValidCountryCode.Should().Be(validCountry);
        if (result.IsValidCountryCode)
        {
            result.IdentifierIds.Should().NotBeNull();
            result.IdentifierIds.Should().HaveSameCount(expectedIds);
            result.IdentifierIds.OrderBy(item => item).Should().ContainInOrder(expectedIds?.OrderBy(item => item));
        }
        else
        {
            result.IdentifierIds.Should().BeNull();
        }
    }

    #endregion

    #region GetCountryAssignedIdentifiers

    [Theory]
    [InlineData("DE", new [] { BpdmIdentifierId.EU_VAT_ID_DE, BpdmIdentifierId.CH_UID }, new [] { BpdmIdentifierId.EU_VAT_ID_DE }, new [] { UniqueIdentifierId.VAT_ID }, true)]
    [InlineData("DE", new BpdmIdentifierId [] {}, new BpdmIdentifierId [] {}, new UniqueIdentifierId [] {}, true)]
    [InlineData("PT", new [] { BpdmIdentifierId.EU_VAT_ID_DE, BpdmIdentifierId.CH_UID }, new BpdmIdentifierId [] {}, new UniqueIdentifierId[] {}, true)]
    [InlineData("XY", new [] { BpdmIdentifierId.EU_VAT_ID_DE, BpdmIdentifierId.CH_UID }, null, null, false)]
    public async Task GetCountryAssignedIdentifiers_ReturnsExpectedResult(string countryCode, IEnumerable<BpdmIdentifierId> bpdmIdentifiers, IEnumerable<BpdmIdentifierId>? expectedBpdmIds, IEnumerable<UniqueIdentifierId>? expectedUniqueIds, bool validCountry)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetCountryAssignedIdentifiers(bpdmIdentifiers, countryCode).ConfigureAwait(false);

        // Assert
        result.IsValidCountry.Should().Be(validCountry);
        if (result.IsValidCountry)
        {
            var expectedIds = (IEnumerable<(BpdmIdentifierId BpdmId, UniqueIdentifierId UniqueId)>)expectedBpdmIds!.Zip(expectedUniqueIds!);
            result.Identifiers.Should().NotBeNull();
            result.Identifiers.Should().HaveSameCount(expectedIds);
            result.Identifiers.OrderBy(item => item).Should().ContainInOrder(expectedIds?.OrderBy(item => item));
        }
        else
        {
            result.Identifiers.Should().BeNull();
        }
    }

    #endregion

    #region setup

    private async Task<StaticDataRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new StaticDataRepository(context);
        return sut;
    }

    #endregion
}
