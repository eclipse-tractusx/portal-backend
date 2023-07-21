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
/// Tests the functionality of the <see cref="CompanyRepository"/>
/// </summary>
public class CompanyRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _validCompanyId = new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");

    public CompanyRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region Create ServiceProviderCompanyDetail

    [Fact]
    public async Task CreateServiceProviderCompanyDetail_ReturnsExpectedResult()
    {
        // Arrange
        const string url = "https://service-url.com";
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = sut.CreateProviderCompanyDetail(_validCompanyId, url, entity =>
        {
            entity.AutoSetupCallbackUrl = "https://test.de";
        });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        results.CompanyId.Should().Be(_validCompanyId);
        results.AutoSetupUrl.Should().Be(url);
        results.AutoSetupCallbackUrl.Should().Be("https://test.de");
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<ProviderCompanyDetail>();
        var providerCompanyDetail = changedEntries.Single().Entity as ProviderCompanyDetail;
        providerCompanyDetail!.AutoSetupUrl.Should().Be(url);
        providerCompanyDetail.AutoSetupCallbackUrl.Should().Be("https://test.de");
    }

    #endregion

    #region Create ServiceProviderCompanyDetail

    [Fact]
    public async Task GetServiceProviderCompanyDetailAsync_WithExistingUser_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderCompanyDetailAsync(CompanyRoleId.SERVICE_PROVIDER, new Guid("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd")).ConfigureAwait(false);

        // Assert
        result.Should().NotBe(default);
        result.ProviderDetailReturnData.Should().NotBeNull();
        result.ProviderDetailReturnData.CompanyId.Should().Be(new Guid("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd"));
        result.IsProviderCompany.Should().BeTrue();
    }

    [Fact]
    public async Task GetServiceProviderCompanyDetailAsync_WithNotExistingDetails_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderCompanyDetailAsync(CompanyRoleId.SERVICE_PROVIDER, Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    [Fact]
    public async Task GetServiceProviderCompanyDetailAsync_WithExistingUserAndNotProvider_ReturnsIsCompanyUserFalse()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderCompanyDetailAsync(CompanyRoleId.OPERATOR, new("ac861325-bc54-4583-bcdc-9e9f2a38ff84")).ConfigureAwait(false);

        // Assert
        result.Should().NotBe(default);
        result.IsProviderCompany.Should().BeFalse();
    }

    #endregion

    #region IsValidCompanyRoleOwner

    [Theory]
    [InlineData("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", new[] { CompanyRoleId.SERVICE_PROVIDER }, true, true)]
    [InlineData("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", new[] { CompanyRoleId.SERVICE_PROVIDER, CompanyRoleId.OPERATOR }, true, true)]
    [InlineData("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", new[] { CompanyRoleId.OPERATOR }, true, false)]
    [InlineData("deadbeef-dead-beef-dead-beefdeadbeef", new[] { CompanyRoleId.SERVICE_PROVIDER }, false, false)]
    public async Task IsValidCompanyRoleOwner_ReturnsExpected(Guid companyId, IEnumerable<CompanyRoleId> companyRoleIds, bool isValidCompany, bool isCompanyRoleOwner)
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.IsValidCompanyRoleOwner(companyId, companyRoleIds);

        // Assert
        results.IsValidCompanyId.Should().Be(isValidCompany);
        results.IsCompanyRoleOwner.Should().Be(isCompanyRoleOwner);
    }

    #endregion

    #region GetCompanyBpnAndSelfDescriptionDocumentByIdAsync

    [Fact]
    public async Task GetCompanyBpnByIdAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetCompanyBpnAndSelfDescriptionDocumentByIdAsync(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87")).ConfigureAwait(false);

        // Assert
        results.Should().NotBe(default);
        results.Bpn.Should().NotBeNullOrEmpty();
        results.Bpn.Should().Be("BPNL00000003CRHK");
    }

    [Fact]
    public async Task GetCompanyBpnByIdAsync_WithNotExistingId_ReturnsEmpty()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetCompanyBpnAndSelfDescriptionDocumentByIdAsync(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        results.Should().Be(default);
    }

    #endregion

    #region AttachAndModifyServiceProviderDetails

    [Fact]
    public async Task AttachAndModifyServiceProviderDetails_Changed_ReturnsExpectedResult()
    {
        // Arrange
        const string url = "https://service-url.com/new";
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.AttachAndModifyProviderCompanyDetails(new Guid("ee8b4b4a-056e-4f0b-bc2a-cc1adbedf122"),
            detail => { detail.AutoSetupUrl = null!; },
            detail => { detail.AutoSetupUrl = url; });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<ProviderCompanyDetail>().Which.AutoSetupUrl.Should().Be(url);
        var entry = changedEntries.Single();
        entry.Entity.Should().BeOfType<ProviderCompanyDetail>().Which.AutoSetupUrl.Should().Be(url);
        entry.State.Should().Be(Microsoft.EntityFrameworkCore.EntityState.Modified);
    }

    [Fact]
    public async Task AttachAndModifyServiceProviderDetails_Unchanged_ReturnsExpectedResult()
    {
        // Arrange
        const string url = "https://service-url.com/new";
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.AttachAndModifyProviderCompanyDetails(new Guid("ee8b4b4a-056e-4f0b-bc2a-cc1adbedf122"),
            detail => { detail.AutoSetupUrl = url; },
            detail => { detail.AutoSetupUrl = url; });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeFalse();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var entry = changedEntries.Single();
        entry.Entity.Should().BeOfType<ProviderCompanyDetail>().Which.AutoSetupUrl.Should().Be(url);
        entry.State.Should().Be(Microsoft.EntityFrameworkCore.EntityState.Unchanged);
    }

    #endregion

    #region AttachAndModifyAddress

    [Fact]
    public async Task AttachAndModifyAddress_Changed_ReturnsExpectedResult()
    {
        // Arrange
        const string city = "Munich";
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.AttachAndModifyAddress(new Guid("b4db3945-19a7-4a50-97d6-e66e8dfd04fb"),
            address => { address.City = null!; },
            address => { address.City = city; });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<Address>().Which.City.Should().Be(city);
        var entry = changedEntries.Single();
        entry.Entity.Should().BeOfType<Address>().Which.City.Should().Be(city);
        entry.State.Should().Be(Microsoft.EntityFrameworkCore.EntityState.Modified);
    }

    [Fact]
    public async Task AttachAndModifyAddress_Unchanged_ReturnsExpectedResult()
    {
        // Arrange
        const string city = "Munich";
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.AttachAndModifyAddress(new Guid("b4db3945-19a7-4a50-97d6-e66e8dfd04fb"),
            address => { address.City = city; },
            address => { address.City = city; });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeFalse();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var entry = changedEntries.Single();
        entry.Entity.Should().BeOfType<Address>().Which.City.Should().Be(city);
        entry.State.Should().Be(Microsoft.EntityFrameworkCore.EntityState.Unchanged);
    }

    #endregion

    #region AttachAndModifyServiceProviderDetails

    [Fact]
    public async Task CheckServiceProviderDetailsExistsForUser_WithValidIamUser_ReturnsDetailId()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderCompanyDetailsExistsForUser(new("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd")).ConfigureAwait(false);

        // Assert
        result.Should().NotBe(default);
    }

    [Fact]
    public async Task CheckServiceProviderDetailsExistsForUser_WithNotExistingIamUser_ReturnsEmpty()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderCompanyDetailsExistsForUser(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    #endregion

    #region CreateUpdateDeleteIdentifiers

    [Theory]
    [InlineData(
        new[] { UniqueIdentifierId.COMMERCIAL_REG_NUMBER, UniqueIdentifierId.EORI, UniqueIdentifierId.LEI_CODE, UniqueIdentifierId.VAT_ID }, // initialKeys
        new[] { UniqueIdentifierId.COMMERCIAL_REG_NUMBER, UniqueIdentifierId.EORI, UniqueIdentifierId.LEI_CODE, UniqueIdentifierId.VIES },   // updateKeys
        new[] { "value-1", "value-2", "value-3", "value-4" },                                                                                // initialValues
        new[] { "value-1", "changed-1", "changed-2", "added-1" },                                                                            // updateValues
        new[] { UniqueIdentifierId.VIES },                                                                                                   // addedEntityKeys
        new[] { "added-1" },                                                                                                                 // addedEntityValues
        new[] { UniqueIdentifierId.EORI, UniqueIdentifierId.LEI_CODE },                                                                      // updatedEntityKeys
        new[] { "changed-1", "changed-2" },                                                                                                  // updatedEntityValues
        new[] { UniqueIdentifierId.VAT_ID }                                                                                                  // removedEntityKeys
    )]
    [InlineData(
        new[] { UniqueIdentifierId.EORI, UniqueIdentifierId.LEI_CODE, UniqueIdentifierId.VAT_ID },                                           // initialKeys
        new[] { UniqueIdentifierId.COMMERCIAL_REG_NUMBER, UniqueIdentifierId.EORI, UniqueIdentifierId.VIES },                                // updateKeys
        new[] { "value-1", "value-2", "value-3" },                                                                                           // initialValues
        new[] { "added-1", "changed-1", "added-2" },                                                                                         // updateValues
        new[] { UniqueIdentifierId.COMMERCIAL_REG_NUMBER, UniqueIdentifierId.VIES },                                                         // addedEntityKeys
        new[] { "added-1", "added-2" },                                                                                                       // addedEntityValues
        new[] { UniqueIdentifierId.EORI },                                                                                                   // updatedEntityKeys
        new[] { "changed-1" },                                                                                                               // updatedEntityValues
        new[] { UniqueIdentifierId.LEI_CODE, UniqueIdentifierId.VAT_ID }                                                                     // removedEntityKeys
    )]

    public async Task CreateUpdateDeleteIdentifiers(
        IEnumerable<UniqueIdentifierId> initialKeys, IEnumerable<UniqueIdentifierId> updateKeys,
        IEnumerable<string> initialValues, IEnumerable<string> updateValues,
        IEnumerable<UniqueIdentifierId> addedEntityKeys, IEnumerable<string> addedEntityValues,
        IEnumerable<UniqueIdentifierId> updatedEntityKeys, IEnumerable<string> updatedEntityValues,
        IEnumerable<UniqueIdentifierId> removedEntityKeys)
    {
        var companyId = Guid.NewGuid();
        var initialItems = initialKeys.Zip(initialValues).Select(x => ((UniqueIdentifierId InitialKey, string InitialValue))(x.First, x.Second)).ToImmutableArray();
        var updateItems = updateKeys.Zip(updateValues).Select(x => ((UniqueIdentifierId UpdateKey, string UpdateValue))(x.First, x.Second)).ToImmutableArray();
        var addedEntities = addedEntityKeys.Zip(addedEntityValues).Select(x => new CompanyIdentifier(companyId, x.First, x.Second)).OrderBy(x => x.UniqueIdentifierId).ToImmutableArray();
        var updatedEntities = updatedEntityKeys.Zip(updatedEntityValues).Select(x => new CompanyIdentifier(companyId, x.First, x.Second)).OrderBy(x => x.UniqueIdentifierId).ToImmutableArray();
        var removedEntities = removedEntityKeys.Select(x => new CompanyIdentifier(companyId, x, null!)).OrderBy(x => x.UniqueIdentifierId).ToImmutableArray();

        var (sut, context) = await CreateSut().ConfigureAwait(false);

        sut.CreateUpdateDeleteIdentifiers(companyId, initialItems, updateItems);

        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().AllSatisfy(entry => entry.Entity.Should().BeOfType<CompanyIdentifier>());
        changedEntries.Should().HaveCount(addedEntities.Length + updatedEntities.Length + removedEntities.Length);
        var added = changedEntries.Where(entry => entry.State == Microsoft.EntityFrameworkCore.EntityState.Added).Select(x => (CompanyIdentifier)x.Entity).ToImmutableArray();
        var modified = changedEntries.Where(entry => entry.State == Microsoft.EntityFrameworkCore.EntityState.Modified).Select(x => (CompanyIdentifier)x.Entity).ToImmutableArray();
        var deleted = changedEntries.Where(entry => entry.State == Microsoft.EntityFrameworkCore.EntityState.Deleted).Select(x => (CompanyIdentifier)x.Entity).ToImmutableArray();

        added.Should().HaveSameCount(addedEntities);
        added.OrderBy(x => x.UniqueIdentifierId).Zip(addedEntities).Should().AllSatisfy(x => (x.First.UniqueIdentifierId == x.Second.UniqueIdentifierId && x.First.Value == x.Second.Value).Should().BeTrue());
        modified.Should().HaveSameCount(updatedEntities);
        modified.OrderBy(x => x.UniqueIdentifierId).Zip(updatedEntities).Should().AllSatisfy(x => (x.First.UniqueIdentifierId == x.Second.UniqueIdentifierId && x.First.Value == x.Second.Value).Should().BeTrue());
        deleted.Should().HaveSameCount(removedEntities);
        deleted.OrderBy(x => x.UniqueIdentifierId).Zip(removedEntities).Should().AllSatisfy(x => x.First.UniqueIdentifierId.Should().Be(x.Second.UniqueIdentifierId));
    }

    #endregion

    #region GetCompanyDetailsAsync

    [Fact]
    public async Task GetCompanyDetailsAsync_ReturnsExpected()
    {
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        var result = await sut.GetCompanyDetailsAsync(new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87")).ConfigureAwait(false);

        result.Should().NotBeNull();

        result!.CompanyId.Should().Be(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));
        result.Name.Should().Be("Catena-X");
        result.ShortName.Should().Be("Catena-X");
        result.BusinessPartnerNumber.Should().Be("BPNL00000003CRHK");
        result.CountryAlpha2Code.Should().Be("DE");
        result.City.Should().Be("Munich");
        result.StreetName.Should().Be("Street");
        result.StreetNumber.Should().Be("1");
        result.ZipCode.Should().Be("00001");
        result.CompanyRole.Should().Contain(CompanyRoleId.ACTIVE_PARTICIPANT);
    }

    #endregion

    #region CompanyAssignedUseCaseId

    [Fact]
    public async Task GetCompanyAssigendUseCaseDetailsAsync_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = sut.GetCompanyAssigendUseCaseDetailsAsync(new("0dcd8209-85e2-4073-b130-ac094fb47106"));

        // Assert
        result.Should().NotBeNull();
        var data = await result.FirstAsync();
        data!.useCaseId.Should().Be("06b243a4-ba51-4bf3-bc40-5d79a2231b86");
        data.name.Should().Be("Traceability");
    }

    [Fact]
    public async Task GetCompanyStatusAndUseCaseIdAsync_ReturnsExpected()
    {
        // Arrange
        var useCaseId = new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b86");
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetCompanyStatusAndUseCaseIdAsync(new("0dcd8209-85e2-4073-b130-ac094fb47106"), useCaseId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsActiveCompanyStatus.Should().BeFalse();
        result.IsUseCaseIdExists.Should().BeTrue();
        result.IsValidCompany.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCompanyAssignedUseCase_ReturnsExpectedResult()
    {
        // Arrange
        var useCaseId = new Guid("1aacde78-35ec-4df3-ba1e-f988cddcbbd8");
        var companyId = new Guid("0dcd8209-85e2-4073-b130-ac094fb47106");
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.CreateCompanyAssignedUseCase(companyId, useCaseId);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Should().AllSatisfy(entry => entry.State.Should().Be(EntityState.Added));
        changedEntries.Select(x => x.Entity).Should().AllBeOfType<CompanyAssignedUseCase>();
        changedEntries.Select(x => x.Entity).Cast<CompanyAssignedUseCase>().Select(x => (x.CompanyId, x.UseCaseId))
            .Should().Contain((companyId, useCaseId));
    }

    [Fact]
    public async Task RemoveCompanyAssignedUseCase_ReturnsExpectedResult()
    {
        // Arrange
        var useCaseId = new Guid("1aacde78-35ec-4df3-ba1e-f988cddcbbd8");
        var companyId = new Guid("0dcd8209-85e2-4073-b130-ac094fb47106");
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.RemoveCompanyAssignedUseCase(companyId, useCaseId);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Select(x => x.Entity).Should().AllBeOfType<CompanyAssignedUseCase>();
        var deletedEntities = changedEntries.Where(x => x.State == EntityState.Deleted).Select(x => (CompanyAssignedUseCase)x.Entity);
        deletedEntities.Should().HaveCount(1);
        deletedEntities.Select(x => (x.CompanyId, x.UseCaseId)).Should().Contain((companyId, useCaseId));
    }

    #endregion

    #region GetCompanyRoleAndConsentAgreement

    [Fact]
    public async Task GetCompanyRoleAndConsentAgreementDetailsAsync_ReturnsExpected()
    {
        var (sut, context) = await CreateSut().ConfigureAwait(false);
        var companyId = new Guid("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd");
        var activeDescription = "The participant role is covering the data provider, data consumer or app user scenario. As participant you are an active member of the network with enabled services to particiapte as contributer and user.";
        var serviceDscription = "The Service Provider is able to offer 3rd party services, such as dataspace service offerings to CX Members. CX members can subscribe for those services.";
        var appDescription = "The App Provider is a company which is providing application software via the CX marketplace. As app provider you can participate and use the developer hub, release and offer applications to the network and manage your applications.";

        var result = await sut.GetCompanyRoleAndConsentAgreementDataAsync(companyId, Constants.DefaultLanguage).ToListAsync().ConfigureAwait(false);

        result.Should().NotBeNull()
            .And.HaveCount(3)
            .And.Satisfy(
                x => x.CompanyRoleId == CompanyRoleId.ACTIVE_PARTICIPANT && x.RoleDescription == activeDescription && x.CompanyRolesActive == false && x.Agreements.Count() == 3 && x.Agreements.All(agreement => agreement.ConsentStatus == 0),
                x => x.CompanyRoleId == CompanyRoleId.APP_PROVIDER && x.RoleDescription == appDescription && x.CompanyRolesActive == false && x.Agreements.Count() == 1 && x.Agreements.All(agreement => agreement.ConsentStatus == 0),
                x => x.CompanyRoleId == CompanyRoleId.SERVICE_PROVIDER && x.RoleDescription == serviceDscription && x.CompanyRolesActive == true && x.Agreements.Count() == 2
                    && x.Agreements.Any(agr => agr.AgreementId == new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1094") && agr.DocumentId == null && agr.AgreementName == "Terms & Conditions - Consultant" && agr.ConsentStatus == ConsentStatusId.ACTIVE)
                    && x.Agreements.Any(agr => agr.AgreementId == new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1017") && agr.DocumentId == new Guid("00000000-0000-0000-0000-000000000004") && agr.AgreementName == "Terms & Conditions Service Provider" && agr.ConsentStatus == 0));
    }

    #endregion

    #region  GetCompanyRolesData

    [Fact]
    public async Task GetCompanyRolesDataAsync_ReturnsExpected()
    {
        // Arrange
        var companyRoleIds = new[] { CompanyRoleId.SERVICE_PROVIDER, CompanyRoleId.ACTIVE_PARTICIPANT };
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetCompanyRolesDataAsync(new("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd"), companyRoleIds).ConfigureAwait(false);

        // Assert
        result.IsValidCompany.Should().BeTrue();
        result.IsCompanyActive.Should().BeTrue();
        result.CompanyRoleIds.Should().NotBeNull()
            .And.Contain(CompanyRoleId.SERVICE_PROVIDER);
        result.ConsentStatusDetails.Should().NotBeNull()
            .And.Contain(x => x.ConsentStatusId == ConsentStatusId.ACTIVE);
    }

    #endregion

    #region GetAgreementAssignedRolesDataAsync

    [Fact]
    public async Task GetAgreementAssignedRolesDataAsync_ReturnsExpected()
    {
        // Arrange
        var companyRoleIds = new[] { CompanyRoleId.APP_PROVIDER };
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetAgreementAssignedRolesDataAsync(companyRoleIds).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull()
            .And.HaveCount(1)
            .And.Satisfy(
                x => x.AgreementId == new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1011") && x.CompanyRoleId == CompanyRoleId.APP_PROVIDER
            );
    }

    [Fact]
    public async Task GetAgreementAssignedRolesDataAsync_ReturnsExpectedOrder()
    {
        // Arrange
        var companyRoleIds = new[] { CompanyRoleId.APP_PROVIDER, CompanyRoleId.SERVICE_PROVIDER, CompanyRoleId.ACTIVE_PARTICIPANT };
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetAgreementAssignedRolesDataAsync(companyRoleIds).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull()
            .And.HaveCount(6)
            .And.Satisfy(
                x => x.AgreementId == new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1010") && x.CompanyRoleId == CompanyRoleId.ACTIVE_PARTICIPANT,
                x => x.AgreementId == new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1013") && x.CompanyRoleId == CompanyRoleId.ACTIVE_PARTICIPANT,
                x => x.AgreementId == new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1090") && x.CompanyRoleId == CompanyRoleId.ACTIVE_PARTICIPANT,
                x => x.AgreementId == new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1094") && x.CompanyRoleId == CompanyRoleId.SERVICE_PROVIDER,
                x => x.AgreementId == new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1017") && x.CompanyRoleId == CompanyRoleId.SERVICE_PROVIDER,
                x => x.AgreementId == new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1011") && x.CompanyRoleId == CompanyRoleId.APP_PROVIDER
            );
        result.Select(x => x.CompanyRoleId).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task IsCompanyStatusActive()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetCompanyStatusDataAsync(new("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd")).ConfigureAwait(false);

        // Assert
        result.IsValid.Should().BeTrue();
        result.IsActive.Should().BeTrue();
    }

    #endregion

    #region GetBpnAndTechnicalUserRoleIds

    [Fact]
    public async Task GetCompanyIdAndBpnForIamUserUntrackedAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetBpnAndTechnicalUserRoleIds(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), "technical_roles_management").ConfigureAwait(false);

        // Assert
        result.Should().NotBe(default);
        result.Bpn.Should().Be("BPNL00000003CRHK");
        result.TechnicalUserRoleIds.Should().HaveCount(9).And.OnlyHaveUniqueItems();
    }

    #endregion

    #region GetOwnCompanAndCompanyUseryIdWithCompanyNameAndUserEmailAsync

    [Fact]
    public async Task GetOwnCompanAndCompanyUseryIdWithCompanyNameAndUserEmailAsync_WithValidIamUser_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyInformationAsync(new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87")).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.OrganizationName.Should().Be("Catena-X");
    }

    [Fact]
    public async Task GetOwnCompanAndCompanyUseryIdWithCompanyNameAndUserEmailAsync_WithNotExistingIamUser_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyInformationAsync(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetOperatorBpns

    [Fact]
    public async Task GetOperatorBpns_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOperatorBpns().ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().BeOfType<OperatorBpnData>()
            .And.Match<OperatorBpnData>(x => x.Bpn == "BPNL00000003CRHK" && x.OperatorName == "Catena-X");
    }

    #endregion

    #region Setup

    private async Task<(ICompanyRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new CompanyRepository(context);
        return (sut, context);
    }

    #endregion
}
