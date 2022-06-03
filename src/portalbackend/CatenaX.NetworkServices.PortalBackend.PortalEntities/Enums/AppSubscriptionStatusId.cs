namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

/// <summary>
/// Status values for app subscriptions.
/// </summary>
public enum AppSubscriptionStatusId
{
    /// <summary>
    /// App subscription not yet confirmed.
    /// </summary>
    PENDING = 1,

    /// <summary>
    /// App subscription active.
    /// </summary>
    ACTIVE = 2,

    /// <summary>
    /// App subscription declared to be ended.
    /// </summary>
    INACTIVE = 3
}
