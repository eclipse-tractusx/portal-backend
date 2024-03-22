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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="ServiceRepositoryTests"/>
/// </summary>
public class ConnectorRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _userCompanyId = new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");

    public ConnectorRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetAllCompanyConnectorsForCompanyId

    [Fact]
    public async Task GetAllCompanyConnectorsForCompanyId_ReturnsExpectedAppCount()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await Pagination.CreateResponseAsync(
            0,
            10,
            15,
            sut.GetAllCompanyConnectorsForCompanyId(_userCompanyId));

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().HaveCount(3).And.Satisfy(
            x => x.Name == "Test Connector 6"
                && x.TechnicalUser!.Id == new Guid("cd436931-8399-4c1d-bd81-7dffb298c7ca")
                && x.TechnicalUser.Name == "test-user-service-accounts"
                && x.ConnectorUrl == "www.google.de",
            x => x.Name == "Test Connector 1"
                && x.TechnicalUser == null
                && x.ConnectorUrl == "www.google.de",
            x => x.Name == "Test Connector 42"
                && x.TechnicalUser == null
                && x.ConnectorUrl == "www.google.de");
    }

    #endregion

    #region CreateConnector

    [Fact]
    public async Task CreateConnector_ReturnsExpected()
    {
        // Arrange
        var (sut, context) = await CreateSut();

        // Act
        var result = sut.CreateConnector("Test connector", "de", "https://www.test.de", con =>
        {
            con.ProviderId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");
        });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        result.Name.Should().Be("Test connector");
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<Connector>().Which.Name.Should().Be("Test connector");
    }

    [Fact]
    public async Task CreateConnector_WithServiceAccount_ReturnsExpected()
    {
        // Arrange
        var (sut, context) = await CreateSut();

        // Act
        var result = sut.CreateConnector("Test connector", "de", "https://www.test.de", con =>
        {
            con.ProviderId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");
        });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        result.Name.Should().Be("Test connector");
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle()
            .Which.Entity.Should().BeOfType<Connector>()
            .Which.Name.Should().Be("Test connector");
    }

    #endregion

    #region AttachAndModify

    [Fact]
    public async Task AttachAndModify_ReturnsExpected()
    {
        // Arrange
        var (sut, context) = await CreateSut();

        // Act
        sut.AttachAndModifyConnector(new Guid("5aea3711-cc54-47b4-b7eb-ba9f3bf1cb15"), null, con =>
        {
            con.ProviderId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");
            con.TypeId = ConnectorTypeId.CONNECTOR_AS_A_SERVICE;
        });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Modified);
        changedEntity.Entity.Should().BeOfType<Connector>().Which.TypeId.Should().Be(ConnectorTypeId.CONNECTOR_AS_A_SERVICE);
    }

    #endregion

    #region GetConnectorByIdForIamUser

    [Fact]
    public async Task GetConnectorByIdForIamUser_ReturnsExpectedAppCount()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorByIdForCompany(new Guid("7e86a0b8-6903-496b-96d1-0ef508206833"), _userCompanyId);

        // Assert
        result.Should().NotBeNull();
        result.IsProviderCompany.Should().BeTrue();
        result.ConnectorData.Name.Should().Be("Test Connector 1");
        result.ConnectorData.TechnicalUser.Should().BeNull();
        result.ConnectorData.ConnectorUrl.Should().Be("www.google.de");
    }

    [Fact]
    public async Task GetConnectorByIdForIamUser_WithoutExistingId_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorByIdForCompany(Guid.NewGuid(), _userCompanyId);

        // Assert
        result.Should().Be(default);
    }

    [Fact]
    public async Task GetConnectorByIdForIamUser_WithoutMatchingUser_ReturnsIsProviderUserFalse()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorByIdForCompany(new Guid("5aea3711-cc54-47b4-b7eb-ba9f3bf1cb15"), Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result.IsProviderCompany.Should().BeFalse();
    }

    #endregion

    #region GetConnectorInformationByIdForIamUser

    [Fact]
    public async Task GetConnectorInformationByIdForIamUser_ReturnsExpectedAppCount()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorInformationByIdForIamUser(new Guid("7e86a0b8-6903-496b-96d1-0ef508206833"), _userCompanyId);

        // Assert
        result.Should().NotBeNull();
        result.IsProviderUser.Should().BeTrue();
    }

    [Fact]
    public async Task GetConnectorInformationByIdForIamUser_WithoutExistingId_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorInformationByIdForIamUser(Guid.NewGuid(), _userCompanyId);

        // Assert
        result.Should().Be(default);
    }

    [Fact]
    public async Task GetConnectorInformationByIdForIamUser_WithoutMatchingUser_ReturnsIsProviderUserFalse()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorInformationByIdForIamUser(new Guid("5aea3711-cc54-47b4-b7eb-ba9f3bf1cb15"), Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result.IsProviderUser.Should().BeFalse();
    }

    #endregion

    #region GetConnectorDataById

    [Fact]
    public async Task GetConnectorDataById_ReturnsExpectedAppCount()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorDataById(new Guid("7e86a0b8-6903-496b-96d1-0ef508206833"));

        // Assert
        result.Should().NotBeNull();
        result.ConnectorId.Should().Be(new Guid("7e86a0b8-6903-496b-96d1-0ef508206833"));
        result.SelfDescriptionDocumentId.Should().BeNull();
    }

    [Fact]
    public async Task GetConnectorDataById_WithoutExistingId_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorDataById(Guid.NewGuid());

        // Assert
        result.Should().Be(default);
    }

    #endregion

    #region GetSelfDescriptionDocumentData

    [Fact]
    public async Task GetSelfDescriptionDocumentDataAsync_WithoutDocumentId_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorDeleteDataAsync(new Guid("7e86a0b8-6903-496b-96d1-0ef508206833"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));

        // Assert
        result.Should().NotBeNull();
        result!.IsProvidingOrHostCompany.Should().BeTrue();
        result.SelfDescriptionDocumentId.Should().BeNull();
    }

    [Fact]
    public async Task GetSelfDescriptionDocumentDataAsync_WithDocumentId_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorDeleteDataAsync(new Guid("7e86a0b8-6903-496b-96d1-0ef508206839"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));

        // Assert
        result.Should().NotBeNull();
        result!.IsProvidingOrHostCompany.Should().BeTrue();
        result.SelfDescriptionDocumentId.Should().Be(new Guid("e020787d-1e04-4c0b-9c06-bd1cd44724b3"));
        result.DocumentStatusId.Should().Be(DocumentStatusId.LOCKED);
    }

    [Fact]
    public async Task GetSelfDescriptionDocumentDataAsync_WithoutExistingCompanyId_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorDeleteDataAsync(new Guid("7e86a0b8-6903-496b-96d1-0ef508206839"), Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.IsProvidingOrHostCompany.Should().BeFalse();
    }

    [Fact]
    public async Task GetSelfDescriptionDocumentDataAsync_WithoutExistingConnectorId_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorDeleteDataAsync(new Guid(), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSelfDescriptionDocumentDataAsync_WithConnectorOfferSubscription_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorDeleteDataAsync(new Guid("4618c650-709c-4580-956a-85b76eecd4b8"), new Guid("41fd2ab8-71cd-4546-9bef-a388d91b2542"));

        // Assert
        result.Should().NotBeNull();
        result!.ConnectorStatus.Should().Be(ConnectorStatusId.PENDING);
        result!.ConnectorOfferSubscriptions.Should().Satisfy(
            x => x.AssignedOfferSubscriptionIds == new Guid("014afd09-e51a-4ecf-83ab-a5380d9af832")
            && x.OfferSubscriptionStatus == OfferSubscriptionStatusId.PENDING);
    }

    #endregion

    #region GetManagedConnectorsForIamUser

    [Fact]
    public async Task GetManagedConnectorsForIamUser_ReturnsExpectedAppCount()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetManagedConnectorsForCompany(_userCompanyId).Invoke(0, 10);

        // Assert
        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
        result.Data.Should().ContainSingle().And.Satisfy(
            x => x.Name == "Test Connector 3" &&
                x.Type == ConnectorTypeId.CONNECTOR_AS_A_SERVICE &&
                x.Status == ConnectorStatusId.PENDING &&
                x.TechnicalUser!.Id == new Guid("d0c8ae19-d4f3-49cc-9cb4-6c766d4680f4") &&
                x.TechnicalUser.Name == "sa-test" &&
                x.TechnicalUser.Description == "SA with connector" &&
                x.ConnectorUrl == "www.google.de");
    }

    [Fact]
    public async Task GetManagedConnectorsForIamUser_WithoutMatchingUser_ReturnsIsProviderUserFalse()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetManagedConnectorsForCompany(Guid.NewGuid()).Invoke(0, 10);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetConnectorUpdateInformation

    [Fact]
    public async Task GetConnectorUpdateInformation_ReturnsExpectedAppCount()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorUpdateInformation(new Guid("7e86a0b8-6903-496b-96d1-0ef508206833"), _userCompanyId);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(ConnectorStatusId.PENDING);
        result.Type.Should().Be(ConnectorTypeId.COMPANY_CONNECTOR);
    }

    [Fact]
    public async Task GetConnectorUpdateInformation_WithoutExistingConnector_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorUpdateInformation(Guid.NewGuid(), _userCompanyId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetConnectorEndPointDataAsync

    [Theory]
    [InlineData(new[] { "BPNL00000003AYRE", "BPNL00000003CRHK" }, 3, 2)]
    [InlineData(new string[] { }, 4, 3)]
    [InlineData(new[] { "not a bpn" }, 0, 0)]
    public async Task GetConnectorEndPointDataAsync_WithExistingConnector_ReturnsExpectedResult(IEnumerable<string> bpns, int numResults, int numGroups)
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorEndPointDataAsync(bpns).ToListAsync();

        // Assert
        result.Should().NotBeNull().And.HaveCount(numResults);
        if (numResults > 0)
        {
            var grouped = result.GroupBy(x => x.BusinessPartnerNumber, x => x.ConnectorEndpoint);
            var presorted = await result.ToAsyncEnumerable().PreSortedGroupBy(x => x.BusinessPartnerNumber, x => x.ConnectorEndpoint).ToListAsync();
            grouped.Should().HaveCount(numGroups);
            presorted.Should().HaveCount(numGroups);
            grouped.Join(presorted, g => g.Key, p => p.Key, (g, p) => (g, p)).Should().AllSatisfy(
                j => j.g.OrderBy(x => x).Should().ContainInOrder(j.p.OrderBy(x => x))
            );
            if (bpns.Any())
            {
                grouped.Select(x => x.Key).Should().HaveSameCount(bpns).And.AllSatisfy(x => bpns.Should().Contain(x));
            }
        }
    }

    #endregion

    #region DeleteConnector

    [Fact]
    public async Task DeleteConnector_ExecutesExpected()
    {
        // Arrange
        var (sut, context) = await CreateSut();

        // Act
        sut.DeleteConnector(new Guid("7e86a0b8-6903-496b-96d1-0ef508206833"));

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var removedEntity = changedEntries.Single();
        removedEntity.State.Should().Be(EntityState.Deleted);
    }

    #endregion

    #region CreateConnectorAssignedSubscriptions

    [Fact]
    public async Task CreateConnectorAssignedSubscriptions_ExecutesExpected()
    {
        // Arrange
        var (sut, context) = await CreateSut();

        // Act
        var result = sut.CreateConnectorAssignedSubscriptions(new Guid("7e86a0b8-6903-496b-96d1-0ef508206833"), new Guid("0b2ca541-206d-48ad-bc02-fb61fbcb5552"));

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        result.OfferSubscriptionId.Should().Be("0b2ca541-206d-48ad-bc02-fb61fbcb5552");
        result.ConnectorId.Should().Be("7e86a0b8-6903-496b-96d1-0ef508206833");
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().State.Should().Be(EntityState.Added);
        changedEntries.Single().Entity.Should().BeOfType<ConnectorAssignedOfferSubscription>().Which.OfferSubscriptionId.Should().Be("0b2ca541-206d-48ad-bc02-fb61fbcb5552");
    }

    [Fact]
    public async Task CreateConnectorAssignedSubscriptions_WithManaged_ThrowsException()
    {
        // Arrange
        var (sut, context) = await CreateSut();

        // Act
        sut.CreateConnectorAssignedSubscriptions(new Guid("7e86a0b8-6903-496b-96d1-0ef508206839"), new Guid("0b2ca541-206d-48ad-bc02-fb61fbcb5552"));
        async Task Act() => await context.SaveChangesAsync();

        // Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(Act);
        ex.Message.Should().Be("An error occurred while saving the entity changes. See the inner exception for details.");
    }

    #endregion

    #region DeleteConnector

    [Fact]
    public async Task DeleteConnectorAssignedSubscriptions_ExecutesExpected()
    {
        // Arrange
        var (sut, context) = await CreateSut();

        // Act
        sut.DeleteConnectorAssignedSubscriptions(new Guid("7e86a0b8-6903-496b-96d1-0ef508206833"), Enumerable.Repeat(new Guid("0b2ca541-206d-48ad-bc02-fb61fbcb5552"), 1));

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var removedEntity = changedEntries.Single();
        removedEntity.State.Should().Be(EntityState.Deleted);
    }

    #endregion

    private async Task<(ConnectorsRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        var sut = new ConnectorsRepository(context);
        return (sut, context);
    }
}
