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

using CatenaX.NetworkServices.Keycloak.Library.Common.Extensions;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CatenaX.NetworkServices.Keycloak.Library;

public partial class KeycloakClient
{
    private ISerializer _serializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
    });

    private readonly Url _url;
    private readonly string _userName;
    private readonly string _password;
    private readonly string _clientSecret;
    private readonly Func<Task<string>> _getTokenAsync;
    private readonly string _authRealm;
    private readonly string _clientId;

    private KeycloakClient(string url)
    {
        _url = url;
    }

    public KeycloakClient(string url, string userName, string password, string authRealm)
        : this(url)
    {
        _userName = userName;
        _password = password;
        _authRealm = authRealm;
    }

    private KeycloakClient(string url, string userName, string password, string authRealm, string clientId, string clientSecret)
        : this(url)
    {
        _userName = userName;
        _password = password;
        _clientSecret = clientSecret;
        _clientId = clientId;
        _authRealm = authRealm;
    }

    public KeycloakClient(string url, Func<string> getToken, string authRealm = null)
        : this(url)
    {
        _getTokenAsync = () => Task.FromResult(getToken());
        _authRealm = authRealm;
    }

    public KeycloakClient(string url, Func<Task<string>> getTokenAsync, string authRealm = null)
        : this(url)
    {
        _getTokenAsync = getTokenAsync;
        _authRealm = authRealm;
    }

    public static KeycloakClient CreateWithClientId(string url, string clientId, string clientSecret, string authRealm = null)
    {
        return new KeycloakClient(url, userName: null, password: null, authRealm, clientId, clientSecret);
    }

    public void SetSerializer(ISerializer serializer)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    private Task<IFlurlRequest> GetBaseUrlAsync(string targetRealm) => new Url(_url)
        .AppendPathSegment("/auth")
        .ConfigureRequest(settings => settings.JsonSerializer = _serializer)
        .WithAuthenticationAsync(_getTokenAsync, _url, _authRealm ?? targetRealm, _userName, _password, _clientSecret, _clientId);
}
