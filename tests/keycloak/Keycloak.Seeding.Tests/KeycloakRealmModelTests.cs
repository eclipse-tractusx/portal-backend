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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;
namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Tests;

public class KeycloakRealmModelTests
{
    [Fact]
    public async Task SeedDataHandlerImportsExpected()
    {
        // Arrange
        var sut = new SeedDataHandler();

        // Act
        await sut.Import("TestSeeds/test-realm.json", CancellationToken.None).ConfigureAwait(false);

        var keycloakRealm = sut.KeycloakRealm;
        var clients = sut.Clients;
        var clientRoles = sut.ClientRoles;
        var realmRoles = sut.RealmRoles;
        var identityProviders = sut.IdentityProviders;
        var identityProviderMappers = sut.IdentityProviderMappers;
        var users = sut.Users;
        var authenticationFlows = sut.TopLevelCustomAuthenticationFlows;
        var clientScopes = sut.ClientScopes;

        // Assert
        keycloakRealm.Should().NotBeNull()
            .And.Match<KeycloakRealm>(x =>
                x.Id == "TestRealmId" &&
                x.Realm == "TestRealm" &&
                x.DisplayName == "TestRealm Display Name" &&
                x.DisplayNameHtml == "TestRealm HTML Display Name" &&
                x.NotBefore == 0 &&
                x.DefaultSignatureAlgorithm == "RS256" &&
                x.RevokeRefreshToken == false &&
                x.RefreshTokenMaxReuse == 0 &&
                x.AccessTokenLifespan == 300 &&
                x.AccessTokenLifespanForImplicitFlow == 900 &&
                x.SsoSessionIdleTimeout == 1800 &&
                x.SsoSessionMaxLifespan == 36000 &&
                x.SsoSessionIdleTimeoutRememberMe == 0 &&
                x.SsoSessionMaxLifespanRememberMe == 0 &&
                x.OfflineSessionIdleTimeout == 2592000 &&
                x.OfflineSessionMaxLifespanEnabled == false &&
                x.OfflineSessionMaxLifespan == 5184000 &&
                x.ClientSessionIdleTimeout == 0 &&
                x.ClientSessionMaxLifespan == 0 &&
                x.ClientOfflineSessionIdleTimeout == 0 &&
                x.ClientOfflineSessionMaxLifespan == 0 &&
                x.AccessCodeLifespan == 60 &&
                x.AccessCodeLifespanUserAction == 300 &&
                x.AccessCodeLifespanLogin == 1800 &&
                x.ActionTokenGeneratedByAdminLifespan == 43200 &&
                x.ActionTokenGeneratedByUserLifespan == 300 &&
                x.Oauth2DeviceCodeLifespan == 600 &&
                x.Oauth2DevicePollingInterval == 5 &&
                x.Enabled == true &&
                x.SslRequired == "external" &&
                x.RegistrationAllowed == false &&
                x.RegistrationEmailAsUsername == false &&
                x.RememberMe == false &&
                x.VerifyEmail == false &&
                x.LoginWithEmailAllowed == true &&
                x.DuplicateEmailsAllowed == false &&
                x.ResetPasswordAllowed == false &&
                x.EditUsernameAllowed == false &&
                x.BruteForceProtected == false &&
                x.PermanentLockout == false &&
                x.MaxFailureWaitSeconds == 900 &&
                x.MinimumQuickLoginWaitSeconds == 60 &&
                x.WaitIncrementSeconds == 60 &&
                x.QuickLoginCheckMilliSeconds == 1000 &&
                x.MaxDeltaTimeSeconds == 43200 &&
                x.FailureFactor == 30 &&
                x.Roles != null &&
                x.Roles.Client != null &&
                x.Roles.Client == clientRoles &&
                x.Roles.Realm != null &&
                x.Roles.Realm == realmRoles &&
                x.Groups != null &&
                // roles and groups are being asserted separately
                x.DefaultRole != null &&
                x.DefaultRole.Id == "fd20bacb-f39f-499c-8fc3-c3d14e0770d9" &&
                x.DefaultRole.Name == "default-roles-testrealm" &&
                x.DefaultRole.Description == "${role_default-roles}" &&
                x.DefaultRole.Composite == true &&
                x.DefaultRole.ClientRole == false &&
                x.DefaultRole.ContainerId == "TestRealm" &&
                x.RequiredCredentials != null &&
                x.RequiredCredentials.SequenceEqual(new[] { "password" }) &&
                x.OtpPolicyType == "totp" &&
                x.OtpPolicyAlgorithm == "HmacSHA1" &&
                x.OtpPolicyInitialCounter == 0 &&
                x.OtpPolicyDigits == 6 &&
                x.OtpPolicyLookAheadWindow == 1 &&
                x.OtpPolicyPeriod == 30 &&
                x.OtpSupportedApplications != null &&
                x.OtpSupportedApplications.SequenceEqual(new[] { "FreeOTP", "Google Authenticator" }) &&
                x.PasswordPolicy == "notUsername(undefined) and notEmail(undefined)" &&
                x.WebAuthnPolicyRpEntityName == "keycloak" &&
                x.WebAuthnPolicySignatureAlgorithms != null &&
                x.WebAuthnPolicySignatureAlgorithms.SequenceEqual(new[] { "ES256" }) &&
                x.WebAuthnPolicyRpId == "" &&
                x.WebAuthnPolicyAttestationConveyancePreference == "not specified" &&
                x.WebAuthnPolicyAuthenticatorAttachment == "not specified" &&
                x.WebAuthnPolicyRequireResidentKey == "not specified" &&
                x.WebAuthnPolicyUserVerificationRequirement == "not specified" &&
                x.WebAuthnPolicyCreateTimeout == 0 &&
                x.WebAuthnPolicyAvoidSameAuthenticatorRegister == false &&
                x.WebAuthnPolicyAcceptableAaguids != null &&
                x.WebAuthnPolicyAcceptableAaguids.SequenceEqual(Enumerable.Empty<string>()) &&
                x.WebAuthnPolicyPasswordlessRpEntityName == "keycloak" &&
                x.WebAuthnPolicyPasswordlessSignatureAlgorithms != null &&
                x.WebAuthnPolicyPasswordlessSignatureAlgorithms.SequenceEqual(new[] { "ES256" }) &&
                x.WebAuthnPolicyPasswordlessRpId == "" &&
                x.WebAuthnPolicyPasswordlessAttestationConveyancePreference == "not specified" &&
                x.WebAuthnPolicyPasswordlessAuthenticatorAttachment == "not specified" &&
                x.WebAuthnPolicyPasswordlessRequireResidentKey == "not specified" &&
                x.WebAuthnPolicyPasswordlessUserVerificationRequirement == "not specified" &&
                x.WebAuthnPolicyPasswordlessCreateTimeout == 0 &&
                x.WebAuthnPolicyPasswordlessAvoidSameAuthenticatorRegister == false &&
                x.WebAuthnPolicyPasswordlessAcceptableAaguids != null &&
                x.WebAuthnPolicyPasswordlessAcceptableAaguids.SequenceEqual(Enumerable.Empty<string>()) &&
                x.Users != null &&
                x.ScopeMappings != null &&
                x.ClientScopeMappings != null &&
                x.Clients != null &&
                x.Clients == clients &&
                x.ClientScopes != null &&
                x.ClientScopes == clientScopes &&
                // users, scopeMappings, clientScopeMappings, clients, clientScopes are being asserted separately
                x.DefaultDefaultClientScopes != null &&
                x.DefaultDefaultClientScopes.SequenceEqual(new[] { "role_list", "profile", "email", "roles", "web-origins" }) &&
                x.DefaultOptionalClientScopes != null &&
                x.DefaultOptionalClientScopes.SequenceEqual(new[] { "offline_access", "address", "phone", "microprofile-jwt" }) &&
                x.BrowserSecurityHeaders != null &&
                x.BrowserSecurityHeaders.ContentSecurityPolicyReportOnly == "" &&
                x.BrowserSecurityHeaders.XContentTypeOptions == "nosniff" &&
                x.BrowserSecurityHeaders.XRobotsTag == "none" &&
                x.BrowserSecurityHeaders.XFrameOptions == "SAMEORIGIN" &&
                x.BrowserSecurityHeaders.ContentSecurityPolicy == "frame-src 'self'; frame-ancestors 'self'; object-src 'none';" &&
                x.BrowserSecurityHeaders.XXSSProtection == "1; mode=block" &&
                x.BrowserSecurityHeaders.StrictTransportSecurity == "max-age=31536000; includeSubDomains" &&
                x.SmtpServer != null &&
                x.SmtpServer.ReplyToDisplayName == "Test Reply To Display Name" &&
                x.SmtpServer.Starttls == "true" &&
                x.SmtpServer.Auth == "true" &&
                x.SmtpServer.Ssl == "true" &&
                x.SmtpServer.EnvelopeFrom == "envelope-from@test.host" &&
                x.SmtpServer.Password == "**********" &&
                x.SmtpServer.Port == "25" &&
                x.SmtpServer.Host == "test.host" &&
                x.SmtpServer.ReplyTo == "reply-to@test.host" &&
                x.SmtpServer.From == "from@test.host" &&
                x.SmtpServer.FromDisplayName == "Test From Display Name" &&
                x.SmtpServer.User == "testSmtpLogin" &&
                x.LoginTheme == "base" &&
                x.AccountTheme == "base" &&
                x.AdminTheme == "base" &&
                x.EmailTheme == "base" &&
                x.EventsEnabled == false &&
                x.EventsListeners != null &&
                x.EventsListeners.SequenceEqual(new[] { "jboss-logging" }) &&
                x.EnabledEventTypes != null &&
                x.EnabledEventTypes.SequenceEqual(Enumerable.Empty<string>()) &&
                x.AdminEventsEnabled == false &&
                x.AdminEventsDetailsEnabled == false &&
                x.IdentityProviders != null &&
                x.IdentityProviders == identityProviders &&
                x.IdentityProviderMappers != null &&
                x.IdentityProviderMappers == identityProviderMappers &&
                // identityProviders, identityProviderMappers, components are being asserted separately
                x.InternationalizationEnabled == true &&
                x.SupportedLocales != null &&
                x.SupportedLocales.SequenceEqual(new[] { "de", "no", "ru", "sv", "pt-BR", "lt", "en", "it", "fr", "hu", "zh-CN", "es", "cs", "ja", "sk", "pl", "da", "ca", "nl", "tr" }) &&
                x.DefaultLocale == "en" &&
                // authenticationFlows, authenticatorConfigs, requiredActions are being asserted separately
                x.BrowserFlow == "browser" &&
                x.RegistrationFlow == "registration" &&
                x.DirectGrantFlow == "direct grant" &&
                x.ResetCredentialsFlow == "reset credentials" &&
                x.ClientAuthenticationFlow == "clients" &&
                x.DockerAuthenticationFlow == "docker auth" &&
                x.Attributes != null &&
                x.Attributes.SequenceEqual(new Dictionary<string, string> {
                    { "cibaBackchannelTokenDeliveryMode", "poll" },
                    { "cibaAuthRequestedUserHint", "login_hint" },
                    { "oauth2DevicePollingInterval", "5" },
                    { "clientOfflineSessionMaxLifespan", "0" },
                    { "clientSessionIdleTimeout", "0" },
                    { "userProfileEnabled", "false" },
                    { "clientOfflineSessionIdleTimeout", "0" },
                    { "cibaInterval", "5" },
                    { "cibaExpiresIn", "120" },
                    { "oauth2DeviceCodeLifespan", "600" },
                    { "parRequestUriLifespan", "60" },
                    { "clientSessionMaxLifespan", "0" },
                    { "frontendUrl", "http://frontend.url" }
                }) &&
                x.KeycloakVersion == "16.1.1" &&
                x.UserManagedAccessAllowed == false
            );

        keycloakRealm.Groups.Should().ContainSingle()
            .Which.Should().Match<GroupModel>(x =>
                x.Id == "145bc75c-7755-4cd2-a746-45097fb2883a" &&
                x.Name == "Test Group 1" &&
                x.Path == "/Test Group 1" &&
                x.Attributes.NullOrContentEqual(
                    new Dictionary<string, IEnumerable<string>>
                    {
                        { "Test Group 1 Attribute", new [] { "Test Group 1 Attribute Value" } }
                    },
                    null) &&
                x.RealmRoles != null &&
                x.RealmRoles.SequenceEqual(
                    new[]
                    {
                        "offline_access"
                    }) &&
                x.ClientRoles.NullOrContentEqual(
                    new Dictionary<string, IEnumerable<string>>
                    {
                        { "realm-management", new [] { "create-client" } }
                    },
                    null)
            );

        realmRoles.Should().HaveCount(4).And.Satisfy(
            x =>
                x.Id == "fd20bacb-f39f-499c-8fc3-c3d14e0770d9" &&
                x.Name == "default-roles-testrealm" &&
                x.Description == "${role_default-roles}" &&
                x.Composite == true &&
                x.Composites != null &&
                x.Composites.Realm != null &&
                x.Composites.Realm.SequenceEqual(new[] { "offline_access", "uma_authorization" }) &&
                x.Composites.Client != null &&
                x.Composites.Client.NullOrContentEqual(
                    new Dictionary<string, IEnumerable<string>>
                    {
                        { "account", new [] { "view-profile", "manage-account" } }
                    },
                    null
                ) &&
                x.ClientRole == false &&
                x.ContainerId == "TestRealm" &&
                x.Attributes != null &&
                !x.Attributes.Any(),
            x =>
                x.Id == "e967f3b2-535a-4805-ac04-2b5004684b2f" &&
                x.Name == "offline_access" &&
                x.Description == "${role_offline-access}" &&
                x.Composite == false &&
                x.Composites == null &&
                x.ClientRole == false &&
                x.ContainerId == "TestRealm" &&
                x.Attributes != null &&
                !x.Attributes.Any(),
            x =>
                x.Id == "a1960dd3-b079-4e7c-91e0-42a51bbb5e30",
            x =>
                x.Id == "e9b8d11f-8e45-4910-8dbb-aa206764f1bc");

        clientRoles.Should().HaveCount(8)
            .And.ContainKeys(new[] { "realm-management", "security-admin-console", "admin-cli", "account-console", "broker", "TestClientId", "account", "TestServiceAccount1" });

        clientRoles.Should().ContainKey("TestClientId")
            .WhoseValue.Should().HaveCount(2)
            .And.Satisfy(
                x =>
                    x.Id == "889fd981-c56f-4b46-bc43-f62e1004185e" &&
                    x.Name == "Test Composite Role" &&
                    x.Description == "Test Composite Role Description" &&
                    x.Composite == true &&
                    x.Composites != null &&
                    x.Composites.Client.NullOrContentEqual(
                        new Dictionary<string, IEnumerable<string>>
                        {
                            { "TestClientId", new [] { "test_role_1" } }
                        },
                        null) &&
                    x.ClientRole == true &&
                    x.ContainerId == "654052fa-59c4-484e-90f7-0c389c0e9d37" &&
                    x.Attributes.NullOrContentEqual(
                        new Dictionary<string, IEnumerable<string>>
                        {
                            { "Test Composite Role Attribute", new [] { "Test Composite Role Attribute Value" } }
                        },
                        null
                    ),
                x =>
                    x.Id == "a178ae7c-be38-44b6-8b9b-c90ccf9c7d51" &&
                    x.Name == "test_role_1" &&
                    x.Description == "Test Role 1 Description" &&
                    x.Composite == false &&
                    x.Composites == null &&
                    x.ClientRole == true &&
                    x.ContainerId == "654052fa-59c4-484e-90f7-0c389c0e9d37" &&
                    x.Attributes.NullOrContentEqual(
                        new Dictionary<string, IEnumerable<string>>
                        {
                            { "test_role_1_attribute", new [] { "test_role_1_attribute_value" } }
                        },
                        null
                    ));

        users.Should().HaveCount(2).And.Satisfy(
            x =>
                x.Id == "345577d1-6232-4fac-ad44-6ef8e3924993" &&
                x.CreatedTimestamp == 1690020450507 &&
                x.Username == "service-account-testserviceaccount1" &&
                x.Enabled == true &&
                x.Totp == false &&
                x.EmailVerified == false &&
                x.ServiceAccountClientId == "TestServiceAccount1" &&
                x.DisableableCredentialTypes != null &&
                !x.DisableableCredentialTypes.Any() &&
                x.RequiredActions != null &&
                !x.RequiredActions.Any() &&
                x.RealmRoles != null &&
                x.RealmRoles.SequenceEqual(new[] { "default-roles-testrealm" }) &&
                x.NotBefore == 0 &&
                x.Groups != null &&
                !x.Groups.Any(),
            x =>
                x.Id == "502dabcf-01c7-47d9-a88e-0be4279097b5" &&
                x.CreatedTimestamp == 1652788086549 &&
                x.Username == "testuser1" &&
                x.Enabled == true &&
                x.Totp == false &&
                x.EmailVerified == false &&
                x.FirstName == "Test" &&
                x.LastName == "User" &&
                x.Email == "test.user@mail.org" &&
                x.Attributes.NullOrContentEqual(new Dictionary<string, IEnumerable<string>> { { "foo", new[] { "DEADBEEF", "deadbeef" } } }, null) &&
                x.Credentials != null &&
                !x.Credentials.Any() &&
                x.DisableableCredentialTypes != null &&
                !x.DisableableCredentialTypes.Any() &&
                x.FederatedIdentities != null &&
                x.FederatedIdentities.Select(i => new ValueTuple<string?, string?, string?>(i.IdentityProvider, i.UserId, i.UserName)).NullOrContentEqual(new[] { new ValueTuple<string?, string?, string?>("Test Identity Provider", "testIdentityProviderUserId1", "testIdentityProviderUserName1") }, null) &&
                x.RealmRoles != null &&
                x.RealmRoles.SequenceEqual(new[] { "default-roles-testrealm", "test_realm_role_1" }) &&
                x.ClientRoles.NullOrContentEqual(new Dictionary<string, IEnumerable<string>> { { "TestClientId", new[] { "test_role_1" } } }, null) &&
                x.NotBefore == 0 &&
                x.Groups != null &&
                !x.Groups.Any());

        keycloakRealm.RequiredActions.Should().HaveCount(7).And.Satisfy(
            x =>
                x.Alias == "CONFIGURE_TOTP" &&
                x.Name == "Configure OTP" &&
                x.ProviderId == "CONFIGURE_TOTP" &&
                x.Enabled == true &&
                x.DefaultAction == false &&
                x.Priority == 10 &&
                x.Config != null,
            x =>
                x.Alias == "terms_and_conditions",
            x =>
                x.Alias == "UPDATE_PASSWORD",
            x =>
                x.Alias == "UPDATE_PROFILE",
            x =>
                x.Alias == "VERIFY_EMAIL",
            x =>
                x.Alias == "delete_account",
            x =>
                x.Alias == "update_user_locale");

        keycloakRealm.ClientProfiles.Should().NotBeNull()
            .And.Match<ClientProfilesModel>(
                x =>
                    x.Profiles != null &&
                    !x.Profiles.Any());

        keycloakRealm.ClientPolicies.Should().NotBeNull()
            .And.Match<ClientPoliciesModel>(
                x =>
                    x.Policies != null &&
                    !x.Policies.Any());
    }
}
