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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class NetworkRepositoryTests
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _validCompanyId = new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");

    public NetworkRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region CreateNetworkRegistration

    [Fact]
    public async Task CreateNetworkRegistration_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);
        var externalId = Guid.NewGuid();
        var processId = new Guid("0cc208c3-bdf6-456c-af81-6c3ebe14fe07");

        // Act
        var results = sut.CreateNetworkRegistration(externalId, _validCompanyId, processId);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        results.CompanyId.Should().Be(_validCompanyId);
        results.ProcessId.Should().Be(processId);
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<NetworkRegistration>();
        var networkRegistration = changedEntries.Single().Entity as NetworkRegistration;
        networkRegistration!.CompanyId.Should().Be(_validCompanyId);
        networkRegistration.ProcessId.Should().Be(processId);
    }

    #endregion

    #region CheckExternalIdExists

    [Fact]
    public async Task CheckExternalIdExists_WithValid_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.CheckExternalIdExists(new Guid("c5547c9a-6ace-4ab7-9253-af65a66278f2")).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckExternalIdExists_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.CheckExternalIdExists(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetNetworkRegistrationDataForProcessIdAsync

    [Fact]
    public async Task GetNetworkRegistrationDataForProcessIdAsync_WithValid_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetNetworkRegistrationDataForProcessIdAsync(new Guid("0cc208c3-bdf6-456c-af81-6c3ebe14fe07")).ConfigureAwait(false);

        // Assert
        result.Should().Be(new Guid("67ace0a9-b6df-438b-935a-fe858b8598dd"));
    }

    #endregion

    #region Setup

    private async Task<(NetworkRepository sut, PortalDbContext context)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new NetworkRepository(context);
        return (sut, context);
    }
    
    private async Task<NetworkRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new NetworkRepository(context);
        return sut;
    }

    #endregion
}
