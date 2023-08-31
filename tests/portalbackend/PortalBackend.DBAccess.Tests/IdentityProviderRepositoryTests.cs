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

    [Fact]
    public async Task GetOwnCompanyIdentityProviderAliasUntrackedAsync_WithValid_ReturnsExpected()
    {
        var identityProviderId = new Guid("38f56465-ce26-4f25-9745-1791620dc199");
        var sut = await CreateSut().ConfigureAwait(false);

        var result = await sut.GetOwnCompanyIdentityProviderAliasUntrackedAsync(identityProviderId, _companyId).ConfigureAwait(false);

        // Assert
        result.Alias.Should().Be("Test-Alias");
        result.IsOwnCompany.Should().BeTrue();
        result.TypeId.Should().Be(IdentityProviderTypeId.OWN);
    }

    #endregion

    #region GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync

    [Fact]
    public async Task GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync_WithValidOwner_ReturnsExpected()
    {
        var identityProviderId = new Guid("38f56465-ce26-4f25-9745-1791620dc199");
        var sut = await CreateSut().ConfigureAwait(false);

        var result = await sut.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, _companyId, _companyId).ConfigureAwait(false);

        // Assert
        result.Alias.Should().Be("Test-Alias");
        result.IsOwner.Should().BeTrue();
        result.IsSameCompany.Should().BeTrue();
    }

    [Fact]
    public async Task GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync_WithInvalidOwner_ReturnsExpected()
    {
        var identityProviderId = new Guid("38f56465-ce26-4f25-9745-1791620dc199");
        var sut = await CreateSut().ConfigureAwait(false);

        var result = await sut.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, _companyId, Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        result.Alias.Should().Be("Test-Alias");
        result.IsOwner.Should().BeFalse();
        result.IsSameCompany.Should().BeTrue();
    }

    [Fact]
    public async Task GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync_WithInvalidCompany_ReturnsExpected()
    {
        var identityProviderId = new Guid("38f56465-ce26-4f25-9745-1791620dc199");
        var sut = await CreateSut().ConfigureAwait(false);

        var result = await sut.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, Guid.NewGuid(), _companyId).ConfigureAwait(false);

        // Assert
        result.Alias.Should().Be("Test-Alias");
        result.IsOwner.Should().BeTrue();
        result.IsSameCompany.Should().BeFalse();
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
            x => x.Alias == "Idp-123" && x.CategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED && x.TypeId == IdentityProviderTypeId.OWN,
            x => x.Alias == "Test-Alias" && x.CategoryId == IdentityProviderCategoryId.KEYCLOAK_OIDC && x.TypeId == IdentityProviderTypeId.OWN);
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
