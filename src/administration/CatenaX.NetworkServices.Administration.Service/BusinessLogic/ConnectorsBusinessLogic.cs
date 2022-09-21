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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CatenaX.NetworkServices.PortalBackend.DBAccess;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IConnectorsBusinessLogic"/> making use of <see cref="IConnectorsRepository"/> to retrieve data.
/// </summary>
public class ConnectorsBusinessLogic : IConnectorsBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IConnectorsSdFactoryService _connectorsSdFactoryService;
    private readonly ConnectorsSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Access to the needed repositories</param>
    /// <param name="options">The options</param>
    /// <param name="connectorsSdFactoryService">Access to the connectorsSdFactory</param>
    public ConnectorsBusinessLogic(IPortalRepositories portalRepositories, IOptions<ConnectorsSettings> options, IConnectorsSdFactoryService connectorsSdFactoryService)
    {
        _portalRepositories = portalRepositories;
        _settings = options.Value;
        _connectorsSdFactoryService = connectorsSdFactoryService;
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
            throw new ControllerArgumentException($"Location {location} does not exist", nameof(location));
        }

        var parameters = provider == host || !host.HasValue
            ? Enumerable.Repeat(((Guid companyId, bool bpnRequested)) new ValueTuple<Guid, bool>(provider, true), 1)
            : (IEnumerable<(Guid companyId, bool bpnRequested)>) new [] { (provider, true), (host.Value, false) }.AsEnumerable();
        var companyData = await _portalRepositories
            .GetInstance<ICompanyRepository>()
            .GetConnectorCreationCompanyDataAsync(parameters)
            .ToListAsync().ConfigureAwait(false);

        if (companyData.All(data => data.CompanyId != provider))
        {
            throw new ControllerArgumentException($"Company {provider} does not exist", nameof(provider));
        }

        if (provider != host && host.HasValue && companyData.All(data => data.CompanyId != host))
        {
            throw new ControllerArgumentException($"Company {host} does not exist", nameof(host));
        }

        var providerBusinessPartnerNumber = companyData.Single(data => data.CompanyId == provider).BusinessPartnerNumber;

        if (providerBusinessPartnerNumber == null)
        {
            throw new UnexpectedConditionException($"provider company {provider} has no businessPartnerNumber assigned");
        }

        var createdConnector = _portalRepositories.GetInstance<IConnectorsRepository>().CreateConnector(name, location.ToUpper(), connectorUrl,
            (connector) =>
            {
                connector.ProviderId = provider;
                connector.HostId = host;
                connector.TypeId = type;
                connector.StatusId = status;
            });

        await _connectorsSdFactoryService.RegisterConnector(connectorInputModel, accessToken, providerBusinessPartnerNumber).ConfigureAwait(false);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);

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
