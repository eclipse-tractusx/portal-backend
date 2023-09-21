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

using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Common.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
    private ISerializer _serializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
    });

    private readonly Url _url;
    private readonly string? _userName;
    private readonly string? _password;
    private readonly string? _clientSecret;
    private readonly Func<CancellationToken, Task<string>>? _getTokenAsync;
    private readonly string? _authRealm;
    private readonly string? _clientId;
    private readonly bool _useAuthTrail;

    private KeycloakClient(string url)
    {
        _url = url;
    }

    public KeycloakClient(string url, string? userName, string? password, string? authRealm, bool useAuthTrail)
        : this(url)
    {
        _userName = userName;
        _password = password;
        _authRealm = authRealm;
        _useAuthTrail = useAuthTrail;
    }

    private KeycloakClient(string url, string? userName, string? password, string? authRealm, string? clientId, string? clientSecret, bool useAuthTrail)
        : this(url)
    {
        _userName = userName;
        _password = password;
        _clientSecret = clientSecret;
        _clientId = clientId;
        _authRealm = authRealm;
        _useAuthTrail = useAuthTrail;
    }

    public KeycloakClient(string url, Func<string> getToken, string? authRealm = null)
        : this(url)
    {
        _getTokenAsync = (_) => Task.FromResult(getToken());
        _authRealm = authRealm;
    }

    public KeycloakClient(string url, Func<CancellationToken, Task<string>> getTokenAsync, string? authRealm = null)
        : this(url)
    {
        _getTokenAsync = getTokenAsync;
        _authRealm = authRealm;
    }

    public static KeycloakClient CreateWithClientId(string url, string? clientId, string? clientSecret, bool useAuthTrail, string? authRealm = null)
    {
        return new KeycloakClient(url, userName: null, password: null, authRealm, clientId, clientSecret, useAuthTrail);
    }

    public void SetSerializer(ISerializer serializer)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    private Task<IFlurlRequest> GetBaseUrlAsync(string targetRealm, CancellationToken cancellationToken = default)
    {
        var url = new Url(_url);
        if(_useAuthTrail) 
            url = url
            .AppendPathSegment("/auth");

        return url
            .ConfigureRequest(settings => settings.JsonSerializer = _serializer)
            .WithAuthenticationAsync(_getTokenAsync, _url, _authRealm ?? targetRealm, _userName, _password, _clientSecret, _clientId, _useAuthTrail, cancellationToken);
    }
}
