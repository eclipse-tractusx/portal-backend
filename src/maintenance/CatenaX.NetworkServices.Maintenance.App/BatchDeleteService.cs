using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.Maintenance.App;

/// <summary>
/// Service to delete the pending and inactive documents as well as the depending consents from the database
/// </summary>
public class BatchDeleteService : BackgroundService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<BatchDeleteService> _logger;
    private readonly int _days;

    /// <summary>
    /// Creates a new instance of <see cref="BatchDeleteService"/>
    /// </summary>
    /// <param name="applicationLifetime">Application lifetime</param>
    /// <param name="serviceScopeFactory">access to the services</param>
    /// <param name="logger">the logger</param>
    /// <param name="config">the apps configuration</param>
    public BatchDeleteService(
        IHostApplicationLifetime applicationLifetime,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<BatchDeleteService> logger,
        IConfiguration config)
    {
        _applicationLifetime = applicationLifetime;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _days = config.GetValue<int>("DeleteIntervalInDays");
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PortalDbContext>();
            
        if (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation($"Cleaning up documents and consents older {_days} days...");
                await dbContext.Database.ExecuteSqlInterpolatedAsync($"WITH documentids AS (DELETE FROM portal.documents WHERE date_created < {DateTimeOffset.UtcNow.AddDays(-_days)} AND (document_status_id = {(int)DocumentStatusId.PENDING} OR document_status_id = {(int) DocumentStatusId.INACTIVE}) RETURNING id) DELETE FROM portal.consents WHERE document_id IN (SELECT id FROM documentids);", stoppingToken).ConfigureAwait(false);
                _logger.LogInformation($"Documents older than {_days} days and depending consents successfully cleaned up.");
            }
            catch (Exception ex)
            {
                Environment.ExitCode = 1;
                _logger.LogError($"Database clean up failed with error: {ex.Message}");
            }
        }

        _applicationLifetime.StopApplication();
    }
}
