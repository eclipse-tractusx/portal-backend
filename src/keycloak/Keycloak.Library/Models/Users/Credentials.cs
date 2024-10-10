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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;

public class Credentials
{
    [JsonPropertyName("algorithm")]
    public string? Algorithm { get; set; }
    [JsonPropertyName("config")]
    public IDictionary<string, string>? Config { get; set; }
    [JsonPropertyName("counter")]
    public int? Counter { get; set; }
    [JsonPropertyName("createdDate")]
    public long? CreatedDate { get; set; }
    [JsonPropertyName("device")]
    public string? Device { get; set; }
    [JsonPropertyName("digits")]
    public int? Digits { get; set; }
    [JsonPropertyName("hashIterations")]
    public int? HashIterations { get; set; }
    [JsonPropertyName("hashSaltedValue")]
    public string? HashSaltedValue { get; set; }
    [JsonPropertyName("period")]
    public int? Period { get; set; }
    [JsonPropertyName("salt")]
    public string? Salt { get; set; }
    [JsonPropertyName("temporary")]
    public bool? Temporary { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
