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

using Newtonsoft.Json;

namespace CatenaX.NetworkServices.Keycloak.Library.Models.AuthenticationManagement;

public class RequiredActionProvider
{
    [JsonProperty("alias")]
    public string Alias { get; set; }
    [JsonProperty("config")]
    public IDictionary<string, object> Config { get; set; }
    [JsonProperty("defaultAction")]
    public bool? DefaultAction { get; set; }
    [JsonProperty("enabled")]
    public bool? Enabled { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("priority")]
    public int? Priority { get; set; }
    [JsonProperty("providerId")]
    public string ProviderId { get; set; }
}
