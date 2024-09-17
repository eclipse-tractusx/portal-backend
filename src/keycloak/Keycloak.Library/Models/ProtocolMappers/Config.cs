/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2024 BMW Group AG
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.ProtocolMappers;

public class Config
{
    [JsonProperty("single")]
    public string? Single { get; set; }
    [JsonProperty("attribute.nameformat")]
    public string? AttributeNameFormat { get; set; }
    [JsonProperty("attribute.name")]
    public string? AttributeName { get; set; }
    [JsonProperty("userinfo.token.claim")]
    public string? UserInfoTokenClaim { get; set; }
    [JsonProperty("user.attribute")]
    public string? UserAttribute { get; set; }
    [JsonProperty("id.token.claim")]
    public string? IdTokenClaim { get; set; }
    [JsonProperty("access.token.claim")]
    public string? AccessTokenClaim { get; set; }
    [JsonProperty("introspection.token.claim")]
    public string? IntrospectionTokenClaim { get; set; }
    [JsonProperty("lightweight.claim")]
    public string? LightweightClaim { get; set; }
    [JsonProperty("claim.name")]
    public string? ClaimName { get; set; }
    [JsonProperty("jsonType.label")]
    public string? JsonTypelabel { get; set; }
    [JsonProperty("user.attribute.formatted")]
    public string? UserAttributeFormatted { get; set; }
    [JsonProperty("user.attribute.country")]
    public string? UserAttributeCountry { get; set; }
    [JsonProperty("user.attribute.postal_code")]
    public string? UserAttributePostalCode { get; set; }
    [JsonProperty("user.attribute.street")]
    public string? UserAttributeStreet { get; set; }
    [JsonProperty("user.attribute.region")]
    public string? UserAttributeRegion { get; set; }
    [JsonProperty("user.attribute.locality")]
    public string? UserAttributeLocality { get; set; }
    [JsonProperty("included.client.audience")]
    public string? IncludedClientAudience { get; set; }
    [JsonProperty("included.custom.audience")]
    public string? IncludedCustomAudience { get; set; }
    [JsonProperty("multivalued")]
    public string? Multivalued { get; set; }
    [JsonProperty("user.session.note")]
    public string? UserSessionNote { get; set; }
}
