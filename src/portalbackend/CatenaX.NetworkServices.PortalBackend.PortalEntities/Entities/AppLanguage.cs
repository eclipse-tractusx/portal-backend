using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class AppLanguage
{
    private AppLanguage() 
    {
        LanguageShortName = string.Empty;
    }

    public AppLanguage(Guid appId, string languageShortName)
    {
        AppId = appId;
        LanguageShortName = languageShortName;
    }

    public Guid AppId { get; private set; }

    [StringLength(2, MinimumLength = 2)]
    public string LanguageShortName { get; private set; }

    // Navigation properties
    public virtual App? App { get; private set; }
    public virtual Language? Language { get; private set; }
}
