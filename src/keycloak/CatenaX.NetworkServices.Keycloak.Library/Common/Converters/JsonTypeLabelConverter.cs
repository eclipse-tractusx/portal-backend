/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using CatenaX.NetworkServices.Keycloak.Library.Models.Root;

namespace CatenaX.NetworkServices.Keycloak.Library.Common.Converters;

public class JsonTypeLabelConverter : JsonEnumConverter<JsonTypeLabel>
{
    private static readonly Dictionary<JsonTypeLabel, string> s_pairs = new Dictionary<JsonTypeLabel, string>
    {
        [JsonTypeLabel.Boolean] = "boolean",
        [JsonTypeLabel.ClientList] = "clientlist",
        [JsonTypeLabel.File] = "file",
        [JsonTypeLabel.List] = "list",
        [JsonTypeLabel.MultivaluedList] = "multivaluedlist",
        [JsonTypeLabel.MultivaluedString] = "multivaluedstring",
        [JsonTypeLabel.Password] = "password",
        [JsonTypeLabel.Role] = "role",
        [JsonTypeLabel.Script] = "script",
        [JsonTypeLabel.String] = "string",
        [JsonTypeLabel.Text] = "text"
    };

    protected override string EntityString { get; } = nameof(JsonTypeLabel).ToLower();

    protected override string ConvertToString(JsonTypeLabel value) => s_pairs[value];

    protected override JsonTypeLabel ConvertFromString(string s)
    {
        var pair = s_pairs.FirstOrDefault(kvp => kvp.Value.Equals(s, StringComparison.OrdinalIgnoreCase));
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (EqualityComparer<KeyValuePair<JsonTypeLabel, string>>.Default.Equals(pair))
        {
            throw new ArgumentException($"Unknown {EntityString}: {s}");
        }

        return pair.Key;
    }
}
