namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class AppInstance
{
    private AppInstance()
    {
        AppSubscriptionDetails = new HashSet<AppSubscriptionDetail>();
    }

    public AppInstance(Guid id, Guid appId, Guid iamClientId) : this()
    {
        Id = id;
        AppId = appId;
        IamClientId = iamClientId;
    }

    public Guid Id { get; set; }
    public Guid AppId { get; private set; }
    public Guid IamClientId { get; private set; }

    // Navigation properties
    public virtual Offer? App { get; private set; }
    public virtual IamClient? IamClient { get; private set; }
    public virtual ICollection<AppSubscriptionDetail> AppSubscriptionDetails { get; private set; }
}
