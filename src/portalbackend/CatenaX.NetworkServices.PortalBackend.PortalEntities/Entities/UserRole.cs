using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class UserRole
{
    private UserRole()
    {
        UserRoleText = null!;
        CompanyUsers = new HashSet<CompanyUser>();
        UserRoleDescriptions = new HashSet<UserRoleDescription>();
    }

    public UserRole(Guid id, string userRoleText, Guid iamClientId) : this()
    {
        Id = id;
        UserRoleText = userRoleText;
        IamClientId = iamClientId;
    }

    [Key]
    public Guid Id { get; private set; }

    [MaxLength(255)]
    [Column("user_role")]
    public string UserRoleText { get; set; }

    public Guid IamClientId { get; set; }

    // Navigation properties
    public virtual IamClient? IamClient { get; set; }
    public virtual ICollection<CompanyUser> CompanyUsers { get; private set; }
    public virtual ICollection<UserRoleDescription> UserRoleDescriptions { get; private set; }
}
