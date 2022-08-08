using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Tests;

public class TestDbFactory : IAsyncLifetime
{
    private readonly TestcontainerDatabase _container;
    
    public TestDbFactory()
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

    public PortalDbContext GetPortalDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<PortalDbContext>();
        optionsBuilder.UseNpgsql(
            _container.ConnectionString,
            x => x.MigrationsAssembly(typeof(PortalDbContextFactory).Assembly.GetName().Name)
                .MigrationsHistoryTable("__efmigrations_history_portal")
        );

        var context =  new PortalDbContext(optionsBuilder.Options);
        context.Database.EnsureCreated();
        //context.Database.Migrate();
        return context;
    }
    
    public async Task InitializeAsync() => await _container.StartAsync();

    public new async Task DisposeAsync() => await _container.DisposeAsync();
}