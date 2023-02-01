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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="ServiceRepositoryTests"/>
/// </summary>
public class ServiceRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _offerId = new("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5");

    public ServiceRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region CreateService

    [Fact]
    public async Task CreateService_ReturnsExpectedAppCount()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = sut.CreateOffer("Catena X", OfferTypeId.SERVICE, service =>
        {
            service.Name = "Test Service";
            service.ContactEmail = "test@email.com";
        });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        results.Name.Should().Be("Test Service");
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<Offer>().Which.Name.Should().Be("Test Service");
    }

    #endregion

    #region GetServiceDetailByIdUntrackedAsync

    [Fact]
    public async Task GetServiceDetailByIdUntrackedAsync_WithNotExistingService_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetOfferDetailByIdUntrackedAsync(Guid.NewGuid(), "en", "3d8142f1-860b-48aa-8c2b-1ccb18699f65", OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        (results == default).Should().BeTrue();
    }

    [Fact]
    public async Task GetServiceDetailByIdUntrackedAsync_ReturnsServiceDetailData()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOfferDetailByIdUntrackedAsync(_offerId, "en", "3d8142f1-860b-48aa-8c2b-1ccb18699f65", OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Newest Service");
        result.ContactEmail.Should().Be("service-test@mail.com");
        result.Provider.Should<string>().Be("Catena X");
    }

    #endregion

    #region GetOfferProviderDetailsAsync
    
    [Fact]
    public async Task GetOfferProviderDetailsAsync_WithExistingOffer_ReturnsOfferProviderDetails()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOfferProviderDetailsAsync(_offerId, OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task GetOfferProviderDetailsAsync_WithNotExistingOffer_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOfferProviderDetailsAsync(Guid.NewGuid(), OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }
    
    #endregion
    
    private async Task<(OfferRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new OfferRepository(context);
        return (sut, context);
    }
}
