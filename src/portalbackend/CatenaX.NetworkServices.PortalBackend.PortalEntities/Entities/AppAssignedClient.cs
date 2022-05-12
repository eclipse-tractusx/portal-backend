namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class AppAssignedClient
{
    private AppAssignedClient() {}
    public AppAssignedClient(Guid appId, Guid iamClientId)
    {
        AppId = appId;
        IamClientId = iamClientId;
    }

    public Guid AppId { get; private set; }
    public Guid IamClientId { get; private set; }

    // Navigation properties
    public virtual App? App { get; private set; }
    public virtual IamClient? IamClient { get; private set; }
}
