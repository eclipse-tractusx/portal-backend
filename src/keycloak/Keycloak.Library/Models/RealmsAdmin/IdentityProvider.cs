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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;

public class IdentityProvider
{
    [JsonPropertyName("alias")]
    public string? Alias { get; set; }
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
    [JsonPropertyName("internalId")]
    public string? InternalId { get; set; }
    [JsonPropertyName("providerId")]
    public string? ProviderId { get; set; }
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
    [JsonPropertyName("updateProfileFirstLoginMode")]
    public string? UpdateProfileFirstLoginMode { get; set; }
    [JsonPropertyName("trustEmail")]
    public bool? TrustEmail { get; set; }
    [JsonPropertyName("storeToken")]
    public bool? StoreToken { get; set; }
    [JsonPropertyName("addReadTokenRoleOnCreate")]
    public bool? AddReadTokenRoleOnCreate { get; set; }
    [JsonPropertyName("authenticateByDefault")]
    public bool? AuthenticateByDefault { get; set; }
    [JsonPropertyName("linkOnly")]
    public bool? LinkOnly { get; set; }
    [JsonPropertyName("firstBrokerLoginFlowAlias")]
    public string? FirstBrokerLoginFlowAlias { get; set; }
    [JsonPropertyName("config")]
    public Config? Config { get; set; }
}
