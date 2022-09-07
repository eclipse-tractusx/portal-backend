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

    /// <summary>
    /// construtor used for the Attach case
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="languageShortName"></param>
    public OfferDescription(Guid appId, string languageShortName)
    {
        OfferId = appId;
        LanguageShortName = languageShortName;
        DescriptionLong = null!;
        DescriptionShort = null!;
    }
    
    /// <summary>
    /// construtor used for the Remove case
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="languageShortName"></param>
    /// <param name="descriptionLong"></param>
    public OfferDescription(Guid appId, string languageShortName, string descriptionLong)
    {
        OfferId = appId;
        LanguageShortName = languageShortName;
        DescriptionLong = descriptionLong;
        DescriptionShort = null!;
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
