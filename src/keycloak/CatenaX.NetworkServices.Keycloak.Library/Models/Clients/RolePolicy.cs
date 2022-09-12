/********************************************************************************
 * Copyright (c) 2021,2022 Contributors to https://github.com/lvermeulen/Keycloak.Net.git and BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using CatenaX.NetworkServices.Keycloak.Library.Common.Converters;
using CatenaX.NetworkServices.Keycloak.Library.Models.AuthorizationPermissions;
using Newtonsoft.Json;

namespace CatenaX.NetworkServices.Keycloak.Library.Models.Clients;

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
