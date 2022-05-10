using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyUserRole
{
    private CompanyUserRole()
    {
        CompanyUserRoleText = null!;
        Apps = new HashSet<App>();
        CompanyUsers = new HashSet<CompanyUser>();
        CompanyUserRoleDescriptions = new HashSet<CompanyUserRoleDescription>();
    }

    public CompanyUserRole(Guid id, string companyUserRoleText, Guid iamClientId) : this()
    {
        Id = id;
        CompanyUserRoleText = companyUserRoleText;
        IamClientId = iamClientId;
    }

    [Key]
    public Guid Id { get; private set; }

    [MaxLength(255)]
    [Column("company_user_role")]
    public string CompanyUserRoleText { get; set; }

    public Guid IamClientId { get; set; }

    // Navigation properties
    public virtual IamClient? IamClient { get; set; }
    public virtual ICollection<App> Apps { get; private set; }
    public virtual ICollection<CompanyUser> CompanyUsers { get; private set; }
    public virtual ICollection<CompanyUserRoleDescription> CompanyUserRoleDescriptions { get; private set; }
}
