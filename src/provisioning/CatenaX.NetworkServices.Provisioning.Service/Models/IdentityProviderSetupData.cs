using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Provisioning.Service.Models
{
    public class IdentityProviderSetupData
    {
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }
        [JsonPropertyName("metadataUrl")]
        public string MetadataUrl { get; set; }
        [JsonPropertyName("clientAuthMethod")]
        public string ClientAuthMethod { get; set; }
        [JsonPropertyName("clientSecret")]
        public string ClientSecret { get; set; }
        [JsonPropertyName("organisationName")]
        public string OrganisationName { get; set; }
    }
}
