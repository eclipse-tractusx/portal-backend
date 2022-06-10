using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Value table for app subscription statuses.
/// </summary>
public class AppSubscriptionStatus
{
    /// <summary>
    /// Constructor.
    /// </summary>
    private AppSubscriptionStatus()
    {
        Label = null!;
        AppSubscriptions = new HashSet<CompanyAssignedApp>();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="appSubscriptionStatusId">Id of the subscription to wrap into entity.</param>
    public AppSubscriptionStatus(AppSubscriptionStatusId appSubscriptionStatusId) : this()
    {
        Id = appSubscriptionStatusId;
        Label = appSubscriptionStatusId.ToString();
    }

    /// <summary>
    /// Id of the subscription status.
    /// </summary>
    public AppSubscriptionStatusId Id { get; private set; }

    /// <summary>
    /// Label of the subscription status.
    /// </summary>
    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties

    /// <summary>
    /// All AppSubscriptions currently with this status.
    /// </summary>
    public virtual ICollection<CompanyAssignedApp> AppSubscriptions { get; private set; }
}
