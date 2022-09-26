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

using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Models;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Enums;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;

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
        if (result != default)
        {
            throw new NotFoundException($"user {iamAdminId} is not associated with any company");
        }

        var (name, description, iamClientAuthMethod, userRoleIds) = serviceAccountCreationInfos;
        var (clientId, serviceAccountData, serviceAccountId, userRoleData) = await _serviceAccountCreation.CreateServiceAccountAsync(name, description, iamClientAuthMethod, userRoleIds, result.CompanyId, Enumerable.Repeat(result.Bpn, 1));

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return new ServiceAccountDetails(
            serviceAccountId,
            clientId,
            serviceAccountCreationInfos.Name,
            serviceAccountCreationInfos.Description,
            serviceAccountCreationInfos.IamClientAuthMethod,
            userRoleData)
        {
            Secret = serviceAccountData.AuthData.Secret
        };
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
        var result = await _portalRepositories.GetInstance<IServiceAccountRepository>().GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(serviceAccountId, iamAdminId);

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
        var result = await _portalRepositories.GetInstance<IServiceAccountRepository>().GetOwnCompanyServiceAccountWithIamClientIdAsync(serviceAccountId, iamAdminId).ConfigureAwait(false);
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
        var serviceAccounts = _portalRepositories.GetInstance<IServiceAccountRepository>().GetOwnCompanyServiceAccountsUntracked(iamAdminId);

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

    public IAsyncEnumerable<UserRoleWithDescription> GetServiceAccountRolesAsync(string? languageShortName = null) =>
        _portalRepositories.GetInstance<IUserRolesRepository>().GetServiceAccountRolesAsync(_settings.ClientId,languageShortName);
}
