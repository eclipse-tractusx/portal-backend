using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Language
    {
        public Language()
        {
            AppDescriptions = new HashSet<AppDescription>();
        }

        public Language(string languageShortName) : this()
        {
            LanguageShortName = languageShortName;
        }

        [Key]
        [StringLength(2, MinimumLength = 2)]
        public string LanguageShortName { get; set; }

        [MaxLength(255)]
        public string LongNameDe { get; set; }

        [MaxLength(255)]
        public string LongNameEn { get; set; }

        public virtual ICollection<AppDescription> AppDescriptions { get; set; }
    }
}
