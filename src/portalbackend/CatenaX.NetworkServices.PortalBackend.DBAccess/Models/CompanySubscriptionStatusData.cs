using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

/// <summary>
/// View model containing the ID of a company and its app subscription status in a specific context.
/// </summary>
public record CompanySubscriptionStatusData(Guid CompanyId, AppSubscriptionStatusId AppSubscriptionStatus)
{
    /// <summary>
    /// Id of the company.
    /// </summary>
    public Guid CompanyId { get; } = CompanyId;

    /// <summary>
    /// Subscription status of the company.
    /// </summary>
    public AppSubscriptionStatusId AppSubscriptionStatus { get; } = AppSubscriptionStatus;
}
