/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 ********************************************************************************/

using Newtonsoft.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders;

public class Config
{
    [JsonProperty("hideOnLoginPage")]
    public string? HideOnLoginPage { get; set; }
    [JsonProperty("clientSecret")]
    public string? ClientSecret { get; set; }
    [JsonProperty("clientId")]
    public string? ClientId { get; set; }
    [JsonProperty("disableUserInfo")]
    public string? DisableUserInfo { get; set; }
    [JsonProperty("useJwksUrl")]
    public string? UseJwksUrl { get; set; }
    [JsonProperty("tokenUrl")]
    public string? TokenUrl { get; set; }
    [JsonProperty("authorizationUrl")]
    public string? AuthorizationUrl { get; set; }
    [JsonProperty("logoutUrl")]
    public string? LogoutUrl { get; set; }
    [JsonProperty("jwksUrl")]
    public string? JwksUrl { get; set; }
    [JsonProperty("clientAuthMethod")]
    public string? ClientAuthMethod { get; set; }
    [JsonProperty("clientAssertionSigningAlg")]
    public string? ClientAssertionSigningAlg { get; set; }
    [JsonProperty("syncMode")]
    public string? SyncMode { get; set; }
    [JsonProperty("validateSignature")]
    public string? ValidateSignature { get; set; }
    [JsonProperty("userInfoUrl")]
    public string? UserInfoUrl { get; set; }
    [JsonProperty("issuer")]
    public string? Issuer { get; set; }

    // for SAML:
    [JsonProperty("nameIDPolicyFormat")]
    public string? NameIDPolicyFormat { get; set; }
    [JsonProperty("principalType")]
    public string? PrincipalType { get; set; }
    [JsonProperty("signatureAlgorithm")]
    public string? SignatureAlgorithm { get; set; }
    [JsonProperty("xmlSigKeyInfoKeyNameTransformer")]
    public string? XmlSigKeyInfoKeyNameTransformer { get; set; }
    [JsonProperty("allowCreate")]
    public string? AllowCreate { get; set; }
    [JsonProperty("entityId")]
    public string? EntityId { get; set; }
    [JsonProperty("authnContextComparisonType")]
    public string? AuthnContextComparisonType { get; set; }
    [JsonProperty("backchannelSupported")]
    public string? BackchannelSupported { get; set; }
    [JsonProperty("postBindingResponse")]
    public string? PostBindingResponse { get; set; }
    [JsonProperty("postBindingAuthnRequest")]
    public string? PostBindingAuthnRequest { get; set; }
    [JsonProperty("postBindingLogout")]
    public string? PostBindingLogout { get; set; }
    [JsonProperty("wantAuthnRequestsSigned")]
    public string? WantAuthnRequestsSigned { get; set; }
    [JsonProperty("wantAssertionsSigned")]
    public string? WantAssertionsSigned { get; set; }
    [JsonProperty("wantAssertionsEncrypted")]
    public string? WantAssertionsEncrypted { get; set; }
    [JsonProperty("forceAuthn")]
    public string? ForceAuthn { get; set; }
    [JsonProperty("signSpMetadata")]
    public string? SignSpMetadata { get; set; }
    [JsonProperty("loginHint")]
    public string? LoginHint { get; set; }
    [JsonProperty("singleSignOnServiceUrl")]
    public string? SingleSignOnServiceUrl { get; set; }
    [JsonProperty("allowedClockSkew")]
    public string? AllowedClockSkew { get; set; }
    [JsonProperty("attributeConsumingServiceIndex")]
    public string? AttributeConsumingServiceIndex { get; set; }
}
