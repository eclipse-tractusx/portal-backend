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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.AuthorizationPermissions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;

public class Policy
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonConverter(typeof(EnumMemberConverter<PolicyType>))]
    public PolicyType Type { get; set; }

    [JsonConverter(typeof(EnumMemberConverter<PolicyDecisionLogic>))]
    public PolicyDecisionLogic Logic { get; set; }

    [JsonConverter(typeof(EnumMemberConverter<DecisionStrategy>))]
    public DecisionStrategy DecisionStrategy { get; set; }

    [JsonPropertyName("config")]
    public PolicyConfig Config { get; set; }
}

public class RolePolicy
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonConverter(typeof(EnumMemberConverter<PolicyType>))]
    public PolicyType Type { get; set; } = PolicyType.Role;

    [JsonConverter(typeof(EnumMemberConverter<PolicyDecisionLogic>))]
    public PolicyDecisionLogic Logic { get; set; }

    [JsonConverter(typeof(EnumMemberConverter<DecisionStrategy>))]
    public DecisionStrategy DecisionStrategy { get; set; }

    [JsonPropertyName("roles")]
    public IEnumerable<RoleConfig> RoleConfigs { get; set; }
}

public class RoleConfig
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("required")]
    public bool Required { get; set; }
}

public enum PolicyType
{
    [EnumMember(Value = "role")]
    Role,

    [EnumMember(Value = "client")]
    Client,

    [EnumMember(Value = "time")]
    Time,

    [EnumMember(Value = "user")]
    User,

    [EnumMember(Value = "aggregate")]
    Aggregate,

    [EnumMember(Value = "group")]
    Group,

    [EnumMember(Value = "js")]
    Js
}

public class PolicyConfig
{
    [JsonPropertyName("roles")]
    public IEnumerable<RoleConfig> RoleConfigs { get; set; }
}
