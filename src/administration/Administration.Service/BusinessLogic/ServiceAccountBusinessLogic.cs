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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
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
    private readonly ServiceAccountSettings _settings;
    

    public ServiceAccountBusinessLogic(
        IProvisioningManager provisioningManager,
        IPortalRepositories portalRepositories,
        IOptions<ServiceAccountSettings> options,
        IServiceAccountCreation serviceAccountCreation)
    {
        _provisioningManager = provisioningManager;
        _portalRepositories = portalRepositories;
        _serviceAccountCreation = serviceAccountCreation;
        _settings = options.Value;
    }

    public async Task<ServiceAccountDetails> CreateOwnCompanyServiceAccountAsync(ServiceAccountCreationInfo serviceAccountCreationInfos, string iamAdminId)
    {
        if (serviceAccountCreationInfos.IamClientAuthMethod != IamClientAuthMethod.SECRET)
        {
            throw new ControllerArgumentException("other authenticationType values than SECRET are not supported yet", "authenticationType"); //TODO implement other authenticationTypes
        }
        if (string.IsNullOrWhiteSpace(serviceAccountCreationInfos.Name))
        {
            throw new ControllerArgumentException("name must not be empty","name");
        }

        var result = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyIdAndBpnForIamUserUntrackedAsync(iamAdminId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"user {iamAdminId} is not associated with any company");
        }

        var companyServiceAccountTypeId = CompanyServiceAccountTypeId.OWN;
        var (clientId, serviceAccountData, serviceAccountId, userRoleData) = await _serviceAccountCreation.CreateServiceAccountAsync(serviceAccountCreationInfos, result.CompanyId, Enumerable.Repeat(result.Bpn, 1), companyServiceAccountTypeId, false).ConfigureAwait(false);

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

    public async Task<int> DeleteOwnCompanyServiceAccountAsync(Guid serviceAccountId, string iamAdminId)
    {
        var serviceAccountRepository = _portalRepositories.GetInstance<IServiceAccountRepository>();
        var serviceAccount = await serviceAccountRepository.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(serviceAccountId, iamAdminId).ConfigureAwait(false);
        if (serviceAccount == null)
        {
            throw new NotFoundException($"serviceAccount {serviceAccountId} not found in company of user {iamAdminId}");
        }
        serviceAccount.CompanyServiceAccountStatusId = CompanyServiceAccountStatusId.INACTIVE;
        if (serviceAccount.IamServiceAccount != null)
        {
            await _provisioningManager.DeleteCentralClientAsync(serviceAccount.IamServiceAccount.ClientId).ConfigureAwait(false);
            serviceAccountRepository.RemoveIamServiceAccount(serviceAccount.IamServiceAccount);
        }
        foreach(var companyServiceAccountAssignedRole in serviceAccount.CompanyServiceAccountAssignedRoles)
        {
            serviceAccountRepository.RemoveCompanyServiceAccountAssignedRole(companyServiceAccountAssignedRole);
        }
        return await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async Task<ServiceAccountDetails> GetOwnCompanyServiceAccountDetailsAsync(Guid serviceAccountId, string iamAdminId)
    {
        var result = await _portalRepositories.GetInstance<IServiceAccountRepository>().GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(serviceAccountId, iamAdminId);

        if (result == null)
        {
            throw new NotFoundException($"serviceAccount {serviceAccountId} not found in company of {iamAdminId}");
        }
        if (result.ClientId == null || result.ClientClientId == null)
        {
            throw new ConflictException($"undefined clientId for serviceAccount {serviceAccountId}");
        }
        var authData = await _provisioningManager.GetCentralClientAuthDataAsync(result.ClientId).ConfigureAwait(false);
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

    public async Task<ServiceAccountDetails> ResetOwnCompanyServiceAccountSecretAsync(Guid serviceAccountId, string iamAdminId)
    {
        var result = await _portalRepositories.GetInstance<IServiceAccountRepository>().GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(serviceAccountId, iamAdminId);

        if (result == null)
        {
            throw new NotFoundException($"serviceAccount {serviceAccountId} not found in company of {iamAdminId}");
        }
        if (result.ClientId == null || result.ClientClientId == null)
        {
            throw new ConflictException($"undefined clientId for serviceAccount {serviceAccountId}");
        }
        var authData = await _provisioningManager.ResetCentralClientAuthDataAsync(result.ClientId).ConfigureAwait(false);
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

    public async Task<ServiceAccountDetails> UpdateOwnCompanyServiceAccountDetailsAsync(Guid serviceAccountId, ServiceAccountEditableDetails serviceAccountEditableDetails, string iamAdminId)
    {
        if (serviceAccountEditableDetails.IamClientAuthMethod != IamClientAuthMethod.SECRET)
        {
            throw new ArgumentException("other authenticationType values than SECRET are not supported yet", "authenticationType"); //TODO implement other authenticationTypes
        }
        if (serviceAccountId != serviceAccountEditableDetails.ServiceAccountId)
        {
            throw new ArgumentException($"serviceAccountId {serviceAccountId} from path does not match the one in body {serviceAccountEditableDetails.ServiceAccountId}","serviceAccountId");
        }
        var serviceAccountRepository = _portalRepositories.GetInstance<IServiceAccountRepository>();
        var result = await serviceAccountRepository.GetOwnCompanyServiceAccountWithIamClientIdAsync(serviceAccountId, iamAdminId).ConfigureAwait(false);
        if (result == null)
        {
            throw new NotFoundException($"serviceAccount {serviceAccountId} not found in company of {iamAdminId}");
        }
        if (result.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.INACTIVE)
        {
            throw new ArgumentException($"serviceAccount {serviceAccountId} is already INACTIVE");
        }
        if (result.ClientId == null)
        {
            throw new ConflictException($"clientId of serviceAccount {serviceAccountId} should not be null");
        }
        if (result.ClientClientId == null)
        {
            throw new ConflictException($"clientClientId of serviceAccount {serviceAccountId} should not be null");
        }

        await _provisioningManager.UpdateCentralClientAsync(
            result.ClientId,
            new ClientConfigData(
                serviceAccountEditableDetails.Name,
                serviceAccountEditableDetails.Description,
                serviceAccountEditableDetails.IamClientAuthMethod)).ConfigureAwait(false);
        
        var authData = await _provisioningManager.GetCentralClientAuthDataAsync(result.ClientId).ConfigureAwait(false);

        serviceAccountRepository.AttachAndModifyCompanyServiceAccount(
            serviceAccountId,
            sa => {
                sa.Name = result.Name;
                sa.Description = result.Description;
            },
            sa => {
                sa.Name = serviceAccountEditableDetails.Name;
                sa.Description = serviceAccountEditableDetails.Description;
            });

        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        return new ServiceAccountDetails(
            result.ServiceAccountId,
            result.ClientClientId,
            serviceAccountEditableDetails.Name,
            serviceAccountEditableDetails.Description,
            authData.IamClientAuthMethod,
            result.UserRoleDatas,
            result.CompanyServiceAccountTypeId,
            authData.Secret,
            result.OfferSubscriptionId);
    }

    public Task<Pagination.Response<CompanyServiceAccountData>> GetOwnCompanyServiceAccountsDataAsync(int page, int size, string iamAdminId) =>
        Pagination.CreateResponseAsync(
            page,
            size,
            15,
            _portalRepositories.GetInstance<IServiceAccountRepository>().GetOwnCompanyServiceAccountsUntracked(iamAdminId));

    public IAsyncEnumerable<UserRoleWithDescription> GetServiceAccountRolesAsync(string? languageShortName = null) =>
        _portalRepositories.GetInstance<IUserRolesRepository>().GetServiceAccountRolesAsync(_settings.ClientId,languageShortName);
}
