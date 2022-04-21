using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class AppLicense
    {
        private AppLicense()
        {
            Licensetext = null!;
            Apps = new HashSet<App>();
        }

        public AppLicense(Guid id, string licensetext) : this()
        {
            Id = id;
            Licensetext = licensetext;
        }

        [Key]
        public Guid Id { get; private set; }

        [MaxLength(255)]
        public string Licensetext { get; set; }

        public virtual ICollection<App> Apps { get; private set; }
    }
}
