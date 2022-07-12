using CatenaX.NetworkServices.Provisioning.Library.Enums;
using Flurl;
using System.Text.Json;
using Keycloak.Net.Models.IdentityProviders;
using Keycloak.Net.Models.OpenIDConfiguration;

namespace CatenaX.NetworkServices.Provisioning.Library;

public partial class ProvisioningManager
{
    private static readonly IReadOnlyDictionary<string,IamIdentityProviderClientAuthMethod> IdentityProviderClientAuthTypesIamClientAuthMethodDictionary = new Dictionary<string,IamIdentityProviderClientAuthMethod>()
    {
        { "private_key_jwt", IamIdentityProviderClientAuthMethod.JWT },
        { "client_secret_post", IamIdentityProviderClientAuthMethod.SECRET_POST },
        { "client_secret_basic", IamIdentityProviderClientAuthMethod.SECRET_BASIC },
        { "client_secret_jwt", IamIdentityProviderClientAuthMethod.SECRET_JWT }
    };

    private static readonly IReadOnlyDictionary<IamIdentityProviderClientAuthMethod,string> IamIdentityProviderClientAuthMethodsInternalDictionary = new Dictionary<IamIdentityProviderClientAuthMethod,string>()
    {
        { IamIdentityProviderClientAuthMethod.JWT, "private_key_jwt" },
        { IamIdentityProviderClientAuthMethod.SECRET_POST, "client_secret_post" },
        { IamIdentityProviderClientAuthMethod.SECRET_BASIC, "client_secret_basic" },
        { IamIdentityProviderClientAuthMethod.SECRET_JWT, "client_secret_jwt" }
    };

    public async Task<string> GetNextCentralIdentityProviderNameAsync() =>
        _Settings.IdpPrefix + (await _ProvisioningDBAccess!.GetNextIdentityProviderSequenceAsync().ConfigureAwait(false));

    private async Task CreateCentralIdentityProviderAsync(string alias, string organisationName, IdentityProvider identityProvider)
    {
        var newIdp = CloneIdentityProvider(identityProvider);
        newIdp.Alias = alias;
        newIdp.DisplayName = organisationName;
        if (!await _CentralIdp.CreateIdentityProviderAsync(_Settings.CentralRealm, newIdp).ConfigureAwait(false))
        {
            throw new Exception($"failed to set up central identityprovider {alias} for {organisationName}");
        }
    }

    private async Task UpdateCentralIdentityProviderUrlsAsync(string alias, OpenIDConfiguration config)
    {
        var identityProvider = await _CentralIdp.GetIdentityProviderAsync(_Settings.CentralRealm, alias).ConfigureAwait(false);
        if (identityProvider == null)
        {
            throw new Exception($"failed to retrieve central identityprovider {alias}");
        }
        identityProvider.Config.AuthorizationUrl = config.AuthorizationEndpoint.ToString();
        identityProvider.Config.TokenUrl = config.TokenEndpoint.ToString();
        identityProvider.Config.LogoutUrl = config.EndSessionEndpoint.ToString();
        identityProvider.Config.JwksUrl = config.JwksUri.ToString();
        if (! await _CentralIdp.UpdateIdentityProviderAsync(_Settings.CentralRealm, alias, identityProvider).ConfigureAwait(false))
        {
            throw new Exception($"failed to update central identityprovider {alias}");
        }
    }

    private async Task<IdentityProvider> SetIdentityProviderMetadataFromUrlAsync(IdentityProvider identityProvider, string url)
    {
        var metadata = await _CentralIdp.ImportIdentityProviderFromUrlAsync(_Settings.CentralRealm, url).ConfigureAwait(false);
        if (metadata == null || metadata.Count() == 0)
        {
            throw new Exception("{url} did return no metadata");
        }
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

    public async Task<IdentityProvider> GetCentralIdentityProviderAsync(string alias)
    {
        var identityprovider = await _CentralIdp.GetIdentityProviderAsync(_Settings.CentralRealm, alias).ConfigureAwait(false);
        if (identityprovider == null)
        {
            throw new Exception($"failed to retrieve central identityprovider {alias}");
        }
        return identityprovider;
    }

    public async Task UpdateCentralIdentityProviderAsync(string alias, IdentityProvider identityProvider)
    {
        if (! await _CentralIdp.UpdateIdentityProviderAsync(_Settings.CentralRealm, alias, identityProvider).ConfigureAwait(false))
        {
            throw new Exception($"failed to update config of central identityprovider {alias}");
        }
    }

    private async Task EnableCentralIdentityProviderAsync(string alias)
    {
        var identityProvider = await _CentralIdp.GetIdentityProviderAsync(_Settings.CentralRealm, alias).ConfigureAwait(false);
        if (identityProvider != null)
        {
            identityProvider.Enabled = true;
            identityProvider.Config.HideOnLoginPage = "false";
            if (await _CentralIdp.UpdateIdentityProviderAsync(_Settings.CentralRealm, alias, identityProvider).ConfigureAwait(false)) return;
        }
        throw new Exception($"failed to enable central identityprovider {alias}");
    }

    private async Task<string> GetCentralBrokerEndpointOIDCAsync(string alias)
    {
        var openidconfig = await _CentralIdp.GetOpenIDConfigurationAsync(_Settings.CentralRealm).ConfigureAwait(false);
        if (openidconfig == null)
        {
            throw new Exception($"failed to retrieve central openidconfig");
        }
        return new Url(openidconfig.Issuer)
            .AppendPathSegment("/broker/")
            .AppendPathSegment(alias)
            .AppendPathSegment("/endpoint")
            .ToString();
    }

    private async Task<string> GetCentralBrokerEndpointSAMLAsync(string alias)
    {
        var samlDescriptor = await _CentralIdp.GetSAMLMetaDataAsync(_Settings.CentralRealm).ConfigureAwait(false);
        if (samlDescriptor == null)
        {
            throw new Exception($"failed to retrieve central samldescriptor");
        }
        return new Url(samlDescriptor.EntityId)
            .AppendPathSegment("/broker/")
            .AppendPathSegment(alias)
            .AppendPathSegment("/endpoint")
            .ToString();
    }

    private async Task CreateCentralIdentityProviderTenantMapperAsync(string alias)
    {
        if (! await _CentralIdp.AddIdentityProviderMapperAsync(_Settings.CentralRealm, alias, new IdentityProviderMapper
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
        }).ConfigureAwait(false))
        {
            throw new Exception($"failed to create tenant-mapper for identityprovider {alias}");
        }
    }
    private async Task CreateCentralIdentityProviderOrganisationMapperAsync(string alias, string organisationName)
    {
        if (! await _CentralIdp.AddIdentityProviderMapperAsync(_Settings.CentralRealm, alias, new IdentityProviderMapper
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
        }).ConfigureAwait(false))
        {
            throw new Exception($"failed to create organisation-mapper for identityprovider {alias}, organisation {organisationName}");
        }
    }

    private async Task CreateCentralIdentityProviderUsernameMapperAsync(string alias)
    {
        if (! await _CentralIdp.AddIdentityProviderMapperAsync(_Settings.CentralRealm, alias, new IdentityProviderMapper
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
        }).ConfigureAwait(false))
        {
            throw new Exception($"failed to create username-mapper for identityprovider {alias}");
        }
    }

    public async Task<string> GetOrganisationFromCentralIdentityProviderMapperAsync(string alias)
    {
        var mapperName = _Settings.MappedCompanyAttribute + "-mapper";
        var mapper = (await _CentralIdp.GetIdentityProviderMappersAsync(_Settings.CentralRealm, alias).ConfigureAwait(false))
            .SingleOrDefault( x => x.Name.Equals(mapperName));
        var organisation = mapper?.Config["attribute.value"] as string;
        if (organisation == null)
        {
            throw new Exception($"unable to retrieve organisation-mapper for {alias}");
        }
        return organisation;
    }

    private IdentityProvider GetIdentityProviderTemplate(IamIdentityProviderProtocol providerProtocol)
    {
        switch (providerProtocol)
        {
            case IamIdentityProviderProtocol.OIDC:
                return _Settings.OidcIdentityProvider;
            case IamIdentityProviderProtocol.SAML:
                return _Settings.SamlIdentityProvider;
            default:
                throw new ArgumentOutOfRangeException($"unexpexted value of providerProtocol: {providerProtocol.ToString()}");
        }
    }

    private IamIdentityProviderClientAuthMethod IdentityProviderClientAuthTypeToIamClientAuthMethod(string clientAuthMethod)
    {
        try
        {
            return IdentityProviderClientAuthTypesIamClientAuthMethodDictionary[clientAuthMethod];
        }
        catch (KeyNotFoundException)
        {
            throw new ArgumentException($"unexpected value of clientAuthMethod: {clientAuthMethod}","clientAuthMethod");
        }
    }

    private string IamIdentityProviderClientAuthMethodToInternal(IamIdentityProviderClientAuthMethod iamClientAuthMethod)
    {
        try
        {
            return IamIdentityProviderClientAuthMethodsInternalDictionary[iamClientAuthMethod];
        }
        catch (KeyNotFoundException)
        {
            throw new ArgumentException($"unexpected value of IamClientAuthMethod: {iamClientAuthMethod}","authMethod");
        }
    }

    private IdentityProvider CloneIdentityProvider(IdentityProvider identityProvider) =>
        JsonSerializer.Deserialize<IdentityProvider>(JsonSerializer.Serialize(identityProvider))!;
}
