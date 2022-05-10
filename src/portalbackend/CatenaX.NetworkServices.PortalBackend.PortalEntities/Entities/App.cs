using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class App
    {
        private App()
        {
            Provider = null!;
            Agreements = new HashSet<Agreement>();
            AppDescriptions = new HashSet<AppDescription>();
            AppDetailImages = new HashSet<AppDetailImage>();
            Companies = new HashSet<Company>();
            UserRoles = new HashSet<UserRole>();
            AppLicenses = new HashSet<AppLicense>();
            UseCases = new HashSet<UseCase>();
            CompanyUsers = new HashSet<CompanyUser>();
            Tags = new HashSet<AppTag>();
            SupportedLanguages = new HashSet<Language>();
        }

        public App(Guid id, string provider, DateTimeOffset dateCreated) : this()
        {
            Id = id;
            Provider = provider;
            DateCreated = dateCreated;
        }

        [Key]
        public Guid Id { get; private set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        public DateTimeOffset DateCreated { get; private set; }

        public DateTimeOffset? DateReleased { get; set; }

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

        // Navigation properties
        public virtual Company? ProviderCompany { get; set; }
        public virtual AppStatus? AppStatus{ get; set; }
        public virtual ICollection<AppTag> Tags { get; private set; }
        public virtual ICollection<Company> Companies { get; private set; }
        public virtual ICollection<Agreement> Agreements { get; private set; }
        public virtual ICollection<AppDescription> AppDescriptions { get; private set; }
        public virtual ICollection<AppDetailImage> AppDetailImages { get; private set; }
        public virtual ICollection<UserRole> UserRoles { get; private set; }
        public virtual ICollection<AppLicense> AppLicenses { get; private set; }
        public virtual ICollection<UseCase> UseCases { get; private set; }
        public virtual ICollection<CompanyUser> CompanyUsers { get; private set; }
        public virtual ICollection<Language> SupportedLanguages { get; private set; }
    }
}
