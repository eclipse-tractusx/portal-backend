namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyUserAssignedRole
{
    private CompanyUserAssignedRole() {}

    public CompanyUserAssignedRole(Guid companyUserId, Guid userRoleId)
    {
        CompanyUserId = companyUserId;
        UserRoleId = userRoleId;
    }

    public Guid CompanyUserId { get; private set; }
    public Guid UserRoleId { get; private set; }

    // Navigation properties
    public virtual CompanyUser? CompanyUser { get; private set; }
    public virtual CompanyUserRole? UserRole { get; private set; }
}
