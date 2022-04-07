using Newtonsoft.Json;

namespace Keycloak.Net.Models.IdentityProviders
{
    public class Config
    {
        [JsonProperty("hideOnLoginPage")]
        public string HideOnLoginPage { get; set; }
        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }
        [JsonProperty("clientId")]
        public string ClientId { get; set; }
        [JsonProperty("disableUserInfo")]
        public string DisableUserInfo { get; set; }
        [JsonProperty("useJwksUrl")]
        public string UseJwksUrl { get; set; }
        [JsonProperty("tokenUrl")]
        public string TokenUrl { get; set; }
        [JsonProperty("authorizationUrl")]
        public string AuthorizationUrl { get; set; }
        [JsonProperty("logoutUrl")]
        public string LogoutUrl { get; set; }
        [JsonProperty("jwksUrl")]
        public string JwksUrl { get; set; }
        [JsonProperty("clientAuthMethod")]
        public string ClientAuthMethod { get; set; }
        [JsonProperty("clientAssertionSigningAlg")]
        public string ClientAssertionSigningAlg { get; set; }
        [JsonProperty("syncMode")]
        public string SyncMode{ get; set; }
        [JsonProperty("validateSignature")]
        public string ValidateSignature { get; set; }
        [JsonProperty("userInfoUrl")]
        public string UserInfoUrl { get; set; }
        [JsonProperty("issuer")]
        public string Issuer { get; set; }
    }
}