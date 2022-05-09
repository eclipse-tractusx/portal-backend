using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.App.Service.InputModels
{
    /// <summary>
    /// Simple model to specify descriptions for a language.
    /// </summary>
    public class LocalizedDescription
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="languageCode">Two character language code.</param>
        /// <param name="longDescription">Long description in specified language.</param>
        /// <param name="shortDescription">Short description in specified language.</param>
        public LocalizedDescription(string languageCode, string longDescription, string shortDescription)
        {
            LanguageCode = languageCode;
            LongDescription = longDescription;
            ShortDescription = shortDescription;
        }

        /// <summary>
        /// Two character language code.
        /// </summary>
        [StringLength(2, MinimumLength = 2)]
        public string LanguageCode { get; set; }

        /// <summary>
        /// Long description in specified language.
        /// </summary>
        [MaxLength(4096)]
        public string LongDescription { get; set; }

        /// <summary>
        /// Short description in specified language.
        /// </summary>
        [MaxLength(255)]
        public string ShortDescription { get; set; }
    }
}
