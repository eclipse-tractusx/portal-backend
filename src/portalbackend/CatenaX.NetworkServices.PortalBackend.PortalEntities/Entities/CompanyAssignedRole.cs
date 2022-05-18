using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyAssignedRole
{
    private CompanyAssignedRole() {}

    public CompanyAssignedRole(Guid companyId, CompanyRoleId companyRoleId)
    {
        CompanyId = companyId;
        CompanyRoleId = companyRoleId;
    }

    public Guid CompanyId { get; private set; }
    public CompanyRoleId CompanyRoleId { get; private set; }

    // Navigation properties
    public virtual Company? Company { get; private set; }
    public virtual CompanyRole? CompanyRole { get; private set; }
}
