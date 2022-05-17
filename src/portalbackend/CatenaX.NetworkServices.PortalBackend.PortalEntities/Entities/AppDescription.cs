using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class AppDescription
{
    private AppDescription()
    {
        LanguageShortName = null!;
        DescriptionLong = null!;
        DescriptionShort = null!;
    }

    public AppDescription(Guid appId, string languageShortName, string descriptionLong, string descriptionShort)
    {
        AppId = appId;
        LanguageShortName = languageShortName;
        DescriptionLong = descriptionLong;
        DescriptionShort = descriptionShort;
    }
    
    [MaxLength(4096)]
    public string DescriptionLong { get; set; }

    [MaxLength(255)]
    public string DescriptionShort { get; set; }

    public Guid AppId { get; private set; }

    [StringLength(2, MinimumLength = 2)]
    public string LanguageShortName { get; private set; }

    // Navigation properties
    public virtual App? App { get; private set; }
    public virtual Language? Language { get; private set; }
}
