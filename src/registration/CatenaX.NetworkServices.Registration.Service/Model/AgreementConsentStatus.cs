using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Registration.Service.Model
{
    public class AgreementConsentStatus
    {
        public AgreementConsentStatus(ConsentStatusId consentStatusId, AgreementCategoryId agreementCategoryId, string name)
        {
            ConsentStatusId = consentStatusId;
            AgreementCategoryId = agreementCategoryId;
            Name = name;
        }

        [JsonPropertyName("consentStatus")]
        public ConsentStatusId ConsentStatusId { get; }

        [JsonPropertyName("agreementCategory")]
        public AgreementCategoryId AgreementCategoryId { get; }

        [JsonPropertyName("agreementType")]
        public string? AgreementType { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; }
    }
}
