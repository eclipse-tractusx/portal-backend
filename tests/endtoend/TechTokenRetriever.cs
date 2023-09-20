/********************************************************************************
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

using RestAssured.Response.Logging;
using System.IdentityModel.Tokens.Jwt;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public static class TechTokenRetriever
{
    public static string GetToken(string tokenUrl, string clientId, string? clientSecret)
    {
        if (clientSecret is null)
        {
            throw new Exception("No client secret provided while trying to get a token.");
        }
        var formData = new[]
        {
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "openid"),
            new KeyValuePair<string, string>("client_id", clientId),
        };

        var token = Given()
            .ContentType("application/x-www-form-urlencoded")
            .FormData(formData)
            .When()
            .Post(tokenUrl)
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .And()
            .Extract()
            .Body("$.access_token").ToString();
        return token ?? throw new Exception("No token received");
    }
}
