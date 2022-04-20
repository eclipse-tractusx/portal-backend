﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class AppLicense
    {
        public AppLicense()
        {
            Apps = new HashSet<App>();
        }

        [Key]
        public Guid Id { get; set; }

        [MaxLength(255)]
        public string Licensetext { get; set; }

        public virtual ICollection<App> Apps { get; set; }
    }
}
