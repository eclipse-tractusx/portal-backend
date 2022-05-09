using System.Net;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;

namespace CatenaX.NetworkServices.Administration.Service.Controllers
{
    [ApiController]
    [Route("api/administration/user")]
    public class UserController : ControllerBase
    {

        private readonly ILogger<UserController> _logger;
        private readonly IUserBusinessLogic _logic;
        public UserController(ILogger<UserController> logger, IUserBusinessLogic logic)
        {
            _logger = logger;
            _logic = logic;
        }

        [HttpPost]
        [Authorize(Policy = "CheckTenant")]
        [Authorize(Roles = "add_user_account")]
        [Route("tenant/{tenant}/users")]
        public Task<IEnumerable<string>> ExecuteUserCreation([FromRoute] string tenant, [FromBody] IEnumerable<UserCreationInfo> usersToCreate) =>
            WithIamUserId(createdByName => _logic.CreateTenantUsersAsync(usersToCreate, tenant, createdByName));

        [HttpPost]
        [Authorize(Roles = "add_user_account")]
        [Route("company/{companyId}/users")]
        public Task<IEnumerable<string>> ExecuteCompanyUserCreation([FromRoute] Guid companyId, [FromBody] IEnumerable<UserCreationInfo> usersToCreate) =>
            WithIamUserId(createdByName => _logic.CreateCompanyUsersAsync(usersToCreate, companyId, createdByName));

        [HttpGet]
        [Authorize(Policy = "CheckTenant")]
        [Authorize(Roles = "view_user_management")]
        [Route("tenant/{tenant}/users")]
        public Task<IEnumerable<JoinedUserInfo>> QueryJoinedUsers(
                [FromRoute] string tenant,
                [FromQuery] string? userId = null,
                [FromQuery] string? providerUserId = null,
                [FromQuery] string? userName = null,
                [FromQuery] string? firstName = null,
                [FromQuery] string? lastName = null,
                [FromQuery] string? email = null
            ) => _logic.GetUsersAsync(tenant, userId, providerUserId, userName, firstName, lastName, email);

        [HttpGet]
        [Authorize(Roles = "view_client_roles")]
        [Route("client/{clientId}/roles")]
        public Task<IEnumerable<string>> ReturnRoles([FromRoute] string clientId) =>
            _logic.GetAppRolesAsync(clientId);

        [HttpDelete]
        [Authorize(Policy = "CheckTenant")]
        [Route("tenant/{tenant}/ownUser")]
        public Task ExecuteOwnUserDeletion([FromRoute] string tenant) =>
            WithIamUserId(userName => _logic.DeleteUserAsync(tenant, userName));

        [HttpDelete]
        [Authorize(Policy = "CheckTenant")]
        [Authorize(Roles = "delete_user_account")]
        [Route("tenant/{tenant}/users")]
        public Task ExecuteUserDeletion([FromRoute] string tenant, [FromBody] UserIds usersToDelete) =>
            _logic.DeleteUsersAsync(usersToDelete, tenant);

        [HttpPut]
        [Authorize(Roles = "approve_new_partner")]
        [Route("company/{companyId}/bpnAtRegistrationApproval")]
        public Task BpnAttributeAddingAtRegistrationApproval([FromRoute] Guid companyId) =>
            _logic.AddBpnAttributeAtRegistrationApprovalAsync(companyId);

        [HttpPut]
        [Authorize(Roles = "modify_user_account")]
        [Route("bpn")]
        public Task BpnAttributeAdding([FromBody] IEnumerable<UserUpdateBpn> usersToAddBpn) =>
            _logic.AddBpnAttributeAsync(usersToAddBpn);

        //TODO: full functionality is not yet delivered and currently the service is working with a submitted Json file
        [HttpPost]
        [Authorize(Roles = "approve_new_partner")]
        [Route("welcomeEmail")]
        public async Task<IActionResult> PostRegistrationWelcomeEmailAsync([FromBody] WelcomeData welcomeData)
        {
            try
            {
                if (await _logic.PostRegistrationWelcomeEmailAsync(welcomeData).ConfigureAwait(false))
                {
                    return Ok();
                }
                _logger.LogError("unsuccessful");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut]
        [Authorize(Policy = "CheckTenant")]
        [Authorize(Roles = "modify_user_account")]
        [Route("tenant/{tenant}/users/{userId}/resetpassword")]
        public async Task<IActionResult> ResetUserPassword([FromRoute] string tenant, [FromRoute] string userId)
        {
            try
            {
                var adminuserId = User.Claims.SingleOrDefault(x => x.Type == "sub").Value as string;
                if (await _logic.CanResetPassword(adminuserId).ConfigureAwait(false))
                {
                    var updatedPassword = await _logic.ResetUserPasswordAsync(tenant, userId).ConfigureAwait(false);
                    if (!updatedPassword)
                    {
                        return StatusCode((int)HttpStatusCode.InternalServerError);
                    }
                    return Ok(updatedPassword);

                }
                return StatusCode((int)HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        private T WithIamUserId<T>(Func<string, T> _next)
        {
            var sub = User.Claims.SingleOrDefault(x => x.Type == "sub")?.Value as string;
            if (String.IsNullOrWhiteSpace(sub))
            {
                throw new ArgumentException("claim sub must not be null or empty","sub");
            }
            return _next(sub);
        }
    }
}
