using Microsoft.EntityFrameworkCore;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Seeding;

internal class DbInitializer<TDbContext> where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly DbSeeder _dbSeeder;

    public DbInitializer(TDbContext dbContext, DbSeeder dbSeeder)
    {
        _dbContext = dbContext;
        _dbSeeder = dbSeeder;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.Database.GetMigrations().Any() && await _dbContext.Database.CanConnectAsync(cancellationToken))
        {
            await _dbSeeder.SeedDatabaseAsync(cancellationToken);
        }
    }
}
