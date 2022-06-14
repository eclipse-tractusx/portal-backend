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

        /// <summary>
        /// Creates new users for the company of the current user
        /// </summary>
        /// <param name="usersToCreate">the users that should be created</param>
        /// <returns>Returns the emails of the new users</returns>
        /// <response code="200">Returns the emails of the new users.</response>
        /// <response code="401">User is unauthorized.</response>
        [HttpPost]
        [Authorize(Roles = "add_user_account")]
        [Route("owncompany/users")]
        [ProducesResponseType(typeof(IAsyncEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType( StatusCodes.Status401Unauthorized)]
        public IAsyncEnumerable<string> CreateOwnCompanyUsers([FromBody] IEnumerable<UserCreationInfo> usersToCreate) =>
            this.WithIamUserId(createdByName => _logic.CreateOwnCompanyUsersAsync(usersToCreate, createdByName));

        /// <summary>
        /// Gets the user data for the current users company.
        /// </summary>
        /// <param name="userEntityId"></param>
        /// <param name="companyUserId"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="email"></param>
        /// <param name="status"></param>
        /// <returns>Returns a list of company user data for the current users company.</returns>
        /// <response code="200">Returns a list of company user data for the current users company.</response>
        /// <response code="401">User is unauthorized.</response>
        [HttpGet]
        [Authorize(Roles = "view_user_management")]
        [Route("owncompany/users")]
        [ProducesResponseType(typeof(IAsyncEnumerable<CompanyUserData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        /// <summary>
        /// Gets the user details for the given user Id
        /// </summary>
        /// <param name="companyUserId">Id of the user to get the details for.</param>
        /// <returns>Returns the company user details.</returns>
        /// <response code="200">Returns the company user details.</response>
        /// <response code="401">User is unauthorized.</response>
        [HttpGet]
        [Authorize(Roles = "view_user_management")]
        [Route("owncompany/users/{companyUserId}")]
        [ProducesResponseType(typeof(CompanyUserDetails), StatusCodes.Status200OK)]
        [ProducesResponseType( StatusCodes.Status401Unauthorized)]
        public Task<CompanyUserDetails> GetOwnCompanyUserDetails([FromRoute] Guid companyUserId) =>
            this.WithIamUserId(iamUserId => _logic.GetOwnCompanyUserDetails(companyUserId, iamUserId));

        /// <summary>
        /// Adds the given business partner numbers to the user for the given id.
        /// </summary>
        /// <param name="companyUserId">Id of the user to add the business partner numbers to.</param>
        /// <param name="businessPartnerNumbers">the business partner numbers that should be added.</param>
        /// <returns></returns>
        /// <response code="200">The business partner numbers have been added successfully.</response>
        /// <response code="401">User is unauthorized.</response>
        [HttpPost]
        [Authorize(Roles = "modify_user_account")]
        [Route("owncompany/users/{companyUserId}/businessPartnerNumbers")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType( StatusCodes.Status401Unauthorized)]
        public Task<int> AddOwnCompanyUserBusinessPartnerNumbers(Guid companyUserId, IEnumerable<string> businessPartnerNumbers) =>
            this.WithIamUserId(iamUserId => _logic.AddOwnCompanyUsersBusinessPartnerNumbersAsync(companyUserId, businessPartnerNumbers, iamUserId));

        /// <summary>
        /// Adds the given business partner number to the user for the given id.
        /// </summary>
        /// <param name="companyUserId">Id of the user to add the business partner numbers to.</param>
        /// <param name="businessPartnerNumber">the business partner number that should be added.</param>
        /// <returns></returns>
        /// <response code="200">The business partner number have been added successfully.</response>
        /// <response code="401">User is unauthorized.</response>
        [HttpPut]
        [Authorize(Roles = "modify_user_account")]
        [Route("owncompany/users/{companyUserId}/businessPartnerNumbers/{businessPartnerNumber}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<int> AddOwnCompanyUserBusinessPartnerNumber(Guid companyUserId, string businessPartnerNumber) =>
            this.WithIamUserId(iamUserId => _logic.AddOwnCompanyUsersBusinessPartnerNumberAsync(companyUserId, businessPartnerNumber, iamUserId));

        /// <summary>
        /// Deletes the users with the given ids.
        /// </summary>
        /// <param name="usersToDelete">The ids of the users that should be deleted.</param>
        /// <returns></returns>
        /// <response code="200">Users have successfully been deleted.</response>
        /// <response code="401">User is unauthorized.</response>
        [HttpDelete]
        [Authorize(Roles = "delete_user_account")]
        [Route("owncompany/users")]
        [ProducesResponseType(typeof(IAsyncEnumerable<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType( StatusCodes.Status401Unauthorized)]
        public IAsyncEnumerable<Guid> DeleteOwnCompanyUsers([FromBody] IEnumerable<Guid> usersToDelete) =>
            this.WithIamUserId(adminUserId => _logic.DeleteOwnCompanyUsersAsync(usersToDelete, adminUserId));

        /// <summary>
        /// Resets the password for the given user
        /// </summary>
        /// <param name="companyUserId">Id of the user whose password should be reset.</param>
        /// <returns></returns>
        /// <response code="200">Returns <c>true</c> if the password was reset, <c>false</c> if it hasn't been reset.</response>
        /// <response code="401">User is unauthorized.</response>
        [HttpPut]
        [Authorize(Roles = "modify_user_account")]
        [Route("owncompany/users/{companyUserId}/resetPassword")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<bool> ResetOwnCompanyUserPassword([FromRoute] Guid companyUserId) =>
            this.WithIamUserId(adminUserId => _logic.ExecuteOwnCompanyUserPasswordReset(companyUserId, adminUserId));

        /// <summary>
        /// Gets the client roles for the given app.
        /// </summary>
        /// <param name="appId">Id of the app which roles should be returned.</param>
        /// <param name="languageShortName">OPTIONAL: The language short name.</param>
        /// <returns>Returns the client roles for the given app.</returns>
        /// <response code="200">Returns the client roles.</response>
        /// <response code="401">User is unauthorized.</response>
        [HttpGet]
        [Authorize(Roles = "view_client_roles")]
        [Route("app/{appId}/roles")]
        [ProducesResponseType(typeof(IAsyncEnumerable<ClientRoles>), StatusCodes.Status200OK)]
        [ProducesResponseType( StatusCodes.Status401Unauthorized)]
        public IAsyncEnumerable<ClientRoles> GetClientRolesAsync([FromRoute] Guid appId, string? languageShortName = null) =>
            _logic.GetClientRolesAsync(appId,languageShortName);

        /// <summary>
        /// Gets the user details for the current user.
        /// </summary>
        /// <returns>Returns the company user details.</returns>
        /// <response code="200">Returns the company user details.</response>
        [HttpGet]
        [Route("ownUser")]
        [ProducesResponseType(typeof(CompanyUserDetails), StatusCodes.Status200OK)]
        public Task<CompanyUserDetails> GetOwnUserDetails() =>
            this.WithIamUserId(iamUserId => _logic.GetOwnUserDetails(iamUserId));

        /// <summary>
        /// Updates the user details for the given companyUserId.
        /// </summary>
        /// <param name="companyUserId">Id of the user that should be updated.</param>
        /// <param name="ownCompanyUserEditableDetails">The new details for the user.</param>
        /// <returns>Returns the updated company user details</returns>
        /// <response code="200">Returns the updated company user details.</response>
        [HttpPut]
        [Route("ownUser/{companyUserId}")]
        [ProducesResponseType(typeof(CompanyUserDetails), StatusCodes.Status200OK)]
        public Task<CompanyUserDetails> UpdateOwnUserDetails([FromRoute] Guid companyUserId, [FromBody] OwnCompanyUserEditableDetails ownCompanyUserEditableDetails) =>
            this.WithIamUserId(iamUserId => _logic.UpdateOwnUserDetails(companyUserId, ownCompanyUserEditableDetails, iamUserId));

        /// <summary>
        /// Deletes the own user
        /// </summary>
        /// <param name="companyUserId">Id of the user that should be deleted</param>
        /// <returns></returns>
        /// <response code="200">Successfully deleted the user.</response>
        [HttpDelete]
        [Route("ownUser/{companyUserId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
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
    }
}
