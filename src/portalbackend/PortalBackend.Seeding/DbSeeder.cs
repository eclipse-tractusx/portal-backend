using Microsoft.Extensions.Logging;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Seeding;

internal class DbSeeder
{
    private readonly CustomSeederRunner _seederRunner;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(CustomSeederRunner seederRunner, ILogger<DbSeeder> logger)
    {
        _seederRunner = seederRunner;
        _logger = logger;
    }

    public async Task SeedDatabaseAsync(CancellationToken cancellationToken)
    {
        // Theoretically we could run not generic code in here, that would mean we have to adjust the code from generic to custom code
        _logger.LogInformation("Run custom seeder");
        await _seederRunner.RunSeedersAsync(cancellationToken);
    }
}