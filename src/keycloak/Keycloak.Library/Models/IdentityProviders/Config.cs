/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2022 BMW Group AG
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders;

public class Config
{
    [JsonPropertyName("hideOnLoginPage")]
    public string? HideOnLoginPage { get; set; }
    [JsonPropertyName("clientSecret")]
    public string? ClientSecret { get; set; }
    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }
    [JsonPropertyName("disableUserInfo")]
    public string? DisableUserInfo { get; set; }
    [JsonPropertyName("useJwksUrl")]
    public string? UseJwksUrl { get; set; }
    [JsonPropertyName("tokenUrl")]
    public string? TokenUrl { get; set; }
    [JsonPropertyName("authorizationUrl")]
    public string? AuthorizationUrl { get; set; }
    [JsonPropertyName("logoutUrl")]
    public string? LogoutUrl { get; set; }
    [JsonPropertyName("jwksUrl")]
    public string? JwksUrl { get; set; }
    [JsonPropertyName("clientAuthMethod")]
    public string? ClientAuthMethod { get; set; }
    [JsonPropertyName("clientAssertionSigningAlg")]
    public string? ClientAssertionSigningAlg { get; set; }
    [JsonPropertyName("syncMode")]
    public string? SyncMode { get; set; }
    [JsonPropertyName("validateSignature")]
    public string? ValidateSignature { get; set; }
    [JsonPropertyName("userInfoUrl")]
    public string? UserInfoUrl { get; set; }
    [JsonPropertyName("issuer")]
    public string? Issuer { get; set; }

    // for SAML:
    [JsonPropertyName("nameIDPolicyFormat")]
    public string? NameIDPolicyFormat { get; set; }
    [JsonPropertyName("principalType")]
    public string? PrincipalType { get; set; }
    [JsonPropertyName("signatureAlgorithm")]
    public string? SignatureAlgorithm { get; set; }
    [JsonPropertyName("xmlSigKeyInfoKeyNameTransformer")]
    public string? XmlSigKeyInfoKeyNameTransformer { get; set; }
    [JsonPropertyName("allowCreate")]
    public string? AllowCreate { get; set; }
    [JsonPropertyName("entityId")]
    public string? EntityId { get; set; }
    [JsonPropertyName("authnContextComparisonType")]
    public string? AuthnContextComparisonType { get; set; }
    [JsonPropertyName("backchannelSupported")]
    public string? BackchannelSupported { get; set; }
    [JsonPropertyName("postBindingResponse")]
    public string? PostBindingResponse { get; set; }
    [JsonPropertyName("postBindingAuthnRequest")]
    public string? PostBindingAuthnRequest { get; set; }
    [JsonPropertyName("postBindingLogout")]
    public string? PostBindingLogout { get; set; }
    [JsonPropertyName("wantAuthnRequestsSigned")]
    public string? WantAuthnRequestsSigned { get; set; }
    [JsonPropertyName("wantAssertionsSigned")]
    public string? WantAssertionsSigned { get; set; }
    [JsonPropertyName("wantAssertionsEncrypted")]
    public string? WantAssertionsEncrypted { get; set; }
    [JsonPropertyName("forceAuthn")]
    public string? ForceAuthn { get; set; }
    [JsonPropertyName("signSpMetadata")]
    public string? SignSpMetadata { get; set; }
    [JsonPropertyName("loginHint")]
    public string? LoginHint { get; set; }
    [JsonPropertyName("singleSignOnServiceUrl")]
    public string? SingleSignOnServiceUrl { get; set; }
    [JsonPropertyName("allowedClockSkew")]
    public string? AllowedClockSkew { get; set; }
    [JsonPropertyName("attributeConsumingServiceIndex")]
    public string? AttributeConsumingServiceIndex { get; set; }
}
