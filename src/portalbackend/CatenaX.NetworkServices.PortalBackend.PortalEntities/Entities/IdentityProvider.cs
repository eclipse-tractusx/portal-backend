using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class IdentityProvider
    {
        public IdentityProvider() {
            Companies = new HashSet<Company>();
        }
        
        public IdentityProvider(IdentityProviderCategoryId identityProviderCategoryId) : this()
        {
            IdentityProviderCategoryId = identityProviderCategoryId;
        }

        [Key]
        public Guid Id { get; set; }

        public DateTime? DateCreated { get; set; }

        public IdentityProviderCategoryId IdentityProviderCategoryId { get; set; }

        public virtual IdentityProviderCategory? IdentityProviderCategory { get; set; }
        public virtual IamIdentityProvider? IamIdentityProvider { get; set; }
        public virtual ICollection<Company> Companies { get; set; }
    }
}
