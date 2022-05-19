using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities;

internal class PortalDbContextFactory : IDesignTimeDbContextFactory<PortalDbContext>
{
    public PortalDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();
        var optionsBuilder = new DbContextOptionsBuilder<PortalDbContext>();
        optionsBuilder.UseNpgsql(
            config.GetConnectionString("PortalDb"),
            x => x.MigrationsAssembly(typeof(PortalDbContextFactory).Assembly.GetName().Name)
                  .MigrationsHistoryTable("__efmigrations_history_portal")
        );

        return new PortalDbContext(optionsBuilder.Options);
    }
}
