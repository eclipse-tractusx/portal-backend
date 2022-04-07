using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Company : BaseEntity
    {
        public Company()
        {
            Agreements = new HashSet<Agreement>();
            BoughtApps = new HashSet<App>();
            ProvidedApps = new HashSet<App>();
            CompanyApplications = new HashSet<CompanyApplication>();
            IdentityProviders = new HashSet<IdentityProvider>();
            CompanyUsers = new HashSet<CompanyUser>();
            Consents = new HashSet<Consent>();
            CompanyRoles = new HashSet<CompanyRole>();
            UseCases = new HashSet<UseCase>();
        }

        [MaxLength(20)]
        public string? Bpn { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(255)]
        public string? Parent { get; set; }

        [MaxLength(255)]
        public string? Shortname { get; set; }

        public CompanyStatusId? CompanyStatusId { get; set; }

        public Guid? AddressId { get; set; }

        public virtual Address? Address { get; set; }
        public virtual CompanyStatus? CompanyStatus { get; set; }
        public virtual ICollection<App> ProvidedApps { get; set; }
        public virtual ICollection<App> BoughtApps { get; set; }
        public virtual ICollection<Agreement> Agreements { get; set; }
        public virtual ICollection<CompanyApplication> CompanyApplications { get; set; }
        public virtual ICollection<IdentityProvider> IdentityProviders { get; set; }
        public virtual ICollection<CompanyUser> CompanyUsers { get; set; }
        public virtual ICollection<Consent> Consents { get; set; }
        public virtual ICollection<CompanyRole> CompanyRoles { get; set; }
        public virtual ICollection<UseCase> UseCases { get; set; }
    }
}
