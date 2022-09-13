/********************************************************************************
 * Copyright (c) 2021,2022 Contributors to https://github.com/lvermeulen/Keycloak.Net.git and BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using CatenaX.NetworkServices.Keycloak.Library.Common.Extensions;
using CatenaX.NetworkServices.Keycloak.Library.Models.Groups;
using CatenaX.NetworkServices.Keycloak.Library.Models.Users;
using Flurl.Http;
using Newtonsoft.Json;

namespace CatenaX.NetworkServices.Keycloak.Library;

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
		string? locationPathAndQuery = response.ResponseMessage.Headers.Location?.PathAndQuery;
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
			return new SetPasswordResponse {Success = true};

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
