namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class OfferSubscriptionProcessData
{
    private OfferSubscriptionProcessData()
    {
        OfferUrl = null!;
    }

    public OfferSubscriptionProcessData(Guid offerSubscriptionId, string offerUrl)
    {
        OfferSubscriptionId = offerSubscriptionId;
        OfferUrl = offerUrl;
    }

    public Guid OfferSubscriptionId { get; set; }

    public string OfferUrl { get; set; }

    public virtual OfferSubscription? OfferSubscription { get; private set; }
}
