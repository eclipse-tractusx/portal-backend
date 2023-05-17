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

    public ConnectorRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region CreateConnector

    [Fact]
    public async Task CreateConnector_ReturnsExpected()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

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

    #endregion

    #region AttachAndModify

    [Fact]
    public async Task AttachAndModify_ReturnsExpected()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

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
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConnectorByIdForIamUser(new Guid("7e86a0b8-6903-496b-96d1-0ef508206833"), "502dabcf-01c7-47d9-a88e-0be4279097b5").ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsProviderUser.Should().BeTrue();
    }

    [Fact]
    public async Task GetConnectorByIdForIamUser_WithoutExistingId_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConnectorByIdForIamUser(Guid.NewGuid(), "502dabcf-01c7-47d9-a88e-0be4279097b5").ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    [Fact]
    public async Task GetConnectorByIdForIamUser_WithoutMatchingUser_ReturnsIsProviderUserFalse()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConnectorByIdForIamUser(new Guid("5aea3711-cc54-47b4-b7eb-ba9f3bf1cb15"), Guid.NewGuid().ToString()).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsProviderUser.Should().BeFalse();
    }

    #endregion

    #region GetConnectorInformationByIdForIamUser

    [Fact]
    public async Task GetConnectorInformationByIdForIamUser_ReturnsExpectedAppCount()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConnectorInformationByIdForIamUser(new Guid("7e86a0b8-6903-496b-96d1-0ef508206833"), "502dabcf-01c7-47d9-a88e-0be4279097b5").ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsProviderUser.Should().BeTrue();
    }

    [Fact]
    public async Task GetConnectorInformationByIdForIamUser_WithoutExistingId_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConnectorInformationByIdForIamUser(Guid.NewGuid(), "623770c5-cf38-4b9f-9a35-f8b9ae972e2e").ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    [Fact]
    public async Task GetConnectorInformationByIdForIamUser_WithoutMatchingUser_ReturnsIsProviderUserFalse()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConnectorInformationByIdForIamUser(new Guid("5aea3711-cc54-47b4-b7eb-ba9f3bf1cb15"), Guid.NewGuid().ToString()).ConfigureAwait(false);

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
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConnectorDataById(new Guid("7e86a0b8-6903-496b-96d1-0ef508206833")).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.ConnectorId.Should().Be(new Guid("7e86a0b8-6903-496b-96d1-0ef508206833"));
        result.SelfDescriptionDocumentId.Should().BeNull();
    }

    [Fact]
    public async Task GetConnectorDataById_WithoutExistingId_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConnectorDataById(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    #endregion

    #region GetSelfDescriptionDocumentData

    [Fact]
    public async Task GetSelfDescriptionDocumentDataAsync_WithoutDocumentId_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConnectorDeleteDataAsync(new Guid("7e86a0b8-6903-496b-96d1-0ef508206833")).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsConnectorIdExist.Should().BeTrue();
        result.SelfDescriptionDocumentId.Should().BeNull();
        result.DocumentStatusId.Should().BeNull();
    }

    [Fact]
    public async Task GetSelfDescriptionDocumentDataAsync_WithDocumentId_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConnectorDeleteDataAsync(new Guid("7e86a0b8-6903-496b-96d1-0ef508206839")).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsConnectorIdExist.Should().BeTrue();
        result.SelfDescriptionDocumentId.Should().Be(new Guid("e020787d-1e04-4c0b-9c06-bd1cd44724b3"));
        result.DocumentStatusId.Should().Be(DocumentStatusId.LOCKED);
    }

    [Fact]
    public async Task GetSelfDescriptionDocumentDataAsync_WithoutExistingConnectorId_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConnectorDeleteDataAsync(new Guid()).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsConnectorIdExist.Should().BeFalse();
        result.SelfDescriptionDocumentId.Should().BeNull();
        result.DocumentStatusId.Should().BeNull();
    }

    #endregion

    #region CreateConnectorClientDetails

    [Fact]
    public async Task CreateConnectorClientDetails_ReturnsExpected()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.CreateConnectorClientDetails(new Guid("ca7259eb-a3a3-4cc6-9e53-463bf0700af5"), "12345");

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<ConnectorClientDetail>().Which.ClientId.Should().Be("12345");
    }

    #endregion

    #region DeleteConnectorClientDetails

    [Fact]
    public async Task DeleteConnectorClientDetails_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.DeleteConnectorClientDetails(new Guid("f032a035-d035-11ec-9d64-0242ac120002"));

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var entry = changedEntries.Single();
        entry.Entity.Should().BeOfType<ConnectorClientDetail>();
        entry.State.Should().Be(EntityState.Deleted);
    }

    #endregion

    #region GetManagedConnectorsForIamUser

    [Fact]
    public async Task GetManagedConnectorsForIamUser_ReturnsExpectedAppCount()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetManagedConnectorsForIamUser("502dabcf-01c7-47d9-a88e-0be4279097b5").Invoke(0, 10).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
        result.Data.Should().ContainSingle().Which.Name.Should().Be("Test Connector 3");
    }

    [Fact]
    public async Task GetManagedConnectorsForIamUser_WithoutMatchingUser_ReturnsIsProviderUserFalse()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetManagedConnectorsForIamUser(Guid.NewGuid().ToString()).Invoke(0, 10).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetConnectorUpdateInformation

    [Fact]
    public async Task GetConnectorUpdateInformation_ReturnsExpectedAppCount()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConnectorUpdateInformation(new Guid("7e86a0b8-6903-496b-96d1-0ef508206833"), "502dabcf-01c7-47d9-a88e-0be4279097b5").ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(ConnectorStatusId.PENDING);
        result.Type.Should().Be(ConnectorTypeId.COMPANY_CONNECTOR);
    }

    [Fact]
    public async Task GetConnectorUpdateInformation_WithoutExistingConnector_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConnectorUpdateInformation(Guid.NewGuid(), "502dabcf-01c7-47d9-a88e-0be4279097b5").ConfigureAwait(false);

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
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConnectorEndPointDataAsync(bpns).ToListAsync().ConfigureAwait(false);

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

    private async Task<(ConnectorsRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new ConnectorsRepository(context);
        return (sut, context);
    }
}
