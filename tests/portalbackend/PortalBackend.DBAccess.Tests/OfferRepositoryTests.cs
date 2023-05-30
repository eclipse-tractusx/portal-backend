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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
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
    private const string UserCompanyId = "2dc4249f-b5ca-4d42-bef1-7a7a950a4f87";
    private readonly Guid _userCompanyId = new(UserCompanyId);

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
        var result = await sut.CheckAppExistsById(new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007")).ConfigureAwait(false);

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
        var offerDetail = await sut.GetOfferProviderDetailsAsync(new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007"), OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail!.OfferName.Should().Be("Trace-X");
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
        var offers = await sut.GetAllActiveAppsAsync(null!, Constants.DefaultLanguage).ToListAsync().ConfigureAwait(false);

        // Assert
        offers.Should().HaveCount(3);
    }

    #endregion

    #region GetOfferDetailsByIdAsync

    [Fact]
    public async Task GetOfferDetailsByIdAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetOfferDetailsByIdAsync(new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007"), _userCompanyId, null, "de", OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail!.Title.Should().Be("Trace-X");
        offerDetail.Documents.Should().NotBeNull();
        var documentTypeId = offerDetail.Documents.Select(x => x.DocumentTypeId);
        documentTypeId.Should().NotContain(DocumentTypeId.APP_LEADIMAGE);
        documentTypeId.Should().NotContain(DocumentTypeId.APP_IMAGE);
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
        sut.RemoveAppLanguages(new[] { (new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), "de") });

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
        var offerDetail = await sut.GetAppUpdateData(new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007"), _userCompanyId, new[] { "de" }).ConfigureAwait(false);

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
        sut.AddServiceAssignedServiceTypes(new[] { (new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), ServiceTypeId.DATASPACE_SERVICE) });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<ServiceDetail>().Which.ServiceTypeId.Should().Be(ServiceTypeId.DATASPACE_SERVICE);
    }

    #endregion

    #region RemoveServiceAssignedServiceTypes

    [Fact]
    public async Task RemoveServiceAssignedServiceTypes_WithExisting_RemovesServiceAssignedServiceType()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        sut.RemoveServiceAssignedServiceTypes(new[] { (new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), ServiceTypeId.CONSULTANCE_SERVICE) });

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
        var offerDetail = await sut.GetActiveServicesPaginationSource(sorting, null, Constants.DefaultLanguage)(0, 15).ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail!.Count.Should().Be(4);
        offerDetail.Data.Should().HaveCount(4);
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
    [InlineData(ServiceTypeId.CONSULTANCE_SERVICE, 0, 2, 1, 1)]
    [InlineData(ServiceTypeId.DATASPACE_SERVICE, 0, 2, 2, 2)]
    [InlineData(null, 0, 2, 4, 2)]
    [InlineData(null, 1, 1, 4, 1)]
    [InlineData(null, 2, 1, 4, 1)]
    public async Task GetActiveServices_WithExistingServiceAndServiceType_ReturnsExpectedResult(ServiceTypeId? serviceTypeId, int page, int size, int count, int numData)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var offerDetail = await sut.GetActiveServicesPaginationSource(null, serviceTypeId, Constants.DefaultLanguage)(page, size).ConfigureAwait(false);

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
        var offerDetail = await sut.GetServiceDetailByIdUntrackedAsync(new Guid("ac1cf001-7fbc-1f2f-817f-bce0000c0001"), "de", _userCompanyId).ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull()
            .And.BeOfType<ServiceDetailData>();
        offerDetail!.Title.Should().Be("Consulting Service - Data Readiness");
        offerDetail.Documents.Should()
            .NotBeEmpty()
            .And.AllSatisfy(
                x => x.DocumentTypeId.Should().Be(DocumentTypeId.ADDITIONAL_DETAILS)
            );
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
            OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        offerDetail.Should().NotBeNull();
        offerDetail.OfferStatus.Should().Be(OfferStatusId.ACTIVE);
        offerDetail.ActiveDocumentStatusDatas.Should().ContainSingle().And.Satisfy(
            x => x.DocumentId == new Guid("0d68c68c-d689-474c-a3be-8493f99feab2") && x.StatusId == DocumentStatusId.LOCKED);
    }

    #endregion

    #region GetProviderOfferDataWithConsentStatusAsync

    [Fact]
    public async Task GetProviderOfferDataWithConsentStatusAsync_APP_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderOfferDataWithConsentStatusAsync(new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007"), _userCompanyId, OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        result.IsProviderCompanyUser.Should().BeTrue();
        result.OfferProviderData.Should().NotBeNull();
        result.OfferProviderData!.LeadPictureId.Should().NotBeEmpty();
        result.OfferProviderData.LeadPictureId.Should().Be(new Guid("e020787d-1e04-4c0b-9c06-bd1cd44724b1"));
        result.OfferProviderData.UseCase.Should().NotBeNull();
        result.OfferProviderData.ServiceTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task GetProviderOfferDataWithConsentStatusAsync_APP_InvalidUser_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderOfferDataWithConsentStatusAsync(new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007"), Guid.NewGuid(), OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        result.IsProviderCompanyUser.Should().BeFalse();
        result.OfferProviderData.Should().BeNull();
    }

    [Fact]
    public async Task GetProviderOfferDataWithConsentStatusAsync_SERVICE_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderOfferDataWithConsentStatusAsync(new Guid("ac1cf001-7fbc-1f2f-817f-bce0000c0001"), _userCompanyId, OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        result.IsProviderCompanyUser.Should().BeTrue();
        result.OfferProviderData.Should().NotBeNull();
        result.OfferProviderData!.UseCase.Should().BeNull();
        result.OfferProviderData.ServiceTypeIds.Should().NotBeNull();
    }

    #endregion

    #region GetProvidedOffersData

    [Theory]
    [InlineData(
        OfferTypeId.APP,
        UserCompanyId,
        new[] {
            "ac1cf001-7fbc-1f2f-817f-bce0572c0007",
            "99c5fd12-8085-4de2-abfd-215e1ee4baa4",
            "99c5fd12-8085-4de2-abfd-215e1ee4baa6",
            "99c5fd12-8085-4de2-abfd-215e1ee4baa7",
            "99c5fd12-8085-4de2-abfd-215e1ee4baa9",
        },
        new[] {
            "e020787d-1e04-4c0b-9c06-bd1cd44724b1",
            "00000000-0000-0000-0000-000000000000",
            "00000000-0000-0000-0000-000000000000",
            "00000000-0000-0000-0000-000000000000",
        })]
    [InlineData(
        OfferTypeId.SERVICE,
        UserCompanyId,
        new[] {
            "ac1cf001-7fbc-1f2f-817f-bce0000c0001",
            "99c5fd12-8085-4de2-abfd-215e1ee4baa5",
            "99c5fd12-8085-4de2-abfd-215e1ee4baa8"
        },
        new[] {
            "00000000-0000-0000-0000-000000000000",
            "00000000-0000-0000-0000-000000000000",
        })]
    [InlineData(
        OfferTypeId.CORE_COMPONENT,
        UserCompanyId,
        new[] {
            "0ffcb416-1101-4ba6-8d4a-a9dfa31745a4",
            "9b957704-3505-4445-822c-d7ef80f27fcd",
            "9ef01c20-6d9d-41ef-b336-fa64e1e2e4c2",
        },
        new[] {
            "00000000-0000-0000-0000-000000000000",
            "00000000-0000-0000-0000-000000000000",
            "00000000-0000-0000-0000-000000000000",
        })]
    [InlineData(
        OfferTypeId.APP,
        "00000000-0000-0000-0000-000000000000",
        new string[] { },
        new string[] { })]
    public async Task GetProvidedOffersData_ReturnsExpectedResult(OfferTypeId offerTypeId, Guid userCompanyId, IEnumerable<string> offerIds, IEnumerable<string> leadPictureIds)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var providerAppData = await sut.GetProvidedOffersData(offerTypeId, userCompanyId).ToListAsync().ConfigureAwait(false);

        // Assert
        providerAppData.Should().HaveSameCount(offerIds);
        if (offerIds.Any())
        {
            providerAppData.Select(x => x.Id).Should().Contain(offerIds.Select(x => new Guid(x)));
            providerAppData.Join(offerIds.Select(x => new Guid(x)).Zip(leadPictureIds.Select(x => new Guid(x))), data => data.Id, zip => zip.First, (data, zip) => (data, zip)).Should().AllSatisfy(x => x.data.LeadPictureId.Should().Be(x.zip.Second));
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
        new[] { "8895a7b6-39bc-4483-a1de-958e19eb9109", "eac5baeb-65ce-47fd-954e-bf5b4d411ba0", "581aafa7-f43c-40fc-a64c-e7be13e6c861" },                                         // initialKeys
        new[] { "581aafa7-f43c-40fc-a64c-e7be13e6c861", "09986f28-a8be-4df7-a61b-2a1e9c243b74", "f5e5cc9a-eb76-4d72-bd0f-09af6dcd7190", "a69f819b-9d27-43c6-9ca0-fe37f11cfbdc" }, // updateKeys
        new[] { "09986f28-a8be-4df7-a61b-2a1e9c243b74", "f5e5cc9a-eb76-4d72-bd0f-09af6dcd7190", "a69f819b-9d27-43c6-9ca0-fe37f11cfbdc" },                                         // addedEntityKeys
        new[] { "8895a7b6-39bc-4483-a1de-958e19eb9109", "eac5baeb-65ce-47fd-954e-bf5b4d411ba0" }                                                                                  // removedEntityKeys
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
        var result = await sut.GetProviderCompanyUserIdForOfferUntrackedAsync(new Guid("0ffcb416-1101-4ba6-8d4a-a9dfa31745a4"), _userCompanyId, OfferStatusId.CREATED, OfferTypeId.CORE_COMPONENT);

        // Assert
        result.Should().NotBeNull();
        result.IsStatusMatching.Should().BeFalse();
    }

    #endregion

    #region GetInReviewAppData

    [Fact]
    public async Task GetInReviewAppDataByIdAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var InReviewAppofferDetail = await sut.GetInReviewAppDataByIdAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA6"), OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        InReviewAppofferDetail.Should().NotBeNull();
        InReviewAppofferDetail!.title.Should().Be("Latest Service");
        InReviewAppofferDetail!.Documents.Should().NotBeNull();
        var documenttypeId = InReviewAppofferDetail!.Documents.Select(x => x.DocumentTypeId);
        documenttypeId.Should().NotContain(DocumentTypeId.APP_LEADIMAGE);
        documenttypeId.Should().NotContain(DocumentTypeId.APP_IMAGE);
        InReviewAppofferDetail.OfferStatusId.Should().Be(OfferStatusId.IN_REVIEW);
    }

    [Fact]
    public async Task GetInReviewAppDataByIdAsync_InCorrectOfferStatus_ReturnsNullt()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var InReviewAppofferDetail = await sut.GetInReviewAppDataByIdAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA7"), OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        InReviewAppofferDetail.Should().BeNull();
    }

    #endregion

    #region OfferDescription

    [Fact]
    public async Task GetActiveOfferDescriptionDataByIdAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        //Act
        var result = await sut.GetActiveOfferDescriptionDataByIdAsync(new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007"), OfferTypeId.APP, _userCompanyId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.IsStatusActive.Should().BeTrue();
        result.IsProviderCompanyUser.Should().BeTrue();
        result.OfferDescriptionDatas.Should().NotBeNull();
        result.OfferDescriptionDatas!.Select(od => od.LanguageCode).Should().Contain("en");
    }

    [Fact]
    public async Task GetActiveOfferDescriptionDataByIdAsync_InvalidUser_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        //Act
        var result = await sut.GetActiveOfferDescriptionDataByIdAsync(new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007"), OfferTypeId.APP, Guid.NewGuid()).ConfigureAwait(false);

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
        var existingOfferDescription = new[] {
            new LocalizedDescription("en", "some long Description for testing","some short Description for testing"),
            new LocalizedDescription("de", "some long Description for testing","some short Description for testing")
        };
        var modifedOfferDescription = new[] {
            ("en", "some long Description in english, for testing","some short Description in english for testing"),
            ("de", "some long Description in germen, for testing","some short Description in germen for testing")
        };

        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        //Act
        sut.CreateUpdateDeleteOfferDescriptions(appId, existingOfferDescription, modifedOfferDescription);

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
        var existingOfferDescription = new[] {
            new LocalizedDescription("en", "some long Description for testing","some short Description for testing"),
            new LocalizedDescription("de", "some long Description for testing","some short Description for testing")
        };
        var modifedOfferDescription = new[] {
            ("en", "some long Description for testing","some short Description for testing"),
            ("de", "some long Description for testing","some short Description for testing"),
            newOfferDescriptionEntry
        };

        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        //Act
        sut.CreateUpdateDeleteOfferDescriptions(appId, existingOfferDescription, modifedOfferDescription);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Should().AllSatisfy(entry => entry.State.Should().Be(EntityState.Added));
        changedEntries.Select(x => x.Entity).Should().AllBeOfType<OfferDescription>();
        changedEntries.Select(x => x.Entity).Cast<OfferDescription>().Select(x => (x.LanguageShortName, x.DescriptionLong, x.DescriptionShort)).Should().Contain(new[] { newOfferDescriptionEntry });
    }

    [Fact]
    public async Task CreateUpdateDeleteOfferDescriptions_Deleted_ReturnsExpectedResult()
    {
        // Arrange
        var appId = new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4");
        var existingOfferDescriptions = new[] {
            new LocalizedDescription("en", "some long Description for testing","some short Description for testing"),
            new LocalizedDescription("de", "some long Description for testing","some short Description for testing")
        };
        var modifedOfferDescriptions = new[] { ("de", "modified long Description for testing", "modified short Description for testing") };

        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        //Act
        sut.CreateUpdateDeleteOfferDescriptions(appId, existingOfferDescriptions, modifedOfferDescriptions);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(2);
        changedEntries.Select(x => x.Entity).Should().AllBeOfType<OfferDescription>();
        var deletedEntities = changedEntries.Where(x => x.State == EntityState.Deleted).Select(x => (OfferDescription)x.Entity);
        deletedEntities.Should().HaveCount(1);
        deletedEntities.Select(x => x.LanguageShortName).Should().Contain(new[] { "en" });
        var modifiedEntities = changedEntries.Where(x => x.State == EntityState.Modified).Select(x => (OfferDescription)x.Entity);
        modifiedEntities.Should().HaveCount(1);
        modifiedEntities.Select(x => (x.LanguageShortName, x.DescriptionLong, x.DescriptionShort)).Should().Contain(modifedOfferDescriptions);

    }

    #endregion

    #region GetOfferReleaseDataById

    [Theory]
    [InlineData("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA9", "Test App", "Catena-X", "2dc4249f-b5ca-4d42-bef1-7a7a950a4f87", false, false)]
    [InlineData("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA7", "Latest App", "Catena-X", "2dc4249f-b5ca-4d42-bef1-7a7a950a4f87", true, true)]
    public async Task GetOfferReleaseDataByIdAsync_ReturnsExpected(Guid offerId, string name, string companyName, Guid companyId, bool hasTechnicalUserProfiles, bool hasPrivacyPolicies)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOfferReleaseDataByIdAsync(offerId, OfferTypeId.APP).ConfigureAwait(false);

        // Assert

        result.Should().NotBeNull();
        result!.Name.Should().Be(name);
        result!.CompanyName.Should().Be(companyName);
        result!.ProviderCompanyId.Should().Be(companyId);
        result.HasPrivacyPolicies.Should().Be(hasPrivacyPolicies);
        result.HasTechnicalUserProfiles.Should().Be(hasTechnicalUserProfiles);
    }

    #endregion

    #region RemoveOfferAssignedLicenses

    [Fact]
    public async Task RemoveOfferAssignedLicenses_WithExisting_OfferAssignedLicense()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        IEnumerable<(Guid, Guid)> offerAssignedLicenseData = new[] { (new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new Guid("6ca00fc6-4c82-47d8-8616-059ebe65232b")) };
        // Act
        sut.RemoveOfferAssignedLicenses(offerAssignedLicenseData);

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Deleted);
    }

    [Fact]
    public async Task RemoveOfferAssignedUseCases_WithExisting_OfferAssignedUseCase()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        IEnumerable<(Guid, Guid)> offerAssignedUseCaseData = new[] { (new Guid("5cf74ef8-e0b7-4984-a872-474828beb510"), new Guid("6909ccc7-37c8-4088-99ab-790f20702460")) };
        // Act
        sut.RemoveOfferAssignedUseCases(offerAssignedUseCaseData);

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Deleted);
    }

    [Fact]
    public async Task RemoveOfferAssignedPrivacyPolicies_WithExisting_OfferAssignedPrivacyPolicy()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        IEnumerable<(Guid, PrivacyPolicyId)> offerAssignedPrivacyPolicyData = new[] { (new Guid("a9112853-75ac-4967-9844-7478536e5111"), PrivacyPolicyId.COMPANY_DATA) };
        // Act
        sut.RemoveOfferAssignedPrivacyPolicies(offerAssignedPrivacyPolicyData);

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Deleted);
    }

    [Fact]
    public async Task RemoveOfferAssignedDocuments_WithExisting_OfferAssignedDocument()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        IEnumerable<(Guid, Guid)> offerAssignedDocumentsData = new[] { (new Guid("ac1cf001-7fbc-1f2f-817f-bce0574c000f"), new Guid("0e062e49-54ab-47a3-a217-d3f09fbe0459")) };
        // Act
        sut.RemoveOfferAssignedDocuments(offerAssignedDocumentsData);

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Deleted);
    }

    [Fact]
    public async Task RemoveOfferTags_WithExisting_OfferTag()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        IEnumerable<(Guid, string)> offerTagsData = new[] { (new Guid("5cf74ef8-e0b7-4984-a872-474828beb5d1"), "Traceability") };
        // Act
        sut.RemoveOfferTags(offerTagsData);

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Deleted);
    }

    [Fact]
    public async Task RemoveOfferDescriptions_WithExisting_OfferDescription()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        IEnumerable<(Guid, string)> offerDescriptionLanguages = new[] { (new Guid("5cf74ef8-e0b7-4984-a872-474828beb5d2"), "de") };
        // Act
        sut.RemoveOfferDescriptions(offerDescriptionLanguages);

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Deleted);
    }

    [Fact]
    public async Task RemoveOffer_WithExisting_Offer()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        sut.RemoveOffer(new Guid("5cf74ef8-e0b7-4984-a872-474828beb5d2"));

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Deleted);
    }

    [Theory]
    [InlineData("ac1cf001-7fbc-1f2f-817f-bce0572c0007", OfferTypeId.APP, UserCompanyId, OfferStatusId.ACTIVE, true, true, true, true, true)]
    [InlineData("deadbeef-dead-beef-dead-beefdeadbeef", OfferTypeId.APP, UserCompanyId, OfferStatusId.ACTIVE, false, false, false, false, false)]
    [InlineData("ac1cf001-7fbc-1f2f-817f-bce0572c0007", OfferTypeId.SERVICE, UserCompanyId, OfferStatusId.ACTIVE, true, false, true, true, false)]
    [InlineData("ac1cf001-7fbc-1f2f-817f-bce0572c0007", OfferTypeId.APP, "00000000-0000-0000-0000-000000000000", OfferStatusId.ACTIVE, true, true, true, false, false)]
    [InlineData("ac1cf001-7fbc-1f2f-817f-bce0572c0007", OfferTypeId.APP, UserCompanyId, OfferStatusId.CREATED, true, true, false, true, false)]
    public async Task GetAppUntrackedAsync_ReturnsExpectedResult(Guid offerId, OfferTypeId offerTypeId, Guid userCompanyId, OfferStatusId offerStatusId, bool isValidApp, bool isOfferType, bool isOfferStatus, bool isCompanyUser, bool hasData)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act 
        var result = await sut.GetAppDeleteDataAsync(offerId, offerTypeId, userCompanyId, offerStatusId).ConfigureAwait(false);

        // Assert
        result.IsValidApp.Should().Be(isValidApp);
        result.IsOfferType.Should().Be(isOfferType);
        result.IsOfferStatus.Should().Be(isOfferStatus);
        result.IsProviderCompanyUser.Should().Be(isCompanyUser);
        if (hasData)
        {
            result.DeleteData.Should().NotBeNull();
            result.DeleteData!.DocumentIdStatus.Should().NotBeEmpty();
            result.DeleteData.OfferLicenseIds.Should().NotBeEmpty();
            result.DeleteData.UseCaseIds.Should().NotBeEmpty();
            result.DeleteData.PolicyIds.Should().BeEmpty();
            result.DeleteData.LanguageCodes.Should().NotBeEmpty();
            result.DeleteData.TagNames.Should().NotBeEmpty();
            result.DeleteData.DescriptionLanguageShortNames.Should().NotBeEmpty();
        }
        else
        {
            result.DeleteData.Should().BeNull();
        }
    }

    #endregion

    #region GetInsertActiveAppUserRoleDataAsync

    [Fact]
    public async Task GetInsertActiveAppUserRoleDataAsync_WithValidUserAndApp_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetInsertActiveAppUserRoleDataAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA7"), OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.OfferExists.Should().BeTrue();
        result.AppName.Should().Be("Latest App");
        result.ProviderCompanyId.Should().Be(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));
        result.ClientClientIds.Should().BeEmpty();
    }

    [Fact]
    public async Task GetInsertActiveAppUserRoleDataAsync_WithNotExistingApp_ReturnsDefault()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetInsertActiveAppUserRoleDataAsync(Guid.NewGuid(), OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    [Fact]
    public async Task GetInsertActiveAppUserRoleDataAsync_WithNotExistingUser_CompanyUserIdIsNull()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetInsertActiveAppUserRoleDataAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA7"), OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.OfferExists.Should().BeTrue();
        result.AppName.Should().Be("Latest App");
        result.ProviderCompanyId.Should().Be(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));
        result.ClientClientIds.Should().BeEmpty();
    }

    [Fact]
    public async Task GetInsertActiveAppUserRoleDataAsync_WithWrongOfferType_ReturnsDefault()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetInsertActiveAppUserRoleDataAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA7"), OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    #endregion

    #region GetOfferAssignedAppLeadImageDocumentById

    [Fact]
    public async Task GetOfferAssignedAppLeadImageDocumentsByIdAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOfferAssignedAppLeadImageDocumentsByIdAsync(new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007"), _userCompanyId, OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsUserOfProvider.Should().BeTrue();
        result.IsStatusActive.Should().BeTrue();
        var documentId = result.documentStatusDatas.Select(x => x.DocumentId);
        var documentStatus = result.documentStatusDatas.Select(x => x.StatusId);
        documentId.Should().Contain(new Guid("e020787d-1e04-4c0b-9c06-bd1cd44724b1"));
        documentStatus.Should().Contain(DocumentStatusId.LOCKED);
    }

    #endregion

    #region GetServiceDetailsByIdAsync

    [Fact]
    public async Task GetServiceDetailsByIdAsync_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetServiceDetailsByIdAsync(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfea")).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("SDE with EDC");
        result.Provider.Should().Be("Service Provider");
        result.OfferStatusId.Should().Be(OfferStatusId.ACTIVE);
    }
    [Fact]
    public async Task GetServiceDetailsByIdAsync_InCorrectOfferStatus_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetServiceDetailsByIdAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA8")).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllInReviewStatusServiceAsync

    [Theory]
    [InlineData(new[] { OfferStatusId.ACTIVE }, OfferSorting.NameDesc, null, "en")]
    [InlineData(new[] { OfferStatusId.ACTIVE }, OfferSorting.NameAsc, null, "en")]
    public async Task GetAllInReviewStatusServiceAsync_ReturnsExpectedResult(IEnumerable<OfferStatusId> statusids, OfferSorting? sorting, string? serviceName, string languagename)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetAllInReviewStatusServiceAsync(statusids, OfferTypeId.SERVICE, sorting, serviceName, languagename, Constants.DefaultLanguage)(0, 10).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().NotBeEmpty();
        result!.Data.Should().HaveCount(4);
        if (sorting == OfferSorting.NameAsc)
        {
            result.Data.Select(data => data.Provider).Should().BeInAscendingOrder();
        }
        if (sorting == OfferSorting.NameDesc)
        {
            result.Data.Select(data => data.Provider).Should().BeInDescendingOrder();
        }
    }

    [Fact]
    public async Task GetAllInReviewStatusServiceAsync_ReturnsExpectedResult_WithDateSorting()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetAllInReviewStatusServiceAsync(new[] { OfferStatusId.ACTIVE }, OfferTypeId.SERVICE, OfferSorting.NameAsc, null, "en", Constants.DefaultLanguage)(0, 10).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().NotBeEmpty()
            .And.HaveCount(4)
            .And.StartWith(new InReviewServiceData(new Guid("ac1cf001-7fbc-1f2f-817f-bce0000c0001"), "Consulting Service - Data Readiness", OfferStatusId.ACTIVE, "Catena-X", "Lorem ipsum dolor sit amet, consectetur adipiscing elit."));
    }

    #endregion

    #region GetOfferStatusDataByIdAsync

    [Fact]
    public async Task GetOfferStatusDataByIdAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOfferStatusDataByIdAsync(new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007"), OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsStatusInReview.Should().BeFalse();
        result.OfferName.Should().Be("Trace-X");
        result.ProviderCompanyId.Should().Be(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));
        result.IsSingleInstance.Should().BeTrue();
    }

    #endregion

    #region GetOfferWithSetupDataById

    [Fact]
    public async Task GetOfferWithSetupDataById_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOfferWithSetupDataById(new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007"), _userCompanyId, OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.OfferStatus.Should().Be(OfferStatusId.ACTIVE);
        result.IsUserOfProvidingCompany.Should().BeTrue();
        result.SetupTransferData.Should().NotBeNull();
        result.SetupTransferData!.IsSingleInstance.Should().BeTrue();
    }

    #endregion

    #region AttachAndModifyAppInstanceSetup

    [Fact]
    public async Task AttachAndModifyAppInstanceSetup_WithAppInstanceSetup_UpdatesStatus()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        sut.AttachAndModifyAppInstanceSetup(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), data =>
        {
            data.InstanceUrl = "https://newtest.de";
        });

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Modified);
        changedEntity.Entity.Should().BeOfType<AppInstanceSetup>().Which.InstanceUrl.Should().Be("https://newtest.de");
    }

    #endregion

    #region CreateAppInstanceSetup

    [Fact]
    public async Task CreateAppInstanceSetup_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        var results = sut.CreateAppInstanceSetup(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), entity =>
        {
            entity.IsSingleInstance = true;
            entity.InstanceUrl = "https://www.test.de";
        });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        results.InstanceUrl.Should().Be("https://www.test.de");
        results.IsSingleInstance.Should().BeTrue();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<AppInstanceSetup>().Which.InstanceUrl.Should().Be("https://www.test.de");
    }

    #endregion

    #region GetSingleInstanceOfferData

    [Fact]
    public async Task GetSingleInstanceOfferData_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetSingleInstanceOfferData(new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007"), OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Instances.Should().HaveCount(1).And.ContainSingle(x => x.InstanceId == new Guid("b161d570-f6ff-45b4-a077-243f72487af6"));
        result.OfferName.Should().Be("Trace-X");
    }

    #endregion

    #region GetOfferActiveStatusDataById

    [Theory]
    [InlineData("ac1cf001-7fbc-1f2f-817f-bce0572c0007", OfferTypeId.APP, true, true)]
    [InlineData("ac1cf001-7fbc-1f2f-817f-bce0000c0001", OfferTypeId.SERVICE, true, true)]
    [InlineData("ac1cf001-7fbc-1f2f-817f-bce0572c0008", OfferTypeId.APP, false, false)]
    [InlineData("ac1cf001-7fbc-1f2f-817f-bce0572c0009", OfferTypeId.SERVICE, false, false)]
    public async Task GetOfferActiveStatusDataByIdAsync_ReturnsExpectedResult(Guid offerId, OfferTypeId offerTypeId, bool isStatusActive, bool isUserCompanyProvider)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOfferActiveStatusDataByIdAsync(offerId, offerTypeId, new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87")).ConfigureAwait(false);
        // Assert
        result.Should().NotBeNull();
        result.IsStatusActive.Should().Be(isStatusActive);
        result.IsUserCompanyProvider.Should().Be(isUserCompanyProvider);

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
