using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
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

    /// <summary>
    /// Adds the given company assigned app to the database
    /// </summary>
    /// <param name="companyAssignedApp">The company assigned app that should be added to the database</param>
    void AddCompanyAssignedApp(CompanyAssignedApp companyAssignedApp);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="companyId"></param>
    IAsyncEnumerable<(Guid AppId, AppSubscriptionStatusId AppSubscriptionStatus)> GetCompanySubscribedAppSubscriptionStatusesForCompanyUntrackedAsync(Guid companyId);

    /// <summary>
    /// Finds the company assigned app with the company id and app id
    /// </summary>
    /// <param name="companyId">id of the company</param>
    /// <param name="appId">id of the app</param>
    /// <returns>Returns the found app or null</returns>
    Task<CompanyAssignedApp?> FindAsync(Guid companyId, Guid appId);

    /// <summary>
    /// Gets the provided app subscription statuses for the user and given company
    /// </summary>
    /// <param name="companyId">Id of the company</param>
    /// <returns>Returns a IAsyncEnumerable of the found <see cref="AppCompanySubscriptionStatusData"/></returns>
    IAsyncEnumerable<AppCompanySubscriptionStatusData> GetCompanyProvidedAppSubscriptionStatusesForUserAsync(Guid companyId);
}
