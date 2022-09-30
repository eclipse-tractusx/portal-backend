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
using FluentAssertions;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="OfferSubscriptionsRepositoryTests"/>
/// </summary>
public class OfferSubscriptionsRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly IFixture _fixture;
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _existingSubscruptionId = new("28149c6d-833f-49c5-aea2-ab6a5a37f462");
    private const string IamUserId = "623770c5-cf38-4b9f-9a35-f8b9ae972e2d";
    private const string InvalidUser = "4b8f156e-5dfc-4a58-9384-1efb195c1c34";

    public OfferSubscriptionsRepositoryTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetAutoSetupDataAsync

    [Fact]
    public async Task GetAutoSetupDataAsync_WithValidSubscriptionAndUser_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetThirdPartyAutoSetupDataAsync(_existingSubscruptionId, IamUserId).ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAutoSetupDataAsync_WithNotExistingSubscription_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetThirdPartyAutoSetupDataAsync(Guid.NewGuid(), IamUserId).ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results.Should().Be(default);
    }

    [Fact]
    public async Task GetAutoSetupDataAsync_WithNotExistingUser_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetThirdPartyAutoSetupDataAsync(_existingSubscruptionId, Guid.NewGuid().ToString()).ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results.IsUsersCompany.Should().Be(false);
    }

    [Fact]
    public async Task GetAutoSetupDataAsync_WithOtherCompanyUser_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetThirdPartyAutoSetupDataAsync(_existingSubscruptionId, InvalidUser).ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results.IsUsersCompany.Should().Be(false);
    }

    #endregion

    private async Task<(OfferSubscriptionsRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        _fixture.Inject(context);
        var sut = _fixture.Create<OfferSubscriptionsRepository>();
        return (sut, context);
    }
}
