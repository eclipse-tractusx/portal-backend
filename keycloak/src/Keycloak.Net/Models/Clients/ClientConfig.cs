using Newtonsoft.Json;

namespace Keycloak.Net.Models.Clients
{
    public class ClientConfig
    {
        [JsonProperty("userinfotokenclaim")]
        public string UserInfoTokenClaim { get; set; }
        [JsonProperty("user.attribute")]
        public string UserAttribute { get; set; }
        [JsonProperty("idtokenclaim")]
        public string IdTokenClaim { get; set; }
        [JsonProperty("accesstokenclaim")]
        public string AccessTokenClaim { get; set; }
        [JsonProperty("claimname")]
        public string ClaimName { get; set; }
        [JsonProperty("jsonTypelabel")]
        public string JsonTypelabel { get; set; }
        [JsonProperty("friendly.name")]
        public string FriendlyName { get; set; }
        [JsonProperty("attribute.name")]
        public string AttributeName { get; set; }
    }
}