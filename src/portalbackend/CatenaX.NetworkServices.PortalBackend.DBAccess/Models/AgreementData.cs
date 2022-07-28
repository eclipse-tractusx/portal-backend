using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class AgreementData
    {
        public AgreementData(Guid agreementId, string agreementName)
        {
            AgreementId = agreementId;
            AgreementName = agreementName;
        }

        [JsonPropertyName("agreementId")]
        public Guid AgreementId { get; set; }

        [JsonPropertyName("name")]
        public string AgreementName { get; set; }
    }
}
