/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

[ApiController]
[Route("api/administration/user")]
[Produces("application/json")]
[Consumes("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserBusinessLogic _logic;
    private readonly IUserUploadBusinessLogic _uploadLogic;
    private readonly IUserRolesBusinessLogic _rolesLogic;

    /// <summary>
    /// Creates a new instance of <see cref="UserController"/>
    /// </summary>
    /// <param name="logic">The User Business Logic</param>
    /// <param name="uploadLogic">The User Upload Business Logic</param>
    /// <param name="rolesLogic">The User Roles Management Business Logic</param>
    public UserController(IUserBusinessLogic logic, IUserUploadBusinessLogic uploadLogic, IUserRolesBusinessLogic rolesLogic)
    {
        _logic = logic;
        _uploadLogic = uploadLogic;
        _rolesLogic = rolesLogic;
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
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/users")]
    [ProducesResponseType(typeof(IAsyncEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IAsyncEnumerable<string> CreateOwnCompanyUsers([FromBody] IEnumerable<UserCreationInfo> usersToCreate) =>
        this.WithUserIdAndCompanyId(identity => _logic.CreateOwnCompanyUsersAsync(usersToCreate, identity));

    /// <summary>
    /// Create new users for the companies shared identityprovider by upload of csv-file
    /// </summary>
    /// <param name="document">The file including the users</param>
    /// <param name="cancellationToken">the CancellationToken for this request (provided by the Controller)</param>
    /// <returns>Returns a status of the document processing</returns>
    /// <remarks>
    /// Example: POST: api/administration/user/owncompany/usersfile
    /// </remarks>
    /// <response code="200">Returns a file of users.</response>
    /// <response code="400">user is not associated with a company.</response>
    /// <response code="415">Content type didn't match the expected value.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpPost]
    [Authorize(Roles = "add_user_account")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Consumes("multipart/form-data")]
    [Route("owncompany/usersfile")]
    [RequestFormLimits(ValueLengthLimit = 819200, MultipartBodyLengthLimit = 819200)]
    [ProducesResponseType(typeof(UserCreationStats), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public ValueTask<UserCreationStats> UploadOwnCompanySharedIdpUsersFileAsync([FromForm(Name = "document")] IFormFile document, CancellationToken cancellationToken) =>
        this.WithUserIdAndCompanyId(identity => _uploadLogic.UploadOwnCompanySharedIdpUsersAsync(document, identity, cancellationToken));

    /// <summary>
    /// Create a new user for a specific identityprovider
    /// </summary>
    /// <param name="identityProviderId">the id of the identityprovider</param>
    /// <param name="userToCreate">properties and identityprovider link data for the user to create</param>
    /// <returns>the id of the newly created company-user</returns>
    /// <remarks>
    /// Example: POST: api/administration/user/owncompany/identityprovider/{identityProviderId}/users
    /// </remarks>
    /// <response code="201">Record Created Successfully</response>
    /// <response code="400">Input is incorrect.</response>
    /// <response code="500">Unexpected Error</response>
    /// <response code="404">No Record Found.</response>
    /// <response code="409">Company Name is null.</response>
    [HttpPost]
    [Authorize(Roles = "add_user_account")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/identityprovider/{identityProviderId}/users")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<CreatedAtRouteResult> CreateOwnIdpOwnCompanyUser([FromBody] UserCreationInfoIdp userToCreate, [FromRoute] Guid identityProviderId)
    {
        var result = await this.WithUserIdAndCompanyId(identity => _logic.CreateOwnCompanyIdpUserAsync(identityProviderId, userToCreate, identity)).ConfigureAwait(false);
        return CreatedAtRoute(nameof(GetOwnCompanyUserDetails), new { companyUserId = result }, result);
    }

    /// <summary>
    /// Create new users for a specific identityprovider by upload of csv-file
    /// </summary>
    /// <param name="identityProviderId">the id of the identityprovider</param>
    /// <param name="document">The file including the users</param>
    /// <param name="cancellationToken">the CancellationToken for this request (provided by the Controller)</param>
    /// <returns>Returns a status of the document processing</returns>
    /// <remarks>
    /// Example: POST: api/administration/user/owncompany/identityprovider/{identityProviderId}/usersfile
    /// </remarks>
    /// <response code="200">Returns a file of users.</response>
    /// <response code="400">user is not associated with a company.</response>
    /// <response code="415">Content type didn't match the expected value.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpPost]
    [Authorize(Roles = "add_user_account")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Consumes("multipart/form-data")]
    [Route("owncompany/identityprovider/{identityProviderId}/usersfile")]
    [RequestFormLimits(ValueLengthLimit = 819200, MultipartBodyLengthLimit = 819200)]
    [ProducesResponseType(typeof(UserCreationStats), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public ValueTask<UserCreationStats> UploadOwnCompanyUsersIdentityProviderFileAsync([FromRoute] Guid identityProviderId, [FromForm(Name = "document")] IFormFile document, CancellationToken cancellationToken) =>
            this.WithUserIdAndCompanyId(identity => _uploadLogic.UploadOwnCompanyIdpUsersAsync(identityProviderId, document, identity, cancellationToken));

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
    /// <remarks> Example: GET: api/administration/user/owncompany/users?page=0&amp;size=5</remarks>
    /// <remarks> Example: GET: api/administration/user/owncompany/users?page=0&amp;size=5&amp;userEntityId="31404026-64ee-4023-a122-3c7fc40e57b1"</remarks>
    /// <response code="200">Result as a Company User Data</response>
    [HttpGet]
    [Authorize(Roles = "view_user_management")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
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
        this.WithCompanyId(companyId => _logic.GetOwnCompanyUserDatasAsync(
            companyId,
            page,
            size,
            new(companyUserId, userEntityId, firstName, lastName, email)));

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
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/users/{companyUserId}", Name = nameof(GetOwnCompanyUserDetails))]
    [ProducesResponseType(typeof(CompanyUserDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<CompanyUserDetails> GetOwnCompanyUserDetails([FromRoute] Guid companyUserId) =>
        this.WithCompanyId(companyId => _logic.GetOwnCompanyUserDetailsAsync(companyUserId, companyId));

    /// <summary>
    /// Updates the portal-roles for the user
    /// </summary>
    /// <param name="companyUserId"></param>
    /// <param name="offerId"></param>
    /// <param name="roles"></param>
    /// <returns></returns>
    /// <remarks>Example: PUT: api/administration/user/owncompany/users/{companyUserId}/coreoffers/{offerId}/roles</remarks>
    /// <response code="200">Roles got successfully updated user account.</response>
    /// <response code="400">Invalid User roles for client</response>
    /// <response code="404">User not found</response>
    [HttpPut]
    [Authorize(Roles = "modify_user_account")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/users/{companyUserId}/coreoffers/{offerId}/roles")]
    [ProducesResponseType(typeof(IEnumerable<UserRoleWithId>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<IEnumerable<UserRoleWithId>> ModifyCoreUserRolesAsync([FromRoute] Guid companyUserId, [FromRoute] Guid offerId, [FromBody] IEnumerable<string> roles) =>
        this.WithCompanyId(companyId => _rolesLogic.ModifyCoreOfferUserRolesAsync(offerId, companyUserId, roles, companyId));

    /// <summary>
    /// Updates the app-roles for the user
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id of the application</param>
    /// <param name="companyUserId"></param>
    /// <param name="roles"></param>
    /// <returns></returns>
    /// <remarks>Example: PUT: api/administration/user/owncompany/users/{companyUserId}/apps/{appId}/roles</remarks>
    /// <response code="200">Roles got successfully updated user account.</response>
    /// <response code="400">Invalid User roles for client</response>
    /// <response code="404">User not found</response>
    [HttpPut]
    [Authorize(Roles = "modify_user_account")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/users/{companyUserId}/apps/{appId}/roles")]
    [ProducesResponseType(typeof(IEnumerable<UserRoleWithId>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<IEnumerable<UserRoleWithId>> ModifyAppUserRolesAsync([FromRoute] Guid companyUserId, [FromRoute] Guid appId, [FromBody] IEnumerable<string> roles) =>
        this.WithCompanyId(companyId => _rolesLogic.ModifyAppUserRolesAsync(appId, companyUserId, roles, companyId));

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
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/users/{companyUserId}/businessPartnerNumbers")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<int> AddOwnCompanyUserBusinessPartnerNumbers(Guid companyUserId, IEnumerable<string> businessPartnerNumbers) =>
        this.WithCompanyId(companyId => _logic.AddOwnCompanyUsersBusinessPartnerNumbersAsync(companyUserId, businessPartnerNumbers, companyId));

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
    /// <response code="500">Internal Server Error.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpPut]
    [Authorize(Roles = "modify_user_account")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/users/{companyUserId}/businessPartnerNumbers/{businessPartnerNumber}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public Task<int> AddOwnCompanyUserBusinessPartnerNumber(Guid companyUserId, string businessPartnerNumber) =>
        this.WithCompanyId(companyId => _logic.AddOwnCompanyUsersBusinessPartnerNumberAsync(companyUserId, businessPartnerNumber, companyId));

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
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/users")]
    [ProducesResponseType(typeof(IAsyncEnumerable<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IAsyncEnumerable<Guid> DeleteOwnCompanyUsers([FromBody] IEnumerable<Guid> usersToDelete) =>
        this.WithCompanyId(companyId => _logic.DeleteOwnCompanyUsersAsync(usersToDelete, companyId));

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
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpPut]
    [Authorize(Roles = "modify_user_account")]
    [Route("owncompany/users/{companyUserId}/resetPassword")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public Task<bool> ResetOwnCompanyUserPassword([FromRoute] Guid companyUserId) =>
        this.WithUserIdAndCompanyId(identity => _logic.ExecuteOwnCompanyUserPasswordReset(companyUserId, identity));

    /// <summary>
    /// Gets the core offer roles
    /// </summary>
    /// <param name="languageShortName" example="DE">The shortname of the user role description"</param>
    /// <remarks>
    /// Example: GET: api/administration/user/owncompany/roles/coreoffers <br />
    /// Example: GET: api/administration/user/owncompany/roles/coreoffers?languageShortName=DE
    /// </remarks>
    /// <returns>Returns a collection of offer role infos</returns>
    /// <response code="200">A list of OfferRoleInfos.</response>
    [HttpGet]
    [Authorize(Roles = "view_client_roles")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/roles/coreoffers")]
    [ProducesResponseType(typeof(IEnumerable<OfferRoleInfos>), StatusCodes.Status200OK)]
    public IAsyncEnumerable<OfferRoleInfos> GetCoreOfferRoles([FromQuery] string? languageShortName = null) =>
        this.WithCompanyId(companyId => _rolesLogic.GetCoreOfferRoles(companyId, languageShortName));

    /// <summary>
    /// Gets the client roles for the given app.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id of the app which roles should be returned.</param>
    /// <param name="languageShortName">OPTIONAL: The language short name.</param>
    /// <returns>Returns the client roles for the given app.</returns>
    /// <remarks>Example: GET: api/administration/user/owncompany/roles/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645</remarks>
    /// <response code="200">Returns the client roles.</response>
    /// <response code="400">The language does not exist.</response>
    /// <response code="404">The app was not found.</response>
    [HttpGet]
    [Authorize(Roles = "view_client_roles")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/roles/apps/{appId}")]
    [ProducesResponseType(typeof(OfferRoleInfos), StatusCodes.Status200OK)]
    public IAsyncEnumerable<OfferRoleInfo> GetAppRolesAsync([FromRoute] Guid appId, [FromQuery] string? languageShortName = null) =>
        this.WithCompanyId(companyId => _rolesLogic.GetAppRolesAsync(appId, companyId, languageShortName));

    /// <summary>
    /// Gets the client roles for the given app.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id of the app which roles should be returned.</param>
    /// <param name="languageShortName">OPTIONAL: The language short name.</param>
    /// <returns>Returns the client roles for the given app.</returns>
    /// <remarks>Example: GET: api/administration/user/app/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/roles</remarks>
    /// <response code="200">Returns the client roles.</response>
    /// <response code="400">The language does not exist.</response>
    /// <response code="404">The app was not found.</response>
    [Obsolete("to be replaced by endpoint /user/owncompany/roles/apps/{appid}. Remove as soon frontend is adjusted")]
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
    [Authorize(Roles = "view_own_user_account")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Route("ownUser")]
    [ProducesResponseType(typeof(CompanyOwnUserDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<CompanyOwnUserDetails> GetOwnUserDetails() =>
        this.WithUserId(userId => _logic.GetOwnUserDetails(userId));

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
    [Authorize(Roles = "change_own_user_account")]
    [Route("ownUser/{companyUserId}")]
    [ProducesResponseType(typeof(CompanyUserDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<CompanyUserDetails> UpdateOwnUserDetails([FromRoute] Guid companyUserId, [FromBody] OwnCompanyUserEditableDetails ownCompanyUserEditableDetails) =>
        this.WithUserId(userId => _logic.UpdateOwnUserDetails(companyUserId, ownCompanyUserEditableDetails, userId));

    /// <summary>
    /// Deletes the own user
    /// </summary>
    /// <param name="companyUserId" example="ac1cf001-7fbc-1f2f-817f-bce0575a0011">Id of the user that should be deleted</param>
    /// <returns></returns>
    /// <remarks>Example: DELETE: api/administration/user/ownUser/ac1cf001-7fbc-1f2f-817f-bce0575a0011</remarks>
    /// <response code="200">Successfully deleted the user.</response>
    /// <response code="403">Invalid or not existing user id.</response>
    /// <response code="409">User is not associated with company.</response>
    [HttpDelete]
    [Authorize(Roles = "delete_own_user_account")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Route("ownUser/{companyUserId}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public Task<int> DeleteOwnUser([FromRoute] Guid companyUserId) =>
        this.WithUserId(userId => _logic.DeleteOwnUserAsync(companyUserId, userId));

    /// <summary>
    /// Get for given app id all the company assigned users
    /// </summary>
    /// <param name="appId">Get company app users by appId</param>
    /// <param name="page">page index start from 0</param>
    /// <param name="size">size to get number of records</param>
    /// <param name="firstName">First Name of User</param>
    /// <param name="lastName">Last Name of User</param>
    /// <param name="email">Email Id of User</param>
    /// <param name="roleName">User role name</param>
    /// <param name="hasRole">Defines whether the users should be filtered with a app role</param>
    /// <returns>Returns the company users with assigned role for the requested app id</returns>
    /// <remarks>Example: GET: /api/administration/user/owncompany/apps/5cf74ef8-e0b7-4984-a872-474828beb5d3/users?page=0&amp;size=15</remarks>
    /// <response code="200">Result as a Company App Users Details</response>
    [HttpGet]
    [Authorize(Roles = "view_user_management")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Route("owncompany/apps/{appId}/users")]
    [ProducesResponseType(typeof(Pagination.Response<CompanyAppUserDetails>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<CompanyAppUserDetails>> GetCompanyAppUsersAsync(
        [FromRoute] Guid appId,
        [FromQuery] int page = 0,
        [FromQuery] int size = 15,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] string? email = null,
        [FromQuery] string? roleName = null,
        [FromQuery] bool? hasRole = null) =>
        this.WithUserId(userId => _logic.GetOwnCompanyAppUsersAsync(
            appId,
            userId,
            page,
            size,
            new CompanyUserFilter(
                firstName,
                lastName,
                email,
                roleName,
                hasRole)));

    /// <summary>
    /// Updates the roles for the user
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id of the application</param>
    /// <param name="userRoleInfo"></param>
    /// <returns></returns>
    /// <remarks>Example: PUT: api/administration/user/app/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/roles</remarks>
    /// <response code="200">Roles got successfully updated user account.</response>
    /// <response code="400">Invalid User roles for client</response>
    /// <response code="404">User not found</response>
    [Obsolete("to be replaced by endpoint /user/owncompany/users/{companyUserId}/apps/{appId}/roles. remove as soon frontend has been adjusted")]
    [HttpPut]
    [Authorize(Roles = "modify_user_account")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("app/{appId}/roles")]
    [ProducesResponseType(typeof(IEnumerable<UserRoleWithId>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<IEnumerable<UserRoleWithId>> ModifyUserRolesAsync([FromRoute] Guid appId, [FromBody] UserRoleInfo userRoleInfo) =>
        this.WithCompanyId(companyId => _rolesLogic.ModifyUserRoleAsync(appId, userRoleInfo, companyId));

    /// <summary>
    /// Delete BPN assigned to user from DB and Keycloack.
    /// </summary>
    /// <param name="companyUserId" example="4f06431c-25ae-40ad-9cac-9dee8fe4754d">ID of the company user to be deleted.</param>
    /// <param name="businessPartnerNumber" example="CAXSDUMMYTESTCX1">BPN to be deleted.</param>
    /// <remarks>Example: DELETE: /api/administration/user/owncompany/users/4f06431c-25ae-40ad-9cac-9dee8fe4754d/businessPartnerNumbers/CAXSDUMMYTESTCX1</remarks>
    /// <response code="200">Empty response on success.</response>
    /// <response code="403">ForbiddenException if both users does not belongs to same company</response>
    /// <response code="404">Record not found.</response>
    /// <response code="409">User is not associated in keycloak.</response>
    [HttpDelete]
    [Authorize(Roles = "modify_user_account")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/users/{companyUserId}/businessPartnerNumbers/{businessPartnerNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public Task<int> DeleteOwnCompanyUserBusinessPartnerNumber([FromRoute] Guid companyUserId, [FromRoute] string businessPartnerNumber) =>
        this.WithUserIdAndCompanyId(identity => _logic.DeleteOwnUserBusinessPartnerNumbersAsync(companyUserId, businessPartnerNumber, identity));
}
