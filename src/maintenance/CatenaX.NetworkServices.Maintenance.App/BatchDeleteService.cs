using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.Maintenance.App;

/// <summary>
/// Service to delete the pending and inactive documents as well as the depending consents from the database
/// </summary>
public class BatchDeleteService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<BatchDeleteService> _logger;
    private readonly int _days;

    /// <summary>
    /// Creates a new instance of <see cref="BatchDeleteService"/>
    /// </summary>
    /// <param name="serviceScopeFactory">access to the services</param>
    /// <param name="logger">the logger</param>
    /// <param name="config">the apps configuration</param>
    public BatchDeleteService(IServiceScopeFactory serviceScopeFactory, ILogger<BatchDeleteService> logger, IConfiguration config)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _days = config.GetValue<int>("DeleteIntervalInDays");
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PortalDbContext>();
            
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            _logger.LogInformation("Cleaning up consents...");
            dbContext.Database.ExecuteSqlRaw($"DELETE FROM portal.consents WHERE document_id IN (SELECT id FROM portal.documents where date_created < now() - interval '{_days} days' and (document_status_id = 1 or document_status_id = 3))");
            _logger.LogInformation("Cleaning up documents...");
            dbContext.Database.ExecuteSqlRaw($"DELETE FROM portal.documents where date_created < now() - interval '{_days} days' and (document_status_id = 1 or document_status_id = 3)");
            transaction.Commit();
            _logger.LogInformation($"Documents older than {_days} days and depending consents successfully cleaned up.");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError($"Database clean up failed with error: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}