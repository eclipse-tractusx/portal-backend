using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class App
    {
        public App()
        {
            Agreements = new HashSet<Agreement>();
            AppDescriptions = new HashSet<AppDescription>();
            AppDetailImages = new HashSet<AppDetailImage>();
            Companies = new HashSet<Company>();
            CompanyUserRoles = new HashSet<CompanyUserRole>();
            AppLicenses = new HashSet<AppLicense>();
            UseCases = new HashSet<UseCase>();
            CompanyUsers = new HashSet<CompanyUser>();
        }

        public App(string provider) : this()
        {
            Provider = provider;
        }

        [Key]
        public Guid Id { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateReleased { get; set; }

        [MaxLength(255)]
        public string? ThumbnailUrl { get; set; }

        [MaxLength(255)]
        public string? AppUrl { get; set; }

        [MaxLength(255)]
        public string? MarketingUrl { get; set; }

        [MaxLength(255)]
        public string? ContactEmail { get; set; }

        [MaxLength(255)]
        public string? ContactNumber { get; set; }

        [MaxLength(255)]
        public string Provider { get; set; }

        public Guid? ProviderCompanyId { get; set; }

        public AppStatusId AppStatusId { get; set; }

        public virtual Company? ProviderCompany { get; set; }
        public virtual AppStatus? AppStatus{ get; set; }
        public virtual ICollection<Company> Companies { get; set; }
        public virtual ICollection<Agreement> Agreements { get; set; }
        public virtual ICollection<AppDescription> AppDescriptions { get; set; }
        public virtual ICollection<AppDetailImage> AppDetailImages { get; set; }
        public virtual ICollection<CompanyUserRole> CompanyUserRoles { get; set; }
        public virtual ICollection<AppLicense> AppLicenses { get; set; }
        public virtual ICollection<UseCase> UseCases { get; set; }
        public virtual ICollection<CompanyUser> CompanyUsers { get; set; }
    }
}
