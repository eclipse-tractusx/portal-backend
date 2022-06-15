using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
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

        /// <summary>
        /// Creates a new instance of <see cref="UserController"/> 
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="logic">The User Business Logic</param>
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
        /// <remarks>Example: POST: api/administration/user/owncompany/users</remarks>
        /// <response code="200">Returns the emails of the new users.</response>
        /// <response code="400">Invalid Role.</response>
        /// <response code="401">User is unauthorized.</response>
        /// <response code="500">Internal server error occured, e.g. a database error.</response>
        [HttpPost]
        [Authorize(Roles = "add_user_account")]
        [Route("owncompany/users")]
        [ProducesResponseType(typeof(IAsyncEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType( StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
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
        /// <remarks>Example: GET: api/administration/user/owncompany/users</remarks>
        /// <response code="200">Returns a list of company user data for the current users company.</response>
        /// <response code="401">User is unauthorized.</response>
        /// <response code="500">Internal server error occured, e.g. a database error.</response>
        [HttpGet]
        [Authorize(Roles = "view_user_management")]
        [Route("owncompany/users")]
        [ProducesResponseType(typeof(IAsyncEnumerable<CompanyUserData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
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
        /// <param name="companyUserId" example="ac1cf001-7fbc-1f2f-817f-bce0575a0011">Id of the user to get the details for.</param>
        /// <returns>Returns the company user details.</returns>
        /// <remarks>Example: GET: api/administration/user/owncompany/users/ac1cf001-7fbc-1f2f-817f-bce0575a0011</remarks>
        /// <response code="200">Returns the company user details.</response>
        /// <response code="401">User is unauthorized.</response>
        /// <response code="404">No company-user data found</response>
        /// <response code="500">Internal server error occured, e.g. a database error.</response>
        [HttpGet]
        [Authorize(Roles = "view_user_management")]
        [Route("owncompany/users/{companyUserId}")]
        [ProducesResponseType(typeof(CompanyUserDetails), StatusCodes.Status200OK)]
        [ProducesResponseType( StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public Task<CompanyUserDetails> GetOwnCompanyUserDetails([FromRoute] Guid companyUserId) =>
            this.WithIamUserId(iamUserId => _logic.GetOwnCompanyUserDetails(companyUserId, iamUserId));

        /// <summary>
        /// Adds the given business partner numbers to the user for the given id.
        /// </summary>
        /// <param name="companyUserId" example="ac1cf001-7fbc-1f2f-817f-bce0575a0011">Id of the user to add the business partner numbers to.</param>
        /// <param name="businessPartnerNumbers">the business partner numbers that should be added.</param>
        /// <returns></returns>
        /// <remarks>Example: POST: api/administration/user/owncompany/users/ac1cf001-7fbc-1f2f-817f-bce0575a0011/businessPartnerNumbers</remarks>
        /// <response code="200">The business partner numbers have been added successfully.</response>
        /// <response code="400">Business Partner Numbers must not exceed 20 characters.</response>
        /// <response code="401">User is unauthorized.</response>
        /// <response code="404">User not found in company.</response>
        /// <response code="500">Internal server error occured, e.g. a database error.</response>
        [HttpPost]
        [Authorize(Roles = "modify_user_account")]
        [Route("owncompany/users/{companyUserId}/businessPartnerNumbers")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public Task<int> AddOwnCompanyUserBusinessPartnerNumbers(Guid companyUserId, IEnumerable<string> businessPartnerNumbers) =>
            this.WithIamUserId(iamUserId => _logic.AddOwnCompanyUsersBusinessPartnerNumbersAsync(companyUserId, businessPartnerNumbers, iamUserId));

        /// <summary>
        /// Adds the given business partner number to the user for the given id.
        /// </summary>
        /// <param name="companyUserId" example="ac1cf001-7fbc-1f2f-817f-bce0575a0011">Id of the user to add the business partner numbers to.</param>
        /// <param name="businessPartnerNumber" example="CAXSDUMMYCATENAZZ">the business partner number that should be added.</param>
        /// <returns></returns>
        /// <remarks>Example: PUT: api/administration/user/owncompany/users/ac1cf001-7fbc-1f2f-817f-bce0575a0011/businessPartnerNumbers/CAXSDUMMYCATENAZZ</remarks>
        /// <response code="200">The business partner number have been added successfully.</response>
        /// <response code="400">Business Partner Numbers must not exceed 20 characters.</response>
        /// <response code="401">User is unauthorized.</response>
        /// <response code="404">User not found in company.</response>
        /// <response code="500">Internal server error occured, e.g. a database error.</response>
        [HttpPut]
        [Authorize(Roles = "modify_user_account")]
        [Route("owncompany/users/{companyUserId}/businessPartnerNumbers/{businessPartnerNumber}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public Task<int> AddOwnCompanyUserBusinessPartnerNumber(Guid companyUserId, string businessPartnerNumber) =>
            this.WithIamUserId(iamUserId => _logic.AddOwnCompanyUsersBusinessPartnerNumberAsync(companyUserId, businessPartnerNumber, iamUserId));

        /// <summary>
        /// Deletes the users with the given ids.
        /// </summary>
        /// <param name="usersToDelete">The ids of the users that should be deleted.</param>
        /// <returns></returns>
        /// <remarks>Example: DELETE: api/administration/user/owncompany/users</remarks>
        /// <response code="200">Users have successfully been deleted.</response>
        /// <response code="401">User is unauthorized.</response>
        /// <response code="500">Internal server error occured, e.g. a database error.</response>
        [HttpDelete]
        [Authorize(Roles = "delete_user_account")]
        [Route("owncompany/users")]
        [ProducesResponseType(typeof(IAsyncEnumerable<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType( StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public IAsyncEnumerable<Guid> DeleteOwnCompanyUsers([FromBody] IEnumerable<Guid> usersToDelete) =>
            this.WithIamUserId(adminUserId => _logic.DeleteOwnCompanyUsersAsync(usersToDelete, adminUserId));

        /// <summary>
        /// Resets the password for the given user
        /// </summary>
        /// <param name="companyUserId" example="ac1cf001-7fbc-1f2f-817f-bce0575a0011">Id of the user whose password should be reset.</param>
        /// <returns></returns>
        /// <remarks>Example: PUT: api/administration/user/owncompany/users/ac1cf001-7fbc-1f2f-817f-bce0575a0011/resetPassword</remarks>
        /// <response code="200">Returns <c>true</c> if the password was reset, <c>false</c> if it hasn't been reset.</response>
        /// <response code="400">The password was reset to often</response>
        /// <response code="401">User is unauthorized.</response>
        /// <response code="404">Cannot identify companyId or shared idp</response>
        /// <response code="500">Internal server error occured, e.g. a database error.</response>
        [HttpPut]
        [Authorize(Roles = "modify_user_account")]
        [Route("owncompany/users/{companyUserId}/resetPassword")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public Task<bool> ResetOwnCompanyUserPassword([FromRoute] Guid companyUserId) =>
            this.WithIamUserId(adminUserId => _logic.ExecuteOwnCompanyUserPasswordReset(companyUserId, adminUserId));

        /// <summary>
        /// Gets the client roles for the given app.
        /// </summary>
        /// <param name="appId">Id of the app which roles should be returned.</param>
        /// <param name="languageShortName">OPTIONAL: The language short name.</param>
        /// <returns>Returns the client roles for the given app.</returns>
        /// <remarks>Example: GET: api/administration/user/owncompany/users/ac1cf001-7fbc-1f2f-817f-bce0575a0011/resetPassword</remarks>
        /// <response code="200">Returns the client roles.</response>
        /// <response code="401">User is unauthorized.</response>
        /// <response code="404">Either the app was not found or the language does not exist.</response>
        /// <response code="500">Internal server error occured, e.g. a database error.</response>
        [HttpGet]
        [Authorize(Roles = "view_client_roles")]
        [Route("app/{appId}/roles")]
        [ProducesResponseType(typeof(IAsyncEnumerable<ClientRoles>), StatusCodes.Status200OK)]
        [ProducesResponseType( StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public IAsyncEnumerable<ClientRoles> GetClientRolesAsync([FromRoute] Guid appId, string? languageShortName = null) =>
            _logic.GetClientRolesAsync(appId,languageShortName);

        /// <summary>
        /// Gets the user details for the current user.
        /// </summary>
        /// <returns>Returns the company user details.</returns>
        /// <remarks>Example: GET: api/administration/user/ownUser</remarks>
        /// <response code="200">Returns the company user details.</response>
        /// <response code="404">no company-user data found for user</response>
        /// <response code="500">Internal server error occured, e.g. a database error.</response>
        [HttpGet]
        [Route("ownUser")]
        [ProducesResponseType(typeof(CompanyUserDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public Task<CompanyUserDetails> GetOwnUserDetails() =>
            this.WithIamUserId(iamUserId => _logic.GetOwnUserDetails(iamUserId));

        /// <summary>
        /// Updates the user details for the given companyUserId.
        /// </summary>
        /// <param name="companyUserId" example="ac1cf001-7fbc-1f2f-817f-bce0575a0011">Id of the user that should be updated.</param>
        /// <param name="ownCompanyUserEditableDetails">The new details for the user.</param>
        /// <returns>Returns the updated company user details</returns>
        /// <remarks>Example: PUT: api/administration/user/ownUser/ac1cf001-7fbc-1f2f-817f-bce0575a0011</remarks>
        /// <response code="200">Returns the updated company user details.</response>
        /// <response code="403">invalid company User Id for user</response>
        /// <response code="404">no shared realm userid found for the id in realm</response>
        /// <response code="500">Internal server error occured, e.g. a database error.</response>
        [HttpPut]
        [Route("ownUser/{companyUserId}")]
        [ProducesResponseType(typeof(CompanyUserDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public Task<CompanyUserDetails> UpdateOwnUserDetails([FromRoute] Guid companyUserId, [FromBody] OwnCompanyUserEditableDetails ownCompanyUserEditableDetails) =>
            this.WithIamUserId(iamUserId => _logic.UpdateOwnUserDetails(companyUserId, ownCompanyUserEditableDetails, iamUserId));

        /// <summary>
        /// Deletes the own user
        /// </summary>
        /// <param name="companyUserId" example="ac1cf001-7fbc-1f2f-817f-bce0575a0011">Id of the user that should be deleted</param>
        /// <returns></returns>
        /// <remarks>Example: DELETE: api/administration/user/ownUser/ac1cf001-7fbc-1f2f-817f-bce0575a0011</remarks>
        /// <response code="200">Successfully deleted the user.</response>
        /// <response code="403">invalid company User Id for user</response>
        /// <response code="500">Internal server error occured, e.g. a database error.</response>
        [HttpDelete]
        [Route("ownUser/{companyUserId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
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
