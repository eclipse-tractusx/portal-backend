using CatenaX.NetworkServices.Keycloak.ErrorHandling;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using Keycloak.Net.Models.Users;
using System.Text.Json;

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

        private async Task<string> CreateCentralUserAsync(string alias, UserProfile profile)
        {
            var newUser = CloneUser(_Settings.CentralUser);
            newUser.UserName = profile.UserName;
            newUser.FirstName = profile.FirstName;
            newUser.LastName = profile.LastName;
            newUser.Email = profile.Email;
            newUser.Attributes ??= new Dictionary<string,IEnumerable<string>>();
            newUser.Attributes[_Settings.MappedIdpAttribute] = Enumerable.Repeat<string>(alias,1);
            newUser.Attributes[_Settings.MappedCompanyAttribute] = Enumerable.Repeat<string>(profile.OrganisationName,1);
            if (!String.IsNullOrWhiteSpace(profile.BusinessPartnerNumber))
            {
                newUser.Attributes[_Settings.MappedBpnAttribute] = Enumerable.Repeat<string>(profile.BusinessPartnerNumber,1);
            }
            var newUserId = await _CentralIdp.CreateAndRetrieveUserIdAsync(_Settings.CentralRealm, newUser).ConfigureAwait(false);
            if (newUserId == null)
            {
                throw new Exception($"failed to created central user {profile.UserName} for identityprovider {alias}, organisation {profile.OrganisationName}");
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

        public async Task<bool> UpdateSharedRealmUserAsync(string realm, string userId, string firstName, string lastName, string email)
        {
            var user = await _SharedIdp.GetUserAsync(realm, userId).ConfigureAwait(false);
            if (user == null)
            {
                throw new KeycloakEntityNotFoundException($"userId {userId} not found in shared realm {realm}");
            }
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;
            return await _SharedIdp.UpdateUserAsync(realm, userId, user).ConfigureAwait(false);
        }

        public Task<bool> DeleteSharedRealmUserAsync(string realm, string userId) =>
            _SharedIdp.DeleteUserAsync(realm, userId);

        public Task<bool> DeleteCentralRealmUserAsync(string userId) =>
            _CentralIdp.DeleteUserAsync(_Settings.CentralRealm, userId);

        private User CloneUser(User user) =>
            JsonSerializer.Deserialize<User>(JsonSerializer.Serialize(user))!;
    }
}
