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
using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="OfferRepository"/>
/// </summary>
public class OfferRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    public OfferRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region CheckAppExistsById

    [Fact]
    public async Task CheckAppExistsById_WithExistingEntry_ReturnsTrue()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.CheckAppExistsById(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4")).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckAppExistsById_WithNonExistingEntry_ReturnsFalse()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.CheckAppExistsById(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
    
    #region GetOfferProviderDetailsAsync
    
    [Fact]
    public async Task GetOfferProviderDetailsAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetOfferProviderDetailsAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail!.OfferName.Should().Be("Top App");
    }

    [Fact]
    public async Task GetOfferProviderDetailsAsync_WithNotExistingApp_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetOfferProviderDetailsAsync(Guid.NewGuid(), OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        offerDetail.Should().BeNull();
    }

    [Fact]
    public async Task GetOfferProviderDetailsAsync_WithWrongOfferType_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetOfferProviderDetailsAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        offerDetail.Should().BeNull();
    }

    #endregion
    
    #region GetAllActiveApps
    
    [Fact]
    public async Task GetAllActiveApps_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offers = await sut.GetAllActiveAppsAsync(null).ToListAsync().ConfigureAwait(false);

        // Assert
        offers.Should().HaveCount(1);
    }

    #endregion

    #region GetAllActiveApps
    
    [Fact]
    public async Task GetOfferDetailsByIdAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetOfferDetailsByIdAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), "623770c5-cf38-4b9f-9a35-f8b9ae972e2e", null, "de", OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail!.Title.Should().Be("Top App");
    }

    #endregion

    #region Create Offer

    [Fact]
    public async Task CreateOffer_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        var results = sut.CreateOffer("Catena-X", OfferTypeId.APP, offer =>
        {
            offer.Name = "Test App";
        });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        results.Name.Should().Be("Test App");
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<Offer>().Which.Name.Should().Be("Test App");
    }

    #endregion
    
    #region AttachAndModifyOffer
    
    [Fact]
    public async Task AttachAndModifyOffer_WithExistingOffer_UpdatesStatus()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        sut.AttachAndModifyOffer(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), offer =>
        {
            offer.Name = "test abc";
        });

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Modified);
        changedEntity.Entity.Should().BeOfType<Offer>().Which.Name.Should().Be("test abc");
    }
    
    #endregion

    #region Delete Offer
    
    [Fact]
    public async Task DeleteOffer_WithExistingOffer_RemovesOffer()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        sut.DeleteOffer(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"));

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Deleted);
    }

    #endregion
    
    #region Create Offer

    [Fact]
    public async Task CreateOfferLicense_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        var results = sut.CreateOfferLicenses("110");

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        results.Licensetext.Should().Be("110");
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<OfferLicense>().Which.Licensetext.Should().Be("110");
    }

    #endregion

    #region AttachAndModifyOfferLicense
    
    [Fact]
    public async Task AttachAndModifyOfferLicense_WithExistingOfferLicense_UpdatesLicenseText()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        sut.AttachAndModifyOfferLicense(new Guid("6ca00fc6-4c82-47d8-8616-059ebe65232b"), offerLicense => offerLicense.Licensetext = "666");

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Modified);
        changedEntity.Entity.Should().BeOfType<OfferLicense>().Which.Licensetext.Should().Be("666");
    }
    
    #endregion

    #region Remove Offer Assigned License
    
    [Fact]
    public async Task RemoveOfferAssignedLicense_WithExisting_RemovesOfferAssignedLicense()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        sut.RemoveOfferAssignedLicense(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new Guid("6ca00fc6-4c82-47d8-8616-059ebe65232b"));

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Deleted);
    }

    #endregion

    #region RemoveAppLanguages
    
    [Fact]
    public async Task RemoveAppLanguages_WithExisting_RemovesAppLanguages()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        sut.RemoveAppLanguages(new [] { (new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), "de") });

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Deleted);
    }

    #endregion
    
    #region GetAppUpdateData
    
    [Fact]
    public async Task GetAppUpdateData_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetAppUpdateData(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), "623770c5-cf38-4b9f-9a35-f8b9ae972e2e", new []{"de"}, new []{ new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b90")}).ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail!.IsUserOfProvider.Should().Be(true);
    }

    #endregion
    
    #region RemoveServiceAssignedServiceTypes
    
    [Fact]
    public async Task AddServiceAssignedServiceTypes_WithExisting_RemovesServiceAssignedServiceType()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        sut.AddServiceAssignedServiceTypes(new [] { (new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), ServiceTypeId.DATASPACE_SERVICE) });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<ServiceAssignedServiceType>().Which.ServiceTypeId.Should().Be(ServiceTypeId.DATASPACE_SERVICE);
    }

    #endregion

    #region RemoveServiceAssignedServiceTypes
    
    [Fact]
    public async Task RemoveServiceAssignedServiceTypes_WithExisting_RemovesServiceAssignedServiceType()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        sut.RemoveServiceAssignedServiceTypes(new [] { (new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), ServiceTypeId.CONSULTANCE_SERVICE) });

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Deleted);
    }

    #endregion
    
    #region GetActiveServices
    
    [Theory]
    [InlineData(ServiceOverviewSorting.ProviderAsc)]
    [InlineData(ServiceOverviewSorting.ProviderDesc)]
    [InlineData(ServiceOverviewSorting.ReleaseDateAsc)]
    [InlineData(ServiceOverviewSorting.ReleaseDateDesc)]
    public async Task GetActiveServices_ReturnsExpectedResult(ServiceOverviewSorting sorting)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetActiveServicesPaginationSource(sorting, null)(0, 15).ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail!.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetActiveServices_WithExistingServiceAndServiceType_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetActiveServicesPaginationSource(null, ServiceTypeId.CONSULTANCE_SERVICE)(0, 15).ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail!.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetActiveServices_WithoutExistingServiceAndServiceType_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetActiveServicesPaginationSource(null, ServiceTypeId.DATASPACE_SERVICE)(0, 15).ConfigureAwait(false);

        // Assert
        offerDetail.Should().BeNull();
    }

    #endregion

    #region GetServiceDetailById
    
    [Fact]
    public async Task GetServiceDetailByIdUntrackedAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetServiceDetailByIdUntrackedAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), "de", "623770c5-cf38-4b9f-9a35-f8b9ae972e2e").ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail!.Title.Should().Be("Newest Service");
    }

    #endregion

    #region GetServiceUpdateData
    
    [Fact]
    public async Task GetServiceUpdateData_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetServiceUpdateData(
            new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"),
            Enumerable.Repeat(ServiceTypeId.CONSULTANCE_SERVICE, 1),
            "623770c5-cf38-4b9f-9a35-f8b9ae972e2e").ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail!.OfferState.Should().Be(OfferStatusId.ACTIVE);
    }

    #endregion

    #region Setup
    
    private async Task<OfferRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new OfferRepository(context);
        return sut;
    }

    private async Task<(OfferRepository repo, PortalDbContext context)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new OfferRepository(context);
        return (sut, context);
    }

    #endregion
}
