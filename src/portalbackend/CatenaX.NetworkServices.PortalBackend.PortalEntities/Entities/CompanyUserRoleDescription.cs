using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyUserRoleDescription
{
    private CompanyUserRoleDescription()
    {
        CompanyUserRoleId = default!;
        LanguageShortName = null!;
        Description = null!;
    }

    public CompanyUserRoleDescription(Guid companyUserRoleId, string languageShortName, string description)
    {
        CompanyUserRoleId = companyUserRoleId;
        LanguageShortName = languageShortName;
        Description = description;
    }

    public Guid CompanyUserRoleId { get; private set; }

    [StringLength(2, MinimumLength = 2)]
    public string LanguageShortName { get; private set; }

    public string Description { get; set; }

    public virtual CompanyUserRole? CompanyUserRole { get; private set; }
    public virtual Language? Language { get; private set; }
}
