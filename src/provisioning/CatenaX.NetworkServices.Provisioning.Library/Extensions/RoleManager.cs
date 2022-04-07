using Keycloak.Net.Models.Roles;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningManager
    {
        private async Task<(string,IEnumerable<Role>)> GetCentralClientIdRolesAsync(string clientName, IEnumerable<string> roleNames)
        {
            var count = roleNames.Count();
            var client = await GetCentralClientViewableAsync(clientName).ConfigureAwait(false);
            if (client == null) return (null,null);
            switch (count)
            {
                case 0:
                    return (client.Id, Enumerable.Empty<Role>());
                case 1:
                    return (client.Id, Enumerable.Repeat<Role>(await _CentralIdp.GetRoleByNameAsync(_Settings.CentralRealm, client.Id, roleNames.Single()).ConfigureAwait(false), 1));
                default:
                {
                    return (client.Id, (await _CentralIdp.GetRolesAsync(_Settings.CentralRealm, client.Id).ConfigureAwait(false))
                        .Where( x => roleNames.Contains(x.Name)));
                }
            }
        }

        public async Task<bool> AssignClientRolesToCentralUserAsync(string centralUserId, IDictionary<string,IEnumerable<string>> clientRoleNames)
        {
            return (await Task.WhenAll(clientRoleNames.Select( async x => 
                {
                    var (client, roleNames) = x;
                    var (clientId, roles) = await GetCentralClientIdRolesAsync(client, roleNames).ConfigureAwait(false);
                    return await _CentralIdp.AddClientRoleMappingsToUserAsync(_Settings.CentralRealm, centralUserId, clientId, roles).ConfigureAwait(false);
                }
            )).ConfigureAwait(false)).All( x => x );
        }
    }
}
