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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Web.Identity;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

/// <summary>
/// Controller providing actions for displaying, filtering and updating identityProviders for companies.
/// </summary>
[EnvironmentRoute("MVC_ROUTING_BASEPATH", "identityprovider")]
[ApiController]
public class IdentityProviderController : ControllerBase
{
    private readonly IIdentityProviderBusinessLogic _businessLogic;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="identityProviderBusinessLogic">IdentityProvider business logic.</param>
    public IdentityProviderController(IIdentityProviderBusinessLogic identityProviderBusinessLogic)
    {
        _businessLogic = identityProviderBusinessLogic;
    }

    /// <summary>
    /// Gets the details of the own company identity provider
    /// </summary>
    /// <returns>Returns the details of the own company identity provider</returns>
    /// <remarks>
    /// Example: GET: api/administration/identityprovider/owncompany/identityproviders
    /// </remarks>
    /// <response code="200">Returns a list of identityProviderDetails.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpGet]
    [Authorize(Roles = "view_idp")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/identityproviders")]
    [ProducesResponseType(typeof(List<IdentityProviderDetails>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public ValueTask<List<IdentityProviderDetails>> GetOwnCompanyIdentityProviderDetails() =>
        _businessLogic.GetOwnCompanyIdentityProvidersAsync().ToListAsync();

    /// <summary>
    /// Create an identity provider
    /// </summary>
    /// <param name="protocol">Type of the protocol the identity provider should be created for</param>
    /// <param name="typeId">IdentityProvider type (OWN or MANAGED)</param>
    /// <param name="displayName">displayName of identityprovider to be set up (optional)</param>
    /// <returns>Returns details of the created identity provider</returns>
    /// <remarks>
    /// Example: POST: api/administration/identityprovider/owncompany/identityproviders
    /// </remarks>
    /// <response code="200">Returns a list of identityProviderDetails.</response>
    /// <response code="400">The protocol didn't match an expected value.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpPost]
    [Authorize(Roles = "add_idp")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/identityproviders")]
    [ProducesResponseType(typeof(IdentityProviderDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public async ValueTask<ActionResult<IdentityProviderDetails>> CreateOwnCompanyIdentityProvider([FromQuery] IamIdentityProviderProtocol protocol, [FromQuery] IdentityProviderTypeId typeId, [FromQuery] string? displayName = null)
    {
        var details = await _businessLogic.CreateOwnCompanyIdentityProviderAsync(protocol, typeId, displayName).ConfigureAwait(false);
        return (ActionResult<IdentityProviderDetails>)CreatedAtRoute(nameof(GetOwnCompanyIdentityProvider), new { identityProviderId = details.IdentityProviderId }, details);
    }

    /// <summary>
    /// Gets a specific identity provider with the connected Companies
    /// </summary>
    /// <param name="identityProviderId">Id of the identity provider</param>
    /// <returns>Returns details of the identity provider</returns>
    /// <remarks>
    /// Example: GET: api/administration/identityprovider/network/identityproviders/managed/{identityProviderId}
    /// </remarks>
    /// <response code="200">Return the details of the identityProvider.</response>
    /// <response code="400">The user is not associated with the owner company.</response>
    /// <response code="500">Unexpected value of protocol.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpGet]
    [Authorize(Roles = "view_managed_idp")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("network/identityproviders/managed/{identityProviderId}")]
    [ProducesResponseType(typeof(IdentityProviderDetailsWithConnectedCompanies), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public ValueTask<IdentityProviderDetailsWithConnectedCompanies> GetOwnIdentityProviderWithConnectedCompanies([FromRoute] Guid identityProviderId) =>
        _businessLogic.GetOwnIdentityProviderWithConnectedCompanies(identityProviderId);

    /// <summary>
    /// Gets a specific identity provider
    /// </summary>
    /// <param name="identityProviderId">Id of the identity provider</param>
    /// <returns>Returns details of the identity provider</returns>
    /// <remarks>
    /// Example: GET: api/administration/identityprovider/owncompany/identityproviders/6CFEEF93-CB37-405B-B65A-02BEEB81629F
    /// </remarks>
    /// <response code="200">Returns a list of identityProviderDetails.</response>
    /// <response code="400">The user is not associated with a company.</response>
    /// <response code="500">Unexpected value of protocol.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpGet]
    [Authorize(Roles = "view_idp")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/identityproviders/{identityProviderId}", Name = nameof(GetOwnCompanyIdentityProvider))]
    [ProducesResponseType(typeof(IdentityProviderDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public ValueTask<IdentityProviderDetails> GetOwnCompanyIdentityProvider([FromRoute] Guid identityProviderId) =>
        _businessLogic.GetOwnCompanyIdentityProviderAsync(identityProviderId);

    /// <summary>
    /// Sets the status of the given Identity Provider
    /// </summary>
    /// <param name="identityProviderId">Id of the identity provider</param>
    /// <param name="enabled">true if the provider should be enabled, otherwise false</param>
    /// <returns>Returns details of the identity provider</returns>
    /// <remarks>
    /// Example: POST: api/administration/identityprovider/owncompany/identityproviders/6CFEEF93-CB37-405B-B65A-02BEEB81629F/status
    /// </remarks>
    /// <response code="200">Returns a list of identityProviderDetails.</response>
    /// <response code="400">Unexpected value for category of identityProvider.</response>
    /// <response code="403">The identityProvider is not associated with company of user.</response>
    /// <response code="404">The identityProvider does not exist.</response>
    /// <response code="500">Unexpected value of protocol.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpPost]
    [Authorize(Roles = "disable_idp")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/identityproviders/{identityProviderId}/status")]
    [ProducesResponseType(typeof(IdentityProviderDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public ValueTask<IdentityProviderDetails> SetOwnCompanyIdentityProviderStatus([FromRoute] Guid identityProviderId, [FromQuery] bool enabled) =>
        _businessLogic.SetOwnCompanyIdentityProviderStatusAsync(identityProviderId, enabled);

    /// <summary>
    /// Updates the details of the identity provider
    /// </summary>
    /// <param name="identityProviderId">Id of the identity provider</param>
    /// <param name="details">possible changes for the identity provider</param>
    /// <returns>Returns details of the identity provider</returns>
    /// <remarks>
    /// Example: PUT: api/administration/identityprovider/owncompany/identityproviders/6CFEEF93-CB37-405B-B65A-02BEEB81629F
    /// </remarks>
    /// <response code="200">Returns a list of identityProviderDetails.</response>
    /// <response code="400">Unexpected value for category of identityProvider.</response>
    /// <response code="403">The identityProvider is not associated with company of user.</response>
    /// <response code="404">The identityProvider does not exist.</response>
    /// <response code="500">Unexpected value of protocol.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpPut]
    [Authorize(Roles = "setup_idp")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/identityproviders/{identityProviderId}")]
    [ProducesResponseType(typeof(IdentityProviderDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public ValueTask<IdentityProviderDetails> UpdateOwnCompanyIdentityProvider([FromRoute] Guid identityProviderId, [FromBody] IdentityProviderEditableDetails details) =>
        _businessLogic.UpdateOwnCompanyIdentityProviderAsync(identityProviderId, details);

    /// <summary>
    /// Deletes the identity provider with the given id
    /// </summary>
    /// <param name="identityProviderId">Id of the identity provider</param>
    /// <returns>Returns no content</returns>
    /// <remarks>
    /// Example: DELETE: api/administration/identityprovider/owncompany/identityproviders/6CFEEF93-CB37-405B-B65A-02BEEB81629F
    /// </remarks>
    /// <response code="200">Returns a list of identityProviderDetails.</response>
    /// <response code="400">Unexpected value for category of identityProvider.</response>
    /// <response code="403">The identityProvider is not associated with company of user.</response>
    /// <response code="404">The identityProvider does not exist.</response>
    /// <response code="500">Unexpected value of protocol.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpDelete]
    [Authorize(Roles = "delete_idp")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/identityproviders/{identityProviderId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public async Task<NoContentResult> DeleteOwnCompanyIdentityProvider([FromRoute] Guid identityProviderId)
    {
        await _businessLogic.DeleteCompanyIdentityProviderAsync(identityProviderId).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Gets the company users for the identity providers
    /// </summary>
    /// <param name="identityProviderIds">Ids of the identity providers</param>
    /// <param name="unlinkedUsersOnly">Only users that doesn't match the given ids</param>
    /// <returns>Returns a list of user identity provider data</returns>
    /// <remarks>
    /// Example: GET: api/administration/identityprovider/owncompany/users
    /// </remarks>
    /// <response code="200">Returns a list of user identity provider data.</response>
    /// <response code="400">No identity provider was provided.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpGet]
    [Authorize(Roles = "view_user_management")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/users")]
    [ProducesResponseType(typeof(IAsyncEnumerable<UserIdentityProviderData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public IAsyncEnumerable<UserIdentityProviderData> GetOwnCompanyUsersIdentityProviderDataAsync([FromQuery] IEnumerable<Guid> identityProviderIds, [FromQuery] bool unlinkedUsersOnly = false) =>
        _businessLogic.GetOwnCompanyUsersIdentityProviderDataAsync(identityProviderIds, unlinkedUsersOnly);

    /// <summary>
    /// Gets the company users for the identity providers as a file
    /// </summary>
    /// <param name="identityProviderIds">Ids of the identity providers</param>
    /// <param name="unlinkedUsersOnly">Only users that doesn't match the given ids</param>
    /// <returns>Returns a file of users</returns>
    /// <remarks>
    /// Example: GET: api/administration/identityprovider/owncompany/usersfile
    /// </remarks>
    /// <response code="200">Returns a file of users.</response>
    /// <response code="400">No identity provider was provided.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpGet]
    [Authorize(Roles = "modify_user_account")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/usersfile")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public IActionResult GetOwnCompanyUsersIdentityProviderFileAsync([FromQuery] IEnumerable<Guid> identityProviderIds, [FromQuery] bool unlinkedUsersOnly = false)
    {
        var (stream, contentType, fileName, encoding) = _businessLogic.GetOwnCompanyUsersIdentityProviderLinkDataStream(identityProviderIds, unlinkedUsersOnly);
        return File(stream, string.Join("; ", contentType, encoding.WebName), fileName);
    }

    /// <summary>
    /// Upload the users and adds them to the user provider 
    /// </summary>
    /// <param name="document">The file including the users</param>
    /// <param name="cancellationToken">the CancellationToken for this request (provided by the Controller)</param>
    /// <returns>Returns a status of the document processing</returns>
    /// <remarks>
    /// Example: POST: api/administration/identityprovider/owncompany/usersfile
    /// </remarks>
    /// <response code="200">Returns a file of users.</response>
    /// <response code="400">user is not associated with a company.</response>
    /// <response code="415">Content type didn't match the expected value.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpPost]
    [Authorize(Roles = "modify_user_account")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Consumes("multipart/form-data")]
    [Route("owncompany/usersfile")]
    [RequestFormLimits(ValueLengthLimit = 819200, MultipartBodyLengthLimit = 819200)]
    [ProducesResponseType(typeof(IdentityProviderUpdateStats), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public ValueTask<IdentityProviderUpdateStats> UploadOwnCompanyUsersIdentityProviderFileAsync([FromForm(Name = "document")] IFormFile document, CancellationToken cancellationToken) =>
        _businessLogic.UploadOwnCompanyUsersIdentityProviderLinkDataAsync(document, cancellationToken);

    /// <summary>
    /// Adds the user to the given identity provider
    /// </summary>
    /// <param name="companyUserId">Id of the company user</param>
    /// <param name="identityProviderLinkData">The link data for the identity provider</param>
    /// <returns>Returns the link data</returns>
    /// <remarks>
    /// Example: POST: api/administration/identityprovider/owncompany/users/A744E2AA-55AA-4511-9F42-80371220BE26/identityprovider
    /// </remarks>
    /// <response code="200">Returns the link data.</response>
    /// <response code="400">user is not associated with a company.</response>
    /// <response code="403">user does not belong to company of companyUserId.</response>
    /// <response code="404">companyUserId does not exist.</response>
    /// <response code="409">identityProviderLink for identityProvider already exists for user.</response>
    /// <response code="500">companyUserId is not linked to keycloak</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [Obsolete("use CreateOrUpdateOwnCompanyUserIdentityProviderDataAsync (PUT api/administration/identityprovider/owncompany/users/{companyUserId/identityprovider/{identityProviderId}) instead")]
    [HttpPost]
    [Authorize(Roles = "modify_user_account")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/users/{companyUserId}/identityprovider")]
    [ProducesResponseType(typeof(UserIdentityProviderLinkData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public async ValueTask<ActionResult<UserIdentityProviderLinkData>> AddOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromBody] UserIdentityProviderLinkData identityProviderLinkData)
    {
        var linkData = await _businessLogic.CreateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderLinkData).ConfigureAwait(false);
        return (ActionResult<UserIdentityProviderLinkData>)CreatedAtRoute(
            nameof(GetOwnCompanyUserIdentityProviderDataAsync),
            new { companyUserId = companyUserId, identityProviderId = linkData.identityProviderId },
            linkData);
    }

    /// <summary>
    /// Updates the given user for the given identity provider
    /// </summary>
    /// <param name="companyUserId">Id of the company user</param>
    /// <param name="identityProviderId">Id of the identity provider</param>
    /// <param name="userLinkData">Data that should be updated</param>
    /// <returns>Returns the link data</returns>
    /// <remarks>
    /// Example: PUT: api/administration/identityprovider/owncompany/users/A744E2AA-55AA-4511-9F42-80371220BE26/identityprovider/7DAAF6C3-BEB1-466B-A87A-98DB8CE194B2
    /// </remarks>
    /// <response code="200">Returns the link data.</response>
    /// <response code="400">user is not associated with a company.</response>
    /// <response code="403">user does not belong to company of companyUserId.</response>
    /// <response code="404">companyUserId does not exist.</response>
    /// <response code="500">companyUserId is not linked to keycloak</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpPut]
    [Authorize(Roles = "modify_user_account")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/users/{companyUserId}/identityprovider/{identityProviderId}")]
    [ProducesResponseType(typeof(UserIdentityProviderLinkData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public ValueTask<UserIdentityProviderLinkData> CreateOrUpdateOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromRoute] Guid identityProviderId, [FromBody] UserLinkData userLinkData) =>
        _businessLogic.CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId, userLinkData);

    /// <summary>
    /// Gets the given user for the given identity provider
    /// </summary>
    /// <param name="companyUserId">Id of the company user</param>
    /// <param name="identityProviderId">Id of the identity provider</param>
    /// <returns>Returns the link data</returns>
    /// <remarks>
    /// Example: GET: api/administration/identityprovider/owncompany/users/A744E2AA-55AA-4511-9F42-80371220BE26/identityprovider/7DAAF6C3-BEB1-466B-A87A-98DB8CE194B2
    /// </remarks>
    /// <response code="200">Returns the link data.</response>
    /// <response code="400">user is not associated with a company.</response>
    /// <response code="403">user does not belong to company of companyUserId.</response>
    /// <response code="404">companyUserId does not exist.</response>
    /// <response code="500">companyUserId is not linked to keycloak</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpGet]
    [Authorize(Roles = "view_user_management")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/users/{companyUserId}/identityprovider/{identityProviderId}", Name = nameof(GetOwnCompanyUserIdentityProviderDataAsync))]
    [ProducesResponseType(typeof(UserIdentityProviderLinkData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public ValueTask<UserIdentityProviderLinkData> GetOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromRoute] Guid identityProviderId) =>
        _businessLogic.GetOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId);

    /// <summary>
    /// Deletes the given user on the given identity provider
    /// </summary>
    /// <param name="companyUserId">Id of the company user</param>
    /// <param name="identityProviderId">Id of the identity provider</param>
    /// <returns>Returns no content</returns>
    /// <remarks>
    /// Example: DELETE: api/administration/identityprovider/owncompany/users/A744E2AA-55AA-4511-9F42-80371220BE26/identityprovider/7DAAF6C3-BEB1-466B-A87A-98DB8CE194B2
    /// </remarks>
    /// <response code="200">Returns the link data.</response>
    /// <response code="400">user is not associated with a company.</response>
    /// <response code="403">user does not belong to company of companyUserId.</response>
    /// <response code="404">companyUserId does not exist.</response>
    /// <response code="500">companyUserId is not linked to keycloak</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpDelete]
    [Authorize(Roles = "modify_user_account")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/users/{companyUserId}/identityprovider/{identityProviderId}")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public async ValueTask<ActionResult> DeleteOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromRoute] Guid identityProviderId)
    {
        await _businessLogic.DeleteOwnCompanyUserIdentityProviderDataAsync(companyUserId, identityProviderId).ConfigureAwait(false);
        return (ActionResult)NoContent();
    }
}
