using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class AppStatus
    {
        [Key]
        public AppStatusId AppStatusId { get; set; }

        public string Label { get; set; }

        public virtual ICollection<App> Apps { get; set; }
    }
}
