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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class ServiceAccountBusinessLogic : IServiceAccountBusinessLogic
{
    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IServiceAccountCreation _serviceAccountCreation;
    private readonly IIdentityData _identityData;
    private readonly ServiceAccountSettings _settings;

    public ServiceAccountBusinessLogic(
        IProvisioningManager provisioningManager,
        IPortalRepositories portalRepositories,
        IOptions<ServiceAccountSettings> options,
        IServiceAccountCreation serviceAccountCreation,
        IIdentityService identityService)
    {
        _provisioningManager = provisioningManager;
        _portalRepositories = portalRepositories;
        _serviceAccountCreation = serviceAccountCreation;
        _identityData = identityService.IdentityData;
        _settings = options.Value;
    }

    public async Task<ServiceAccountDetails> CreateOwnCompanyServiceAccountAsync(ServiceAccountCreationInfo serviceAccountCreationInfos)
    {
        if (serviceAccountCreationInfos.IamClientAuthMethod != IamClientAuthMethod.SECRET)
        {
            throw new ControllerArgumentException("other authenticationType values than SECRET are not supported yet", "authenticationType"); //TODO implement other authenticationTypes
        }
        if (string.IsNullOrWhiteSpace(serviceAccountCreationInfos.Name))
        {
            throw new ControllerArgumentException("name must not be empty", "name");
        }

        var companyId = _identityData.CompanyId;
        var result = await _portalRepositories.GetInstance<ICompanyRepository>().GetBpnAndTechnicalUserRoleIds(companyId, _settings.ClientId).ConfigureAwait(false);
        if (result == default)
        {
            throw new ConflictException($"company {companyId} does not exist");
        }
        if (string.IsNullOrEmpty(result.Bpn))
        {
            throw new ConflictException($"bpn not set for company {companyId}");
        }

        var unassignable = serviceAccountCreationInfos.UserRoleIds.Except(result.TechnicalUserRoleIds);
        if (unassignable.Any())
        {
            throw new ControllerArgumentException($"The roles {string.Join(",", unassignable)} are not assignable to a service account", "userRoleIds");
        }

        var companyServiceAccountTypeId = CompanyServiceAccountTypeId.OWN;
        var (clientId, serviceAccountData, serviceAccountId, userRoleData) = await _serviceAccountCreation.CreateServiceAccountAsync(serviceAccountCreationInfos, companyId, new[] { result.Bpn }, companyServiceAccountTypeId, false, true).ConfigureAwait(false);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
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
        var serviceAccountRepository = _portalRepositories.GetInstance<IServiceAccountRepository>();
        var companyId = _identityData.CompanyId;
        var result = await serviceAccountRepository.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(serviceAccountId, companyId).ConfigureAwait(false);
        if (result == default)
        {
            throw new ConflictException($"serviceAccount {serviceAccountId} not found for company {companyId}");
        }
        if (result.statusId == ConnectorStatusId.ACTIVE || result.statusId == ConnectorStatusId.PENDING)
        {
            throw new ConflictException($"Technical User is linked to an active connector. Change the link or deactivate the connector to delete the technical user.");
        }
        if (result.OfferStatusId == OfferSubscriptionStatusId.ACTIVE)
        {
            throw new ConflictException($"Technical User is linked to an active subscription. Deactivate the subscription to delete the technical user.");
        }
        _portalRepositories.GetInstance<IUserRepository>().AttachAndModifyIdentity(serviceAccountId, null, i =>
        {
            i.UserStatusId = UserStatusId.INACTIVE;
        });

        // serviceAccount
        if (!string.IsNullOrWhiteSpace(result.ClientClientId))
        {
            await _provisioningManager.DeleteCentralClientAsync(result.ClientClientId).ConfigureAwait(false);
        }

        _portalRepositories.GetInstance<IUserRolesRepository>().DeleteCompanyUserAssignedRoles(result.UserRoleIds.Select(userRoleId => (serviceAccountId, userRoleId)));

        if (result.ConnectorId != null)
        {
            _portalRepositories.GetInstance<IConnectorsRepository>().AttachAndModifyConnector(result.ConnectorId.Value,
                connector =>
                {
                    connector.CompanyServiceAccountId = serviceAccountId;
                },
                connector =>
                {
                    connector.CompanyServiceAccountId = null;
                });
        }

        return await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async Task<ServiceAccountConnectorOfferData> GetOwnCompanyServiceAccountDetailsAsync(Guid serviceAccountId)
    {
        var companyId = _identityData.CompanyId;
        var result = await _portalRepositories.GetInstance<IServiceAccountRepository>().GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(serviceAccountId, companyId);
        if (result == null)
        {
            throw new ConflictException($"serviceAccount {serviceAccountId} not found for company {companyId}");
        }
        if (result.ClientClientId == null)
        {
            throw new ConflictException($"undefined clientId for serviceAccount {serviceAccountId}");
        }

        var internalClientId = await _provisioningManager.GetIdOfCentralClientAsync(result.ClientClientId).ConfigureAwait(false);

        var authData = await _provisioningManager.GetCentralClientAuthDataAsync(internalClientId).ConfigureAwait(false);
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
        var result = await _portalRepositories.GetInstance<IServiceAccountRepository>().GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(serviceAccountId, companyId);
        if (result == null)
        {
            throw new ConflictException($"serviceAccount {serviceAccountId} not found for company {companyId}");
        }
        if (result.ClientClientId == null)
        {
            throw new ConflictException($"undefined clientId for serviceAccount {serviceAccountId}");
        }
        var authData = await _provisioningManager.ResetCentralClientAuthDataAsync(result.ClientClientId).ConfigureAwait(false);
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
            throw new ArgumentException("other authenticationType values than SECRET are not supported yet", "authenticationType"); //TODO implement other authenticationTypes
        }
        if (serviceAccountId != serviceAccountDetails.ServiceAccountId)
        {
            throw new ArgumentException($"serviceAccountId {serviceAccountId} from path does not match the one in body {serviceAccountDetails.ServiceAccountId}", nameof(serviceAccountId));
        }

        var companyId = _identityData.CompanyId;
        var serviceAccountRepository = _portalRepositories.GetInstance<IServiceAccountRepository>();
        var result = await serviceAccountRepository.GetOwnCompanyServiceAccountWithIamClientIdAsync(serviceAccountId, companyId).ConfigureAwait(false);
        if (result == null)
        {
            throw new ConflictException($"serviceAccount {serviceAccountId} not found for company {companyId}");
        }
        if (result.UserStatusId == UserStatusId.INACTIVE)
        {
            throw new ConflictException($"serviceAccount {serviceAccountId} is already INACTIVE");
        }
        if (result.ClientClientId == null)
        {
            throw new ConflictException($"clientClientId of serviceAccount {serviceAccountId} should not be null");
        }

        var internalClientId = await _provisioningManager.UpdateCentralClientAsync(
            result.ClientClientId,
            new ClientConfigData(
                serviceAccountDetails.Name,
                serviceAccountDetails.Description,
                serviceAccountDetails.IamClientAuthMethod)).ConfigureAwait(false);

        var authData = await _provisioningManager.GetCentralClientAuthDataAsync(internalClientId).ConfigureAwait(false);

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

        await _portalRepositories.SaveAsync().ConfigureAwait(false);

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

    public Task<Pagination.Response<CompanyServiceAccountData>> GetOwnCompanyServiceAccountsDataAsync(int page, int size, string? clientId, bool? isOwner, bool isUserStatusActive) =>
        Pagination.CreateResponseAsync(
            page,
            size,
            15,
            _portalRepositories.GetInstance<IServiceAccountRepository>().GetOwnCompanyServiceAccountsUntracked(_identityData.CompanyId, clientId, isOwner, isUserStatusActive ? UserStatusId.ACTIVE : UserStatusId.INACTIVE));

    public IAsyncEnumerable<UserRoleWithDescription> GetServiceAccountRolesAsync(string? languageShortName) =>
        _portalRepositories.GetInstance<IUserRolesRepository>().GetServiceAccountRolesAsync(_identityData.CompanyId, _settings.ClientId, languageShortName ?? Constants.DefaultLanguage);
}
