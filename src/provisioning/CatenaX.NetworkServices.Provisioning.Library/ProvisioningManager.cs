using Keycloak.Net;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatenaX.NetworkServices.Keycloak.Factory;
using CatenaX.NetworkServices.Provisioning.DBAccess;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Keycloak.DBAccess;
using System;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningManager : IProvisioningManager
    {
        private readonly KeycloakClient _CentralIdp;
        private readonly KeycloakClient _SharedIdp;
        private readonly IKeycloakDBAccess? _KeycloakDBAccess;
        private readonly IProvisioningDBAccess? _ProvisioningDBAccess;
        private readonly ProvisioningSettings _Settings;

        public ProvisioningManager(IKeycloakFactory keycloakFactory, IKeycloakDBAccess? keycloakDBAccess, IProvisioningDBAccess? provisioningDBAccess, IOptions<ProvisioningSettings> options)
        {
            _CentralIdp = keycloakFactory.CreateKeycloakClient("central");
            _SharedIdp = keycloakFactory.CreateKeycloakClient("shared");
            _Settings = options.Value;
            _KeycloakDBAccess = keycloakDBAccess;
            _ProvisioningDBAccess = provisioningDBAccess;
        }

        public ProvisioningManager(IKeycloakFactory keycloakFactory, IKeycloakDBAccess keycloakDBAccess, IOptions<ProvisioningSettings> options)
            : this(keycloakFactory, keycloakDBAccess, null, options)
        {
        }

        public ProvisioningManager(IKeycloakFactory keycloakFactory, IProvisioningDBAccess provisioningDBAccess, IOptions<ProvisioningSettings> options)
            : this(keycloakFactory, null, provisioningDBAccess, options)
        {
        }

        public ProvisioningManager(IKeycloakFactory keycloakFactory, IOptions<ProvisioningSettings> options)
            : this(keycloakFactory, null, null, options)
        {
        }

        public async Task SetupSharedIdpAsync(string idpName, string organisationName)
        {
            await CreateCentralIdentityProviderAsync(idpName, organisationName).ConfigureAwait(false);

            await CreateSharedRealmAsync(idpName, organisationName).ConfigureAwait(false);

            await UpdateCentralIdentityProviderUrlsAsync(idpName, await GetSharedRealmOpenIDConfigAsync(idpName).ConfigureAwait(false)).ConfigureAwait(false);

            await CreateCentralIdentityProviderTenantMapperAsync(idpName).ConfigureAwait(false);

            await CreateCentralIdentityProviderOrganisationMapperAsync(idpName, organisationName).ConfigureAwait(false);

            await CreateCentralIdentityProviderUsernameMapperAsync(idpName).ConfigureAwait(false);

            await CreateSharedRealmIdentityProviderClientAsync(idpName, new IdentityProviderClientConfig(
                await GetCentralBrokerEndpointAsync(idpName).ConfigureAwait(false),
                await GetCentralRealmJwksUrlAsync().ConfigureAwait(false)
            )).ConfigureAwait(false);

            await EnableCentralIdentityProviderAsync(idpName).ConfigureAwait(false);
        }

        public async Task<string> SetupOwnIdpAsync(string organisationName, string clientId, string metadataUrl, string clientAuthMethod, string? clientSecret)
        {
            var idpName = await GetNextCentralIdentityProviderNameAsync().ConfigureAwait(false);

            await CreateCentralIdentityProviderAsync(idpName, organisationName).ConfigureAwait(false);

            var identityProvider = await SetIdentityProviderMetadataFromUrlAsync(await GetCentralIdentityProviderAsync(idpName).ConfigureAwait(false), metadataUrl).ConfigureAwait(false);

            identityProvider.Config.ClientId = clientId;
            identityProvider.Config.ClientAuthMethod = clientAuthMethod;
            identityProvider.Config.ClientSecret = clientSecret;

            await UpdateCentralIdentityProviderAsync(idpName, identityProvider).ConfigureAwait(false);

            await CreateCentralIdentityProviderTenantMapperAsync(idpName).ConfigureAwait(false);

            await CreateCentralIdentityProviderOrganisationMapperAsync(idpName, organisationName).ConfigureAwait(false);

            await CreateCentralIdentityProviderUsernameMapperAsync(idpName).ConfigureAwait(false);

            await EnableCentralIdentityProviderAsync(idpName).ConfigureAwait(false);

            return idpName;
        }

        public async Task<string> CreateSharedUserLinkedToCentralAsync(string idpName, UserProfile userProfile, string organisationName)
        {
            var userIdShared = await CreateSharedRealmUserAsync(idpName, userProfile).ConfigureAwait(false);

            if (userIdShared == null)
            {
                throw new Exception($"failed to created user {userProfile.UserName} in shared realm {idpName}");
            }

            var userIdCentral = await CreateCentralUserAsync(idpName, new UserProfile(
                idpName + "." + userIdShared,
                userProfile.FirstName,
                userProfile.LastName,
                userProfile.Email), organisationName).ConfigureAwait(false);

            if (userIdCentral == null)
            {
                throw new Exception($"failed to created user {userProfile.UserName} in central realm for organization {organisationName}");
            }

            await LinkCentralSharedRealmUserAsync(idpName, userIdCentral, userIdShared, userProfile.UserName).ConfigureAwait(false);

            return userIdCentral;
        }

        public Task AssignInvitedUserInitialRoles(string centralUserId) =>
            AssignClientRolesToCentralUserAsync(centralUserId, _Settings.InvitedUserInitialRoles);

        public async Task<IEnumerable<string>> GetClientRolesAsync(string clientId)
        {
            var idOfClient = await GetIdOfClientFromClientIDAsync(clientId).ConfigureAwait(false);
            return (await _CentralIdp.GetRolesAsync(_Settings.CentralRealm, idOfClient).ConfigureAwait(false))
                .Select(g => g.Name);
        }

        public async Task<IEnumerable<string>> GetClientRolesCompositeAsync(string clientId)
        {
            var idOfClient = await GetIdOfClientFromClientIDAsync(clientId).ConfigureAwait(false);
            return (await _CentralIdp.GetRolesAsync(_Settings.CentralRealm, idOfClient).ConfigureAwait(false))
                .Where(r => r.Composite == true)
                .Select(g => g.Name);
        }

        public async Task<string> GetProviderUserIdForCentralUserIdAsync(string userId)
        {
            var providerUserid = (await _CentralIdp.GetUserSocialLoginsAsync(_Settings.CentralRealm, userId).ConfigureAwait(false))
                .SingleOrDefault()?.UserId;
            if (providerUserid == null)
            {
                throw new Exception($"failed to retrieve provider userid for {userId}");
            }
            return providerUserid;
        }

        public async Task DeleteSharedAndCentralUserAsync(string idpName, string userIdShared)
        {
            var userIdCentral = await GetCentralUserIdForProviderIdAsync(idpName, userIdShared).ConfigureAwait(false);

            await DeleteSharedRealmUserAsync(idpName, userIdShared).ConfigureAwait(false);

            await DeleteCentralRealmUserAsync(_Settings.CentralRealm, userIdCentral).ConfigureAwait(false);
        }

        public async Task<IEnumerable<JoinedUserInfo>> GetJoinedUsersAsync(string idpName,
                                                               string? userId = null,
                                                               string? providerUserId = null,
                                                               string? userName = null,
                                                               string? firstName = null,
                                                               string? lastName = null,
                                                               string? email = null)
        {
            return (await _KeycloakDBAccess!.GetUserJoinedFederatedIdentityAsync(idpName,
                                                                 _Settings.CentralRealmId,
                                                                 userId,
                                                                 providerUserId,
                                                                 userName,
                                                                 firstName,
                                                                 lastName,
                                                                 email))
                .Select(x => new JoinedUserInfo
                {
                    userId = x.id,
                    providerUserId = x.federated_user_id,
                    userName = x.federated_username,
                    enabled = x.enabled,
                    firstName = x.first_name,
                    lastName = x.last_name,
                    email = x.email
                });
        }

        public async Task<string> SetupClientAsync(string redirectUrl)
        {
            var clientId = await GetNextClientIdAsync().ConfigureAwait(false);
            var internalId = (await CreateCentralOIDCClientAsync(clientId, redirectUrl).ConfigureAwait(false));
            await CreateCentralOIDCClientAudienceMapperAsync(internalId, clientId).ConfigureAwait(false);
            return clientId;
        }

        public async Task AddBpnAttributetoUserAsync(string userId, IEnumerable<string> bpns)
        {
            var user = await _CentralIdp.GetUserAsync(_Settings.CentralRealm, userId).ConfigureAwait(false);
            if (user == null)
            {
                throw new Exception($"failed to retrieve central user {userId}");
            }
            user.Attributes ??= new Dictionary<string, IEnumerable<string>>();
            user.Attributes["bpn"] = (user.Attributes.TryGetValue("bpn", out var existingBpns))
                ? existingBpns.Concat(bpns).Distinct()
                : bpns;
            if (!await _CentralIdp.UpdateUserAsync(_Settings.CentralRealm, userId.ToString(), user).ConfigureAwait(false))
            {
                throw new Exception($"failed to set bpns {bpns} for central user {userId}");
            }
        }

        public async Task<bool> ResetUserPasswordAsync(string realm, string userId, IEnumerable<string> requiredActions)
        {
            var providerUserId = await GetProviderUserIdForCentralUserIdAsync(userId).ConfigureAwait(false);
            return await _SharedIdp.SendUserUpdateAccountEmailAsync(realm, providerUserId, requiredActions).ConfigureAwait(false);

        }

        public async Task<IEnumerable<string>> GetClientRoleMappingsForUserAsync(string userId, string clientId)
        {
            var idOfClient = await GetIdOfClientFromClientIDAsync(clientId).ConfigureAwait(false);
            return (await _CentralIdp.GetClientRoleMappingsForUserAsync(_Settings.CentralRealm, userId, idOfClient).ConfigureAwait(false))
                .Where(r => r.Composite == true).Select(x => x.Name);
        }
    }
}
