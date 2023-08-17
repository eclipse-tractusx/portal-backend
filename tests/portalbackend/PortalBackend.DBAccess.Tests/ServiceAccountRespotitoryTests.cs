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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="ServiceAccountRepository"/>
/// </summary>
public class ServiceAccountRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _validCompanyId = new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");
    private readonly Guid _validSubscriptionId = new("eb98bdf5-14e1-4feb-a954-453eac0b93cd");
    private readonly Guid _validServiceAccountId = new("7e85a0b8-0001-ab67-10d1-0ef508201006");

    public ServiceAccountRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

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
            "test",
            "Only a test service account",
            "test-1",
            "sa1",
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
        result.ClientId.Should().Be("test-1");
        result.ClientClientId.Should().Be("sa1");
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<CompanyServiceAccount>().Which.OfferSubscriptionId.Should().Be(_validSubscriptionId);
    }

    #endregion

    #region GetOwnCompanyServiceAccountWithIamClientIdAsync

    [Fact]
    public async Task GetOwnCompanyServiceAccountWithIamClientIdAsync_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountWithIamClientIdAsync(_validServiceAccountId, _validCompanyId).ConfigureAwait(false);

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
        var result = await sut.GetOwnCompanyServiceAccountWithIamClientIdAsync(Guid.NewGuid(), _validCompanyId).ConfigureAwait(false);

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
        var result = await sut.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(_validServiceAccountId, _validCompanyId).ConfigureAwait(false);

        // Assert
        result.Should().NotBe(default);
        result!.ClientId.Should().Be("dab9dd17-0d31-46c7-b313-aca61225dcd1");
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync_WithoutExistingSa_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(Guid.NewGuid(), _validCompanyId).ConfigureAwait(false);

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
        var result = await sut.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(_validServiceAccountId, _validCompanyId).ConfigureAwait(false);

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
        var result = await sut.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(Guid.NewGuid(), _validCompanyId).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailedDataUntrackedAsync_WithValidProviderWithDifferentOwner_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);
        Guid companyServiceAccountId = new("93eecd4e-ca47-4dd2-85bf-775ea72eb000");
        Guid companyId = new("41fd2ab8-71cd-4546-9bef-a388d91b2542");
        // Act
        var result = await sut.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(companyServiceAccountId, companyId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailedDataUntrackedAsync_WithInvalidCompanyId_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);
        Guid companyServiceAccountId = new("93eecd4e-ca47-4dd2-85bf-775ea72eb000");
        Guid companyId = new("41fd2ab8-71cd-4546-9bef-a388d91b2544");
        // Act
        var result = await sut.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(companyServiceAccountId, companyId).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetOwnCompanyServiceAccountDetailedDataUntrackedAsync

    [Theory]
    [InlineData(3, 0, 10, 3)]
    [InlineData(3, 1, 9, 2)]
    public async Task GetOwnCompanyServiceAccountsUntracked_ReturnsExpectedResult(int count, int page, int size, int expected)
    {
        // Arrange
        var newvalidCompanyId = new Guid("41fd2ab8-71cd-4546-9bef-a388d91b2542");
        var (sut, _) = await CreateSut().ConfigureAwait(false);
        // Act
        var result = await sut.GetOwnCompanyServiceAccountsUntracked(newvalidCompanyId, null, null)(page, size).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Count.Should().Be(count);
        result.Data.Should().HaveCount(expected);
        if (expected > 0)
        {
            result.Data.First().CompanyServiceAccountTypeId.Should().Be(CompanyServiceAccountTypeId.MANAGED);
            result.Data.First().IsOwner.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithClientIdAndOwner_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountsUntracked(_validCompanyId, "sa-cl5-custodian-1", true)(0, 10).ConfigureAwait(false);

        // Assert
        result!.Count.Should().Be(1);
        result.Data.Should().HaveCount(1)
            .And.Satisfy(x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.OWN);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithClientIdAndProvider_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountsUntracked(new("41fd2ab8-71cd-4546-9bef-a388d91b2543"), "sa-x-2", false)(0, 10).ConfigureAwait(false);

        // Assert
        result!.Count.Should().Be(1);
        result.Data.Should().HaveCount(1)
            .And.Satisfy(x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.MANAGED);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithOnlyClientId_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountsUntracked(_validCompanyId, "sa-cl5-custodian-1", null)(0, 10).ConfigureAwait(false);

        // Assert
        result!.Count.Should().Be(1);
        result.Data.Should().HaveCount(1)
            .And.Satisfy(x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.OWN);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithSearch_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountsUntracked(_validCompanyId, "sa-cl", null)(0, 10).ConfigureAwait(false);

        // Assert
        result!.Count.Should().Be(11);
        result.Data.Should().HaveCount(10);
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
