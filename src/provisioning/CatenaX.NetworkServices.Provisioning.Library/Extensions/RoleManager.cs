using CatenaX.NetworkServices.Framework.ErrorHandling;
using Keycloak.Net.Models.Roles;
using System.Collections;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningManager
    {
        private async Task<(string, IEnumerable<Role>)> GetCentralClientIdRolesAsync(string clientName, IEnumerable<string> roleNames)
        {
            var count = roleNames.Count();
            var client = await GetCentralClientViewableAsync(clientName).ConfigureAwait(false);
            switch (count)
            {
                case 0:
                    return (client.Id, Enumerable.Empty<Role>());
                case 1:
                    return (client.Id, Enumerable.Repeat<Role>(await _CentralIdp.GetRoleByNameAsync(_Settings.CentralRealm, client.Id, roleNames.Single()).ConfigureAwait(false), 1));
                default:
                    var roles = (await _CentralIdp.GetRolesAsync(_Settings.CentralRealm, client.Id).ConfigureAwait(false)).Where(x => roleNames.Contains(x.Name));
                    return (client.Id, roles);
            }
        }

        public async Task<IEnumerable<string>> AssignClientRolesToCentralUserAsync(string centralUserId, IDictionary<string, IEnumerable<string>> clientRoleNames)
        {
            var results = await Task.WhenAll(clientRoleNames.Select(async x =>
                {
                    var (client, roleNames) = x;
                    var (clientId, roles) = await GetCentralClientIdRolesAsync(client, roleNames).ConfigureAwait(false);
                    await _CentralIdp.AddClientRoleMappingsToUserAsync(_Settings.CentralRealm, centralUserId, clientId, roles).ConfigureAwait(false);
                    return roles;
                    
                }
            )).ConfigureAwait(false);
            return results.Select(role => role.FirstOrDefault().Name);
        }
    }
}
