using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public IAsyncEnumerable<string> CreateOwnCompanyUsers([FromBody] IEnumerable<UserCreationInfo> usersToCreate) =>
            this.WithIamUserId(createdByName => _logic.CreateOwnCompanyUsersAsync(usersToCreate, createdByName));

        [HttpGet]
        [Authorize(Roles = "view_user_management")]
        [Route("owncompany/users")]
        public IAsyncEnumerable<CompanyUserData> GetOwnCompanyUserDatasAsync(
            [FromQuery] string? userEntityId = null,
            [FromQuery] Guid? companyUserId = null,
            [FromQuery] string? firstName = null,
            [FromQuery] string? lastName = null,
            [FromQuery] string? email = null,
            [FromQuery] CompanyUserStatusId? status = null) =>
            this.WithIamUserId(adminUserId => _logic.GetOwnCompanyUserDatasAsync(
                adminUserId,
                companyUserId,
                userEntityId,
                firstName,
                lastName,
                email,
                status));

        [HttpGet]
        [Authorize(Roles = "view_user_management")]
        [Route("owncompany/users/{companyUserId}")]
        public Task<CompanyUserDetails> GetOwnCompanyUserDetails([FromRoute] Guid companyUserId) =>
            this.WithIamUserId(iamUserId => _logic.GetOwnCompanyUserDetails(companyUserId, iamUserId));

        [HttpDelete]
        [Authorize(Roles = "delete_user_account")]
        [Route("owncompany/users")]
        public IAsyncEnumerable<Guid> DeleteOwnCompanyUsers([FromBody] IEnumerable<Guid> usersToDelete) =>
            this.WithIamUserId(adminUserId => _logic.DeleteOwnCompanyUsersAsync(usersToDelete, adminUserId));

        [HttpPut]
        [Authorize(Roles = "modify_user_account")]
        [Route("owncompany/users/{companyUserId}/resetPassword")]
        public Task<bool> ResetOwnCompanyUserPassword([FromRoute] Guid companyUserId) =>
            this.WithIamUserId(adminUserId => _logic.ExecuteOwnCompanyUserPasswordReset(companyUserId, adminUserId));

        [HttpGet]
        [Authorize(Roles = "view_client_roles")]
        [Route("app/{appId}/roles")]
        public IAsyncEnumerable<ClientRoles> GetClientRolesAsync([FromRoute] Guid appId, string? languageShortName = null) =>
            _logic.GetClientRolesAsync(appId, languageShortName);

        [HttpGet]
        [Route("ownUser")]
        public Task<CompanyUserDetails> GetOwnUserDetails() =>
            this.WithIamUserId(iamUserId => _logic.GetOwnUserDetails(iamUserId));

        [HttpPut]
        [Route("ownUser/{companyUserId}")]
        public Task<CompanyUserDetails> UpdateOwnUserDetails([FromRoute] Guid companyUserId, [FromBody] OwnCompanyUserEditableDetails ownCompanyUserEditableDetails) =>
            this.WithIamUserId(iamUserId => _logic.UpdateOwnUserDetails(companyUserId, ownCompanyUserEditableDetails, iamUserId));

        [HttpDelete]
        [Route("ownUser/{companyUserId}")]
        public Task<int> DeleteOwnUser([FromRoute] Guid companyUserId) =>
            this.WithIamUserId(iamUserId => _logic.DeleteOwnUserAsync(companyUserId, iamUserId));

        [HttpPut]
        [Authorize(Roles = "modify_user_account")]
        [Route("bpn")]
        public Task BpnAttributeAdding([FromBody] IEnumerable<UserUpdateBpn> usersToAddBpn) =>
            _logic.AddBpnAttributeAsync(usersToAddBpn);

        [Obsolete]
        [HttpPut]
        [Authorize(Roles = "modify_user_account")]
        [Route("users/{companyUserId}/resetpassword")]
        public Task<bool> ResetUserPassword([FromRoute] Guid companyUserId) =>
            this.WithIamUserId(adminUserId => _logic.ExecuteOwnCompanyUserPasswordReset(companyUserId, adminUserId));

        [HttpPost]
        [Authorize(Roles = "modify_user_account")]
        [Route("users/{appId}/userrole")]
        public Task<string> AddUserRole([FromRoute] Guid appId, [FromBody] UserRoleInfo userRoleInfo) =>
            this.WithIamUserId(adminUserId => _logic.AddUserRoleAsync(appId, userRoleInfo, adminUserId));
    }
}
