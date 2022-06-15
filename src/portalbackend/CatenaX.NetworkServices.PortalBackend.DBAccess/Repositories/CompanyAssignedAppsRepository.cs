using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// Implementation of <see cref="ICompanyAssignedAppsRepository"/> accessing database with EF Core.
public class CompanyAssignedAppsRepository : ICompanyAssignedAppsRepository
{
    private readonly PortalDbContext context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">PortalDb context.</param>
    public CompanyAssignedAppsRepository(PortalDbContext portalDbContext)
    {
        this.context = portalDbContext;
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
        await this.context.SaveChangesAsync().ConfigureAwait(false);
    }
    
    private async Task<CompanyAssignedApp?> GetActiveSubscriptionByCompanyAndAppIdAsync(Guid companyId, Guid appId)
    {
        return await this.context.CompanyAssignedApps
            .SingleOrDefaultAsync(x => x.CompanyId == companyId && x.AppId == appId && x.AppSubscriptionStatusId == AppSubscriptionStatusId.ACTIVE);
    }
}
