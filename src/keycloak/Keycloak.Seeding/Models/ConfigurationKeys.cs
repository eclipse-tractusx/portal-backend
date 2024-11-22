namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

public static class ConfigurationKeys
{
    public const string RolesConfigKey = "ROLES";
    public const string LocalizationsConfigKey = "LOCALIZATIONS";
    public const string UserProfileConfigKey = "USERPROFILE";
    public const string ClientScopesConfigKey = "CLIENTSCOPES";
    public const string ClientsConfigKey = "CLIENTS";
    public const string IdentityProvidersConfigKey = "IDENTITYPROVIDERS";
    public const string IdentityProviderMappersConfigKey = "IDENTITYPROVIDERMAPPERS";
    public const string UsersConfigKey = "USERS";
    // TODO (PS): Clarify how to define the identity providers which should be skipped
    public const string FederatedIdentitiesConfigKeys = "FEDERATEDIDENTITIES";
    public const string ClientScopeMappersConfigKey = "CLIENTSCOPEMAPPERS";
    public const string ProtocolMappersConfigKey = "PROTOCOLMAPPERS";
    // TODO (PS): Clarify how to define the auth flows which should be skipped
    public const string AuthenticationFlowsConfigKey = "AUTHENTICATIONFLOWS";
    public const string ClientProtocolMapperConfigKey = "CLIENTPROTOCOLMAPPER";
    public const string ClientRolesConfigKey = "CLIENTROLES";
    public const string AuthenticationFlowExecutionConfigKey = "AUTHENTICATIONFLOWEXECUTION";
    public const string AuthenticatorConfigConfigKey = "AUTHENTICATORCONFIG";
}
