using CatenaX.NetworkServices.Framework.ErrorHandling;
using Keycloak.Net.Models.Roles;
using System.Collections;
using Keycloak.Net.Models.Clients;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningManager
    {
        private async Task<(string, IEnumerable<Role>)> GetCentralClientIdRolesAsync(string clientName, IEnumerable<string> roleNames)
        {
            var count = roleNames.Count();
            Client client = null;
            try
            {
            IEnumerable<Role> roles;
            client = await GetCentralClientViewableAsync(clientName).ConfigureAwait(false);
            switch (count)
            {
                case 0:
                    return (client.Id, Enumerable.Empty<Role>());
                case 1:
                    return (client.Id, Enumerable.Repeat<Role>(await _CentralIdp.GetRoleByNameAsync(_Settings.CentralRealm, client.Id, roleNames.Single()).ConfigureAwait(false), 1));
                default:
                     roles = (await _CentralIdp.GetRolesAsync(_Settings.CentralRealm, client.Id).ConfigureAwait(false)).Where(x => roleNames.Contains(x.Name));
                     return (client.Id, roles);
            }
            
            }
            catch(Exception ex)
            {
                if (ex is NotFoundException)
                {
                    if (client == null)
                    {
                        return (null, Enumerable.Empty<Role>());
                    }
                    return (client.Id, Enumerable.Empty<Role>());
                }
            }
            return (null, Enumerable.Empty<Role>());
        }

        public async Task<IDictionary<string, IEnumerable<string>>[]> AssignClientRolesToCentralUserAsync(string centralUserId, IDictionary<string, IEnumerable<string>> clientRoleNames)
        {
            var clientRoles = await Task.WhenAll(clientRoleNames.Select(async x =>
                {
                    var (client, roleNames) = x;
                    var (clientId, roles) = await GetCentralClientIdRolesAsync(client, roleNames).ConfigureAwait(false);
                    await _CentralIdp.AddClientRoleMappingsToUserAsync(_Settings.CentralRealm, centralUserId, clientId, roles).ConfigureAwait(false);
                    IEnumerable<string> rolesList = roleNames.Except(roles.Select(role => role.Name));
                    return  (client:client, rolesList:rolesList);
                    
                }
            )).ConfigureAwait(false);
            var assignedClientRoles = new Dictionary<string, IEnumerable<string>>[] 
            {
              clientRoles.ToDictionary(clientRole => clientRole.client, clientRole => clientRole.rolesList)
            };
            return assignedClientRoles;
        }
    }
}
