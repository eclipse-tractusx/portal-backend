/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
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

using System.Net.Http.Headers;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Microsoft.Extensions.Options;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Service to handle communication with the connectors sd factory
/// </summary>
public class ConnectorsSdFactoryService : IConnectorsSdFactoryService
{
    private readonly ConnectorsSettings _settings;

    /// <summary>
    /// Creates a new instance of <see cref="ConnectorsSdFactoryService"/>
    /// </summary>
    /// <param name="options">The options</param>
    public ConnectorsSdFactoryService(IOptions<ConnectorsSettings> options)
    {
        _settings = options.Value;
    }

    /// <inheritdoc />
    public async Task RegisterConnector(ConnectorInputModel connectorInputModel, string accessToken, string bpn)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        // The hardcoded values (headquarterCountry, legalCountry, sdType, issuer) will be fetched from the user input or db in future
        var requestModel = new ConnectorSdFactoryRequestModel(bpn, "DE", "DE", connectorInputModel.ConnectorUrl,
            "connector", bpn, bpn, "BPNL000000000000");
        var response = await httpClient.PostAsJsonAsync(_settings.SdFactoryUrl, requestModel).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new ServiceException($"Access to SD factory failed with status code {response.StatusCode}", response.StatusCode);
        }
    }
}
