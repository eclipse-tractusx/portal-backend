using CatenaX.NetworkServices.Framework.ErrorHandling;
using Keycloak.Net.Models.Roles;

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
                    if (roles.Count() != count)
                    {
                        throw new NotFoundException($"invalid roles for client {clientName}: [{String.Join(",",roleNames.Except(roles.Select(role => role.Name)))}]");
                    }
                    return (client.Id, roles);
            }
        }

        public async Task AssignClientRolesToCentralUserAsync(string centralUserId, IDictionary<string, IEnumerable<string>> clientRoleNames)
        {
            if (!(await Task.WhenAll(clientRoleNames.Select(async x =>
                {
                    var (client, roleNames) = x;
                    var (clientId, roles) = await GetCentralClientIdRolesAsync(client, roleNames).ConfigureAwait(false);
                    return await _CentralIdp.AddClientRoleMappingsToUserAsync(_Settings.CentralRealm, centralUserId, clientId, roles).ConfigureAwait(false);
                }
            )).ConfigureAwait(false)).All(x => x))
            {
                throw new Exception($"failed to assign client-roles {clientRoleNames} to central user {centralUserId}");
            }
        }
    }
}
