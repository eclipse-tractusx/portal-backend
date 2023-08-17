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

public class SmtpServer
{
    [JsonProperty("host")]
    public string? Host { get; set; }
    [JsonProperty("ssl")]
    public string? Ssl { get; set; }
    [JsonProperty("starttls")]
    public string? StartTls { get; set; }
    [JsonProperty("user")]
    public string? User { get; set; }
    [JsonProperty("password")]
    public string? Password { get; set; }
    [JsonProperty("auth")]
    public string? Auth { get; set; }
    [JsonProperty("from")]
    public string? From { get; set; }
    [JsonProperty("fromDisplayName")]
    public string? FromDisplayName { get; set; }
    [JsonProperty("replyTo")]
    public string? ReplyTo { get; set; }
    [JsonProperty("replyToDisplayName")]
    public string? ReplyToDisplayName { get; set; }
    [JsonProperty("envelopeFrom")]
    public string? EnvelopeFrom { get; set; }
    [JsonProperty("port")]
    public string? Port { get; set; }
}
