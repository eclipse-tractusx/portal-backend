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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.AuthenticationManagement;

public class AuthenticationFlowExecution
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("requirement")]
    public string? Requirement { get; set; }
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("requirementChoices")]
    public IEnumerable<string>? RequirementChoices { get; set; }
    [JsonPropertyName("configurable")]
    public bool? Configurable { get; set; }
    [JsonPropertyName("providerId")]
    public string? ProviderId { get; set; }
    [JsonPropertyName("level")]
    public int? Level { get; set; }
    [JsonPropertyName("index")]
    public int? Index { get; set; }
    [JsonPropertyName("authenticationFlow")]
    public bool? AuthenticationFlow { get; set; }
    [JsonPropertyName("flowId")]
    public string? FlowId { get; set; }
    [JsonPropertyName("authenticationConfig")]
    public string? AuthenticationConfig { get; set; }
}
