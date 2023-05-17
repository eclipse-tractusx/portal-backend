/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

using Newtonsoft.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;

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
