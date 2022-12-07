/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using Castle.Core.Logging;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.TestSeeds;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
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
        sut.AttachAndModifyConnector(new Guid("5aea3711-cc54-47b4-b7eb-ba9f3bf1cb15"), con =>
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
        var result = await sut.GetConnectorByIdForIamUser(new Guid("5aea3711-cc54-47b4-b7eb-ba9f3bf1cb15"), "623770c5-cf38-4b9f-9a35-f8b9ae972e2e").ConfigureAwait(false);

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
        var result = await sut.GetConnectorByIdForIamUser(Guid.NewGuid(), "623770c5-cf38-4b9f-9a35-f8b9ae972e2e").ConfigureAwait(false);

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
        var result = await sut.GetConnectorInformationByIdForIamUser(new Guid("5aea3711-cc54-47b4-b7eb-ba9f3bf1cb15"), "623770c5-cf38-4b9f-9a35-f8b9ae972e2e").ConfigureAwait(false);

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

    private async Task<(ConnectorsRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new ConnectorsRepository(context);
        return (sut, context);
    }
}
