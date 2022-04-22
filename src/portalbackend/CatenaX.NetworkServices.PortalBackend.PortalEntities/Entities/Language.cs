using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Language
    {
        private Language()
        {
            LanguageShortName = null!;
            LongNameDe = null!;
            LongNameEn = null!;
            AppDescriptions = new HashSet<AppDescription>();
        }

        public Language(string languageShortName, string longNameDe, string longNameEn) : this()
        {
            LanguageShortName = languageShortName;
            LongNameDe = longNameDe;
            LongNameEn = longNameEn;
        }

        [Key]
        [StringLength(2, MinimumLength = 2)]
        public string LanguageShortName { get; private set; }

        [MaxLength(255)]
        public string LongNameDe { get; set; }

        [MaxLength(255)]
        public string LongNameEn { get; set; }

        // Navigation properties
        public virtual ICollection<AppDescription> AppDescriptions { get; private set; }
    }
}
