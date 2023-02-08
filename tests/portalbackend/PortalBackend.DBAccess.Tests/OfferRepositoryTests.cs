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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;
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
        offerDetail.Documents.Should().NotBeNull();
        var documenttypeId = offerDetail!.Documents.Select(x => x.documentTypeId);
        documenttypeId.Should().NotContain(DocumentTypeId.APP_LEADIMAGE);
        documenttypeId.Should().NotContain(DocumentTypeId.APP_IMAGE);
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
        var offerDetail = await sut.GetAppUpdateData(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), "623770c5-cf38-4b9f-9a35-f8b9ae972e2e", new []{"de"}).ConfigureAwait(false);

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
    [InlineData(ServiceOverviewSorting.ProviderAsc, new [] { "Newest Service", "Newest Service 2" }, new [] { "some short Description", null })]
    [InlineData(ServiceOverviewSorting.ProviderDesc, new [] { "Newest Service 2", "Newest Service" }, new [] { null, "some short Description" })]
    [InlineData(ServiceOverviewSorting.ReleaseDateAsc, new [] { "Newest Service", "Newest Service 2" }, new [] { "some short Description", null })]
    [InlineData(ServiceOverviewSorting.ReleaseDateDesc, new [] { "Newest Service 2", "Newest Service" }, new [] { null, "some short Description" })]
    public async Task GetActiveServices_ReturnsExpectedResult(ServiceOverviewSorting sorting, IEnumerable<string> names, IEnumerable<string> descriptions)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetActiveServicesPaginationSource(sorting, null)(0, 15).ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail!.Count.Should().Be(names.Count());
        offerDetail.Data.Should().HaveSameCount(names);
        offerDetail.Data.Select(data => data.Title).Should().ContainInOrder(names);
        offerDetail.Data.Select(data => data.Description).Should().ContainInOrder(descriptions);
    }

    [Theory]
    [InlineData(ServiceTypeId.CONSULTANCE_SERVICE, 0, 2, 1, 1)]
    [InlineData(ServiceTypeId.DATASPACE_SERVICE, 0, 2, 0, 0)]
    [InlineData(null, 0, 2, 2, 2)]
    [InlineData(null, 1, 1, 2, 1)]
    [InlineData(null, 2, 1, 2, 0)]
    public async Task GetActiveServices_WithExistingServiceAndServiceType_ReturnsExpectedResult(ServiceTypeId? serviceTypeId, int page, int size, int count, int numData)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetActiveServicesPaginationSource(null, serviceTypeId)(page, size).ConfigureAwait(false);

        // Assert
        if (count == 0)
        {
            offerDetail.Should().BeNull();
        }
        else
        {
            offerDetail.Should().NotBeNull();
            offerDetail!.Count.Should().Be(count);
            offerDetail.Data.Should().HaveCount(numData);
        }
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
        offerDetail!.Documents.Select(x => x.documentTypeId).Should().Contain(DocumentTypeId.ADDITIONAL_DETAILS);
        Assert.IsType<ServiceDetailData>(offerDetail);
    }

    #endregion

    #region GetOfferDeclineDataAsync
    
    [Fact]
    public async Task GetOfferDeclineDataAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetOfferDeclineDataAsync(
            new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"),
            "3d8142f1-860b-48aa-8c2b-1ccb18699f65",
            OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail.OfferStatus.Should().Be(OfferStatusId.ACTIVE);
    }

    #endregion

    #region GetProviderOfferDataWithConsentStatusAsync

    [Fact]
    public async Task GetProviderOfferDataWithConsentStatusAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderOfferDataWithConsentStatusAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"),"3d8142f1-860b-48aa-8c2b-1ccb18699f65",OfferTypeId.APP).ConfigureAwait(false);
        
        // Assert
        result.OfferProviderData.Should().NotBeNull();
        result.OfferProviderData.LeadPictureId.Should().NotBeEmpty();
        result.OfferProviderData.LeadPictureId.Should().Be(new Guid("90a24c6d-1092-4590-ae89-a9d2bff1ea41"));
    }

    #endregion

    #region GetProvidedOffersData

    [Theory]
    [InlineData(OfferTypeId.APP, "3d8142f1-860b-48aa-8c2b-1ccb18699f65", new [] { "90a24c6d-1092-4590-ae89-a9d2bff1ea41", "00000000-0000-0000-0000-000000000000" })]
    [InlineData(OfferTypeId.SERVICE, "3d8142f1-860b-48aa-8c2b-1ccb18699f65", new [] { "00000000-0000-0000-0000-000000000000" })]
    [InlineData(OfferTypeId.CORE_COMPONENT, "3d8142f1-860b-48aa-8c2b-1ccb18699f65", new string[] {})]
    [InlineData(OfferTypeId.APP, "no such user", new string[] { })]
    public async Task GetProvidedOffersData_ReturnsExpectedResult(OfferTypeId offerTypeId, string iamUserId, IEnumerable<string> leadPictureIds)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var providerAppData = await sut.GetProvidedOffersData(offerTypeId, iamUserId).ToListAsync().ConfigureAwait(false);

        // Assert
        providerAppData.Should().HaveSameCount(leadPictureIds);
        providerAppData.OrderBy(item => item.LeadPictureId).Select(item => item.LeadPictureId).Should().ContainInOrder(leadPictureIds.Select(item => new Guid(item)).OrderBy(item => item));
    }

    #endregion

#region CreateDeleteAppAssignedUseCases

    [Theory]
    [InlineData(
        new [] { "8895a7b6-39bc-4483-a1de-958e19eb9109", "eac5baeb-65ce-47fd-954e-bf5b4d411ba0", "581aafa7-f43c-40fc-a64c-e7be13e6c861" },                                         // initialKeys
        new [] { "581aafa7-f43c-40fc-a64c-e7be13e6c861", "09986f28-a8be-4df7-a61b-2a1e9c243b74", "f5e5cc9a-eb76-4d72-bd0f-09af6dcd7190", "a69f819b-9d27-43c6-9ca0-fe37f11cfbdc" }, // updateKeys
        new [] { "09986f28-a8be-4df7-a61b-2a1e9c243b74", "f5e5cc9a-eb76-4d72-bd0f-09af6dcd7190", "a69f819b-9d27-43c6-9ca0-fe37f11cfbdc" },                                         // addedEntityKeys
        new [] { "8895a7b6-39bc-4483-a1de-958e19eb9109", "eac5baeb-65ce-47fd-954e-bf5b4d411ba0" }                                                                                  // removedEntityKeys
    )]

    public async Task CreateDeleteAppAssignedUseCases_Success(
        IEnumerable<string> initialKeys, IEnumerable<string> updateKeys,
        IEnumerable<string> addedEntityKeys, IEnumerable<string> removedEntityKeys)
    {
        var appId = Guid.NewGuid();
        var initialItems = initialKeys.Select(x => new Guid(x)).ToImmutableArray();
        var updateItems = updateKeys.Select(x => new Guid(x)).ToImmutableArray();
        var addedEntities = addedEntityKeys.Select(x => new AppAssignedUseCase(appId, new Guid(x))).OrderBy(x => x.UseCaseId).ToImmutableArray();
        var removedEntities = removedEntityKeys.Select(x => new AppAssignedUseCase(appId, new Guid(x))).OrderBy(x => x.UseCaseId).ToImmutableArray();

        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        sut.CreateDeleteAppAssignedUseCases(appId, initialItems, updateItems);

        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().AllSatisfy(entry => entry.Entity.Should().BeOfType<AppAssignedUseCase>());
        changedEntries.Should().HaveCount(addedEntities.Length + removedEntities.Length);
        var added = changedEntries.Where(entry => entry.State == Microsoft.EntityFrameworkCore.EntityState.Added).Select(x => (AppAssignedUseCase)x.Entity).ToImmutableArray();
        var modified = changedEntries.Where(entry => entry.State == Microsoft.EntityFrameworkCore.EntityState.Modified).Select(x => (AppAssignedUseCase)x.Entity).ToImmutableArray();
        var deleted = changedEntries.Where(entry => entry.State == Microsoft.EntityFrameworkCore.EntityState.Deleted).Select(x => (AppAssignedUseCase)x.Entity).ToImmutableArray();

        added.Should().HaveSameCount(addedEntities);
        added.OrderBy(x => x.UseCaseId).Zip(addedEntities).Should().AllSatisfy(x => (x.First.AppId == x.Second.AppId && x.First.UseCaseId == x.Second.UseCaseId).Should().BeTrue());
        modified.Should().BeEmpty();
        deleted.Should().HaveSameCount(removedEntities);
        deleted.OrderBy(x => x.UseCaseId).Zip(removedEntities).Should().AllSatisfy(x => (x.First.AppId == x.Second.AppId && x.First.UseCaseId == x.Second.UseCaseId).Should().BeTrue());
   }

    #endregion

    #region GetProviderCompanyUserIdforOffer

    [Fact]
    public async Task GetProviderCompanyUserIdForOfferUntrackedAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderCompanyUserIdForOfferUntrackedAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), "623770c5-cf38-4b9f-9a35-f8b9ae972e2e", OfferStatusId.CREATED, OfferTypeId.SERVICE);

        // Assert
        result.Should().NotBeNull();
        result.CompanyUserId.Should().Be(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"));
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
