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
        var internalClientId = await CreateCentralServiceAccountClient(clientId, config.Name, config.IamClientAuthMethod);
        var serviceAccountUser = await _CentralIdp.GetUserForServiceAccountAsync(_Settings.CentralRealm, internalClientId).ConfigureAwait(false);
        if (serviceAccountUser == null)
        {
            throw new Exception($"error retrieving service account user for newly created service-account-client {internalClientId}");
        }
        await AssignClientRolesToCentralUserAsync(serviceAccountUser.Id, config.ClientRoles).ConfigureAwait(false);
        try
        {
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
