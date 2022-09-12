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

namespace CatenaX.NetworkServices.Keycloak.Library.Models.Root;

public class MemoryInfo
{
    [JsonProperty("total")]
    public long Total { get; set; }

    [JsonProperty("totalFormated")]
    public string TotalFormatted { get; set; }

    [JsonProperty("used")]
    public long Used { get; set; }

    [JsonProperty("usedFormated")]
    public string UsedFormatted { get; set; }

    [JsonProperty("free")]
    public long Free { get; set; }

    [JsonProperty("freePercentage")]
    public long FreePercentage { get; set; }

    [JsonProperty("freeFormated")]
    public string FreeFormatted { get; set; }
}
