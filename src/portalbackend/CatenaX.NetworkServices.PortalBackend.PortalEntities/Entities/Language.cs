using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Language
    {
        private Language()
        {
            ShortName = null!;
            LongNameDe = null!;
            LongNameEn = null!;
            AppDescriptions = new HashSet<AppDescription>();
            CompanyRoleDescriptions = new HashSet<CompanyRoleDescription>();
            SupportingApps = new HashSet<App>();
        }

        public Language(string languageShortName, string longNameDe, string longNameEn) : this()
        {
            ShortName = languageShortName;
            LongNameDe = longNameDe;
            LongNameEn = longNameEn;
        }

        [Key]
        [StringLength(2, MinimumLength = 2)]
        public string ShortName { get; private set; }

        [MaxLength(255)]
        public string LongNameDe { get; set; }

        [MaxLength(255)]
        public string LongNameEn { get; set; }

        // Navigation properties
        public virtual ICollection<AppDescription> AppDescriptions { get; private set; }
        public virtual ICollection<CompanyRoleDescription> CompanyRoleDescriptions { get; private set; }
        public virtual ICollection<App> SupportingApps { get; private set; }
    }
}
