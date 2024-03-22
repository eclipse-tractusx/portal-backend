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
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="OfferRepository"/>
/// </summary>
public class AppInstanceRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    public AppInstanceRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region CreateAppInstance

    [Fact]
    public async Task CreateAppInstance_CallsExpected()
    {
        // Arrange
        var clientId = new Guid("f032a046-d035-11ec-9d64-0242ac120002");
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.CreateAppInstance(new Guid("5cf74ef8-e0b7-4984-a872-474828beb5d2"), clientId);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<AppInstance>().Which.IamClientId.Should().Be(clientId);
    }

    #endregion

    #region RemoveAppInstance

    [Fact]

    public async Task RemoveAppInstance_Success()
    {
        var appInstanceId = new Guid("b161d570-f6ff-45b4-a077-243f72487af6");
        var (sut, context) = await CreateSutWithContext();

        sut.RemoveAppInstance(appInstanceId);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var entry = changedEntries.Single();
        entry.Entity.Should().BeOfType<AppInstance>();
        entry.State.Should().Be(EntityState.Deleted);
    }

    #endregion

    #region CheckInstanceExistsForOffer

    [Fact]
    public async Task CheckInstanceExistsForOffer_WithExistingAppInstance_ReturnsTrue()
    {
        var offerId = new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007");
        var sut = await CreateSut();

        var result = await sut.CheckInstanceExistsForOffer(offerId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckInstanceExistsForOffer_WithoutExisting_ReturnsFalse()
    {
        var sut = await CreateSut();

        var result = await sut.CheckInstanceExistsForOffer(Guid.NewGuid());

        result.Should().BeFalse();
    }

    #endregion

    #region GetAssignedServiceAccounts

    [Fact]
    public async Task GetAssignedServiceAccounts_WithExistingAppInstance_ReturnsExpected()
    {
        var instanceId = new Guid("ab25c218-9ab3-4f1a-b6f4-6394fbc33c5a");
        var sut = await CreateSut();

        var result = await sut.GetAssignedServiceAccounts(instanceId).ToListAsync();

        result.Should().HaveCount(1)
            .And.ContainSingle().Which.Should().Be(new Guid("7e85a0b8-0001-ab67-10d1-0ef508201006"));
    }

    [Fact]
    public async Task GetAssignedServiceAccounts_WithoutExisting_ReturnsEmpty()
    {
        var sut = await CreateSut();

        var result = await sut.GetAssignedServiceAccounts(Guid.NewGuid()).ToListAsync();

        result.Should().BeEmpty();
    }

    #endregion

    #region CheckInstanceHasAssignedSubscriptions

    [Theory]
    [InlineData("e080bb4b-567b-477e-adcf-080efc457d38", false)]
    [InlineData("ab25c218-9ab3-4f1a-b6f4-6394fbc33c5a", true)]
    public async Task CheckInstanceHasAssignedSubscriptions_WithExistingAppInstance_ReturnsExpected(Guid instanceId, bool expected)
    {
        var sut = await CreateSut();

        var result = await sut.CheckInstanceHasAssignedSubscriptions(instanceId);

        result.Should().Be(expected);
    }

    #endregion

    #region RemoveAppInstance

    [Fact]

    public async Task RemoveAppInstanceAssignedServiceAccounts_Success()
    {
        var appInstanceId = new Guid("ab25c218-9ab3-4f1a-b6f4-6394fbc33c5a");
        var serviceAccountId = new Guid("7e85a0b8-0001-ab67-10d1-0ef508201006");
        var (sut, context) = await CreateSutWithContext();

        sut.RemoveAppInstanceAssignedServiceAccounts(appInstanceId, Enumerable.Repeat(serviceAccountId, 1));

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var entry = changedEntries.Single();
        entry.Entity.Should().BeOfType<AppInstanceAssignedCompanyServiceAccount>();
        entry.State.Should().Be(EntityState.Deleted);
    }

    #endregion

    #region Setup

    private async Task<(AppInstanceRepository repo, PortalDbContext context)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        var sut = new AppInstanceRepository(context);
        return (sut, context);
    }

    private async Task<AppInstanceRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        var sut = new AppInstanceRepository(context);
        return sut;
    }

    #endregion
}
