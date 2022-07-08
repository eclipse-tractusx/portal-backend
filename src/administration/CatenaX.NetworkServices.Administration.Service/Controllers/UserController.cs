﻿using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.Models;
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
        
        /// <summary>
        /// Get Company User Data
        /// </summary>
        /// <param name="page">page index start from 0</param>
        /// <param name="size">size to get number of records</param>
        /// <param name="userEntityId">User Entity Id</param>
        /// <param name="companyUserId">Company User Id</param>
        /// <param name="firstName">First Name of User</param>
        /// <param name="lastName">Last Name of User</param>
        /// <param name="email">Email Id of User</param>
        /// <returns>Paginated Result of Company User Data</returns>
        /// <remarks> Example: GET: api/administration/user/owncompany/users?page=0&size=5</remarks>
        /// <remarks> Example: GET: api/administration/user/owncompany/users?page=0&size=5&userEntityId="31404026-64ee-4023-a122-3c7fc40e57b1"</remarks>
        /// <response code="200">Result as a Company User Data</response>
        [HttpGet]
        [Authorize(Roles = "view_user_management")]
        [Route("owncompany/users")]
        [ProducesResponseType(typeof(Pagination.Response<CompanyUserData>), StatusCodes.Status200OK)]
        public Task<Pagination.Response<CompanyUserData>> GetOwnCompanyUserDatasAsync(
            [FromQuery] int page, 
            [FromQuery] int size,
            [FromQuery] string? userEntityId = null,
            [FromQuery] Guid? companyUserId = null,
            [FromQuery] string? firstName = null,
            [FromQuery] string? lastName = null,
            [FromQuery] string? email = null) =>
            this.WithIamUserId(adminUserId => _logic.GetOwnCompanyUserDatasAsync(
                adminUserId,
                page,
                size,
                companyUserId,
                userEntityId,
                firstName,
                lastName,
                email));

        [HttpGet]
        [Authorize(Roles = "view_user_management")]
        [Route("owncompany/users/{companyUserId}")]
        public Task<CompanyUserDetails> GetOwnCompanyUserDetails([FromRoute] Guid companyUserId) =>
            this.WithIamUserId(iamUserId => _logic.GetOwnCompanyUserDetails(companyUserId, iamUserId));

        [HttpPost]
        [Authorize(Roles = "modify_user_account")]
        [Route("owncompany/users/{companyUserId}/businessPartnerNumbers")]
        public Task<int> AddOwnCompanyUserBusinessPartnerNumbers(Guid companyUserId, IEnumerable<string> businessPartnerNumbers) =>
            this.WithIamUserId(iamUserId => _logic.AddOwnCompanyUsersBusinessPartnerNumbersAsync(companyUserId, businessPartnerNumbers, iamUserId));

        [HttpPut]
        [Authorize(Roles = "modify_user_account")]
        [Route("owncompany/users/{companyUserId}/businessPartnerNumbers/{businessPartnerNumber}")]
        public Task<int> AddOwnCompanyUserBusinessPartnerNumber(Guid companyUserId, string businessPartnerNumber) =>
            this.WithIamUserId(iamUserId => _logic.AddOwnCompanyUsersBusinessPartnerNumberAsync(companyUserId, businessPartnerNumber, iamUserId));

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

        [Obsolete]
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

        [HttpGet]
        [Authorize(Roles = "view_user_management")]
        [Route("owncompany/apps/{appId}/users")]
        public Task<Pagination.Response<CompanyAppUserDetails>> GetCompanyAppUsersAsync([FromRoute] Guid appId,[FromQuery] int page = 0, [FromQuery] int size = 15) =>
            this.WithIamUserId(iamUserId => _logic.GetCompanyAppUsersAsync(appId,iamUserId, page, size));
            
        [HttpPost]
        [Authorize(Roles = "modify_user_account")]
        [Route("app/{appId}/roles")]
        public Task<UserRoleMessage> AddUserRole([FromRoute] Guid appId, [FromBody] UserRoleInfo userRoleInfo) =>
            this.WithIamUserId(adminUserId => _logic.AddUserRoleAsync(appId, userRoleInfo, adminUserId));
    }
}
