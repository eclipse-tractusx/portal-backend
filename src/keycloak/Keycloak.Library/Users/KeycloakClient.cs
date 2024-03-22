/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
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

using Flurl.Http;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Groups;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task CreateUserAsync(string realm, User user, CancellationToken cancellationToken = default) =>
        await InternalCreateUserAsync(realm, user, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

    private async Task<IFlurlResponse> InternalCreateUserAsync(string realm, User user, CancellationToken cancellationToken) => await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/users")
        .PostJsonAsync(user, cancellationToken)
        .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<string?> CreateAndRetrieveUserIdAsync(string realm, User user, CancellationToken cancellationToken = default)
    {
        var response = await InternalCreateUserAsync(realm, user, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var locationPathAndQuery = response.ResponseMessage.Headers.Location?.PathAndQuery;
        return locationPathAndQuery != null ? locationPathAndQuery.Substring(locationPathAndQuery.LastIndexOf("/", StringComparison.Ordinal) + 1) : null;
    }

    public async Task<IEnumerable<User>> GetUsersAsync(string realm, bool? briefRepresentation = null, string? email = null, int? first = null,
        string? firstName = null, string? lastName = null, int? max = null, string? search = null, string? username = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(briefRepresentation)] = briefRepresentation,
            [nameof(email)] = email,
            [nameof(first)] = first,
            [nameof(firstName)] = firstName,
            [nameof(lastName)] = lastName,
            [nameof(max)] = max,
            [nameof(search)] = search,
            [nameof(username)] = username
        };

        return await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<User>>(cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<int> GetUsersCountAsync(string realm) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/count")
            .GetJsonAsync<int>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<User> GetUserAsync(string realm, string userId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .GetJsonAsync<User>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task UpdateUserAsync(string realm, string userId, User user, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .PutJsonAsync(user, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task DeleteUserAsync(string realm, string userId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .DeleteAsync()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    [Obsolete("Not working yet")]
    public async Task<string> GetUserConsentsAsync(string realm, string userId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/consents")
            .GetStringAsync()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task RevokeUserConsentAndOfflineTokensAsync(string realm, string userId, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/consents/")
            .AppendPathSegment(clientId, true)
            .DeleteAsync()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task DisableUserCredentialsAsync(string realm, string userId, IEnumerable<string> credentialTypes) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/disable-credential-types")
            .PutJsonAsync(credentialTypes)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task SendUserUpdateAccountEmailAsync(string realm, string userId, IEnumerable<string> requiredActions, string? clientId = null, int? lifespan = null, string? redirectUri = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            ["client_id"] = clientId,
            [nameof(lifespan)] = lifespan,
            ["redirect_uri"] = redirectUri
        };

        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/execute-actions-email")
            .SetQueryParams(queryParams)
            .PutJsonAsync(requiredActions)
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<IEnumerable<FederatedIdentity>> GetUserSocialLoginsAsync(string realm, string userId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/users/")
        .AppendPathSegment(userId, true)
        .AppendPathSegment("/federated-identity")
        .GetJsonAsync<IEnumerable<FederatedIdentity>>()
        .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task AddUserSocialLoginProviderAsync(string realm, string userId, string provider, FederatedIdentity federatedIdentity, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/federated-identity/")
            .AppendPathSegment(provider, true)
            .PostJsonAsync(federatedIdentity, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task RemoveUserSocialLoginProviderAsync(string realm, string userId, string provider, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/federated-identity/")
            .AppendPathSegment(provider, true)
            .DeleteAsync(cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IEnumerable<Group>> GetUserGroupsAsync(string realm, string userId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/groups")
            .GetJsonAsync<IEnumerable<Group>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<int> GetUserGroupsCountAsync(string realm, string userId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/groups/count")
            .GetJsonAsync()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task UpdateUserGroupAsync(string realm, string userId, string groupId, Group group) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .PutJsonAsync(group)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task DeleteUserGroupAsync(string realm, string userId, string groupId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .DeleteAsync()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IDictionary<string, object>> ImpersonateUserAsync(string realm, string userId)
    {
        using var stringContent = new StringContent("");
        return await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/impersonation")
            .PostAsync(stringContent)
            .ReceiveJson<IDictionary<string, object>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task RemoveUserSessionsAsync(string realm, string userId)
    {
        using var stringContent = new StringContent("");
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/logout")
            .PostAsync(stringContent)
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    [Obsolete("Not working yet")]
    public async Task<IEnumerable<UserSession>> GetUserOfflineSessionsAsync(string realm, string userId, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/offline-sessions/")
            .AppendPathSegment(clientId, true)
            .GetJsonAsync<IEnumerable<UserSession>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task RemoveUserTotpAsync(string realm, string userId)
    {
        using var stringContent = new StringContent("");
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/remove-totp")
            .PutAsync(stringContent)
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task ResetUserPasswordAsync(string realm, string userId, Credentials credentials) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/reset-password")
            .PutJsonAsync(credentials)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task ResetUserPasswordAsync(string realm, string userId, string password, bool temporary = true)
    {
        var credentials = new Credentials
        {
            Value = password,
            Temporary = temporary
        };
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/reset-password")
            .PutJsonAsync(credentials)
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<SetPasswordResponse> SetUserPasswordAsync(string realm, string userId, string password)
    {
        var credentials = new Credentials
        {
            Value = password,
            Temporary = true
        };
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AllowAnyHttpStatus()
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/reset-password")
            .PutJsonAsync(credentials)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (response.ResponseMessage.IsSuccessStatusCode)
            return new SetPasswordResponse { Success = true };

        return await response.GetJsonAsync<SetPasswordResponse>();
    }

    public async Task VerifyUserEmailAddressAsync(string realm, string userId, string? clientId = null, string? redirectUri = null)
    {
        var queryParams = new Dictionary<string, object?>();
        if (!string.IsNullOrEmpty(clientId))
        {
            queryParams.Add("client_id", clientId);
        }

        if (!string.IsNullOrEmpty(redirectUri))
        {
            queryParams.Add("redirect_uri", redirectUri);
        }

        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/send-verify-email")
            .SetQueryParams(queryParams)
            .PutJsonAsync(null)
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<IEnumerable<UserSession>> GetUserSessionsAsync(string realm, string userId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/sessions")
            .GetJsonAsync<IEnumerable<UserSession>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);
}
