/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 ********************************************************************************/

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;

public class Realm
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("realm")]
    // ReSharper disable once InconsistentNaming
    public string? _Realm { get; set; }
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
    [JsonPropertyName("displayNameHtml")]
    public string? DisplayNameHtml { get; set; }
    [JsonPropertyName("notBefore")]
    public int? NotBefore { get; set; }
    [JsonPropertyName("defaultSignatureAlgorithm")]
    public string? DefaultSignatureAlgorithm { get; set; }
    [JsonPropertyName("revokeRefreshToken")]
    public bool? RevokeRefreshToken { get; set; }
    [JsonPropertyName("refreshTokenMaxReuse")]
    public int? RefreshTokenMaxReuse { get; set; }
    [JsonPropertyName("accessTokenLifespan")]
    public int? AccessTokenLifespan { get; set; }
    [JsonPropertyName("accessTokenLifespanForImplicitFlow")]
    public int? AccessTokenLifespanForImplicitFlow { get; set; }
    [JsonPropertyName("ssoSessionIdleTimeout")]
    public int? SsoSessionIdleTimeout { get; set; }
    [JsonPropertyName("ssoSessionMaxLifespan")]
    public int? SsoSessionMaxLifespan { get; set; }
    [JsonPropertyName("ssoSessionIdleTimeoutRememberMe")]
    public int? SsoSessionIdleTimeoutRememberMe { get; set; }
    [JsonPropertyName("ssoSessionMaxLifespanRememberMe")]
    public int? SsoSessionMaxLifespanRememberMe { get; set; }
    [JsonPropertyName("offlineSessionIdleTimeout")]
    public int? OfflineSessionIdleTimeout { get; set; }
    [JsonPropertyName("offlineSessionMaxLifespanEnabled")]
    public bool? OfflineSessionMaxLifespanEnabled { get; set; }
    [JsonPropertyName("offlineSessionMaxLifespan")]
    public int? OfflineSessionMaxLifespan { get; set; }
    [JsonPropertyName("accessCodeLifespan")]
    public int? AccessCodeLifespan { get; set; }
    [JsonPropertyName("accessCodeLifespanUserAction")]
    public int? AccessCodeLifespanUserAction { get; set; }
    [JsonPropertyName("accessCodeLifespanLogin")]
    public int? AccessCodeLifespanLogin { get; set; }
    [JsonPropertyName("actionTokenGeneratedByAdminLifespan")]
    public int? ActionTokenGeneratedByAdminLifespan { get; set; }
    [JsonPropertyName("actionTokenGeneratedByUserLifespan")]
    public int? ActionTokenGeneratedByUserLifespan { get; set; }
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
    [JsonPropertyName("sslRequired")]
    public string? SslRequired { get; set; }
    [JsonPropertyName("registrationAllowed")]
    public bool? RegistrationAllowed { get; set; }
    [JsonPropertyName("registrationEmailAsUsername")]
    public bool? RegistrationEmailAsUsername { get; set; }
    [JsonPropertyName("rememberMe")]
    public bool? RememberMe { get; set; }
    [JsonPropertyName("verifyEmail")]
    public bool? VerifyEmail { get; set; }
    [JsonPropertyName("loginWithEmailAllowed")]
    public bool? LoginWithEmailAllowed { get; set; }
    [JsonPropertyName("duplicateEmailsAllowed")]
    public bool? DuplicateEmailsAllowed { get; set; }
    [JsonPropertyName("resetPasswordAllowed")]
    public bool? ResetPasswordAllowed { get; set; }
    [JsonPropertyName("editUsernameAllowed")]
    public bool? EditUsernameAllowed { get; set; }
    [JsonPropertyName("bruteForceProtected")]
    public bool? BruteForceProtected { get; set; }
    [JsonPropertyName("permanentLockout")]
    public bool? PermanentLockout { get; set; }
    [JsonPropertyName("maxFailureWaitSeconds")]
    public int? MaxFailureWaitSeconds { get; set; }
    [JsonPropertyName("minimumQuickLoginWaitSeconds")]
    public int? MinimumQuickLoginWaitSeconds { get; set; }
    [JsonPropertyName("waitIncrementSeconds")]
    public int? WaitIncrementSeconds { get; set; }
    [JsonPropertyName("quickLoginCheckMilliSeconds")]
    public int? QuickLoginCheckMilliSeconds { get; set; }
    [JsonPropertyName("maxDeltaTimeSeconds")]
    public int? MaxDeltaTimeSeconds { get; set; }
    [JsonPropertyName("failureFactor")]
    public int? FailureFactor { get; set; }
    [JsonPropertyName("defaultRole")]
    public Role? DefaultRole { get; set; }
    [JsonPropertyName("requiredCredentials")]
    public IEnumerable<string>? RequiredCredentials { get; set; }
    [JsonPropertyName("otpPolicyType")]
    public string? OtpPolicyType { get; set; }
    [JsonPropertyName("otpPolicyAlgorithm")]
    public string? OtpPolicyAlgorithm { get; set; }
    [JsonPropertyName("otpPolicyInitialCounter")]
    public int? OtpPolicyInitialCounter { get; set; }
    [JsonPropertyName("otpPolicyDigits")]
    public int? OtpPolicyDigits { get; set; }
    [JsonPropertyName("otpPolicyLookAheadWindow")]
    public int? OtpPolicyLookAheadWindow { get; set; }
    [JsonPropertyName("otpPolicyPeriod")]
    public int? OtpPolicyPeriod { get; set; }
    [JsonPropertyName("otpSupportedApplications")]
    public IEnumerable<string>? OtpSupportedApplications { get; set; }
    [JsonPropertyName("browserSecurityHeaders")]
    public BrowserSecurityHeaders? BrowserSecurityHeaders { get; set; }
    [JsonPropertyName("smtpServer")]
    public SmtpServer? SmtpServer { get; set; }
    [JsonPropertyName("loginTheme")]
    public string? LoginTheme { get; set; }
    [JsonPropertyName("accountTheme")]
    public string? AccountTheme { get; set; }
    [JsonPropertyName("adminTheme")]
    public string? AdminTheme { get; set; }
    [JsonPropertyName("emailTheme")]
    public string? EmailTheme { get; set; }
    [JsonPropertyName("eventsEnabled")]
    public bool? EventsEnabled { get; set; }
    [JsonPropertyName("eventsListeners")]
    public IEnumerable<string>? EventsListeners { get; set; }
    [JsonPropertyName("enabledEventTypes")]
    public IEnumerable<string>? EnabledEventTypes { get; set; }
    [JsonPropertyName("adminEventsEnabled")]
    public bool? AdminEventsEnabled { get; set; }
    [JsonPropertyName("adminEventsDetailsEnabled")]
    public bool? AdminEventsDetailsEnabled { get; set; }
    [JsonPropertyName("identityProviders")]
    public IEnumerable<IdentityProvider>? IdentityProviders { get; set; }
    [JsonPropertyName("internationalizationEnabled")]
    public bool? InternationalizationEnabled { get; set; }
    [JsonPropertyName("supportedLocales")]
    public IEnumerable<string>? SupportedLocales { get; set; }
    [JsonPropertyName("browserFlow")]
    public string? BrowserFlow { get; set; }
    [JsonPropertyName("registrationFlow")]
    public string? RegistrationFlow { get; set; }
    [JsonPropertyName("directGrantFlow")]
    public string? DirectGrantFlow { get; set; }
    [JsonPropertyName("resetCredentialsFlow")]
    public string? ResetCredentialsFlow { get; set; }
    [JsonPropertyName("clientAuthenticationFlow")]
    public string? ClientAuthenticationFlow { get; set; }
    [JsonPropertyName("dockerAuthenticationFlow")]
    public string? DockerAuthenticationFlow { get; set; }
    [JsonPropertyName("attributes")]
    public IDictionary<string, string>? Attributes { get; set; }
    [JsonPropertyName("userManagedAccessAllowed")]
    public bool? UserManagedAccessAllowed { get; set; }
    [JsonPropertyName("passwordPolicy")]
    public string? PasswordPolicy { get; set; }
}
