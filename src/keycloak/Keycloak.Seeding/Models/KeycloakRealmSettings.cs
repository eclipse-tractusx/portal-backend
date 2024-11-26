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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

public class KeycloakRealmSettings
{
    [Required]
    public string Realm { get; set; } = null!;
    [Required]
    public string InstanceName { get; set; } = null!;
    [Required]
    [DistinctValues]
    public IEnumerable<string> DataPaths { get; set; } = null!;
    public bool Create { get; set; }
    public bool Update { get; set; }
    public bool Delete { get; set; }
    [DistinctValues("x => x.Key")]
    public IEnumerable<SeederConfiguration>? SeederConfigurations { get; set; }
    public string? Id { get; set; }
    public string? DisplayName { get; set; }
    public string? DisplayNameHtml { get; set; }
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
    public RolesSettings? Roles { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<GroupSettings>? Groups { get; set; }
    public RoleSettings? DefaultRole { get; set; }
    [DistinctValues]
    public IEnumerable<string>? DefaultGroups { get; set; }
    [DistinctValues]
    public IEnumerable<string>? RequiredCredentials { get; set; }
    public string? OtpPolicyType { get; set; }
    public string? OtpPolicyAlgorithm { get; set; }
    public int? OtpPolicyInitialCounter { get; set; }
    public int? OtpPolicyDigits { get; set; }
    public int? OtpPolicyLookAheadWindow { get; set; }
    public int? OtpPolicyPeriod { get; set; }
    [DistinctValues]
    public IEnumerable<string>? OtpSupportedApplications { get; set; }
    public string? PasswordPolicy { get; set; }
    public IDictionary<string, IDictionary<string, string>?>? LocalizationTexts { get; set; }
    public string? WebAuthnPolicyRpEntityName { get; set; }
    [DistinctValues]
    public IEnumerable<string>? WebAuthnPolicySignatureAlgorithms { get; set; }
    public string? WebAuthnPolicyRpId { get; set; }
    public string? WebAuthnPolicyAttestationConveyancePreference { get; set; }
    public string? WebAuthnPolicyAuthenticatorAttachment { get; set; }
    public string? WebAuthnPolicyRequireResidentKey { get; set; }
    public string? WebAuthnPolicyUserVerificationRequirement { get; set; }
    public int? WebAuthnPolicyCreateTimeout { get; set; }
    public bool? WebAuthnPolicyAvoidSameAuthenticatorRegister { get; set; }
    [DistinctValues]
    public IEnumerable<string>? WebAuthnPolicyAcceptableAaguids { get; set; }
    public string? WebAuthnPolicyPasswordlessRpEntityName { get; set; }
    [DistinctValues]
    public IEnumerable<string>? WebAuthnPolicyPasswordlessSignatureAlgorithms { get; set; }
    public string? WebAuthnPolicyPasswordlessRpId { get; set; }
    public string? WebAuthnPolicyPasswordlessAttestationConveyancePreference { get; set; }
    public string? WebAuthnPolicyPasswordlessAuthenticatorAttachment { get; set; }
    public string? WebAuthnPolicyPasswordlessRequireResidentKey { get; set; }
    public string? WebAuthnPolicyPasswordlessUserVerificationRequirement { get; set; }
    public int? WebAuthnPolicyPasswordlessCreateTimeout { get; set; }
    public bool? WebAuthnPolicyPasswordlessAvoidSameAuthenticatorRegister { get; set; }
    [DistinctValues]
    public IEnumerable<string>? WebAuthnPolicyPasswordlessAcceptableAaguids { get; set; }
    [DistinctValues("x => x.Username")]
    public IEnumerable<UserSettings>? Users { get; set; }
    [DistinctValues("x => x.ClientScope")]
    public IEnumerable<ScopeMappingSettings>? ScopeMappings { get; set; }
    [DistinctValues("x => x.ClientId")]
    public IEnumerable<ClientScopeMappingSettingsEntry>? ClientScopeMappings { get; set; }
    [DistinctValues("x => x.ClientId")]
    public IEnumerable<ClientSettings>? Clients { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<ClientScopeSettings>? ClientScopes { get; set; }
    [DistinctValues]
    public IEnumerable<string>? DefaultDefaultClientScopes { get; set; }
    [DistinctValues]
    public IEnumerable<string>? DefaultOptionalClientScopes { get; set; }
    public BrowserSecurityHeadersSettings? BrowserSecurityHeaders { get; set; }
    public SmtpServerSettings? SmtpServer { get; set; }
    public string? LoginTheme { get; set; }
    public string? AccountTheme { get; set; }
    public string? AdminTheme { get; set; }
    public string? EmailTheme { get; set; }
    public bool? EventsEnabled { get; set; }
    [DistinctValues]
    public IEnumerable<string>? EventsListeners { get; set; }
    [DistinctValues]
    public IEnumerable<string>? EnabledEventTypes { get; set; }
    public bool? AdminEventsEnabled { get; set; }
    public bool? AdminEventsDetailsEnabled { get; set; }
    [DistinctValues("x => x.Alias")]
    public IEnumerable<IdentityProviderSettings>? IdentityProviders { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<IdentityProviderMapperSettings>? IdentityProviderMappers { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<ComponentSettingsEntry>? Components { get; set; }
    public bool? InternationalizationEnabled { get; set; }
    [DistinctValues]
    public IEnumerable<string>? SupportedLocales { get; set; }
    public string? DefaultLocale { get; set; }
    [DistinctValues("x => x.Alias")]
    public IEnumerable<AuthenticationFlowSettings>? AuthenticationFlows { get; set; }
    [DistinctValues("x => x.Alias")]
    public IEnumerable<AuthenticatorConfigSettings>? AuthenticatorConfig { get; set; }
    [DistinctValues("x => x.Alias")]
    public IEnumerable<RequiredActionSettings>? RequiredActions { get; set; }
    public string? BrowserFlow { get; set; }
    public string? RegistrationFlow { get; set; }
    public string? DirectGrantFlow { get; set; }
    public string? ResetCredentialsFlow { get; set; }
    public string? ClientAuthenticationFlow { get; set; }
    public string? DockerAuthenticationFlow { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<AttributeSettings>? Attributes { get; set; }
    public string? KeycloakVersion { get; set; }
    public bool? UserManagedAccessAllowed { get; set; }
    public ClientProfilesSettings? ClientProfiles { get; set; }
    public ClientPoliciesSettings? ClientPolicies { get; set; }
}

public class AttributeSettings
{
    [Required]
    public string? Name { get; set; }
    public string? Value { get; set; }
}

public class MultiValueAttributeSettings
{
    [Required]
    public string? Name { get; set; }
    [DistinctValues]
    public IEnumerable<string>? Values { get; set; }
}

public class RolesSettings
{
    [DistinctValues("x => x.Name")]
    public IEnumerable<RoleSettings>? Realm { get; set; }
    [DistinctValues("x => x.ClientId")]
    public IEnumerable<ClientRoleSettings>? Client { get; set; }
};

public class ClientRoleSettings
{
    [Required]
    public string? ClientId { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<RoleSettings>? Roles { get; set; }
}

public class CompositeRolesSettings
{
    public IEnumerable<string>? Realm { get; set; }
    [DistinctValues("x => x.ClientId")]
    public IEnumerable<CompositeClientRolesSettings>? Client { get; set; }
};

public class CompositeClientRolesSettings
{
    [Required]
    public string? ClientId { get; set; }
    [DistinctValues]
    public IEnumerable<string>? Roles { get; set; }
}

public class RoleSettings
{
    public string? Id { get; set; }
    [Required]
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? Composite { get; set; }
    public bool? ClientRole { get; set; }
    public string? ContainerId { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<MultiValueAttributeSettings>? Attributes { get; set; }
    public CompositeRolesSettings? Composites { get; set; }
}

public class UserSettings
{
    public string? Id { get; set; }
    public long? CreatedTimestamp { get; set; }
    [Required]
    public string? Username { get; set; }
    public bool? Enabled { get; set; }
    public bool? Totp { get; set; }
    public bool? EmailVerified { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<MultiValueAttributeSettings>? Attributes { get; set; }
    public IEnumerable<CredentialsSettings>? Credentials { get; set; }
    [DistinctValues]
    public IEnumerable<string>? DisableableCredentialTypes { get; set; }
    [DistinctValues]
    public IEnumerable<string>? RequiredActions { get; set; }
    [DistinctValues("x => x.IdentityProvider")]
    public IEnumerable<FederatedIdentitySettings>? FederatedIdentities { get; set; }
    [DistinctValues]
    public IEnumerable<string>? RealmRoles { get; set; }
    [DistinctValues("x => x.ClientId")]
    public IEnumerable<UserClientRolesSettings>? ClientRoles { get; set; }
    public int? NotBefore { get; set; }
    [DistinctValues]
    public IEnumerable<string>? Groups { get; set; }
    public string? ServiceAccountClientId { get; set; }
    public UserAccessSettings? Access { get; set; }
    public IEnumerable<ClientConsentSettings>? ClientConsents { get; set; }
    public string? FederationLink { get; set; }
    public string? Origin { get; set; }
    public string? Self { get; set; }
}

public class CredentialsSettings
{
    public string? Algorithm { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<CredentialsConfigSettings>? Config { get; set; }
    public int? Counter { get; set; }
    public long? CreatedDate { get; set; }
    public string? Device { get; set; }
    public int? Digits { get; set; }
    public int? HashIterations { get; set; }
    public string? HashSaltedValue { get; set; }
    public int? Period { get; set; }
    public string? Salt { get; set; }
    public bool? Temporary { get; set; }
    public string? Type { get; set; }
    public string? Value { get; set; }
}

public class CredentialsConfigSettings
{
    [Required]
    public string? Name { get; set; }
    public string? Value { get; set; }
}

public class UserClientRolesSettings
{
    [Required]
    public string? ClientId { get; set; }
    [DistinctValues]
    public IEnumerable<string>? Roles { get; set; }
}

public class FederatedIdentitySettings
{
    [Required]
    public string? IdentityProvider { get; set; }
    [Required]
    public string? UserId { get; set; }
    [Required]
    public string? UserName { get; set; }
}

public class UserAccessSettings
{
    public bool? ManageGroupMembership { get; set; }
    public bool? View { get; set; }
    public bool? MapRoles { get; set; }
    public bool? Impersonate { get; set; }
    public bool? Manage { get; set; }
}

public class ClientConsentSettings
{
    [Required]
    public string? ClientId { get; set; }
    public IEnumerable<string>? GrantedClientScopes { get; set; }
    public long? CreatedDate { get; set; }
    public long? LastUpdatedDate { get; set; }
}

public class GroupSettings
{
    public string? Id { get; set; }
    [Required]
    public string? Name { get; set; }
    public string? Path { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<MultiValueAttributeSettings>? Attributes { get; set; }
    [DistinctValues]
    public IEnumerable<string>? RealmRoles { get; set; }
    [DistinctValues("x => x.ClientId")]
    public IEnumerable<UserClientRolesSettings>? ClientRoles { get; set; }
    [DistinctValues]
    public IEnumerable<string>? SubGroups { get; set; }
}

public class ScopeMappingSettings
{
    [Required]
    public string? ClientScope { get; set; }
    [DistinctValues]
    public IEnumerable<string>? Roles { get; set; }
}

public class ClientScopeMappingSettings
{
    [Required]
    public string? Client { get; set; }
    [DistinctValues]
    public IEnumerable<string>? Roles { get; set; }
}

public class ClientScopeMappingSettingsEntry
{
    [Required]
    public string? ClientId { get; set; }
    [DistinctValues("x => x.Client")]
    public IEnumerable<ClientScopeMappingSettings>? ClientScopeMappings { get; set; }
}

public class ClientSettings
{
    public string? Id { get; set; }
    [Required]
    public string? ClientId { get; set; }
    public string? Name { get; set; }
    public string? RootUrl { get; set; }
    public string? BaseUrl { get; set; }
    public bool? SurrogateAuthRequired { get; set; }
    public bool? Enabled { get; set; }
    public bool? AlwaysDisplayInConsole { get; set; }
    public string? ClientAuthenticatorType { get; set; }
    [DistinctValues]
    public IEnumerable<string>? RedirectUris { get; set; }
    [DistinctValues]
    public IEnumerable<string>? WebOrigins { get; set; }
    public int? NotBefore { get; set; }
    public bool? BearerOnly { get; set; }
    public bool? ConsentRequired { get; set; }
    public bool? StandardFlowEnabled { get; set; }
    public bool? ImplicitFlowEnabled { get; set; }
    public bool? DirectAccessGrantsEnabled { get; set; }
    public bool? ServiceAccountsEnabled { get; set; }
    public bool? PublicClient { get; set; }
    public bool? FrontchannelLogout { get; set; }
    public string? Protocol { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<ClientAttributeSettings>? Attributes { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<AuthenticationFlowBindingOverrideSettings>? AuthenticationFlowBindingOverrides { get; set; }
    public bool? FullScopeAllowed { get; set; }
    public int? NodeReRegistrationTimeout { get; set; }
    [DistinctValues]
    public IEnumerable<string>? DefaultClientScopes { get; set; }
    [DistinctValues]
    public IEnumerable<string>? OptionalClientScopes { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<ProtocolMapperSettings>? ProtocolMappers { get; set; }
    public ClientAccessSettings? Access { get; set; }
    public string? Secret { get; set; }
    public string? AdminUrl { get; set; }
    public string? Description { get; set; }
    public bool? AuthorizationServicesEnabled { get; set; }
}

public class ClientAttributeSettings
{
    [Required]
    public string? Name { get; set; }
    public string? Value { get; set; }
}

public class AuthenticationFlowBindingOverrideSettings
{
    [Required]
    public string? Name { get; set; }
    public string? Value { get; set; }
}

public class ClientAccessSettings
{
    public bool? Configure { get; set; }
    public bool? Manage { get; set; }
    public bool? View { get; set; }
}

public class ProtocolMapperSettings
{
    public string? Id { get; set; }
    [Required]
    public string? Name { get; set; }
    public string? Protocol { get; set; }
    public string? ProtocolMapper { get; set; }
    public bool? ConsentRequired { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<ProtocolMapperConfigSettings>? Config { get; set; }
}

public class ProtocolMapperConfigSettings
{
    [Required]
    public string? Name { get; set; }
    public string? Value { get; set; }
}

public class ClientScopeSettings
{
    public string? Id { get; set; }
    [Required]
    public string? Name { get; set; }
    public string? Protocol { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<ClientAttributeSettings>? Attributes { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<ProtocolMapperSettings>? ProtocolMappers { get; set; }
    public string? Description { get; set; }
}

public class BrowserSecurityHeadersSettings
{
    public string? ContentSecurityPolicyReportOnly { get; set; }
    public string? XContentTypeOptions { get; set; }
    public string? XRobotsTag { get; set; }
    public string? XFrameOptions { get; set; }
    public string? ContentSecurityPolicy { get; set; }
    [property: JsonPropertyName("xXSSProtection")]
    public string? XXSSProtection { get; set; }
    public string? StrictTransportSecurity { get; set; }
}

public class SmtpServerSettings
{
    public string? Password { get; set; }
    public string? Starttls { get; set; }
    public string? Auth { get; set; }
    public string? Port { get; set; }
    public string? Host { get; set; }
    public string? ReplyToDisplayName { get; set; }
    public string? ReplyTo { get; set; }
    public string? FromDisplayName { get; set; }
    public string? From { get; set; }
    public string? EnvelopeFrom { get; set; }
    public string? Ssl { get; set; }
    public string? User { get; set; }
}

public class IdentityProviderSettings
{
    [Required]
    public string? Alias { get; set; }
    public string? DisplayName { get; set; }
    public string? InternalId { get; set; }
    public string? ProviderId { get; set; }
    public bool? Enabled { get; set; }
    public string? UpdateProfileFirstLoginMode { get; set; }
    public bool? TrustEmail { get; set; }
    public bool? StoreToken { get; set; }
    public bool? AddReadTokenRoleOnCreate { get; set; }
    public bool? AuthenticateByDefault { get; set; }
    public bool? LinkOnly { get; set; }
    public string? FirstBrokerLoginFlowAlias { get; set; }
    public string? PostBrokerLoginFlowAlias { get; set; }
    public IdentityProviderConfigSettings? Config { get; set; }
}

public class IdentityProviderConfigSettings
{
    public string? HideOnLoginPage { get; set; }
    public string? ClientSecret { get; set; }
    public string? DisableUserInfo { get; set; }
    public string? ValidateSignature { get; set; }
    public string? ClientId { get; set; }
    public string? TokenUrl { get; set; }
    public string? AuthorizationUrl { get; set; }
    public string? ClientAuthMethod { get; set; }
    public string? JwksUrl { get; set; }
    public string? LogoutUrl { get; set; }
    public string? ClientAssertionSigningAlg { get; set; }
    public string? SyncMode { get; set; }
    public string? UseJwksUrl { get; set; }
    public string? UserInfoUrl { get; set; }
    public string? Issuer { get; set; }
    // for Saml:
    public string? NameIDPolicyFormat { get; set; }
    public string? PrincipalType { get; set; }
    public string? SignatureAlgorithm { get; set; }
    public string? XmlSigKeyInfoKeyNameTransformer { get; set; }
    public string? AllowCreate { get; set; }
    public string? EntityId { get; set; }
    public string? AuthnContextComparisonType { get; set; }
    public string? BackchannelSupported { get; set; }
    public string? PostBindingResponse { get; set; }
    public string? PostBindingAuthnRequest { get; set; }
    public string? PostBindingLogout { get; set; }
    public string? WantAuthnRequestsSigned { get; set; }
    public string? WantAssertionsSigned { get; set; }
    public string? WantAssertionsEncrypted { get; set; }
    public string? ForceAuthn { get; set; }
    public string? SignSpMetadata { get; set; }
    public string? LoginHint { get; set; }
    public string? SingleSignOnServiceUrl { get; set; }
    public string? AllowedClockSkew { get; set; }
    public string? AttributeConsumingServiceIndex { get; set; }
}

public class IdentityProviderMapperSettings
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? IdentityProviderAlias { get; set; }
    public string? IdentityProviderMapper { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<IdentityProviderMapperConfigSettings>? Config { get; set; }
}

public class IdentityProviderMapperConfigSettings
{
    [Required]
    public string? Name { get; set; }
    public string? Value { get; set; }
}

public class ComponentSettings
{
    public string? Id { get; set; }
    [Required]
    public string? Name { get; set; }
    public string? ProviderId { get; set; }
    public string? SubType { get; set; }
    public object? SubComponents { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<ComponentConfigSettings>? Config { get; set; }
}

public class ComponentSettingsEntry
{
    [Required]
    public string? Name { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<ComponentSettings>? ComponentSettings { get; set; }
}

public class ComponentConfigSettings
{
    [Required]
    public string? Name { get; set; }
    [DistinctValues]
    public IEnumerable<string>? Values { get; set; }
}

public class AuthenticationFlowSettings
{
    public string? Id { get; set; }
    [Required]
    public string? Alias { get; set; }
    public string? Description { get; set; }
    public string? ProviderId { get; set; }
    public bool? TopLevel { get; set; }
    public bool? BuiltIn { get; set; }
    [DistinctValues("x => x.Authenticator")]
    public IEnumerable<AuthenticationExecutionSettings>? AuthenticationExecutions { get; set; }
}

public class AuthenticationExecutionSettings
{
    public string? Authenticator { get; set; }
    public bool? AuthenticatorFlow { get; set; }
    public string? Requirement { get; set; }
    public int? Priority { get; set; }
    public bool? UserSetupAllowed { get; set; }
    public bool? AutheticatorFlow { get; set; }
    public string? FlowAlias { get; set; }
    public string? AuthenticatorConfig { get; set; }
}

public class AuthenticatorConfigSettings
{
    public string? Id { get; set; }
    public string? Alias { get; set; }
    [DistinctValues("x => x.Name")]
    public IEnumerable<AuthenticatorConfigConfigSettings>? Config { get; set; }
}

public class AuthenticatorConfigConfigSettings
{
    [Required]
    public string? Name { get; set; }
    public string? Value { get; set; }
}

public class RequiredActionSettings
{
    public string? Alias { get; set; }
    public string? Name { get; set; }
    public string? ProviderId { get; set; }
    public bool? Enabled { get; set; }
    public bool? DefaultAction { get; set; }
    public int? Priority { get; set; }
    public object? Config { get; set; }
}

public class ClientProfilesSettings
{
    public IEnumerable<object>? Profiles { get; set; }
}

public class ClientPoliciesSettings
{
    public IEnumerable<object>? Policies { get; set; }
}
