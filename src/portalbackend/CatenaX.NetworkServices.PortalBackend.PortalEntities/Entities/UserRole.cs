using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class UserRole
{
    private UserRole()
    {
        UserRoleText = null!;
        CompanyUsers = new HashSet<CompanyUser>();
        CompanyServiceAccounts = new HashSet<CompanyServiceAccount>();
        UserRoleDescriptions = new HashSet<UserRoleDescription>();
    }

    public UserRole(Guid id, string userRoleText, Guid offerId) : this()
    {
        Id = id;
        UserRoleText = userRoleText;
        OfferId = offerId;
    }

    public Guid Id { get; private set; }

    [MaxLength(255)]
    [Column("user_role")]
    public string UserRoleText { get; set; }

    public Guid OfferId { get; set; }

    // Navigation properties
    public virtual Offer? Offer { get; set; }
    public virtual ICollection<CompanyUser> CompanyUsers { get; private set; }
    public virtual ICollection<CompanyServiceAccount> CompanyServiceAccounts { get; private set; }
    public virtual ICollection<UserRoleDescription> UserRoleDescriptions { get; private set; }
}
