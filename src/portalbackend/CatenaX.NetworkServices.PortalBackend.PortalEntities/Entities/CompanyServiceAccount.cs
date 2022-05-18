using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyServiceAccount
{
    private CompanyServiceAccount()
    {
        Name = default!;
        Description = default!;
        UserRoles = new HashSet<UserRole>();
        CompanyServiceAccountAssignedRoles = new HashSet<CompanyServiceAccountAssignedRole>();
    }
    
    public CompanyServiceAccount(Guid id, Guid companyId, CompanyServiceAccountStatusId companyServiceAccountStatusId, string name, string description, DateTimeOffset dateCreated) : this()
    {
        Id = id;
        DateCreated = dateCreated;
        CompanyId = companyId;
        CompanyServiceAccountStatusId = companyServiceAccountStatusId;
        Name = name;
        Description = description;
    }

    [Key]
    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    public Guid CompanyId { get; private set; }

    [MaxLength(255)]
    public string Name { get; set; }

    public string Description { get; set; }

    public CompanyServiceAccountStatusId CompanyServiceAccountStatusId { get; set; }

    // Navigation properties
    public virtual Company? Company { get; private set; }
    public virtual IamServiceAccount? IamServiceAccount { get; set; }
    public virtual CompanyServiceAccountStatus? CompanyServiceAccountStatus { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; private set; }
    public virtual ICollection<CompanyServiceAccountAssignedRole> CompanyServiceAccountAssignedRoles { get; private set; }
}
