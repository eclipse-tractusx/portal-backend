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
    private readonly Guid _serviceAccountWithOfferSubscriptions = new("d0c8ae19-d4f3-49cc-9cb4-6c766d4680f3");
    private readonly Guid _serviceAccountWithConnector = new("d0c8ae19-d4f3-49cc-9cb4-6c766d4680f4");
    private readonly Guid _validCompanyUserId = new("ac1cf001-7fbc-1f2f-817f-bce058020006");
    private readonly Guid _validProviderId = new("0dcd8209-85e2-4073-b130-ac094fb47106");
    private readonly Guid _validSubscriberCompanyId = new("ac861325-bc54-4583-bcdc-9e9f2a38ff84");
    private readonly Guid _connectorHostId = new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");

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
        result!.ClientId.Should().Be("7e85a0b8-0001-ab67-10d1-000000001006");
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

    [Fact]
    public async Task GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync_WithValidProviderWithDifferentOwner_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);
        Guid companyServiceAccountId = new("93eecd4e-ca47-4dd2-85bf-775ea72eb000");
        Guid companyId = new("41fd2ab8-71cd-4546-9bef-a388d91b2542");
        // Act
        var result = await sut.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(companyServiceAccountId, companyId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
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

    #endregion

    #region GetOwnCompanyServiceAccountDetailedDataUntrackedAsync

    [Theory]
    [InlineData(10, 0, 10, 10)]
    [InlineData(10, 1, 9, 9)]
    public async Task GetOwnCompanyServiceAccountsUntracked_ReturnsExpectedResult(int count, int page, int size, int expected)
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountsUntracked(_validCompanyId)(page, size).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Count.Should().Be(count);
        result.Data.Should().HaveCount(expected);
        if (expected > 0)
        {
            result.Data.First().CompanyServiceAccountTypeId.Should().Be(CompanyServiceAccountTypeId.OWN);
            result.Data.First().IsOwner.Should().BeTrue();
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

    #region  IsCompanyServiceAccountLinkedCompany

    [Fact]
    public async Task IsCompanyServiceAccountLinkedCompany_ForOwnerCompany_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.IsCompanyServiceAccountLinkedCompany(_validServiceAccountId, _validCompanyId).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCompanyServiceAccountLinkedCompany_ForProviderCompany_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.IsCompanyServiceAccountLinkedCompany(_serviceAccountWithOfferSubscriptions, _validProviderId).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCompanyServiceAccountLinkedCompany_ForConnectorProviderCompany_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.IsCompanyServiceAccountLinkedCompany(_serviceAccountWithConnector, _connectorHostId).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCompanyServiceAccountLinkedCompany_ForSubscriberCompany_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.IsCompanyServiceAccountLinkedCompany(_serviceAccountWithOfferSubscriptions, _validSubscriberCompanyId).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsCompanyServiceAccountLinkedCompany_ForCompanyUserId_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.IsCompanyServiceAccountLinkedCompany(_validCompanyUserId, _validCompanyId).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
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
