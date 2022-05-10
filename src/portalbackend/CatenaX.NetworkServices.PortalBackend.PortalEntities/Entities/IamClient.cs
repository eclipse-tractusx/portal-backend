using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class IamClient
{
    private IamClient()
    {
        ClientClientId = null!;
    }

    public IamClient(Guid id, string clientClientId)
    {
        Id = id;
        ClientClientId = clientClientId;
    }

    [Key]
    public Guid Id { get; private set; }

    [StringLength(255)]
    public string ClientClientId { get; private set; }

    // Navigation properties
    public virtual ICollection<CompanyUserRole>? CompanyUserRoles { get; private set; }
}
