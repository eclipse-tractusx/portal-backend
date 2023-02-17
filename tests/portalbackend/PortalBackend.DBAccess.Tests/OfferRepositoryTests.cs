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
        var result = await sut.CheckAppExistsById(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfec")).ConfigureAwait(false);

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
        var offerDetail = await sut.GetOfferProviderDetailsAsync(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfeb"), OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail!.OfferName.Should().Be("Capacity Management");
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
        offers.Should().HaveCount(20);
    }

    #endregion

    #region GetAllActiveApps
    
    [Fact]
    public async Task GetOfferDetailsByIdAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetOfferDetailsByIdAsync(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfeb"), "3d8142f1-860b-48aa-8c2b-1ccb18699f65", null, "de", OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail!.Title.Should().Be("Capacity Management");
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
        var offerDetail = await sut.GetAppUpdateData(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfeb"), "502dabcf-01c7-47d9-a88e-0be4279097b5", new []{"de"}).ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail!.IsUserOfProvider.Should().BeTrue();
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
//    [InlineData(ServiceOverviewSorting.ReleaseDateAsc)] TODO cannot test as data contains ambigous values of date_released
//    [InlineData(ServiceOverviewSorting.ReleaseDateDesc)]
    public async Task GetActiveServices_ReturnsExpectedResult(ServiceOverviewSorting sorting)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetActiveServicesPaginationSource(sorting, null)(0, 15).ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail!.Count.Should().Be(7);
        offerDetail.Data.Should().HaveCount(7);
        if (sorting == ServiceOverviewSorting.ProviderAsc)
        {
            offerDetail.Data.Select(data => data.Provider).Should().BeInAscendingOrder();
        }
        if (sorting == ServiceOverviewSorting.ProviderDesc)
        {
            offerDetail.Data.Select(data => data.Provider).Should().BeInDescendingOrder();
        }
    }

    [Theory]
    [InlineData(ServiceTypeId.CONSULTANCE_SERVICE, 0, 2, 3, 2)]
    [InlineData(ServiceTypeId.DATASPACE_SERVICE, 0, 2, 3, 2)]
    [InlineData(null, 0, 2, 7, 2)]
    [InlineData(null, 1, 1, 7, 1)]
    [InlineData(null, 2, 1, 7, 1)]
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
        var offerDetail = await sut.GetServiceDetailByIdUntrackedAsync(new Guid("ac1cf001-7fbc-1f2f-817f-bce0000c0001"), "de", "502dabcf-01c7-47d9-a88e-0be4279097b5").ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail!.Title.Should().Be("Consulting Service - Data Readiness");
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
            new Guid("ac1cf001-7fbc-1f2f-817f-bce0000c0001"),
            "502dabcf-01c7-47d9-a88e-0be4279097b5",
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
        var result = await sut.GetProviderOfferDataWithConsentStatusAsync(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfeb"), "3d8142f1-860b-48aa-8c2b-1ccb18699f65", OfferTypeId.APP).ConfigureAwait(false);
        
        // Assert
        result.OfferProviderData.Should().NotBeNull();
        result.OfferProviderData.LeadPictureId.Should().NotBeEmpty();
        result.OfferProviderData.LeadPictureId.Should().Be(new Guid("d0d45a39-521f-4fa8-b8f4-146e20ce7575"));
    }

    #endregion

    #region GetProvidedOffersData

    [Theory]
    [InlineData(
        OfferTypeId.APP,
        "502dabcf-01c7-47d9-a88e-0be4279097b5",
        new [] {
            "5cf74ef8-e0b7-4984-a872-474828beb5d1",
            "5cf74ef8-e0b7-4984-a872-474828beb5d2",
            "5cf74ef8-e0b7-4984-a872-474828beb5d3",
            "5cf74ef8-e0b7-4984-a872-474828beb5d4",
            "5cf74ef8-e0b7-4984-a872-474828beb5d5",
            "5cf74ef8-e0b7-4984-a872-474828beb5d6",
            "5cf74ef8-e0b7-4984-a872-474828beb5d9",
            "a16e73b9-5277-4b69-9f8d-3b227495dfeb",
            "ac1cf001-7fbc-1f2f-817f-bce0572c0007",
            "f9cad59d-84b3-4880-a550-4072c26a6b93",
            "99c5fd12-8085-4de2-abfd-215e1ee4baa4",
            "99c5fd12-8085-4de2-abfd-215e1ee4baa6",
        },
        new [] {
            "d6eb6ec2-24a6-40c5-becb-2142c62fb117",
            "184cde16-52d4-4865-81f6-b5b45e3c9051",
            "184cde16-52d4-4865-81f6-b5b45e3c9050",
            "4487ce3a-2018-4e8f-82df-6d5f3440193a",
            "8cd4d4a3-e57c-4e19-a8a3-5f2b4fdfb9ad",
            "00af3c73-32ab-49c2-b02c-b65536a61aac",
            "00000000-0000-0000-0000-000000000000",
            "d0d45a39-521f-4fa8-b8f4-146e20ce7575",
            "384fa860-c48a-4c1f-bbe5-8f47877ad37e",
            "a221b9d8-e79a-43c4-9a25-edec28071c3c",
            "00000000-0000-0000-0000-000000000000",
            "00000000-0000-0000-0000-000000000000",
        })]
    [InlineData(
        OfferTypeId.SERVICE,
        "502dabcf-01c7-47d9-a88e-0be4279097b5",
        new [] {
            "ac1cf001-7fbc-1f2f-817f-bce0000c0001",
            "ac1cf001-7fbc-1f2f-817f-bce0000c0002",
            "ac1cf001-7fbc-1f2f-817f-bce0000c0003",
            "ac1cf001-7fbc-1f2f-817f-bce0000c0004",
            "ac1cf001-7fbc-1f2f-817f-bce0000c0005",
            "99c5fd12-8085-4de2-abfd-215e1ee4baa5",
        },
        new [] {
            "00000000-0000-0000-0000-000000000000",
            "00000000-0000-0000-0000-000000000000",
            "00000000-0000-0000-0000-000000000000",
            "00000000-0000-0000-0000-000000000000",
            "c6bc1f44-ad94-4478-b2b0-e741a77e83a9",
            "00000000-0000-0000-0000-000000000000",
        })]
    [InlineData(
        OfferTypeId.CORE_COMPONENT,
        "502dabcf-01c7-47d9-a88e-0be4279097b5",
        new [] {
            "0ffcb416-1101-4ba6-8d4a-a9dfa31745a4",
            "9b957704-3505-4445-822c-d7ef80f27fcd",
            "9ef01c20-6d9d-41ef-b336-fa64e1e2e4c2",
        },
        new [] {
            "00000000-0000-0000-0000-000000000000",
            "00000000-0000-0000-0000-000000000000",
            "00000000-0000-0000-0000-000000000000",
        })]
    [InlineData(
        OfferTypeId.APP,
        "no such user",
        new string[] { },
        new string[] { })]
    public async Task GetProvidedOffersData_ReturnsExpectedResult(OfferTypeId offerTypeId, string iamUserId, IEnumerable<string> offerIds, IEnumerable<string> leadPictureIds)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var providerAppData = await sut.GetProvidedOffersData(offerTypeId, iamUserId).ToListAsync().ConfigureAwait(false);

        // Assert
        providerAppData.Should().HaveSameCount(offerIds);
        if (offerIds.Any())
        {
            providerAppData.Select(x => x.Id).Should().Contain(offerIds.Select(x => new Guid(x)));
            providerAppData.Join(offerIds.Select(x => new Guid(x)).Zip(leadPictureIds.Select(x => new Guid(x))),data => data.Id, zip => zip.First, (data,zip) => (data,zip)).Should().AllSatisfy(x => x.data.LeadPictureId.Should().Be(x.zip.Second));
        }
        else
        {
            providerAppData.Should().BeEmpty();
        }
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
        var result = await sut.GetProviderCompanyUserIdForOfferUntrackedAsync(new Guid("0ffcb416-1101-4ba6-8d4a-a9dfa31745a4"), "502dabcf-01c7-47d9-a88e-0be4279097b5", OfferStatusId.ACTIVE, OfferTypeId.CORE_COMPONENT);

        // Assert
        result.Should().NotBeNull();
        result.CompanyUserId.Should().Be(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020006"));
    }

    #endregion

    #region GetInReviewAppData
    
    [Fact]
    public async Task GetInReviewAppDataByIdAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var InReviewAppofferDetail = await sut.GetinReviewAppDataByIdAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA6"), OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        InReviewAppofferDetail.Should().NotBeNull();
        InReviewAppofferDetail!.title.Should().Be("Latest Service");
        InReviewAppofferDetail!.Documents.Should().NotBeNull();
        var documenttypeId = InReviewAppofferDetail!.Documents.Select(x => x.documentTypeId);
        documenttypeId.Should().NotContain(DocumentTypeId.APP_LEADIMAGE);
        documenttypeId.Should().NotContain(DocumentTypeId.APP_IMAGE);
    }

    #endregion

    #region OfferDescription

    [Fact]
    public async Task GetActiveOfferDescriptionDataByIdAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        //Act
        var result = await sut.GetActiveOfferDescriptionDataByIdAsync(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfeb"), OfferTypeId.APP, "502dabcf-01c7-47d9-a88e-0be4279097b5").ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.IsStatusActive.Should().BeTrue();
        result.IsProviderCompanyUser.Should().BeTrue();
        result.OfferDescriptionDatas.Should().NotBeNull();
        result.OfferDescriptionDatas!.Select(od => od.languageCode).Should().Contain("en");
    }

    [Fact]
    public async Task GetActiveOfferDescriptionDataByIdAsync_InvalidUser_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        //Act
        var result = await sut.GetActiveOfferDescriptionDataByIdAsync(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfeb"), OfferTypeId.APP, "invalid user").ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.IsStatusActive.Should().BeTrue();
        result.IsProviderCompanyUser.Should().BeFalse();
        result.OfferDescriptionDatas.Should().BeNull();
    }

    [Fact]
    public async Task CreateUpdateDeleteOfferDescriptions_Changed_ReturnsExpectedResult()
    {
        // Arrange
        var appId = new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4");
        var existingOfferDescription = new [] { 
            new OfferDescriptionData("en", "some long Description for testing","some short Description for testing"),
            new OfferDescriptionData("de", "some long Description for testing","some short Description for testing")
        };
        var modifedOfferDescription = new [] { 
            ("en", "some long Description in english, for testing","some short Description in english for testing"),
            ("de", "some long Description in germen, for testing","some short Description in germen for testing")
        };

        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        //Act
        sut.CreateUpdateDeleteOfferDescriptions(appId,existingOfferDescription,modifedOfferDescription);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(2);
        changedEntries.Should().AllSatisfy(entry => entry.State.Should().Be(EntityState.Modified));
        changedEntries.Select(x => x.Entity).Should().AllBeOfType<OfferDescription>();
        changedEntries.Select(x => x.Entity).Cast<OfferDescription>().Select(x => (x.LanguageShortName, x.DescriptionLong, x.DescriptionShort)).Should().Contain(modifedOfferDescription);
    }

    [Fact]
    public async Task CreateUpdateDeleteOfferDescriptions_added_ReturnsExpectedResult()
    {
        // Arrange
        var appId = new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4");
        var newOfferDescriptionEntry = ("es", "newly added long Description for testing", "newly added short Description for testing");
        var existingOfferDescription = new [] { 
            new OfferDescriptionData("en", "some long Description for testing","some short Description for testing"),
            new OfferDescriptionData("de", "some long Description for testing","some short Description for testing")
        };
        var modifedOfferDescription = new [] { 
            ("en", "some long Description for testing","some short Description for testing"),
            ("de", "some long Description for testing","some short Description for testing"),
            newOfferDescriptionEntry
        };

        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        //Act
        sut.CreateUpdateDeleteOfferDescriptions(appId,existingOfferDescription,modifedOfferDescription);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Should().AllSatisfy(entry => entry.State.Should().Be(EntityState.Added));
        changedEntries.Select(x => x.Entity).Should().AllBeOfType<OfferDescription>();
        changedEntries.Select(x => x.Entity).Cast<OfferDescription>().Select(x => (x.LanguageShortName, x.DescriptionLong, x.DescriptionShort)).Should().Contain(new [] { newOfferDescriptionEntry });
    }

    [Fact]
    public async Task CreateUpdateDeleteOfferDescriptions_Deleted_ReturnsExpectedResult()
    {
        // Arrange
        var appId = new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4");
        var existingOfferDescriptions = new [] { 
            new OfferDescriptionData("en", "some long Description for testing","some short Description for testing"),
            new OfferDescriptionData("de", "some long Description for testing","some short Description for testing")
        };
        var modifedOfferDescriptions = new [] { ("de", "modified long Description for testing","modified short Description for testing") };

        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        //Act
        sut.CreateUpdateDeleteOfferDescriptions(appId,existingOfferDescriptions,modifedOfferDescriptions);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(2);
        changedEntries.Select(x => x.Entity).Should().AllBeOfType<OfferDescription>();
        var deletedEntities = changedEntries.Where(x => x.State == EntityState.Deleted).Select(x => (OfferDescription)x.Entity);
        deletedEntities.Should().HaveCount(1);
        deletedEntities.Select(x => x.LanguageShortName).Should().Contain(new [] { "en" });
        var modifiedEntities = changedEntries.Where(x => x.State == EntityState.Modified).Select(x => (OfferDescription)x.Entity);
        modifiedEntities.Should().HaveCount(1);
        modifiedEntities.Select(x => (x.LanguageShortName, x.DescriptionLong, x.DescriptionShort)).Should().Contain(modifedOfferDescriptions);
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
