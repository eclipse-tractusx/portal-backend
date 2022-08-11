using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.Tests.Shared.TestSeeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CatenaX.NetworkServices.Tests.Shared.IntegrationTests;

public static class ServiceCollectionExtensions
{
    public static void RemoveProdDbContext<T>(this IServiceCollection services) where T : DbContext
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<T>));
        if (descriptor != null) services.Remove(descriptor);
    }

    public static void EnsureDbCreated(this IServiceCollection services, IList<Action<PortalDbContext>>? setupDbActions) 
    {
        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var context = scopedServices.GetRequiredService<PortalDbContext>();
        context.Database.Migrate();
        BaseSeed.SeedBasedata().Invoke(context);
        if (setupDbActions is not null && setupDbActions.Any())
        {
            foreach (var setupAction in setupDbActions)
            {
                setupAction?.Invoke(context);
            }
        }

        context.SaveChanges();
    }
}