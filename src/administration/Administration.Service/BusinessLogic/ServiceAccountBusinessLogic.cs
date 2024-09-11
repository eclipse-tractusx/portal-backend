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
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class ServiceAccountBusinessLogic(
    IProvisioningManager provisioningManager,
    IPortalRepositories portalRepositories,
    IOptions<ServiceAccountSettings> options,
    IServiceAccountCreation serviceAccountCreation,
    IIdentityService identityService,
    IServiceAccountManagement serviceAccountManagement)
    : IServiceAccountBusinessLogic
{
    private readonly IIdentityData _identityData = identityService.IdentityData;
    private readonly ServiceAccountSettings _settings = options.Value;

    private const string CompanyId = "companyId";

    public async Task<IEnumerable<ServiceAccountDetails>> CreateOwnCompanyServiceAccountAsync(ServiceAccountCreationInfo serviceAccountCreationInfos)
    {
        if (serviceAccountCreationInfos.IamClientAuthMethod != IamClientAuthMethod.SECRET)
        {
            throw ControllerArgumentException.Create(AdministrationServiceAccountErrors.SERVICE_AUTH_SECRET_ARGUMENT, parameters: [new("authenticationType", serviceAccountCreationInfos.IamClientAuthMethod.ToString())]);//TODO implement other authenticationTypes
        }

        if (string.IsNullOrWhiteSpace(serviceAccountCreationInfos.Name))
        {
            throw ControllerArgumentException.Create(AdministrationServiceAccountErrors.SERVICE_NAME_EMPTY_ARGUMENT, parameters: [new("name", serviceAccountCreationInfos.Name)]);
        }

        var companyId = _identityData.CompanyId;
        var result = await portalRepositories.GetInstance<ICompanyRepository>().GetBpnAndTechnicalUserRoleIds(companyId, _settings.ClientId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_COMPANY_NOT_EXIST_CONFLICT, [new(CompanyId, companyId.ToString())]);
        }

        if (string.IsNullOrEmpty(result.Bpn))
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_BPN_NOT_SET_CONFLICT, [new(CompanyId, companyId.ToString())]);
        }

        serviceAccountCreationInfos.UserRoleIds.Except(result.TechnicalUserRoleIds)
            .IfAny(unassignable => throw ControllerArgumentException.Create(AdministrationServiceAccountErrors.SERVICE_ROLES_NOT_ASSIGN_ARGUMENT, parameters: [new("unassignable", string.Join(",", unassignable)), new("userRoleIds", string.Join(",", result.TechnicalUserRoleIds))]));

        const CompanyServiceAccountTypeId CompanyServiceAccountTypeId = CompanyServiceAccountTypeId.OWN;
        var (_, _, serviceAccounts) = await serviceAccountCreation.CreateServiceAccountAsync(serviceAccountCreationInfos, companyId, [result.Bpn], CompanyServiceAccountTypeId, false, true, new ServiceAccountCreationProcessData(ProcessTypeId.DIM_TECHNICAL_USER, null)).ConfigureAwait(ConfigureAwaitOptions.None);

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return serviceAccounts.Select(sa => new ServiceAccountDetails(
            sa.ServiceAccountId,
            sa.ClientId,
            sa.Name,
            sa.Description,
            sa.Status,
            sa.ServiceAccountData?.AuthData.IamClientAuthMethod,
            sa.UserRoleData,
            CompanyServiceAccountTypeId,
            sa.ServiceAccountData?.AuthData.Secret));
    }

    public async Task<int> DeleteOwnCompanyServiceAccountAsync(Guid serviceAccountId)
    {
        var serviceAccountRepository = portalRepositories.GetInstance<IServiceAccountRepository>();
        var companyId = _identityData.CompanyId;
        var technicalUserCreationSteps = new[]
        {
            ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_CREATE_DIM_TECHNICAL_USER,
            ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE,
            ProcessStepTypeId.RETRIGGER_AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE
        };
        var result = await serviceAccountRepository.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(serviceAccountId, companyId, technicalUserCreationSteps).ConfigureAwait(ConfigureAwaitOptions.None)
                ?? throw NotFoundException.Create(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_FOUND, [new("serviceAccountId", serviceAccountId.ToString()), new(CompanyId, companyId.ToString())]);

        if (result.StatusId is ConnectorStatusId.ACTIVE or ConnectorStatusId.PENDING)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_USERID_ACTIVATION_PENDING_CONFLICT);
        }

        if (result.OfferStatusId == OfferSubscriptionStatusId.ACTIVE)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_USERID_ACTIVATION_ACTIVE_CONFLICT);
        }

        // serviceAccount
        await serviceAccountManagement.DeleteServiceAccount(serviceAccountId, new DeleteServiceAccountData(result.UserRoleIds, result.ClientClientId, result.IsDimServiceAccount, result.CreationProcessInProgress, result.ProcessId)).ConfigureAwait(ConfigureAwaitOptions.None);
        ModifyConnectorForDeleteServiceAccount(serviceAccountId, result);

        return await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private void ModifyConnectorForDeleteServiceAccount(Guid serviceAccountId, OwnServiceAccountData result)
    {
        if (result.ConnectorId != null)
        {
            portalRepositories.GetInstance<IConnectorsRepository>().AttachAndModifyConnector(result.ConnectorId.Value,
                connector =>
                {
                    connector.CompanyServiceAccountId = serviceAccountId;
                },
                connector =>
                {
                    connector.CompanyServiceAccountId = null;
                });
        }
    }

    public async Task<ServiceAccountConnectorOfferData> GetOwnCompanyServiceAccountDetailsAsync(Guid serviceAccountId)
    {
        var companyId = _identityData.CompanyId;
        var result = await portalRepositories.GetInstance<IServiceAccountRepository>().GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(serviceAccountId, companyId);

        if (result == null)
        {
            throw NotFoundException.Create(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_FOUND, [new("serviceAccountId", serviceAccountId.ToString()), new(CompanyId, companyId.ToString())]);
        }

        IamClientAuthMethod? iamClientAuthMethod;
        string? secret;

        if (result.DimServiceAccountData != null)
        {
            iamClientAuthMethod = IamClientAuthMethod.SECRET;
            var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(_settings.EncryptionConfigIndex);
            secret = cryptoHelper.Decrypt(
                result.DimServiceAccountData.ClientSecret,
                result.DimServiceAccountData.InitializationVector);
        }
        else if (result.ClientClientId != null)
        {
            var internalClientId = await provisioningManager.GetIdOfCentralClientAsync(result.ClientClientId).ConfigureAwait(ConfigureAwaitOptions.None);
            var authData = await provisioningManager.GetCentralClientAuthDataAsync(internalClientId).ConfigureAwait(ConfigureAwaitOptions.None);
            iamClientAuthMethod = authData.IamClientAuthMethod;
            secret = authData.Secret;
        }
        else
        {
            iamClientAuthMethod = null;
            secret = null;
        }

        return new ServiceAccountConnectorOfferData(
            result.ServiceAccountId,
            result.ClientClientId,
            result.Name,
            result.Description,
            iamClientAuthMethod,
            result.UserRoleDatas,
            result.CompanyServiceAccountTypeId,
            result.Status,
            secret,
            result.ConnectorData,
            result.OfferSubscriptionData,
            result.CompanyLastEditorData!.Name,
            result.CompanyLastEditorData.CompanyName,
            result.SubscriptionId);
    }

    public async Task<ServiceAccountDetails> ResetOwnCompanyServiceAccountSecretAsync(Guid serviceAccountId)
    {
        var companyId = _identityData.CompanyId;
        var result = await portalRepositories.GetInstance<IServiceAccountRepository>().GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(serviceAccountId, companyId);
        if (result == null)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_FOUND, [new("serviceAccountId", serviceAccountId.ToString()), new(CompanyId, companyId.ToString())]);
        }

        if (result.ClientClientId == null)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_UNDEFINED_CLIENTID_CONFLICT, [new("serviceAccountId", serviceAccountId.ToString())]);
        }

        var authData = await provisioningManager.ResetCentralClientAuthDataAsync(result.ClientClientId).ConfigureAwait(ConfigureAwaitOptions.None);
        return new ServiceAccountDetails(
            result.ServiceAccountId,
            result.ClientClientId,
            result.Name,
            result.Description,
            result.Status,
            authData.IamClientAuthMethod,
            result.UserRoleDatas,
            result.CompanyServiceAccountTypeId,
            authData.Secret,
            result.SubscriptionId);
    }

    public async Task<ServiceAccountDetails> UpdateOwnCompanyServiceAccountDetailsAsync(Guid serviceAccountId, ServiceAccountEditableDetails serviceAccountDetails)
    {
        if (serviceAccountDetails.IamClientAuthMethod != IamClientAuthMethod.SECRET)
        {
            throw ControllerArgumentException.Create(AdministrationServiceAccountErrors.SERVICE_AUTH_SECRET_ARGUMENT, parameters: [new("authenticationType", serviceAccountDetails.IamClientAuthMethod.ToString())]); //TODO implement other authenticationTypes
        }

        if (serviceAccountId != serviceAccountDetails.ServiceAccountId)
        {
            throw ControllerArgumentException.Create(AdministrationServiceAccountErrors.SERVICE_ID_PATH_NOT_MATCH_ARGUMENT, parameters: [new("serviceAccountId", serviceAccountId.ToString()), new("serviceAccountDetailsServiceAccountId", serviceAccountDetails.ServiceAccountId.ToString())]);
        }

        var companyId = _identityData.CompanyId;
        var serviceAccountRepository = portalRepositories.GetInstance<IServiceAccountRepository>();
        var result = await serviceAccountRepository.GetOwnCompanyServiceAccountWithIamClientIdAsync(serviceAccountId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == null)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_FOUND, [new("serviceAccountId", serviceAccountId.ToString()), new(CompanyId, companyId.ToString())]);
        }

        if (result.UserStatusId == UserStatusId.INACTIVE)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_INACTIVE_CONFLICT, [new("serviceAccountId", serviceAccountId.ToString())]);
        }

        if (result.ClientClientId == null)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_CLIENTID_NOT_NULL_CONFLICT, [new("serviceAccountId", serviceAccountId.ToString())]);
        }

        ClientAuthData? authData;
        if (result.CompanyServiceAccountKindId == CompanyServiceAccountKindId.INTERNAL)
        {
            var internalClientId = await provisioningManager.UpdateCentralClientAsync(
                result.ClientClientId,
                new ClientConfigData(
                    serviceAccountDetails.Name,
                    serviceAccountDetails.Description,
                    serviceAccountDetails.IamClientAuthMethod)).ConfigureAwait(ConfigureAwaitOptions.None);

            authData = await provisioningManager.GetCentralClientAuthDataAsync(internalClientId).ConfigureAwait(ConfigureAwaitOptions.None);
        }
        else
        {
            authData = null;
        }

        serviceAccountRepository.AttachAndModifyCompanyServiceAccount(
            serviceAccountId,
            result.ServiceAccountVersion,
            sa =>
            {
                sa.Name = result.Name;
                sa.Description = result.Description;
            },
            sa =>
            {
                sa.Name = serviceAccountDetails.Name;
                sa.Description = serviceAccountDetails.Description;
            });

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);

        return new ServiceAccountDetails(
            serviceAccountId,
            result.ClientClientId,
            serviceAccountDetails.Name,
            serviceAccountDetails.Description,
            result.UserStatusId,
            authData?.IamClientAuthMethod,
            result.UserRoleDatas,
            result.CompanyServiceAccountTypeId,
            authData?.Secret,
            result.OfferSubscriptionId);
    }

    public Task<Pagination.Response<CompanyServiceAccountData>> GetOwnCompanyServiceAccountsDataAsync(int page, int size, string? clientId, bool? isOwner, bool filterForInactive, IEnumerable<UserStatusId>? userStatusIds)
    {
        IEnumerable<UserStatusId> filterUserStatusIds;
        if (userStatusIds?.Any() ?? false)
        {
            filterUserStatusIds = userStatusIds;
        }
        else
        {
            filterUserStatusIds = filterForInactive ? [UserStatusId.INACTIVE] : [UserStatusId.ACTIVE, UserStatusId.PENDING, UserStatusId.PENDING_DELETION];
        }

        return Pagination.CreateResponseAsync(
            page,
            size,
            15,
            portalRepositories.GetInstance<IServiceAccountRepository>().GetOwnCompanyServiceAccountsUntracked(_identityData.CompanyId, clientId, isOwner, filterUserStatusIds));
    }

    public IAsyncEnumerable<UserRoleWithDescription> GetServiceAccountRolesAsync(string? languageShortName) =>
        portalRepositories.GetInstance<IUserRolesRepository>().GetServiceAccountRolesAsync(_identityData.CompanyId, _settings.ClientId, languageShortName ?? Constants.DefaultLanguage);

    public async Task HandleServiceAccountCreationCallback(Guid processId, AuthenticationDetail callbackData)
    {
        var processData = await portalRepositories.GetInstance<IProcessStepRepository>().GetProcessDataForServiceAccountCallback(processId, [ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE])
            .ConfigureAwait(ConfigureAwaitOptions.None);

        var context = processData.ProcessData.CreateManualProcessData(ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE, portalRepositories, () => $"externalId {processId}");

        if (processData.ServiceAccountId is null)
        {
            throw new ConflictException($"ServiceAccountId must be set for process {processId}");
        }
        if (processData.ServiceAccountVersion is null)
        {
            throw new UnexpectedConditionException("ServiceAccountVersion or IdentityVersion should never be null here");
        }

        CreateDimServiceAccount(callbackData, processData.ServiceAccountId.Value, processData.ServiceAccountVersion.Value);

        if (processData.ProcessTypeId == ProcessTypeId.OFFER_SUBSCRIPTION)
        {
            context.ScheduleProcessSteps([ProcessStepTypeId.TRIGGER_ACTIVATE_SUBSCRIPTION]);
        }
        context.FinalizeProcessStep();
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private void CreateDimServiceAccount(AuthenticationDetail callbackData, Guid serviceAccountId, Guid serviceAccountVersion)
    {
        var serviceAccountRepository = portalRepositories.GetInstance<IServiceAccountRepository>();
        portalRepositories.GetInstance<IUserRepository>().AttachAndModifyIdentity(serviceAccountId,
            i => { i.UserStatusId = UserStatusId.PENDING; },
            i => { i.UserStatusId = UserStatusId.ACTIVE; });

        serviceAccountRepository.AttachAndModifyCompanyServiceAccount(serviceAccountId, serviceAccountVersion,
            sa => { sa.ClientClientId = null; },
            sa => { sa.ClientClientId = callbackData.ClientId; });

        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(_settings.EncryptionConfigIndex);
        var (secret, initializationVector) = cryptoHelper.Encrypt(callbackData.ClientSecret);

        serviceAccountRepository.CreateDimCompanyServiceAccount(serviceAccountId, callbackData.AuthenticationServiceUrl, secret, initializationVector, _settings.EncryptionConfigIndex);
    }

    public async Task HandleServiceAccountDeletionCallback(Guid processId)
    {
        var processData = await portalRepositories.GetInstance<IProcessStepRepository>()
            .GetProcessDataForServiceAccountDeletionCallback(processId,
                [ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE])
            .ConfigureAwait(ConfigureAwaitOptions.None);

        var context = processData.ProcessData.CreateManualProcessData(ProcessStepTypeId.AWAIT_DELETE_DIM_TECHNICAL_USER,
            portalRepositories, () => $"externalId {processId}");

        portalRepositories.GetInstance<IUserRepository>().AttachAndModifyIdentity(
            processData.ServiceAccountId ?? throw new ConflictException($"ServiceAccountId must be set for process {processId}"),
            null,
            i =>
            {
                i.UserStatusId = UserStatusId.DELETED;
            });

        context.FinalizeProcessStep();
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
