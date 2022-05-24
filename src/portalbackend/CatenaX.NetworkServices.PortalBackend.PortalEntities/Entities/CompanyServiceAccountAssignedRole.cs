namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyServiceAccountAssignedRole
{
    private CompanyServiceAccountAssignedRole() {}

    public CompanyServiceAccountAssignedRole(Guid companyServiceAccountId, Guid userRoleId)
    {
        CompanyServiceAccountId = companyServiceAccountId;
        UserRoleId = userRoleId;
    }

    public Guid CompanyServiceAccountId { get; private set; }
    public Guid UserRoleId { get; private set; }

    // Navigation properties
    public virtual CompanyServiceAccount? CompanyServiceAccount { get; private set; }
    public virtual UserRole? UserRole { get; private set; }
}
