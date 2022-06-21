using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for accessing company assigned apps on persistence layer.
/// </summary>
public interface ICompanyAssignedAppsRepository
{
    /// <summary>
    /// Updates a CompanyAssignedApp for the given company and app id to the given status from the persistence layer.
    /// </summary>
    /// <param name="companyId">Id of the company.</param>
    /// <param name="appId">Id of the app.</param>
    /// <param name="statusId">The new status of the subscription.</param>
    public Task UpdateSubscriptionStatusAsync(Guid companyId, Guid appId, AppSubscriptionStatusId statusId);
}
