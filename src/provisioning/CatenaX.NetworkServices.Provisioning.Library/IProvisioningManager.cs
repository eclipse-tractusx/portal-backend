using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatenaX.NetworkServices.Provisioning.Library.Models;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public interface IProvisioningManager
    {
        Task<string> GetNextCentralIdentityProviderNameAsync();
        Task<bool> SetupSharedIdpAsync(string idpName, string organisationName);
        Task<string> CreateSharedUserLinkedToCentralAsync(string idpName, UserProfile userProfile, string companyName);
        Task<bool> AssignInvitedUserInitialRoles(string centralUserId);
        Task<bool> AssignClientRolesToCentralUserAsync(string centralUserId, IDictionary<string,IEnumerable<string>> clientRoleNames);
        Task<IEnumerable<string>> GetClientRolesAsync(string clientId);
        Task<IEnumerable<string>> GetClientRolesCompositeAsync(string clientId);
        Task<string> GetOrganisationFromCentralIdentityProviderMapperAsync(string alias);
        Task<string> SetupOwnIdpAsync(string organisationName, string clientId, string metadataUrl, string clientAuthMethod, string clientSecret);
        Task<string> GetProviderUserIdForCentralUserIdAsync(string userId);
        Task<bool> DeleteSharedAndCentralUserAsync(string idpName, string userId);
        Task<IEnumerable<JoinedUserInfo>> GetJoinedUsersAsync(
            string idpName,
            string userId = null,
            string providerUserId = null,
            string userName = null,
            string firstName = null,
            string lastName = null,
            string email = null);
        Task<string> SetupClientAsync(string redirectUrl);
        Task<bool> AddBpnAttributetoUserAsync(Guid centralUserId, IEnumerable<string> bpns);
        Task<bool> ResetUserPasswordAsync(string realm, string userId, IEnumerable<string> requiredActions);
    }
}
