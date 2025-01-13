/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.BpnDidResolver.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.Net.Http.Json;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.BpnDidResolver.Library;

public class BpnDidResolverService : IBpnDidResolverService
{
    private readonly ITokenService _tokenService;
    private readonly BpnDidResolverSettings _settings;
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// Creates a new instance of <see cref="BpnDidResolverService"/>
    /// </summary>
    /// <param name="tokenService"></param>
    /// <param name="options"></param>
    public BpnDidResolverService(ITokenService tokenService, IOptions<BpnDidResolverSettings> options)
    {
        _tokenService = tokenService;
        _settings = options.Value;
    }

    public async Task<bool> TransmitDidAndBpn(string did, string bpn, CancellationToken cancellationToken)
    {
        using var httpClient = await _tokenService.GetAuthorizedClient<BpnDidResolverService>(_settings, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var data = new BpnMappingData(bpn, did);

        async ValueTask<(bool, string?)> CreateErrorMessage(HttpResponseMessage errorResponse) =>
            (false, (await errorResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)));

        var result = await httpClient.PostAsJsonAsync("api/management/bpn-directory", data, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("transmit-did-bpn", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE, CreateErrorMessage).ConfigureAwait(false);
        return result.IsSuccessStatusCode;
    }
}
