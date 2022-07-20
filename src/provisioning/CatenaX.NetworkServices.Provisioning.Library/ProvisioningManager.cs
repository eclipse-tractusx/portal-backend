using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Keycloak.Factory;
using CatenaX.NetworkServices.Provisioning.DBAccess;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using Keycloak.Net;
using Keycloak.Net.Models.Users;
using Microsoft.Extensions.Options;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningManager : IProvisioningManager
    {
        private readonly KeycloakClient _CentralIdp;
        private readonly KeycloakClient _SharedIdp;
        private readonly IProvisioningDBAccess? _ProvisioningDBAccess;
        private readonly ProvisioningSettings _Settings;

        public ProvisioningManager(IKeycloakFactory keycloakFactory, IProvisioningDBAccess? provisioningDBAccess, IOptions<ProvisioningSettings> options)
        {
            _CentralIdp = keycloakFactory.CreateKeycloakClient("central");
            _SharedIdp = keycloakFactory.CreateKeycloakClient("shared");
            _Settings = options.Value;
            _ProvisioningDBAccess = provisioningDBAccess;
        }

        public ProvisioningManager(IKeycloakFactory keycloakFactory, IOptions<ProvisioningSettings> options)
            : this(keycloakFactory, null, options)
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

        public async Task<string> CreateSharedUserLinkedToCentralAsync(string idpName, UserProfile userProfile)
        {
            var userIdShared = await CreateSharedRealmUserAsync(idpName, userProfile).ConfigureAwait(false);

            if (userIdShared == null)
            {
                throw new Exception($"failed to created user {userProfile.UserName} in shared realm {idpName}");
            }

            var userIdCentral = await CreateCentralUserAsync(idpName, new UserProfile(
                idpName + "." + userIdShared,
                userProfile.Email,
                userProfile.OrganisationName) {
                    FirstName = userProfile.FirstName,
                    LastName = userProfile.LastName,
                    BusinessPartnerNumber = userProfile.BusinessPartnerNumber
                }).ConfigureAwait(false);

            if (userIdCentral == null)
            {
                throw new Exception($"failed to created user {userProfile.UserName} in central realm for organization {userProfile.OrganisationName}");
            }

            await LinkCentralSharedRealmUserAsync(idpName, userIdCentral, userIdShared, userProfile.UserName).ConfigureAwait(false);

            return userIdCentral;
        }

        public async Task<IEnumerable<string>> GetClientRolesAsync(string clientId)
        {
            var idOfClient = await GetCentralInternalClientIdFromClientIDAsync(clientId).ConfigureAwait(false);
            return (await _CentralIdp.GetRolesAsync(_Settings.CentralRealm, idOfClient).ConfigureAwait(false))
                .Select(g => g.Name);
        }

        public async Task<IEnumerable<string>> GetClientRolesCompositeAsync(string clientId)
        {
            var idOfClient = await GetCentralInternalClientIdFromClientIDAsync(clientId).ConfigureAwait(false);
            return (await _CentralIdp.GetRolesAsync(_Settings.CentralRealm, idOfClient).ConfigureAwait(false))
                .Where(r => r.Composite == true)
                .Select(g => g.Name);
        }

        public async Task<string?> GetProviderUserIdForCentralUserIdAsync(string identityProvider, string userId) =>
            (await _CentralIdp.GetUserSocialLoginsAsync(_Settings.CentralRealm, userId).ConfigureAwait(false))
                .Where(federatedIdentity => federatedIdentity.IdentityProvider == identityProvider)
                .SingleOrDefault()?.UserId;

        public async Task<string> SetupClientAsync(string redirectUrl)
        {
            var clientId = await GetNextClientIdAsync().ConfigureAwait(false);
            var internalId = await CreateCentralOIDCClientAsync(clientId, redirectUrl).ConfigureAwait(false);
            await CreateCentralOIDCClientAudienceMapperAsync(internalId, clientId).ConfigureAwait(false);
            return clientId;
        }

        public async Task AddBpnAttributetoUserAsync(string userId, IEnumerable<string> bpns)
        {
            User user;
            try
            {
                user = await _CentralIdp.GetUserAsync(_Settings.CentralRealm, userId).ConfigureAwait(false);
                if (user == null)
                {
                    throw new Exception($"failed to retrieve central user {userId}");
                }
            }
            catch (EntityNotFoundException ex)
            {
                throw ex;
            }
            
            user.Attributes ??= new Dictionary<string, IEnumerable<string>>();
            user.Attributes[_Settings.MappedBpnAttribute] = (user.Attributes.TryGetValue(_Settings.MappedBpnAttribute, out var existingBpns))
                ? existingBpns.Concat(bpns).Distinct()
                : bpns;
            if (!await _CentralIdp.UpdateUserAsync(_Settings.CentralRealm, userId.ToString(), user).ConfigureAwait(false))
            {
                throw new Exception($"failed to set bpns {bpns} for central user {userId}");
            }
        }

        public async Task<bool> ResetSharedUserPasswordAsync(string realm, string userId)
        {
            string providerUserId = string.Empty;
            try
            {
                providerUserId = await GetProviderUserIdForCentralUserIdAsync(realm, userId).ConfigureAwait(false);
                if (providerUserId == null)
                {
                    throw new ArgumentOutOfRangeException($"userId {userId} is not linked to shared realm {realm}");
                }
            }
            catch (EntityNotFoundException ex)
            {
               throw ex;
            }

            return await _SharedIdp.SendUserUpdateAccountEmailAsync(realm, providerUserId, Enumerable.Repeat("UPDATE_PASSWORD", 1)).ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> GetClientRoleMappingsForUserAsync(string userId, string clientId)
        {
            var idOfClient = await GetCentralInternalClientIdFromClientIDAsync(clientId).ConfigureAwait(false);
            return (await _CentralIdp.GetClientRoleMappingsForUserAsync(_Settings.CentralRealm, userId, idOfClient).ConfigureAwait(false))
                .Where(r => r.Composite == true).Select(x => x.Name);
        }
    }
}
