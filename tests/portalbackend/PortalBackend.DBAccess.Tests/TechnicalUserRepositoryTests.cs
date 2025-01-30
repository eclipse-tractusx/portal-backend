/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="TechnicalUserRepository"/>
/// </summary>
public class TechnicalUserRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _validCompanyId = new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");
    private readonly Guid _validSubscriptionId = new("eb98bdf5-14e1-4feb-a954-453eac0b93cd");
    private readonly Guid _validServiceAccountId = new("7e85a0b8-0001-ab67-10d1-0ef508201007");

    public TechnicalUserRepositoryTests(TestDbFixture testDbFixture)
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
        var result = sut.CreateTechnicalUser(
            _validCompanyId,
            "test",
            "Only a test service account",
            "sa1",
            TechnicalUserTypeId.MANAGED,
            TechnicalUserKindId.INTERNAL,
            sa =>
            {
                sa.OfferSubscriptionId = _validSubscriptionId;
            });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        result.OfferSubscriptionId.Should().Be(_validSubscriptionId);
        result.TechnicalUserTypeId.Should().Be(TechnicalUserTypeId.MANAGED);
        result.ClientClientId.Should().Be("sa1");
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle()
            .Which.Entity.Should().BeOfType<TechnicalUser>().Which.OfferSubscriptionId.Should().Be(_validSubscriptionId);
    }

    #endregion

    #region GetOwnCompanyServiceAccountWithIamClientIdAsync

    [Fact]
    public async Task GetOwnCompanyServiceAccountWithIamClientIdAsync_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetTechnicalUserWithRoleDataClientIdAsync(_validServiceAccountId, _validCompanyId);

        // Assert
        result.Should().NotBeNull().And.Match<TechnicalUserWithRoleDataClientId>(
            x => x.TechnicalUserTypeId == TechnicalUserTypeId.OWN &&
                 x.TechnicalUserKindId == TechnicalUserKindId.INTERNAL);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountWithIamClientIdAsync_WithoutExistingSa_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetTechnicalUserWithRoleDataClientIdAsync(Guid.NewGuid(), _validCompanyId);

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
        var result = await sut.GetOwnTechnicalUserWithIamUserRolesAsync(_validServiceAccountId, _validCompanyId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert
        result.Should().NotBe(default);
        result!.ClientClientId.Should().Be("sa-cl5-custodian-2");
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync_WithoutExistingSa_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnTechnicalUserWithIamUserRolesAsync(Guid.NewGuid(), _validCompanyId, Enumerable.Empty<ProcessStepTypeId>());

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
        var result = await sut.GetOwnTechnicalUserWithIamUserRolesAsync(companyServiceAccountId, companyId, Enumerable.Empty<ProcessStepTypeId>());
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
        var result = await sut.GetOwnTechnicalUserDataUntrackedAsync(_validServiceAccountId, _validCompanyId);

        // Assert
        result.Should().NotBeNull()
            .And.Match<TechnicalUserDetailedData>(x =>
                x.TechnicalUserTypeId == TechnicalUserTypeId.OWN &&
                x.Status == UserStatusId.ACTIVE &&
                x.CompanyLastEditorData != null &&
                x.CompanyLastEditorData.CompanyName == "CX-Test-Access" &&
                x.CompanyLastEditorData.Name == "CX Admin");
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailedDataUntrackedAsync_WithoutExistingSa_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnTechnicalUserDataUntrackedAsync(Guid.NewGuid(), _validCompanyId);

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
        var result = await sut.GetOwnTechnicalUserDataUntrackedAsync(companyServiceAccountId, companyId);

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
        var result = await sut.GetOwnTechnicalUserDataUntrackedAsync(companyServiceAccountId, companyId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailedDataUntrackedAsync_WithInactiveStatus_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();
        Guid companyServiceAccountId = new("38c92162-6328-40ce-80f3-22e3f3e9b94d");
        Guid companyId = new("729e0af2-6723-4a7f-85a1-833d84b39bdf");
        // Act
        var result = await sut.GetOwnTechnicalUserDataUntrackedAsync(companyServiceAccountId, companyId);

        // Assert
        result.Should().NotBeNull()
            .And.Match<TechnicalUserDetailedData>(x =>
                x.ClientClientId == "sa-x-inactive" &&
                x.TechnicalUserTypeId == TechnicalUserTypeId.MANAGED &&
                x.Status == UserStatusId.INACTIVE);
    }

    #endregion

    #region GetOwnCompanyServiceAccountsUntracked

    [Theory]
    [InlineData(3, 0, 10, 3)]
    [InlineData(3, 1, 9, 2)]
    public async Task GetOwnCompanyServiceAccountsUntracked_ReturnsExpectedResult(int count, int page, int size, int expected)
    {
        // Arrange
        var newvalidCompanyId = new Guid("41fd2ab8-71cd-4546-9bef-a388d91b2542");
        var (sut, _) = await CreateSut();
        // Act
        var result = await sut.GetOwnTechnicalUsers(newvalidCompanyId, null, null, [UserStatusId.ACTIVE])(page, size);

        // Assert
        result.Should().NotBeNull()
            .And.Match<Framework.Models.Pagination.Source<CompanyServiceAccountData>>(x =>
                x.Count == count &&
                x.Data.Count() == expected);
        if (expected > 0)
        {
            result!.Data.First().Should().Match<CompanyServiceAccountData>(y =>
                y.TechnicalUserTypeId == TechnicalUserTypeId.MANAGED &&
                y.IsOwner &&
                !y.IsProvider);
        }
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithClientIdAndOwner_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnTechnicalUsers(_validCompanyId, "sa-cl5-custodian-2", true, [UserStatusId.ACTIVE])(0, 10);

        // Assert
        result!.Count.Should().Be(1);
        result.Data.Should().HaveCount(1)
            .And.Satisfy(x => x.TechnicalUserTypeId == TechnicalUserTypeId.OWN);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithOwnerTrue_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnTechnicalUsers(_validCompanyId, null, true, [UserStatusId.ACTIVE])(0, 10).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Count.Should().Be(21);
        result.Data.Should().HaveCount(10)
            .And.AllSatisfy(x => x.Should().Match<CompanyServiceAccountData>(y =>
                y.TechnicalUserTypeId == TechnicalUserTypeId.OWN &&
                y.UserStatusId == UserStatusId.ACTIVE))
            .And.BeInAscendingOrder(x => x.Name)
            .And.Satisfy(
                x => x.TechnicalUserId == new Guid("4ce1b774-3d00-4e07-9a53-ae1f64193392"),
                x => x.TechnicalUserId == new Guid("a946f314-f53e-4c72-9124-40b72bcc59aa"),
                x => x.TechnicalUserId == new Guid("7e85a0b8-0001-ab67-10d1-0ef508201029"),
                x => x.TechnicalUserId == new Guid("7e85a0b8-0001-ab67-10d1-0ef508201026"),
                x => x.TechnicalUserId == new Guid("7e85a0b8-0001-ab67-10d1-0ef508201027"),
                x => x.TechnicalUserId == new Guid("7e85a0b8-0001-ab67-10d1-0ef508201030"),
                x => x.TechnicalUserId == new Guid("f3498fe6-e0e5-413b-a725-39bf5c7c1959"),
                x => x.TechnicalUserId == new Guid("ab7f01ea-cbb9-4d58-9efa-ea992395f997"),
                x => x.TechnicalUserId == new Guid("7e85a0b8-0001-ab67-10d1-0ef508201031"),
                x => x.TechnicalUserId == new Guid("7e85a0b8-0001-ab67-10d1-0ef508201032"));
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithOwnerFalse_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnTechnicalUsers(_validCompanyId, null, false, [UserStatusId.ACTIVE])(0, 10).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
        result.Data.Should().HaveCount(1)
            .And.Satisfy(x => x.TechnicalUserTypeId == TechnicalUserTypeId.MANAGED
                && !x.IsOwner && x.IsProvider);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithProviderCompany_ReturnsProviderOwned()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnTechnicalUsers(new Guid("729e0af2-6723-4a7f-85a1-833d84b39bdf"), null, null, [UserStatusId.ACTIVE])(0, 10).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Count.Should().Be(2);
        result.Data.Should().HaveCount(2)
            .And.Satisfy(
                x => x.TechnicalUserTypeId == TechnicalUserTypeId.PROVIDER_OWNED && x.IsOwner && x.IsProvider,
                x => x.TechnicalUserTypeId == TechnicalUserTypeId.MANAGED && x.IsOwner && !x.IsProvider);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithSubscribingCompany_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnTechnicalUsers(new Guid("41fd2ab8-7123-4546-9bef-a388d91b2999"), null, null, [UserStatusId.ACTIVE])(0, 10).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithClientIdAndProvider_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnTechnicalUsers(new("41fd2ab8-71cd-4546-9bef-a388d91b2543"), "sa-x-2", false, [UserStatusId.ACTIVE])(0, 10);

        // Assert
        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
        result.Data.Should().HaveCount(1)
            .And.Satisfy(x => x.TechnicalUserTypeId == TechnicalUserTypeId.MANAGED);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithOnlyClientId_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnTechnicalUsers(_validCompanyId, "sa-cl5-custodian-2", null, [UserStatusId.ACTIVE])(0, 10);

        // Assert
        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
        result.Data.Should().ContainSingle()
            .Which.TechnicalUserTypeId.Should().Be(TechnicalUserTypeId.OWN);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithSearch_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnTechnicalUsers(_validCompanyId, "sa-cl", null, [UserStatusId.ACTIVE])(0, 10);

        // Assert
        result.Should().NotBeNull();
        result!.Count.Should().Be(18);
        result.Data.Should().HaveCount(10);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithUserStatusId_InActive_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnTechnicalUsers(new Guid("729e0af2-6723-4a7f-85a1-833d84b39bdf"), null, null, [UserStatusId.INACTIVE])(0, 10);

        // Assert
        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
        result.Data.Should().ContainSingle()
            .Which.Should().Match<CompanyServiceAccountData>(x =>
                x.TechnicalUserId == new Guid("38c92162-6328-40ce-80f3-22e3f3e9b94d") &&
                x.ClientId == "sa-x-inactive" &&
                x.TechnicalUserTypeId == TechnicalUserTypeId.MANAGED &&
                x.UserStatusId == UserStatusId.INACTIVE);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithMultipleStatus_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnTechnicalUsers(new("729e0af2-6723-4a7f-85a1-833d84b39bdf"), null, null, [UserStatusId.ACTIVE, UserStatusId.INACTIVE, UserStatusId.PENDING, UserStatusId.DELETED])(0, 10);

        // Assert
        result.Should().NotBeNull();
        result!.Data.DistinctBy(x => x.UserStatusId).Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountsUntracked_WithInvalidCompanyId_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnTechnicalUsers(new Guid("deadbeef-dead-beef-dead-beefdeadbeef"), null, null, [UserStatusId.ACTIVE, UserStatusId.INACTIVE, UserStatusId.PENDING, UserStatusId.DELETED])(0, 10);

        // Assert
        result.Should().BeNull();
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

    [Fact]
    public async Task CheckActiveServiceAccountExistsForCompanyAsyncForPendingStatus_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.CheckActiveServiceAccountExistsForCompanyAsync(new Guid("4ce1b774-3d00-4e07-9a53-ae1f64193394"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckActiveServiceAccountExistsForCompanyAsyncForInactiveStatus_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.CheckActiveServiceAccountExistsForCompanyAsync(new Guid("38c92162-6328-40ce-80f3-22e3f3e9b94d"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CreateDimCompanyServiceAccount

    [Fact]
    public async Task CreateDimCompanyServiceAccount_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSut();

        // Act
        sut.CreateExternalTechnicalUser(
            _validServiceAccountId,
            "https://example.org/auth",
            "test"u8.ToArray(),
            "test"u8.ToArray(),
            0
        );

        // Assert
        var changeTracker = context.ChangeTracker;
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should().ContainSingle()
            .Which.Entity.Should().BeOfType<ExternalTechnicalUser>()
            .Which.EncryptionMode.Should().Be(0);
    }

    #endregion

    #region CreateDimUserCreationData

    [Fact]
    public async Task CreateDimUserCreationData_ReturnsExpectedResult()
    {
        // Arrange
        var processId = Guid.NewGuid();
        var (sut, context) = await CreateSut();

        // Act
        sut.CreateExternalTechnicalUserCreationData(
            _validServiceAccountId,
            processId
        );

        // Assert
        var changeTracker = context.ChangeTracker;
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should().ContainSingle()
            .Which.Entity.Should().BeOfType<ExternalTechnicalUserCreationData>()
            .Which.ProcessId.Should().Be(processId);
    }

    #endregion

    #region AttachAndModifyServiceAccount

    [Fact]
    public async Task AttachAndModifyServiceAccount_ReturnsExpected()
    {
        // Arrange
        var id = Guid.NewGuid();
        var version = Guid.NewGuid();

        var (sut, context) = await CreateSut();

        // Act
        sut.AttachAndModifyTechnicalUser(id, version,
            x =>
            {
                x.Description = "test";
                x.ClientClientId = "foo";
            },
            x => x.ClientClientId = "bar");

        // Assert
        var changeTracker = context.ChangeTracker;
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should().ContainSingle()
            .Which.Entity.Should().BeOfType<TechnicalUser>()
            .Which.Should().Match<TechnicalUser>(
                x => x.Id == id && x.Version != version && x.Description == "test" && x.ClientClientId == "bar"
            );
    }

    #endregion

    #region Setup

    private async Task<(TechnicalUserRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        var sut = new TechnicalUserRepository(context);
        return (sut, context);
    }

    #endregion
}
