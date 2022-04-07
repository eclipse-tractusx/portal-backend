using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyIdentityProvider
    {
        public CompanyIdentityProvider() {}
        public CompanyIdentityProvider(Company company, IdentityProvider identityProvider)
        {
            Company = company;
            IdentityProvider = identityProvider;
        }

        public Guid CompanyId { get; set; }
        public Guid IdentityProviderId { get; set; }

        public virtual Company Company { get; set; }
        public virtual IdentityProvider IdentityProvider { get; set; }
    }
}
