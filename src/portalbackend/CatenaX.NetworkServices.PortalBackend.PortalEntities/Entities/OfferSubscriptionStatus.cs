using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Value table for app subscription statuses.
/// </summary>
public class OfferSubscriptionStatus
{
    /// <summary>
    /// Constructor.
    /// </summary>
    private OfferSubscriptionStatus()
    {
        Label = null!;
        OfferSubscriptions = new HashSet<OfferSubscription>();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="offerSubscriptionStatusId">Id of the subscription to wrap into entity.</param>
    public OfferSubscriptionStatus(OfferSubscriptionStatusId offerSubscriptionStatusId) : this()
    {
        Id = offerSubscriptionStatusId;
        Label = offerSubscriptionStatusId.ToString();
    }

    /// <summary>
    /// Id of the subscription status.
    /// </summary>
    public OfferSubscriptionStatusId Id { get; private set; }

    /// <summary>
    /// Label of the subscription status.
    /// </summary>
    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties

    /// <summary>
    /// All AppSubscriptions currently with this status.
    /// </summary>
    public virtual ICollection<OfferSubscription> OfferSubscriptions { get; private set; }
}
