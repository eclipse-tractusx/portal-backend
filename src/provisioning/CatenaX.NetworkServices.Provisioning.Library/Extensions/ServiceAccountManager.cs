using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Models;

namespace CatenaX.NetworkServices.Provisioning.Library;

public partial class ProvisioningManager
{
    public async Task<string> GetNextServiceAccountClientIdAsync() =>
        _Settings.ServiceAccountClientPrefix + (await _ProvisioningDBAccess!.GetNextClientSequenceAsync().ConfigureAwait(false));

    public async Task<ServiceAccountData> SetupCentralServiceAccountClientAsync(string clientId, ClientConfigRolesData config)
    {
        try
        {
            var internalClientId = await CreateCentralServiceAccountClient(clientId, config.Name, config.IamClientAuthMethod);
            var serviceAccountUser = await _CentralIdp.GetUserForServiceAccountAsync(_Settings.CentralRealm, internalClientId).ConfigureAwait(false);
            if (serviceAccountUser == null) //TODO this check might be obsolete, verify NotFoundException not being thrown.
            {
                throw new Exception($"error retrieving service account user for newly created service-account-client {internalClientId}");
            }
            var assignedRoles = await AssignClientRolesToCentralUserAsync(serviceAccountUser.Id, config.ClientRoles).ConfigureAwait(false);

            var unassignedClientRoles = config.ClientRoles
                .Select(clientRoles => (client: clientRoles.Key, roles: clientRoles.Value.Except(assignedRoles[clientRoles.Key])))
                .Where(clientRoles => clientRoles.roles.Count() > 0);

            if (unassignedClientRoles.Count() > 0)
            {
                throw new Exception($"inconsistend data. roles were not assigned in keycloak: {String.Join(", ",unassignedClientRoles.Select(clientRoles => $"client: {clientRoles.client}, roles: [{String.Join(", ",clientRoles.roles)}]"))}");
            }

            return new ServiceAccountData(
                internalClientId,
                serviceAccountUser.Id,
                await GetCentralClientAuthDataAsync(internalClientId).ConfigureAwait(false));
        }
        catch(NotFoundException nfe)
        {
            throw new Exception(nfe?.Message);
        }
    }

    private async Task<string> CreateCentralServiceAccountClient(string clientId, string name, IamClientAuthMethod iamClientAuthMethod)
    {
        var newClient = CloneClient(_Settings.ServiceAccountClient);
        newClient.ClientId = clientId;
        newClient.Name = name;
        newClient.ClientAuthenticatorType = IamClientAuthMethodToInternal(iamClientAuthMethod);
        var newClientId = await _CentralIdp.CreateClientAndRetrieveClientIdAsync(_Settings.CentralRealm, newClient).ConfigureAwait(false);
        if (newClientId == null)
        {
            throw new Exception($"failed to create new client {clientId} in central realm");
        }
        return newClientId;
    }
}
