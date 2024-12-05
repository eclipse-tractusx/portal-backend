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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
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
using System.Collections.Immutable;
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
    async Task<(bool HasExternalServiceAccount, Guid? processId, IEnumerable<CreatedServiceAccountData> ServiceAccounts)> IServiceAccountCreation.CreateServiceAccountAsync(ServiceAccountCreationInfo creationData,
            Guid companyId,
            IEnumerable<string> bpns,
            TechnicalUserTypeId technicalUserTypeId,
            bool enhanceTechnicalUserName,
            bool enabled,
            ServiceAccountCreationProcessData? processData,
            Action<TechnicalUser>? setOptionalParameter)
    {
        var (name, description, iamClientAuthMethod, userRoleIds) = creationData;
        var technicalUserRepository = portalRepositories.GetInstance<ITechnicalUserRepository>();
        var userRolesRepository = portalRepositories.GetInstance<IUserRolesRepository>();

        var userRoleData = await GetAndValidateUserRoleData(userRolesRepository, userRoleIds).ConfigureAwait(ConfigureAwaitOptions.None);
        var dimConfigRoles = _settings.DimUserRoles.SelectMany(x => x.UserRoleNames.Select(userRoleName => (x.ClientId, userRoleName)));

        var serviceAccounts = ImmutableList.CreateBuilder<CreatedServiceAccountData>();

        if (userRoleData.ExceptBy(dimConfigRoles, roleData => (roleData.ClientClientId, roleData.UserRoleText)).IfAny(
            async roleData =>
            {
                var keycloakRoleData = roleData.ToImmutableList();
                var (clientId, enhancedName, serviceAccountData) = await CreateKeycloakServiceAccount(bpns, enhanceTechnicalUserName, enabled, name, description, iamClientAuthMethod, keycloakRoleData).ConfigureAwait(ConfigureAwaitOptions.None);
                var serviceAccountId = CreateDatabaseServiceAccount(companyId, UserStatusId.ACTIVE, technicalUserTypeId, TechnicalUserKindId.INTERNAL, name, clientId, description, keycloakRoleData, technicalUserRepository, userRolesRepository, setOptionalParameter);
                serviceAccounts.Add(new CreatedServiceAccountData(
                    serviceAccountId,
                    enhancedName,
                    description,
                    UserStatusId.ACTIVE,
                    clientId,
                    serviceAccountData,
                    keycloakRoleData));
            },
            out var keycloakRolesTask))
        {
            await keycloakRolesTask.ConfigureAwait(ConfigureAwaitOptions.None);
        }

        Guid? processId = null;
        var hasExternalServiceAccount = userRoleData.IntersectBy(dimConfigRoles, roleData => (roleData.ClientClientId, roleData.UserRoleText)).IfAny(
            roleData =>
            {
                var dimRoleData = roleData.ToImmutableList();
                var dimSaName = $"dim-{name}";
                var dimServiceAccountId = CreateDatabaseServiceAccount(companyId, UserStatusId.PENDING, technicalUserTypeId, TechnicalUserKindId.EXTERNAL, dimSaName, null, description, dimRoleData, technicalUserRepository, userRolesRepository, setOptionalParameter);
                var processStepRepository = portalRepositories.GetInstance<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>();
                if (processData?.ProcessTypeId is not null)
                {
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

                    portalRepositories.GetInstance<ITechnicalUserRepository>().CreateExternalTechnicalUserCreationData(dimServiceAccountId, processId.Value);
                }

                serviceAccounts.Add(new CreatedServiceAccountData(
                    dimServiceAccountId,
                    dimSaName,
                    description,
                    UserStatusId.PENDING,
                    null,
                    null,
                    dimRoleData));
            });

        return (hasExternalServiceAccount, processId, serviceAccounts.ToImmutable());
    }

    private static async Task<IEnumerable<UserRoleData>> GetAndValidateUserRoleData(IUserRolesRepository userRolesRepository, IEnumerable<Guid> userRoleIds)
    {
        var userRoleData = await userRolesRepository
            .GetUserRoleDataUntrackedAsync(userRoleIds).ToListAsync().ConfigureAwait(false);
        if (userRoleData.Count != userRoleIds.Count())
        {
            userRoleIds.Except(userRoleData.Select(x => x.UserRoleId)).IfAny(missingRoleIds =>
                throw NotFoundException.Create(ProvisioningServiceErrors.USER_NOT_VALID_USERROLEID, [new("missingRoleIds", string.Join(", ", missingRoleIds))]));
        }

        return userRoleData;
    }

    private Guid CreateDatabaseServiceAccount(
        Guid companyId,
        UserStatusId userStatusId,
        TechnicalUserTypeId technicalUserTypeId,
        TechnicalUserKindId technicalUserKindId,
        string name,
        string? clientId,
        string description,
        IEnumerable<UserRoleData> userRoleData,
        ITechnicalUserRepository serviceAccountsRepository,
        IUserRolesRepository userRolesRepository,
        Action<TechnicalUser>? setOptionalParameter)
    {
        var identity = portalRepositories.GetInstance<IUserRepository>().CreateIdentity(companyId, userStatusId, IdentityTypeId.COMPANY_SERVICE_ACCOUNT, null);
        var serviceAccount = serviceAccountsRepository.CreateTechnicalUser(
            identity.Id,
            name,
            description,
            clientId,
            technicalUserTypeId,
            technicalUserKindId,
            setOptionalParameter);

        userRolesRepository.CreateIdentityAssignedRoleRange(
            userRoleData.Select(x => (identity.Id, x.UserRoleId)));

        return serviceAccount.Id;
    }

    private async Task<(string clientId, string enhancedName, ServiceAccountData serviceAccountData)> CreateKeycloakServiceAccount(IEnumerable<string> bpns, bool enhanceTechnicalUserName, bool enabled,
        string name, string description, IamClientAuthMethod iamClientAuthMethod, IEnumerable<UserRoleData> userRoleData)
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

        if (bpns.IfAny(async businessPartnerNumbers =>
        {
            await provisioningManager.AddBpnAttributetoUserAsync(serviceAccountData.IamUserId, businessPartnerNumbers).ConfigureAwait(ConfigureAwaitOptions.None);
            await provisioningManager.AddProtocolMapperAsync(serviceAccountData.InternalClientId).ConfigureAwait(ConfigureAwaitOptions.None);
        }, out var bpnTask))
        {
            await bpnTask.ConfigureAwait(ConfigureAwaitOptions.None);
        }

        return (clientId, enhancedName, serviceAccountData);
    }

    private async Task<string> GetNextServiceAccountClientIdWithIdAsync()
    {
        var id = await provisioningDbAccess.GetNextClientSequenceAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return $"{_settings.ServiceAccountClientPrefix}{id}";
    }
}
