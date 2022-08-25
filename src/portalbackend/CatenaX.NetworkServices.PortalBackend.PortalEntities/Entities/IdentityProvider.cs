using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class IdentityProvider
{
    private IdentityProvider() {
        Companies = new HashSet<Company>();
        CompanyIdentityProviders = new HashSet<CompanyIdentityProvider>();
    }
    
    public IdentityProvider(Guid id, IdentityProviderCategoryId identityProviderCategoryId, DateTimeOffset dateCreated) : this()
    {
        Id = id;
        IdentityProviderCategoryId = identityProviderCategoryId;
        DateCreated = dateCreated;
    }

    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    public IdentityProviderCategoryId IdentityProviderCategoryId { get; private set; }

    // Navigation properties
    public virtual IdentityProviderCategory? IdentityProviderCategory { get; private set; }
    public virtual IamIdentityProvider? IamIdentityProvider { get; set; }
    public virtual ICollection<Company> Companies { get; private set; }
    public virtual ICollection<CompanyIdentityProvider> CompanyIdentityProviders { get; private set; }
}
