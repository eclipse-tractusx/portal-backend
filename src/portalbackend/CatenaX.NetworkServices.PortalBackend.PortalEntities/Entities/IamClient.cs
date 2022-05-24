using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class IamClient
{
    private IamClient()
    {
        ClientClientId = null!;
        UserRoles = new HashSet<UserRole>();
        Apps = new HashSet<App>();
    }

    public IamClient(Guid id, string clientClientId) : this()
    {
        Id = id;
        ClientClientId = clientClientId;
    }

    public Guid Id { get; private set; }

    [StringLength(255)]
    public string ClientClientId { get; private set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; private set; }
    public virtual ICollection<App> Apps { get; private set; }
}
