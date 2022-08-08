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

public class IntegrationTestFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class 
    where TDbContext : DbContext
{
    private readonly TestcontainerDatabase _container;
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
            services.RemoveProdDbContext<TDbContext>();
            services.AddDbContext<TDbContext>(options => { options.UseNpgsql(_container.ConnectionString); });
            services.EnsureDbCreated<TDbContext>();
            services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();

            var serviceProvider = services.BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<TDbContext>();
            context.Database.Migrate();
        });
    }

    public async Task InitializeAsync() => await _container.StartAsync();

    public new async Task DisposeAsync() => await _container.DisposeAsync();
}