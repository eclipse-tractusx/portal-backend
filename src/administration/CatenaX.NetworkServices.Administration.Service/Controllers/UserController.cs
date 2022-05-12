using System;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

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
        public async Task<IActionResult> ExecuteUserCreation([FromRoute] string tenant, [FromBody] IEnumerable<UserCreationInfo> usersToCreate)
        {
            try
            {
                var createdByName = User.Claims.SingleOrDefault(x => x.Type == "name").Value as string;
                var createdUsers = await _logic.CreateUsersAsync(usersToCreate, tenant, createdByName).ConfigureAwait(false);
                return Ok(createdUsers);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

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
        public async Task<IActionResult> ExecuteOwnUserDeletion([FromRoute] string tenant)
        {
            try
            {
                var userName = User.Claims.SingleOrDefault(x => x.Type == "sub")?.Value as string;
                await _logic.DeleteUserAsync(tenant, userName).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpDelete]
        [Authorize(Policy = "CheckTenant")]
        [Authorize(Roles = "delete_user_account")]
        [Route("tenant/{tenant}/users")]
        public async Task<IActionResult> ExecuteUserDeletion([FromRoute] string tenant, [FromBody] UserIds usersToDelete)
        {
            try
            {
                return Ok(await _logic.DeleteUsersAsync(usersToDelete, tenant).ConfigureAwait(false));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut]
        [Authorize(Roles = "approve_new_partner")]
        [Route("company/{companyId}/bpnAtRegistrationApproval")]
        public async Task<IActionResult> BpnAttributeAddingAtRegistrationApproval([FromRoute] Guid companyId)
        {
            try
            {
                return Ok(await _logic.AddBpnAttributeAtRegistrationApprovalAsync(companyId).ConfigureAwait(false));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut]
        [Authorize(Roles = "modify_user_account")]
        [Route("bpn")]
        public async Task<IActionResult> BpnAttributeAdding([FromBody] IEnumerable<UserUpdateBpn> usersToAddBpn)
        {
            try
            {
                return Ok(await _logic.AddBpnAttributeAsync(usersToAddBpn).ConfigureAwait(false));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [Authorize(Roles = "approve_new_partner")]
        [Route("application/{applicationId}/welcomeEmail")]
        public async Task<bool> PostRegistrationWelcomeEmailAsync([FromRoute] Guid applicationId) =>
            await _logic.PostRegistrationWelcomeEmailAsync(applicationId).ConfigureAwait(false);
                
        

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
    }
}
