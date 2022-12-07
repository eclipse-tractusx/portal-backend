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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using FluentAssertions;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="AgreementRepositoryTests"/>
/// </summary>
public class AgreementRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    public AgreementRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region Check Agreement exists

    [Fact]
    public async Task CheckAgreementExistsAsync_WithExistingEntry_ReturnsTrue()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .CheckAgreementExistsForSubscriptionAsync(new Guid("979a29b1-40c2-4169-979c-43c3156dbf64"), new Guid("28149c6d-833f-49c5-aea2-ab6a5a37f462"), OfferTypeId.SERVICE)
            .ConfigureAwait(false);

        // Assert
        results.Should().BeTrue();
    }

    [Fact]
    public async Task CheckAgreementExistsAsync_WithInvalidSubscription_ReturnsFalse()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .CheckAgreementExistsForSubscriptionAsync(new Guid("979a29b1-40c2-4169-979c-43c3156dbf64"), Guid.NewGuid(), OfferTypeId.SERVICE)
            .ConfigureAwait(false);

        // Assert
        results.Should().BeFalse();
    }

    [Fact]
    public async Task CheckAgreementExistsAsync_WithInvalidOfferType_ReturnsFalse()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .CheckAgreementExistsForSubscriptionAsync(new Guid("979a29b1-40c2-4169-979c-43c3156dbf64"), new Guid("28149c6d-833f-49c5-aea2-ab6a5a37f462"), OfferTypeId.APP)
            .ConfigureAwait(false);

        // Assert
        results.Should().BeFalse();
    }

    [Fact]
    public async Task CheckAgreementExistsAsync_WithInvalidGuid_ReturnsFalse()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.CheckAgreementExistsForSubscriptionAsync(Guid.NewGuid(), new Guid("28149c6d-833f-49c5-aea2-ab6a5a37f462"), OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        results.Should().BeFalse();
    }

    #endregion

    #region Get Agreements

    [Fact]
    public async Task GetActiveServices_ReturnsExpectedAppCount()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetAgreementsForCompanyRolesUntrackedAsync().ToListAsync().ConfigureAwait(false);

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(1);
    }

    #endregion

    #region Get OfferAgreementData for IamUser
    
    [Fact]
    public async Task GetOfferAgreementDataForIamUser_WithExistingUser_ReturnsExpectedCount()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);
    
        // Act
        var results = await sut.GetOfferAgreementDataForOfferId(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), OfferTypeId.SERVICE).ToListAsync().ConfigureAwait(false);
    
        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOfferAgreementDataForIamUser_WithNotExistingUser_ReturnsEmptyList()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOfferAgreementDataForOfferId(Guid.NewGuid(), OfferTypeId.SERVICE).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region CheckAgreementsExistsForOffer

    [Fact]
    public async Task CheckAgreementsExistsForOfferAsync_WithExistingAgreementForOffer_ReturnsTrue()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.CheckAgreementsExistsForOfferAsync(Enumerable.Repeat(new Guid("979a29b1-40c2-4169-979c-43c3156dbf64"), 1), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckAgreementsExistsForOfferAsync_WithNonExistingAgreementForOffer_ReturnsFalse()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.CheckAgreementsExistsForOfferAsync(Enumerable.Repeat(new Guid("979a29b1-40c2-4169-979c-43c3156dbf64"), 1), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    private async Task<(AgreementRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new AgreementRepository(context);
        return (sut, context);
    }
}
