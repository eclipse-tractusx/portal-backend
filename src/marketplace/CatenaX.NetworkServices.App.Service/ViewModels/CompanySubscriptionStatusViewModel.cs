using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.App.Service.ViewModels;

/// <summary>
/// View model containing the ID of a company and its app subscription status in a specific context.
/// </summary>
public class CompanySubscriptionStatusViewModel
{
    /// <summary>
    /// Id of the company.
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Subscription status of the company.
    /// </summary>
    public AppSubscriptionStatusId AppSubscriptionStatus { get; set; }
}
