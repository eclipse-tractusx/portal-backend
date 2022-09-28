/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Common.Converters;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.AuthorizationPermissions;
using Newtonsoft.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;

public class Policy
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonConverter(typeof(PolicyTypeConverter))]
    public PolicyType Type { get; set; }

    [JsonConverter(typeof(PolicyDecisionLogicConverter))]
    public PolicyDecisionLogic Logic { get; set; } 

    [JsonConverter(typeof(DecisionStrategiesConverter))]
    public DecisionStrategy DecisionStrategy { get; set; }

    [JsonProperty("config")]
    public PolicyConfig Config { get; set; }
}

public class RolePolicy
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonConverter(typeof(PolicyTypeConverter))]
    public PolicyType Type { get; set; } = PolicyType.Role;

    [JsonConverter(typeof(PolicyDecisionLogicConverter))]
    public PolicyDecisionLogic Logic { get; set; } 

    [JsonConverter(typeof(DecisionStrategiesConverter))]
    public DecisionStrategy DecisionStrategy { get; set; }

    [JsonProperty("roles")]
    public IEnumerable<RoleConfig> RoleConfigs { get; set; }
}

public class RoleConfig
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("required")]
    public bool Required { get; set; }
}

public enum PolicyType
{
    Role,
    Client,
    Time,
    User,
    Aggregate,
    Group,
    Js
}

public class PolicyConfig
{
    [JsonProperty("roles")]
    public IEnumerable<RoleConfig> RoleConfigs { get; set; }
}
