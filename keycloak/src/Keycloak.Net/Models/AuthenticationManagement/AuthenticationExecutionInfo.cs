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

﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Keycloak.Net.Models.AuthenticationManagement
{
    public class AuthenticationExecutionInfo
    {
        [JsonProperty("alias")]
        public string Alias { get; set; }
        [JsonProperty("authenticationConfig")]
        public string AuthenticationConfig { get; set; }
        [JsonProperty("authenticationFlow")]
        public bool? AuthenticationFlow { get; set; }
        [JsonProperty("configurable")]
        public bool? Configurable { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("flowId")]
        public string FlowId { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("index")]
        public int? Index { get; set; }
        [JsonProperty("level")]
        public int? Level { get; set; }
        [JsonProperty("providerId")]
        public string ProviderId { get; set; }
        [JsonProperty("requirement")]
        public string Requirement { get; set; }
        [JsonProperty("requirementChoices")]
        public IEnumerable<string> RequirementChoices { get; set; }
    }
}
