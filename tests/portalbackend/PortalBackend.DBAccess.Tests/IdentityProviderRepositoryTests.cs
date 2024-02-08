/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class IdentityProviderRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _companyId = new("ac861325-bc54-4583-bcdc-9e9f2a38ff84");

    public IdentityProviderRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region CreateCompanyIdentityProvider

    [Fact]
    public async Task CreateCompanyIdentityProvider_WithValid_ReturnsExpected()
    {
        var identityProviderId = new Guid("38f56465-ce26-4f25-9745-1791620dc198");
        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        var result = sut.CreateCompanyIdentityProvider(_companyId, identityProviderId);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        result.CompanyId.Should().Be(_companyId);
        result.IdentityProviderId.Should().Be(identityProviderId);
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<CompanyIdentityProvider>();
        var idp = changedEntries.Single().Entity as CompanyIdentityProvider;
        idp!.CompanyId.Should().Be(_companyId);
        idp.IdentityProviderId.Should().Be(identityProviderId);
    }

    #endregion

    #region CreateIamIdentityProvider

    [Fact]
    public async Task CreateIamIdentityProvider_WithValid_ReturnsExpected()
    {
        var identityProviderId = new Guid("38f56465-ce26-4f25-9745-1791620dc198");
        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        var result = sut.CreateIamIdentityProvider(identityProviderId, "idp-999");

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        result.IamIdpAlias.Should().Be("idp-999");
        result.IdentityProviderId.Should().Be(identityProviderId);
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<IamIdentityProvider>();
        var idp = changedEntries.Single().Entity as IamIdentityProvider;
        idp!.IamIdpAlias.Should().Be("idp-999");
        idp.IdentityProviderId.Should().Be(identityProviderId);
    }

    #endregion

    #region GetOwnCompanyIdentityProviderAliasUntrackedAsync

    [Theory]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc198", "ac861325-bc54-4583-bcdc-9e9f2a38ff84", "Idp-123", true, IdentityProviderTypeId.MANAGED)]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc198", "3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", "Idp-123", true, IdentityProviderTypeId.MANAGED)]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc199", "ac861325-bc54-4583-bcdc-9e9f2a38ff84", "Test-Alias", false, IdentityProviderTypeId.OWN)]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc199", "2dc4249f-b5ca-4d42-bef1-7a7a950a4f88", "Test-Alias", true, IdentityProviderTypeId.OWN)]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc201", "ac861325-bc54-4583-bcdc-9e9f2a38ff84", "Shared-Alias", true, IdentityProviderTypeId.SHARED)]
    public async Task GetOwnCompanyIdentityProviderAliasUntrackedAsync_WithValid_ReturnsExpected(Guid identityProviderId, Guid companyId, string alias, bool isOwnOrOwner, IdentityProviderTypeId typeId)
    {
        var sut = await CreateSut().ConfigureAwait(false);

        var result = await sut.GetOwnCompanyIdentityProviderAliasUntrackedAsync(identityProviderId, companyId).ConfigureAwait(false);

        // Assert
        result.Alias.Should().Be(alias);
        result.IsOwnOrOwnerCompany.Should().Be(isOwnOrOwner);
        result.TypeId.Should().Be(typeId);
    }

    #endregion

    #region GetOwnCompanyIdentityProviderStatusUpdateData

    [Theory]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc198", "ac861325-bc54-4583-bcdc-9e9f2a38ff84", true, "Idp-123", true, true, "Bayerische Motorenwerke AG", new[] { "3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", "0dcd8209-85e2-4073-b130-ac094fb47106" })]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc198", "3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", true, "Idp-123", false, true, "Bayerische Motorenwerke AG", new[] { "3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", "0dcd8209-85e2-4073-b130-ac094fb47106" })]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc199", "ac861325-bc54-4583-bcdc-9e9f2a38ff84", true, "Test-Alias", false, false, "CX-Test-Access", new[] { "2dc4249f-b5ca-4d42-bef1-7a7a950a4f88" })]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc199", "2dc4249f-b5ca-4d42-bef1-7a7a950a4f88", true, "Test-Alias", true, false, "CX-Test-Access", new[] { "2dc4249f-b5ca-4d42-bef1-7a7a950a4f88" })]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc200", "41fd2ab8-71cd-4546-9bef-a388d91b2542", true, "Test-Alias2", false, false, "Catena-X", new[] { "41fd2ab8-71cd-4546-9bef-a388d91b2542", "41fd2ab8-7123-4546-9bef-a388d91b2999", "3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", "0dcd8209-85e2-4073-b130-ac094fb47106", "2dc4249f-b5ca-4d42-bef1-7a7a950a4f88" })]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc200", "41fd2ab8-71cd-4546-9bef-a388d91b2542", false, "Test-Alias2", false, false, "Catena-X", new[] { "41fd2ab8-71cd-4546-9bef-a388d91b2542", "41fd2ab8-7123-4546-9bef-a388d91b2999", "3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", "0dcd8209-85e2-4073-b130-ac094fb47106", "2dc4249f-b5ca-4d42-bef1-7a7a950a4f88" })]
    public async Task GetOwnCompanyIdentityProviderStatusUpdateData_WithValidOwner_ReturnsExpected(Guid identityProviderId, Guid companyId, bool queryAliase, string alias, bool isOwner, bool companyUsersLinked, string ownerCompanyName, IEnumerable<string>? companyIds)
    {
        var sut = await CreateSut().ConfigureAwait(false);

        var result = await sut.GetOwnCompanyIdentityProviderStatusUpdateData(identityProviderId, companyId, queryAliase).ConfigureAwait(false);

        // Assert
        result.IdentityProviderData.Alias.Should().Be(alias);
        result.IsOwner.Should().Be(isOwner);
        result.CompanyUsersLinked.Should().Be(companyUsersLinked);
        result.IdpOwnerName.Should().Be(ownerCompanyName);
        companyIds.Should().NotBeNull();
        if (queryAliase)
        {
            if (alias == "Test-Alias2")
            {
                result.CompanyIdAliase.Should().HaveCount(5).And.Satisfy(
                    x => x.CompanyId == new Guid("41fd2ab8-71cd-4546-9bef-a388d91b2542") && x.Aliase.SequenceEqual(new[] { "Test-Alias2" }),
                    x => x.CompanyId == new Guid("41fd2ab8-7123-4546-9bef-a388d91b2999") && x.Aliase.SequenceEqual(new[] { "Test-Alias2" }),
                    x => x.CompanyId == new Guid("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd") && x.Aliase.Order().SequenceEqual(new[] { "Idp-123", "Test-Alias2" }),
                    x => x.CompanyId == new Guid("0dcd8209-85e2-4073-b130-ac094fb47106") && x.Aliase.Order().SequenceEqual(new[] { "Idp-123", "Test-Alias2" }),
                    x => x.CompanyId == new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f88") && x.Aliase.Order().SequenceEqual(new[] { "Test-Alias", "Test-Alias2" })
                );
            }
            else
            {
                result.CompanyIdAliase.Should().Match<IEnumerable<(Guid CompanyId, IEnumerable<string> Aliase)>>(cida => cida.Select(x => x.CompanyId).Order().SequenceEqual(companyIds!.Select(i => new Guid(i)).Order()) &&
                    cida.Select(x => x.Aliase).All(a => a.Order().SequenceEqual(new[] { alias, "Test-Alias2" })));
            }
        }
        else
        {
            result.CompanyIdAliase.Should().BeNull();
        }
    }

    #endregion

    #region GetOwnCompanyIdentityProviderUpdateData

    [Theory]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc200", "41fd2ab8-71cd-4546-9bef-a388d91b2542", "Test-Alias2", false)]
    public async Task GetOwnCompanyIdentityProviderUpdateData_WithValidOwner_ReturnsExpected(Guid identityProviderId, Guid companyId, string alias, bool isOwner)
    {
        var sut = await CreateSut().ConfigureAwait(false);

        var result = await sut.GetOwnCompanyIdentityProviderUpdateData(identityProviderId, companyId).ConfigureAwait(false);

        // Assert
        result.Alias.Should().Be(alias);
        result.IsOwner.Should().Be(isOwner);
    }

    #endregion

    #region GetOwnCompanyIdentityProviderUpdateDataForDelete

    [Theory]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc198", "ac861325-bc54-4583-bcdc-9e9f2a38ff84", "Idp-123", true, "Bayerische Motorenwerke AG", new[] { "3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", "0dcd8209-85e2-4073-b130-ac094fb47106" })]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc198", "3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", "Idp-123", false, "Bayerische Motorenwerke AG", new[] { "3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", "0dcd8209-85e2-4073-b130-ac094fb47106" })]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc199", "ac861325-bc54-4583-bcdc-9e9f2a38ff84", "Test-Alias", false, "CX-Test-Access", new[] { "2dc4249f-b5ca-4d42-bef1-7a7a950a4f88" })]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc199", "2dc4249f-b5ca-4d42-bef1-7a7a950a4f88", "Test-Alias", true, "CX-Test-Access", new[] { "2dc4249f-b5ca-4d42-bef1-7a7a950a4f88" })]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc200", "41fd2ab8-71cd-4546-9bef-a388d91b2542", "Test-Alias2", false, "Catena-X", new[] { "41fd2ab8-71cd-4546-9bef-a388d91b2542", "41fd2ab8-7123-4546-9bef-a388d91b2999", "3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", "0dcd8209-85e2-4073-b130-ac094fb47106", "2dc4249f-b5ca-4d42-bef1-7a7a950a4f88" })]
    public async Task GetOwnCompanyIdentityProviderUpdateDataForDelete_WithValidOwner_ReturnsExpected(Guid identityProviderId, Guid companyId, string alias, bool isOwner, string ownerCompanyName, IEnumerable<string>? companyIds)
    {
        var sut = await CreateSut().ConfigureAwait(false);

        var result = await sut.GetOwnCompanyIdentityProviderUpdateDataForDelete(identityProviderId, companyId).ConfigureAwait(false);

        // Assert
        result.Alias.Should().Be(alias);
        result.IsOwner.Should().Be(isOwner);
        result.IdpOwnerName.Should().Be(ownerCompanyName);
        if (alias == "Test-Alias2")
        {
            result.CompanyIdAliase.Should().HaveCount(5).And.Satisfy(
                x => x.CompanyId == new Guid("41fd2ab8-71cd-4546-9bef-a388d91b2542") && x.Aliase.SequenceEqual(new[] { "Test-Alias2" }),
                x => x.CompanyId == new Guid("41fd2ab8-7123-4546-9bef-a388d91b2999") && x.Aliase.SequenceEqual(new[] { "Test-Alias2" }),
                x => x.CompanyId == new Guid("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd") && x.Aliase.Order().SequenceEqual(new[] { "Idp-123", "Test-Alias2" }),
                x => x.CompanyId == new Guid("0dcd8209-85e2-4073-b130-ac094fb47106") && x.Aliase.Order().SequenceEqual(new[] { "Idp-123", "Test-Alias2" }),
                x => x.CompanyId == new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f88") && x.Aliase.Order().SequenceEqual(new[] { "Test-Alias", "Test-Alias2" })
            );
        }
        else
        {
            result.CompanyIdAliase.Should().Match<IEnumerable<(Guid CompanyId, IEnumerable<string> Aliase)>>(cida => cida.Select(x => x.CompanyId).Order().SequenceEqual(companyIds!.Select(i => new Guid(i)).Order()) &&
                cida.Select(x => x.Aliase).All(a => a.Order().SequenceEqual(new[] { alias, "Test-Alias2" })));
        }
    }

    #endregion

    #region GetCompanyIdentityProviderCategoryDataUntracked

    [Fact]
    public async Task GetCompanyIdentityProviderCategoryDataUntracked_WithValid_ReturnsExpected()
    {
        var sut = await CreateSut().ConfigureAwait(false);

        var results = await sut.GetCompanyIdentityProviderCategoryDataUntracked(_companyId).ToListAsync().ConfigureAwait(false);

        // Assert
        results.Should().HaveCount(3);
        results.Should().Satisfy(
            x => x.Alias == "Idp-123" && x.CategoryId == IdentityProviderCategoryId.KEYCLOAK_OIDC && x.TypeId == IdentityProviderTypeId.MANAGED,
            x => x.Alias == "Shared-Alias" && x.CategoryId == IdentityProviderCategoryId.KEYCLOAK_OIDC && x.TypeId == IdentityProviderTypeId.SHARED,
            x => x.Alias == "Managed-Alias" && x.CategoryId == IdentityProviderCategoryId.KEYCLOAK_OIDC && x.TypeId == IdentityProviderTypeId.MANAGED);
    }

    #endregion

    #region GetSingleManagedIdentityProviderAliasDataUntracked

    [Fact]
    public async Task GetSingleManagedIdentityProviderAliasDataUntracked_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetSingleManagedIdentityProviderAliasDataUntracked(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87")).ConfigureAwait(false);

        // Assert
        result.Should().Match<(Guid IdentityProviderId, string? Alias)>(x => x.IdentityProviderId == new Guid("38f56465-ce26-4f25-9745-1791620dc200") && x.Alias == "Test-Alias2");
    }

    [Fact]
    public async Task GetSingleManagedIdentityProviderAliasDataUntracked_WithCompanyOwningMultipleManagedIdps_Throws()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var Act = () => sut.GetSingleManagedIdentityProviderAliasDataUntracked(new Guid("ac861325-bc54-4583-bcdc-9e9f2a38ff84"));

        // Assert
        var result = await Assert.ThrowsAsync<InvalidOperationException>(Act).ConfigureAwait(false);
    }

    [Fact]
    public async Task GetSingleManagedIdentityProviderAliasDataUntracked_WithCompanyOwningOwnIdp_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetSingleManagedIdentityProviderAliasDataUntracked(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f88")).ConfigureAwait(false);

        // Assert
        result.Should().Be(((Guid, string?))default);
    }

    #endregion

    #region GetManagedIdentityProviderAliasDataUntracked

    [Fact]
    public async Task GetManagedIdentityProviderAliasDataUntracked_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetManagedIdentityProviderAliasDataUntracked(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), new[] { new Guid("38f56465-ce26-4f25-9745-1791620dc200") }).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Match<(Guid IdentityProviderId, string? Alias)>(x => x.IdentityProviderId == new Guid("38f56465-ce26-4f25-9745-1791620dc200") && x.Alias == "Test-Alias2");
    }

    [Fact]
    public async Task GetManagedIdentityProviderAliasDataUntracked_WithOtherCompanyIdentityProviderId_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetManagedIdentityProviderAliasDataUntracked(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), new[] { new Guid("38f56465-ce26-4f25-9745-1791620dc199") }).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetManagedIdentityProviderAliasDataUntracked_WithCompanyOwningOwnIdp_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetManagedIdentityProviderAliasDataUntracked(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f88"), new[] { new Guid("38f56465-ce26-4f25-9745-1791620dc199") }).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateCompanyIdentityProviders

    [Fact]
    public async Task CreateCompanyIdentityProviders_WithValid_ReturnsExpected()
    {
        var identityProviderId = new Guid("38f56465-ce26-4f25-9745-1791620dc198");
        var identityProviderId2 = new Guid("07543ffd-fac9-436a-b60e-5f599e9bc748");
        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        sut.CreateCompanyIdentityProviders(new ValueTuple<Guid, Guid>[]
        {
            (_companyId, identityProviderId),
            (_companyId, identityProviderId2)
        });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty()
            .And.HaveCount(2);
        changedEntries.Select(x => x.Entity).Should().AllBeOfType<CompanyIdentityProvider>().Which.Should().Satisfy(
                idp => idp.CompanyId == _companyId && idp.IdentityProviderId == identityProviderId,
                idp => idp.CompanyId == _companyId && idp.IdentityProviderId == identityProviderId2
            );
    }

    #endregion

    #region GetCompanyNameIdpAliaseUntrackedAsync

    [Fact]
    public async Task GetCompanyNameIdpAliaseUntrackedAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetCompanyNameIdpAliaseUntrackedAsync(new Guid("8b42e6de-7b59-4217-a63c-198e83d93776"), null, IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.SHARED).ConfigureAwait(false);

        // Assert
        result.Should().NotBe(default);
        result.Company.CompanyId.Should().Be(new Guid("ac861325-bc54-4583-bcdc-9e9f2a38ff84"));
        result.Company.CompanyName.Should().Be("Bayerische Motorenwerke AG");
        result.IdpAliase.Should().HaveCount(1).And.Satisfy(x => x.IdentityProviderId == new Guid("38f56465-ce26-4f25-9745-1791620dc201") && x.Alias == "Shared-Alias");
    }

    #endregion

    #region GetCompanyNameIdpAliasUntrackedAsync

    [Fact]
    public async Task GetCompanyNameIdpAliasUntrackedAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetCompanyNameIdpAliasUntrackedAsync(new Guid("38f56465-ce26-4f25-9745-1791620dc201"), new Guid("8b42e6de-7b59-4217-a63c-198e83d93776")).ConfigureAwait(false);

        // Assert
        result.Should().NotBe(default);
        result.Company.CompanyId.Should().Be(new Guid("ac861325-bc54-4583-bcdc-9e9f2a38ff84"));
        result.Company.CompanyName.Should().Be("Bayerische Motorenwerke AG");
        result.CompanyUser.Email.Should().Be("test@email.com");
        result.CompanyUser.FirstName.Should().Be("First");
        result.CompanyUser.LastName.Should().Be("User");
        result.IdentityProvider.IdpAlias.Should().Be("Shared-Alias");
        result.IdentityProvider.IsSharedIdp.Should().Be(true);
    }

    #endregion

    #region GetOwnIdentityProviderWithConnectedCompanies

    [Fact]
    public async Task GetOwnIdentityProviderWithConnectedCompanies_WithNotOwned_ReturnsExpected()
    {
        var sut = await CreateSut().ConfigureAwait(false);

        var result = await sut.GetOwnIdentityProviderWithConnectedCompanies(new Guid("38f56465-ce26-4f25-9745-1791620dc198"), new Guid("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd")).ConfigureAwait(false);

        // Assert
        result.Alias.Should().Be("Idp-123");
        result.IsOwnerCompany.Should().BeFalse();
        result.TypeId.Should().Be(IdentityProviderTypeId.MANAGED);
        result.ConnectedCompanies.Should().HaveCount(2).And.Satisfy(
            x => x.CompanyId == new Guid("0dcd8209-85e2-4073-b130-ac094fb47106") && x.CompanyName == "SAP AG",
            x => x.CompanyId == new Guid("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd") && x.CompanyName == "Service Provider");
    }

    [Fact]
    public async Task GetOwnIdentityProviderWithConnectedCompanies_WithValid_ReturnsExpected()
    {
        var sut = await CreateSut().ConfigureAwait(false);

        var result = await sut.GetOwnIdentityProviderWithConnectedCompanies(new Guid("38f56465-ce26-4f25-9745-1791620dc199"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f88")).ConfigureAwait(false);

        // Assert
        result.Alias.Should().Be("Test-Alias");
        result.IsOwnerCompany.Should().BeTrue();
        result.TypeId.Should().Be(IdentityProviderTypeId.OWN);
        result.ConnectedCompanies.Should().ContainSingle().And.Satisfy(x =>
            x.CompanyId == new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f88") && x.CompanyName == "CX-Test-Access");
    }

    [Fact]
    public async Task GetOwnIdentityProviderWithConnectedCompanies_WithMultipleValid_ReturnsExpected()
    {
        var sut = await CreateSut().ConfigureAwait(false);

        var result = await sut.GetOwnIdentityProviderWithConnectedCompanies(new Guid("38f56465-ce26-4f25-9745-1791620dc198"), new Guid("ac861325-bc54-4583-bcdc-9e9f2a38ff84")).ConfigureAwait(false);

        // Assert
        result.Alias.Should().Be("Idp-123");
        result.IsOwnerCompany.Should().BeTrue();
        result.TypeId.Should().Be(IdentityProviderTypeId.MANAGED);
        result.ConnectedCompanies.Should().HaveCount(2).And.Satisfy(
            x => x.CompanyId == new Guid("0dcd8209-85e2-4073-b130-ac094fb47106") && x.CompanyName == "SAP AG",
            x => x.CompanyId == new Guid("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd") && x.CompanyName == "Service Provider");
    }

    #endregion

    #region Setup    

    private async Task<(IdentityProviderRepository, PortalDbContext)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new IdentityProviderRepository(context);
        return (sut, context);
    }

    private async Task<IdentityProviderRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new IdentityProviderRepository(context);
        return sut;
    }

    #endregion
}
