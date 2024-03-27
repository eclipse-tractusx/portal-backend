/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Root;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Common.Converters;

public class NameConverter : JsonEnumConverter<Name>
{
    private static readonly Dictionary<Name, string> s_pairs = new Dictionary<Name, string>
    {
        [Name.Base] = "base",
        [Name.Keycloak] = "keycloak",
        [Name.RhSso] = "rh-sso"
    };

    protected override string EntityString { get; } = nameof(Name).ToLower();

    protected override string ConvertToString(Name value) => s_pairs[value];

    protected override Name ConvertFromString(string s)
    {
        var pair = s_pairs.FirstOrDefault(kvp => kvp.Value.Equals(s, StringComparison.OrdinalIgnoreCase));
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (EqualityComparer<KeyValuePair<Name, string>>.Default.Equals(pair))
        {
            throw new ArgumentException($"Unknown {EntityString}: {s}");
        }

        return pair.Key;
    }
}
