using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class IdentityProviderCategory
    {
        public IdentityProviderCategory()
        {
            IdentityProviders = new HashSet<IdentityProvider>();
        }

        [Key]
        public IdentityProviderCategoryId IdentityProviderCategoryId { get; set; }

        [MaxLength(255)]
        public string? Label { get; set; }

        public virtual ICollection<IdentityProvider> IdentityProviders { get; set; }
    }
}
