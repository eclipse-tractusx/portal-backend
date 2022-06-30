using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// Implementation of <see cref="ICompanyAssignedAppsRepository"/> accessing database with EF Core.
public class CompanyAssignedAppsRepository : ICompanyAssignedAppsRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">PortalDb context.</param>
    public CompanyAssignedAppsRepository(PortalDbContext portalDbContext)
    {
        this._context = portalDbContext;
    }

    /// <inheritdoc />
    public async Task UpdateSubscriptionStatusAsync(Guid companyId, Guid appId, AppSubscriptionStatusId statusId)
    {
        var subscription = await this.GetActiveSubscriptionByCompanyAndAppIdAsync(companyId, appId).ConfigureAwait(false);
        if (subscription is null)
        {
            throw new ArgumentException($"There is no active subscription for company '{companyId}' and app '{appId}'", nameof(subscription));
        }

        subscription.AppSubscriptionStatusId = AppSubscriptionStatusId.INACTIVE;
    }

    /// <inheritdoc />
    public void AddCompanyAssignedApp(CompanyAssignedApp companyAssignedApp) =>
        this._context.CompanyAssignedApps.Add(companyAssignedApp);

    /// <inheritdoc />
    public IAsyncEnumerable<(Guid AppId, AppSubscriptionStatusId AppSubscriptionStatus)> GetCompanySubscribedAppSubscriptionStatusesForCompanyUntrackedAsync(Guid companyId) =>
        this._context.CompanyAssignedApps.AsNoTracking()
            .Where(s => s.CompanyId == companyId)
            .Select(s => ((Guid AppId, AppSubscriptionStatusId AppSubscriptionStatus))
                new ValueTuple<Guid, AppSubscriptionStatusId>(s.AppId, s.AppSubscriptionStatusId))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public async Task<CompanyAssignedApp?> FindAsync(Guid companyId, Guid appId) =>
        await this._context.CompanyAssignedApps.FindAsync(companyId, appId);

    /// <inheritdoc />
    public IAsyncEnumerable<AppCompanySubscriptionStatusData> GetCompanyProvidedAppSubscriptionStatusesForUserAsync(Guid companyId) =>
        this._context.CompanyAssignedApps.AsNoTracking()
            .Where(s => s.App!.ProviderCompanyId == companyId)
            .GroupBy(s => s.AppId)
            .Select(g => new AppCompanySubscriptionStatusData
            {
                AppId = g.Key,
                CompanySubscriptionStatuses = g.Select(s => 
                    new CompanySubscriptionStatusData(s.CompanyId,s.AppSubscriptionStatusId))
                    .ToList()
            })
            .ToAsyncEnumerable();

    private async Task<CompanyAssignedApp?> GetActiveSubscriptionByCompanyAndAppIdAsync(Guid companyId, Guid appId)
    {
        return await this._context.CompanyAssignedApps
            .SingleOrDefaultAsync(x => x.CompanyId == companyId && x.AppId == appId && x.AppSubscriptionStatusId == AppSubscriptionStatusId.ACTIVE);
    }
}
