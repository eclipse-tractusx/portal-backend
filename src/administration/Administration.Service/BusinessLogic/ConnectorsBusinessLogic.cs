/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IConnectorsBusinessLogic"/> making use of <see cref="IConnectorsRepository"/> to retrieve data.
/// </summary>
public class ConnectorsBusinessLogic : IConnectorsBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly ISdFactoryService _sdFactoryService;
    private readonly IDapsService _dapsService;
    private readonly ConnectorsSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Access to the needed repositories</param>
    /// <param name="options">The options</param>
    /// <param name="sdFactoryService">Access to the connectorsSdFactory</param>
    /// <param name="dapsService">Access to the daps service</param>
    public ConnectorsBusinessLogic(IPortalRepositories portalRepositories, IOptions<ConnectorsSettings> options, ISdFactoryService sdFactoryService, IDapsService dapsService)
    {
        _portalRepositories = portalRepositories;
        _settings = options.Value;
        _sdFactoryService = sdFactoryService;
        _dapsService = dapsService;
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
                        new ConnectorData(c.Name, c.Location!.Alpha2Code, c.Id, c.TypeId, c.StatusId)
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
    public Task<ConnectorData> CreateConnectorAsync(ConnectorInputModel connectorInputModel, string accessToken, string iamUserId, CancellationToken cancellationToken)
    {
        ValidateCertificationType(connectorInputModel.Certificate);
        return CreateConnectorInternalAsync(connectorInputModel, accessToken, iamUserId, cancellationToken);
    }

    public Task<ConnectorData> CreateManagedConnectorAsync(ManagedConnectorInputModel connectorInputModel, string accessToken, string iamUserId, CancellationToken cancellationToken)
    {
        ValidateCertificationType(connectorInputModel.Certificate);
        return CreateManagedConnectorInternalAsync(connectorInputModel, accessToken, iamUserId, cancellationToken);
    }

    private void ValidateCertificationType(IFormFile? certificate)
    {
        if (certificate != null && !_settings.ValidCertificationContentTypes.Contains(certificate.ContentType))
        {
            throw new UnsupportedMediaTypeException(
                $"Only {string.Join(",", _settings.ValidCertificationContentTypes)} files are allowed.");
        }
    }

    private async Task<ConnectorData> CreateConnectorInternalAsync(ConnectorInputModel connectorInputModel, string accessToken, string iamUserId, CancellationToken cancellationToken)
    {
        var (name, connectorUrl, status, location, certificate) = connectorInputModel;
        await CheckLocationExists(location);

        var companyId = await GetCompanyOfUserOrTechnicalUser(iamUserId).ConfigureAwait(false);
        var providerBpn = await _portalRepositories
            .GetInstance<ICompanyRepository>()
            .GetCompanyBpnByIdAsync(companyId)
            .ConfigureAwait(false);

        if (string.IsNullOrEmpty(providerBpn))
        {
            throw new UnexpectedConditionException($"provider company {companyId} has no businessPartnerNumber assigned");
        }

        var connectorRequestModel = new ConnectorRequestModel(name, connectorUrl, ConnectorTypeId.COMPANY_CONNECTOR, status,
            location, companyId, companyId);
        var createdConnector = await CreateAndRegisterConnectorAsync(
            connectorRequestModel,
            accessToken,
            providerBpn,
            certificate,
            cancellationToken).ConfigureAwait(false);
        return new ConnectorData(createdConnector.Name, createdConnector.LocationId, createdConnector.Id,
            createdConnector.TypeId, createdConnector.StatusId);
    }

    private async Task<ConnectorData> CreateManagedConnectorInternalAsync(ManagedConnectorInputModel connectorInputModel, string accessToken, string iamUserId, CancellationToken cancellationToken)
    {
        var companyId = await GetCompanyOfUserOrTechnicalUser(iamUserId).ConfigureAwait(false);
        var (name, connectorUrl, status, location, providerBpn, certificate) = connectorInputModel;
        await CheckLocationExists(location).ConfigureAwait(false);

        var providerId = await _portalRepositories
            .GetInstance<ICompanyRepository>()
            .GetCompanyIdByBpnAsync(providerBpn)
            .ConfigureAwait(false);

        if (providerId == Guid.Empty)
        {
            throw new ControllerArgumentException($"Company {providerBpn} does not exist", nameof(providerBpn));
        }

        var connectorRequestModel = new ConnectorRequestModel(name, connectorUrl, ConnectorTypeId.CONNECTOR_AS_A_SERVICE,
            status, location, providerId, companyId);
        var createdConnector = await CreateAndRegisterConnectorAsync(
            connectorRequestModel,
            accessToken,
            providerBpn,
            certificate,
            cancellationToken).ConfigureAwait(false);
        return new ConnectorData(createdConnector.Name, createdConnector.LocationId, createdConnector.Id,
            createdConnector.TypeId, createdConnector.StatusId);
    }

    private async Task<Guid> GetCompanyOfUserOrTechnicalUser(string iamUserId)
    {
        var iamUserCompanyId = await _portalRepositories.GetInstance<IUserRepository>().GetOwnCompanyId(iamUserId)
            .ConfigureAwait(false);
        // if not check for technical user
        if (iamUserCompanyId == Guid.Empty)
        {
            iamUserCompanyId = await _portalRepositories.GetInstance<IUserRepository>()
                .GetServiceAccountCompany(iamUserId)
                .ConfigureAwait(false);
        }

        return iamUserCompanyId;
    }

    private async Task CheckLocationExists(string location)
    {
        if (!await _portalRepositories.GetInstance<ICountryRepository>()
                .CheckCountryExistsByAlpha2CodeAsync(location.ToUpper()).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"Location {location} does not exist", nameof(location));
        }
    }

    private async Task<Connector> CreateAndRegisterConnectorAsync(ConnectorRequestModel connectorInputModel,
        string accessToken, string businessPartnerNumber, IFormFile? file, CancellationToken cancellationToken)
    {
        var (name, connectorUrl, type, status, location, provider, host) = connectorInputModel;

        var connectorsRepository = _portalRepositories.GetInstance<IConnectorsRepository>();
        var createdConnector = connectorsRepository.CreateConnector(
            name,
            location.ToUpper(),
            connectorUrl,
            connector =>
            {
                connector.ProviderId = provider;
                connector.HostId = host;
                connector.TypeId = type;
                connector.StatusId = status;
            });

        if (file is not null)
        {
            var dapsCallSuccessful = false;
            try
            {
                dapsCallSuccessful = await _dapsService
                    .EnableDapsAuthAsync(name, accessToken, connectorUrl, businessPartnerNumber, file, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (ServiceException)
            {
                // No error should be visible for the user
            }

            createdConnector.DapsRegistrationSuccessful = dapsCallSuccessful;
        }

        var documentId = await _sdFactoryService
            .RegisterConnectorAsync(connectorInputModel, accessToken, businessPartnerNumber, cancellationToken)
            .ConfigureAwait(false);
        createdConnector.SelfDescriptionDocumentId = documentId;

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return createdConnector;
    }

    /// <inheritdoc/>
    public Task DeleteConnectorAsync(Guid connectorId) =>
        _portalRepositories.GetInstance<IConnectorsRepository>().DeleteConnectorAsync(connectorId);

    /// <inheritdoc/>
    public IAsyncEnumerable<ConnectorEndPointData> GetCompanyConnectorEndPointAsync(IEnumerable<string> bpns) =>
        _portalRepositories.GetInstance<IConnectorsRepository>()
            .GetConnectorEndPointDataAsync(bpns)
            .PreSortedGroupBy(data => data.BusinessPartnerNumber)
            .Select(group =>
                new ConnectorEndPointData(
                    group.Key,
                    group.Select(x => x.ConnectorEndpoint)));

    /// <inheritdoc />
    public async Task<bool> TriggerDapsAsync(Guid connectorId, IFormFile certificate, string accessToken, string iamUserId, CancellationToken cancellationToken)
    {
        var connectorsRepository = _portalRepositories
            .GetInstance<IConnectorsRepository>();
        var connector = await connectorsRepository
            .GetConnectorInformationByIdForIamUser(connectorId, iamUserId)
            .ConfigureAwait(false);
        
        if (connector == default)
        {
            throw new NotFoundException($"Connector {connectorId} does not exists");
        }

        if (!connector.IsProviderUser)
        {
            throw new ForbiddenException("User is not provider of the connector");
        }

        var connectorData = connector.ConnectorInformationData;
        var dapsCallSuccessful = await _dapsService
            .EnableDapsAuthAsync(connectorData.Name, accessToken, connectorData.Url, connectorData.Bpn, certificate, cancellationToken)
            .ConfigureAwait(false);
        connectorsRepository.AttachAndModifyConnector(connectorId, con =>
        {
            con.DapsRegistrationSuccessful = dapsCallSuccessful;
        });
        
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return dapsCallSuccessful;
    }
}
