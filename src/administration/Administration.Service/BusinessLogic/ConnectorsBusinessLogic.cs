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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Library;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IConnectorsBusinessLogic"/> making use of <see cref="IConnectorsRepository"/> to retrieve data.
/// </summary>
public class ConnectorsBusinessLogic(
    IPortalRepositories portalRepositories,
    IOptions<ConnectorsSettings> options,
    ISdFactoryBusinessLogic sdFactoryBusinessLogic,
    IIdentityService identityService,
    IServiceAccountManagement serviceAccountManagement,
    ILogger<ConnectorsBusinessLogic> logger)
    : IConnectorsBusinessLogic
{
    private static readonly Regex BpnRegex = new(@"(\w|\d){16}", RegexOptions.None, TimeSpan.FromSeconds(1));
    private readonly IIdentityData _identityData = identityService.IdentityData;
    private readonly ConnectorsSettings _settings = options.Value;

    /// <inheritdoc/>
    public Task<Pagination.Response<ConnectorData>> GetAllCompanyConnectorDatas(int page, int size) =>
        Pagination.CreateResponseAsync(
            page,
            size,
            _settings.MaxPageSize,
            portalRepositories.GetInstance<IConnectorsRepository>().GetAllCompanyConnectorsForCompanyId(_identityData.CompanyId));

    /// <inheritdoc/>
    public Task<Pagination.Response<ManagedConnectorData>> GetManagedConnectorForCompany(int page, int size) =>
        Pagination.CreateResponseAsync(
            page,
            size,
            _settings.MaxPageSize,
            portalRepositories.GetInstance<IConnectorsRepository>().GetManagedConnectorsForCompany(_identityData.CompanyId));

    public async Task<ConnectorData> GetCompanyConnectorData(Guid connectorId)
    {
        var companyId = _identityData.CompanyId;
        var result = await portalRepositories.GetInstance<IConnectorsRepository>().GetConnectorByIdForCompany(connectorId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw NotFoundException.Create(AdministrationConnectorErrors.CONNECTOR_NOT_FOUND, new ErrorParameter[] { new("connectorId", connectorId.ToString()) });
        }

        if (!result.IsProviderCompany)
        {
            throw ForbiddenException.Create(AdministrationConnectorErrors.CONNECTOR_NOT_PROVIDER_COMPANY, new ErrorParameter[] { new("companyId", companyId.ToString()), new("connectorId", connectorId.ToString()) });
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
        var companyId = _identityData.CompanyId;
        var (name, connectorUrl, location, technicalUserId) = connectorInputModel;
        await CheckLocationExists(location);

        var result = await portalRepositories
            .GetInstance<ICompanyRepository>()
            .GetCompanyBpnAndSelfDescriptionDocumentByIdAsync(companyId)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (string.IsNullOrEmpty(result.Bpn))
        {
            throw UnexpectedConditionException.Create(AdministrationConnectorErrors.CONNECTOR_UNEXPECTED_NO_BPN_ASSIGNED, new ErrorParameter[] { new("companyId", companyId.ToString()) });
        }

        await ValidateTechnicalUser(technicalUserId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);

        var connectorRequestModel = new ConnectorRequestModel(name, connectorUrl, ConnectorTypeId.COMPANY_CONNECTOR, location, companyId, companyId, technicalUserId);
        return await CreateAndRegisterConnectorAsync(
            connectorRequestModel,
            result.Bpn,
            result.SelfDescriptionDocumentId,
            null,
            companyId,
            cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task<Guid> CreateManagedConnectorInternalAsync(ManagedConnectorInputModel connectorInputModel, CancellationToken cancellationToken)
    {
        var companyId = _identityData.CompanyId;
        var (name, connectorUrl, location, subscriptionId, technicalUserId) = connectorInputModel;
        await CheckLocationExists(location).ConfigureAwait(ConfigureAwaitOptions.None);

        var result = await portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .CheckOfferSubscriptionWithOfferProvider(subscriptionId, companyId)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (!result.Exists)
        {
            throw NotFoundException.Create(AdministrationConnectorErrors.CONNECTOR_NOT_OFFERSUBSCRIPTION_EXIST, new ErrorParameter[] { new("subscriptionId", subscriptionId.ToString()) });
        }

        if (!result.IsOfferProvider)
        {
            throw ForbiddenException.Create(AdministrationConnectorErrors.CONNECTOR_NOT_PROVIDER_COMPANY_OFFER);
        }

        if (result.OfferSubscriptionAlreadyLinked)
        {
            throw ConflictException.Create(AdministrationConnectorErrors.CONNECTOR_CONFLICT_OFFERSUBSCRIPTION_LINKED);
        }

        if (result.OfferSubscriptionStatus != OfferSubscriptionStatusId.ACTIVE &&
            result.OfferSubscriptionStatus != OfferSubscriptionStatusId.PENDING)
        {
            throw ConflictException.Create(AdministrationConnectorErrors.CONNECTOR_CONFLICT_STATUS_ACTIVE_OR_PENDING, new ErrorParameter[] { new("offerSubscriptionStatusIdActive", OfferSubscriptionStatusId.ACTIVE.ToString()), new("offerSubscriptionStatusIdPending", OfferSubscriptionStatusId.PENDING.ToString()) });
        }

        if (string.IsNullOrWhiteSpace(result.ProviderBpn))
        {
            throw ConflictException.Create(AdministrationConnectorErrors.CONNECTOR_CONFLICT_SET_BPN, new ErrorParameter[] { new("companyId", result.CompanyId.ToString()) });
        }

        await ValidateTechnicalUser(technicalUserId, result.CompanyId).ConfigureAwait(ConfigureAwaitOptions.None);

        var connectorRequestModel = new ConnectorRequestModel(name, connectorUrl, ConnectorTypeId.CONNECTOR_AS_A_SERVICE, location, companyId, result.CompanyId, technicalUserId);
        return await CreateAndRegisterConnectorAsync(
            connectorRequestModel,
            result.ProviderBpn,
            result.SelfDescriptionDocumentId,
            subscriptionId,
            result.CompanyId,
            cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task CheckLocationExists(string location)
    {
        if (!await portalRepositories.GetInstance<ICountryRepository>()
                .CheckCountryExistsByAlpha2CodeAsync(location.ToUpper()).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw ControllerArgumentException.Create(AdministrationConnectorErrors.CONNECTOR_ARGUMENT_LOCATION_NOT_EXIST, new ErrorParameter[] { new("location", location) });
        }
    }

    private async Task ValidateTechnicalUser(Guid? technicalUserId, Guid companyId)
    {
        if (technicalUserId == null)
        {
            return;
        }

        if (!await portalRepositories.GetInstance<IServiceAccountRepository>()
                .CheckActiveServiceAccountExistsForCompanyAsync(technicalUserId.Value, companyId).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw ControllerArgumentException.Create(AdministrationConnectorErrors.CONNECTOR_ARGUMENT_TECH_USER_NOT_ACTIVE, new ErrorParameter[] { new("technicalUserId", technicalUserId.Value.ToString()), new("companyId", companyId.ToString()) });
        }
    }

    private async Task<Guid> CreateAndRegisterConnectorAsync(
        ConnectorRequestModel connectorInputModel,
        string businessPartnerNumber,
        Guid? selfDescriptionDocumentId,
        Guid? subscriptionId,
        Guid companyId,
        CancellationToken cancellationToken)
    {
        if (selfDescriptionDocumentId is null && !_settings.ClearinghouseConnectDisabled)
        {
            throw ConflictException.Create(AdministrationConnectorErrors.CONNECTOR_CONFLICT_NO_DESCRIPTION, [new("companyId", companyId.ToString())]);
        }

        var (name, connectorUrl, type, location, provider, host, technicalUserId) = connectorInputModel;

        var connectorsRepository = portalRepositories.GetInstance<IConnectorsRepository>();
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
                connector.StatusId = _settings.ClearinghouseConnectDisabled ? ConnectorStatusId.ACTIVE : ConnectorStatusId.PENDING;
                if (technicalUserId != null)
                {
                    connector.CompanyServiceAccountId = technicalUserId;
                }
            });

        if (subscriptionId != null)
        {
            connectorsRepository.CreateConnectorAssignedSubscriptions(createdConnector.Id, subscriptionId.Value);
        }

        if (!_settings.ClearinghouseConnectDisabled)
        {
            var selfDescriptionDocumentUrl = $"{_settings.SelfDescriptionDocumentUrl}/{selfDescriptionDocumentId}";
            await sdFactoryBusinessLogic
                .RegisterConnectorAsync(createdConnector.Id, selfDescriptionDocumentUrl, businessPartnerNumber, cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.None);
        }

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return createdConnector.Id;
    }

    /// <inheritdoc/>
    public async Task DeleteConnectorAsync(Guid connectorId, bool deleteServiceAccount)
    {
        var companyId = _identityData.CompanyId;
        var connectorsRepository = portalRepositories.GetInstance<IConnectorsRepository>();
        var processStepsToFilter = new[]
        {
            ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_CREATE_DIM_TECHNICAL_USER,
            ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE,
            ProcessStepTypeId.RETRIGGER_AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE
        };

        var result = await connectorsRepository.GetConnectorDeleteDataAsync(connectorId, companyId, processStepsToFilter).ConfigureAwait(ConfigureAwaitOptions.None) ?? throw NotFoundException.Create(AdministrationConnectorErrors.CONNECTOR_NOT_FOUND, new ErrorParameter[] { new("connectorId", connectorId.ToString()) });
        if (!result.IsProvidingOrHostCompany)
        {
            throw ForbiddenException.Create(AdministrationConnectorErrors.CONNECTOR_NOT_PROVIDER_COMPANY_NOR_HOST, new ErrorParameter[] { new("companyId", companyId.ToString()), new("connectorId", connectorId.ToString()) });
        }

        if (result is { ServiceAccountId: not null, UserStatusId: UserStatusId.ACTIVE or UserStatusId.PENDING } && deleteServiceAccount)
        {
            await serviceAccountManagement.DeleteServiceAccount(result.ServiceAccountId!.Value, result.DeleteServiceAccountData).ConfigureAwait(false);
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
                throw ConflictException.Create(AdministrationConnectorErrors.CONNECTOR_CONFLICT_DELETION_DECLINED);
        }
    }

    private async Task DeleteConnector(Guid connectorId, IEnumerable<ConnectorOfferSubscription> connectorOfferSubscriptions, Guid selfDescriptionDocumentId, DocumentStatusId documentStatus, IConnectorsRepository connectorsRepository)
    {
        portalRepositories.GetInstance<IDocumentRepository>().AttachAndModifyDocument(
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
            con.CompanyServiceAccountId = null;
            con.StatusId = ConnectorStatusId.INACTIVE;
            con.DateLastChanged = DateTimeOffset.UtcNow;
        });
        await portalRepositories.SaveAsync();
    }

    private async Task DeleteConnectorWithDocuments(Guid connectorId, Guid selfDescriptionDocumentId, IEnumerable<ConnectorOfferSubscription> connectorOfferSubscriptions, IConnectorsRepository connectorsRepository)
    {
        portalRepositories.GetInstance<IDocumentRepository>().RemoveDocument(selfDescriptionDocumentId);
        RemoveConnectorAssignedOfferSubscriptions(connectorId, connectorOfferSubscriptions, connectorsRepository);
        connectorsRepository.DeleteConnector(connectorId);
        await portalRepositories.SaveAsync();
    }

    private async Task DeleteConnectorWithoutDocuments(Guid connectorId, IEnumerable<ConnectorOfferSubscription> connectorOfferSubscriptions, IConnectorsRepository connectorsRepository)
    {
        RemoveConnectorAssignedOfferSubscriptions(connectorId, connectorOfferSubscriptions, connectorsRepository);
        connectorsRepository.DeleteConnector(connectorId);
        await portalRepositories.SaveAsync();
    }

    private static void RemoveConnectorAssignedOfferSubscriptions(Guid connectorId, IEnumerable<ConnectorOfferSubscription> connectorOfferSubscriptions, IConnectorsRepository connectorsRepository)
    {
        var activeConnectorOfferSubscription = connectorOfferSubscriptions.Where(cos => cos.OfferSubscriptionStatus == OfferSubscriptionStatusId.ACTIVE)
            .Select(cos => cos.AssignedOfferSubscriptionIds);
        if (activeConnectorOfferSubscription.Any())
        {
            throw ForbiddenException.Create(AdministrationConnectorErrors.CONNECTOR_DELETION_FAILED_OFFER_SUBSCRIPTION, new ErrorParameter[] { new("connectorId", connectorId.ToString()), new("activeConnectorOfferSubscription", string.Join(",", activeConnectorOfferSubscription)) });
        }

        var assignedOfferSubscriptions = connectorOfferSubscriptions.Select(cos => cos.AssignedOfferSubscriptionIds);
        if (assignedOfferSubscriptions.Any())
        {
            connectorsRepository.DeleteConnectorAssignedSubscriptions(connectorId, assignedOfferSubscriptions);
        }
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<ConnectorEndPointData> GetCompanyConnectorEndPointAsync(IEnumerable<string>? bpns)
    {
        bpns ??= Enumerable.Empty<string>();

        bpns.Where(bpn => !BpnRegex.IsMatch(bpn)).IfAny(invalid =>
            throw ControllerArgumentException.Create(AdministrationConnectorErrors.CONNECTOR_ARGUMENT_INCORRECT_BPN, new ErrorParameter[] { new("bpns", string.Join(", ", invalid)) }));

        return portalRepositories.GetInstance<IConnectorsRepository>()
            .GetConnectorEndPointDataAsync(bpns.Select(x => x.ToUpper()))
            .PreSortedGroupBy(data => data.BusinessPartnerNumber)
            .Select(group =>
                new ConnectorEndPointData(
                    group.Key,
                    group.Select(x => x.ConnectorEndpoint)));
    }

    /// <inheritdoc />
    public async Task ProcessClearinghouseSelfDescription(SelfDescriptionResponseData data, CancellationToken cancellationToken)
    {
        logger.LogInformation("Process SelfDescription called with the following data {@Data}", data.ToString().Replace(Environment.NewLine, string.Empty));

        var result = await portalRepositories.GetInstance<IConnectorsRepository>()
            .GetConnectorDataById(data.ExternalId)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (result == default)
        {
            throw NotFoundException.Create(AdministrationConnectorErrors.CONNECTOR_NOT_EXIST, new ErrorParameter[] { new("externalId", data.ExternalId.ToString()) });
        }

        if (result.SelfDescriptionDocumentId != null)
        {
            throw ConflictException.Create(AdministrationConnectorErrors.CONNECTOR_CONFLICT_ALREADY_ASSIGNED, new ErrorParameter[] { new("externalId", data.ExternalId.ToString()) });
        }

        await sdFactoryBusinessLogic.ProcessFinishSelfDescriptionLpForConnector(data, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public Task UpdateConnectorUrl(Guid connectorId, ConnectorUpdateRequest data)
    {
        data.ConnectorUrl.EnsureValidHttpUrl(() => nameof(data.ConnectorUrl));
        return UpdateConnectorUrlInternal(connectorId, data);
    }

    private async Task UpdateConnectorUrlInternal(Guid connectorId, ConnectorUpdateRequest data)
    {
        var connectorsRepository = portalRepositories
            .GetInstance<IConnectorsRepository>();
        var connector = await connectorsRepository
            .GetConnectorUpdateInformation(connectorId, _identityData.CompanyId)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (connector == null)
        {
            throw NotFoundException.Create(AdministrationConnectorErrors.CONNECTOR_NOT_FOUND, new ErrorParameter[] { new("connectorId", connectorId.ToString()) });
        }

        if (connector.ConnectorUrl == data.ConnectorUrl)
        {
            return;
        }

        if (!connector.IsHostCompany)
        {
            throw ForbiddenException.Create(AdministrationConnectorErrors.CONNECTOR_NOT_HOST_COMPANY, new ErrorParameter[] { new("companyId", _identityData.CompanyId.ToString()) });
        }

        if (connector.Status == ConnectorStatusId.INACTIVE)
        {
            throw ConflictException.Create(AdministrationConnectorErrors.CONNECTOR_CONFLICT_INACTIVE_STATE, new ErrorParameter[] { new("connectorId", connectorId.ToString()), new("connectorStatusId", ConnectorStatusId.INACTIVE.ToString()) });
        }

        var bpn = connector.Type == ConnectorTypeId.CONNECTOR_AS_A_SERVICE
            ? connector.Bpn
            : await portalRepositories.GetInstance<IUserRepository>()
                .GetCompanyBpnForIamUserAsync(_identityData.IdentityId)
                .ConfigureAwait(ConfigureAwaitOptions.None);
        if (string.IsNullOrWhiteSpace(bpn))
        {
            throw ConflictException.Create(AdministrationConnectorErrors.CONNECTOR_CONFLICT_SET_BPN, new ErrorParameter[] { new("companyId", _identityData.CompanyId.ToString()) });
        }

        connectorsRepository.AttachAndModifyConnector(connectorId, null, con =>
        {
            con.ConnectorUrl = data.ConnectorUrl;
        });

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<OfferSubscriptionConnectorData> GetConnectorOfferSubscriptionData(bool? connectorIdSet) =>
        portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetConnectorOfferSubscriptionData(connectorIdSet, _identityData.CompanyId);

    public Task<Pagination.Response<ConnectorMissingSdDocumentData>> GetConnectorsWithMissingSdDocument(int page, int size) =>
        Pagination.CreateResponseAsync(
            page,
            size,
            _settings.MaxPageSize,
            portalRepositories.GetInstance<IConnectorsRepository>().GetConnectorsWithMissingSdDocument());

    public async Task TriggerSelfDescriptionCreation()
    {
        var connectorRepository = portalRepositories.GetInstance<IConnectorsRepository>();
        var processStepRepository = portalRepositories.GetInstance<IProcessStepRepository>();
        var connectorIds = connectorRepository.GetConnectorIdsWithMissingSelfDescription();
        await foreach (var connectorId in connectorIds)
        {
            var processId = processStepRepository.CreateProcess(ProcessTypeId.SELF_DESCRIPTION_CREATION).Id;
            processStepRepository.CreateProcessStep(ProcessStepTypeId.SELF_DESCRIPTION_CONNECTOR_CREATION, ProcessStepStatusId.TODO, processId);
            connectorRepository.AttachAndModifyConnector(connectorId, c => c.SdCreationProcessId = null, c => c.SdCreationProcessId = processId);
        }

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task RetriggerSelfDescriptionCreation(Guid processId)
    {
        const ProcessStepTypeId NextStep = ProcessStepTypeId.SELF_DESCRIPTION_CONNECTOR_CREATION;
        const ProcessStepTypeId StepToTrigger = ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_CONNECTOR_CREATION;
        var (validProcessId, processData) = await portalRepositories.GetInstance<IProcessStepRepository>().IsValidProcess(processId, ProcessTypeId.SELF_DESCRIPTION_CREATION, Enumerable.Repeat(StepToTrigger, 1)).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!validProcessId)
        {
            throw new NotFoundException($"process {processId} does not exist");
        }

        var context = processData.CreateManualProcessData(StepToTrigger, portalRepositories, () => $"processId {processId}");

        context.ScheduleProcessSteps(Enumerable.Repeat(NextStep, 1));
        context.FinalizeProcessStep();
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
