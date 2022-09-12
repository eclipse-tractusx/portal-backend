using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class OfferDetailImage
{
    public OfferDetailImage(Guid id, Guid appId, string imageUrl)
    {
        Id = id;
        OfferId = appId;
        ImageUrl = imageUrl;
    }

    public OfferDetailImage(Guid id)
    {
        Id = id;
        OfferId = Guid.Empty;
        ImageUrl = null!;
    }

    public Guid Id { get; private set; }

    public Guid OfferId { get; set; }

    [MaxLength(255)]
    public string ImageUrl { get; set; }

    // Navigation properties
    public virtual Offer? Offer { get; set; }
}
