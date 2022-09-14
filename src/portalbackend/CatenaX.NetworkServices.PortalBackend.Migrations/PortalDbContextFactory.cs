using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Laraue.EfCoreTriggers.PostgreSql.Extensions;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities;

public class PortalDbContextFactory : IDesignTimeDbContextFactory<PortalDbContext>
{
    public PortalDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("secrets/appsettings.json", true) // Only used in k8s deployment
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();
        var optionsBuilder = new DbContextOptionsBuilder<PortalDbContext>();
        optionsBuilder.UseNpgsql(
            config.GetConnectionString("PortalDb"),
            x => x.MigrationsAssembly(typeof(PortalDbContextFactory).Assembly.GetName().Name)
                  .MigrationsHistoryTable("__efmigrations_history_portal")
        ).UsePostgreSqlTriggers();

        return new PortalDbContext(optionsBuilder.Options);
    }
}
