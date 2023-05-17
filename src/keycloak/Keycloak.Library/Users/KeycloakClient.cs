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

using Flurl.Http;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Groups;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
	public async Task CreateUserAsync(string realm, User user) =>
		await InternalCreateUserAsync(realm, user).ConfigureAwait(false);

	private async Task<IFlurlResponse> InternalCreateUserAsync(string realm, User user) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
		.AppendPathSegment("/admin/realms/")
		.AppendPathSegment(realm, true)
		.AppendPathSegment("/users")
		.PostJsonAsync(user)
		.ConfigureAwait(false);

	public async Task<string?> CreateAndRetrieveUserIdAsync(string realm, User user)
	{
		var response = await InternalCreateUserAsync(realm, user).ConfigureAwait(false);
		var locationPathAndQuery = response.ResponseMessage.Headers.Location?.PathAndQuery;
		return locationPathAndQuery != null ? locationPathAndQuery.Substring(locationPathAndQuery.LastIndexOf("/", StringComparison.Ordinal) + 1) : null;
	}

	public async Task<IEnumerable<User>> GetUsersAsync(string realm, bool? briefRepresentation = null, string? email = null, int? first = null,
		string? firstName = null, string? lastName = null, int? max = null, string? search = null, string? username = null)
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

		return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users")
			.SetQueryParams(queryParams)
			.GetJsonAsync<IEnumerable<User>>()
			.ConfigureAwait(false);
	}

	public async Task<int> GetUsersCountAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/count")
			.GetJsonAsync<int>()
			.ConfigureAwait(false);

	public async Task<User> GetUserAsync(string realm, string userId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.GetJsonAsync<User>()
			.ConfigureAwait(false);

	public async Task UpdateUserAsync(string realm, string userId, User user) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.PutJsonAsync(user)
			.ConfigureAwait(false);

	public async Task DeleteUserAsync(string realm, string userId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.DeleteAsync()
			.ConfigureAwait(false);

	[Obsolete("Not working yet")]
	public async Task<string> GetUserConsentsAsync(string realm, string userId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/consents")
			.GetStringAsync()
			.ConfigureAwait(false);

	public async Task RevokeUserConsentAndOfflineTokensAsync(string realm, string userId, string clientId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/consents/")
			.AppendPathSegment(clientId, true)
			.DeleteAsync()
			.ConfigureAwait(false);

	public async Task DisableUserCredentialsAsync(string realm, string userId, IEnumerable<string> credentialTypes) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/disable-credential-types")
			.PutJsonAsync(credentialTypes)
			.ConfigureAwait(false);

	public async Task SendUserUpdateAccountEmailAsync(string realm, string userId, IEnumerable<string> requiredActions, string? clientId = null, int? lifespan = null, string? redirectUri = null)
	{
		var queryParams = new Dictionary<string, object?>
		{
			["client_id"] = clientId,
			[nameof(lifespan)] = lifespan,
			["redirect_uri"] = redirectUri
		};

		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/execute-actions-email")
			.SetQueryParams(queryParams)
			.PutJsonAsync(requiredActions)
			.ConfigureAwait(false);
	}

	public async Task<IEnumerable<FederatedIdentity>> GetUserSocialLoginsAsync(string realm, string userId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
		.AppendPathSegment("/admin/realms/")
		.AppendPathSegment(realm, true)
		.AppendPathSegment("/users/")
		.AppendPathSegment(userId, true)
		.AppendPathSegment("/federated-identity")
		.GetJsonAsync<IEnumerable<FederatedIdentity>>()
		.ConfigureAwait(false);

	public async Task AddUserSocialLoginProviderAsync(string realm, string userId, string provider, FederatedIdentity federatedIdentity) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/federated-identity/")
			.AppendPathSegment(provider, true)
			.PostJsonAsync(federatedIdentity)
			.ConfigureAwait(false);

	public async Task RemoveUserSocialLoginProviderAsync(string realm, string userId, string provider) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/federated-identity/")
			.AppendPathSegment(provider, true)
			.DeleteAsync()
			.ConfigureAwait(false);

	public async Task<IEnumerable<Group>> GetUserGroupsAsync(string realm, string userId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/groups")
			.GetJsonAsync<IEnumerable<Group>>()
			.ConfigureAwait(false);

	public async Task<int> GetUserGroupsCountAsync(string realm, string userId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/groups/count")
			.GetJsonAsync()
			.ConfigureAwait(false);

	public async Task UpdateUserGroupAsync(string realm, string userId, string groupId, Group group) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/groups/")
			.AppendPathSegment(groupId, true)
			.PutJsonAsync(group)
			.ConfigureAwait(false);

	public async Task DeleteUserGroupAsync(string realm, string userId, string groupId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/groups/")
			.AppendPathSegment(groupId, true)
			.DeleteAsync()
			.ConfigureAwait(false);

	public async Task<IDictionary<string, object>> ImpersonateUserAsync(string realm, string userId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/impersonation")
			.PostAsync(new StringContent(""))
			.ReceiveJson<IDictionary<string, object>>()
			.ConfigureAwait(false);

	public async Task RemoveUserSessionsAsync(string realm, string userId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/logout")
			.PostAsync(new StringContent(""))
			.ConfigureAwait(false);

	[Obsolete("Not working yet")]
	public async Task<IEnumerable<UserSession>> GetUserOfflineSessionsAsync(string realm, string userId, string clientId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/offline-sessions/")
			.AppendPathSegment(clientId, true)
			.GetJsonAsync<IEnumerable<UserSession>>()
			.ConfigureAwait(false);

	public async Task RemoveUserTotpAsync(string realm, string userId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/remove-totp")
			.PutAsync(new StringContent(""))
			.ConfigureAwait(false);

	public async Task ResetUserPasswordAsync(string realm, string userId, Credentials credentials) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/reset-password")
			.PutJsonAsync(credentials)
			.ConfigureAwait(false);

	public async Task ResetUserPasswordAsync(string realm, string userId, string password, bool temporary = true)
	{
		var credentials = new Credentials
		{
			Value = password,
			Temporary = temporary
		};
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/reset-password")
			.PutJsonAsync(credentials)
			.ConfigureAwait(false);
	}

	public async Task<SetPasswordResponse> SetUserPasswordAsync(string realm, string userId, string password)
	{
		var credentials = new Credentials
		{
			Value = password,
			Temporary = true
		};
		var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AllowAnyHttpStatus()
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/reset-password")
			.PutJsonAsync(credentials)
			.ConfigureAwait(false);
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

		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/send-verify-email")
			.SetQueryParams(queryParams)
			.PutJsonAsync(null)
			.ConfigureAwait(false);
	}

	public async Task<IEnumerable<UserSession>> GetUserSessionsAsync(string realm, string userId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/sessions")
			.GetJsonAsync<IEnumerable<UserSession>>()
			.ConfigureAwait(false);
}
