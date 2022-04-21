using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Keycloak.Net.Models.Users;
using CatenaX.NetworkServices.Provisioning.Library.Models;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningManager
    {
        private async Task<string> CreateSharedRealmUserAsync(string realm, UserProfile profile)
        {
            var newUser = CloneUser(_Settings.SharedUser);
            newUser.UserName = profile.UserName;
            newUser.FirstName = profile.FirstName;
            newUser.LastName = profile.LastName;
            newUser.Email = profile.Email;
            newUser.Credentials ??= profile.Password == null ? null : Enumerable.Repeat( new Credentials { Type = "Password", Value = profile.Password }, 1);
            var newUserId = await _SharedIdp.CreateAndRetrieveUserIdAsync(realm, newUser).ConfigureAwait(false);
            if (newUserId == null)
            {
                throw new Exception($"failed to created shared user {profile.UserName} in realm {realm}");
            }
            return newUserId;
        }

        private async Task<string> CreateCentralUserAsync(string alias, UserProfile profile, string companyName)
        {
            var newUser = CloneUser(_Settings.CentralUser);
            newUser.UserName = profile.UserName;
            newUser.FirstName = profile.FirstName;
            newUser.LastName = profile.LastName;
            newUser.Email = profile.Email;
            newUser.Attributes ??= new Dictionary<string,IEnumerable<string>>();
            newUser.Attributes[_Settings.MappedIdpAttribute] = Enumerable.Repeat<string>(alias,1);
            newUser.Attributes[_Settings.MappedCompanyAttribute] = Enumerable.Repeat<string>(companyName,1);
            var newUserId = await _CentralIdp.CreateAndRetrieveUserIdAsync(_Settings.CentralRealm, newUser).ConfigureAwait(false);
            if (newUserId == null)
            {
                throw new Exception($"failed to created central user {profile.UserName} for identityprovider {alias}, organisation {companyName}");
            }
            return newUserId;
        }

        private async Task LinkCentralSharedRealmUserAsync(string alias, string centralUserId, string sharedUserId, string sharedUserName)
        {
            if (! await _CentralIdp.AddUserSocialLoginProviderAsync(_Settings.CentralRealm, centralUserId, alias, new FederatedIdentity {
                IdentityProvider = alias,
                UserId = sharedUserId,
                UserName = sharedUserName
            }).ConfigureAwait(false))
            {
                throw new Exception($"failed to create link in between central user {centralUserId} and shared realm {alias} user {sharedUserId}");
            }
        }

        private async Task<string> GetCentralUserIdForProviderIdAsync(string idpName, string providerUserId)
        {
            var centralUserId = (await _CentralIdp.GetUsersAsync(_Settings.CentralRealm, username: idpName + "." + providerUserId, max: 1, briefRepresentation: true).ConfigureAwait(false))
                .SingleOrDefault()
                ?.Id;
            if (centralUserId == null)
            {
                throw new Exception($"failed to retrieve central userid for identityprovider {idpName} user {providerUserId}");
            }
            return centralUserId;
        }

        private User CloneUser(User user) =>
            JsonSerializer.Deserialize<User>(JsonSerializer.Serialize(user));

        private async Task DeleteSharedRealmUserAsync(string realm, string userId)
        {
            if (! await _SharedIdp.DeleteUserAsync(realm, userId).ConfigureAwait(false))
            {
                throw new Exception($"failed to delete shared realm {realm} user {userId}");
            }
        }

        private async Task DeleteCentralRealmUserAsync(string realm, string userId)
        {
            if (! await _CentralIdp.DeleteUserAsync(realm, userId))
            {
                throw new Exception($"failed to delete central realm {realm} user {userId}");
            }
        }
    }
}
