/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.IO;

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Web;

public static class UrlHelper
{
    private static readonly string[] ValidUriSchemes = { "http", "https" };

    public static void EnsureValidHttpsUrl(this string? url, Func<string>? getUrlParameterName)
    {
        EnsureValidUrlInternal(url, getUrlParameterName, Enumerable.Repeat("https", 1));
    }

    public static void EnsureValidHttpUrl(this string? url, Func<string>? getUrlParameterName)
    {
        EnsureValidUrlInternal(url, getUrlParameterName, ValidUriSchemes);
    }

    private static void EnsureValidUrlInternal(string? url, Func<string>? getUrlParameterName, IEnumerable<string> validUriSchemes)
    {
        var paramName = getUrlParameterName?.Invoke() ?? "";
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ControllerArgumentException($"{paramName} must be set", paramName);
        }

        try
        {
            var uri = new Uri(url);
            if (!uri.IsAbsoluteUri)
            {
                throw new ControllerArgumentException($"url {url} must be an absolute Url",
                    paramName);
            }

            if (!validUriSchemes.Contains(uri.Scheme))
            {
                throw new ControllerArgumentException($"url {url} must either start with http:// or https://",
                    paramName);
            }

            if (!uri.IsWellFormedOriginalString())
            {
                throw new ControllerArgumentException($"url {url} is not wellformed", paramName);
            }
        }
        catch (UriFormatException ufe)
        {
            throw new ControllerArgumentException($"url {url} cannot be parsed: {ufe.Message}", paramName);
        }
    }

    public static string AppendToPathEncoded(this string path, string parameter)
    {
        return $"{path.Trim('/')}/{HttpUtility.UrlEncode(parameter)}";
    }

    public static string AppendToPath(this string path, string parameter)
    {
        return $"{path.Trim('/')}/{parameter}";
    }
}
