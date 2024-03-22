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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="OfferRepository"/>
/// </summary>
public class ClientRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    public ClientRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region CreateClient

    [Fact]
    public async Task CreateClient_ReturnsExpectedResult()

    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();

        // Act
        var result = sut.CreateClient("test");

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        result.ClientClientId.Should().Be("test");
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<IamClient>().Which.ClientClientId.Should().Be("test");
    }

    #endregion

    #region RemoveClient

    [Fact]
    public async Task RemoveClient_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.RemoveClient(new Guid("f032a035-d035-11ec-9d64-0242ac120002"));

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var entry = changedEntries.Single();
        entry.Entity.Should().BeOfType<IamClient>();
        entry.State.Should().Be(EntityState.Deleted);
    }

    #endregion

    #region Setup

    private async Task<(ClientRepository repo, PortalDbContext context)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        var sut = new ClientRepository(context);
        return (sut, context);
    }

    #endregion
}
