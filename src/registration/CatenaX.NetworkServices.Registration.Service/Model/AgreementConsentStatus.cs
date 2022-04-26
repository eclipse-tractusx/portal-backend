using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Registration.Service.Model
{
    public class AgreementConsentStatus
    {
        [JsonPropertyName("consentStatus")]
        public ConsentStatusId ConsentStatusId { get; set; }

        [JsonPropertyName("agreementCategory")]
        public AgreementCategoryId AgreementCategoryId { get; set; }

        [JsonPropertyName("agreementType")]
        public string? AgreementType { get; set; }
    }
}
