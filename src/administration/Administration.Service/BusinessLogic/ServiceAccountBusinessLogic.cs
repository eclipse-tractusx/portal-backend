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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
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

    public async Task<ServiceAccountDetails> CreateOwnCompanyServiceAccountAsync(ServiceAccountCreationInfo serviceAccountCreationInfos)
    {
        if (serviceAccountCreationInfos.IamClientAuthMethod != IamClientAuthMethod.SECRET)
        {
            throw ControllerArgumentException.Create(AdministrationServiceAccountErrors.SERVICE_AUTH_SECRET_ARGUMENT, [new("authenticationType", serviceAccountCreationInfos.IamClientAuthMethod.ToString())]);//TODO implement other authenticationTypes
        }
        if (string.IsNullOrWhiteSpace(serviceAccountCreationInfos.Name))
        {
            throw ControllerArgumentException.Create(AdministrationServiceAccountErrors.SERVICE_NAME_EMPTY_ARGUMENT, [new("name", serviceAccountCreationInfos.Name)]);
        }

        var companyId = _identityData.CompanyId;
        var result = await portalRepositories.GetInstance<ICompanyRepository>().GetBpnAndTechnicalUserRoleIds(companyId, _settings.ClientId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_COMPANY_NOT_EXIST_CONFLICT, [new("companyId", companyId.ToString())]);
        }
        if (string.IsNullOrEmpty(result.Bpn))
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_BPN_NOT_SET_CONFLICT, [new("companyId", companyId.ToString())]);
        }

        serviceAccountCreationInfos.UserRoleIds.Except(result.TechnicalUserRoleIds)
            .IfAny(unassignable => throw ControllerArgumentException.Create(AdministrationServiceAccountErrors.SERVICE_ROLES_NOT_ASSIGN_ARGUMENT, [new("unassignable", string.Join(",", unassignable)), new("userRoleIds", string.Join(",", result.TechnicalUserRoleIds))]));

        var companyServiceAccountTypeId = CompanyServiceAccountTypeId.OWN;
        var (clientId, serviceAccountData, serviceAccountId, userRoleData) = await serviceAccountCreation.CreateServiceAccountAsync(serviceAccountCreationInfos, companyId, [result.Bpn], companyServiceAccountTypeId, false, true).ConfigureAwait(ConfigureAwaitOptions.None);
        var createDimTechnicalUser = userRoleData.Any(userDataItem => _settings.DimCreationRoles.Any(configDataItem => userDataItem.ClientClientId == configDataItem.ClientId && configDataItem.UserRoleNames.Contains(userDataItem.UserRoleText)));
        if (createDimTechnicalUser)
        {
            var processStepRepository = portalRepositories.GetInstance<IProcessStepRepository>();
            var process = processStepRepository.CreateProcess(ProcessTypeId.DIM_TECHNICAL_USER);
            processStepRepository.CreateProcessStep(ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER, ProcessStepStatusId.TODO, process.Id);
            portalRepositories.GetInstance<IServiceAccountRepository>().CreateDimUserCreationData(serviceAccountId, process.Id);
        }

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return new ServiceAccountDetails(
            serviceAccountId,
            clientId,
            serviceAccountCreationInfos.Name,
            serviceAccountCreationInfos.Description,
            serviceAccountCreationInfos.IamClientAuthMethod,
            userRoleData,
            companyServiceAccountTypeId,
            serviceAccountData.AuthData.Secret);
    }

    public async Task<int> DeleteOwnCompanyServiceAccountAsync(Guid serviceAccountId)
    {
        var serviceAccountRepository = portalRepositories.GetInstance<IServiceAccountRepository>();
        var companyId = _identityData.CompanyId;
        var result = await serviceAccountRepository.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(serviceAccountId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_CONFLICT, [new("serviceAccountId", serviceAccountId.ToString()), new("companyId", companyId.ToString())]);
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
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_CONFLICT, [new("serviceAccountId", serviceAccountId.ToString()), new("companyId", companyId.ToString())]);
        }
        if (result.ClientClientId == null)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_UNDEFINED_CLIENTID_CONFLICT, [new("serviceAccountId", serviceAccountId.ToString())]);
        }

        var internalClientId = await provisioningManager.GetIdOfCentralClientAsync(result.ClientClientId).ConfigureAwait(ConfigureAwaitOptions.None);

        var authData = await provisioningManager.GetCentralClientAuthDataAsync(internalClientId).ConfigureAwait(ConfigureAwaitOptions.None);
        return new ServiceAccountConnectorOfferData(
            result.ServiceAccountId,
            result.ClientClientId,
            result.Name,
            result.Description,
            authData.IamClientAuthMethod,
            result.UserRoleDatas,
            result.CompanyServiceAccountTypeId,
            authData.Secret,
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
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_CONFLICT, [new("serviceAccountId", serviceAccountId.ToString()), new("companyId", companyId.ToString())]);
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
            throw ControllerArgumentException.Create(AdministrationServiceAccountErrors.SERVICE_AUTH_SECRET_ARGUMENT, [new("authenticationType", serviceAccountDetails.IamClientAuthMethod.ToString())]); //TODO implement other authenticationTypes
        }
        if (serviceAccountId != serviceAccountDetails.ServiceAccountId)
        {
            throw ControllerArgumentException.Create(AdministrationServiceAccountErrors.SERVICE_ID_PATH_NOT_MATCH_ARGUMENT, [new("serviceAccountId", serviceAccountId.ToString()), new("serviceAccountDetailsServiceAccountId", serviceAccountDetails.ServiceAccountId.ToString())]);
        }

        var companyId = _identityData.CompanyId;
        var serviceAccountRepository = portalRepositories.GetInstance<IServiceAccountRepository>();
        var result = await serviceAccountRepository.GetOwnCompanyServiceAccountWithIamClientIdAsync(serviceAccountId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == null)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_CONFLICT, [new("serviceAccountId", serviceAccountId.ToString()), new("companyId", companyId.ToString())]);
        }
        if (result.UserStatusId == UserStatusId.INACTIVE)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_INACTIVE_CONFLICT, [new("serviceAccountId", serviceAccountId.ToString())]);
        }
        if (result.ClientClientId == null)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_CLIENTID_NOT_NULL_CONFLICT, [new("serviceAccountId", serviceAccountId.ToString())]);
        }

        var internalClientId = await provisioningManager.UpdateCentralClientAsync(
            result.ClientClientId,
            new ClientConfigData(
                serviceAccountDetails.Name,
                serviceAccountDetails.Description,
                serviceAccountDetails.IamClientAuthMethod)).ConfigureAwait(ConfigureAwaitOptions.None);

        var authData = await provisioningManager.GetCentralClientAuthDataAsync(internalClientId).ConfigureAwait(ConfigureAwaitOptions.None);

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
            authData.IamClientAuthMethod,
            result.UserRoleDatas,
            result.CompanyServiceAccountTypeId,
            authData.Secret,
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

    public async Task HandleServiceAccountCreationCallback(Guid externalId, AuthenticationDetail callbackData)
    {
        var processData = await portalRepositories.GetInstance<IProcessStepRepository>().GetProcessDataForServiceAccountCallback(externalId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (!processData.ProcessExists)
        {
            throw new NotFoundException($"Process {externalId} does not exist");
        }

        if (processData.ProcessTypeId == ProcessTypeId.OFFER_SUBSCRIPTION)
        {
            await HandleOfferSubscriptionTechnicalUserCallback(externalId, callbackData, processData.ProcessSteps, processData.SubscriptionData).ConfigureAwait(ConfigureAwaitOptions.None);
        }
        else if (processData.ProcessTypeId == ProcessTypeId.DIM_TECHNICAL_USER)
        {
            await HandleDimTechnicalUserCallback(callbackData, processData.ProcessSteps, processData.ServiceAccountData);
        }
    }

    private async Task HandleOfferSubscriptionTechnicalUserCallback(Guid externalId, AuthenticationDetail callbackData, IEnumerable<(Guid ProcessStepId, ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId)> processSteps, (Guid? OfferSubscriptionId, Guid? CompanyId, string? OfferName) subscriptionData)
    {
        if (processSteps.Count(x => x is
            {
                ProcessStepTypeId: ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE,
                ProcessStepStatusId: ProcessStepStatusId.TODO
            }) != 1)
        {
            throw new ConflictException($"{ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE} must be in Status {ProcessStepStatusId.TODO}");
        }

        if (subscriptionData.OfferSubscriptionId is null)
        {
            throw new ConflictException($"OfferSubscriptionId must be set for Process {externalId}");
        }

        if (subscriptionData.CompanyId is null)
        {
            throw new ConflictException($"CompanyId must be set for Process {externalId}");
        }

        if (subscriptionData.OfferName is null)
        {
            throw new ConflictException($"OfferName must be set for Process {externalId}");
        }

        var processStep = processSteps.Single(x => x is
        {
            ProcessStepTypeId: ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE,
            ProcessStepStatusId: ProcessStepStatusId.TODO
        });
        var name = $"sa-{subscriptionData.OfferName}-{subscriptionData.OfferSubscriptionId}";
        CreateDimServiceAccount(callbackData, subscriptionData.CompanyId.Value, name, CompanyServiceAccountTypeId.MANAGED, x => x.OfferSubscriptionId = subscriptionData.OfferSubscriptionId);
        var processStepRepository = portalRepositories.GetInstance<IProcessStepRepository>();
        processStepRepository.AttachAndModifyProcessStep(processStep.ProcessStepId,
            ps => { ps.ProcessStepStatusId = processStep.ProcessStepStatusId; },
            ps => { ps.ProcessStepStatusId = ProcessStepStatusId.DONE; }
        );
        processStepRepository.CreateProcessStep(ProcessStepTypeId.TRIGGER_ACTIVATE_SUBSCRIPTION, ProcessStepStatusId.TODO, externalId);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task HandleDimTechnicalUserCallback(AuthenticationDetail callbackData, IEnumerable<(Guid ProcessStepId, ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId)> processSteps, (string? ServiceAccountName, Guid? CompanyId) serviceAccountData)
    {
        if (processSteps.Count(x => x is
            {
                ProcessStepTypeId: ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE,
                ProcessStepStatusId: ProcessStepStatusId.TODO
            }) != 1)
        {
            throw new ConflictException($"{ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE} must be in Status {ProcessStepStatusId.TODO}");
        }

        if (serviceAccountData.ServiceAccountName is null)
        {
            throw new ConflictException("Service Account Name must be set");
        }

        if (serviceAccountData.CompanyId is null)
        {
            throw new ConflictException("Company Id must be set");
        }

        var processStep = processSteps.Single(x => x is
        {
            ProcessStepTypeId: ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE,
            ProcessStepStatusId: ProcessStepStatusId.TODO
        });
        var name = $"dim-{serviceAccountData.ServiceAccountName}";
        CreateDimServiceAccount(callbackData, serviceAccountData.CompanyId.Value, name, CompanyServiceAccountTypeId.OWN, null);
        var processStepRepository = portalRepositories.GetInstance<IProcessStepRepository>();
        processStepRepository.AttachAndModifyProcessStep(processStep.ProcessStepId,
            ps => { ps.ProcessStepStatusId = processStep.ProcessStepStatusId; },
            ps => { ps.ProcessStepStatusId = ProcessStepStatusId.DONE; }
        );
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private void CreateDimServiceAccount(AuthenticationDetail callbackData, Guid companyId, string name, CompanyServiceAccountTypeId serviceAccountTypeId, Action<CompanyServiceAccount>? setOptionalParameters)
    {
        var identity = portalRepositories.GetInstance<IUserRepository>().CreateIdentity(companyId, UserStatusId.ACTIVE, IdentityTypeId.COMPANY_SERVICE_ACCOUNT, null);
        var serviceAccountRepository = portalRepositories.GetInstance<IServiceAccountRepository>();
        var serviceAccount = serviceAccountRepository.CreateCompanyServiceAccount(
            identity.Id,
            name,
            "Technical User for the DIM Wallet",
            callbackData.ClientId,
            serviceAccountTypeId,
            setOptionalParameters);

        var cryptoConfig = _settings.EncryptionConfigs.SingleOrDefault(x => x.Index == _settings.EncryptionConfigIndex) ?? throw new ConfigurationException($"EncryptionModeIndex {_settings.EncryptionConfigIndex} is not configured");
        var (secret, initializationVector) = CryptoHelper.Encrypt(callbackData.ClientSecret, Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);

        serviceAccountRepository.CreateDimCompanyServiceAccount(serviceAccount.Id, callbackData.AuthenticationServiceUrl, secret, initializationVector, _settings.EncryptionConfigIndex);
    }
}
