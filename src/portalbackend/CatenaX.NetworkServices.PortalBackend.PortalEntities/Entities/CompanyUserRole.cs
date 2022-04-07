using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyUserRole : BaseEntity
    {
        public CompanyUserRole() {}
        public CompanyUserRole(string companyUserRoleText, string namede, string nameen)
        {
            Apps = new HashSet<App>();
            CompanyUsers = new HashSet<CompanyUser>();
            CompanyUserRoleText = companyUserRoleText;
            Namede = namede;
            Nameen = nameen;
        }

        [MaxLength(255)]
        public string CompanyUserRoleText { get; set; }

        [MaxLength(255)]
        public string Namede { get; set; }

        [MaxLength(255)]
        public string Nameen { get; set; }

        public virtual ICollection<App> Apps { get; set; }
        public virtual ICollection<CompanyUser> CompanyUsers { get; set; }
    }
}
