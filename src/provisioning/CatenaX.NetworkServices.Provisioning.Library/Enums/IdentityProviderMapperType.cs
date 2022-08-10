namespace CatenaX.NetworkServices.Provisioning.Library.Enums;

public enum IdentityProviderMapperType
{
    HARDCODED_SESSION_ATTRIBUTE = 1,
    HARDCODED_ATTRIBUTE = 2,
    OIDC_ADVANCED_GROUP = 3,
    OIDC_USER_ATTRIBUTE = 4,
    OIDC_ADVANCED_ROLE = 5,
    OIDC_HARDCODED_ROLE = 6,
    OIDC_ROLE = 7,
    OIDC_USERNAME = 8,
    KEYCLOAK_OIDC_ROLE = 9,
}
