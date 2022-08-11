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
        _context =  new PortalDbContext(optionsBuilder.Options);
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