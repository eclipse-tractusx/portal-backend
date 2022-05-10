namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class AppAssignedCompanyUserRole
{
    private AppAssignedCompanyUserRole() {}
    public AppAssignedCompanyUserRole(Guid appId, Guid companyUserRoleId)
    {
        AppId = appId;
        CompanyUserRoleId = companyUserRoleId;
    }

    public Guid AppId { get; private set; }
    public Guid CompanyUserRoleId { get; private set; }

    // Navigation properties
    public virtual App? App { get; private set; }
    public virtual CompanyUserRole? CompanyUserRole { get; private set; }
}
