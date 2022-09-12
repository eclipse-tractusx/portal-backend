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

public class AuthenticationExecutionById
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("authenticator")]
    public string Authenticator { get; set; }
    [JsonProperty("authenticatorFlow")]
    public bool? AuthenticatorFlow { get; set; }
    [JsonProperty("requirement")]
    public string Requirement { get; set; }
    [JsonProperty("priority")]
    public int? Priority { get; set; }
    [JsonProperty("parentFlow")]
    public string ParentFlow { get; set; }
    [JsonProperty("optional")]
    public bool? Optional { get; set; }
    [JsonProperty("enabled")]
    public bool? Enabled { get; set; }
    [JsonProperty("required")]
    public bool? Required { get; set; }
    [JsonProperty("alternative")]
    public bool? Alternative { get; set; }
    [JsonProperty("disabled")]
    public bool? Disabled { get; set; }
}
