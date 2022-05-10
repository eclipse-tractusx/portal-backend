namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class AppAssignedUserRole
{
    private AppAssignedUserRole() {}
    public AppAssignedUserRole(Guid appId, Guid userRoleId)
    {
        AppId = appId;
        UserRoleId = userRoleId;
    }

    public Guid AppId { get; private set; }
    public Guid UserRoleId { get; private set; }

    // Navigation properties
    public virtual App? App { get; private set; }
    public virtual UserRole? UserRole { get; private set; }
}
