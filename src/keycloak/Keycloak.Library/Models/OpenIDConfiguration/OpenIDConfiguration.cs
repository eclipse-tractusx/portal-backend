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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.OpenIDConfiguration;

public class OpenIDConfiguration
{
    [JsonProperty("issuer")]
    public Uri Issuer { get; set; }

    [JsonProperty("authorization_endpoint")]
    public Uri AuthorizationEndpoint { get; set; }

    [JsonProperty("token_endpoint")]
    public Uri TokenEndpoint { get; set; }

    [JsonProperty("token_introspection_endpoint")]
    public Uri TokenIntrospectionEndpoint { get; set; }

    [JsonProperty("userinfo_endpoint")]
    public Uri UserinfoEndpoint { get; set; }

    [JsonProperty("end_session_endpoint")]
    public Uri EndSessionEndpoint { get; set; }

    [JsonProperty("jwks_uri")]
    public Uri JwksUri { get; set; }

    [JsonProperty("check_session_iframe")]
    public Uri CheckSessionIframe { get; set; }

    [JsonProperty("grant_types_supported")]
    public string[] GrantTypesSupported { get; set; }

    [JsonProperty("response_types_supported")]
    public string[] ResponseTypesSupported { get; set; }

    [JsonProperty("subject_types_supported")]
    public string[] SubjectTypesSupported { get; set; }

    [JsonProperty("id_token_signing_alg_values_supported")]
    public string[] IdTokenSigningAlgValuesSupported { get; set; }

    [JsonProperty("id_token_encryption_alg_values_supported")]
    public string[] IdTokenEncryptionAlgValuesSupported { get; set; }

    [JsonProperty("id_token_encryption_enc_values_supported")]
    public string[] IdTokenEncryptionEncValuesSupported { get; set; }

    [JsonProperty("userinfo_signing_alg_values_supported")]
    public string[] UserinfoSigningAlgValuesSupported { get; set; }

    [JsonProperty("request_object_signing_alg_values_supported")]
    public string[] RequestObjectSigningAlgValuesSupported { get; set; }

    [JsonProperty("response_modes_supported")]
    public string[] ResponseModesSupported { get; set; }

    [JsonProperty("registration_endpoint")]
    public Uri RegistrationEndpoint { get; set; }

    [JsonProperty("token_endpoint_auth_methods_supported")]
    public string[] TokenEndpointAuthMethodsSupported { get; set; }

    [JsonProperty("token_endpoint_auth_signing_alg_values_supported")]
    public string[] TokenEndpointAuthSigningAlgValuesSupported { get; set; }

    [JsonProperty("claims_supported")]
    public string[] ClaimsSupported { get; set; }

    [JsonProperty("claim_types_supported")]
    public string[] ClaimTypesSupported { get; set; }

    [JsonProperty("claims_parameter_supported")]
    public bool ClaimsParameterSupported { get; set; }

    [JsonProperty("scopes_supported")]
    public string[] ScopesSupported { get; set; }

    [JsonProperty("request_parameter_supported")]
    public bool RequestParameterSupported { get; set; }

    [JsonProperty("request_uri_parameter_supported")]
    public bool RequestUriParameterSupported { get; set; }

    [JsonProperty("code_challenge_methods_supported")]
    public string[] CodeChallengeMethodsSupported { get; set; }

    [JsonProperty("tls_client_certificate_bound_access_tokens")]
    public bool TlsClientCertificateBoundAccessTokens { get; set; }

    [JsonProperty("introspection_endpoint")]
    public Uri IntrospectionEndpoint { get; set; }
}
