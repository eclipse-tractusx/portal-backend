using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyStatus
    {
        public CompanyStatus()
        {
            Companies = new HashSet<Company>();
        }

        [Key]
        public CompanyStatusId CompanyStatusId { get; set; }

        [MaxLength(255)]
        public string? Label { get; set; }

        public virtual ICollection<Company> Companies { get; set; }
    }
}
