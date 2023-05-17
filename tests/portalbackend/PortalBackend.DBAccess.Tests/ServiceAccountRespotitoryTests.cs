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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="ServiceAccountRepository"/>
/// </summary>
public class ServiceAccountRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly IFixture _fixture;
    private readonly TestDbFixture _dbTestDbFixture;
    private const string IamUserId = "502dabcf-01c7-47d9-a88e-0be4279097b5";
    private readonly Guid _validCompanyId = new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");
    private readonly Guid _validSubscriptionId = new("eb98bdf5-14e1-4feb-a954-453eac0b93cd");
    private readonly Guid _validServiceAccountId = new("7e85a0b8-0001-ab67-10d1-0ef508201006");

    public ServiceAccountRepositoryTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _dbTestDbFixture = testDbFixture;
    }

    #region CreateCompanyServiceAccount

    [Fact]
    public async Task CreateCompanyServiceAccount_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = sut.CreateCompanyServiceAccount(
            _validCompanyId,
            CompanyServiceAccountStatusId.ACTIVE,
            "test",
            "Only a test service account",
            CompanyServiceAccountTypeId.MANAGED,
            sa =>
            {
                sa.OfferSubscriptionId = _validSubscriptionId;
            });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        result.OfferSubscriptionId.Should().Be(_validSubscriptionId);
        result.CompanyServiceAccountTypeId.Should().Be(CompanyServiceAccountTypeId.MANAGED);
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<CompanyServiceAccount>().Which.OfferSubscriptionId.Should().Be(_validSubscriptionId);
    }

    #endregion

    #region RemoveIamServiceAccount

    [Fact]
    public async Task RemoveIamServiceAccount_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.RemoveIamServiceAccount(IamUserId);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Should().AllSatisfy(x => x.State.Should().Be(EntityState.Deleted));
        changedEntries.Single().Entity.Should().BeOfType<IamServiceAccount>().Which.ClientId.Should().Be(IamUserId);
    }

    #endregion

    #region CreateCompanyServiceAccountAssignedRoles

    [Fact]
    public async Task CreateCompanyServiceAccountAssignedRole_ReturnsExpectedResult()
    {
        // Arrange
        var companyServiceAccountAssignedRoleIds = _fixture.CreateMany<(Guid ServiceAccountId, Guid UserRoleId)>(3).ToImmutableArray();
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.CreateCompanyServiceAccountAssignedRoles(companyServiceAccountAssignedRoleIds);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().HaveCount(3).And.AllSatisfy(x => x.State.Should().Be(EntityState.Added));
        changedEntries.Select(entry => entry.Entity).Should().AllBeOfType<CompanyServiceAccountAssignedRole>().Which.Should().Satisfy(
            x => x.CompanyServiceAccountId == companyServiceAccountAssignedRoleIds[0].ServiceAccountId && x.UserRoleId == companyServiceAccountAssignedRoleIds[0].UserRoleId,
            x => x.CompanyServiceAccountId == companyServiceAccountAssignedRoleIds[1].ServiceAccountId && x.UserRoleId == companyServiceAccountAssignedRoleIds[1].UserRoleId,
            x => x.CompanyServiceAccountId == companyServiceAccountAssignedRoleIds[2].ServiceAccountId && x.UserRoleId == companyServiceAccountAssignedRoleIds[2].UserRoleId
        );
    }

    #endregion

    #region RemoveCompanyServiceAccountAssignedRoles

    [Fact]
    public async Task RemoveCompanyServiceAccountAssignedRole_ReturnsExpectedResult()
    {
        // Arrange
        var companyServiceAccountAssignedRoleIds = _fixture.CreateMany<(Guid ServiceAccountId, Guid UserRoleId)>(3).ToImmutableArray();
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.RemoveCompanyServiceAccountAssignedRoles(companyServiceAccountAssignedRoleIds);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().HaveCount(3).And.AllSatisfy(x => x.State.Should().Be(EntityState.Deleted));
        changedEntries.Select(entry => entry.Entity).Should().AllBeOfType<CompanyServiceAccountAssignedRole>().Which.Should().Satisfy(
            x => x.CompanyServiceAccountId == companyServiceAccountAssignedRoleIds[0].ServiceAccountId && x.UserRoleId == companyServiceAccountAssignedRoleIds[0].UserRoleId,
            x => x.CompanyServiceAccountId == companyServiceAccountAssignedRoleIds[1].ServiceAccountId && x.UserRoleId == companyServiceAccountAssignedRoleIds[1].UserRoleId,
            x => x.CompanyServiceAccountId == companyServiceAccountAssignedRoleIds[2].ServiceAccountId && x.UserRoleId == companyServiceAccountAssignedRoleIds[2].UserRoleId
        );
    }

    #endregion

    #region GetOwnCompanyServiceAccountWithIamClientIdAsync

    [Fact]
    public async Task GetOwnCompanyServiceAccountWithIamClientIdAsync_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountWithIamClientIdAsync(_validServiceAccountId, IamUserId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.CompanyServiceAccountTypeId.Should().Be(CompanyServiceAccountTypeId.OWN);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountWithIamClientIdAsync_WithoutExistingSa_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountWithIamClientIdAsync(Guid.NewGuid(), IamUserId).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync

    [Fact]
    public async Task GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(_validServiceAccountId, IamUserId).ConfigureAwait(false);

        // Assert
        result.Should().NotBe(default);
        result!.ClientId.Should().Be("7e85a0b8-0001-ab67-10d1-000000001006");
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync_WithoutExistingSa_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(Guid.NewGuid(), IamUserId).ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    #endregion

    #region GetOwnCompanyServiceAccountDetailedDataUntrackedAsync

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailedDataUntrackedAsync_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(_validServiceAccountId, IamUserId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.CompanyServiceAccountTypeId.Should().Be(CompanyServiceAccountTypeId.OWN);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailedDataUntrackedAsync_WithoutExistingSa_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(Guid.NewGuid(), IamUserId).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetOwnCompanyServiceAccountDetailedDataUntrackedAsync

    [Theory]
    [InlineData(9, 0, 10, 9)]
    [InlineData(9, 1, 9, 8)]
    public async Task GetOwnCompanyServiceAccountsUntracked_ReturnsExpectedResult(int count, int page, int size, int expected)
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountsUntracked(IamUserId)(page, size).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Count.Should().Be(count);
        result.Data.Should().HaveCount(expected);
        if (expected > 0)
        {
            result.Data.First().CompanyServiceAccountTypeId.Should().Be(CompanyServiceAccountTypeId.OWN);
        }
    }

    #endregion

    #region CheckActiveServiceAccountExistsForCompanyAsync

    [Fact]
    public async Task CheckActiveServiceAccountExistsForCompanyAsync_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.CheckActiveServiceAccountExistsForCompanyAsync(_validServiceAccountId, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87")).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Setup

    private async Task<(ServiceAccountRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new ServiceAccountRepository(context);
        return (sut, context);
    }

    #endregion
}
