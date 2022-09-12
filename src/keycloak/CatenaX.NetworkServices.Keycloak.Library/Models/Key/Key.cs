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

namespace CatenaX.NetworkServices.Keycloak.Library.Models.Key;

public class Key
{
    [JsonProperty("providerId")]
    public string ProviderId { get; set; }
    [JsonProperty("providerPriority")]
    public int? ProviderPriority { get; set; }
    [JsonProperty("kid")]
    public string Kid { get; set; }
    [JsonProperty("status")]
    public string Status { get; set; }
    [JsonProperty("type")]
    public string Type { get; set; }
    [JsonProperty("algorithm")]
    public string Algorithm { get; set; }
    [JsonProperty("publicKey")]
    public string PublicKey { get; set; }
    [JsonProperty("certificate")]
    public string Certificate { get; set; }
}
