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

    public static void EnsureDbCreated<TDbContext>(this IServiceCollection services, Action<TDbContext>? setupDbContext) 
        where TDbContext : DbContext
    {
        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var context = scopedServices.GetRequiredService<TDbContext>();
        context.Database.Migrate();
        setupDbContext?.Invoke(context);
    }
}