using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class IamClient
{
    private IamClient()
    {
        ClientClientId = null!;
        AppInstances = new HashSet<AppInstance>();
    }

    public IamClient(Guid id, string clientClientId) : this()
    {
        Id = id;
        ClientClientId = clientClientId;
    }

    public Guid Id { get; private set; }

    [StringLength(255)]
    public string ClientClientId { get; private set; }

    public virtual ICollection<AppInstance> AppInstances { get; private set; }
}
