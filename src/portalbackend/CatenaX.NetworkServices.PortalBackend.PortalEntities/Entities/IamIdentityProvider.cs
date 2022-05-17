using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class IamIdentityProvider
{
    private IamIdentityProvider()
    {
        IamIdpAlias = null!;
    }

    public IamIdentityProvider(string iamIdpAlias, Guid identityProviderId)
    {
        IamIdpAlias = iamIdpAlias;
        IdentityProviderId = identityProviderId;
    }

    public Guid IdentityProviderId { get; private set; }

    [Key]
    [MaxLength(255)]
    public string IamIdpAlias { get; private set; }

    // Navigation properties
    public virtual IdentityProvider? IdentityProvider { get; private set; }
}
