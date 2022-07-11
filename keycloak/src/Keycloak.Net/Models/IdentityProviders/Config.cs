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

        // for SAML:
        [JsonProperty("nameIDPolicyFormat")]
        public string NameIDPolicyFormat { get; set; }
        [JsonProperty("principalType")]
        public string PrincipalType { get; set; }
        [JsonProperty("signatureAlgorithm")]
        public string SignatureAlgorithm { get; set; }
        [JsonProperty("xmlSigKeyInfoKeyNameTransformer")]
        public string XmlSigKeyInfoKeyNameTransformer { get; set; }
        [JsonProperty("allowCreate")]
        public string AllowCreate { get; set; }
        [JsonProperty("entityId")]
        public string EntityId { get; set; }
        [JsonProperty("authnContextComparisonType")]
        public string AuthnContextComparisonType { get; set; }
        [JsonProperty("backchannelSupported")]
        public string BackchannelSupported { get; set; }
        [JsonProperty("postBindingResponse")]
        public string PostBindingResponse { get; set; }
        [JsonProperty("postBindingAuthnRequest")]
        public string PostBindingAuthnRequest { get; set; }
        [JsonProperty("postBindingLogout")]
        public string PostBindingLogout { get; set; }
        [JsonProperty("wantAuthnRequestsSigned")]
        public string WantAuthnRequestsSigned { get; set; }
        [JsonProperty("wantAssertionsSigned")]
        public string WantAssertionsSigned { get; set; }
        [JsonProperty("wantAssertionsEncrypted")]
        public string WantAssertionsEncrypted { get; set; }
        [JsonProperty("forceAuthn")]
        public string ForceAuthn { get; set; }
        [JsonProperty("signSpMetadata")]
        public string SignSpMetadata { get; set; }
        [JsonProperty("loginHint")]
        public string LoginHint { get; set; }
        [JsonProperty("singleSignOnServiceUrl")]
        public string SingleSignOnServiceUrl { get; set; }
        [JsonProperty("allowedClockSkew")]
        public string AllowedClockSkew { get; set; }
        [JsonProperty("attributeConsumingServiceIndex")]
        public string AttributeConsumingServiceIndex { get; set; }
    }
}