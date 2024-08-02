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
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.Net.Http.Json;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Dim.Library;

public class DimService(
    ITokenService tokenService,
    IHttpClientFactory httpClientFactory,
    IOptions<DimSettings> settings)
    : IDimService
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly DimSettings _settings = settings.Value;

    /// <inhertidoc />
    public async Task<bool> CreateWalletAsync(string companyName, string bpn, string didDocumentLocation, CancellationToken cancellationToken)
    {
        using var httpClient = await tokenService.GetAuthorizedClient<DimService>(_settings, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await httpClient.PostAsJsonAsync($"setup-dim?companyName={Uri.EscapeDataString(companyName)}&bpn={Uri.EscapeDataString(bpn)}&didDocumentLocation={Uri.EscapeDataString(didDocumentLocation)}", Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("dim-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> ValidateDid(string did, CancellationToken cancellationToken)
    {
        using var httpClient = httpClientFactory.CreateClient("universalResolver");
        var result = await httpClient.GetAsync($"1.0/identifiers/{Uri.EscapeDataString(did)}", cancellationToken)
            .CatchingIntoServiceExceptionFor("validate-did", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);

        if (!result.IsSuccessStatusCode)
        {
            return false;
        }

        var validationResult = await result.Content.ReadFromJsonAsync<DidValidationResult>(Options, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return validationResult != null && string.IsNullOrWhiteSpace(validationResult.DidResolutionMetadata.Error);
    }

    /// <inhertidoc />
    public async Task CreateTechnicalUser(string bpn, TechnicalUserData technicalUserData, CancellationToken cancellationToken)
    {
        using var httpClient = await tokenService.GetAuthorizedClient<DimService>(_settings, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await httpClient.PostAsJsonAsync($"technical-user/{Uri.EscapeDataString(bpn)}", technicalUserData, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("technical-user-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);
    }

    /// <inhertidoc />
    public async Task DeleteTechnicalUser(string bpn, TechnicalUserData technicalUserData, CancellationToken cancellationToken)
    {
        using var httpClient = await tokenService.GetAuthorizedClient<DimService>(_settings, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await httpClient.PostAsJsonAsync($"technical-user/{Uri.EscapeDataString(bpn)}/delete", technicalUserData, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("technical-user-delete-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);
    }
}
