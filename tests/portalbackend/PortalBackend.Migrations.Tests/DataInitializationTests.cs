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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Tests;

/// <summary>
/// This class is only to test if the setup data are correctly set
/// </summary>
public class DataInitializationTests : IClassFixture<ConsortiaDataDbFixture>
{
    private readonly ConsortiaDataDbFixture _dbTestDbFixture;

    public DataInitializationTests(ConsortiaDataDbFixture testDbFixture)
    {
        _dbTestDbFixture = testDbFixture;
    }

    [Fact]
    public async Task TestDataInitialization_EnsureCreated()
    {
        // Arrange
        var context = _dbTestDbFixture.GetPortalDbContext();
        
        // Act
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync().ConfigureAwait(false);
        
        // Assert
        pendingMigrations.Should().BeEmpty();
    }
}