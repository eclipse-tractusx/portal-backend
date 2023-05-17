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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Root;

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
