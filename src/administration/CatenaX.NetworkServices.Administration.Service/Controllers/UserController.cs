﻿/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
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

using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Framework.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Administration.Service.Controllers
{
    [ApiController]
    [Route("api/administration/user")]
    [Produces("application/json")]
    [Consumes("application/json")]
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
        /// <response code="200">User successfully created and invite email send</response>
        /// <response code="400">Provided input is not sufficient.</response>
        [HttpPost]
        [Authorize(Roles = "add_user_account")]
        [Route("owncompany/users")]
        [ProducesResponseType(typeof(IAsyncEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
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

        /// <summary>
        /// Gets the user details for the given user Id
        /// </summary>
        /// <param name="companyUserId" example="ac1cf001-7fbc-1f2f-817f-bce0575a0011">Id of the user to get the details for.</param>
        /// <returns>Returns the company user details.</returns>
        /// <remarks>Example: GET: api/administration/user/owncompany/users/ac1cf001-7fbc-1f2f-817f-bce0575a0011</remarks>
        /// <response code="200">Returns the company user details.</response>
        /// <response code="404">User not found</response>
        [HttpGet]
        [Authorize(Roles = "view_user_management")]
        [Route("owncompany/users/{companyUserId}")]
        [ProducesResponseType(typeof(CompanyUserDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
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
        /// <response code="404">User not found.</response>
        [HttpPost]
        [Authorize(Roles = "modify_user_account")]
        [Route("owncompany/users/{companyUserId}/businessPartnerNumbers")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
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
        /// <response code="404">User is not existing.</response>
        [HttpPut]
        [Authorize(Roles = "modify_user_account")]
        [Route("owncompany/users/{companyUserId}/businessPartnerNumbers/{businessPartnerNumber}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public Task<int> AddOwnCompanyUserBusinessPartnerNumber(Guid companyUserId, string businessPartnerNumber) =>
            this.WithIamUserId(iamUserId => _logic.AddOwnCompanyUsersBusinessPartnerNumberAsync(companyUserId, businessPartnerNumber, iamUserId));

        /// <summary>
        /// Deletes the users with the given ids.
        /// </summary>
        /// <param name="usersToDelete">The ids of the users that should be deleted.</param>
        /// <returns></returns>
        /// <remarks>Example: DELETE: api/administration/user/owncompany/users</remarks>
        /// <response code="200">Users have successfully been deleted.</response>
        /// <response code="404">Record was not found. User is either not existing or not connected to the respective company.</response>
        [HttpDelete]
        [Authorize(Roles = "delete_user_account")]
        [Route("owncompany/users")]
        [ProducesResponseType(typeof(IAsyncEnumerable<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public IAsyncEnumerable<Guid> DeleteOwnCompanyUsers([FromBody] IEnumerable<Guid> usersToDelete) =>
            this.WithIamUserId(adminUserId => _logic.DeleteOwnCompanyUsersAsync(usersToDelete, adminUserId));

        /// <summary>
        /// Resets the password for the given user
        /// </summary>
        /// <param name="companyUserId" example="ac1cf001-7fbc-1f2f-817f-bce0575a0011">Id of the user whose password should be reset.</param>
        /// <returns></returns>
        /// <remarks>Example: PUT: api/administration/user/owncompany/users/ac1cf001-7fbc-1f2f-817f-bce0575a0011/resetPassword</remarks>
        /// <response code="200">The password was successfully reset.</response>
        /// <response code="400">Maximum amount of password resets reached. Password reset function is locked for the user for a certain time.</response>
        /// <response code="404">User id not found.</response>
        /// <response code="500">Internal Server Error, e.g. the password reset failed.</response>
        [HttpPut]
        [Authorize(Roles = "modify_user_account")]
        [Route("owncompany/users/{companyUserId}/resetPassword")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public Task<bool> ResetOwnCompanyUserPassword([FromRoute] Guid companyUserId) =>
            this.WithIamUserId(adminUserId => _logic.ExecuteOwnCompanyUserPasswordReset(companyUserId, adminUserId));

        /// <summary>
        /// Gets the client roles for the given app.
        /// </summary>
        /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id of the app which roles should be returned.</param>
        /// <param name="languageShortName">OPTIONAL: The language short name.</param>
        /// <returns>Returns the client roles for the given app.</returns>
        /// <remarks>Example: GET: api/administration/user/owncompany/app/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/roles</remarks>
        /// <response code="200">Returns the client roles.</response>
        /// <response code="400">The language does not exist.</response>
        /// <response code="404">The app was not found.</response>
        [HttpGet]
        [Authorize(Roles = "view_client_roles")]
        [Route("app/{appId}/roles")]
        [ProducesResponseType(typeof(IAsyncEnumerable<ClientRoles>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public IAsyncEnumerable<ClientRoles> GetClientRolesAsync([FromRoute] Guid appId, string? languageShortName = null) =>
            _logic.GetClientRolesAsync(appId, languageShortName);

        /// <summary>
        /// Gets the user details for the current user.
        /// </summary>
        /// <returns>Returns the company user details.</returns>
        /// <remarks>Example: GET: api/administration/user/ownUser</remarks>
        /// <response code="200">Returns the company user details.</response>
        /// <response code="404">User is not existing/found.</response>
        [HttpGet]
        [Route("ownUser")]
        [ProducesResponseType(typeof(CompanyUserDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
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
        /// <response code="403">Invalid companyUserId for user.</response>
        /// <response code="404">No shared realm userid found for the id in realm</response>
        [HttpPut]
        [Route("ownUser/{companyUserId}")]
        [ProducesResponseType(typeof(CompanyUserDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public Task<CompanyUserDetails> UpdateOwnUserDetails([FromRoute] Guid companyUserId, [FromBody] OwnCompanyUserEditableDetails ownCompanyUserEditableDetails) =>
            this.WithIamUserId(iamUserId => _logic.UpdateOwnUserDetails(companyUserId, ownCompanyUserEditableDetails, iamUserId));

        /// <summary>
        /// Deletes the own user
        /// </summary>
        /// <param name="companyUserId" example="ac1cf001-7fbc-1f2f-817f-bce0575a0011">Id of the user that should be deleted</param>
        /// <returns></returns>
        /// <remarks>Example: DELETE: api/administration/user/ownUser/ac1cf001-7fbc-1f2f-817f-bce0575a0011</remarks>
        /// <response code="200">Successfully deleted the user.</response>
        /// <response code="403">Invalid or not existing user id.</response>
        /// <response code="404">User is not existing/found.</response>
        [HttpDelete]
        [Route("ownUser/{companyUserId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
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

         /// <summary>
        /// Get company app users by appId
        /// </summary>
        /// <param name="appId">Get company app users by appId</param>
        /// <param name="page">page index start from 0</param>
        /// <param name="size">size to get number of records</param>
        /// <returns>Returns the company users with assigned role for the requested app id</returns>
        /// <remarks>Example: GET: /api/administration/user/owncompany/apps/5cf74ef8-e0b7-4984-a872-474828beb5d3/users?page=0&size=15</remarks>
        /// <response code="200">Result as a Company App Users Details</response>
        [HttpGet]
        [Authorize(Roles = "view_user_management")]
        [Route("owncompany/apps/{appId}/users")]
        [ProducesResponseType(typeof(Pagination.Response<CompanyAppUserDetails>), StatusCodes.Status200OK)]
        public Task<Pagination.Response<CompanyAppUserDetails>> GetCompanyAppUsersAsync([FromRoute] Guid appId,[FromQuery] int page = 0, [FromQuery] int size = 15) =>
            this.WithIamUserId(iamUserId => _logic.GetOwnCompanyAppUsersAsync(appId,iamUserId, page, size));

        /// <summary>
        /// Adds a user role
        /// </summary>
        /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id of the application</param>
        /// <param name="userRoleInfo"></param>
        /// <returns></returns>
        /// <remarks>Example: POST: api/administration/user/app/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/roles</remarks>
        /// <response code="200">Role got successfully added to the user account.</response>
        /// <response code="400">Invalid User roles for client</response>
        /// <response code="404">User not found</response>
        [HttpPost]
        [Authorize(Roles = "modify_user_account")]
        [Route("app/{appId}/roles")]
        [ProducesResponseType(typeof(UserRoleMessage), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public Task<UserRoleMessage> AddUserRole([FromRoute] Guid appId, [FromBody] UserRoleInfo userRoleInfo) =>
            this.WithIamUserId(adminUserId => _logic.AddUserRoleAsync(appId, userRoleInfo, adminUserId));
    }
}
