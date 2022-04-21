using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyUserRole
    {
        private CompanyUserRole()
        {
            CompanyUserRoleText = null!;
            Namede = null!;
            Nameen = null!;
            Apps = new HashSet<App>();
            CompanyUsers = new HashSet<CompanyUser>();
        }

        public CompanyUserRole(Guid id, string companyUserRoleText, string namede, string nameen) : this()
        {
            Id = id;
            CompanyUserRoleText = companyUserRoleText;
            Namede = namede;
            Nameen = nameen;
        }

        [Key]
        public Guid Id { get; private set; }

        [MaxLength(255)]
        public string CompanyUserRoleText { get; set; }

        [MaxLength(255)]
        public string Namede { get; set; }

        [MaxLength(255)]
        public string Nameen { get; set; }

        public virtual ICollection<App> Apps { get; private set; }
        public virtual ICollection<CompanyUser> CompanyUsers { get; private set; }
    }
}
