using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyRole
    {
        public CompanyRole()
        {
            Companies = new HashSet<Company>();
        }

        public CompanyRole(string companyRoleText, string nameDe, string nameEn) : this()
        {
            CompanyRoleText = companyRoleText;
            NameDe = nameDe;
            NameEn = nameEn;
        }

        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string CompanyRoleText { get; set; }

        [MaxLength(255)]
        public string NameDe { get; set; }

        [MaxLength(255)]
        public string NameEn { get; set; }


        public virtual AgreementAssignedCompanyRole? AgreementAssignedCompanyRole { get; set; }
        public virtual ICollection<Company> Companies { get; set; }
    }
}
