using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class AppStatus
    {
        private AppStatus()
        {
            Label = null!;
            Apps = new HashSet<App>();
        }

        public AppStatus(AppStatusId appStatusId) : this()
        {
            AppStatusId = appStatusId;
            Label = appStatusId.ToString();
        }

        [Key]
        public AppStatusId AppStatusId { get; private set; }

        public string Label { get; private set; }

        // Navigation properties
        public virtual ICollection<App> Apps { get; private set; }
    }
}
