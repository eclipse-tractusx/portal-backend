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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Common.Converters;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.AuthorizationPermissions;

public class AuthorizationPermission
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonConverter(typeof(AuthorizationPermissionTypeConverter))]
    public AuthorizationPermissionType Type { get; set; }

    [JsonConverter(typeof(PolicyDecisionLogicConverter))]
    public PolicyDecisionLogic Logic { get; set; }

    [JsonConverter(typeof(DecisionStrategiesConverter))]
    public DecisionStrategy DecisionStrategy { get; set; }

    [JsonProperty("resourceType")]
    public string ResourceType { get; set; }

    [JsonProperty("resources")]
    public IEnumerable<string> ResourceIds { get; set; }

    [JsonProperty("scopes")]
    public IEnumerable<string> ScopeIds { get; set; }

    [JsonProperty("policies")]
    public IEnumerable<string> PolicyIds { get; set; }
}

public enum PolicyDecisionLogic
{
    Positive,
    Negative
}

public enum AuthorizationPermissionType
{
    Scope,
    Resource
}

public enum DecisionStrategy
{
    Unanimous,
    Affirmative,
    Consensus
}
