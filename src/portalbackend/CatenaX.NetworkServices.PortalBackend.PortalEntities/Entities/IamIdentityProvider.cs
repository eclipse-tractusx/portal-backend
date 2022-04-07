using System;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class IamIdentityProvider
    {
        public IamIdentityProvider() {}
        public IamIdentityProvider(string iamIdpAlias)
        {
            IamIdpAlias = iamIdpAlias;
        }

        public Guid IdentityProviderId { get; set; }

        [MaxLength(255)]
        public string IamIdpAlias { get; set; }

        public virtual IdentityProvider? IdentityProvider { get; set; }
    }
}
