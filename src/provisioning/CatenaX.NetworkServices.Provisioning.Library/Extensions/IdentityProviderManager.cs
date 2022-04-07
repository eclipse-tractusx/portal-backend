using Flurl;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Keycloak.Net.Models.IdentityProviders;
using Keycloak.Net.Models.OpenIDConfiguration;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningManager
    {
        public async Task<string> GetNextCentralIdentityProviderNameAsync() =>
            _Settings.IdpPrefix + (await _ProvisioningDBAccess.GetNextIdentityProviderSequenceAsync().ConfigureAwait(false)).sequence_id;

        private Task<bool> CreateCentralIdentityProviderAsync(string alias, string organisationName)
        {
            var newIdp = CloneIdentityProvider(_Settings.CentralIdentityProvider);
            newIdp.Alias = alias;
            newIdp.DisplayName = organisationName;
            return _CentralIdp.CreateIdentityProviderAsync(_Settings.CentralRealm, newIdp);
        }

        private async Task<bool> UpdateCentralIdentityProviderUrlsAsync(string alias, OpenIDConfiguration config)
        {
            var identityProvider = await _CentralIdp.GetIdentityProviderAsync(_Settings.CentralRealm, alias).ConfigureAwait(false);
            identityProvider.Config.AuthorizationUrl = config.AuthorizationEndpoint.ToString();
            identityProvider.Config.TokenUrl = config.TokenEndpoint.ToString();
            identityProvider.Config.LogoutUrl = config.EndSessionEndpoint.ToString();
            identityProvider.Config.JwksUrl = config.JwksUri.ToString();
            return await _CentralIdp.UpdateIdentityProviderAsync(_Settings.CentralRealm, alias, identityProvider).ConfigureAwait(false);
        }

        private async Task<IdentityProvider> SetIdentityProviderMetadataFromUrlAsync(IdentityProvider identityProvider, string url)
        {
            var metadata = await _CentralIdp.ImportIdentityProviderFromUrlAsync(_Settings.CentralRealm, url).ConfigureAwait(false);
            if (metadata == null || metadata.Count() == 0) return null;
            var changed = CloneIdentityProvider(identityProvider);
            changed.Config ??= new Config();
            foreach(var (key, value) in metadata)
            {
                switch(key)
                {
                    case "userInfoUrl":
                        changed.Config.UserInfoUrl = value as string;
                        break;
                    case "validateSignature":
                        changed.Config.ValidateSignature = value as string;
                        break;
                    case "tokenUrl":
                        changed.Config.TokenUrl = value as string;
                        break;
                    case "authorizationUrl":
                        changed.Config.AuthorizationUrl = value as string;
                        break;
                    case "jwksUrl":
                        changed.Config.JwksUrl = value as string;
                        break;
                    case "logoutUrl":
                        changed.Config.LogoutUrl = value as string;
                        break;
                    case "issuer":
                        changed.Config.Issuer = value as string;
                        break;
                    case "useJwksUrl":
                        changed.Config.UseJwksUrl = value as string;
                        break;
                }
            }
            return changed;
        }

        public Task<IdentityProvider> GetCentralIdentityProviderAsync(string alias) =>
            _CentralIdp.GetIdentityProviderAsync(_Settings.CentralRealm, alias);

        public Task<bool> UpdateCentralIdentityProviderAsync(string alias, IdentityProvider identityProvider) =>
            _CentralIdp.UpdateIdentityProviderAsync(_Settings.CentralRealm, alias, identityProvider);

        private async Task<bool> EnableCentralIdentityProviderAsync(string alias)
        {
            var identityProvider = await _CentralIdp.GetIdentityProviderAsync(_Settings.CentralRealm, alias).ConfigureAwait(false);
            identityProvider.Enabled = true;
            identityProvider.Config.HideOnLoginPage = "false";
            return await _CentralIdp.UpdateIdentityProviderAsync(_Settings.CentralRealm, alias, identityProvider).ConfigureAwait(false);
        }

        private async Task<string> GetCentralBrokerEndpointAsync(string alias)
        {
            return new Url ((await _CentralIdp.GetOpenIDConfigurationAsync(_Settings.CentralRealm).ConfigureAwait(false)).Issuer)
                .AppendPathSegment("/broker/")
                .AppendPathSegment(alias)
                .AppendPathSegment("/endpoint/*")
                .ToString();
        }

        private Task<bool> CreateCentralIdentityProviderTenantMapperAsync(string alias)
        {
            return _CentralIdp.AddIdentityProviderMapperAsync(_Settings.CentralRealm, alias, new IdentityProviderMapper
            {
                Name=_Settings.MappedIdpAttribute + "-mapper",
                _IdentityProviderMapper="hardcoded-attribute-idp-mapper",
                IdentityProviderAlias=alias,
                Config=new Dictionary<string,object>
                {
                    ["syncMode"]="INHERIT",
                    ["attribute"]=_Settings.MappedIdpAttribute,
                    ["attribute.value"]=alias
                }
            });
        }
        private Task<bool> CreateCentralIdentityProviderOrganisationMapperAsync(string alias, string organisationName)
        {
            return _CentralIdp.AddIdentityProviderMapperAsync(_Settings.CentralRealm, alias, new IdentityProviderMapper
            {
                Name=_Settings.MappedCompanyAttribute + "-mapper",
                _IdentityProviderMapper="hardcoded-attribute-idp-mapper",
                IdentityProviderAlias=alias,
                Config=new Dictionary<string,object>
                {
                    ["syncMode"]="INHERIT",
                    ["attribute"]=_Settings.MappedCompanyAttribute,
                    ["attribute.value"]=organisationName
                }
            });
        }

        private Task<bool> CreateCentralIdentityProviderUsernameMapperAsync(string alias)
        {
            return _CentralIdp.AddIdentityProviderMapperAsync(_Settings.CentralRealm, alias, new IdentityProviderMapper
            {
                Name="username-mapper",
                _IdentityProviderMapper="oidc-username-idp-mapper",
                IdentityProviderAlias=alias,
                Config=new Dictionary<string,object>
                {
                    ["syncMode"]="INHERIT",
                    ["target"]="LOCAL",
                    ["template"]=_Settings.UserNameMapperTemplate
                }
            });
        }

        public async Task<string> GetOrganisationFromCentralIdentityProviderMapperAsync(string alias)
        {
            var mapperName = _Settings.MappedCompanyAttribute + "-mapper";
            var mapper = (await _CentralIdp.GetIdentityProviderMappersAsync(_Settings.CentralRealm, alias).ConfigureAwait(false))
                .SingleOrDefault( x => x.Name.Equals(mapperName));
            return mapper == null ? null : mapper.Config["attribute.value"] as string;
        }

        private IdentityProvider CloneIdentityProvider(IdentityProvider identityProvider) =>
            JsonSerializer.Deserialize<IdentityProvider>(JsonSerializer.Serialize(identityProvider));
    }
}
