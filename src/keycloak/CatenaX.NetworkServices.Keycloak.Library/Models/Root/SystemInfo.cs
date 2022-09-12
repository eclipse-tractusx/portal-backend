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

public class SystemInfo
{
    [JsonProperty("version")]
    public string Version { get; set; }

    [JsonProperty("serverTime")]
    public string ServerTime { get; set; }

    [JsonProperty("uptime")]
    public string Uptime { get; set; }

    [JsonProperty("uptimeMillis")]
    public long UptimeMillis { get; set; }

    [JsonProperty("javaVersion")]
    public string JavaVersion { get; set; }

    [JsonProperty("javaVendor")]
    public string JavaVendor { get; set; }

    [JsonProperty("javaVm")]
    public string JavaVm { get; set; }

    [JsonProperty("javaVmVersion")]
    public string JavaVmVersion { get; set; }

    [JsonProperty("javaRuntime")]
    public string JavaRuntime { get; set; }

    [JsonProperty("javaHome")]
    public string JavaHome { get; set; }

    [JsonProperty("osName")]
    public string OsName { get; set; }

    [JsonProperty("osArchitecture")]
    public string OsArchitecture { get; set; }

    [JsonProperty("osVersion")]
    public string OsVersion { get; set; }

    [JsonProperty("fileEncoding")]
    public string FileEncoding { get; set; }

    [JsonProperty("userName")]
    public string UserName { get; set; }

    [JsonProperty("userDir")]
    public string UserDir { get; set; }

    [JsonProperty("userTimezone")]
    public string UserTimezone { get; set; }

    [JsonProperty("userLocale")]
    public string UserLocale { get; set; }
}
