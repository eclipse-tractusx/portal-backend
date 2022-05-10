using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyUserRole
{
    private CompanyUserRole()
    {
        CompanyUserRoleText = null!;
        Namede = null!;
        Nameen = null!;
        Apps = new HashSet<App>();
        CompanyUsers = new HashSet<CompanyUser>();
    }

    public CompanyUserRole(Guid id, string companyUserRoleText, string namede, string nameen) : this()
    {
        Id = id;
        CompanyUserRoleText = companyUserRoleText;
        Namede = namede;
        Nameen = nameen;
    }

    [Key]
    public Guid Id { get; private set; }

    [MaxLength(255)]
    [Column("company_user_role")]
    public string CompanyUserRoleText { get; set; }

    [MaxLength(255)]
    public string Namede { get; set; }

    [MaxLength(255)]
    public string Nameen { get; set; }

    // Navigation properties
    public virtual ICollection<App> Apps { get; private set; }
    public virtual ICollection<CompanyUser> CompanyUsers { get; private set; }
}
