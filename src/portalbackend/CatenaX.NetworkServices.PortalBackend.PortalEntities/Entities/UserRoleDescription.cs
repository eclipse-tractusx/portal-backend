using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class UserRoleDescription
{
    private UserRoleDescription()
    {
        UserRoleId = default!;
        LanguageShortName = null!;
        Description = null!;
    }

    public UserRoleDescription(Guid userRoleId, string languageShortName, string description)
    {
        UserRoleId = userRoleId;
        LanguageShortName = languageShortName;
        Description = description;
    }

    public Guid UserRoleId { get; private set; }

    [StringLength(2, MinimumLength = 2)]
    public string LanguageShortName { get; private set; }

    [MaxLength(255)]
    public string Description { get; set; }

    public virtual UserRole? UserRole { get; private set; }
    public virtual Language? Language { get; private set; }
}
