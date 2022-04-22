using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyIdentityProvider
    {
        private CompanyIdentityProvider() {}

        public CompanyIdentityProvider(Guid companyId, Guid identityProviderId)
        {
            CompanyId = companyId;
            IdentityProviderId = identityProviderId;
        }

        public Guid CompanyId { get; private set; }
        public Guid IdentityProviderId { get; private set; }

        // Navigation properties
        public virtual Company? Company { get; private set; }
        public virtual IdentityProvider? IdentityProvider { get; private set; }
    }
}
