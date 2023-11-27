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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
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
    private readonly IIdentityService _identityService;
    private readonly ILogger<ConnectorsBusinessLogic> _logger;
    private readonly ConnectorsSettings _settings;
    private static readonly Regex bpnRegex = new(@"(\w|\d){16}", RegexOptions.None, TimeSpan.FromSeconds(1));

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Access to the needed repositories</param>
    /// <param name="options">The options</param>
    /// <param name="sdFactoryBusinessLogic">Access to the connectorsSdFactory</param>
    /// <param name="identityService">Access to the current logged in user</param>
    /// <param name="logger">Access to the logger</param>
    public ConnectorsBusinessLogic(IPortalRepositories portalRepositories, IOptions<ConnectorsSettings> options, ISdFactoryBusinessLogic sdFactoryBusinessLogic, IIdentityService identityService, ILogger<ConnectorsBusinessLogic> logger)
    {
        _portalRepositories = portalRepositories;
        _settings = options.Value;
        _sdFactoryBusinessLogic = sdFactoryBusinessLogic;
        _identityService = identityService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Pagination.Response<ConnectorData>> GetAllCompanyConnectorDatas(int page, int size) =>
        Pagination.CreateResponseAsync(
            page,
            size,
            _settings.MaxPageSize,
            _portalRepositories.GetInstance<IConnectorsRepository>().GetAllCompanyConnectorsForCompanyId(_identityService.IdentityData.CompanyId));

    /// <inheritdoc/>
    public Task<Pagination.Response<ManagedConnectorData>> GetManagedConnectorForCompany(int page, int size) =>
        Pagination.CreateResponseAsync(
            page,
            size,
            _settings.MaxPageSize,
            _portalRepositories.GetInstance<IConnectorsRepository>().GetManagedConnectorsForCompany(_identityService.IdentityData.CompanyId));

    public async Task<ConnectorData> GetCompanyConnectorData(Guid connectorId)
    {
        var companyId = _identityService.IdentityData.CompanyId;
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
    public Task<Guid> CreateConnectorAsync(ConnectorInputModel connectorInputModel, CancellationToken cancellationToken) =>
        CreateConnectorInternalAsync(connectorInputModel, cancellationToken);

    public Task<Guid> CreateManagedConnectorAsync(ManagedConnectorInputModel connectorInputModel, CancellationToken cancellationToken) =>
        CreateManagedConnectorInternalAsync(connectorInputModel, cancellationToken);

    private async Task<Guid> CreateConnectorInternalAsync(ConnectorInputModel connectorInputModel, CancellationToken cancellationToken)
    {
        var companyId = _identityService.IdentityData.CompanyId;
        var (name, connectorUrl, location, technicalUserId) = connectorInputModel;
        await CheckLocationExists(location);

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
            null,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<Guid> CreateManagedConnectorInternalAsync(ManagedConnectorInputModel connectorInputModel, CancellationToken cancellationToken)
    {
        var companyId = _identityService.IdentityData.CompanyId;
        var (name, connectorUrl, location, subscriptionId, technicalUserId) = connectorInputModel;
        await CheckLocationExists(location).ConfigureAwait(false);

        var result = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .CheckOfferSubscriptionWithOfferProvider(subscriptionId, companyId)
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
            throw new ConflictException($"provider company {result.CompanyId} has no self description document");
        }

        if (string.IsNullOrWhiteSpace(result.ProviderBpn))
        {
            throw new ConflictException($"The bpn of compay {result.CompanyId} must be set");
        }

        await ValidateTechnicalUser(technicalUserId, result.CompanyId).ConfigureAwait(false);

        var connectorRequestModel = new ConnectorRequestModel(name, connectorUrl, ConnectorTypeId.CONNECTOR_AS_A_SERVICE, location, result.CompanyId, companyId, technicalUserId);
        return await CreateAndRegisterConnectorAsync(
            connectorRequestModel,
            result.ProviderBpn,
            result.SelfDescriptionDocumentId!.Value,
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
                connector.DateLastChanged = DateTimeOffset.UtcNow;
                connector.StatusId = ConnectorStatusId.PENDING;
                if (technicalUserId != null)
                {
                    connector.CompanyServiceAccountId = technicalUserId;
                }
            });

        if (subscriptionId != null)
        {
            connectorsRepository.CreateConnectorAssignedSubscriptions(createdConnector.Id, subscriptionId.Value);
        }

        var selfDescriptionDocumentUrl = $"{_settings.SelfDescriptionDocumentUrl}/{selfDescriptionDocumentId}";
        await _sdFactoryBusinessLogic
            .RegisterConnectorAsync(createdConnector.Id, selfDescriptionDocumentUrl, businessPartnerNumber, cancellationToken)
            .ConfigureAwait(false);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return createdConnector.Id;
    }

    /// <inheritdoc/>
    public async Task DeleteConnectorAsync(Guid connectorId)
    {
        var companyId = _identityService.IdentityData.CompanyId;
        var connectorsRepository = _portalRepositories.GetInstance<IConnectorsRepository>();
        var result = await connectorsRepository.GetConnectorDeleteDataAsync(connectorId, companyId).ConfigureAwait(false) ?? throw new NotFoundException($"Connector {connectorId} does not exist");
        if (!result.IsProvidingOrHostCompany)
        {
            throw new ForbiddenException($"company {companyId} is neither provider nor host-company of connector {connectorId}");
        }
        if (result.ServiceAccountId.HasValue && result.UserStatusId != UserStatusId.INACTIVE)
        {
            _portalRepositories.GetInstance<IUserRepository>().AttachAndModifyIdentity(result.ServiceAccountId.Value, null, i =>
            {
                i.UserStatusId = UserStatusId.INACTIVE;
            });
        }

        switch (result.ConnectorStatus)
        {
            case ConnectorStatusId.PENDING when result.SelfDescriptionDocumentId == null:
                await DeleteConnectorWithoutDocuments(connectorId, result.ConnectorOfferSubscriptions, connectorsRepository);
                break;
            case ConnectorStatusId.PENDING:
                await DeleteConnectorWithDocuments(connectorId, result.SelfDescriptionDocumentId.Value, result.ConnectorOfferSubscriptions, connectorsRepository);
                break;
            case ConnectorStatusId.ACTIVE when result.SelfDescriptionDocumentId != null && result.DocumentStatusId != null:
                await DeleteConnector(connectorId, result.ConnectorOfferSubscriptions, result.SelfDescriptionDocumentId.Value, result.DocumentStatusId.Value, connectorsRepository);
                break;
            default:
                throw new ConflictException("Connector status does not match a deletion scenario. Deletion declined");
        }
    }

    private async Task DeleteConnector(Guid connectorId, IEnumerable<ConnectorOfferSubscription> connectorOfferSubscriptions, Guid selfDescriptionDocumentId, DocumentStatusId documentStatus, IConnectorsRepository connectorsRepository)
    {
        _portalRepositories.GetInstance<IDocumentRepository>().AttachAndModifyDocument(
            selfDescriptionDocumentId,
            a => { a.DocumentStatusId = documentStatus; },
            a => { a.DocumentStatusId = DocumentStatusId.INACTIVE; });
        RemoveConnectorAssignedOfferSubscriptions(connectorId, connectorOfferSubscriptions, connectorsRepository);
        await DeleteUpdateConnectorDetail(connectorId, connectorsRepository);
    }

    private async Task DeleteUpdateConnectorDetail(Guid connectorId, IConnectorsRepository connectorsRepository)
    {
        connectorsRepository.AttachAndModifyConnector(connectorId, null, con =>
        {
            con.StatusId = ConnectorStatusId.INACTIVE;
            con.DateLastChanged = DateTimeOffset.UtcNow;
        });
        await _portalRepositories.SaveAsync();
    }

    private async Task DeleteConnectorWithDocuments(Guid connectorId, Guid selfDescriptionDocumentId, IEnumerable<ConnectorOfferSubscription> connectorOfferSubscriptions, IConnectorsRepository connectorsRepository)
    {
        _portalRepositories.GetInstance<IDocumentRepository>().RemoveDocument(selfDescriptionDocumentId);
        RemoveConnectorAssignedOfferSubscriptions(connectorId, connectorOfferSubscriptions, connectorsRepository);
        connectorsRepository.DeleteConnector(connectorId);
        await _portalRepositories.SaveAsync();
    }

    private async Task DeleteConnectorWithoutDocuments(Guid connectorId, IEnumerable<ConnectorOfferSubscription> connectorOfferSubscriptions, IConnectorsRepository connectorsRepository)
    {
        RemoveConnectorAssignedOfferSubscriptions(connectorId, connectorOfferSubscriptions, connectorsRepository);
        connectorsRepository.DeleteConnector(connectorId);
        await _portalRepositories.SaveAsync();
    }

    private static void RemoveConnectorAssignedOfferSubscriptions(Guid connectorId, IEnumerable<ConnectorOfferSubscription> connectorOfferSubscriptions, IConnectorsRepository connectorsRepository)
    {
        var activeConnectorOfferSubscription = connectorOfferSubscriptions.Where(cos => cos.OfferSubscriptionStatus == OfferSubscriptionStatusId.ACTIVE)
            .Select(cos => cos.AssignedOfferSubscriptionIds);
        if (activeConnectorOfferSubscription.Any())
        {
            throw new ForbiddenException($"Deletion Failed. Connector {connectorId} connected to an active offer subscription [{string.Join(",", activeConnectorOfferSubscription)}]");
        }
        var assignedOfferSubscriptions = connectorOfferSubscriptions.Select(cos => cos.AssignedOfferSubscriptionIds);
        if (assignedOfferSubscriptions.Any())
        {
            connectorsRepository.DeleteConnectorAssignedSubscriptions(connectorId, assignedOfferSubscriptions);
        }
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
    public async Task ProcessClearinghouseSelfDescription(SelfDescriptionResponseData data, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Process SelfDescription called with the following data {Data}", data);

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

        await _sdFactoryBusinessLogic.ProcessFinishSelfDescriptionLpForConnector(data, _identityService.IdentityId, cancellationToken).ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task UpdateConnectorUrl(Guid connectorId, ConnectorUpdateRequest data)
    {
        data.ConnectorUrl.EnsureValidHttpUrl(() => nameof(data.ConnectorUrl));
        return UpdateConnectorUrlInternal(connectorId, data);
    }

    private async Task UpdateConnectorUrlInternal(Guid connectorId, ConnectorUpdateRequest data)
    {
        var identity = _identityService.IdentityData;
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

        var bpn = connector.Type == ConnectorTypeId.CONNECTOR_AS_A_SERVICE
            ? connector.Bpn
            : await _portalRepositories.GetInstance<IUserRepository>()
                .GetCompanyBpnForIamUserAsync(identity.UserId)
                .ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(bpn))
        {
            throw new ConflictException("The business partner number must be set here");
        }

        connectorsRepository.AttachAndModifyConnector(connectorId, null, con =>
        {
            con.ConnectorUrl = data.ConnectorUrl;
        });

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<OfferSubscriptionConnectorData> GetConnectorOfferSubscriptionData(bool? connectorIdSet) =>
        _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetConnectorOfferSubscriptionData(connectorIdSet, _identityService.IdentityData.CompanyId);
}
