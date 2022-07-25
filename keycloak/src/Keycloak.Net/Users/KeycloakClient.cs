using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Keycloak.Net.Common.Extensions;
using Keycloak.Net.Models.Groups;
using Keycloak.Net.Models.Users;
using Newtonsoft.Json;

namespace Keycloak.Net
{
	public partial class KeycloakClient
	{
		public async Task<bool> CreateUserAsync(string realm, User user)
		{
			var response = await InternalCreateUserAsync(realm, user).ConfigureAwait(false);
			return response.IsSuccessStatusCode;
		}

		private async Task<HttpResponseMessage> InternalCreateUserAsync(string realm, User user) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users")
			.PostJsonAsync(user)
			.ConfigureAwait(false);

		public async Task<string> CreateAndRetrieveUserIdAsync(string realm, User user)
		{
			var response = await InternalCreateUserAsync(realm, user).ConfigureAwait(false);
			string locationPathAndQuery = response.Headers.Location.PathAndQuery;
			string userId = response.IsSuccessStatusCode ? locationPathAndQuery.Substring(locationPathAndQuery.LastIndexOf("/", StringComparison.Ordinal) + 1) : null;
			return userId;
		}

		public async Task<IEnumerable<User>> GetUsersAsync(string realm, bool? briefRepresentation = null, string email = null, int? first = null,
			string firstName = null, string lastName = null, int? max = null, string search = null, string username = null)
		{
			var queryParams = new Dictionary<string, object>
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

		public async Task<int> GetUsersCountAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/count")
			.GetJsonAsync<int>()
			.ConfigureAwait(false);

		public async Task<User> GetUserAsync(string realm, string userId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.GetJsonAsync<User>()
			.ConfigureAwait(false);

		public async Task<bool> UpdateUserAsync(string realm, string userId, User user)
		{
			var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
				.AppendPathSegment("/admin/realms/")
				.AppendPathSegment(realm, true)
				.AppendPathSegment("/users/")
				.AppendPathSegment(userId, true)
				.PutJsonAsync(user)
				.ConfigureAwait(false);
			return response.IsSuccessStatusCode;
		}

		public async Task<bool> DeleteUserAsync(string realm, string userId)
		{
			var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
				.AppendPathSegment("/admin/realms/")
				.AppendPathSegment(realm, true)
				.AppendPathSegment("/users/")
				.AppendPathSegment(userId, true)
				.DeleteAsync()
				.ConfigureAwait(false);
			return response.IsSuccessStatusCode;
		}

		[Obsolete("Not working yet")]
		public async Task<string> GetUserConsentsAsync(string realm, string userId)
		{
			return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
				.AppendPathSegment("/admin/realms/")
				.AppendPathSegment(realm, true)
				.AppendPathSegment("/users/")
				.AppendPathSegment(userId, true)
				.AppendPathSegment("/consents")
				.GetStringAsync()
				.ConfigureAwait(false);
		}

		public async Task<bool> RevokeUserConsentAndOfflineTokensAsync(string realm, string userId, string clientId)
		{
			var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
				.AppendPathSegment("/admin/realms/")
				.AppendPathSegment(realm, true)
				.AppendPathSegment("/users/")
				.AppendPathSegment(userId, true)
				.AppendPathSegment("/consents/")
				.AppendPathSegment(clientId, true)
				.DeleteAsync()
				.ConfigureAwait(false);
			return response.IsSuccessStatusCode;
		}

		public async Task<bool> DisableUserCredentialsAsync(string realm, string userId, IEnumerable<string> credentialTypes)
		{
			var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
				.AppendPathSegment("/admin/realms/")
				.AppendPathSegment(realm, true)
				.AppendPathSegment("/users/")
				.AppendPathSegment(userId, true)
				.AppendPathSegment("/disable-credential-types")
				.PutJsonAsync(credentialTypes)
				.ConfigureAwait(false);
			return response.IsSuccessStatusCode;
		}

		public async Task<bool> SendUserUpdateAccountEmailAsync(string realm, string userId, IEnumerable<string> requiredActions, string clientId = null, int? lifespan = null, string redirectUri = null)
		{
			var queryParams = new Dictionary<string, object>
			{
				["client_id"] = clientId,
				[nameof(lifespan)] = lifespan,
				["redirect_uri"] = redirectUri
			};

			var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
				.AppendPathSegment("/admin/realms/")
				.AppendPathSegment(realm, true)
				.AppendPathSegment("/users/")
				.AppendPathSegment(userId, true)
				.AppendPathSegment("/execute-actions-email")
				.SetQueryParams(queryParams)
				.PutJsonAsync(requiredActions)
				.ConfigureAwait(false);
			return response.IsSuccessStatusCode;
		}

		public async Task<IEnumerable<FederatedIdentity>> GetUserSocialLoginsAsync(string realm, string userId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/federated-identity")
			.GetJsonAsync<IEnumerable<FederatedIdentity>>()
			.ConfigureAwait(false);

		public async Task<bool> AddUserSocialLoginProviderAsync(string realm, string userId, string provider, FederatedIdentity federatedIdentity)
		{
			var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
				.AppendPathSegment("/admin/realms/")
				.AppendPathSegment(realm, true)
				.AppendPathSegment("/users/")
				.AppendPathSegment(userId, true)
				.AppendPathSegment("/federated-identity/")
				.AppendPathSegment(provider, true)
				.PostJsonAsync(federatedIdentity)
				.ConfigureAwait(false);
			return response.IsSuccessStatusCode;
		}

		public async Task<bool> RemoveUserSocialLoginProviderAsync(string realm, string userId, string provider)
		{
			var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
				.AppendPathSegment("/admin/realms/")
				.AppendPathSegment(realm, true)
				.AppendPathSegment("/users/")
				.AppendPathSegment(userId, true)
				.AppendPathSegment("/federated-identity/")
				.AppendPathSegment(provider, true)
				.DeleteAsync()
				.ConfigureAwait(false);
			return response.IsSuccessStatusCode;
		}

		public async Task<IEnumerable<Group>> GetUserGroupsAsync(string realm, string userId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/groups")
			.GetJsonAsync<IEnumerable<Group>>()
			.ConfigureAwait(false);

		public async Task<int> GetUserGroupsCountAsync(string realm, string userId)
		{
			var result = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
				.AppendPathSegment("/admin/realms/")
				.AppendPathSegment(realm, true)
				.AppendPathSegment("/users/")
				.AppendPathSegment(userId, true)
				.AppendPathSegment("/groups/count")
				.GetJsonAsync()
				.ConfigureAwait(false);
			return Convert.ToInt32(DynamicExtensions.GetFirstPropertyValue(result));
		}

		public async Task<bool> UpdateUserGroupAsync(string realm, string userId, string groupId, Group group)
		{
			var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
				.AppendPathSegment("/admin/realms/")
				.AppendPathSegment(realm, true)
				.AppendPathSegment("/users/")
				.AppendPathSegment(userId, true)
				.AppendPathSegment("/groups/")
				.AppendPathSegment(groupId, true)
				.PutJsonAsync(group)
				.ConfigureAwait(false);
			return response.IsSuccessStatusCode;
		}

		public async Task<bool> DeleteUserGroupAsync(string realm, string userId, string groupId)
		{
			var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
				.AppendPathSegment("/admin/realms/")
				.AppendPathSegment(realm, true)
				.AppendPathSegment("/users/")
				.AppendPathSegment(userId, true)
				.AppendPathSegment("/groups/")
				.AppendPathSegment(groupId, true)
				.DeleteAsync()
				.ConfigureAwait(false);
			return response.IsSuccessStatusCode;
		}

		public async Task<IDictionary<string, object>> ImpersonateUserAsync(string realm, string userId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/impersonation")
			.PostAsync(new StringContent(""))
			.ReceiveJson<IDictionary<string, object>>()
			.ConfigureAwait(false);

		public async Task<bool> RemoveUserSessionsAsync(string realm, string userId)
		{
			var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
				.AppendPathSegment("/admin/realms/")
				.AppendPathSegment(realm, true)
				.AppendPathSegment("/users/")
				.AppendPathSegment(userId, true)
				.AppendPathSegment("/logout")
				.PostAsync(new StringContent(""))
				.ConfigureAwait(false);
			return response.IsSuccessStatusCode;
		}

		[Obsolete("Not working yet")]
		public async Task<IEnumerable<UserSession>> GetUserOfflineSessionsAsync(string realm, string userId, string clientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/offline-sessions/")
			.AppendPathSegment(clientId, true)
			.GetJsonAsync<IEnumerable<UserSession>>()
			.ConfigureAwait(false);

		public async Task<bool> RemoveUserTotpAsync(string realm, string userId)
		{
			var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
				.AppendPathSegment("/admin/realms/")
				.AppendPathSegment(realm, true)
				.AppendPathSegment("/users/")
				.AppendPathSegment(userId, true)
				.AppendPathSegment("/remove-totp")
				.PutAsync(new StringContent(""))
				.ConfigureAwait(false);
			return response.IsSuccessStatusCode;
		}

		public async Task<bool> ResetUserPasswordAsync(string realm, string userId, Credentials credentials)
		{
			var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
				.AppendPathSegment("/admin/realms/")
				.AppendPathSegment(realm, true)
				.AppendPathSegment("/users/")
				.AppendPathSegment(userId, true)
				.AppendPathSegment("/reset-password")
				.PutJsonAsync(credentials)
				.ConfigureAwait(false);
			return response.IsSuccessStatusCode;
		}

		public async Task<bool> ResetUserPasswordAsync(string realm, string userId, string password, bool temporary = true)
        {
            HttpResponseMessage response = await InternalResetUserPasswordAsync(realm, userId, password, temporary).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        private async Task<HttpResponseMessage> InternalResetUserPasswordAsync(string realm, string userId, string password, bool temporary)
        {
            var credentials = new Credentials
            {
                Value = password,
                Temporary = temporary
            };
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
				.AppendPathSegment(realm, true)
				.AppendPathSegment("/users/")
				.AppendPathSegment(userId, true)
				.AppendPathSegment("/reset-password")
                .PutJsonAsync(credentials)
                .ConfigureAwait(false);
            return response;
        }

        public async Task<SetPasswordResponse> SetUserPasswordAsync(string realm, string userId, string password)
        {
            var response = await InternalResetUserPasswordAsync(realm, userId, password, false);
            if (response.IsSuccessStatusCode)
                return new SetPasswordResponse {Success = response.IsSuccessStatusCode};

            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SetPasswordResponse>(jsonString);
        }

        public async Task<bool> VerifyUserEmailAddressAsync(string realm, string userId, string clientId = null, string redirectUri = null)
        {
            var queryParams = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(clientId))
            {
				queryParams.Add("client_id", clientId);
            }

            if (!string.IsNullOrEmpty(redirectUri))
            {
                queryParams.Add("redirect_uri", redirectUri);
            }

			var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
				.AppendPathSegment("/admin/realms/")
				.AppendPathSegment(realm, true)
				.AppendPathSegment("/users/")
				.AppendPathSegment(userId, true)
				.AppendPathSegment("/send-verify-email")
				.SetQueryParams(queryParams)
				.PutJsonAsync(null)
				.ConfigureAwait(false);
			return response.IsSuccessStatusCode;
		}

		public async Task<IEnumerable<UserSession>> GetUserSessionsAsync(string realm, string userId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users/")
			.AppendPathSegment(userId, true)
			.AppendPathSegment("/sessions")
			.GetJsonAsync<IEnumerable<UserSession>>()
			.ConfigureAwait(false);
	}
}
