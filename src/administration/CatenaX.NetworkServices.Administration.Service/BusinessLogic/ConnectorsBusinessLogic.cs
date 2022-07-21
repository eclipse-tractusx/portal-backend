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

using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IConnectorsBusinessLogic"/> making use of <see cref="IConnectorsRepository"/> to retrieve data.
/// </summary>
public class ConnectorsBusinessLogic : IConnectorsBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly ConnectorsSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Access to the needed repositories</param>
    /// <param name="options">The options</param>
    public ConnectorsBusinessLogic(IPortalRepositories portalRepositories, IOptions<ConnectorsSettings> options)
    {
        _portalRepositories = portalRepositories;
        _settings = options.Value;
    }

    /// <inheritdoc/>
    public Task<Pagination.Response<ConnectorData>> GetAllCompanyConnectorDatasForIamUserAsyncEnum(string iamUserId, int page, int size)
    {
        var connectors = _portalRepositories.GetInstance<IConnectorsRepository>().GetAllCompanyConnectorsForIamUser(iamUserId);

        return Pagination.CreateResponseAsync(page, size, _settings.MaxPageSize, (skip, take) =>
            new Pagination.AsyncSource<ConnectorData>
            (
                connectors.CountAsync(),
                connectors.OrderByDescending(connector => connector.Name)
                    .Skip(skip)
                    .Take(take)
                    .Select(c => 
                        new ConnectorData(c.Name, c.Location!.Alpha2Code)
                        {
                            Id = c.Id,
                            Status = c.Status!.Id,
                            Type = c.Type!.Id
                        }
                    ).AsAsyncEnumerable()
            )
        );
    }

    public async Task<ConnectorData> GetCompanyConnectorDataForIdIamUserAsync(Guid connectorId, string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<IConnectorsRepository>().GetConnectorByIdForIamUser(connectorId, iamUserId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"connector {connectorId} does not exist");
        }
        if (!result.IsProviderUser)
        {
            throw new ForbiddenException($"user {iamUserId} is not permitted to access connector {connectorId}");
        }
        return result.ConnectorData;
    }

    /// <inheritdoc/>
    public async Task<ConnectorData> CreateConnectorAsync(ConnectorInputModel connectorInputModel, string accessToken)
    {
        var (name, connectorUrl, type, status, location, provider, host) = connectorInputModel;
        if (!await _portalRepositories.GetInstance<ICountryRepository>()
                .CheckCountryExistsByAlpha2CodeAsync(location.ToUpper()).ConfigureAwait(false))
        {
            throw new ArgumentException($"Location {connectorInputModel.Location} does not exist", nameof(location));
        }

        var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();
        if (!await companyRepository.CheckCompanyExistsByIdAsync(provider).ConfigureAwait(false))
        {
            throw new ArgumentException($"Company {connectorInputModel.Provider} does not exist", nameof(provider));
        }

        if (provider != host && host.HasValue && !await companyRepository.CheckCompanyExistsByIdAsync(host.Value).ConfigureAwait(false))
        {
            throw new ArgumentException($"Company {host} does not exist", nameof(host));
        }

        if (!Enum.IsDefined(typeof(ConnectorTypeId), type.ToString()))
            throw new ArgumentException("ConnectorTypeId does not exist.", nameof(type));

        if (!Enum.IsDefined(typeof(ConnectorStatusId), status.ToString()))
            throw new ArgumentException("ConnectorStatusId does not exist.", nameof(status));

        Connector createdConnector;
        HttpResponseMessage response;

        try
        {
            createdConnector = _portalRepositories.GetInstance<IConnectorsRepository>().CreateConnectorAsync(name, location.ToUpper(), connectorUrl,
                (connector) =>
                {
                    connector.ProviderId = provider;
                    connector.HostId = host;
                    connector.TypeId = type;
                    connector.StatusId = status;
                });
            var bpn = (await companyRepository.GetCompanyByIdAsync(connectorInputModel.Provider))!.BusinessPartnerNumber!;
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            // The hardcoded values (headquarterCountry, legalCountry, sdType, issuer) will be fetched from the user input or db in future
            var requestModel = new ConnectorSdFactoryRequestModel(bpn, "DE", "DE", connectorInputModel.ConnectorUrl, "connector", bpn, bpn, "BPNL000000000000");
            response = await httpClient.PostAsJsonAsync(_settings.SdFactoryUrl, requestModel);
        }
        catch (Exception)
        {
            throw;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new ServiceException($"Access to SD factory failed with status code {response.StatusCode}", response.StatusCode);
        }
        
        await _portalRepositories.SaveAsync();

        return new ConnectorData(createdConnector.Name, createdConnector.LocationId)
        {
            Id = createdConnector.Id,
            Status = createdConnector.StatusId,
            Type = createdConnector.TypeId
        };
    }

    /// <inheritdoc/>
    public async Task DeleteConnectorAsync(Guid connectorId)
    {
        await _portalRepositories.GetInstance<IConnectorsRepository>().DeleteConnectorAsync(connectorId).ConfigureAwait(false);
    }
}
