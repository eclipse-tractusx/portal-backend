namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

/// <summary>
/// View model containing an app id and connected company subscription statuses.
/// </summary>
public class AppCompanySubscriptionStatusData
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public AppCompanySubscriptionStatusData()
    {
        CompanySubscriptionStatuses = new HashSet<CompanySubscriptionStatusData>();
    }

    /// <summary>
    /// Id of the app.
    /// </summary>
    public Guid AppId { get; set; }

    /// <summary>
    /// Subscription statuses of subscribing companies.
    /// </summary>
    public ICollection<CompanySubscriptionStatusData> CompanySubscriptionStatuses { get; set; }
}
