using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CatenaX.NetworkServices.Tests.Shared.IntegrationTests;

public class IntegrationTestFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class 
{
    private readonly TestcontainerDatabase _container;
    public IList<Action<PortalDbContext>>? SetupDbActions { get; set; }

    public IntegrationTestFactory()
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

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveProdDbContext<PortalDbContext>();
            services.AddDbContext<PortalDbContext>(options =>
            {
                options.UseNpgsql(_container.ConnectionString,
                    x => x.MigrationsAssembly(typeof(PortalDbContextFactory).Assembly.GetName().Name)
                        .MigrationsHistoryTable("__efmigrations_history_portal"));
            });
            services.EnsureDbCreated(SetupDbActions);
            services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
        });
    }

    public async Task InitializeAsync() => await _container.StartAsync();

    public new async Task DisposeAsync() => await _container.DisposeAsync();
}