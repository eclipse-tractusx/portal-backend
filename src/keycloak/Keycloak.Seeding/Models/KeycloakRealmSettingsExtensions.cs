/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

public static class KeycloakRealmSettingsExtensions
{
    public static KeycloakRealm ToModel(this KeycloakRealmSettings keycloakRealmSettings) =>
        new()
        {
            Id = keycloakRealmSettings.Id,
            Realm = keycloakRealmSettings.Realm,
            DisplayName = keycloakRealmSettings.DisplayName,
            DisplayNameHtml = keycloakRealmSettings.DisplayNameHtml,
            NotBefore = keycloakRealmSettings.NotBefore,
            DefaultSignatureAlgorithm = keycloakRealmSettings.DefaultSignatureAlgorithm,
            RevokeRefreshToken = keycloakRealmSettings.RevokeRefreshToken,
            RefreshTokenMaxReuse = keycloakRealmSettings.RefreshTokenMaxReuse,
            AccessTokenLifespan = keycloakRealmSettings.AccessTokenLifespan,
            AccessTokenLifespanForImplicitFlow = keycloakRealmSettings.AccessTokenLifespanForImplicitFlow,
            SsoSessionIdleTimeout = keycloakRealmSettings.SsoSessionIdleTimeout,
            SsoSessionMaxLifespan = keycloakRealmSettings.SsoSessionMaxLifespan,
            SsoSessionIdleTimeoutRememberMe = keycloakRealmSettings.SsoSessionIdleTimeoutRememberMe,
            SsoSessionMaxLifespanRememberMe = keycloakRealmSettings.SsoSessionMaxLifespanRememberMe,
            OfflineSessionIdleTimeout = keycloakRealmSettings.OfflineSessionIdleTimeout,
            OfflineSessionMaxLifespanEnabled = keycloakRealmSettings.OfflineSessionMaxLifespanEnabled,
            OfflineSessionMaxLifespan = keycloakRealmSettings.OfflineSessionMaxLifespan,
            ClientSessionIdleTimeout = keycloakRealmSettings.ClientSessionIdleTimeout,
            ClientSessionMaxLifespan = keycloakRealmSettings.ClientSessionMaxLifespan,
            ClientOfflineSessionIdleTimeout = keycloakRealmSettings.ClientOfflineSessionIdleTimeout,
            ClientOfflineSessionMaxLifespan = keycloakRealmSettings.ClientOfflineSessionMaxLifespan,
            AccessCodeLifespan = keycloakRealmSettings.AccessCodeLifespan,
            AccessCodeLifespanUserAction = keycloakRealmSettings.AccessCodeLifespanUserAction,
            AccessCodeLifespanLogin = keycloakRealmSettings.AccessCodeLifespanLogin,
            ActionTokenGeneratedByAdminLifespan = keycloakRealmSettings.ActionTokenGeneratedByAdminLifespan,
            ActionTokenGeneratedByUserLifespan = keycloakRealmSettings.ActionTokenGeneratedByUserLifespan,
            Oauth2DeviceCodeLifespan = keycloakRealmSettings.Oauth2DeviceCodeLifespan,
            Oauth2DevicePollingInterval = keycloakRealmSettings.Oauth2DevicePollingInterval,
            Enabled = keycloakRealmSettings.Enabled,
            SslRequired = keycloakRealmSettings.SslRequired,
            RegistrationAllowed = keycloakRealmSettings.RegistrationAllowed,
            RegistrationEmailAsUsername = keycloakRealmSettings.RegistrationEmailAsUsername,
            RememberMe = keycloakRealmSettings.RememberMe,
            VerifyEmail = keycloakRealmSettings.VerifyEmail,
            LoginWithEmailAllowed = keycloakRealmSettings.LoginWithEmailAllowed,
            DuplicateEmailsAllowed = keycloakRealmSettings.DuplicateEmailsAllowed,
            ResetPasswordAllowed = keycloakRealmSettings.ResetPasswordAllowed,
            EditUsernameAllowed = keycloakRealmSettings.EditUsernameAllowed,
            BruteForceProtected = keycloakRealmSettings.BruteForceProtected,
            PermanentLockout = keycloakRealmSettings.PermanentLockout,
            MaxFailureWaitSeconds = keycloakRealmSettings.MaxFailureWaitSeconds,
            MinimumQuickLoginWaitSeconds = keycloakRealmSettings.MinimumQuickLoginWaitSeconds,
            WaitIncrementSeconds = keycloakRealmSettings.WaitIncrementSeconds,
            QuickLoginCheckMilliSeconds = keycloakRealmSettings.QuickLoginCheckMilliSeconds,
            MaxDeltaTimeSeconds = keycloakRealmSettings.MaxDeltaTimeSeconds,
            FailureFactor = keycloakRealmSettings.FailureFactor,
            Roles = keycloakRealmSettings.Roles?.ToModel(),
            Groups = keycloakRealmSettings.Groups?.Select(ToModel),
            DefaultRole = keycloakRealmSettings.DefaultRole?.ToModel(),
            DefaultGroups = keycloakRealmSettings.DefaultGroups,
            RequiredCredentials = keycloakRealmSettings.RequiredCredentials,
            OtpPolicyType = keycloakRealmSettings.OtpPolicyType,
            OtpPolicyAlgorithm = keycloakRealmSettings.OtpPolicyAlgorithm,
            OtpPolicyInitialCounter = keycloakRealmSettings.OtpPolicyInitialCounter,
            OtpPolicyDigits = keycloakRealmSettings.OtpPolicyDigits,
            OtpPolicyLookAheadWindow = keycloakRealmSettings.OtpPolicyLookAheadWindow,
            OtpPolicyPeriod = keycloakRealmSettings.OtpPolicyPeriod,
            OtpSupportedApplications = keycloakRealmSettings.OtpSupportedApplications,
            PasswordPolicy = keycloakRealmSettings.PasswordPolicy,
            LocalizationTexts = keycloakRealmSettings.LocalizationTexts,
            WebAuthnPolicyRpEntityName = keycloakRealmSettings.WebAuthnPolicyRpEntityName,
            WebAuthnPolicySignatureAlgorithms = keycloakRealmSettings.WebAuthnPolicySignatureAlgorithms,
            WebAuthnPolicyRpId = keycloakRealmSettings.WebAuthnPolicyRpId,
            WebAuthnPolicyAttestationConveyancePreference = keycloakRealmSettings.WebAuthnPolicyAttestationConveyancePreference,
            WebAuthnPolicyAuthenticatorAttachment = keycloakRealmSettings.WebAuthnPolicyAuthenticatorAttachment,
            WebAuthnPolicyRequireResidentKey = keycloakRealmSettings.WebAuthnPolicyRequireResidentKey,
            WebAuthnPolicyUserVerificationRequirement = keycloakRealmSettings.WebAuthnPolicyUserVerificationRequirement,
            WebAuthnPolicyCreateTimeout = keycloakRealmSettings.WebAuthnPolicyCreateTimeout,
            WebAuthnPolicyAvoidSameAuthenticatorRegister = keycloakRealmSettings.WebAuthnPolicyAvoidSameAuthenticatorRegister,
            WebAuthnPolicyAcceptableAaguids = keycloakRealmSettings.WebAuthnPolicyAcceptableAaguids,
            WebAuthnPolicyPasswordlessRpEntityName = keycloakRealmSettings.WebAuthnPolicyPasswordlessRpEntityName,
            WebAuthnPolicyPasswordlessSignatureAlgorithms = keycloakRealmSettings.WebAuthnPolicyPasswordlessSignatureAlgorithms,
            WebAuthnPolicyPasswordlessRpId = keycloakRealmSettings.WebAuthnPolicyPasswordlessRpId,
            WebAuthnPolicyPasswordlessAttestationConveyancePreference = keycloakRealmSettings.WebAuthnPolicyPasswordlessAttestationConveyancePreference,
            WebAuthnPolicyPasswordlessAuthenticatorAttachment = keycloakRealmSettings.WebAuthnPolicyPasswordlessAuthenticatorAttachment,
            WebAuthnPolicyPasswordlessRequireResidentKey = keycloakRealmSettings.WebAuthnPolicyPasswordlessRequireResidentKey,
            WebAuthnPolicyPasswordlessUserVerificationRequirement = keycloakRealmSettings.WebAuthnPolicyPasswordlessUserVerificationRequirement,
            WebAuthnPolicyPasswordlessCreateTimeout = keycloakRealmSettings.WebAuthnPolicyPasswordlessCreateTimeout,
            WebAuthnPolicyPasswordlessAvoidSameAuthenticatorRegister = keycloakRealmSettings.WebAuthnPolicyPasswordlessAvoidSameAuthenticatorRegister,
            WebAuthnPolicyPasswordlessAcceptableAaguids = keycloakRealmSettings.WebAuthnPolicyPasswordlessAcceptableAaguids,
            Users = keycloakRealmSettings.Users?.Select(ToModel),
            ScopeMappings = keycloakRealmSettings.ScopeMappings?.Select(ToModel),
            ClientScopeMappings = keycloakRealmSettings.ClientScopeMappings?.Select(ToModel).ToImmutableDictionary(),
            Clients = keycloakRealmSettings.Clients?.Select(ToModel),
            ClientScopes = keycloakRealmSettings.ClientScopes?.Select(ToModel),
            DefaultDefaultClientScopes = keycloakRealmSettings.DefaultDefaultClientScopes,
            DefaultOptionalClientScopes = keycloakRealmSettings.DefaultOptionalClientScopes,
            BrowserSecurityHeaders = keycloakRealmSettings.BrowserSecurityHeaders?.ToModel(),
            SmtpServer = keycloakRealmSettings.SmtpServer?.ToModel(),
            LoginTheme = keycloakRealmSettings.LoginTheme,
            AccountTheme = keycloakRealmSettings.AccountTheme,
            AdminTheme = keycloakRealmSettings.AdminTheme,
            EmailTheme = keycloakRealmSettings.EmailTheme,
            EventsEnabled = keycloakRealmSettings.EventsEnabled,
            EventsListeners = keycloakRealmSettings.EventsListeners,
            EnabledEventTypes = keycloakRealmSettings.EnabledEventTypes,
            AdminEventsEnabled = keycloakRealmSettings.AdminEventsEnabled,
            AdminEventsDetailsEnabled = keycloakRealmSettings.AdminEventsDetailsEnabled,
            IdentityProviders = keycloakRealmSettings.IdentityProviders?.Select(ToModel),
            IdentityProviderMappers = keycloakRealmSettings.IdentityProviderMappers?.Select(ToModel),
            Components = keycloakRealmSettings.Components?.Select(ToModel).ToImmutableDictionary(),
            InternationalizationEnabled = keycloakRealmSettings.InternationalizationEnabled,
            SupportedLocales = keycloakRealmSettings.SupportedLocales,
            DefaultLocale = keycloakRealmSettings.DefaultLocale,
            AuthenticationFlows = keycloakRealmSettings.AuthenticationFlows?.Select(ToModel),
            AuthenticatorConfig = keycloakRealmSettings.AuthenticatorConfig?.Select(ToModel),
            RequiredActions = keycloakRealmSettings.RequiredActions?.Select(ToModel),
            BrowserFlow = keycloakRealmSettings.BrowserFlow,
            RegistrationFlow = keycloakRealmSettings.RegistrationFlow,
            DirectGrantFlow = keycloakRealmSettings.DirectGrantFlow,
            ResetCredentialsFlow = keycloakRealmSettings.ResetCredentialsFlow,
            ClientAuthenticationFlow = keycloakRealmSettings.ClientAuthenticationFlow,
            DockerAuthenticationFlow = keycloakRealmSettings.DockerAuthenticationFlow,
            Attributes = keycloakRealmSettings.Attributes?.Select(ToModel).ToImmutableDictionary(),
            KeycloakVersion = keycloakRealmSettings.KeycloakVersion,
            UserManagedAccessAllowed = keycloakRealmSettings.UserManagedAccessAllowed,
            ClientProfiles = keycloakRealmSettings.ClientProfiles?.ToModel(),
            ClientPolicies = keycloakRealmSettings.ClientPolicies?.ToModel()
        };

    private static KeyValuePair<string, string?> ToModel(AttributeSettings attributeSettings) =>
        KeyValuePair.Create(
            attributeSettings.Name ?? throw new ConfigurationException(),
            attributeSettings.Value);

    private static KeyValuePair<string, IEnumerable<string>?> ToModel(MultiValueAttributeSettings multiValueAttributeSettings) =>
        KeyValuePair.Create(
            multiValueAttributeSettings.Name ?? throw new ConfigurationException("Attribute name must not be null"),
            multiValueAttributeSettings.Values);

    private static KeyValuePair<string, IEnumerable<string>?> ToModel(CompositeClientRolesSettings compositeClientRolesSettings) =>
        KeyValuePair.Create(
            compositeClientRolesSettings.ClientId ?? throw new ConfigurationException("CompositeClientRoles ClientId name must not be null"),
            compositeClientRolesSettings.Roles);

    private static KeyValuePair<string, IEnumerable<RoleModel>?> ToModel(ClientRoleSettings clientRoleSettings) =>
        KeyValuePair.Create(
            clientRoleSettings.ClientId ?? throw new ConfigurationException("clientRoles ClientId name must not be null"),
            clientRoleSettings.Roles?.Select(x => x.ToModel()));

    private static CompositeRolesModel ToModel(this CompositeRolesSettings compositeRolesSettings) =>
        new(compositeRolesSettings.Realm,
            compositeRolesSettings.Client?.Select(ToModel).ToImmutableDictionary());

    private static RoleModel ToModel(this RoleSettings roleSettings) =>
        new(roleSettings.Id,
            roleSettings.Name,
            roleSettings.Description,
            roleSettings.Composite,
            roleSettings.ClientRole,
            roleSettings.ContainerId,
            roleSettings.Attributes?.Select(ToModel).ToImmutableDictionary(),
            roleSettings.Composites?.ToModel());

    private static RolesModel ToModel(this RolesSettings rolesSettings) =>
        new(rolesSettings.Realm?.Select(x => x.ToModel()),
            rolesSettings.Client?.Select(ToModel).ToImmutableDictionary());

    private static KeyValuePair<string, IEnumerable<string>?> ToModel(UserClientRolesSettings userClientRolesSettings) =>
        KeyValuePair.Create(
            userClientRolesSettings.ClientId ?? throw new ConfigurationException("userClientRoles ClientId name must not be null"),
            userClientRolesSettings.Roles);

    private static GroupModel ToModel(GroupSettings groupSettings) =>
        new(groupSettings.Id,
            groupSettings.Name,
            groupSettings.Path,
            groupSettings.Attributes?.Select(ToModel).ToImmutableDictionary(),
            groupSettings.RealmRoles,
            groupSettings.ClientRoles?.Select(ToModel).ToImmutableDictionary(),
            groupSettings.SubGroups);

    public static KeyValuePair<string, string?> ToModel(CredentialsConfigSettings credentialsConfigSettings) =>
        KeyValuePair.Create(
            credentialsConfigSettings.Name ?? throw new ConfigurationException("credentialsConfig name must not be null"),
            credentialsConfigSettings.Value);

    private static CredentialsModel ToModel(CredentialsSettings credentialsSettings) =>
        new(credentialsSettings.Algorithm,
            credentialsSettings.Config?.Select(ToModel).ToImmutableDictionary(),
            credentialsSettings.Counter,
            credentialsSettings.CreatedDate,
            credentialsSettings.Device,
            credentialsSettings.Digits,
            credentialsSettings.HashIterations,
            credentialsSettings.HashSaltedValue,
            credentialsSettings.Period,
            credentialsSettings.Salt,
            credentialsSettings.Temporary,
            credentialsSettings.Type,
            credentialsSettings.Value);

    private static FederatedIdentityModel ToModel(FederatedIdentitySettings federatedIdentitySettings) =>
        new(federatedIdentitySettings.IdentityProvider,
            federatedIdentitySettings.UserId,
            federatedIdentitySettings.UserName);

    private static UserAccessModel ToModel(this UserAccessSettings userAccessSettings) =>
        new(userAccessSettings.ManageGroupMembership,
            userAccessSettings.View,
            userAccessSettings.MapRoles,
            userAccessSettings.Impersonate,
            userAccessSettings.Manage);

    private static UserConsentModel ToModel(ClientConsentSettings clientConsentSettings) =>
        new(clientConsentSettings.ClientId,
            clientConsentSettings.GrantedClientScopes,
            clientConsentSettings.CreatedDate,
            clientConsentSettings.LastUpdatedDate);

    private static UserModel ToModel(UserSettings userSettings) =>
        new(userSettings.Id,
            userSettings.CreatedTimestamp,
            userSettings.Username,
            userSettings.Enabled,
            userSettings.Totp,
            userSettings.EmailVerified,
            userSettings.FirstName,
            userSettings.LastName,
            userSettings.Email,
            userSettings.Attributes?.Select(ToModel).ToImmutableDictionary(),
            userSettings.Credentials?.Select(ToModel),
            userSettings.DisableableCredentialTypes,
            userSettings.RequiredActions,
            userSettings.FederatedIdentities?.Select(ToModel),
            userSettings.RealmRoles,
            userSettings.ClientRoles?.Select(ToModel).ToImmutableDictionary(),
            userSettings.NotBefore,
            userSettings.Groups,
            userSettings.ServiceAccountClientId,
            userSettings.Access?.ToModel(),
            userSettings.ClientConsents?.Select(ToModel),
            userSettings.FederationLink,
            userSettings.Origin,
            userSettings.Self);

    private static ScopeMappingModel ToModel(ScopeMappingSettings scopeMappingSettings) =>
        new(scopeMappingSettings.ClientScope,
            scopeMappingSettings.Roles);

    private static ClientScopeMappingModel ToModel(ClientScopeMappingSettings clientScopeMappingSettings) =>
        new(clientScopeMappingSettings.Client,
            clientScopeMappingSettings.Roles);

    private static KeyValuePair<string, IEnumerable<ClientScopeMappingModel>?> ToModel(ClientScopeMappingSettingsEntry clientScopeMappingSettingsEntry) =>
        KeyValuePair.Create(
            clientScopeMappingSettingsEntry.ClientId ?? throw new ConfigurationException("clientScopeMappingsEntry ClientId name must not be null"),
            clientScopeMappingSettingsEntry.ClientScopeMappings?.Select(ToModel));

    private static KeyValuePair<string, string?> ToModel(ClientAttributeSettings clientAttributeSettings) =>
        KeyValuePair.Create(
            clientAttributeSettings.Name ?? throw new ConfigurationException("clientAttributes Name must not be null"),
            clientAttributeSettings.Value);

    private static KeyValuePair<string, string?> ToModel(AuthenticationFlowBindingOverrideSettings authenticationFlowBindingOverrideSettings) =>
        KeyValuePair.Create(
            authenticationFlowBindingOverrideSettings.Name ?? throw new ConfigurationException("authenticationFlowBindingOverrides Name must not be null"),
            authenticationFlowBindingOverrideSettings.Value);

    private static KeyValuePair<string, string?> ToModel(ProtocolMapperConfigSettings protocolMapperConfigSettings) =>
        KeyValuePair.Create(
            protocolMapperConfigSettings.Name ?? throw new ConfigurationException("protocolMapperConfigs Name must not be null"),
            protocolMapperConfigSettings.Value);

    private static ProtocolMapperModel ToModel(ProtocolMapperSettings protocolMapperSettings) =>
        new(protocolMapperSettings.Id,
            protocolMapperSettings.Name,
            protocolMapperSettings.Protocol,
            protocolMapperSettings.ProtocolMapper,
            protocolMapperSettings.ConsentRequired,
            protocolMapperSettings.Config?.Select(ToModel).ToImmutableDictionary());

    private static ClientAccessModel ToModel(this ClientAccessSettings clientAccessSettings) =>
        new(clientAccessSettings.Configure,
            clientAccessSettings.Manage,
            clientAccessSettings.View);

    private static ClientModel ToModel(ClientSettings clientSettings) =>
        new(clientSettings.Id,
            clientSettings.ClientId,
            clientSettings.Name,
            clientSettings.RootUrl,
            clientSettings.BaseUrl,
            clientSettings.SurrogateAuthRequired,
            clientSettings.Enabled,
            clientSettings.AlwaysDisplayInConsole,
            clientSettings.ClientAuthenticatorType,
            clientSettings.RedirectUris,
            clientSettings.WebOrigins,
            clientSettings.NotBefore,
            clientSettings.BearerOnly,
            clientSettings.ConsentRequired,
            clientSettings.StandardFlowEnabled,
            clientSettings.ImplicitFlowEnabled,
            clientSettings.DirectAccessGrantsEnabled,
            clientSettings.ServiceAccountsEnabled,
            clientSettings.PublicClient,
            clientSettings.FrontchannelLogout,
            clientSettings.Protocol,
            clientSettings.Attributes?.Select(ToModel).ToImmutableDictionary(),
            clientSettings.AuthenticationFlowBindingOverrides?.Select(ToModel).ToImmutableDictionary(),
            clientSettings.FullScopeAllowed,
            clientSettings.NodeReRegistrationTimeout,
            clientSettings.DefaultClientScopes,
            clientSettings.OptionalClientScopes,
            clientSettings.ProtocolMappers?.Select(ToModel),
            clientSettings.Access?.ToModel(),
            clientSettings.Secret,
            clientSettings.AdminUrl,
            clientSettings.Description,
            clientSettings.AuthorizationServicesEnabled);

    private static ClientScopeModel ToModel(this ClientScopeSettings clientScopeSettings) =>
        new(clientScopeSettings.Id,
            clientScopeSettings.Name,
            clientScopeSettings.Protocol,
            clientScopeSettings.Attributes?.Select(ToModel).ToImmutableDictionary(),
            clientScopeSettings.ProtocolMappers?.Select(ToModel),
            clientScopeSettings.Description);

    private static BrowserSecurityHeadersModel ToModel(this BrowserSecurityHeadersSettings browserSecurityHeadersSettings) =>
        new(browserSecurityHeadersSettings.ContentSecurityPolicyReportOnly,
            browserSecurityHeadersSettings.XContentTypeOptions,
            browserSecurityHeadersSettings.XRobotsTag,
            browserSecurityHeadersSettings.XFrameOptions,
            browserSecurityHeadersSettings.ContentSecurityPolicy,
            browserSecurityHeadersSettings.XXSSProtection,
            browserSecurityHeadersSettings.StrictTransportSecurity);

    private static SmtpServerModel ToModel(this SmtpServerSettings smtpServerSettings) =>
        new(smtpServerSettings.Password,
            smtpServerSettings.Starttls,
            smtpServerSettings.Auth,
            smtpServerSettings.Port,
            smtpServerSettings.Host,
            smtpServerSettings.ReplyToDisplayName,
            smtpServerSettings.ReplyTo,
            smtpServerSettings.FromDisplayName,
            smtpServerSettings.From,
            smtpServerSettings.EnvelopeFrom,
            smtpServerSettings.Ssl,
            smtpServerSettings.User);

    private static IdentityProviderConfigModel ToModel(this IdentityProviderConfigSettings identityProviderConfigSettings) =>
        new(identityProviderConfigSettings.HideOnLoginPage,
            identityProviderConfigSettings.ClientSecret,
            identityProviderConfigSettings.DisableUserInfo,
            identityProviderConfigSettings.ValidateSignature,
            identityProviderConfigSettings.ClientId,
            identityProviderConfigSettings.TokenUrl,
            identityProviderConfigSettings.AuthorizationUrl,
            identityProviderConfigSettings.ClientAuthMethod,
            identityProviderConfigSettings.JwksUrl,
            identityProviderConfigSettings.LogoutUrl,
            identityProviderConfigSettings.ClientAssertionSigningAlg,
            identityProviderConfigSettings.SyncMode,
            identityProviderConfigSettings.UseJwksUrl,
            identityProviderConfigSettings.UserInfoUrl,
            identityProviderConfigSettings.Issuer,
            identityProviderConfigSettings.NameIDPolicyFormat,
            identityProviderConfigSettings.PrincipalType,
            identityProviderConfigSettings.SignatureAlgorithm,
            identityProviderConfigSettings.XmlSigKeyInfoKeyNameTransformer,
            identityProviderConfigSettings.AllowCreate,
            identityProviderConfigSettings.EntityId,
            identityProviderConfigSettings.AuthnContextComparisonType,
            identityProviderConfigSettings.BackchannelSupported,
            identityProviderConfigSettings.PostBindingResponse,
            identityProviderConfigSettings.PostBindingAuthnRequest,
            identityProviderConfigSettings.PostBindingLogout,
            identityProviderConfigSettings.WantAuthnRequestsSigned,
            identityProviderConfigSettings.WantAssertionsSigned,
            identityProviderConfigSettings.WantAssertionsEncrypted,
            identityProviderConfigSettings.ForceAuthn,
            identityProviderConfigSettings.SignSpMetadata,
            identityProviderConfigSettings.LoginHint,
            identityProviderConfigSettings.SingleSignOnServiceUrl,
            identityProviderConfigSettings.AllowedClockSkew,
            identityProviderConfigSettings.AttributeConsumingServiceIndex);

    private static IdentityProviderModel ToModel(IdentityProviderSettings identityProviderSettings) =>
        new(identityProviderSettings.Alias,
            identityProviderSettings.DisplayName,
            identityProviderSettings.InternalId,
            identityProviderSettings.ProviderId,
            identityProviderSettings.Enabled,
            identityProviderSettings.UpdateProfileFirstLoginMode,
            identityProviderSettings.TrustEmail,
            identityProviderSettings.StoreToken,
            identityProviderSettings.AddReadTokenRoleOnCreate,
            identityProviderSettings.AuthenticateByDefault,
            identityProviderSettings.LinkOnly,
            identityProviderSettings.FirstBrokerLoginFlowAlias,
            identityProviderSettings.PostBrokerLoginFlowAlias,
            identityProviderSettings.Config?.ToModel());

    private static KeyValuePair<string, string?> ToModel(IdentityProviderMapperConfigSettings identityProviderMapperConfigSettings) =>
        KeyValuePair.Create(
            identityProviderMapperConfigSettings.Name ?? throw new ConfigurationException("identityProviderConfigs Name must not be null"),
            identityProviderMapperConfigSettings.Value);

    private static IdentityProviderMapperModel ToModel(IdentityProviderMapperSettings identityProviderMapperSettings) =>
        new(identityProviderMapperSettings.Id,
            identityProviderMapperSettings.Name,
            identityProviderMapperSettings.IdentityProviderAlias,
            identityProviderMapperSettings.IdentityProviderMapper,
            identityProviderMapperSettings.Config?.Select(ToModel).ToImmutableDictionary());

    private static KeyValuePair<string, IEnumerable<string>?> ToModel(ComponentConfigSettings componentConfigSettings) =>
        KeyValuePair.Create(
            componentConfigSettings.Name ?? throw new ConfigurationException(),
            componentConfigSettings.Values);

    private static ComponentModel ToModel(ComponentSettings componentSettings) =>
        new(componentSettings.Id,
            componentSettings.Name,
            componentSettings.ProviderId,
            componentSettings.SubType,
            componentSettings.SubComponents,
            componentSettings.Config?.Select(ToModel).ToImmutableDictionary());

    private static KeyValuePair<string, IEnumerable<ComponentModel>?> ToModel(ComponentSettingsEntry componentSettingsEntry) =>
        KeyValuePair.Create(
            componentSettingsEntry.Name ?? throw new ConfigurationException(),
            componentSettingsEntry.ComponentSettings?.Select(ToModel));

    private static AuthenticationExecutionModel ToModel(AuthenticationExecutionSettings authenticationExecutionSettings) =>
        new(authenticationExecutionSettings.Authenticator,
            authenticationExecutionSettings.AuthenticatorFlow,
            authenticationExecutionSettings.Requirement,
            authenticationExecutionSettings.Priority,
            authenticationExecutionSettings.UserSetupAllowed,
            authenticationExecutionSettings.AutheticatorFlow,
            authenticationExecutionSettings.FlowAlias,
            authenticationExecutionSettings.AuthenticatorConfig);

    private static AuthenticationFlowModel ToModel(AuthenticationFlowSettings authenticationFlowSettings) =>
        new(authenticationFlowSettings.Id,
            authenticationFlowSettings.Alias,
            authenticationFlowSettings.Description,
            authenticationFlowSettings.ProviderId,
            authenticationFlowSettings.TopLevel,
            authenticationFlowSettings.BuiltIn,
            authenticationFlowSettings.AuthenticationExecutions?.Select(ToModel));

    private static KeyValuePair<string, string?> ToModel(AuthenticatorConfigConfigSettings authenticatorConfigConfigSettings) =>
        KeyValuePair.Create(
            authenticatorConfigConfigSettings.Name ?? throw new ConfigurationException(),
            authenticatorConfigConfigSettings.Value);

    private static AuthenticatorConfigModel ToModel(AuthenticatorConfigSettings authenticatorConfigSettings) =>
        new(authenticatorConfigSettings.Id,
            authenticatorConfigSettings.Alias,
            authenticatorConfigSettings.Config?.Select(ToModel).ToImmutableDictionary());

    private static RequiredActionModel ToModel(this RequiredActionSettings requiredActionSettings) =>
        new(requiredActionSettings.Alias,
            requiredActionSettings.Name,
            requiredActionSettings.ProviderId,
            requiredActionSettings.Enabled,
            requiredActionSettings.DefaultAction,
            requiredActionSettings.Priority,
            requiredActionSettings.Config);  // TODO config is declared as object

    private static ClientProfilesModel ToModel(this ClientProfilesSettings clientProfilesSettings) =>
        new(clientProfilesSettings.Profiles); // TODO profiles is declared as IEnumerable<object>

    private static ClientPoliciesModel ToModel(this ClientPoliciesSettings clientPoliciesSettings) =>
        new(clientPoliciesSettings.Policies); // TODO policies is declared as IEnumerable<object>
}
