/********************************************************************************
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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
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
    IIdentityService identityService)
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
        var (_, serviceAccounts) = await serviceAccountCreation.CreateServiceAccountAsync(serviceAccountCreationInfos, companyId, [result.Bpn], CompanyServiceAccountTypeId, false, true, new ServiceAccountCreationProcessData(ProcessTypeId.DIM_TECHNICAL_USER, null)).ConfigureAwait(ConfigureAwaitOptions.None);

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
        var result = await serviceAccountRepository.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(serviceAccountId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_CONFLICT, [new("serviceAccountId", serviceAccountId.ToString()), new(CompanyId, companyId.ToString())]);
        }

        if (result.statusId == ConnectorStatusId.ACTIVE || result.statusId == ConnectorStatusId.PENDING)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_USERID_ACTIVATION_PENDING_CONFLICT);
        }

        if (result.OfferStatusId == OfferSubscriptionStatusId.ACTIVE)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_USERID_ACTIVATION_ACTIVE_CONFLICT);
        }

        portalRepositories.GetInstance<IUserRepository>().AttachAndModifyIdentity(serviceAccountId, null, i =>
        {
            i.UserStatusId = UserStatusId.INACTIVE;
        });

        // serviceAccount
        if (!string.IsNullOrWhiteSpace(result.ClientClientId))
        {
            await provisioningManager.DeleteCentralClientAsync(result.ClientClientId).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        portalRepositories.GetInstance<IUserRolesRepository>().DeleteCompanyUserAssignedRoles(result.UserRoleIds.Select(userRoleId => (serviceAccountId, userRoleId)));

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

        return await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<ServiceAccountConnectorOfferData> GetOwnCompanyServiceAccountDetailsAsync(Guid serviceAccountId)
    {
        var companyId = _identityData.CompanyId;
        var result = await portalRepositories.GetInstance<IServiceAccountRepository>().GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(serviceAccountId, companyId);
        if (result == null)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_CONFLICT, [new("serviceAccountId", serviceAccountId.ToString()), new(CompanyId, companyId.ToString())]);
        }

        if (result.ClientClientId == null)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_UNDEFINED_CLIENTID_CONFLICT, [new("serviceAccountId", serviceAccountId.ToString())]);
        }

        IamClientAuthMethod iamClientAuthMethod;
        string? secret;
        if (result.DimServiceAccountData == null)
        {
            var internalClientId = await provisioningManager.GetIdOfCentralClientAsync(result.ClientClientId).ConfigureAwait(ConfigureAwaitOptions.None);
            var authData = await provisioningManager.GetCentralClientAuthDataAsync(internalClientId).ConfigureAwait(ConfigureAwaitOptions.None);
            iamClientAuthMethod = authData.IamClientAuthMethod;
            secret = authData.Secret;
        }
        else
        {
            iamClientAuthMethod = IamClientAuthMethod.SECRET;
            secret = Decrypt(
                result.DimServiceAccountData.ClientSecret,
                result.DimServiceAccountData.InitializationVector,
                result.DimServiceAccountData.EncryptionMode);
        }

        return new ServiceAccountConnectorOfferData(
            result.ServiceAccountId,
            result.ClientClientId,
            result.Name,
            result.Description,
            iamClientAuthMethod,
            result.UserRoleDatas,
            result.CompanyServiceAccountTypeId,
            secret,
            result.ConnectorData,
            result.OfferSubscriptionData,
            result.CompanyLastEditorData!.Name,
            result.CompanyLastEditorData.CompanyName,
            result.SubscriptionId);
    }

    private string Decrypt(byte[]? clientSecret, byte[]? initializationVector, int? encryptionMode)
    {
        if (clientSecret == null)
        {
            throw new ConflictException("ClientSecret must not be null");
        }

        if (encryptionMode == null)
        {
            throw new ConflictException("EncryptionMode must not be null");
        }

        var cryptoConfig = _settings.EncryptionConfigs.SingleOrDefault(x => x.Index == encryptionMode) ?? throw new ConfigurationException($"EncryptionModeIndex {encryptionMode} is not configured");

        return CryptoHelper.Decrypt(clientSecret, initializationVector, Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);
    }

    public async Task<ServiceAccountDetails> ResetOwnCompanyServiceAccountSecretAsync(Guid serviceAccountId)
    {
        var companyId = _identityData.CompanyId;
        var result = await portalRepositories.GetInstance<IServiceAccountRepository>().GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(serviceAccountId, companyId);
        if (result == null)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_CONFLICT, [new("serviceAccountId", serviceAccountId.ToString()), new(CompanyId, companyId.ToString())]);
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
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_CONFLICT, [new("serviceAccountId", serviceAccountId.ToString()), new(CompanyId, companyId.ToString())]);
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
            result.ServiceAccountId,
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

    public Task<Pagination.Response<CompanyServiceAccountData>> GetOwnCompanyServiceAccountsDataAsync(int page, int size, string? clientId, bool? isOwner, bool filterForInactive) =>
        Pagination.CreateResponseAsync(
            page,
            size,
            15,
            portalRepositories.GetInstance<IServiceAccountRepository>().GetOwnCompanyServiceAccountsUntracked(_identityData.CompanyId, clientId, isOwner, filterForInactive ? UserStatusId.INACTIVE : UserStatusId.ACTIVE));

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

        switch (processData.ProcessTypeId)
        {
            case ProcessTypeId.OFFER_SUBSCRIPTION:
                HandleOfferSubscriptionTechnicalUserCallback(processData.ServiceAccountId.Value, callbackData, context);
                break;
            case ProcessTypeId.DIM_TECHNICAL_USER:
                CreateDimServiceAccount(callbackData, processData.ServiceAccountId.Value);
                break;
            default:
                throw new ControllerArgumentException($"process {processId} has invalid processType {processData.ProcessTypeId}");
        }

        context.FinalizeProcessStep();
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private void HandleOfferSubscriptionTechnicalUserCallback(Guid serviceAccountId, AuthenticationDetail callbackData, ManualProcessStepData context)
    {
        CreateDimServiceAccount(callbackData, serviceAccountId);
        context.ScheduleProcessSteps([ProcessStepTypeId.TRIGGER_ACTIVATE_SUBSCRIPTION]);
    }

    private void CreateDimServiceAccount(AuthenticationDetail callbackData, Guid serviceAccountId)
    {
        var serviceAccountRepository = portalRepositories.GetInstance<IServiceAccountRepository>();
        portalRepositories.GetInstance<IUserRepository>().AttachAndModifyIdentity(serviceAccountId,
            i => { i.UserStatusId = UserStatusId.PENDING; },
            i => { i.UserStatusId = UserStatusId.ACTIVE; });
        serviceAccountRepository.AttachAndModifyCompanyServiceAccount(serviceAccountId,
            sa => { sa.ClientClientId = null; },
            sa => { sa.ClientClientId = callbackData.ClientId; });

        var cryptoConfig = _settings.EncryptionConfigs.SingleOrDefault(x => x.Index == _settings.EncryptionConfigIndex) ?? throw new ConfigurationException($"EncryptionModeIndex {_settings.EncryptionConfigIndex} is not configured");
        var (secret, initializationVector) = CryptoHelper.Encrypt(callbackData.ClientSecret, Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);

        serviceAccountRepository.CreateDimCompanyServiceAccount(serviceAccountId, callbackData.AuthenticationServiceUrl, secret, initializationVector, _settings.EncryptionConfigIndex);
    }
}
