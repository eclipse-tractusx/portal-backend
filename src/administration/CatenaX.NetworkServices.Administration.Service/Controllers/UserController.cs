using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
        [Authorize(Roles = "add_user_account")]
        [Route("owncompany/users")]
        public IAsyncEnumerable<string> ExecuteCompanyUserCreation([FromBody] IEnumerable<UserCreationInfo> usersToCreate) =>
            WithIamUserId(createdByName => _logic.CreateOwnCompanyUsersAsync(usersToCreate, createdByName));

        [HttpGet]
        [Authorize(Roles = "view_user_management")]
        [Route("owncompany/users")]
        public IAsyncEnumerable<CompanyUserDetails> GetCompanyUserDetailsAsync(
            [FromQuery] string? userEntityId = null,
            [FromQuery] Guid? companyUserId = null,
            [FromQuery] string? firstName = null,
            [FromQuery] string? lastName = null,
            [FromQuery] string? email = null,
            [FromQuery] CompanyUserStatusId? status = null) =>
            WithIamUserId(adminUserId => _logic.GetCompanyUserDetailsAsync(
                adminUserId,
                companyUserId,
                userEntityId,
                firstName,
                lastName,
                email,
                status));

        [HttpGet]
        [Authorize(Roles = "view_client_roles")]
        [Route("client/{clientId}/roles")]
        public Task<IEnumerable<string>> ReturnRoles([FromRoute] string clientId) =>
            _logic.GetAppRolesAsync(clientId);

        [HttpDelete]
        [Route("owncompany/ownUser")]
        public Task<int> ExecuteOwnUserDeletion() =>
            WithIamUserId(iamUserId => _logic.DeleteUserAsync(iamUserId));

        [HttpDelete]
        [Authorize(Roles = "delete_user_account")]
        [Route("owncompany/users")]
        public IAsyncEnumerable<Guid> ExecuteUserDeletion([FromBody] IEnumerable<Guid> usersToDelete) =>
            WithIamUserId(adminUserId => _logic.DeleteUsersAsync(usersToDelete, adminUserId));

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

        [HttpPost]
        [Authorize(Roles = "approve_new_partner")]
        [Route("application/{applicationId}/welcomeEmail")]
        public Task<bool> PostRegistrationWelcomeEmailAsync([FromRoute] Guid applicationId) =>
             _logic.PostRegistrationWelcomeEmailAsync(applicationId);
                
        

        [HttpPut]
        [Authorize(Roles = "modify_user_account")]
        [Route("tenant/{tenant}/users/{companyUserId}/resetpassword")]
        public Task<bool> ResetUserPassword([FromRoute] string tenant, [FromRoute] Guid companyUserId) =>
            WithIamUserId(adminUserId => _logic.ExecutePasswordReset(companyUserId, adminUserId, tenant));

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
