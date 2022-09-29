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

using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Models;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;

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
    public async Task<ConnectorData> CreateConnectorAsync(ConnectorInputModel connectorInputModel, string accessToken, string iamUserId, bool isManaged)
    {
        var companyData = await ValidateCompanyDataAsync(connectorInputModel).ConfigureAwait(false);

        if (isManaged)
        {
            // TODO (PS): Check possibility for technical user
            var (iamUserCompanyId, _) = await _portalRepositories.GetInstance<IUserRepository>().GetOwnCompanAndCompanyUseryId(iamUserId).ConfigureAwait(false);
            if (iamUserCompanyId != connectorInputModel.Host)
            {
                throw new ControllerArgumentException(
                    $"CompanyId {iamUserCompanyId} does not match the host company {connectorInputModel.Host}",
                    nameof(connectorInputModel.Host));
            }
        }
        
        var createdConnector = await CreateAndRegisterConnectorAsync(connectorInputModel, accessToken, companyData).ConfigureAwait(false);
        return new ConnectorData(createdConnector.Name, createdConnector.LocationId)
        {
            Id = createdConnector.Id,
            Status = createdConnector.StatusId,
            Type = createdConnector.TypeId
        };
    }

    private async Task<List<(Guid CompanyId, string? BusinessPartnerNumber)>> ValidateCompanyDataAsync(ConnectorInputModel connectorInputModel)
    {
        if (!await _portalRepositories.GetInstance<ICountryRepository>()
                .CheckCountryExistsByAlpha2CodeAsync(connectorInputModel.Location.ToUpper()).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"Location {connectorInputModel.Location} does not exist", nameof(connectorInputModel.Location));
        }

        var parameters = connectorInputModel.Provider == connectorInputModel.Host || !connectorInputModel.Host.HasValue
            ? Enumerable.Repeat(((Guid companyId, bool bpnRequested)) new ValueTuple<Guid, bool>(connectorInputModel.Provider, true), 1)
            : (IEnumerable<(Guid companyId, bool bpnRequested)>) new [] { (connectorInputModel.Provider, true), (connectorInputModel.Host.Value, false) }.AsEnumerable();
        var companyData = await _portalRepositories
            .GetInstance<ICompanyRepository>()
            .GetConnectorCreationCompanyDataAsync(parameters)
            .ToListAsync().ConfigureAwait(false);

        if (companyData.All(data => data.CompanyId != connectorInputModel.Provider))
        {
            throw new ControllerArgumentException($"Company {connectorInputModel.Provider} does not exist", nameof(connectorInputModel.Provider));
        }

        if (connectorInputModel.Provider != connectorInputModel.Host && connectorInputModel.Host.HasValue && companyData.All(data => data.CompanyId != connectorInputModel.Host))
        {
            throw new ControllerArgumentException($"Company {connectorInputModel.Host} does not exist", nameof(connectorInputModel.Host));
        }

        return companyData;
    }

    private async Task<Connector> CreateAndRegisterConnectorAsync(ConnectorInputModel connectorInputModel, string accessToken, IEnumerable<(Guid CompanyId, string? BusinessPartnerNumber)> companyData)
    {
        var (name, connectorUrl, type, status, location, provider, host) = connectorInputModel;

        var providerBusinessPartnerNumber = companyData.Single(data => data.CompanyId == provider).BusinessPartnerNumber;

        if (providerBusinessPartnerNumber == null)
        {
            throw new UnexpectedConditionException($"provider company {provider} has no businessPartnerNumber assigned");
        }

        var createdConnector = _portalRepositories.GetInstance<IConnectorsRepository>().CreateConnector(name,
            location.ToUpper(), connectorUrl,
            (connector) =>
            {
                connector.ProviderId = provider;
                connector.HostId = host;
                connector.TypeId = type;
                connector.StatusId = status;
            });

        var documentId = await _connectorsSdFactoryService
            .RegisterConnectorAsync(connectorInputModel, accessToken, providerBusinessPartnerNumber)
            .ConfigureAwait(false);
        createdConnector.SelfDescriptionDocumentId = documentId;

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return createdConnector;
    }

    /// <inheritdoc/>
    public async Task DeleteConnectorAsync(Guid connectorId)
    {
        await _portalRepositories.GetInstance<IConnectorsRepository>().DeleteConnectorAsync(connectorId).ConfigureAwait(false);
    }
}
