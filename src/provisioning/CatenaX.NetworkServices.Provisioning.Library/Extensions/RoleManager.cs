using CatenaX.NetworkServices.Framework.ErrorHandling;
using Keycloak.Net.Models.Roles;
using Keycloak.Net.Models.Clients;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningManager
    {
        private async Task<(string?, IEnumerable<Role>)> GetCentralClientIdRolesAsync(string clientName, IEnumerable<string> roleNames)
        {
            var count = roleNames.Count();
            Client? client = null;
            try
            {
                client = await GetCentralClientViewableAsync(clientName).ConfigureAwait(false);
                switch (count)
                {
                    case 0:
                        return (client.Id, Enumerable.Empty<Role>());
                    case 1:
                        return (client.Id, Enumerable.Repeat<Role>(await _CentralIdp.GetRoleByNameAsync(_Settings.CentralRealm, client.Id, roleNames.Single()).ConfigureAwait(false), 1));
                    default:
                        return (client.Id, (await _CentralIdp.GetRolesAsync(_Settings.CentralRealm, client.Id).ConfigureAwait(false)).Where(x => roleNames.Contains(x.Name)));
                }
            }
            catch(NotFoundException)
            {
                return (client?.Id, Enumerable.Empty<Role>());
            }
        }

        public async Task<IDictionary<string, IEnumerable<string>>> AssignClientRolesToCentralUserAsync(string centralUserId, IDictionary<string, IEnumerable<string>> clientRoleNames) =>
            (await Task.WhenAll(clientRoleNames.Select(async x =>
                {
                    var (client, roleNames) = x;
                    var (clientId, roles) = await GetCentralClientIdRolesAsync(client, roleNames).ConfigureAwait(false);
                    if (clientId != null && roles.Count() > 0 &&
                        await _CentralIdp.AddClientRoleMappingsToUserAsync(_Settings.CentralRealm, centralUserId, clientId, roles).ConfigureAwait(false))
                    {
                        return (client: client, rolesList: roles.Select(role => role.Name));
                    }
                    return (client: client, rolesList: Enumerable.Empty<string>());
                }
            )).ConfigureAwait(false))
            .ToDictionary(clientRole => clientRole.client, clientRole => clientRole.rolesList);
    }
}
