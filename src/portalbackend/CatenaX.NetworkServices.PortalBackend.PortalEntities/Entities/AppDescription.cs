using System;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class AppDescription
    {
        public AppDescription() {}

        public AppDescription(string descriptionLong, string descriptionShort, Guid appId, string languageShortName)
        {
            DescriptionLong = descriptionLong;
            DescriptionShort = descriptionShort;
            AppId = appId;
            LanguageShortName = languageShortName;
        }
        
        [MaxLength(4096)]
        public string DescriptionLong { get; set; }

        [MaxLength(255)]
        public string DescriptionShort { get; set; }

        public Guid AppId { get; set; }

        [StringLength(2, MinimumLength = 2)]
        public string LanguageShortName { get; set; }

        public virtual App? App { get; set; }
        public virtual Language? Language { get; set; }
    }
}
