using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class UseCase : BaseEntity
    {
        public UseCase()
        {
            Agreements = new HashSet<Agreement>();
            Companies = new HashSet<Company>();
            Apps = new HashSet<App>();
        }

        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(255)]
        public string? Shortname { get; set; }

        public virtual ICollection<Agreement> Agreements { get; set; }
        public virtual ICollection<Company> Companies { get; set; }
        public virtual ICollection<App> Apps { get; set; }
    }
}
