using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class AppLicense : BaseEntity
    {
        public AppLicense()
        {
            Apps = new HashSet<App>();
        }

        [MaxLength(255)]
        public string? Licensetext { get; set; }

        public virtual ICollection<App> Apps { get; set; }
    }
}
