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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

public partial class ProvisioningManager
{
    private static readonly string MasterRealm = "master";
    public async Task<ServiceAccountData> SetupCentralServiceAccountClientAsync(string clientId, ClientConfigRolesData config, bool enabled)
    {
        var internalClientId = await CreateServiceAccountClient(_CentralIdp, _Settings.CentralRealm, clientId, config.Name, config.IamClientAuthMethod, enabled);
        var serviceAccountUser = await _CentralIdp.GetUserForServiceAccountAsync(_Settings.CentralRealm, internalClientId).ConfigureAwait(false);
        if (serviceAccountUser.Id == null)
        {
            throw new KeycloakEntityConflictException($"serviceAccountUser {internalClientId} has no Id in keycloak");
        }

        var assignedRoles = await AssignClientRolesToCentralUserAsync(serviceAccountUser.Id, config.ClientRoles)
            .ToDictionaryAsync(assigned => assigned.Client, assigned => (assigned.Roles, assigned.Error)).ConfigureAwait(false);

        config.ClientRoles
            .Select(clientRoles => (ClientRoles: clientRoles, Assigned: assignedRoles[clientRoles.Key]))
            .Select(x => (Client: x.ClientRoles.Key, UnAssignedRoles: x.ClientRoles.Value.Except(x.Assigned.Roles), Error: x.Assigned.Error))
            .Where(x => x.UnAssignedRoles.Any())
            .IfAny(unassignedClientRoles =>
            {
                throw new KeycloakNoSuccessException($"inconsistend data. roles were not assigned in keycloak: {string.Join(", ", unassignedClientRoles.Select(clientRoles => $"client: {clientRoles.Client}, roles: [{string.Join(", ", clientRoles.UnAssignedRoles)}], error: {clientRoles.Error?.Message}"))}");
            });

        return new ServiceAccountData(
            internalClientId,
            serviceAccountUser.Id,
            await GetCentralClientAuthDataAsync(internalClientId).ConfigureAwait(false));
    }

    public async Task<string?> GetServiceAccountUserId(string clientId)
    {
        var internalClientId = (await _CentralIdp.GetClientsAsync(_Settings.CentralRealm, clientId).ConfigureAwait(false)).FirstOrDefault(c => c.ClientId == clientId)?.Id;
        if (internalClientId == null)
        {
            throw new KeycloakEntityNotFoundException($"clientId {clientId} not found on central idp");
        }
        return (await _CentralIdp.GetUserForServiceAccountAsync(_Settings.CentralRealm, internalClientId).ConfigureAwait(false)).Id;
    }

    private async Task<(string ClientId, string Secret)> CreateSharedIdpServiceAccountAsync(string realm)
    {
        var sharedIdp = _Factory.CreateKeycloakClient("shared");
        var clientId = GetServiceAccountClientId(realm);
        var internalClientId = await CreateServiceAccountClient(sharedIdp, MasterRealm, clientId, clientId, IamClientAuthMethod.SECRET, true);
        var serviceAccountUser = await sharedIdp.GetUserForServiceAccountAsync(MasterRealm, internalClientId).ConfigureAwait(false);
        var roleCreateRealm = await sharedIdp.GetRoleByNameAsync(MasterRealm, "create-realm").ConfigureAwait(false);

        await sharedIdp.AddRealmRoleMappingsToUserAsync(MasterRealm, serviceAccountUser.Id ?? throw new KeycloakInvalidResponseException("id of serviceAccountUser is null"), Enumerable.Repeat(roleCreateRealm, 1)).ConfigureAwait(false);

        var credentials = await sharedIdp.GetClientSecretAsync(MasterRealm, internalClientId).ConfigureAwait(false);
        return new ValueTuple<string, string>(clientId, credentials.Value);
    }

    private async Task<(string ClientId, string Secret)> GetSharedIdpServiceAccountSecretAsync(string realm)
    {
        var clientId = GetServiceAccountClientId(realm);
        var sharedIdp = _Factory.CreateKeycloakClient("shared");
        var internalClientId = await GetInternalClientIdOfSharedIdpServiceAccount(sharedIdp, clientId).ConfigureAwait(false);
        var credentials = await sharedIdp.GetClientSecretAsync(MasterRealm, internalClientId).ConfigureAwait(false);
        return new ValueTuple<string, string>(clientId, credentials.Value);
    }

    private async Task DeleteSharedIdpServiceAccountAsync(KeycloakClient keycloak, string realm)
    {
        var clientId = GetServiceAccountClientId(realm);
        var internalClientId = await GetInternalClientIdOfSharedIdpServiceAccount(keycloak, clientId).ConfigureAwait(false);
        await keycloak.DeleteClientAsync(MasterRealm, internalClientId).ConfigureAwait(false);
    }

    private async Task<string> CreateServiceAccountClient(KeycloakClient keycloak, string realm, string clientId, string name, IamClientAuthMethod iamClientAuthMethod, bool enabled)
    {
        var newClient = Clone(_Settings.ServiceAccountClient);
        newClient.ClientId = clientId;
        newClient.Name = name;
        newClient.ClientAuthenticatorType = IamClientAuthMethodToInternal(iamClientAuthMethod);
        newClient.Enabled = enabled;
        var newClientId = await keycloak.CreateClientAndRetrieveClientIdAsync(realm, newClient).ConfigureAwait(false);
        if (newClientId == null)
        {
            throw new KeycloakNoSuccessException($"failed to create new client {clientId} in central realm");
        }
        return newClientId;
    }

    private static async Task<string> GetInternalClientIdOfSharedIdpServiceAccount(KeycloakClient keycloak, string clientId)
    {
        var internalClientId = (await keycloak.GetClientsAsync(MasterRealm, clientId).ConfigureAwait(false)).FirstOrDefault(c => c.ClientId == clientId)?.Id;
        if (internalClientId == null)
        {
            throw new KeycloakEntityNotFoundException($"clientId {clientId} not found on shared idp");
        }
        return internalClientId;
    }

    private string GetServiceAccountClientId(string realm) =>
        _Settings.ServiceAccountClientPrefix + realm;
}
