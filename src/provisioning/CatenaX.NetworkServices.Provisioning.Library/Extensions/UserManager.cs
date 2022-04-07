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
        private Task<string> CreateSharedRealmUserAsync(string realm, UserProfile profile)
        {
            var newUser = CloneUser(_Settings.SharedUser);
            newUser.UserName = profile.UserName;
            newUser.FirstName = profile.FirstName;
            newUser.LastName = profile.LastName;
            newUser.Email = profile.Email;
            newUser.Credentials ??= profile.Password == null ? null : Enumerable.Repeat( new Credentials { Type = "Password", Value = profile.Password }, 1);
            return _SharedIdp.CreateAndRetrieveUserIdAsync(realm, newUser);
        }

        private Task<string> CreateCentralUserAsync(string alias, UserProfile profile, string companyName)
        {
            var newUser = CloneUser(_Settings.CentralUser);
            newUser.UserName = profile.UserName;
            newUser.FirstName = profile.FirstName;
            newUser.LastName = profile.LastName;
            newUser.Email = profile.Email;
            newUser.Attributes ??= new Dictionary<string,IEnumerable<string>>();
            newUser.Attributes[_Settings.MappedIdpAttribute] = Enumerable.Repeat<string>(alias,1);
            newUser.Attributes[_Settings.MappedCompanyAttribute] = Enumerable.Repeat<string>(companyName,1);
            return _CentralIdp.CreateAndRetrieveUserIdAsync(_Settings.CentralRealm, newUser);
        }

        private Task<bool> LinkCentralSharedRealmUserAsync(string alias, string centralUserId, string sharedUserId, string sharedUserName) =>
            _CentralIdp.AddUserSocialLoginProviderAsync(_Settings.CentralRealm, centralUserId, alias, new FederatedIdentity {
                IdentityProvider = alias,
                UserId = sharedUserId,
                UserName = sharedUserName
            });

        private async Task<string> GetCentralUserIdForProviderIdAsync(string idpName, string providerUserId) =>
            (await _CentralIdp.GetUsersAsync(_Settings.CentralRealm, username: idpName + "." + providerUserId, max: 1, briefRepresentation: true).ConfigureAwait(false))
                .SingleOrDefault()
                ?.Id;

        private User CloneUser(User user) =>
            JsonSerializer.Deserialize<User>(JsonSerializer.Serialize(user));

        private Task<bool> DeleteSharedRealmUserAsync(string realm, string userId) =>
            _SharedIdp.DeleteUserAsync(realm, userId);

        private Task<bool> DeleteCentralRealmUserAsync(string realm, string userId) =>
            _CentralIdp.DeleteUserAsync(realm, userId);
    }
}
