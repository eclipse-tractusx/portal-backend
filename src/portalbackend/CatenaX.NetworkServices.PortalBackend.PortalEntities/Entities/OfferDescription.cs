using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class OfferDescription
{
    private OfferDescription()
    {
        LanguageShortName = null!;
        DescriptionLong = null!;
        DescriptionShort = null!;
    }

    public OfferDescription(Guid offerId, string languageShortName, string descriptionLong, string descriptionShort)
    {
        OfferId = offerId;
        LanguageShortName = languageShortName;
        DescriptionLong = descriptionLong;
        DescriptionShort = descriptionShort;
    }
    
    [MaxLength(4096)]
    public string DescriptionLong { get; set; }

    [MaxLength(255)]
    public string DescriptionShort { get; set; }

    public Guid OfferId { get; private set; }

    [StringLength(2, MinimumLength = 2)]
    public string LanguageShortName { get; private set; }

    // Navigation properties
    public virtual Offer? Offer { get; private set; }
    public virtual Language? Language { get; private set; }
}
