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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
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
    [InlineData("DE", new[] { UniqueIdentifierId.COMMERCIAL_REG_NUMBER, UniqueIdentifierId.VAT_ID, UniqueIdentifierId.EORI, UniqueIdentifierId.LEI_CODE }, true)]
    [InlineData("PT", new[] { UniqueIdentifierId.COMMERCIAL_REG_NUMBER, UniqueIdentifierId.VAT_ID, UniqueIdentifierId.EORI }, true)]
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
    [InlineData("DE", new[] { BpdmIdentifierId.EU_VAT_ID_DE, BpdmIdentifierId.CH_UID }, new[] { BpdmIdentifierId.EU_VAT_ID_DE }, new[] { UniqueIdentifierId.VAT_ID }, true)]
    [InlineData("DE", new BpdmIdentifierId[] { }, new BpdmIdentifierId[] { }, new UniqueIdentifierId[] { }, true)]
    [InlineData("PT", new[] { BpdmIdentifierId.EU_VAT_ID_DE, BpdmIdentifierId.CH_UID }, new BpdmIdentifierId[] { }, new UniqueIdentifierId[] { }, true)]
    [InlineData("XY", new[] { BpdmIdentifierId.EU_VAT_ID_DE, BpdmIdentifierId.CH_UID }, null, null, false)]
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

    [Fact]
    public async Task GetServiceTypeData_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetServiceTypeData().ToListAsync().ConfigureAwait(false);

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLicenseTypeData_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetLicenseTypeData().ToListAsync().ConfigureAwait(false);

        // Assert
        results.Should().HaveCount(2);
    }

    #region GetAllLanguages

    [Fact]
    public async Task GetAllLanguages_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetAllLanguage().ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(10).And.Satisfy(
            x => x.LanguageShortName == "cn" && x.LanguageLongNames.OrderBy(ln => ln.Language).SequenceEqual(new LanguageDataLongName[] { new("de", "chinesisch"), new("en", "chinese") }),
            x => x.LanguageShortName == "de" && x.LanguageLongNames.OrderBy(ln => ln.Language).SequenceEqual(new LanguageDataLongName[] { new("de", "deutsch"), new("en", "german"), new("xx", "german_xx") }),
            x => x.LanguageShortName == "en" && x.LanguageLongNames.OrderBy(ln => ln.Language).SequenceEqual(new LanguageDataLongName[] { new("de", "englisch"), new("en", "english"), new("xx", "english_xx") }),
            x => x.LanguageShortName == "es" && x.LanguageLongNames.OrderBy(ln => ln.Language).SequenceEqual(new LanguageDataLongName[] { new("de", "spanisch"), new("en", "spanish") }),
            x => x.LanguageShortName == "fr" && x.LanguageLongNames.OrderBy(ln => ln.Language).SequenceEqual(new LanguageDataLongName[] { new("de", "franzoesisch"), new("en", "french") }),
            x => x.LanguageShortName == "jp" && x.LanguageLongNames.OrderBy(ln => ln.Language).SequenceEqual(new LanguageDataLongName[] { new("de", "japanisch"), new("en", "japanese") }),
            x => x.LanguageShortName == "pt" && x.LanguageLongNames.OrderBy(ln => ln.Language).SequenceEqual(new LanguageDataLongName[] { new("de", "portugisisch"), new("en", "portuguese") }),
            x => x.LanguageShortName == "ru" && x.LanguageLongNames.OrderBy(ln => ln.Language).SequenceEqual(new LanguageDataLongName[] { new("de", "russisch"), new("en", "russian") }),
            x => x.LanguageShortName == "xx" && x.LanguageLongNames.OrderBy(ln => ln.Language).SequenceEqual(new LanguageDataLongName[] { new("de", "xx_german"), new("en", "xx_english"), new("xx", "xx_xx") }),
            x => x.LanguageShortName == "yy" && x.LanguageLongNames.Count() == 0
        );
    }

    #endregion

    #region GetCertificateTypes

    [Fact]
    public async Task GetCertificateTypes_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetCertificateTypes().ToListAsync().ConfigureAwait(false);

        // Assert
        results.Should().HaveCount(12);
    }

    [Fact]
    public async Task GetCertificateTypes_WithInactiveCertificateType_ReturnsExpectedResult()
    {
        // Arrange
        var (context, sut) = await CreateSutWithContext().ConfigureAwait(false);
        var active = new CompanyCertificateTypeAssignedStatus(CompanyCertificateTypeId.ISO_15504_SPICE, CompanyCertificateTypeStatusId.ACTIVE);
        var inactive = new CompanyCertificateTypeAssignedStatus(CompanyCertificateTypeId.ISO_15504_SPICE, CompanyCertificateTypeStatusId.INACTVIE);
        context.Remove(active);
        context.Add(inactive);
        await context.SaveChangesAsync().ConfigureAwait(false);

        try
        {
            // Act
            var results = await sut.GetCertificateTypes().ToListAsync().ConfigureAwait(false);

            // Assert
            results.Should().HaveCount(11);
        }
        finally
        {
            context.Remove(inactive);
            context.Add(active);
            await context.SaveChangesAsync().ConfigureAwait(false);
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

    private async Task<(PortalDbContext, StaticDataRepository)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new StaticDataRepository(context);
        return (context, sut);
    }

    #endregion
}
