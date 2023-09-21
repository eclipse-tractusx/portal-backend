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

    #region GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync

    [Theory]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc198", "ac861325-bc54-4583-bcdc-9e9f2a38ff84", true, "Idp-123", true, new[] { "3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", "0dcd8209-85e2-4073-b130-ac094fb47106" })]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc198", "3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", true, "Idp-123", false, new[] { "3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", "0dcd8209-85e2-4073-b130-ac094fb47106" })]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc199", "ac861325-bc54-4583-bcdc-9e9f2a38ff84", true, "Test-Alias", false, new[] { "2dc4249f-b5ca-4d42-bef1-7a7a950a4f88" })]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc199", "2dc4249f-b5ca-4d42-bef1-7a7a950a4f88", true, "Test-Alias", true, new[] { "2dc4249f-b5ca-4d42-bef1-7a7a950a4f88" })]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc200", "41fd2ab8-71cd-4546-9bef-a388d91b2542", true, "Test-Alias2", false, new[] { "41fd2ab8-71cd-4546-9bef-a388d91b2542", "41fd2ab8-7123-4546-9bef-a388d91b2999", "3390c2d7-75c1-4169-aa27-6ce00e1f3cdd", "0dcd8209-85e2-4073-b130-ac094fb47106", "2dc4249f-b5ca-4d42-bef1-7a7a950a4f88" })]
    [InlineData("38f56465-ce26-4f25-9745-1791620dc200", "41fd2ab8-71cd-4546-9bef-a388d91b2542", false, "Test-Alias2", false, null)]
    public async Task GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync_WithValidOwner_ReturnsExpected(Guid identityProviderId, Guid companyId, bool query, string alias, bool isOwner, IEnumerable<string>? companyIds)
    {
        var sut = await CreateSut().ConfigureAwait(false);

        var result = await sut.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, companyId, query).ConfigureAwait(false);

        // Assert
        result.Alias.Should().Be(alias);
        result.IsOwner.Should().Be(isOwner);
        if (query)
        {
            companyIds.Should().NotBeNull();
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
            companyIds.Should().BeNull();
            result.CompanyIdAliase.Should().BeNull();
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
        results.Should().HaveCount(2);
        results.Should().Satisfy(
            x => x.Alias == "Idp-123" && x.CategoryId == IdentityProviderCategoryId.KEYCLOAK_OIDC && x.TypeId == IdentityProviderTypeId.MANAGED,
            x => x.Alias == "Shared-Alias" && x.CategoryId == IdentityProviderCategoryId.KEYCLOAK_OIDC && x.TypeId == IdentityProviderTypeId.SHARED);
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
