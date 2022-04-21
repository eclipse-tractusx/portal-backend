using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyRole
    {
        private CompanyRole()
        {
            CompanyRoleText = null!;
            NameDe = null!;
            NameEn = null!;
            Companies = new HashSet<Company>();
        }

        public CompanyRole(int id, string companyRoleText, string nameDe, string nameEn) : this()
        {
            Id = id;
            CompanyRoleText = companyRoleText;
            NameDe = nameDe;
            NameEn = nameEn;
        }

        [Key]
        public int Id { get; private set; }

        [MaxLength(255)]
        public string CompanyRoleText { get; set; }

        [MaxLength(255)]
        public string NameDe { get; set; }

        [MaxLength(255)]
        public string NameEn { get; set; }

        // Navigation properties
        public virtual AgreementAssignedCompanyRole? AgreementAssignedCompanyRole { get; private set; }
        public virtual ICollection<Company> Companies { get; private set; }
    }
}
