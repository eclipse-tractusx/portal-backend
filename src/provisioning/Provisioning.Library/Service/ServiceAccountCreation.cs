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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using ServiceAccountData = Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models.ServiceAccountData;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

/// <inheritdoc />
public class ServiceAccountCreation(
    IProvisioningManager provisioningManager,
    IPortalRepositories portalRepositories,
    IProvisioningDBAccess provisioningDbAccess,
    IOptions<ServiceAccountCreationSettings> options) : IServiceAccountCreation
{
    private readonly ServiceAccountCreationSettings _settings = options.Value;

    /// <inheritdoc />
    async Task<(bool HasExternalServiceAccount, List<CreatedServiceAccountData> ServiceAccounts)> IServiceAccountCreation.CreateServiceAccountAsync(ServiceAccountCreationInfo creationData,
            Guid companyId,
            IEnumerable<string> bpns,
            CompanyServiceAccountTypeId companyServiceAccountTypeId,
            bool enhanceTechnicalUserName,
            bool enabled,
            ServiceAccountCreationProcessData? processData,
            Action<CompanyServiceAccount>? setOptionalParameter)
    {
        var (name, description, iamClientAuthMethod, userRoleIds) = creationData;
        var serviceAccountsRepository = portalRepositories.GetInstance<IServiceAccountRepository>();
        var userRolesRepository = portalRepositories.GetInstance<IUserRolesRepository>();

        var userRoleData = await GetAndValidateUserRoleData(userRolesRepository, userRoleIds);

        var serviceAccounts = new List<CreatedServiceAccountData>();
        var groupedRoles = userRoleData.GroupBy(x => x.ProviderId);
        var hasExternalServiceAccount = false;
        foreach (var providerRoles in groupedRoles)
        {
            switch (providerRoles.Key)
            {
                case ProviderInformationId.KEYCLOAK:
                    {
                        var (clientId, enhancedName, serviceAccountData) = await CreateKeycloakServiceAccount(bpns, enhanceTechnicalUserName, enabled, name, description, iamClientAuthMethod, providerRoles).ConfigureAwait(ConfigureAwaitOptions.None);
                        var serviceAccountId = CreateDatabaseServiceAccount(companyId, UserStatusId.ACTIVE, companyServiceAccountTypeId, CompanyServiceAccountKindId.INTERNAL, name, clientId, description, providerRoles, serviceAccountsRepository, userRolesRepository, setOptionalParameter);
                        serviceAccounts.Add(new CreatedServiceAccountData(
                            serviceAccountId,
                            enhancedName,
                            description,
                            UserStatusId.ACTIVE,
                            clientId,
                            serviceAccountData,
                            providerRoles.Select(x => new UserRoleData(x.UserRoleId, x.ClientClientId, x.UserRoleText))));
                        break;
                    }
                case ProviderInformationId.SAP_DIM:
                    {
                        var dimSaName = $"dim-{name}";
                        const string ClientPlaceholderId = "xxx";
                        var serviceAccountId = CreateDatabaseServiceAccount(companyId, UserStatusId.PENDING, companyServiceAccountTypeId, CompanyServiceAccountKindId.EXTERNAL, dimSaName, ClientPlaceholderId, description, providerRoles, serviceAccountsRepository, userRolesRepository, setOptionalParameter);
                        var processStepRepository = portalRepositories.GetInstance<IProcessStepRepository>();
                        if (processData?.ProcessTypeId is not null)
                        {
                            Guid processId;
                            if (processData.ProcessId is null)
                            {
                                var process = processStepRepository.CreateProcess(processData.ProcessTypeId.Value);
                                processStepRepository.CreateProcessStep(processData.ProcessTypeId.Value.GetInitialProcessStepTypeIdForSaCreation(), ProcessStepStatusId.TODO, process.Id);
                                processId = process.Id;
                            }
                            else
                            {
                                processId = processData.ProcessId.Value;
                            }

                            portalRepositories.GetInstance<IServiceAccountRepository>().CreateDimUserCreationData(serviceAccountId, processId);
                        }

                        hasExternalServiceAccount = true;
                        serviceAccounts.Add(new CreatedServiceAccountData(
                            serviceAccountId,
                            dimSaName,
                            description,
                            UserStatusId.PENDING,
                            ClientPlaceholderId,
                            new ServiceAccountData("xxx", "xxx", new ClientAuthData(IamClientAuthMethod.JWT)),
                            providerRoles.Select(x => new UserRoleData(x.UserRoleId, x.ClientClientId, x.UserRoleText))));
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(
                        $"Only supported Providers are {ProviderInformationId.KEYCLOAK} and {ProviderInformationId.SAP_DIM}");
            }
        }

        return (hasExternalServiceAccount, serviceAccounts);
    }

    private static async Task<List<UserRoleWithProviderData>> GetAndValidateUserRoleData(IUserRolesRepository userRolesRepository, IEnumerable<Guid> userRoleIds)
    {
        var userRoleData = await userRolesRepository
            .GetUserRoleDataUntrackedAsync(userRoleIds).ToListAsync().ConfigureAwait(false);
        if (userRoleData.Count != userRoleIds.Count())
        {
            var missingRoleIds = userRoleIds.Except(userRoleData.Select(x => x.UserRoleId));

            if (missingRoleIds.Any())
            {
                throw NotFoundException.Create(ProvisioningServiceErrors.USER_NOT_VALID_USERROLEID, new ErrorParameter[] { new("missingRoleIds", string.Join(", ", missingRoleIds)) });
            }
        }

        return userRoleData;
    }

    private Guid CreateDatabaseServiceAccount(
        Guid companyId,
        UserStatusId userStatusId,
        CompanyServiceAccountTypeId companyServiceAccountTypeId,
        CompanyServiceAccountKindId companyServiceAccountKindId,
        string name,
        string clientId,
        string description,
        IEnumerable<UserRoleWithProviderData> userRoleData,
        IServiceAccountRepository serviceAccountsRepository,
        IUserRolesRepository userRolesRepository,
        Action<CompanyServiceAccount>? setOptionalParameter)
    {
        var identity = portalRepositories.GetInstance<IUserRepository>().CreateIdentity(companyId, userStatusId, IdentityTypeId.COMPANY_SERVICE_ACCOUNT, null);
        var serviceAccount = serviceAccountsRepository.CreateCompanyServiceAccount(
            identity.Id,
            name,
            description,
            clientId,
            companyServiceAccountTypeId,
            companyServiceAccountKindId,
            setOptionalParameter);

        foreach (var roleData in userRoleData)
        {
            userRolesRepository.CreateIdentityAssignedRole(serviceAccount.Id, roleData.UserRoleId);
        }

        return serviceAccount.Id;
    }

    private async Task<(string clientId, string enhancedName, ServiceAccountData serviceAccountData)> CreateKeycloakServiceAccount(IEnumerable<string> bpns, bool enhanceTechnicalUserName, bool enabled,
        string name, string description, IamClientAuthMethod iamClientAuthMethod, IEnumerable<UserRoleWithProviderData> userRoleData)
    {
        var clientId = await GetNextServiceAccountClientIdWithIdAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        var enhancedName = enhanceTechnicalUserName ? $"{clientId}-{name}" : name;
        var serviceAccountData = await provisioningManager.SetupCentralServiceAccountClientAsync(
            clientId,
            new ClientConfigRolesData(
                enhancedName,
                description,
                iamClientAuthMethod,
                userRoleData
                    .GroupBy(userRole =>
                        userRole.ClientClientId)
                    .ToDictionary(group =>
                            group.Key,
                        group => group.Select(userRole => userRole.UserRoleText))),
            enabled).ConfigureAwait(ConfigureAwaitOptions.None);

        if (bpns.Any())
        {
            await provisioningManager.AddBpnAttributetoUserAsync(serviceAccountData.IamUserId, bpns).ConfigureAwait(ConfigureAwaitOptions.None);
            await provisioningManager.AddProtocolMapperAsync(serviceAccountData.InternalClientId).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        return (clientId, enhancedName, serviceAccountData);
    }

    private async Task<string> GetNextServiceAccountClientIdWithIdAsync()
    {
        var id = await provisioningDbAccess.GetNextClientSequenceAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return $"{_settings.ServiceAccountClientPrefix}{id}";
    }
}
