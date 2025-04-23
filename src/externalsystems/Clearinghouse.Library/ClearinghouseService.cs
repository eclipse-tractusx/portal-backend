/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.Net.Http.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library;

public class ClearinghouseService(ITokenService tokenService, IOptions<ClearinghouseSettings> options)
    : IClearinghouseService
{
    private readonly ClearinghouseSettings _settings = options.Value;

    /// <inheritdoc />
    public async Task TriggerCompanyDataPost(ClearinghouseTransferData data, CancellationToken cancellationToken)
    {
        var credentials = _settings.GetCountrySpecificSettings(data.LegalEntity.Address.CountryAlpha2Code);
        using var httpClient = await tokenService.GetAuthorizedClient($"{nameof(ClearinghouseService)}{credentials.CountryAlpha2Code}", credentials, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        async ValueTask<(bool, string?)> CreateErrorMessage(HttpResponseMessage errorResponse) =>
            (false, (await errorResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)));

        await httpClient.PostAsJsonAsync(credentials.ValidationPath, data, cancellationToken)
            .CatchingIntoServiceExceptionFor("clearinghouse-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE, CreateErrorMessage).ConfigureAwait(false);
    }
}
