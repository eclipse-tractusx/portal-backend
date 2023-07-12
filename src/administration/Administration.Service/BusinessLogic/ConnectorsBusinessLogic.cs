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
    public Task<Pagination.Response<ConnectorData>> GetAllCompanyConnectorDatas(Guid companyId, int page, int size)
    {
        var connectors = _portalRepositories.GetInstance<IConnectorsRepository>().GetAllCompanyConnectorsForIamUser(companyId);

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
                            c.HostId,
                            c.Host!.Name,
                            c.SelfDescriptionDocumentId,
                            c.SelfDescriptionDocument!.DocumentName)
                    ).AsAsyncEnumerable()
            )
        );
    }

    /// <inheritdoc/>
    public Task<Pagination.Response<ManagedConnectorData>> GetManagedConnectorForCompany(Guid companyId, int page, int size) =>
        Pagination.CreateResponseAsync(
            page,
            size,
            _settings.MaxPageSize,
            _portalRepositories.GetInstance<IConnectorsRepository>().GetManagedConnectorsForCompany(companyId));

    public async Task<ConnectorData> GetCompanyConnectorData(Guid connectorId, Guid companyId)
    {
        var result = await _portalRepositories.GetInstance<IConnectorsRepository>().GetConnectorByIdForCompany(connectorId, companyId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"connector {connectorId} does not exist");
        }
        if (!result.IsProviderCompany)
        {
            throw new ForbiddenException($"company {companyId} is not provider of connector {connectorId}");
        }
        return result.ConnectorData;
    }

    /// <inheritdoc/>
    public Task<Guid> CreateConnectorAsync(ConnectorInputModel connectorInputModel, (Guid UserId, Guid CompanyId) identity, CancellationToken cancellationToken)
    {
        ValidateCertificationType(connectorInputModel.Certificate);
        return CreateConnectorInternalAsync(connectorInputModel, identity, cancellationToken);
    }

    public Task<Guid> CreateManagedConnectorAsync(ManagedConnectorInputModel connectorInputModel, (Guid UserId, Guid CompanyId) identity, CancellationToken cancellationToken)
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

    private async Task<Guid> CreateConnectorInternalAsync(ConnectorInputModel connectorInputModel, (Guid UserId, Guid CompanyId) identity, CancellationToken cancellationToken)
    {
        var (name, connectorUrl, location, certificate, technicalUserId) = connectorInputModel;
        await CheckLocationExists(location);

        var result = await _portalRepositories
            .GetInstance<ICompanyRepository>()
            .GetCompanyBpnAndSelfDescriptionDocumentByIdAsync(identity.CompanyId)
            .ConfigureAwait(false);

        if (string.IsNullOrEmpty(result.Bpn))
        {
            throw new UnexpectedConditionException($"provider company {identity.CompanyId} has no businessPartnerNumber assigned");
        }

        if (result.SelfDescriptionDocumentId is null)
        {
            throw new UnexpectedConditionException($"provider company {identity.CompanyId} has no self description document");
        }
        await ValidateTechnicalUser(technicalUserId, identity.CompanyId).ConfigureAwait(false);

        var connectorRequestModel = new ConnectorRequestModel(name, connectorUrl, ConnectorTypeId.COMPANY_CONNECTOR, location, identity.CompanyId, identity.CompanyId, technicalUserId);
        return await CreateAndRegisterConnectorAsync(
            connectorRequestModel,
            result.Bpn,
            result.SelfDescriptionDocumentId.Value,
            certificate,
            identity.UserId,
            null,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<Guid> CreateManagedConnectorInternalAsync(ManagedConnectorInputModel connectorInputModel, (Guid UserId, Guid CompanyId) identity, CancellationToken cancellationToken)
    {
        var (name, connectorUrl, location, subscriptionId, certificate, technicalUserId) = connectorInputModel;
        await CheckLocationExists(location).ConfigureAwait(false);

        var result = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .CheckOfferSubscriptionWithOfferProvider(subscriptionId, identity.CompanyId)
            .ConfigureAwait(false);

        if (!result.Exists)
        {
            throw new NotFoundException($"OfferSubscription {subscriptionId} does not exist");
        }

        if (!result.IsOfferProvider)
        {
            throw new ForbiddenException("Company is not the provider of the offer");
        }

        if (result.OfferSubscriptionAlreadyLinked)
        {
            throw new ConflictException("OfferSubscription is already linked to a connector");
        }

        if (result.OfferSubscriptionStatus != OfferSubscriptionStatusId.ACTIVE &&
            result.OfferSubscriptionStatus != OfferSubscriptionStatusId.PENDING)
        {
            throw new ConflictException($"The offer subscription must be either {OfferSubscriptionStatusId.ACTIVE} or {OfferSubscriptionStatusId.PENDING}");
        }

        if (result.SelfDescriptionDocumentId is null)
        {
            throw new UnexpectedConditionException($"provider company {result.CompanyId} has no self description document");
        }

        if (string.IsNullOrWhiteSpace(result.ProviderBpn))
        {
            throw new ConflictException($"The bpn of compay {result.CompanyId} must be set");
        }

        await ValidateTechnicalUser(technicalUserId, result.CompanyId).ConfigureAwait(false);

        var connectorRequestModel = new ConnectorRequestModel(name, connectorUrl, ConnectorTypeId.CONNECTOR_AS_A_SERVICE, location, result.CompanyId, identity.CompanyId, technicalUserId);
        return await CreateAndRegisterConnectorAsync(
            connectorRequestModel,
            result.ProviderBpn,
            result.SelfDescriptionDocumentId!.Value,
            certificate,
            identity.UserId,
            subscriptionId,
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
        Guid userId,
        Guid? subscriptionId,
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
                connector.LastEditorId = userId;
                connector.DateLastChanged = DateTimeOffset.UtcNow;
                if (technicalUserId != null)
                {
                    connector.CompanyServiceAccountId = technicalUserId;
                }
            });

        if (subscriptionId != null)
        {
            connectorsRepository.CreateConnectorAssignedSubscriptions(createdConnector.Id, subscriptionId.Value);
        }

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
    public async Task DeleteConnectorAsync(Guid connectorId, Guid userId, CancellationToken cancellationToken)
    {
        var connectorsRepository = _portalRepositories.GetInstance<IConnectorsRepository>();
        var (isValidConnectorId, dapsClientId, selfDescriptionDocumentId,
            documentStatus, connectorStatus, dapsRegistrationSuccess) = await connectorsRepository.GetConnectorDeleteDataAsync(connectorId).ConfigureAwait(false);

        if (!isValidConnectorId)
        {
            throw new NotFoundException($"Connector {connectorId} does not exist");
        }

        switch ((dapsRegistrationSuccess ?? false, connectorStatus))
        {
            case (true, ConnectorStatusId.ACTIVE) when selfDescriptionDocumentId == null:
                await DeleteUpdateConnectorDetail(connectorId, userId, dapsClientId, connectorsRepository, cancellationToken);
                break;
            case (true, ConnectorStatusId.ACTIVE) when selfDescriptionDocumentId != null && documentStatus != null:
                await DeleteConnector(connectorId, userId, dapsClientId, selfDescriptionDocumentId.Value, documentStatus.Value, connectorsRepository, cancellationToken);
                break;
            case (false, ConnectorStatusId.PENDING) when selfDescriptionDocumentId == null:
                await DeleteConnectorWithoutDocuments(connectorId, connectorsRepository);
                break;
            case (false, ConnectorStatusId.PENDING) when selfDescriptionDocumentId != null:
                await DeleteConnectorWithDocuments(connectorId, selfDescriptionDocumentId.Value, connectorsRepository);
                break;
            case (true, ConnectorStatusId.ACTIVE) when selfDescriptionDocumentId != null && documentStatus == null:
                throw new UnexpectedConditionException("documentStatus should never be null here");
            default:
                throw new ConflictException($"Connector status does not match a deletion scenario. Deletion declined");
        }
    }

    private async Task DeleteConnector(Guid connectorId, Guid userId, string? dapsClientId, Guid selfDescriptionDocumentId, DocumentStatusId documentStatus, IConnectorsRepository connectorsRepository, CancellationToken cancellationToken)
    {
        _portalRepositories.GetInstance<IDocumentRepository>().AttachAndModifyDocument(
            selfDescriptionDocumentId,
            a => { a.DocumentStatusId = documentStatus; },
            a => { a.DocumentStatusId = DocumentStatusId.INACTIVE; });

        await DeleteUpdateConnectorDetail(connectorId, userId, dapsClientId, connectorsRepository, cancellationToken);
    }

    private async Task DeleteUpdateConnectorDetail(Guid connectorId, Guid userId, string? dapsClientId, IConnectorsRepository connectorsRepository, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dapsClientId))
        {
            throw new ConflictException("DapsClientId must be set");
        }

        connectorsRepository.DeleteConnectorClientDetails(connectorId);
        connectorsRepository.AttachAndModifyConnector(connectorId, null, con =>
        {
            con.StatusId = ConnectorStatusId.INACTIVE;
            con.LastEditorId = userId;
            con.DateLastChanged = DateTimeOffset.UtcNow;
        });
        await _dapsService.DeleteDapsClient(dapsClientId, cancellationToken).ConfigureAwait(false);
        await _portalRepositories.SaveAsync();
    }

    private async Task DeleteConnectorWithDocuments(Guid connectorId, Guid selfDescriptionDocumentId, IConnectorsRepository connectorsRepository)
    {
        _portalRepositories.GetInstance<IDocumentRepository>().RemoveDocument(selfDescriptionDocumentId);
        connectorsRepository.DeleteConnector(connectorId);
        await _portalRepositories.SaveAsync();
    }

    private async Task DeleteConnectorWithoutDocuments(Guid connectorId, IConnectorsRepository connectorsRepository)
    {
        connectorsRepository.DeleteConnector(connectorId);
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
    public async Task<bool> TriggerDapsAsync(Guid connectorId, IFormFile certificate, (Guid UserId, Guid CompanyId) identity, CancellationToken cancellationToken)
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
            con.LastEditorId = identity.UserId;
        });

        connectorsRepository.CreateConnectorClientDetails(connectorId, response.ClientId);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task ProcessClearinghouseSelfDescription(SelfDescriptionResponseData data, Guid userId, CancellationToken cancellationToken)
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

        await _sdFactoryBusinessLogic.ProcessFinishSelfDescriptionLpForConnector(data, userId, cancellationToken).ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task UpdateConnectorUrl(Guid connectorId, ConnectorUpdateRequest data, (Guid UserId, Guid CompanyId) identity, CancellationToken cancellationToken)
    {
        data.ConnectorUrl.EnsureValidHttpUrl(() => nameof(data.ConnectorUrl));
        return UpdateConnectorUrlInternal(connectorId, data, identity, cancellationToken);
    }

    private async Task UpdateConnectorUrlInternal(Guid connectorId, ConnectorUpdateRequest data, (Guid UserId, Guid CompanyId) identity, CancellationToken cancellationToken)
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

        if (!connector.IsHostCompany)
        {
            throw new ForbiddenException($"Company {identity.CompanyId} is not the connectors host company");
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
                .GetCompanyBpnForIamUserAsync(identity.UserId)
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
