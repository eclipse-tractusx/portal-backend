using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyRoleDescription
{
    private CompanyRoleDescription()
    {
        CompanyRoleId = default!;
        LanguageShortName = null!;
        Description = null!;
    }

    public CompanyRoleDescription(CompanyRoleId companyRoleId, string languageShortName, string description)
    {
        CompanyRoleId = companyRoleId;
        LanguageShortName = languageShortName;
        Description = description;
    }

    public CompanyRoleId CompanyRoleId { get; private set; }

    [StringLength(2, MinimumLength = 2)]
    public string LanguageShortName { get; private set; }

    [MaxLength(255)]
    public string Description { get; set; }

    public virtual CompanyRole? CompanyRole { get; private set; }
    public virtual Language? Language { get; private set; }
}
