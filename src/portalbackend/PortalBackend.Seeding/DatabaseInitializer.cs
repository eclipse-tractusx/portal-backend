using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Seeding;

public class DatabaseInitializer<TDbContext> : IDatabaseInitializer where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializer<TDbContext>> _logger;

    public DatabaseInitializer(TDbContext dbContext, IServiceProvider serviceProvider, ILogger<DatabaseInitializer<TDbContext>> logger)
    {
        _dbContext = dbContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task InitializeDatabasesAsync(CancellationToken cancellationToken) => 
        InitializeDbAsync(cancellationToken);

    private async Task InitializeDbAsync(CancellationToken cancellationToken)
    {
        if ((await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            _logger.LogInformation("Applying Migrations");
            await _dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        }

        // First create a new scope
        using var scope = _serviceProvider.CreateScope();

        // Then run the initialization in the new scope
        await scope.ServiceProvider.GetRequiredService<DbInitializer<TDbContext>>()
            .InitializeAsync(cancellationToken).ConfigureAwait(false);
    }
}