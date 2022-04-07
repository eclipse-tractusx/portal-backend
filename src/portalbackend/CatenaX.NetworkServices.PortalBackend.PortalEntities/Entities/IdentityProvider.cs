using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Collections.Generic;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class IdentityProvider : BaseEntity
    {
        public IdentityProvider()
        {
            Companies = new HashSet<Company>();
        }

        public IdentityProviderCategoryId IdentityProviderCategoryId { get; set; }

        public virtual IdentityProviderCategory? IdentityProviderCategory { get; set; }
        public virtual IamIdentityProvider? IamIdentityProvider { get; set; }
        public virtual ICollection<Company> Companies { get; set; }
    }
}
