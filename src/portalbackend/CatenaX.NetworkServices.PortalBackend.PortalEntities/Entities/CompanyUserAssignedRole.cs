namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyUserAssignedRole
{
    private CompanyUserAssignedRole() {}

    public CompanyUserAssignedRole(Guid companyUserId, Guid companyUserRoleId)
    {
        CompanyUserId = companyUserId;
        CompanyUserRoleId = companyUserRoleId;
    }

    public Guid CompanyUserId { get; private set; }
    public Guid CompanyUserRoleId { get; private set; }

    // Navigation properties
    public virtual CompanyUser? CompanyUser { get; private set; }
    public virtual CompanyUserRole? UserRole { get; private set; }
}
