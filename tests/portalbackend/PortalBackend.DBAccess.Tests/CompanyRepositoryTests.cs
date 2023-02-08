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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using System.Collections.Immutable;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="CompanyRepository"/>
/// </summary>
public class CompanyRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private const string IamUserId = "3d8142f1-860b-48aa-8c2b-1ccb18699f65";
    private readonly Guid _validCompanyId = new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");
    private readonly Guid _validDetailId = new("ee8b4b4a-056e-4f0b-bc2a-cc1adbedf122");
    
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
        var results = sut.CreateProviderCompanyDetail(_validCompanyId, url);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        results.CompanyId.Should().Be(_validCompanyId);
        results.AutoSetupUrl.Should().Be(url);
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<ProviderCompanyDetail>().Which.AutoSetupUrl.Should().Be(url);
    }

    #endregion
    
    #region Create ServiceProviderCompanyDetail

    [Fact]
    public async Task GetServiceProviderCompanyDetailAsync_WithExistingUser_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderCompanyDetailAsync(CompanyRoleId.SERVICE_PROVIDER, "623770c5-cf38-4b9f-9a35-f8b9ae972e2d").ConfigureAwait(false);
        
        // Assert
        result.Should().NotBe(default);
        result.ProviderDetailReturnData.Should().NotBeNull();
        result.ProviderDetailReturnData.CompanyId.Should().Be(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f88"));
        result.IsProviderCompany.Should().BeTrue();
    }

    [Fact]
    public async Task GetServiceProviderCompanyDetailAsync_WithNotExistingDetails_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderCompanyDetailAsync(CompanyRoleId.SERVICE_PROVIDER, Guid.NewGuid().ToString()).ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    [Fact]
    public async Task GetServiceProviderCompanyDetailAsync_WithExistingUserAndNotProvider_ReturnsIsCompanyUserFalse()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderCompanyDetailAsync(CompanyRoleId.OPERATOR, IamUserId).ConfigureAwait(false);

        // Assert
        result.Should().NotBe(default);
        result.IsProviderCompany.Should().BeFalse();
    }

    #endregion

    #region Check Company is ServiceProvider and exists for IamUser

    [Fact]
    public async Task CheckCompanyIsServiceProviderAndExistsForIamUser_WithValidData_ReturnsTrue()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetCompanyIdMatchingRoleAndIamUserOrTechnicalUserAsync("623770c5-cf38-4b9f-9a35-f8b9ae972e2d", CompanyRoleId.SERVICE_PROVIDER);

        // Assert
        results.Should().NotBe(default);
        results.CompanyId.Should().NotBe(Guid.Empty);
        results.IsServiceProviderCompany.Should().BeTrue();
    }
    
    [Fact]
    public async Task CheckCompanyIsServiceProviderAndExistsForIamUser_WithNonServiceProviderCompany_ReturnsFalse()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetCompanyIdMatchingRoleAndIamUserOrTechnicalUserAsync("ad56702b-5908-44eb-a668-9a11a0e100d6", CompanyRoleId.SERVICE_PROVIDER);

        // Assert
        results.Should().NotBe(default);
        results.CompanyId.Should().NotBe(Guid.Empty);
        results.IsServiceProviderCompany.Should().BeFalse();
    }

    #endregion
    
    #region GetCompanyIdByBpn
    
    [Fact]
    public async Task GetCompanyIdByBpn_WithValidData_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetCompanyIdByBpnAsync("BPNL00000003CRHK").ConfigureAwait(false);

        // Assert
        results.Should().NotBe(Guid.Empty);
        results.Should().Be("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");
    }
     
    [Fact]
    public async Task GetCompanyIdByBpn_WithNotExistingBpn_ReturnsEmptyGuid()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetCompanyIdByBpnAsync("NOTEXISTING").ConfigureAwait(false);

        // Assert
        results.Should().Be(Guid.Empty);
    }

    #endregion

    #region GetCompanyBpnById
    
    [Fact]
    public async Task GetCompanyBpnByIdAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetCompanyBpnByIdAsync(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87")).ConfigureAwait(false);

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().Be("BPNL00000003CRHK");
    }
     
    [Fact]
    public async Task GetCompanyBpnByIdAsync_WithNotExistingId_ReturnsEmpty()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetCompanyBpnByIdAsync(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        results.Should().BeNull();
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

    #region AttachAndModifyServiceProviderDetails

    [Fact]
    public async Task CheckServiceProviderDetailsExistsForUser_WithValidIamUser_ReturnsDetailId()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderCompanyDetailsExistsForUser("8be5ee49-4b9c-4008-b641-138305430cc4").ConfigureAwait(false);

        // Assert
        result.Should().NotBe(default);
    }

    [Fact]
    public async Task CheckServiceProviderDetailsExistsForUser_WithNotExistingIamUser_ReturnsEmpty()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderCompanyDetailsExistsForUser(Guid.NewGuid().ToString()).ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    #endregion

    #region CreateUpdateDeleteIdentifiers

    [Theory]
    [InlineData(
        new [] { UniqueIdentifierId.COMMERCIAL_REG_NUMBER, UniqueIdentifierId.EORI, UniqueIdentifierId.LEI_CODE, UniqueIdentifierId.VAT_ID }, // initialKeys
        new [] { UniqueIdentifierId.COMMERCIAL_REG_NUMBER, UniqueIdentifierId.EORI, UniqueIdentifierId.LEI_CODE, UniqueIdentifierId.VIES },   // updateKeys
        new [] { "value-1", "value-2", "value-3", "value-4" },                                                                                // initialValues
        new [] { "value-1", "changed-1", "changed-2", "added-1" },                                                                            // updateValues
        new [] { UniqueIdentifierId.VIES },                                                                                                   // addedEntityKeys
        new [] { "added-1" },                                                                                                                 // addedEntityValues
        new [] { UniqueIdentifierId.EORI, UniqueIdentifierId.LEI_CODE },                                                                      // updatedEntityKeys
        new [] { "changed-1", "changed-2" },                                                                                                  // updatedEntityValues
        new [] { UniqueIdentifierId.VAT_ID }                                                                                                  // removedEntityKeys
    )]
    [InlineData(
        new [] { UniqueIdentifierId.EORI, UniqueIdentifierId.LEI_CODE, UniqueIdentifierId.VAT_ID },                                           // initialKeys
        new [] { UniqueIdentifierId.COMMERCIAL_REG_NUMBER, UniqueIdentifierId.EORI, UniqueIdentifierId.VIES },                                // updateKeys
        new [] { "value-1", "value-2", "value-3" },                                                                                           // initialValues
        new [] { "added-1", "changed-1", "added-2" },                                                                                         // updateValues
        new [] { UniqueIdentifierId.COMMERCIAL_REG_NUMBER, UniqueIdentifierId.VIES },                                                         // addedEntityKeys
        new [] { "added-1", "added-2"},                                                                                                       // addedEntityValues
        new [] { UniqueIdentifierId.EORI },                                                                                                   // updatedEntityKeys
        new [] { "changed-1" },                                                                                                               // updatedEntityValues
        new [] { UniqueIdentifierId.LEI_CODE, UniqueIdentifierId.VAT_ID }                                                                     // removedEntityKeys
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
        deleted.OrderBy(x => x.UniqueIdentifierId).Zip(removedEntities).Should().AllSatisfy(x => (x.First.UniqueIdentifierId == x.Second.UniqueIdentifierId && x.First.Value == x.Second.Value).Should().BeTrue());
   }

    #endregion

    #region GetOwnCompanyDetailsAsync

    [Fact]
    public async Task GetOwnCompanyDetailsAsync_ReturnsExpected()
    {
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        var result = await sut.GetOwnCompanyDetailsAsync("502dabcf-01c7-47d9-a88e-0be4279097b5").ConfigureAwait(false);

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
    }

    #endregion

    #region Setup
    
    private async Task<(CompanyRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new CompanyRepository(context);
        return (sut, context);
    }

    #endregion
}
