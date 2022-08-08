using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CatenaX.NetworkServices.Tests.Shared.IntegrationTests;

public static class IntegrationTestFactoryExtensions
{
    public static void SeedDatabase<TProgram, TDbContext>(this IntegrationTestFactory<TProgram, TDbContext> factory, Action<TDbContext> seedAction)
        where TProgram : class 
        where TDbContext : DbContext
    {
        var scopeFactory = factory.Services.GetService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetService<TDbContext>();
        seedAction.Invoke(context);
    }
}