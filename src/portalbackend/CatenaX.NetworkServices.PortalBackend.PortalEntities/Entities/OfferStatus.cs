using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class OfferStatus
{
    private OfferStatus()
    {
        Label = null!;
        Offers = new HashSet<Offer>();
    }

    public OfferStatus(OfferStatusId offerStatusId) : this()
    {
        Id = offerStatusId;
        Label = offerStatusId.ToString();
    }

    public OfferStatusId Id { get; private set; }

    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<Offer> Offers { get; private set; }
}
