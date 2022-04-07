using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyRole
    {
        public CompanyRole() {}
        public CompanyRole(string companyRoleText, string nameDe, string nameEn)
        {
            Companies = new HashSet<Company>();
            CompanyRoleText = companyRoleText;
            NameDe = nameDe;
            NameEn = nameEn;
        }
        /// <summary>
        /// Primary key of the entity.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Date of entity creation.
        /// </summary>
        [Column("date_created")]
        public DateTime? DateCreated { get; set; }

        /// <summary>
        /// Date of most recent entity modification.
        /// </summary>
        [Column("date_last_changed")]
        public DateTime? DateLastChanged { get; set; }

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
