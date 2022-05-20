using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class IdentityProviderCategory
{
    private IdentityProviderCategory()
    {
        Label = null!;
        IdentityProviders = new HashSet<IdentityProvider>();
    }

    public IdentityProviderCategory(IdentityProviderCategoryId identityProviderCategoryId) : this()
    {
        Id = identityProviderCategoryId;
        Label = identityProviderCategoryId.ToString();
    }

    public IdentityProviderCategoryId Id { get; private set; }

    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<IdentityProvider> IdentityProviders { get; private set; }
}
