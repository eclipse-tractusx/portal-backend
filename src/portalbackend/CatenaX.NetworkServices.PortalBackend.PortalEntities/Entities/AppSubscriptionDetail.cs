using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Detail data for the app subscription
/// </summary>
public class AppSubscriptionDetail
{
    /// <summary>
    /// Only needed for ef
    /// </summary>
    private AppSubscriptionDetail()
    {
        OfferSubscriptions = new HashSet<OfferSubscription>();
    }

    /// <summary>
    /// Creates a new instance of <see cref="AppSubscriptionDetail"/>
    /// </summary>
    /// <param name="id">Id of the entity</param>
    public AppSubscriptionDetail(Guid id) 
        : this()
    {
        this.Id = id;
    }
    
    /// <summary>
    /// Id of the entity
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the app instance.
    /// </summary>
    public Guid? AppInstanceId { get; set; }

    [MaxLength(255)]
    public string? AppSubscriptionUrl { get; set; }

    /// <summary>
    /// Subscribing app instance.
    /// </summary>
    public virtual AppInstance? AppInstance { get; private set; }

    public virtual ICollection<OfferSubscription> OfferSubscriptions { get; private set; }
}