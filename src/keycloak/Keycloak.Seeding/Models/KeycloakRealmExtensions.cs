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

using System.Diagnostics.CodeAnalysis;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

public static class KeycloakRealmExtensions
{
    public static KeycloakRealm Merge(this KeycloakRealm left, KeycloakRealm right) =>
        new()
        {
            Id = right.Id ?? left.Id,
            Realm = right.Realm ?? left.Realm,
            DisplayName = right.DisplayName ?? left.DisplayName,
            DisplayNameHtml = right.DisplayNameHtml ?? left.DisplayNameHtml,
            NotBefore = right.NotBefore ?? left.NotBefore,
            DefaultSignatureAlgorithm = right.DefaultSignatureAlgorithm ?? left.DefaultSignatureAlgorithm,
            RevokeRefreshToken = right.RevokeRefreshToken ?? left.RevokeRefreshToken,
            RefreshTokenMaxReuse = right.RefreshTokenMaxReuse ?? left.RefreshTokenMaxReuse,
            AccessTokenLifespan = right.AccessTokenLifespan ?? left.AccessTokenLifespan,
            AccessTokenLifespanForImplicitFlow = right.AccessTokenLifespanForImplicitFlow ?? left.AccessTokenLifespanForImplicitFlow,
            SsoSessionIdleTimeout = right.SsoSessionIdleTimeout ?? left.SsoSessionIdleTimeout,
            SsoSessionMaxLifespan = right.SsoSessionMaxLifespan ?? left.SsoSessionMaxLifespan,
            SsoSessionIdleTimeoutRememberMe = right.SsoSessionIdleTimeoutRememberMe ?? left.SsoSessionIdleTimeoutRememberMe,
            SsoSessionMaxLifespanRememberMe = right.SsoSessionMaxLifespanRememberMe ?? left.SsoSessionMaxLifespanRememberMe,
            OfflineSessionIdleTimeout = right.OfflineSessionIdleTimeout ?? left.OfflineSessionIdleTimeout,
            OfflineSessionMaxLifespanEnabled = right.OfflineSessionMaxLifespanEnabled ?? left.OfflineSessionMaxLifespanEnabled,
            OfflineSessionMaxLifespan = right.OfflineSessionMaxLifespan ?? left.OfflineSessionMaxLifespan,
            ClientSessionIdleTimeout = right.ClientSessionIdleTimeout ?? left.ClientSessionIdleTimeout,
            ClientSessionMaxLifespan = right.ClientSessionMaxLifespan ?? left.ClientSessionMaxLifespan,
            ClientOfflineSessionIdleTimeout = right.ClientOfflineSessionIdleTimeout ?? left.ClientOfflineSessionIdleTimeout,
            ClientOfflineSessionMaxLifespan = right.ClientOfflineSessionMaxLifespan ?? left.ClientOfflineSessionMaxLifespan,
            AccessCodeLifespan = right.AccessCodeLifespan ?? left.AccessCodeLifespan,
            AccessCodeLifespanUserAction = right.AccessCodeLifespanUserAction ?? left.AccessCodeLifespanUserAction,
            AccessCodeLifespanLogin = right.AccessCodeLifespanLogin ?? left.AccessCodeLifespanLogin,
            ActionTokenGeneratedByAdminLifespan = right.ActionTokenGeneratedByAdminLifespan ?? left.ActionTokenGeneratedByAdminLifespan,
            ActionTokenGeneratedByUserLifespan = right.ActionTokenGeneratedByUserLifespan ?? left.ActionTokenGeneratedByUserLifespan,
            Oauth2DeviceCodeLifespan = right.Oauth2DeviceCodeLifespan ?? left.Oauth2DeviceCodeLifespan,
            Oauth2DevicePollingInterval = right.Oauth2DevicePollingInterval ?? left.Oauth2DevicePollingInterval,
            Enabled = right.Enabled ?? left.Enabled,
            SslRequired = right.SslRequired ?? left.SslRequired,
            RegistrationAllowed = right.RegistrationAllowed ?? left.RegistrationAllowed,
            RegistrationEmailAsUsername = right.RegistrationEmailAsUsername ?? left.RegistrationEmailAsUsername,
            RememberMe = right.RememberMe ?? left.RememberMe,
            VerifyEmail = right.VerifyEmail ?? left.VerifyEmail,
            LoginWithEmailAllowed = right.LoginWithEmailAllowed ?? left.LoginWithEmailAllowed,
            DuplicateEmailsAllowed = right.DuplicateEmailsAllowed ?? left.DuplicateEmailsAllowed,
            ResetPasswordAllowed = right.ResetPasswordAllowed ?? left.ResetPasswordAllowed,
            EditUsernameAllowed = right.EditUsernameAllowed ?? left.EditUsernameAllowed,
            BruteForceProtected = right.BruteForceProtected ?? left.BruteForceProtected,
            PermanentLockout = right.PermanentLockout ?? left.PermanentLockout,
            MaxFailureWaitSeconds = right.MaxFailureWaitSeconds ?? left.MaxFailureWaitSeconds,
            MinimumQuickLoginWaitSeconds = right.MinimumQuickLoginWaitSeconds ?? left.MinimumQuickLoginWaitSeconds,
            WaitIncrementSeconds = right.WaitIncrementSeconds ?? left.WaitIncrementSeconds,
            QuickLoginCheckMilliSeconds = right.QuickLoginCheckMilliSeconds ?? left.QuickLoginCheckMilliSeconds,
            MaxDeltaTimeSeconds = right.MaxDeltaTimeSeconds ?? left.MaxDeltaTimeSeconds,
            FailureFactor = right.FailureFactor ?? left.FailureFactor,
            Roles = Merge(left.Roles, right.Roles, MergeRoles),
            Groups = Merge(left.Groups, right.Groups, x => x.Name, MergeGroup),
            DefaultRole = Merge(left.DefaultRole, right.DefaultRole, MergeRole),
            DefaultGroups = right.DefaultGroups ?? left.DefaultGroups,
            RequiredCredentials = right.RequiredCredentials ?? left.RequiredCredentials,
            OtpPolicyType = right.OtpPolicyType ?? left.OtpPolicyType,
            OtpPolicyAlgorithm = right.OtpPolicyAlgorithm ?? left.OtpPolicyAlgorithm,
            OtpPolicyInitialCounter = right.OtpPolicyInitialCounter ?? left.OtpPolicyInitialCounter,
            OtpPolicyDigits = right.OtpPolicyDigits ?? left.OtpPolicyDigits,
            OtpPolicyLookAheadWindow = right.OtpPolicyLookAheadWindow ?? left.OtpPolicyLookAheadWindow,
            OtpPolicyPeriod = right.OtpPolicyPeriod ?? left.OtpPolicyPeriod,
            OtpSupportedApplications = right.OtpSupportedApplications ?? left.OtpSupportedApplications,
            PasswordPolicy = right.PasswordPolicy ?? left.PasswordPolicy,
            LocalizationTexts = right.LocalizationTexts ?? left.LocalizationTexts,
            WebAuthnPolicyRpEntityName = right.WebAuthnPolicyRpEntityName ?? left.WebAuthnPolicyRpEntityName,
            WebAuthnPolicySignatureAlgorithms = right.WebAuthnPolicySignatureAlgorithms ?? left.WebAuthnPolicySignatureAlgorithms,
            WebAuthnPolicyRpId = right.WebAuthnPolicyRpId ?? left.WebAuthnPolicyRpId,
            WebAuthnPolicyAttestationConveyancePreference = right.WebAuthnPolicyAttestationConveyancePreference ?? left.WebAuthnPolicyAttestationConveyancePreference,
            WebAuthnPolicyAuthenticatorAttachment = right.WebAuthnPolicyAuthenticatorAttachment ?? left.WebAuthnPolicyAuthenticatorAttachment,
            WebAuthnPolicyRequireResidentKey = right.WebAuthnPolicyRequireResidentKey ?? left.WebAuthnPolicyRequireResidentKey,
            WebAuthnPolicyUserVerificationRequirement = right.WebAuthnPolicyUserVerificationRequirement ?? left.WebAuthnPolicyUserVerificationRequirement,
            WebAuthnPolicyCreateTimeout = right.WebAuthnPolicyCreateTimeout ?? left.WebAuthnPolicyCreateTimeout,
            WebAuthnPolicyAvoidSameAuthenticatorRegister = right.WebAuthnPolicyAvoidSameAuthenticatorRegister ?? left.WebAuthnPolicyAvoidSameAuthenticatorRegister,
            WebAuthnPolicyAcceptableAaguids = right.WebAuthnPolicyAcceptableAaguids ?? left.WebAuthnPolicyAcceptableAaguids,
            WebAuthnPolicyPasswordlessRpEntityName = right.WebAuthnPolicyPasswordlessRpEntityName ?? left.WebAuthnPolicyPasswordlessRpEntityName,
            WebAuthnPolicyPasswordlessSignatureAlgorithms = right.WebAuthnPolicyPasswordlessSignatureAlgorithms ?? left.WebAuthnPolicyPasswordlessSignatureAlgorithms,
            WebAuthnPolicyPasswordlessRpId = right.WebAuthnPolicyPasswordlessRpId ?? left.WebAuthnPolicyPasswordlessRpId,
            WebAuthnPolicyPasswordlessAttestationConveyancePreference = right.WebAuthnPolicyPasswordlessAttestationConveyancePreference ?? left.WebAuthnPolicyPasswordlessAttestationConveyancePreference,
            WebAuthnPolicyPasswordlessAuthenticatorAttachment = right.WebAuthnPolicyPasswordlessAuthenticatorAttachment ?? left.WebAuthnPolicyPasswordlessAuthenticatorAttachment,
            WebAuthnPolicyPasswordlessRequireResidentKey = right.WebAuthnPolicyPasswordlessRequireResidentKey ?? left.WebAuthnPolicyPasswordlessRequireResidentKey,
            WebAuthnPolicyPasswordlessUserVerificationRequirement = right.WebAuthnPolicyPasswordlessUserVerificationRequirement ?? left.WebAuthnPolicyPasswordlessUserVerificationRequirement,
            WebAuthnPolicyPasswordlessCreateTimeout = right.WebAuthnPolicyPasswordlessCreateTimeout ?? left.WebAuthnPolicyPasswordlessCreateTimeout,
            WebAuthnPolicyPasswordlessAvoidSameAuthenticatorRegister = right.WebAuthnPolicyPasswordlessAvoidSameAuthenticatorRegister ?? left.WebAuthnPolicyPasswordlessAvoidSameAuthenticatorRegister,
            WebAuthnPolicyPasswordlessAcceptableAaguids = right.WebAuthnPolicyPasswordlessAcceptableAaguids ?? left.WebAuthnPolicyPasswordlessAcceptableAaguids,
            Users = Merge(left.Users, right.Users, x => x.Username, MergeUser),
            ScopeMappings = Merge(left.ScopeMappings, right.ScopeMappings, x => x.ClientScope, (left, right) => right),
            ClientScopeMappings = Merge(left.ClientScopeMappings, right.ClientScopeMappings, x => x.Key, (left, right) => right)?.ToDictionary(),
            Clients = Merge(left.Clients, right.Clients, x => x.ClientId, MergeClient),
            ClientScopes = Merge(left.ClientScopes, right.ClientScopes, x => x.Name, MergeClientScope),
            DefaultDefaultClientScopes = right.DefaultDefaultClientScopes ?? left.DefaultDefaultClientScopes,
            DefaultOptionalClientScopes = right.DefaultOptionalClientScopes ?? left.DefaultOptionalClientScopes,
            BrowserSecurityHeaders = Merge(left.BrowserSecurityHeaders, right.BrowserSecurityHeaders, MergeBrowserSecurityHeaders),
            SmtpServer = Merge(left.SmtpServer, right.SmtpServer, MergeSmtpServer),
            LoginTheme = right.LoginTheme ?? left.LoginTheme,
            AccountTheme = right.AccountTheme ?? left.AccountTheme,
            AdminTheme = right.AdminTheme ?? left.AdminTheme,
            EmailTheme = right.EmailTheme ?? left.EmailTheme,
            EventsEnabled = right.EventsEnabled ?? left.EventsEnabled,
            EventsListeners = right.EventsListeners ?? left.EventsListeners,
            EnabledEventTypes = right.EnabledEventTypes ?? left.EnabledEventTypes,
            AdminEventsEnabled = right.AdminEventsEnabled ?? left.AdminEventsEnabled,
            AdminEventsDetailsEnabled = right.AdminEventsDetailsEnabled ?? left.AdminEventsDetailsEnabled,
            IdentityProviders = Merge(left.IdentityProviders, right.IdentityProviders, x => x.Alias, MergeIdentityProvider),
            IdentityProviderMappers = Merge(left.IdentityProviderMappers, right.IdentityProviderMappers, x => x.Name, MergeIdentityProviderMapper),
            Components = Merge(left.Components, right.Components, x => x.Key, (x, y) => KeyValuePair.Create(x.Key, Merge(x.Value, y.Value, z => z.Name, MergeComponent)))?.ToDictionary(),
            InternationalizationEnabled = right.InternationalizationEnabled ?? left.InternationalizationEnabled,
            SupportedLocales = right.SupportedLocales ?? left.SupportedLocales,
            DefaultLocale = right.DefaultLocale ?? left.DefaultLocale,
            AuthenticationFlows = Merge(left.AuthenticationFlows, right.AuthenticationFlows, x => x.Alias, MergeAuthenticationFlow),
            AuthenticatorConfig = Merge(left.AuthenticatorConfig, right.AuthenticatorConfig, x => x.Alias, MergeAuthenticatorConfig),
            RequiredActions = Merge(left.RequiredActions, right.RequiredActions, x => x.Alias, MergeRequiredAction),
            BrowserFlow = right.BrowserFlow ?? left.BrowserFlow,
            RegistrationFlow = right.RegistrationFlow ?? left.RegistrationFlow,
            DirectGrantFlow = right.DirectGrantFlow ?? left.DirectGrantFlow,
            ResetCredentialsFlow = right.ResetCredentialsFlow ?? left.ResetCredentialsFlow,
            ClientAuthenticationFlow = right.ClientAuthenticationFlow ?? left.ClientAuthenticationFlow,
            DockerAuthenticationFlow = right.DockerAuthenticationFlow ?? left.DockerAuthenticationFlow,
            Attributes = Merge(left.Attributes, right.Attributes, x => x.Key, (left, right) => right)?.ToDictionary(),
            KeycloakVersion = right.KeycloakVersion ?? left.KeycloakVersion,
            UserManagedAccessAllowed = right.UserManagedAccessAllowed ?? left.UserManagedAccessAllowed,
            ClientProfiles = Merge(left.ClientProfiles, right.ClientProfiles, MergeClientProfiles),
            ClientPolicies = Merge(left.ClientPolicies, right.ClientPolicies, MergeClientPolicies)
        };

    [return: NotNullIfNotNull(nameof(left))]
    [return: NotNullIfNotNull(nameof(right))]
    private static TModel? Merge<TModel>(TModel? left, TModel? right, Func<TModel, TModel, TModel> merge) =>
    (left, right) switch
    {
        (null, null) => default,
        (null, _) => right,
        (_, null) => left,
        (_, _) => merge(left, right)
    };

    [return: NotNullIfNotNull(nameof(left))]
    [return: NotNullIfNotNull(nameof(right))]
    private static IEnumerable<TModel>? Merge<TModel, TKey>(IEnumerable<TModel>? left, IEnumerable<TModel>? right, Func<TModel, TKey> select, Func<TModel, TModel, TModel> merge) =>
        Merge(
            left,
            right,
            (left, right) => left.Join(right, select, select, merge)
                    .Concat(left.ExceptBy(right.Select(select), select))
                    .Concat(right.ExceptBy(left.Select(select), select)));

    private static RolesModel MergeRoles(RolesModel left, RolesModel right) =>
        new(Merge(left.Realm, right.Realm, x => x.Name, MergeRole),
            Merge(left.Client, right.Client, x => x.Key, (left, right) => KeyValuePair.Create(left.Key, Merge(left.Value, right.Value, x => x.Name, MergeRole)))?.ToDictionary());

    private static RoleModel MergeRole(RoleModel left, RoleModel right) =>
        new(right.Id ?? left.Id,
            right.Name ?? left.Name,
            right.Description ?? left.Description,
            right.Composite ?? left.Composite,
            right.ClientRole ?? left.ClientRole,
            right.ContainerId ?? left.ContainerId,
            Merge(left.Attributes, right.Attributes, x => x.Key, (left, right) => right)?.ToDictionary(),
            Merge(left.Composites, right.Composites, MergeCompositeRoles));

    private static CompositeRolesModel MergeCompositeRoles(CompositeRolesModel left, CompositeRolesModel right) =>
        new(right.Realm ?? left.Realm,
            Merge(left.Client, right.Client, x => x.Key, (left, right) => right)?.ToDictionary());

    private static GroupModel MergeGroup(GroupModel left, GroupModel right) =>
        new(right.Id ?? left.Id,
            right.Name ?? left.Name,
            right.Path ?? left.Path,
            Merge(left.Attributes, right.Attributes, x => x.Key, (left, right) => right)?.ToDictionary(),
            right.RealmRoles ?? left.RealmRoles,
            Merge(left.ClientRoles, right.ClientRoles, x => x.Key, (left, right) => right)?.ToDictionary(),
            right.SubGroups ?? left.SubGroups);

    private static UserModel MergeUser(UserModel left, UserModel right) =>
        new(right.Id ?? left.Id,
            right.CreatedTimestamp ?? left.CreatedTimestamp,
            right.Username ?? left.Username,
            right.Enabled ?? left.Enabled,
            right.Totp ?? left.Totp,
            right.EmailVerified ?? left.EmailVerified,
            right.FirstName ?? left.FirstName,
            right.LastName ?? left.LastName,
            right.Email ?? left.Email,
            Merge(left.Attributes, right.Attributes, x => x.Key, (left, right) => right)?.ToDictionary(),
            right.Credentials ?? left.Credentials,
            right.DisableableCredentialTypes ?? left.DisableableCredentialTypes,
            right.RequiredActions ?? left.RequiredActions,
            Merge(left.FederatedIdentities, right.FederatedIdentities, x => x.IdentityProvider, MergeFederatedIdentity),
            right.RealmRoles ?? left.RealmRoles,
            Merge(left.ClientRoles, right.ClientRoles, x => x.Key, (left, right) => right)?.ToDictionary(),
            right.NotBefore ?? left.NotBefore,
            right.Groups ?? left.Groups,
            right.ServiceAccountClientId ?? left.ServiceAccountClientId,
            right.Access ?? left.Access,
            right.ClientConsents ?? left.ClientConsents,
            right.FederationLink ?? left.FederationLink,
            right.Origin ?? left.Origin,
            right.Self ?? left.Self);

    private static FederatedIdentityModel MergeFederatedIdentity(FederatedIdentityModel left, FederatedIdentityModel right) =>
        new(right.IdentityProvider ?? left.IdentityProvider,
            right.UserId ?? left.UserId,
            right.UserName ?? left.UserName);

    private static ClientModel MergeClient(ClientModel left, ClientModel right) =>
        new(right.Id ?? left.Id,
            right.ClientId ?? left.ClientId,
            right.Name ?? left.Name,
            right.RootUrl ?? left.RootUrl,
            right.BaseUrl ?? left.BaseUrl,
            right.SurrogateAuthRequired ?? left.SurrogateAuthRequired,
            right.Enabled ?? left.Enabled,
            right.AlwaysDisplayInConsole ?? left.AlwaysDisplayInConsole,
            right.ClientAuthenticatorType ?? left.ClientAuthenticatorType,
            right.RedirectUris ?? left.RedirectUris,
            right.WebOrigins ?? left.WebOrigins,
            right.NotBefore ?? left.NotBefore,
            right.BearerOnly ?? left.BearerOnly,
            right.ConsentRequired ?? left.ConsentRequired,
            right.StandardFlowEnabled ?? left.StandardFlowEnabled,
            right.ImplicitFlowEnabled ?? left.ImplicitFlowEnabled,
            right.DirectAccessGrantsEnabled ?? left.DirectAccessGrantsEnabled,
            right.ServiceAccountsEnabled ?? left.ServiceAccountsEnabled,
            right.PublicClient ?? left.PublicClient,
            right.FrontchannelLogout ?? left.FrontchannelLogout,
            right.Protocol ?? left.Protocol,
            Merge(left.Attributes, right.Attributes, x => x.Key, (left, right) => right)?.ToDictionary(),
            Merge(left.AuthenticationFlowBindingOverrides, right.AuthenticationFlowBindingOverrides, x => x.Key, (left, right) => right)?.ToDictionary(),
            right.FullScopeAllowed ?? left.FullScopeAllowed,
            right.NodeReRegistrationTimeout ?? left.NodeReRegistrationTimeout,
            right.DefaultClientScopes ?? left.DefaultClientScopes,
            right.OptionalClientScopes ?? left.OptionalClientScopes,
            Merge(left.ProtocolMappers, right.ProtocolMappers, x => x.Name, MergeProtocolMapper),
            Merge(left.Access, right.Access, MergeClientAccess),
            right.Secret ?? left.Secret,
            right.AdminUrl ?? left.AdminUrl,
            right.Description ?? left.Description,
            right.AuthorizationServicesEnabled ?? left.AuthorizationServicesEnabled);

    private static ClientAccessModel MergeClientAccess(ClientAccessModel left, ClientAccessModel right) =>
        new(right.Configure ?? left.Configure,
            right.Manage ?? left.Manage,
            right.View ?? left.View);

    private static ClientScopeModel MergeClientScope(ClientScopeModel left, ClientScopeModel right) =>
        new(right.Id ?? left.Id,
            right.Name ?? left.Name,
            right.Protocol ?? left.Protocol,
            Merge(left.Attributes, right.Attributes, x => x.Key, (left, right) => right)?.ToDictionary(),
            Merge(left.ProtocolMappers, right.ProtocolMappers, x => x.Name, MergeProtocolMapper),
            right.Description ?? left.Description);

    private static ProtocolMapperModel MergeProtocolMapper(ProtocolMapperModel left, ProtocolMapperModel right) =>
        new(right.Id ?? left.Id,
            right.Name ?? left.Name,
            right.Protocol ?? left.Protocol,
            right.ProtocolMapper ?? left.ProtocolMapper,
            right.ConsentRequired ?? left.ConsentRequired,
            Merge(left.Config, right.Config, x => x.Key, (left, right) => right)?.ToDictionary());

    private static BrowserSecurityHeadersModel MergeBrowserSecurityHeaders(BrowserSecurityHeadersModel left, BrowserSecurityHeadersModel right) =>
        new(right.ContentSecurityPolicyReportOnly ?? left.ContentSecurityPolicyReportOnly,
            right.XContentTypeOptions ?? left.XContentTypeOptions,
            right.XRobotsTag ?? left.XRobotsTag,
            right.XFrameOptions ?? left.XFrameOptions,
            right.ContentSecurityPolicy ?? left.ContentSecurityPolicy,
            right.XXSSProtection ?? left.XXSSProtection,
            right.StrictTransportSecurity ?? left.StrictTransportSecurity);

    private static SmtpServerModel MergeSmtpServer(SmtpServerModel left, SmtpServerModel right) =>
        new(right.Password ?? left.Password,
            right.Starttls ?? left.Starttls,
            right.Auth ?? left.Auth,
            right.Port ?? left.Port,
            right.Host ?? left.Host,
            right.ReplyToDisplayName ?? left.ReplyToDisplayName,
            right.ReplyTo ?? left.ReplyTo,
            right.FromDisplayName ?? left.FromDisplayName,
            right.From ?? left.From,
            right.EnvelopeFrom ?? left.EnvelopeFrom,
            right.Ssl ?? left.Ssl,
            right.User ?? left.User);

    private static IdentityProviderModel MergeIdentityProvider(IdentityProviderModel left, IdentityProviderModel right) =>
        new(right.Alias ?? left.Alias,
            right.DisplayName ?? left.DisplayName,
            right.InternalId ?? left.InternalId,
            right.ProviderId ?? left.ProviderId,
            right.Enabled ?? left.Enabled,
            right.UpdateProfileFirstLoginMode ?? left.UpdateProfileFirstLoginMode,
            right.TrustEmail ?? left.TrustEmail,
            right.StoreToken ?? left.StoreToken,
            right.AddReadTokenRoleOnCreate ?? left.AddReadTokenRoleOnCreate,
            right.AuthenticateByDefault ?? left.AuthenticateByDefault,
            right.LinkOnly ?? left.LinkOnly,
            right.FirstBrokerLoginFlowAlias ?? left.FirstBrokerLoginFlowAlias,
            right.PostBrokerLoginFlowAlias ?? left.PostBrokerLoginFlowAlias,
            Merge(left.Config, right.Config, MergeIdentityProviderConfig));

    private static IdentityProviderConfigModel MergeIdentityProviderConfig(IdentityProviderConfigModel left, IdentityProviderConfigModel right) =>
        new(right.HideOnLoginPage ?? left.HideOnLoginPage,
            right.ClientSecret ?? left.ClientSecret,
            right.DisableUserInfo ?? left.DisableUserInfo,
            right.ValidateSignature ?? left.ValidateSignature,
            right.ClientId ?? left.ClientId,
            right.TokenUrl ?? left.TokenUrl,
            right.AuthorizationUrl ?? left.AuthorizationUrl,
            right.ClientAuthMethod ?? left.ClientAuthMethod,
            right.JwksUrl ?? left.JwksUrl,
            right.LogoutUrl ?? left.LogoutUrl,
            right.ClientAssertionSigningAlg ?? left.ClientAssertionSigningAlg,
            right.SyncMode ?? left.SyncMode,
            right.UseJwksUrl ?? left.UseJwksUrl,
            right.UserInfoUrl ?? left.UserInfoUrl,
            right.Issuer ?? left.Issuer,
            right.NameIDPolicyFormat ?? left.NameIDPolicyFormat,
            right.PrincipalType ?? left.PrincipalType,
            right.SignatureAlgorithm ?? left.SignatureAlgorithm,
            right.XmlSigKeyInfoKeyNameTransformer ?? left.XmlSigKeyInfoKeyNameTransformer,
            right.AllowCreate ?? left.AllowCreate,
            right.EntityId ?? left.EntityId,
            right.AuthnContextComparisonType ?? left.AuthnContextComparisonType,
            right.BackchannelSupported ?? left.BackchannelSupported,
            right.PostBindingResponse ?? left.PostBindingResponse,
            right.PostBindingAuthnRequest ?? left.PostBindingAuthnRequest,
            right.PostBindingLogout ?? left.PostBindingLogout,
            right.WantAuthnRequestsSigned ?? left.WantAuthnRequestsSigned,
            right.WantAssertionsSigned ?? left.WantAssertionsSigned,
            right.WantAssertionsEncrypted ?? left.WantAssertionsEncrypted,
            right.ForceAuthn ?? left.ForceAuthn,
            right.SignSpMetadata ?? left.SignSpMetadata,
            right.LoginHint ?? left.LoginHint,
            right.SingleSignOnServiceUrl ?? left.SingleSignOnServiceUrl,
            right.AllowedClockSkew ?? left.AllowedClockSkew,
            right.AttributeConsumingServiceIndex ?? left.AttributeConsumingServiceIndex);

    private static IdentityProviderMapperModel MergeIdentityProviderMapper(IdentityProviderMapperModel left, IdentityProviderMapperModel right) =>
        new(right.Id ?? left.Id,
            right.Name ?? left.Name,
            right.IdentityProviderAlias ?? left.IdentityProviderAlias,
            right.IdentityProviderMapper ?? left.IdentityProviderMapper,
            Merge(left.Config, right.Config, x => x.Key, (left, right) => right)?.ToDictionary());

    private static ComponentModel MergeComponent(ComponentModel left, ComponentModel right) =>
        new(right.Id ?? left.Id,
            right.Name ?? left.Name,
            right.ProviderId ?? left.ProviderId,
            right.SubType ?? left.SubType,
            right.SubComponents ?? left.SubComponents,
            Merge(left.Config, right.Config, x => x.Key, (left, right) => right)?.ToDictionary());

    private static AuthenticationFlowModel MergeAuthenticationFlow(AuthenticationFlowModel left, AuthenticationFlowModel right) =>
        new(right.Id ?? left.Id,
            right.Alias ?? left.Alias,
            right.Description ?? left.Description,
            right.ProviderId ?? left.ProviderId,
            right.TopLevel ?? left.TopLevel,
            right.BuiltIn ?? left.BuiltIn,
            Merge(left.AuthenticationExecutions, right.AuthenticationExecutions, x => x.Authenticator, MergeAuthenticationExecution));

    private static AuthenticationExecutionModel MergeAuthenticationExecution(AuthenticationExecutionModel left, AuthenticationExecutionModel right) =>
        new(right.Authenticator ?? left.Authenticator,
            right.AuthenticatorFlow ?? left.AuthenticatorFlow,
            right.Requirement ?? left.Requirement,
            right.Priority ?? left.Priority,
            right.UserSetupAllowed ?? left.UserSetupAllowed,
            right.AutheticatorFlow ?? left.AutheticatorFlow,
            right.FlowAlias ?? left.FlowAlias,
            right.AuthenticatorConfig ?? left.AuthenticatorConfig);

    private static AuthenticatorConfigModel MergeAuthenticatorConfig(AuthenticatorConfigModel left, AuthenticatorConfigModel right) =>
        new(right.Id ?? left.Id,
            right.Alias ?? left.Alias,
            Merge(left.Config, right.Config, x => x.Key, (left, right) => right)?.ToDictionary());

    private static RequiredActionModel MergeRequiredAction(RequiredActionModel left, RequiredActionModel right) =>
        new(right.Alias ?? left.Alias,
            right.Name ?? left.Name,
            right.ProviderId ?? left.ProviderId,
            right.Enabled ?? left.Enabled,
            right.DefaultAction ?? left.DefaultAction,
            right.Priority ?? left.Priority,
            right.Config ?? left.Config);

    private static ClientProfilesModel MergeClientProfiles(ClientProfilesModel left, ClientProfilesModel right) => right;

    private static ClientPoliciesModel MergeClientPolicies(ClientPoliciesModel left, ClientPoliciesModel right) => right;
}
