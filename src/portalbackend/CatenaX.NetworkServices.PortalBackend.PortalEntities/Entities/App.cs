using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class App : BaseEntity
    {
        public App()
        {
            Agreements = new HashSet<Agreement>();
            AppDescriptions = new HashSet<AppDescription>();
            Companies = new HashSet<Company>();
            CompanyUserRoles = new HashSet<CompanyUserRole>();
            AppLicenses = new HashSet<AppLicense>();
            UseCases = new HashSet<UseCase>();
            CompanyUsers = new HashSet<CompanyUser>();
        }

        [MaxLength(255)]
        public string? Name { get; set; }

        public DateTime? DateReleased { get; set; }

        [MaxLength(255)]
        public string? ThumbnailUrl { get; set; }

        [MaxLength(255)]
        public string? AppUrl { get; set; }

        [MaxLength(255)]
        public string? MarketingUrl { get; set; }

        public Guid? VendorCompanyId { get; set; }

        public virtual Company? VendorCompany { get; set; }
        public virtual ICollection<Company> Companies { get; set; }
        public virtual ICollection<Agreement> Agreements { get; set; }
        public virtual ICollection<AppDescription> AppDescriptions { get; set; }
        public virtual ICollection<CompanyUserRole> CompanyUserRoles { get; set; }
        public virtual ICollection<AppLicense> AppLicenses { get; set; }
        public virtual ICollection<UseCase> UseCases { get; set; }
        public virtual ICollection<CompanyUser> CompanyUsers { get; set; }
    }
}
