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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

/// <inheritdoc />
public class ServiceAccountCreation : IServiceAccountCreation
{
    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    /// Creates a new instance of <see cref="ServiceAccountCreation"/>
    /// </summary>
    /// <param name="provisioningManager">Access to the provisioning Manager and keycloak</param>
    /// <param name="portalRepositories">Access to the database</param>
    public ServiceAccountCreation(IProvisioningManager provisioningManager, IPortalRepositories portalRepositories)
    {
        _provisioningManager = provisioningManager;
        _portalRepositories = portalRepositories;
    }

    /// <inheritdoc />
    public async Task<(string clientId, ServiceAccountData serviceAccountData, Guid serviceAccountId, List<UserRoleData> userRoleData)> CreateServiceAccountAsync(
        ServiceAccountCreationInfo creationData,
        Guid companyId,
        IEnumerable<string> bpns,
        CompanyServiceAccountTypeId companyServiceAccountTypeId,
        Action<CompanyServiceAccount>? setOptionalParameter = null)
    {
        var (name, description, iamClientAuthMethod, userRoleIds) = creationData;
        var serviceAccountsRepository = _portalRepositories.GetInstance<IServiceAccountRepository>();

        var userRoleData = await _portalRepositories.GetInstance<IUserRolesRepository>()
            .GetUserRoleDataUntrackedAsync(userRoleIds).ToListAsync().ConfigureAwait(false);
        if (userRoleData.Count != userRoleIds.Count())
        {
            var missingRoleIds = userRoleIds
                .Where(userRoleId => userRoleData.All(userRole => userRole.UserRoleId != userRoleId))
                .ToList();

            if (missingRoleIds.Any())
            {
                throw new NotFoundException($"{missingRoleIds.First()} is not a valid UserRoleId");
            }
        }

        var clientId = await _provisioningManager.GetNextServiceAccountClientIdAsync().ConfigureAwait(false);
        var serviceAccountData = await _provisioningManager.SetupCentralServiceAccountClientAsync(
            clientId,
            new ClientConfigRolesData(
                name,
                description,
                iamClientAuthMethod,
                userRoleData
                    .GroupBy(userRole =>
                        userRole.ClientClientId)
                    .ToDictionary(group =>
                            group.Key,
                        group => group.Select(userRole => userRole.UserRoleText)))).ConfigureAwait(false);
        
        if (bpns.Any())
        {
            await _provisioningManager.AddBpnAttributetoUserAsync(serviceAccountData.UserEntityId, bpns).ConfigureAwait(false);
            await _provisioningManager.AddProtocolMapperAsync(serviceAccountData.InternalClientId).ConfigureAwait(false);
        }
        
        var serviceAccount = serviceAccountsRepository.CreateCompanyServiceAccount(
            companyId,
            CompanyServiceAccountStatusId.ACTIVE,
            name,
            description,
            companyServiceAccountTypeId,
            setOptionalParameter);

        serviceAccountsRepository.CreateIamServiceAccount(
            serviceAccountData.InternalClientId,
            clientId,
            serviceAccountData.UserEntityId,
            serviceAccount.Id);

        foreach (var userRole in userRoleData)
        {
            serviceAccountsRepository.CreateCompanyServiceAccountAssignedRole(serviceAccount.Id, userRole.UserRoleId);
        }

        return (clientId, serviceAccountData, serviceAccount.Id, userRoleData);
    }
}
