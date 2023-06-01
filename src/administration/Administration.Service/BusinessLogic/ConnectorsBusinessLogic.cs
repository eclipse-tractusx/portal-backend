/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Daps.Library;
using Org.Eclipse.TractusX.Portal.Backend.Daps.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IConnectorsBusinessLogic"/> making use of <see cref="IConnectorsRepository"/> to retrieve data.
/// </summary>
public class ConnectorsBusinessLogic : IConnectorsBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly ISdFactoryBusinessLogic _sdFactoryBusinessLogic;
    private readonly IDapsService _dapsService;
    private readonly ConnectorsSettings _settings;
    private static readonly Regex bpnRegex = new(@"(\w|\d){16}", RegexOptions.None, TimeSpan.FromSeconds(1));

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Access to the needed repositories</param>
    /// <param name="options">The options</param>
    /// <param name="sdFactoryBusinessLogic">Access to the connectorsSdFactory</param>
    /// <param name="dapsService">Access to the daps service</param>
    public ConnectorsBusinessLogic(IPortalRepositories portalRepositories, IOptions<ConnectorsSettings> options, ISdFactoryBusinessLogic sdFactoryBusinessLogic, IDapsService dapsService)
    {
        _portalRepositories = portalRepositories;
        _settings = options.Value;
        _sdFactoryBusinessLogic = sdFactoryBusinessLogic;
        _dapsService = dapsService;
    }

    /// <inheritdoc/>
    public Task<Pagination.Response<ConnectorData>> GetAllCompanyConnectorDatasForIamUserAsync(IdentityData identity, int page, int size)
    {
        var connectors = _portalRepositories.GetInstance<IConnectorsRepository>().GetAllCompanyConnectorsForIamUser(identity.CompanyId);

        return Pagination.CreateResponseAsync(page, size, _settings.MaxPageSize, (skip, take) =>
            new Pagination.AsyncSource<ConnectorData>
            (
                connectors.CountAsync(),
                connectors.OrderByDescending(connector => connector.Name)
                    .Skip(skip)
                    .Take(take)
                    .Select(c =>
                        new ConnectorData(
                            c.Name,
                            c.Location!.Alpha2Code,
                            c.Id,
                            c.TypeId,
                            c.StatusId,
                            c.DapsRegistrationSuccessful,
                            c.HostId,
                            c.Host!.Name,
                            c.SelfDescriptionDocumentId,
                            c.SelfDescriptionDocument!.DocumentName)
                    ).AsAsyncEnumerable()
            )
        );
    }

    /// <inheritdoc/>
    public Task<Pagination.Response<ManagedConnectorData>> GetManagedConnectorForIamUserAsync(IdentityData identity, int page, int size) =>
        Pagination.CreateResponseAsync(
            page,
            size,
            _settings.MaxPageSize,
            _portalRepositories.GetInstance<IConnectorsRepository>().GetManagedConnectorsForIamUser(identity.CompanyId));

    public async Task<ConnectorData> GetCompanyConnectorDataForIdIamUserAsync(Guid connectorId, IdentityData identity)
    {
        var result = await _portalRepositories.GetInstance<IConnectorsRepository>().GetConnectorByIdForIamUser(connectorId, identity.CompanyId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"connector {connectorId} does not exist");
        }
        if (!result.IsProviderUser)
        {
            throw new ForbiddenException($"user {identity.UserEntityId} is not permitted to access connector {connectorId}");
        }
        return result.ConnectorData;
    }

    /// <inheritdoc/>
    public Task<Guid> CreateConnectorAsync(ConnectorInputModel connectorInputModel, IdentityData identity, CancellationToken cancellationToken)
    {
        ValidateCertificationType(connectorInputModel.Certificate);
        return CreateConnectorInternalAsync(connectorInputModel, identity, cancellationToken);
    }

    public Task<Guid> CreateManagedConnectorAsync(ManagedConnectorInputModel connectorInputModel, IdentityData identity, CancellationToken cancellationToken)
    {
        ValidateCertificationType(connectorInputModel.Certificate);
        return CreateManagedConnectorInternalAsync(connectorInputModel, identity, cancellationToken);
    }

    private void ValidateCertificationType(IFormFile? certificate)
    {
        if (certificate != null && !_settings.ValidCertificationContentTypes.Contains(certificate.ContentType))
        {
            throw new UnsupportedMediaTypeException(
                $"Only {string.Join(",", _settings.ValidCertificationContentTypes)} files are allowed.");
        }
    }

    private async Task<Guid> CreateConnectorInternalAsync(ConnectorInputModel connectorInputModel, IdentityData identity, CancellationToken cancellationToken)
    {
        var (name, connectorUrl, location, certificate, technicalUserId) = connectorInputModel;
        await CheckLocationExists(location);

        var (companyId, userId) = (identity.CompanyId, CompanyUserId: identity.IdentityId);
        var result = await _portalRepositories
            .GetInstance<ICompanyRepository>()
            .GetCompanyBpnAndSelfDescriptionDocumentByIdAsync(companyId)
            .ConfigureAwait(false);

        if (string.IsNullOrEmpty(result.Bpn))
        {
            throw new UnexpectedConditionException($"provider company {companyId} has no businessPartnerNumber assigned");
        }

        if (result.SelfDescriptionDocumentId is null)
        {
            throw new UnexpectedConditionException($"provider company {companyId} has no self description document");
        }
        await ValidateTechnicalUser(technicalUserId, companyId).ConfigureAwait(false);

        var connectorRequestModel = new ConnectorRequestModel(name, connectorUrl, ConnectorTypeId.COMPANY_CONNECTOR, location, companyId, companyId, technicalUserId);
        return await CreateAndRegisterConnectorAsync(
            connectorRequestModel,
            result.Bpn,
            result.SelfDescriptionDocumentId.Value,
            certificate,
            userId,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<Guid> CreateManagedConnectorInternalAsync(ManagedConnectorInputModel connectorInputModel, IdentityData identity, CancellationToken cancellationToken)
    {
        var (name, connectorUrl, location, providerBpn, certificate, technicalUserId) = connectorInputModel;
        await CheckLocationExists(location).ConfigureAwait(false);

        var result = await _portalRepositories
            .GetInstance<ICompanyRepository>()
            .GetCompanyIdAndSelfDescriptionDocumentByBpnAsync(providerBpn)
            .ConfigureAwait(false);

        if (result == default)
        {
            throw new ControllerArgumentException($"Company {providerBpn} does not exist", nameof(providerBpn));
        }

        if (result.SelfDescriptionDocumentId is null)
        {
            throw new UnexpectedConditionException($"provider company {result.CompanyId} has no self description document");
        }
        await ValidateTechnicalUser(technicalUserId, result.CompanyId).ConfigureAwait(false);

        var connectorRequestModel = new ConnectorRequestModel(name, connectorUrl, ConnectorTypeId.CONNECTOR_AS_A_SERVICE, location, result.CompanyId, identity.CompanyId, technicalUserId);
        return await CreateAndRegisterConnectorAsync(
            connectorRequestModel,
            providerBpn,
            result.SelfDescriptionDocumentId!.Value,
            certificate,
            identity.IdentityId,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task CheckLocationExists(string location)
    {
        if (!await _portalRepositories.GetInstance<ICountryRepository>()
                .CheckCountryExistsByAlpha2CodeAsync(location.ToUpper()).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"Location {location} does not exist", nameof(location));
        }
    }

    private async Task ValidateTechnicalUser(Guid? technicalUserId, Guid companyId)
    {
        if (technicalUserId == null)
        {
            return;
        }

        if (!await _portalRepositories.GetInstance<IServiceAccountRepository>()
                .CheckActiveServiceAccountExistsForCompanyAsync(technicalUserId.Value, companyId).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"Technical User {technicalUserId} is not assigned to company {companyId} or is not active", nameof(technicalUserId));
        }
    }

    private async Task<Guid> CreateAndRegisterConnectorAsync(
        ConnectorRequestModel connectorInputModel,
        string businessPartnerNumber,
        Guid selfDescriptionDocumentId,
        IFormFile? file,
        Guid? companyUserId,
        CancellationToken cancellationToken)
    {
        var (name, connectorUrl, type, location, provider, host, technicalUserId) = connectorInputModel;

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
                connector.LastEditorId = companyUserId;
                connector.DateLastChanged = DateTimeOffset.UtcNow;
                if (technicalUserId != null)
                {
                    connector.CompanyServiceAccountId = technicalUserId;
                }
            });

        DapsResponse? response = null;
        if (file is not null)
        {
            try
            {
                response = await _dapsService
                    .EnableDapsAuthAsync(name, connectorUrl, businessPartnerNumber, file, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (ServiceException)
            {
                // No error should be visible for the user
            }
        }

        if (!string.IsNullOrWhiteSpace(response?.ClientId))
        {
            connectorsRepository.CreateConnectorClientDetails(createdConnector.Id, response.ClientId);
            createdConnector.DapsRegistrationSuccessful = true;
            createdConnector.StatusId = ConnectorStatusId.ACTIVE;
        }
        else
        {
            createdConnector.DapsRegistrationSuccessful = false;
            createdConnector.StatusId = ConnectorStatusId.PENDING;
        }

        var selfDescriptionDocumentUrl = $"{_settings.SelfDescriptionDocumentUrl}/{selfDescriptionDocumentId}";
        await _sdFactoryBusinessLogic
            .RegisterConnectorAsync(createdConnector.Id, selfDescriptionDocumentUrl, businessPartnerNumber, cancellationToken)
            .ConfigureAwait(false);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return createdConnector.Id;
    }

    /// <inheritdoc/>
    public async Task DeleteConnectorAsync(Guid connectorId, IdentityData identity, CancellationToken cancellationToken)
    {
        var connectorsRepository = _portalRepositories.GetInstance<IConnectorsRepository>();
        var result = await connectorsRepository.GetConnectorDeleteDataAsync(connectorId).ConfigureAwait(false);
        if (!result.IsConnectorIdExist)
        {
            throw new NotFoundException($"Connector {connectorId} does not exist");
        }

        if (result.ConnectorStatus == ConnectorStatusId.INACTIVE)
        {
            throw new ConflictException("INACTIVE Connector can not be deleted");
        }

        if (string.IsNullOrWhiteSpace(result.DapsClientId))
        {
            throw new ConflictException("DapsClientId must be set");
        }

        if (result.SelfDescriptionDocumentId != null)
        {
            _portalRepositories.GetInstance<IDocumentRepository>().AttachAndModifyDocument(
                result.SelfDescriptionDocumentId.Value,
                a => { a.DocumentStatusId = result.DocumentStatusId!.Value; },
                a => { a.DocumentStatusId = DocumentStatusId.INACTIVE; });
        }

        connectorsRepository.DeleteConnectorClientDetails(connectorId);
        connectorsRepository.AttachAndModifyConnector(connectorId, null, con =>
        {
            con.StatusId = ConnectorStatusId.INACTIVE;
            con.LastEditorId = identity.IdentityId;
            con.DateLastChanged = DateTimeOffset.UtcNow;
        });
        await _dapsService.DeleteDapsClient(result.DapsClientId, cancellationToken).ConfigureAwait(false);
        await _portalRepositories.SaveAsync();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<ConnectorEndPointData> GetCompanyConnectorEndPointAsync(IEnumerable<string> bpns)
    {
        if (bpns.Any(bpn => !bpnRegex.IsMatch(bpn)))
        {
            throw new ControllerArgumentException($"Incorrect BPN [{string.Join(", ", bpns.Where(bpn => !bpnRegex.IsMatch(bpn)))}] attribute value");
        }

        return _portalRepositories.GetInstance<IConnectorsRepository>()
            .GetConnectorEndPointDataAsync(bpns)
            .PreSortedGroupBy(data => data.BusinessPartnerNumber)
            .Select(group =>
                new ConnectorEndPointData(
                    group.Key,
                    group.Select(x => x.ConnectorEndpoint)));
    }

    /// <inheritdoc />
    public async Task<bool> TriggerDapsAsync(Guid connectorId, IFormFile certificate, IdentityData identity, CancellationToken cancellationToken)
    {
        var connectorsRepository = _portalRepositories
            .GetInstance<IConnectorsRepository>();
        var connector = await connectorsRepository
            .GetConnectorInformationByIdForIamUser(connectorId, identity.CompanyId)
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
        var response = await _dapsService
            .EnableDapsAuthAsync(connectorData.Name, connectorData.Url, connectorData.Bpn, certificate, cancellationToken)
            .ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(response?.ClientId))
        {
            throw new ConflictException("Client Id should be set here");
        }

        connectorsRepository.AttachAndModifyConnector(connectorId, null, con =>
        {
            con.DapsRegistrationSuccessful = true;
            con.StatusId = ConnectorStatusId.ACTIVE;
            con.DateLastChanged = DateTimeOffset.UtcNow;
            con.LastEditorId = identity.IdentityId;
        });

        connectorsRepository.CreateConnectorClientDetails(connectorId, response.ClientId);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task ProcessClearinghouseSelfDescription(SelfDescriptionResponseData data, IdentityData identity, CancellationToken cancellationToken)
    {
        var result = await _portalRepositories.GetInstance<IConnectorsRepository>()
            .GetConnectorDataById(data.ExternalId)
            .ConfigureAwait(false);

        if (result == default)
        {
            throw new NotFoundException($"Connector {data.ExternalId} does not exist");
        }

        if (result.SelfDescriptionDocumentId != null)
        {
            throw new ConflictException($"Connector {data.ExternalId} already has a document assigned");
        }

        await _sdFactoryBusinessLogic.ProcessFinishSelfDescriptionLpForConnector(data, identity.IdentityId, cancellationToken).ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task UpdateConnectorUrl(Guid connectorId, ConnectorUpdateRequest data, IdentityData identity, CancellationToken cancellationToken)
    {
        data.ConnectorUrl.EnsureValidHttpUrl(() => nameof(data.ConnectorUrl));
        return UpdateConnectorUrlInternal(connectorId, data, identity, cancellationToken);
    }

    public async Task UpdateConnectorUrlInternal(Guid connectorId, ConnectorUpdateRequest data, IdentityData identity, CancellationToken cancellationToken)
    {
        var connectorsRepository = _portalRepositories
            .GetInstance<IConnectorsRepository>();
        var connector = await connectorsRepository
            .GetConnectorUpdateInformation(connectorId, identity.CompanyId)
            .ConfigureAwait(false);

        if (connector == null)
        {
            throw new NotFoundException($"Connector {connectorId} does not exists");
        }

        if (connector.ConnectorUrl == data.ConnectorUrl)
        {
            return;
        }

        if (!connector.IsUserOfHostCompany)
        {
            throw new ForbiddenException($"User {identity.UserEntityId} is no user of the connectors host company");
        }

        if (connector.Status == ConnectorStatusId.INACTIVE)
        {
            throw new ConflictException($"Connector {connectorId} is in state {ConnectorStatusId.INACTIVE}");
        }

        if (string.IsNullOrWhiteSpace(connector.DapsClientId))
        {
            throw new ConflictException($"Connector {connectorId} has no client id");
        }

        var bpn = connector.Type == ConnectorTypeId.CONNECTOR_AS_A_SERVICE
            ? connector.Bpn
            : await _portalRepositories.GetInstance<IUserRepository>()
                .GetCompanyBpnForIamUserAsync(identity.IdentityId)
                .ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(bpn))
        {
            throw new ConflictException("The business partner number must be set here");
        }

        await _dapsService
            .UpdateDapsConnectorUrl(connector.DapsClientId, data.ConnectorUrl, bpn, cancellationToken)
            .ConfigureAwait(false);
        connectorsRepository.AttachAndModifyConnector(connectorId, null, con => { con.ConnectorUrl = data.ConnectorUrl; });

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
