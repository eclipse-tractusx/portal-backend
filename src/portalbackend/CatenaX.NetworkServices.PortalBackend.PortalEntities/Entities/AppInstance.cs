namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class AppInstance
{
    private AppInstance()
    {
        CompanyAssignedApps = new HashSet<CompanyAssignedApp>();
    }

    public AppInstance(Guid id, Guid appId, Guid iamClientId) : this()
    {
        Id = id;
        AppId = appId;
        IamClientId = iamClientId;
    }

    public Guid Id { get; set; }
    public Guid AppId { get; private set; }
    public Guid IamClientId { get; private set; }

    // Navigation properties
    public virtual App? App { get; private set; }
    public virtual IamClient? IamClient { get; private set; }
    
    public virtual ICollection<CompanyAssignedApp> CompanyAssignedApps { get; private set; }
}
