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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

public partial class ProvisioningManager
{
    public async Task<ServiceAccountData> SetupCentralServiceAccountClientAsync(string clientId, ClientConfigRolesData config)
    {
        var internalClientId = await CreateServiceAccountClient(_CentralIdp, _Settings.CentralRealm, clientId, config.Name, config.IamClientAuthMethod);
        var serviceAccountUser = await _CentralIdp.GetUserForServiceAccountAsync(_Settings.CentralRealm, internalClientId).ConfigureAwait(false);
        if (serviceAccountUser.Id == null)
        {
            throw new KeycloakEntityConflictException($"serviceAccountUser {internalClientId} has no Id in keycloak");
        }
        var assignedRoles = await AssignClientRolesToCentralUserAsync(serviceAccountUser.Id, config.ClientRoles)
            .ToDictionaryAsync(assigned => assigned.Client, assigned => assigned.Roles).ConfigureAwait(false);

        var unassignedClientRoles = config.ClientRoles
            .Select(clientRoles => (client: clientRoles.Key, roles: clientRoles.Value.Except(assignedRoles[clientRoles.Key])))
            .Where(clientRoles => clientRoles.roles.Any());
 
        if (unassignedClientRoles.Any())
        {
            throw new KeycloakNoSuccessException($"inconsistend data. roles were not assigned in keycloak: {string.Join(", ", unassignedClientRoles.Select(clientRoles => $"client: {clientRoles.client}, roles: [{string.Join(", ", clientRoles.roles)}]"))}");
        }

        return new ServiceAccountData(
            internalClientId,
            serviceAccountUser.Id,
            await GetCentralClientAuthDataAsync(internalClientId).ConfigureAwait(false));
    }

    private async Task<(string ClientId, string Secret)> CreateSharedIdpServiceAccountAsync(string realm)
    {
        var sharedIdp = _Factory.CreateKeycloakClient("shared");
        var clientId = GetServiceAccountClientId(realm);
        var internalClientId = await CreateServiceAccountClient(sharedIdp, "master", clientId, clientId, IamClientAuthMethod.SECRET);
        var serviceAccountUser = await sharedIdp.GetUserForServiceAccountAsync("master", internalClientId).ConfigureAwait(false);
        var roleCreateRealm = await sharedIdp.GetRoleByNameAsync("master", "create-realm").ConfigureAwait(false);

        await sharedIdp.AddRealmRoleMappingsToUserAsync("master", serviceAccountUser.Id, Enumerable.Repeat(roleCreateRealm, 1)).ConfigureAwait(false);

        var credentials = await sharedIdp.GetClientSecretAsync("master", internalClientId).ConfigureAwait(false);
        return new ValueTuple<string,string>(clientId, credentials.Value);
    }

    private async Task<(string ClientId, string Secret)> GetSharedIdpServiceAccountSecretAsync(string realm)
    {
        var clientId = GetServiceAccountClientId(realm);
        var sharedIdp = _Factory.CreateKeycloakClient("shared");
        var internalClientId = await GetInternalClientIdOfSharedIdpServiceAccount(sharedIdp, clientId).ConfigureAwait(false);
        var credentials = await sharedIdp.GetClientSecretAsync("master", internalClientId).ConfigureAwait(false);
        return new ValueTuple<string,string>(clientId, credentials.Value);
    }

    private async Task DeleteSharedIdpServiceAccountAsync(KeycloakClient keycloak, string realm)
    {
        var clientId = GetServiceAccountClientId(realm);
        var internalClientId = await GetInternalClientIdOfSharedIdpServiceAccount(keycloak, clientId).ConfigureAwait(false);
        await keycloak.DeleteClientAsync("master", internalClientId).ConfigureAwait(false);
    }

    private async Task<string> CreateServiceAccountClient(KeycloakClient keycloak, string realm, string clientId, string name, IamClientAuthMethod iamClientAuthMethod)
    {
        var newClient = Clone(_Settings.ServiceAccountClient);
        newClient.ClientId = clientId;
        newClient.Name = name;
        newClient.ClientAuthenticatorType = IamClientAuthMethodToInternal(iamClientAuthMethod);
        var newClientId = await keycloak.CreateClientAndRetrieveClientIdAsync(realm, newClient).ConfigureAwait(false);
        if (newClientId == null)
        {
            throw new KeycloakNoSuccessException($"failed to create new client {clientId} in central realm");
        }
        return newClientId;
    }

    private static async Task<string> GetInternalClientIdOfSharedIdpServiceAccount(KeycloakClient keycloak, string clientId)
    {
        var internalClientId = (await keycloak.GetClientsAsync("master", clientId).ConfigureAwait(false)).FirstOrDefault(c => c.ClientId == clientId)?.Id;
        if (internalClientId == null)
        {
            throw new KeycloakEntityNotFoundException($"clientId {clientId} not found on shared idp");
        }
        return internalClientId;
    }

    private string GetServiceAccountClientId(string realm) =>
        _Settings.ServiceAccountClientPrefix + realm;
}
