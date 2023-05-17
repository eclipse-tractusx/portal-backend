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

public class IdentityProvider
{
    [JsonProperty("alias")]
    public string Alias { get; set; }
    [JsonProperty("internalId")]
    public string InternalId { get; set; }
    [JsonProperty("providerId")]
    public string ProviderId { get; set; }
    [JsonProperty("enabled")]
    public bool? Enabled { get; set; }
    [JsonProperty("updateProfileFirstLoginMode")]
    public string UpdateProfileFirstLoginMode { get; set; }
    [JsonProperty("trustEmail")]
    public bool? TrustEmail { get; set; }
    [JsonProperty("storeToken")]
    public bool? StoreToken { get; set; }
    [JsonProperty("addReadTokenRoleOnCreate")]
    public bool? AddReadTokenRoleOnCreate { get; set; }
    [JsonProperty("authenticateByDefault")]
    public bool? AuthenticateByDefault { get; set; }
    [JsonProperty("linkOnly")]
    public bool? LinkOnly { get; set; }
    [JsonProperty("firstBrokerLoginFlowAlias")]
    public string FirstBrokerLoginFlowAlias { get; set; }
    [JsonProperty("postBrokerLoginFlowAlias")]
    public string PostBrokerLoginFlowAlias { get; set; }
    [JsonProperty("displayName")]
    public string DisplayName { get; set; }
    [JsonProperty("config")]
    public Config Config { get; set; }
}
