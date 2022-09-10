using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class OfferDetailImage
{
    private OfferDetailImage()
    {
        ImageUrl = null!;
    }

    public OfferDetailImage(Guid id, Guid offerId, string imageUrl)
    {
        Id = id;
        OfferId = offerId;
        ImageUrl = imageUrl;
    }

    public Guid Id { get; private set; }

    public Guid OfferId { get; set; }

    [MaxLength(255)]
    public string ImageUrl { get; set; }

    // Navigation properties
    public virtual Offer? Offer { get; set; }
}
