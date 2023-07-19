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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class RealmUpdater : IRealmUpdater
{
    private readonly IKeycloakFactory _keycloakFactory;
    private readonly ISeedDataHandler _seedData;

    public RealmUpdater(IKeycloakFactory keycloakFactory, ISeedDataHandler seedDataHandler)
    {
        _keycloakFactory = keycloakFactory;
        _seedData = seedDataHandler;
    }

    public async Task UpdateRealm(string keycloakInstanceName)
    {
        var keycloak = _keycloakFactory.CreateKeycloakClient(keycloakInstanceName);
        var realm = _seedData.Realm;
        var seedRealm = _seedData.KeycloakRealm;

        Realm keycloakRealm;
        try
        {
            keycloakRealm = await keycloak.GetRealmAsync(realm).ConfigureAwait(false);
        }
        catch (KeycloakEntityNotFoundException)
        {
            keycloakRealm = new Realm
            {
                Id = seedRealm.Id,
                _Realm = seedRealm.Realm
            };
            await keycloak.ImportRealmAsync(realm, keycloakRealm).ConfigureAwait(false);
        }

        if (!Compare(keycloakRealm, seedRealm))
        {
            keycloakRealm._Realm = seedRealm.Realm;
            keycloakRealm.DisplayName = seedRealm.DisplayName;
            keycloakRealm.NotBefore = seedRealm.NotBefore;
            keycloakRealm.RevokeRefreshToken = seedRealm.RevokeRefreshToken;
            keycloakRealm.RefreshTokenMaxReuse = seedRealm.RefreshTokenMaxReuse;
            keycloakRealm.AccessTokenLifespan = seedRealm.AccessTokenLifespan;
            keycloakRealm.AccessTokenLifespanForImplicitFlow = seedRealm.AccessTokenLifespanForImplicitFlow;
            keycloakRealm.SsoSessionIdleTimeout = seedRealm.SsoSessionIdleTimeout;
            keycloakRealm.SsoSessionMaxLifespan = seedRealm.SsoSessionMaxLifespan;
            keycloakRealm.SsoSessionIdleTimeoutRememberMe = seedRealm.SsoSessionIdleTimeoutRememberMe;
            keycloakRealm.SsoSessionMaxLifespanRememberMe = seedRealm.SsoSessionMaxLifespanRememberMe;
            keycloakRealm.OfflineSessionIdleTimeout = seedRealm.OfflineSessionIdleTimeout;
            keycloakRealm.OfflineSessionMaxLifespanEnabled = seedRealm.OfflineSessionMaxLifespanEnabled;
            keycloakRealm.OfflineSessionMaxLifespan = seedRealm.OfflineSessionMaxLifespan;
            keycloakRealm.AccessCodeLifespan = seedRealm.AccessCodeLifespan;
            keycloakRealm.AccessCodeLifespanUserAction = seedRealm.AccessCodeLifespanUserAction;
            keycloakRealm.AccessCodeLifespanLogin = seedRealm.AccessCodeLifespanLogin;
            keycloakRealm.ActionTokenGeneratedByAdminLifespan = seedRealm.ActionTokenGeneratedByAdminLifespan;
            keycloakRealm.ActionTokenGeneratedByUserLifespan = seedRealm.ActionTokenGeneratedByUserLifespan;
            keycloakRealm.Enabled = seedRealm.Enabled;
            keycloakRealm.SslRequired = seedRealm.SslRequired;
            keycloakRealm.RegistrationAllowed = seedRealm.RegistrationAllowed;
            keycloakRealm.RegistrationEmailAsUsername = seedRealm.RegistrationEmailAsUsername;
            keycloakRealm.RememberMe = seedRealm.RememberMe;
            keycloakRealm.VerifyEmail = seedRealm.VerifyEmail;
            keycloakRealm.LoginWithEmailAllowed = seedRealm.LoginWithEmailAllowed;
            keycloakRealm.DuplicateEmailsAllowed = seedRealm.DuplicateEmailsAllowed;
            keycloakRealm.ResetPasswordAllowed = seedRealm.ResetPasswordAllowed;
            keycloakRealm.EditUsernameAllowed = seedRealm.EditUsernameAllowed;
            keycloakRealm.BruteForceProtected = seedRealm.BruteForceProtected;
            keycloakRealm.PermanentLockout = seedRealm.PermanentLockout;
            keycloakRealm.MaxFailureWaitSeconds = seedRealm.MaxFailureWaitSeconds;
            keycloakRealm.MinimumQuickLoginWaitSeconds = seedRealm.MinimumQuickLoginWaitSeconds;
            keycloakRealm.WaitIncrementSeconds = seedRealm.WaitIncrementSeconds;
            keycloakRealm.QuickLoginCheckMilliSeconds = seedRealm.QuickLoginCheckMilliSeconds;
            keycloakRealm.MaxDeltaTimeSeconds = seedRealm.MaxDeltaTimeSeconds;
            keycloakRealm.FailureFactor = seedRealm.FailureFactor;
            // realm.DefaultRole = UpdateDefaultRole(realm.DefaultRole, jsonRealm.DefaultRole);
            keycloakRealm.RequiredCredentials = seedRealm.RequiredCredentials;
            keycloakRealm.OtpPolicyType = seedRealm.OtpPolicyType;
            keycloakRealm.OtpPolicyAlgorithm = seedRealm.OtpPolicyAlgorithm;
            keycloakRealm.OtpPolicyInitialCounter = seedRealm.OtpPolicyInitialCounter;
            keycloakRealm.OtpPolicyDigits = seedRealm.OtpPolicyDigits;
            keycloakRealm.OtpPolicyLookAheadWindow = seedRealm.OtpPolicyLookAheadWindow;
            keycloakRealm.OtpPolicyPeriod = seedRealm.OtpPolicyPeriod;
            keycloakRealm.OtpSupportedApplications = seedRealm.OtpSupportedApplications;
            keycloakRealm.BrowserSecurityHeaders = UpdateBrowserSecurityHeaders(seedRealm.BrowserSecurityHeaders);
            keycloakRealm.SmtpServer = UpdateSmtpServer(seedRealm.SmtpServer);
            keycloakRealm.EventsEnabled = seedRealm.EventsEnabled;
            keycloakRealm.EventsListeners = seedRealm.EventsListeners;
            keycloakRealm.EnabledEventTypes = seedRealm.EnabledEventTypes;
            keycloakRealm.AdminEventsEnabled = seedRealm.AdminEventsEnabled;
            keycloakRealm.AdminEventsDetailsEnabled = seedRealm.AdminEventsDetailsEnabled;
            // realm.IdentityProviders = UpdateIdentityProviders(realm.IdentityProviders, jsonRealm.IdentityProviders);
            keycloakRealm.InternationalizationEnabled = seedRealm.InternationalizationEnabled;
            keycloakRealm.SupportedLocales = seedRealm.SupportedLocales;
            keycloakRealm.BrowserFlow = seedRealm.BrowserFlow;
            keycloakRealm.RegistrationFlow = seedRealm.RegistrationFlow;
            keycloakRealm.DirectGrantFlow = seedRealm.DirectGrantFlow;
            keycloakRealm.ResetCredentialsFlow = seedRealm.ResetCredentialsFlow;
            keycloakRealm.ClientAuthenticationFlow = seedRealm.ClientAuthenticationFlow;
            keycloakRealm.DockerAuthenticationFlow = seedRealm.DockerAuthenticationFlow;
            keycloakRealm.Attributes = seedRealm.Attributes?.ToDictionary(x => x.Key, x => x.Value);
            keycloakRealm.UserManagedAccessAllowed = seedRealm.UserManagedAccessAllowed;
            //realm.PasswordPolicy = jsonRealm.PasswordPolicy;
            keycloakRealm.LoginTheme = seedRealm.LoginTheme;

            await keycloak.UpdateRealmAsync(realm, keycloakRealm).ConfigureAwait(false);
        }
    }

    private static bool Compare(Realm keycloakRealm, KeycloakRealm seedRealm) =>
        keycloakRealm._Realm == seedRealm.Realm &&
        keycloakRealm.DisplayName == seedRealm.DisplayName &&
        keycloakRealm.NotBefore == seedRealm.NotBefore &&
        keycloakRealm.RevokeRefreshToken == seedRealm.RevokeRefreshToken &&
        keycloakRealm.RefreshTokenMaxReuse == seedRealm.RefreshTokenMaxReuse &&
        keycloakRealm.AccessTokenLifespan == seedRealm.AccessTokenLifespan &&
        keycloakRealm.AccessTokenLifespanForImplicitFlow == seedRealm.AccessTokenLifespanForImplicitFlow &&
        keycloakRealm.SsoSessionIdleTimeout == seedRealm.SsoSessionIdleTimeout &&
        keycloakRealm.SsoSessionMaxLifespan == seedRealm.SsoSessionMaxLifespan &&
        keycloakRealm.SsoSessionIdleTimeoutRememberMe == seedRealm.SsoSessionIdleTimeoutRememberMe &&
        keycloakRealm.SsoSessionMaxLifespanRememberMe == seedRealm.SsoSessionMaxLifespanRememberMe &&
        keycloakRealm.OfflineSessionIdleTimeout == seedRealm.OfflineSessionIdleTimeout &&
        keycloakRealm.OfflineSessionMaxLifespanEnabled == seedRealm.OfflineSessionMaxLifespanEnabled &&
        keycloakRealm.OfflineSessionMaxLifespan == seedRealm.OfflineSessionMaxLifespan &&
        keycloakRealm.AccessCodeLifespan == seedRealm.AccessCodeLifespan &&
        keycloakRealm.AccessCodeLifespanUserAction == seedRealm.AccessCodeLifespanUserAction &&
        keycloakRealm.AccessCodeLifespanLogin == seedRealm.AccessCodeLifespanLogin &&
        keycloakRealm.ActionTokenGeneratedByAdminLifespan == seedRealm.ActionTokenGeneratedByAdminLifespan &&
        keycloakRealm.ActionTokenGeneratedByUserLifespan == seedRealm.ActionTokenGeneratedByUserLifespan &&
        keycloakRealm.Enabled == seedRealm.Enabled &&
        keycloakRealm.SslRequired == seedRealm.SslRequired &&
        keycloakRealm.RegistrationAllowed == seedRealm.RegistrationAllowed &&
        keycloakRealm.RegistrationEmailAsUsername == seedRealm.RegistrationEmailAsUsername &&
        keycloakRealm.RememberMe == seedRealm.RememberMe &&
        keycloakRealm.VerifyEmail == seedRealm.VerifyEmail &&
        keycloakRealm.LoginWithEmailAllowed == seedRealm.LoginWithEmailAllowed &&
        keycloakRealm.DuplicateEmailsAllowed == seedRealm.DuplicateEmailsAllowed &&
        keycloakRealm.ResetPasswordAllowed == seedRealm.ResetPasswordAllowed &&
        keycloakRealm.EditUsernameAllowed == seedRealm.EditUsernameAllowed &&
        keycloakRealm.BruteForceProtected == seedRealm.BruteForceProtected &&
        keycloakRealm.PermanentLockout == seedRealm.PermanentLockout &&
        keycloakRealm.MaxFailureWaitSeconds == seedRealm.MaxFailureWaitSeconds &&
        keycloakRealm.MinimumQuickLoginWaitSeconds == seedRealm.MinimumQuickLoginWaitSeconds &&
        keycloakRealm.WaitIncrementSeconds == seedRealm.WaitIncrementSeconds &&
        keycloakRealm.QuickLoginCheckMilliSeconds == seedRealm.QuickLoginCheckMilliSeconds &&
        keycloakRealm.MaxDeltaTimeSeconds == seedRealm.MaxDeltaTimeSeconds &&
        keycloakRealm.FailureFactor == seedRealm.FailureFactor &&
        // realm.DefaultRole != UpdateDefaultRole(realm.DefaultRole, jsonRealm.DefaultRole) &&
        keycloakRealm.RequiredCredentials == seedRealm.RequiredCredentials &&
        keycloakRealm.OtpPolicyType == seedRealm.OtpPolicyType &&
        keycloakRealm.OtpPolicyAlgorithm == seedRealm.OtpPolicyAlgorithm &&
        keycloakRealm.OtpPolicyInitialCounter == seedRealm.OtpPolicyInitialCounter &&
        keycloakRealm.OtpPolicyDigits == seedRealm.OtpPolicyDigits &&
        keycloakRealm.OtpPolicyLookAheadWindow == seedRealm.OtpPolicyLookAheadWindow &&
        keycloakRealm.OtpPolicyPeriod == seedRealm.OtpPolicyPeriod &&
        keycloakRealm.OtpSupportedApplications == seedRealm.OtpSupportedApplications &&
        Compare(keycloakRealm.BrowserSecurityHeaders, seedRealm.BrowserSecurityHeaders) &&
        Compare(keycloakRealm.SmtpServer, seedRealm.SmtpServer) &&
        keycloakRealm.EventsEnabled == seedRealm.EventsEnabled &&
        keycloakRealm.EventsListeners == seedRealm.EventsListeners &&
        keycloakRealm.EnabledEventTypes == seedRealm.EnabledEventTypes &&
        keycloakRealm.AdminEventsEnabled == seedRealm.AdminEventsEnabled &&
        keycloakRealm.AdminEventsDetailsEnabled == seedRealm.AdminEventsDetailsEnabled &&
        // realm.IdentityProviders != UpdateIdentityProviders(realm.IdentityProviders, jsonRealm.IdentityProviders) &&
        keycloakRealm.InternationalizationEnabled == seedRealm.InternationalizationEnabled &&
        keycloakRealm.SupportedLocales == seedRealm.SupportedLocales &&
        keycloakRealm.BrowserFlow == seedRealm.BrowserFlow &&
        keycloakRealm.RegistrationFlow == seedRealm.RegistrationFlow &&
        keycloakRealm.DirectGrantFlow == seedRealm.DirectGrantFlow &&
        keycloakRealm.ResetCredentialsFlow == seedRealm.ResetCredentialsFlow &&
        keycloakRealm.ClientAuthenticationFlow == seedRealm.ClientAuthenticationFlow &&
        keycloakRealm.DockerAuthenticationFlow == seedRealm.DockerAuthenticationFlow &&
        Compare(keycloakRealm.Attributes, seedRealm.Attributes) &&
        keycloakRealm.UserManagedAccessAllowed == seedRealm.UserManagedAccessAllowed &&
        //realm.PasswordPolicy != jsonRealm.PasswordPolicy &&
        keycloakRealm.LoginTheme == seedRealm.LoginTheme;

    private static bool Compare(IDictionary<string, string>? attributes, IReadOnlyDictionary<string, string>? updateAttributes) =>
        attributes == null && updateAttributes == null ||
        attributes != null && updateAttributes != null &&
        attributes.OrderBy(x => x.Key).SequenceEqual(updateAttributes.OrderBy(x => x.Key));

    private static bool Compare(BrowserSecurityHeaders? securityHeaders, BrowserSecurityHeadersModel? updateSecurityHeaders) =>
        securityHeaders == null && updateSecurityHeaders == null ||
        securityHeaders != null && updateSecurityHeaders != null &&
        securityHeaders.ContentSecurityPolicyReportOnly == updateSecurityHeaders.ContentSecurityPolicyReportOnly &&
        securityHeaders.XContentTypeOptions == updateSecurityHeaders.XContentTypeOptions &&
        securityHeaders.XRobotsTag == updateSecurityHeaders.XRobotsTag &&
        securityHeaders.XFrameOptions == updateSecurityHeaders.XFrameOptions &&
        securityHeaders.XXssProtection == updateSecurityHeaders.XXSSProtection &&
        securityHeaders.ContentSecurityPolicy == updateSecurityHeaders.ContentSecurityPolicy &&
        securityHeaders.StrictTransportSecurity == updateSecurityHeaders.StrictTransportSecurity;

    private static BrowserSecurityHeaders? UpdateBrowserSecurityHeaders(BrowserSecurityHeadersModel? updateSecurityHeaders) =>
        updateSecurityHeaders == null
            ? null
            : new BrowserSecurityHeaders
            {
                ContentSecurityPolicyReportOnly = updateSecurityHeaders.ContentSecurityPolicyReportOnly,
                XContentTypeOptions = updateSecurityHeaders.XContentTypeOptions,
                XRobotsTag = updateSecurityHeaders.XRobotsTag,
                XFrameOptions = updateSecurityHeaders.XFrameOptions,
                XXssProtection = updateSecurityHeaders.XXSSProtection,
                ContentSecurityPolicy = updateSecurityHeaders.ContentSecurityPolicy,
                StrictTransportSecurity = updateSecurityHeaders.StrictTransportSecurity
            };

    private static bool Compare(SmtpServer? smtpServer, SmtpServerModel? updateSmtpServer) =>
        smtpServer == null && updateSmtpServer == null ||
        smtpServer != null && updateSmtpServer != null &&
        smtpServer.Host == updateSmtpServer.Host &&
        smtpServer.Ssl == updateSmtpServer.Ssl &&
        smtpServer.StartTls == updateSmtpServer.Starttls &&
        smtpServer.User == updateSmtpServer.User &&
        smtpServer.Password == updateSmtpServer.Password &&
        smtpServer.Auth == updateSmtpServer.Auth &&
        smtpServer.From == updateSmtpServer.From &&
        smtpServer.FromDisplayName == updateSmtpServer.FromDisplayName &&
        smtpServer.ReplyTo == updateSmtpServer.ReplyTo &&
        smtpServer.ReplyToDisplayName == updateSmtpServer.ReplyToDisplayName &&
        smtpServer.EnvelopeFrom == updateSmtpServer.EnvelopeFrom &&
        smtpServer.Port == updateSmtpServer.Port;

    private static SmtpServer? UpdateSmtpServer(SmtpServerModel? updateSmtpServer) =>
        updateSmtpServer == null
            ? null
            : new SmtpServer
            {
                Host = updateSmtpServer.Host,
                Ssl = updateSmtpServer.Ssl,
                StartTls = updateSmtpServer.Starttls,
                User = updateSmtpServer.User,
                Password = updateSmtpServer.Password,
                Auth = updateSmtpServer.Auth,
                From = updateSmtpServer.From,
                FromDisplayName = updateSmtpServer.FromDisplayName,
                ReplyTo = updateSmtpServer.ReplyTo,
                ReplyToDisplayName = updateSmtpServer.ReplyToDisplayName,
                EnvelopeFrom = updateSmtpServer.EnvelopeFrom,
                Port = updateSmtpServer.Port
            };
}
