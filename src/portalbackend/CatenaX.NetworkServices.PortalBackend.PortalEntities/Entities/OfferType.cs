using System.ComponentModel.DataAnnotations;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class OfferType
{
    private OfferType()
    {
        Label = null!;
        Offers = new HashSet<Offer>();
    }

    public OfferType(OfferTypeId offerTypeId) : this()
    {
        Id = offerTypeId;
        Label = offerTypeId.ToString();
    }

    public OfferTypeId Id { get; private set; }

    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<Offer> Offers { get; private set; }
}