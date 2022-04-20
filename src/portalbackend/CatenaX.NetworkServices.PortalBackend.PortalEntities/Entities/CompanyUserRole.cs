using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyUserRole
    {
        public CompanyUserRole()
        {
            Apps = new HashSet<App>();
            CompanyUsers = new HashSet<CompanyUser>();
        }

        public CompanyUserRole(string companyUserRoleText, string namede, string nameen) : this()
        {
            CompanyUserRoleText = companyUserRoleText;
            Namede = namede;
            Nameen = nameen;
        }

        [Key]
        public Guid Id { get; set; }

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
