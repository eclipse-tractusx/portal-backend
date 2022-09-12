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

namespace CatenaX.NetworkServices.Keycloak.Library.Models.RealmsAdmin;

public class Attributes
{
    [JsonProperty("_browser_headerxXSSProtection")]
    public string BrowserHeaderxXssProtection { get; set; }
    [JsonProperty("_browser_headerxFrameOptions")]
    public string BrowserHeaderxFrameOptions { get; set; }
    [JsonProperty("_browser_headerstrictTransportSecurity")]
    public string BrowserHeaderstrictTransportSecurity { get; set; }
    [JsonProperty("permanentLockout")]
    public string PermanentLockout { get; set; }
    [JsonProperty("quickLoginCheckMilliSeconds")]
    public string QuickLoginCheckMilliSeconds { get; set; }
    [JsonProperty("_browser_headerxRobotsTag")]
    public string BrowserHeaderxRobotsTag { get; set; }
    [JsonProperty("maxFailureWaitSeconds")]
    public string MaxFailureWaitSeconds { get; set; }
    [JsonProperty("minimumQuickLoginWaitSeconds")]
    public string MinimumQuickLoginWaitSeconds { get; set; }
    [JsonProperty("failureFactor")]
    public string FailureFactor { get; set; }
    [JsonProperty("actionTokenGeneratedByUserLifespan")]
    public string ActionTokenGeneratedByUserLifespan { get; set; }
    [JsonProperty("maxDeltaTimeSeconds")]
    public string MaxDeltaTimeSeconds { get; set; }
    [JsonProperty("_browser_headerxContentTypeOptions")]
    public string BrowserHeaderxContentTypeOptions { get; set; }
    [JsonProperty("offlineSessionMaxLifespan")]
    public string OfflineSessionMaxLifespan { get; set; }
    [JsonProperty("actionTokenGeneratedByAdminLifespan")]
    public string ActionTokenGeneratedByAdminLifespan { get; set; }
    [JsonProperty("_browser_headercontentSecurityPolicyReportOnly")]
    public string BrowserHeadercontentSecurityPolicyReportOnly { get; set; }
    [JsonProperty("bruteForceProtected")]
    public string BruteForceProtected { get; set; }
    [JsonProperty("_browser_headercontentSecurityPolicy")]
    public string BrowserHeadercontentSecurityPolicy { get; set; }
    [JsonProperty("waitIncrementSeconds")]
    public string WaitIncrementSeconds { get; set; }
    [JsonProperty("offlineSessionMaxLifespanEnabled")]
    public string OfflineSessionMaxLifespanEnabled { get; set; }
}
