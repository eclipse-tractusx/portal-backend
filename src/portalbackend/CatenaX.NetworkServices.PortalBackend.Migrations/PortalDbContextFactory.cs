using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities;

internal class PortalDbContextFactory : IDesignTimeDbContextFactory<PortalDbContext>
{
    public PortalDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var optionsBuilder = new DbContextOptionsBuilder<PortalDbContext>();
        optionsBuilder.UseNpgsql(
            config.GetConnectionString("PortalDb"),
            x => x.MigrationsAssembly(typeof(PortalDbContextFactory).Assembly.GetName().Name)
                  .MigrationsHistoryTable("__EFMigrationsHistory_portal")
        );

        return new PortalDbContext(optionsBuilder.Options);
    }
}
