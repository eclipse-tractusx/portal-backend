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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.OpenIDConfiguration;

public class OpenIDConfiguration
{
    [JsonPropertyName("issuer")]
    public Uri Issuer { get; set; }

    [JsonPropertyName("authorization_endpoint")]
    public Uri AuthorizationEndpoint { get; set; }

    [JsonPropertyName("token_endpoint")]
    public Uri TokenEndpoint { get; set; }

    [JsonPropertyName("token_introspection_endpoint")]
    public Uri TokenIntrospectionEndpoint { get; set; }

    [JsonPropertyName("userinfo_endpoint")]
    public Uri UserinfoEndpoint { get; set; }

    [JsonPropertyName("end_session_endpoint")]
    public Uri EndSessionEndpoint { get; set; }

    [JsonPropertyName("jwks_uri")]
    public Uri JwksUri { get; set; }

    [JsonPropertyName("check_session_iframe")]
    public Uri CheckSessionIframe { get; set; }

    [JsonPropertyName("grant_types_supported")]
    public string[] GrantTypesSupported { get; set; }

    [JsonPropertyName("response_types_supported")]
    public string[] ResponseTypesSupported { get; set; }

    [JsonPropertyName("subject_types_supported")]
    public string[] SubjectTypesSupported { get; set; }

    [JsonPropertyName("id_token_signing_alg_values_supported")]
    public string[] IdTokenSigningAlgValuesSupported { get; set; }

    [JsonPropertyName("id_token_encryption_alg_values_supported")]
    public string[] IdTokenEncryptionAlgValuesSupported { get; set; }

    [JsonPropertyName("id_token_encryption_enc_values_supported")]
    public string[] IdTokenEncryptionEncValuesSupported { get; set; }

    [JsonPropertyName("userinfo_signing_alg_values_supported")]
    public string[] UserinfoSigningAlgValuesSupported { get; set; }

    [JsonPropertyName("request_object_signing_alg_values_supported")]
    public string[] RequestObjectSigningAlgValuesSupported { get; set; }

    [JsonPropertyName("response_modes_supported")]
    public string[] ResponseModesSupported { get; set; }

    [JsonPropertyName("registration_endpoint")]
    public Uri RegistrationEndpoint { get; set; }

    [JsonPropertyName("token_endpoint_auth_methods_supported")]
    public string[] TokenEndpointAuthMethodsSupported { get; set; }

    [JsonPropertyName("token_endpoint_auth_signing_alg_values_supported")]
    public string[] TokenEndpointAuthSigningAlgValuesSupported { get; set; }

    [JsonPropertyName("claims_supported")]
    public string[] ClaimsSupported { get; set; }

    [JsonPropertyName("claim_types_supported")]
    public string[] ClaimTypesSupported { get; set; }

    [JsonPropertyName("claims_parameter_supported")]
    public bool ClaimsParameterSupported { get; set; }

    [JsonPropertyName("scopes_supported")]
    public string[] ScopesSupported { get; set; }

    [JsonPropertyName("request_parameter_supported")]
    public bool RequestParameterSupported { get; set; }

    [JsonPropertyName("request_uri_parameter_supported")]
    public bool RequestUriParameterSupported { get; set; }

    [JsonPropertyName("code_challenge_methods_supported")]
    public string[] CodeChallengeMethodsSupported { get; set; }

    [JsonPropertyName("tls_client_certificate_bound_access_tokens")]
    public bool TlsClientCertificateBoundAccessTokens { get; set; }

    [JsonPropertyName("introspection_endpoint")]
    public Uri IntrospectionEndpoint { get; set; }
}
