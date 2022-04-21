using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyStatus
    {
        private CompanyStatus()
        {
            Label = null!;
            Companies = new HashSet<Company>();
        }

        public CompanyStatus(CompanyStatusId companyStatusId) : this()
        {
            CompanyStatusId = companyStatusId;
            Label = companyStatusId.ToString();
        }

        [Key]
        public CompanyStatusId CompanyStatusId { get; private set; }

        [MaxLength(255)]
        public string Label { get; private set; }

        public virtual ICollection<Company> Companies { get; private set; }
    }
}
