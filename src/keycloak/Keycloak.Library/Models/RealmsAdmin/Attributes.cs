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

using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;

public class Attributes
{
    [JsonPropertyName("_browser_headerxXSSProtection")]
    public string BrowserHeaderxXssProtection { get; set; }
    [JsonPropertyName("_browser_headerxFrameOptions")]
    public string BrowserHeaderxFrameOptions { get; set; }
    [JsonPropertyName("_browser_headerstrictTransportSecurity")]
    public string BrowserHeaderstrictTransportSecurity { get; set; }
    [JsonPropertyName("permanentLockout")]
    public string PermanentLockout { get; set; }
    [JsonPropertyName("quickLoginCheckMilliSeconds")]
    public string QuickLoginCheckMilliSeconds { get; set; }
    [JsonPropertyName("_browser_headerxRobotsTag")]
    public string BrowserHeaderxRobotsTag { get; set; }
    [JsonPropertyName("maxFailureWaitSeconds")]
    public string MaxFailureWaitSeconds { get; set; }
    [JsonPropertyName("minimumQuickLoginWaitSeconds")]
    public string MinimumQuickLoginWaitSeconds { get; set; }
    [JsonPropertyName("failureFactor")]
    public string FailureFactor { get; set; }
    [JsonPropertyName("actionTokenGeneratedByUserLifespan")]
    public string ActionTokenGeneratedByUserLifespan { get; set; }
    [JsonPropertyName("maxDeltaTimeSeconds")]
    public string MaxDeltaTimeSeconds { get; set; }
    [JsonPropertyName("_browser_headerxContentTypeOptions")]
    public string BrowserHeaderxContentTypeOptions { get; set; }
    [JsonPropertyName("offlineSessionMaxLifespan")]
    public string OfflineSessionMaxLifespan { get; set; }
    [JsonPropertyName("actionTokenGeneratedByAdminLifespan")]
    public string ActionTokenGeneratedByAdminLifespan { get; set; }
    [JsonPropertyName("_browser_headercontentSecurityPolicyReportOnly")]
    public string BrowserHeadercontentSecurityPolicyReportOnly { get; set; }
    [JsonPropertyName("bruteForceProtected")]
    public string BruteForceProtected { get; set; }
    [JsonPropertyName("_browser_headercontentSecurityPolicy")]
    public string BrowserHeadercontentSecurityPolicy { get; set; }
    [JsonPropertyName("waitIncrementSeconds")]
    public string WaitIncrementSeconds { get; set; }
    [JsonPropertyName("offlineSessionMaxLifespanEnabled")]
    public string OfflineSessionMaxLifespanEnabled { get; set; }
}
