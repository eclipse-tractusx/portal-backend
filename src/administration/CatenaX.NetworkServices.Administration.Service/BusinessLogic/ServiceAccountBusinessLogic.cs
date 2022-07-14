/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Provisioning.Library.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class ServiceAccountBusinessLogic : IServiceAccountBusinessLogic
{
    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalRepositories _portalRepositories;

    public ServiceAccountBusinessLogic(
        IProvisioningManager provisioningManager,
        IPortalRepositories portalRepositories)
    {
        _provisioningManager = provisioningManager;
        _portalRepositories = portalRepositories;
    }

    public async Task<ServiceAccountDetails> CreateOwnCompanyServiceAccountAsync(ServiceAccountCreationInfo serviceAccountCreationInfos, string iamAdminId)
    {
        if (serviceAccountCreationInfos.IamClientAuthMethod != IamClientAuthMethod.SECRET)
        {
            throw new ArgumentException("other authenticationType values than SECRET are not supported yet", "authenticationType"); //TODO implement other authenticationTypes
        }
        if (String.IsNullOrWhiteSpace(serviceAccountCreationInfos.Name))
        {
            throw new ArgumentException("name must not be empty","name");
        }

        var companyId = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyIdForIamUserUntrackedAsync(iamAdminId).ConfigureAwait(false);
        if (companyId == default)
        {
            throw new NotFoundException($"user {iamAdminId} is not associated with any company");
        }

        var serviceAccountsRepository = _portalRepositories.GetInstance<IServiceAccountsRepository>();

        var userRoleDatas = await _portalRepositories.GetInstance<IUserRolesRepository>().GetUserRoleDataUntrackedAsync(serviceAccountCreationInfos.UserRoleIds).ToListAsync().ConfigureAwait(false);

        if (userRoleDatas.Count() != serviceAccountCreationInfos.UserRoleIds.Count())
        {
            var missingRoleIds = serviceAccountCreationInfos.UserRoleIds
                .Where(userRoleId => userRoleDatas.All(userRoleData => userRoleData.UserRoleId != userRoleId));
            
            if (missingRoleIds.Count() > 0)
            {
                throw new NotFoundException($"{missingRoleIds.First()} is not a valid UserRoleId");
            }
        }

        var clientId = await _provisioningManager.GetNextServiceAccountClientIdAsync().ConfigureAwait(false);

        var serviceAccountData = await _provisioningManager.SetupCentralServiceAccountClientAsync(
            clientId,
            new ClientConfigRolesData(
                serviceAccountCreationInfos.Name,
                serviceAccountCreationInfos.Description,
                serviceAccountCreationInfos.IamClientAuthMethod,
                userRoleDatas
                    .GroupBy(userRoleData =>
                        userRoleData.ClientClientId)
                    .ToDictionary(group =>
                        group.Key,
                        group => group.Select(userRoleData => userRoleData.UserRoleText)))).ConfigureAwait(false);

        var serviceAccount = serviceAccountsRepository.CreateCompanyServiceAccount(
            companyId,
            CompanyServiceAccountStatusId.ACTIVE,
            serviceAccountCreationInfos.Name,
            serviceAccountCreationInfos.Description);

        serviceAccountsRepository.CreateIamServiceAccount(
            serviceAccountData.InternalClientId,
            clientId,
            serviceAccountData.UserEntityId,
            serviceAccount.Id);

        foreach(var userRoleData in userRoleDatas)
        {
            serviceAccountsRepository.CreateCompanyServiceAccountAssignedRole(serviceAccount.Id, userRoleData.UserRoleId);
        }

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return new ServiceAccountDetails(
            serviceAccount.Id,
            clientId,
            serviceAccountCreationInfos.Name,
            serviceAccountCreationInfos.Description,
            serviceAccountCreationInfos.IamClientAuthMethod,
            userRoleDatas)
        {
            Secret = serviceAccountData.AuthData.Secret
        };
    }

    public async Task<int> DeleteOwnCompanyServiceAccountAsync(Guid serviceAccountId, string iamAdminId)
    {
        var serviceAccountRepository = _portalRepositories.GetInstance<IServiceAccountsRepository>();
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
        var result = await _portalRepositories.GetInstance<IServiceAccountsRepository>().GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(serviceAccountId, iamAdminId);

        if (result == null)
        {
            throw new NotFoundException($"serviceAccount {serviceAccountId} not found in company of {iamAdminId}");
        }
        var authData = await _provisioningManager.GetCentralClientAuthDataAsync(result.ClientId).ConfigureAwait(false);
        return new ServiceAccountDetails(
            result.ServiceAccountId,
            result.ClientClientId,
            result.Name,
            result.Description,
            authData.IamClientAuthMethod,
            result.UserRoleDatas)
            {
                Secret = authData.Secret
            };
    }

    public async Task<ServiceAccountDetails> ResetOwnCompanyServiceAccountSecretAsync(Guid serviceAccountId, string iamAdminId)
    {
        var result = await _portalRepositories.GetInstance<IServiceAccountsRepository>().GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(serviceAccountId, iamAdminId);

        if (result == null)
        {
            throw new NotFoundException($"serviceAccount {serviceAccountId} not found in company of {iamAdminId}");
        }
        var authData = await _provisioningManager.ResetCentralClientAuthDataAsync(result.ClientId).ConfigureAwait(false);
        return new ServiceAccountDetails(
            result.ServiceAccountId,
            result.ClientClientId,
            result.Name,
            result.Description,
            authData.IamClientAuthMethod,
            result.UserRoleDatas)
            {
                Secret = authData.Secret
            };
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
        var result = await _portalRepositories.GetInstance<IServiceAccountsRepository>().GetOwnCompanyServiceAccountWithIamClientIdAsync(serviceAccountId, iamAdminId).ConfigureAwait(false);
        if (result == null)
        {
            throw new NotFoundException($"serviceAccount {serviceAccountId} not found in company of user {iamAdminId}");
        }
        var serviceAccount = result.CompanyServiceAccount;
        if (serviceAccount.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.INACTIVE)
        {
            throw new ArgumentException($"serviceAccount {serviceAccountId} is already INACTIVE");
        }

        await _provisioningManager.UpdateCentralClientAsync(
            result.ClientId,
            new ClientConfigData(
                serviceAccountEditableDetails.Name,
                serviceAccountEditableDetails.Description,
                serviceAccountEditableDetails.IamClientAuthMethod)).ConfigureAwait(false);
        
        var authData = await _provisioningManager.GetCentralClientAuthDataAsync(result.ClientId).ConfigureAwait(false);

        serviceAccount.Name = serviceAccountEditableDetails.Name;
        serviceAccount.Description = serviceAccountEditableDetails.Description;

        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        return new ServiceAccountDetails(
            serviceAccount.Id,
            result.ClientClientId,
            serviceAccount.Name,
            serviceAccount.Description,
            authData.IamClientAuthMethod,
            result.UserRoleDatas)
        {
            Secret = authData.Secret
        };
    }

    public Task<Pagination.Response<CompanyServiceAccountData>> GetOwnCompanyServiceAccountsDataAsync(int page, int size, string iamAdminId)
    {
        var serviceAccounts = _portalRepositories.GetInstance<IServiceAccountsRepository>().GetOwnCompanyServiceAccountsUntracked(iamAdminId);

        return Pagination.CreateResponseAsync<CompanyServiceAccountData>(
            page,
            size,
            15,
            (int skip, int take) => new Pagination.AsyncSource<CompanyServiceAccountData>(
                serviceAccounts.CountAsync(),
                serviceAccounts.OrderBy(serviceAccount => serviceAccount.Name)
                    .Skip(skip)
                    .Take(take)
                    .Select(serviceAccount => new CompanyServiceAccountData(
                        serviceAccount.Id,
                        serviceAccount.IamServiceAccount!.ClientClientId,
                        serviceAccount.Name))
                    .AsAsyncEnumerable()));
    }
}
