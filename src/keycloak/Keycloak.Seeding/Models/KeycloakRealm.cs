/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

public class KeycloakRealm
{
    public string? Id { get; set; }
    public string? Realm { get; set; }
    public string? DisplayName { get; set; }
    public int? NotBefore { get; set; }
    public string? DefaultSignatureAlgorithm { get; set; }
    public bool? RevokeRefreshToken { get; set; }
    public int? RefreshTokenMaxReuse { get; set; }
    public int? AccessTokenLifespan { get; set; }
    public int? AccessTokenLifespanForImplicitFlow { get; set; }
    public int? SsoSessionIdleTimeout { get; set; }
    public int? SsoSessionMaxLifespan { get; set; }
    public int? SsoSessionIdleTimeoutRememberMe { get; set; }
    public int? SsoSessionMaxLifespanRememberMe { get; set; }
    public int? OfflineSessionIdleTimeout { get; set; }
    public bool? OfflineSessionMaxLifespanEnabled { get; set; }
    public int? OfflineSessionMaxLifespan { get; set; }
    public int? ClientSessionIdleTimeout { get; set; }
    public int? ClientSessionMaxLifespan { get; set; }
    public int? ClientOfflineSessionIdleTimeout { get; set; }
    public int? ClientOfflineSessionMaxLifespan { get; set; }
    public int? AccessCodeLifespan { get; set; }
    public int? AccessCodeLifespanUserAction { get; set; }
    public int? AccessCodeLifespanLogin { get; set; }
    public int? ActionTokenGeneratedByAdminLifespan { get; set; }
    public int? ActionTokenGeneratedByUserLifespan { get; set; }
    public int? Oauth2DeviceCodeLifespan { get; set; }
    public int? Oauth2DevicePollingInterval { get; set; }
    public bool? Enabled { get; set; }
    public string? SslRequired { get; set; }
    public bool? RegistrationAllowed { get; set; }
    public bool? RegistrationEmailAsUsername { get; set; }
    public bool? RememberMe { get; set; }
    public bool? VerifyEmail { get; set; }
    public bool? LoginWithEmailAllowed { get; set; }
    public bool? DuplicateEmailsAllowed { get; set; }
    public bool? ResetPasswordAllowed { get; set; }
    public bool? EditUsernameAllowed { get; set; }
    public bool? BruteForceProtected { get; set; }
    public bool? PermanentLockout { get; set; }
    public int? MaxFailureWaitSeconds { get; set; }
    public int? MinimumQuickLoginWaitSeconds { get; set; }
    public int? WaitIncrementSeconds { get; set; }
    public int? QuickLoginCheckMilliSeconds { get; set; }
    public int? MaxDeltaTimeSeconds { get; set; }
    public int? FailureFactor { get; set; }
    public RolesModel? Roles { get; set; }
    public IReadOnlyList<GroupModel>? Groups { get; set; }
    public RoleModel? DefaultRole { get; set; }
    public IReadOnlyList<string>? DefaultGroups { get; set; }
    public IReadOnlyList<string>? RequiredCredentials { get; set; }
    public string? OtpPolicyType { get; set; }
    public string? OtpPolicyAlgorithm { get; set; }
    public int? OtpPolicyInitialCounter { get; set; }
    public int? OtpPolicyDigits { get; set; }
    public int? OtpPolicyLookAheadWindow { get; set; }
    public int? OtpPolicyPeriod { get; set; }
    public IReadOnlyList<string>? OtpSupportedApplications { get; set; }
    public string? WebAuthnPolicyRpEntityName { get; set; }
    public IReadOnlyList<string>? WebAuthnPolicySignatureAlgorithms { get; set; }
    public string? WebAuthnPolicyRpId { get; set; }
    public string? WebAuthnPolicyAttestationConveyancePreference { get; set; }
    public string? WebAuthnPolicyAuthenticatorAttachment { get; set; }
    public string? WebAuthnPolicyRequireResidentKey { get; set; }
    public string? WebAuthnPolicyUserVerificationRequirement { get; set; }
    public int? WebAuthnPolicyCreateTimeout { get; set; }
    public bool? WebAuthnPolicyAvoidSameAuthenticatorRegister { get; set; }
    public IReadOnlyList<string>? WebAuthnPolicyAcceptableAaguids { get; set; }
    public string? WebAuthnPolicyPasswordlessRpEntityName { get; set; }
    public IReadOnlyList<string>? WebAuthnPolicyPasswordlessSignatureAlgorithms { get; set; }
    public string? WebAuthnPolicyPasswordlessRpId { get; set; }
    public string? WebAuthnPolicyPasswordlessAttestationConveyancePreference { get; set; }
    public string? WebAuthnPolicyPasswordlessAuthenticatorAttachment { get; set; }
    public string? WebAuthnPolicyPasswordlessRequireResidentKey { get; set; }
    public string? WebAuthnPolicyPasswordlessUserVerificationRequirement { get; set; }
    public int? WebAuthnPolicyPasswordlessCreateTimeout { get; set; }
    public bool? WebAuthnPolicyPasswordlessAvoidSameAuthenticatorRegister { get; set; }
    public IReadOnlyList<string>? WebAuthnPolicyPasswordlessAcceptableAaguids { get; set; }
    public IReadOnlyList<UserModel>? Users { get; set; }
    public IReadOnlyList<ScopeMappingModel>? ScopeMappings { get; set; }
    public IReadOnlyDictionary<string, IReadOnlyList<ClientScopeMappingModel>>? ClientScopeMappings { get; set; }
    public IReadOnlyList<ClientModel>? Clients { get; set; }
    public IReadOnlyList<ClientScopeModel>? ClientScopes { get; set; }
    public IReadOnlyList<string>? DefaultDefaultClientScopes { get; set; }
    public IReadOnlyList<string>? DefaultOptionalClientScopes { get; set; }
    public BrowserSecurityHeadersModel? BrowserSecurityHeaders { get; set; }
    public SmtpServerModel? SmtpServer { get; set; }
    public string? LoginTheme { get; set; }
    public bool? EventsEnabled { get; set; }
    public IReadOnlyList<string>? EventsListeners { get; set; }
    public IReadOnlyList<string>? EnabledEventTypes { get; set; }
    public bool? AdminEventsEnabled { get; set; }
    public bool? AdminEventsDetailsEnabled { get; set; }
    public IReadOnlyList<IdentityProviderModel>? IdentityProviders { get; set; }
    public IReadOnlyList<IdentityProviderMapperModel>? IdentityProviderMappers { get; set; }
    public IReadOnlyDictionary<string, IReadOnlyList<ComponentModel>>? Components { get; set; }
    public bool? InternationalizationEnabled { get; set; }
    public IReadOnlyList<string>? SupportedLocales { get; set; }
    public string? DefaultLocale { get; set; }
    public IReadOnlyList<AuthenticationFlowModel>? AuthenticationFlows { get; set; }
    public IReadOnlyList<AuthenticatorConfigModel>? AuthenticatorConfig { get; set; }
    public IReadOnlyList<RequiredActionModel>? RequiredActions { get; set; }
    public string? BrowserFlow { get; set; }
    public string? RegistrationFlow { get; set; }
    public string? DirectGrantFlow { get; set; }
    public string? ResetCredentialsFlow { get; set; }
    public string? ClientAuthenticationFlow { get; set; }
    public string? DockerAuthenticationFlow { get; set; }
    public IReadOnlyDictionary<string, string>? Attributes { get; set; }
    public string? KeycloakVersion { get; set; }
    public bool? UserManagedAccessAllowed { get; set; }
    public ClientProfilesModel? ClientProfiles { get; set; }
    public ClientPoliciesModel? ClientPolicies { get; set; }
}

public record RolesModel(
    IReadOnlyList<RoleModel>? Realm,
    IReadOnlyDictionary<string, IReadOnlyList<RoleModel>>? Client
);

public record CompositeRolesModel(
    IReadOnlyList<string>? Realm,
    IReadOnlyDictionary<string, IReadOnlyList<string>>? Client
);

public record RoleModel(
    string? Id,
    string? Name,
    string? Description,
    bool? Composite,
    bool? ClientRole,
    string? ContainerId,
    IReadOnlyDictionary<string, IReadOnlyList<string>>? Attributes,
    CompositeRolesModel? Composites
);

public record UserModel(
    string? Id,
    long? CreatedTimestamp,
    string? Username,
    bool? Enabled,
    bool? Totp,
    bool? EmailVerified,
    string? FirstName,
    string? LastName,
    string? Email,
    IReadOnlyDictionary<string, IReadOnlyList<string>>? Attributes,
    IReadOnlyList<object>? Credentials,
    IReadOnlyList<string>? DisableableCredentialTypes,
    IReadOnlyList<string>? RequiredActions,
    IReadOnlyList<FederatedIdentityModel>? FederatedIdentities,
    IReadOnlyList<string>? RealmRoles,
    IReadOnlyDictionary<string, IReadOnlyList<string>>? ClientRoles,
    long? NotBefore,
    IReadOnlyList<string>? Groups,
    string? ServiceAccountClientId
);

public record FederatedIdentityModel(
    string? IdentityProvider,
    string? UserId,
    string? UserName
);

public record GroupModel(
    string? Id,
    string? Name,
    string? Path,
    IReadOnlyDictionary<string, IReadOnlyList<string>>? Attributes,
    IReadOnlyList<string>? RealmRoles,
    IReadOnlyDictionary<string, IReadOnlyList<string>>? ClientRoles,
    IReadOnlyList<string>? SubGroups
);

public record ScopeMappingModel(
    string? ClientScope,
    IReadOnlyList<string>? Roles
);

public record ClientScopeMappingModel(
    string? Client,
    IReadOnlyList<string>? Roles
);

public record ClientModel(
    string? Id,
    string? ClientId,
    string? Name,
    string? RootUrl,
    string? BaseUrl,
    bool? SurrogateAuthRequired,
    bool? Enabled,
    bool? AlwaysDisplayInConsole,
    string? ClientAuthenticatorType,
    IReadOnlyList<string>? RedirectUris,
    IReadOnlyList<string>? WebOrigins,
    int? NotBefore,
    bool? BearerOnly,
    bool? ConsentRequired,
    bool? StandardFlowEnabled,
    bool? ImplicitFlowEnabled,
    bool? DirectAccessGrantsEnabled,
    bool? ServiceAccountsEnabled,
    bool? PublicClient,
    bool? FrontchannelLogout,
    string? Protocol,
    IReadOnlyDictionary<string, string>? Attributes,
    IReadOnlyDictionary<string, string>? AuthenticationFlowBindingOverrides,
    bool? FullScopeAllowed,
    int? NodeReRegistrationTimeout,
    IReadOnlyList<string>? DefaultClientScopes,
    IReadOnlyList<string>? OptionalClientScopes,
    IReadOnlyList<ProtocolMapperModel>? ProtocolMappers,
    ClientAccessModel? Access,
    string? Secret,
    string? AdminUrl,
    string? Description,
    bool? AuthorizationServicesEnabled
);

public record ClientAccessModel(
    bool? Configure,
    bool? Manage,
    bool? View
);

public record ProtocolMapperModel(
    string? Id,
    string? Name,
    string? Protocol,
    string? ProtocolMapper,
    bool? ConsentRequired,
    IReadOnlyDictionary<string, string>? Config
);

public record ClientScopeModel(
    string? Id,
    string? Name,
    string? Protocol,
    IReadOnlyDictionary<string, string>? Attributes,
    IReadOnlyList<ProtocolMapperModel>? ProtocolMappers,
    string? Description
);

public record BrowserSecurityHeadersModel(
    string? ContentSecurityPolicyReportOnly,
    string? XContentTypeOptions,
    string? XRobotsTag,
    string? XFrameOptions,
    string? ContentSecurityPolicy,
    string? XXSSProtection,
    string? StrictTransportSecurity
);

public record SmtpServerModel(
    string? Password,
    string? Starttls,
    string? Auth,
    string? Port,
    string? Host,
    string? ReplyToDisplayName,
    string? ReplyTo,
    string? FromDisplayName,
    string? From,
    string? EnvelopeFrom,
    string? Ssl,
    string? User
);

public record IdentityProviderModel(
    string? Alias,
    string? DisplayName,
    string? InternalId,
    string? ProviderId,
    bool? Enabled,
    string? UpdateProfileFirstLoginMode,
    bool? TrustEmail,
    bool? StoreToken,
    bool? AddReadTokenRoleOnCreate,
    bool? AuthenticateByDefault,
    bool? LinkOnly,
    string? FirstBrokerLoginFlowAlias,
    IdentityProviderConfigModel? Config
);

public record IdentityProviderConfigModel(
    string? HideOnLoginPage,
    string? ValidateSignature,
    string? ClientId,
    string? TokenUrl,
    string? AuthorizationUrl,
    string? ClientAuthMethod,
    string? JwksUrl,
    string? LogoutUrl,
    string? ClientAssertionSigningAlg,
    string? SyncMode,
    string? UseJwksUrl
);

public record IdentityProviderMapperModel(
    string? Id,
    string? Name,
    string? IdentityProviderAlias,
    string? IdentityProviderMapper,
    IReadOnlyDictionary<string, string>? Config
);

public record ComponentModel(
    string? Id,
    string? Name,
    string? ProviderId,
    string? SubType,
    object? SubComponents,
    IReadOnlyDictionary<string, IReadOnlyList<string>>? Config
);

public record AuthenticationFlowModel(
    string? Id,
    string? Alias,
    string? Description,
    string? ProviderId,
    bool? TopLevel,
    bool? BuiltIn,
    IReadOnlyList<AuthenticationExecutionModel>? AuthenticationExecutions
);

public record AuthenticationExecutionModel(
    string? Authenticator,
    bool? AuthenticatorFlow,
    string? Requirement,
    int? Priority,
    bool? UserSetupAllowed,
    bool? AutheticatorFlow,
    string? FlowAlias,
    string? AuthenticatorConfig
);

public record AuthenticatorConfigModel(
    string? Id,
    string? Alias,
    IReadOnlyDictionary<string, string>? Config
);

public record RequiredActionModel(
    string? Alias,
    string? Name,
    string? ProviderId,
    bool? Enabled,
    bool? DefaultAction,
    int? Priority,
    object? Config
);

public record ClientProfilesModel(
    IReadOnlyList<object>? Profiles
);

public record ClientPoliciesModel(
    IReadOnlyList<object>? Policies
);
