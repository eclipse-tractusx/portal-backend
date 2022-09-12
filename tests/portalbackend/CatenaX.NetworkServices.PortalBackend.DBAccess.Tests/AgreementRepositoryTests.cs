/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Tests.Setup;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using FluentAssertions;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="AgreementRepositoryTests"/>
/// </summary>
public class AgreementRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly IFixture _fixture;
    private readonly TestDbFixture _dbTestDbFixture;

    public AgreementRepositoryTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region Check Agreement exists

    [Fact]
    public async Task CheckAgreementExistsAsync_WithExistingEntry_ReturnsTrue()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.CheckAgreementExistsAsync(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019951")).ConfigureAwait(false);

        // Assert
        results.Should().BeTrue();
    }

    [Fact]
    public async Task CheckAgreementExistsAsync_WithInvalidGuid_ReturnsFalse()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.CheckAgreementExistsAsync(Guid.NewGuid()).ConfigureAwait(false);

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
        var results = await sut.GetAgreementsUntrackedAsync().ToListAsync().ConfigureAwait(false);

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(2);
    }

    #endregion

    #region Get OfferAgreementData for IamUser
    
    [Fact]
    public async Task GetOfferAgreementDataForIamUser_WithNotExistingUser_ReturnsEmptyList()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOfferAgreementDataForIamUser(Guid.NewGuid().ToString()).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    private async Task<(AgreementRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        _fixture.Inject(context);
        var sut = _fixture.Create<AgreementRepository>();
        return (sut, context);
    }
}
