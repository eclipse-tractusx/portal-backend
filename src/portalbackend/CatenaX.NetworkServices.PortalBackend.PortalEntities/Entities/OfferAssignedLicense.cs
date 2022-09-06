namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class OfferAssignedLicense
{
    private OfferAssignedLicense() {}

    public OfferAssignedLicense(Guid offerId, Guid offerLicenseId)
    {
        OfferId = offerId;
        OfferLicenseId = offerLicenseId;
    }

    public Guid OfferId { get; private set; }
    public Guid OfferLicenseId { get; private set; }

    // Navigation properties
    public virtual Offer? Offer { get; set; }
    public virtual OfferLicense? OfferLicense { get; set; }
}
