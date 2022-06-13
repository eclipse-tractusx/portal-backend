using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.App.Service.ViewModels;

/// <summary>
/// View model containing the ID of an app and its subscription status in a specific context.
/// </summary>
public class AppSubscriptionStatusViewModel
{
    /// <summary>
    /// Id of the app.
    /// </summary>
    public Guid AppId { get; set; }

    /// <summary>
    /// Subscription status of the app.
    /// </summary>
    public AppSubscriptionStatusId AppSubscriptionStatus { get; set; }
}
