/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.Tests.Shared.TestSeeds;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatenaX.NetworkServices.Tests.Shared.DatabaseRelatedTests;

public class TestDbFixture : IAsyncLifetime
{
    private PortalDbContext _context = null!;
    private readonly PostgreSqlTestcontainer _container;

    public TestDbFixture()
    {
        _container = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "test_db",
                Username = "postgres",
                Password = "postgres",
            })
            .WithImage("postgres")
            .WithCleanUp(true)
            .WithName(Guid.NewGuid().ToString())
            .Build();
    }

    public void Seed(params Action<PortalDbContext>[] seedActions)
    {
        foreach (var seedAction in seedActions)
        {
            seedAction.Invoke(_context);
        }

        _context.SaveChanges();
    }

    public PortalDbContext GetPortalDbContext()
    {
        return _context;
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await _container.StartAsync()
            .ConfigureAwait(false);

        var optionsBuilder = new DbContextOptionsBuilder<PortalDbContext>();
        
        optionsBuilder.UseNpgsql(
            _container.ConnectionString,
            x => x.MigrationsAssembly(typeof(PortalDbContextFactory).Assembly.GetName().Name)
                .MigrationsHistoryTable("__efmigrations_history_portal")
        );
        _context = new PortalDbContext(optionsBuilder.Options);
        await _context.Database.MigrateAsync();
        BaseSeed.SeedBasedata().Invoke(_context);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync()
            .ConfigureAwait(false);
    }
}