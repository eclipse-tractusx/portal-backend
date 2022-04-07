using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyApplicationStatus
    {
        public CompanyApplicationStatus()
        {
            CompanyApplications = new HashSet<CompanyApplication>();
        }

        public CompanyApplicationStatusId ApplicationStatusId { get; set; }

        [MaxLength(255)]
        public string? Label { get; set; }

        public virtual ICollection<CompanyApplication> CompanyApplications { get; set; }
    }
}
