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
        var (sut, context) = await CreateSut();

        // Act
        var result = sut.CreateCompanyServiceAccount(
            _validCompanyId,
            "test",
            "Only a test service account",
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
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyServiceAccountWithIamClientIdAsync(_validServiceAccountId, _validCompanyId);

        // Assert
        result.Should().NotBeNull();
        result!.CompanyServiceAccountTypeId.Should().Be(CompanyServiceAccountTypeId.OWN);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountWithIamClientIdAsync_WithoutExistingSa_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyServiceAccountWithIamClientIdAsync(Guid.NewGuid(), _validCompanyId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync

    [Fact]
    public async Task GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(_validServiceAccountId, _validCompanyId);

        // Assert
        result.Should().NotBe(default);
        result!.ClientClientId.Should().Be("sa-cl5-custodian-1");
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync_WithoutExistingSa_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(Guid.NewGuid(), _validCompanyId);

        // Assert
        result.Should().Be(default);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync_WithValidProviderWithDifferentOwner_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();
        Guid companyServiceAccountId = new("93eecd4e-ca47-4dd2-85bf-775ea72eb000");
        Guid companyId = new("41fd2ab8-71cd-4546-9bef-a388d91b2542");
        // Act
        var result = await sut.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(companyServiceAccountId, companyId);
        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region GetOwnCompanyServiceAccountDetailedDataUntrackedAsync

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailedDataUntrackedAsync_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(_validServiceAccountId, _validCompanyId);

        // Assert
        result.Should().NotBeNull();
        result!.CompanyServiceAccountTypeId.Should().Be(CompanyServiceAccountTypeId.OWN);
        result.CompanyLastEditorData!.CompanyName.Should().Be("CX-Test-Access");
        result.CompanyLastEditorData.Name.Should().Be("CX Admin");
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailedDataUntrackedAsync_WithoutExistingSa_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(Guid.NewGuid(), _validCompanyId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailedDataUntrackedAsync_WithValidProviderWithDifferentOwner_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();
        Guid companyServiceAccountId = new("93eecd4e-ca47-4dd2-85bf-775ea72eb000");
        Guid companyId = new("41fd2ab8-71cd-4546-9bef-a388d91b2542");
        // Act
        var result = await sut.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(companyServiceAccountId, companyId);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailedDataUntrackedAsync_WithInvalidCompanyId_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();
        Guid companyServiceAccountId = new("93eecd4e-ca47-4dd2-85bf-775ea72eb000");
        Guid companyId = new("41fd2ab8-71cd-4546-9bef-a388d91b2544");
        // Act
        var result = await sut.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(companyServiceAccountId, companyId);

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
        var (sut, _) = await CreateSut();
        // Act
        var result = await sut.GetOwnCompanyServiceAccountsUntracked(newvalidCompanyId, null, null, UserStatusId.ACTIVE)(page, size);

        // Assert
        result.Should().NotBeNull();
        result!.Count.Should().Be(count);
        result.Data.Should().HaveCount(expected);
        if (expected > 0)
        {
            result.Data.First().CompanyServiceAccountTypeId.Should().Be(CompanyServiceAccountTypeId.MANAGED);
            result.Data.First().IsOwner.Should().BeTrue();
            result.Data.First().IsProvider.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithClientIdAndOwner_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyServiceAccountsUntracked(_validCompanyId, "sa-cl5-custodian-1", true, UserStatusId.ACTIVE)(0, 10);

        // Assert
        result!.Count.Should().Be(1);
        result.Data.Should().HaveCount(1)
            .And.Satisfy(x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.OWN);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithOwnerTrue_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountsUntracked(_validCompanyId, null, true, UserStatusId.ACTIVE)(0, 10).ConfigureAwait(false);

        // Assert
        result!.Count.Should().Be(13);
        result.Data.Should().HaveCount(10)
            .And.Satisfy(
                x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.OWN && x.ServiceAccountId == new Guid("7e85a0b8-0001-ab67-10d1-0ef508201029"),
                x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.OWN && x.ServiceAccountId == new Guid("7e85a0b8-0001-ab67-10d1-0ef508201026"),
                x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.OWN && x.ServiceAccountId == new Guid("7e85a0b8-0001-ab67-10d1-0ef508201027"),
                x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.OWN && x.ServiceAccountId == new Guid("7e85a0b8-0001-ab67-10d1-0ef508201030"),
                x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.OWN && x.ServiceAccountId == new Guid("7e85a0b8-0001-ab67-10d1-0ef508201031"),
                x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.OWN && x.ServiceAccountId == new Guid("7e85a0b8-0001-ab67-10d1-0ef508201032"),
                x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.OWN && x.ServiceAccountId == new Guid("7e85a0b8-0001-ab67-10d1-0ef508201023"),
                x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.OWN && x.ServiceAccountId == new Guid("7e85a0b8-0001-ab67-10d1-0ef508201024"),
                x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.OWN && x.ServiceAccountId == new Guid("7e85a0b8-0001-ab67-10d1-0ef508201028"),
                x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.OWN && x.ServiceAccountId == new Guid("d0c8ae19-d4f3-49cc-9cb4-6c766d4680f2"));
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithOwnerFalse_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountsUntracked(_validCompanyId, null, false, UserStatusId.ACTIVE)(0, 10).ConfigureAwait(false);

        // Assert
        result!.Count.Should().Be(1);
        result.Data.Should().HaveCount(1)
            .And.Satisfy(x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.MANAGED
                && x.IsOwner == false && x.IsProvider == true);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithClientIdAndProvider_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyServiceAccountsUntracked(new("41fd2ab8-71cd-4546-9bef-a388d91b2543"), "sa-x-2", false, UserStatusId.ACTIVE)(0, 10);

        // Assert
        result!.Count.Should().Be(1);
        result.Data.Should().HaveCount(1)
            .And.Satisfy(x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.MANAGED);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithOnlyClientId_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyServiceAccountsUntracked(_validCompanyId, "sa-cl5-custodian-1", null, UserStatusId.ACTIVE)(0, 10);

        // Assert
        result!.Count.Should().Be(1);
        result.Data.Should().HaveCount(1)
            .And.Satisfy(x => x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.OWN);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithSearch_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyServiceAccountsUntracked(_validCompanyId, "sa-cl", null, UserStatusId.ACTIVE)(0, 10);

        // Assert
        result!.Count.Should().Be(11);
        result.Data.Should().HaveCount(10);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithUserStatusId_InActive_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyServiceAccountsUntracked(new Guid("729e0af2-6723-4a7f-85a1-833d84b39bdf"), null, null, UserStatusId.INACTIVE)(0, 10);

        // Assert
        result!.Count.Should().Be(1);
        result.Data.Should().HaveCount(1)
            .And.Satisfy(x =>
                x.ServiceAccountId == new Guid("38c92162-6328-40ce-80f3-22e3f3e9b94d")
                && x.ClientId == "sa-x-inactive"
                && x.CompanyServiceAccountTypeId == CompanyServiceAccountTypeId.MANAGED);
    }

    #endregion

    #region CheckActiveServiceAccountExistsForCompanyAsync

    [Fact]
    public async Task CheckActiveServiceAccountExistsForCompanyAsync_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.CheckActiveServiceAccountExistsForCompanyAsync(_validServiceAccountId, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Setup

    private async Task<(ServiceAccountRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        var sut = new ServiceAccountRepository(context);
        return (sut, context);
    }

    #endregion
}
