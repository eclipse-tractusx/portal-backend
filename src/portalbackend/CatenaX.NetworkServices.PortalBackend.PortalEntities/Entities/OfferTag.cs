using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class OfferTag
{
    private OfferTag()
    {
        Name = null!;
    }

    public OfferTag(Guid offerId, string name): this()
    {
        OfferId = offerId;
        Name = name;
    }

    public Guid OfferId { get; set; }

    [MaxLength(255)]
    [Column("tag_name")]
    public string Name { get; set; }

    // Navigation properties
    public virtual Offer? Offer { get; set; }
}
