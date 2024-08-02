/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Web.Identity;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

[ApiController]
[EnvironmentRoute("MVC_ROUTING_BASEPATH", "serviceaccount")]
[Produces("application/json")]
[Consumes("application/json")]
public class ServiceAccountController : ControllerBase
{
    private readonly IServiceAccountBusinessLogic _logic;

    /// <summary>
    /// Creates a new instance of <see cref="ServiceAccountController"/> 
    /// </summary>
    /// <param name="logic">The Service Account Buisness Logic</param>
    public ServiceAccountController(IServiceAccountBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    /// Creates a new technical user / service account with selected role under the same org as the requester
    /// </summary>
    /// <param name="serviceAccountCreationInfo"></param>
    /// <returns></returns>
    /// <remarks>Example: POST: api/administration/serviceaccount/owncompany/serviceaccounts</remarks>
    /// <response code="200">The service account was created.</response>
    /// <response code="400">Missing mandatory input values (e.g. name) or not supported authenticationType selected.</response>
    /// <response code="404">Record was not found. Possible reason: invalid user role, requester user invalid.</response>
    [HttpPost]
    [Authorize(Roles = "add_tech_user_management")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/serviceaccounts")]
    [ProducesResponseType(typeof(IEnumerable<ServiceAccountDetails>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<IEnumerable<ServiceAccountDetails>> ExecuteCompanyUserCreation([FromBody] ServiceAccountCreationInfo serviceAccountCreationInfo) =>
        _logic.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfo);

    /// <summary>
    /// Deletes the service account with the given id
    /// </summary>
    /// <param name="serviceAccountId" example="7e85a0b8-0001-ab67-10d1-0ef508201000">Id of the service account that should be deleted.</param>
    /// <returns></returns>
    /// <remarks>Example: DELETE: api/administration/serviceaccount/owncompany/serviceaccounts/7e85a0b8-0001-ab67-10d1-0ef508201000</remarks>
    /// <response code="200">Successful if the service account was deleted.</response>
    /// <response code="404">Record was not found. Service account is either not existing or not connected to the respective company.</response>
    /// <response code="409">Technical User is linked to an active connector. Change the link or deactivate the connector to delete the technical user.</response>
    [HttpDelete]
    [Authorize(Roles = "delete_tech_user_management")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/serviceaccounts/{serviceAccountId}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public Task<int> DeleteServiceAccount([FromRoute] Guid serviceAccountId) =>
        _logic.DeleteOwnCompanyServiceAccountAsync(serviceAccountId);

    /// <summary>
    /// Gets the service account details for the given id
    /// </summary>
    /// <param name="serviceAccountId">Id to get the service account details for.</param>
    /// <returns>Returns a list of service account details.</returns>
    /// <remarks>Example: GET: api/administration/serviceaccount/owncompany/serviceaccounts/7e85a0b8-0001-ab67-10d1-0ef508201000</remarks>
    /// <response code="200">Returns a list of service account details.</response>
    /// <response code="404">Record was not found. Service account is either not existing or not connected to the respective company.</response>
    /// <response code="409">Undefined client for service account.</response>
    [HttpGet]
    [Authorize(Roles = "view_tech_user_management")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/serviceaccounts/{serviceAccountId}", Name = "GetServiceAccountDetails")]
    [ProducesResponseType(typeof(ServiceAccountConnectorOfferData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public Task<ServiceAccountConnectorOfferData> GetServiceAccountDetails([FromRoute] Guid serviceAccountId) =>
        _logic.GetOwnCompanyServiceAccountDetailsAsync(serviceAccountId);

    /// <summary>
    /// Updates the service account details with the given id.
    /// </summary>
    /// <param name="serviceAccountId">Id of the service account details that should be updated.</param>
    /// <param name="serviceAccountDetails">The new values for the details.</param>
    /// <returns>Returns the updated service account details.</returns>
    /// <remarks>Example: PUT: api/administration/serviceaccount/owncompany/serviceaccounts/7e85a0b8-0001-ab67-10d1-0ef508201000</remarks>
    /// <response code="200">Returns the updated service account details.</response>
    /// <response code="400">
    /// Problem could be one of the following: <br />
    /// - other authenticationType values than SECRET are not supported yet <br />
    /// - serviceAccountId from path does not match the one in body <br />
    /// - serviceAccount is already INACTIVE <br />
    /// </response>
    /// <response code="404">Record was not found. Service account is either not existing or not connected to the respective company.</response>
    /// <response code="409">Undefined client for service account.</response>
    [HttpPut]
    [Authorize(Roles = "add_tech_user_management")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/serviceaccounts/{serviceAccountId}")]
    [ProducesResponseType(typeof(ServiceAccountDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public Task<ServiceAccountDetails> PutServiceAccountDetails([FromRoute] Guid serviceAccountId, [FromBody] ServiceAccountEditableDetails serviceAccountDetails) =>
        _logic.UpdateOwnCompanyServiceAccountDetailsAsync(serviceAccountId, serviceAccountDetails);

    /// <summary>
    /// Resets the service account credentials for the given service account Id.
    /// 
    /// </summary>
    /// <param name="serviceAccountId">Id of the service account.</param>
    /// <returns>Returns the service account details.</returns>
    /// <remarks>Example: POST: api/administration/serviceaccount/owncompany/serviceaccounts/7e85a0b8-0001-ab67-10d1-0ef508201000/resetCredentials</remarks>
    /// <response code="200">Returns the service account details.</response>
    /// <response code="404">Record was not found. Service account is either not existing or not connected to the respective company.</response>
    /// <response code="409">Undefined client for service account.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpPost]
    [Authorize(Roles = "add_tech_user_management")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/serviceaccounts/{serviceAccountId}/resetCredentials")]
    [ProducesResponseType(typeof(ServiceAccountDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public Task<ServiceAccountDetails> ResetServiceAccountCredentials([FromRoute] Guid serviceAccountId) =>
        _logic.ResetOwnCompanyServiceAccountSecretAsync(serviceAccountId);

    /// <summary>
    /// Gets the service account data as pagination
    /// </summary>
    /// <param name="page">the page of service account data</param>
    /// <param name="size">number of service account data</param>
    /// <param name="isOwner">isOwner either true or false</param>
    /// <param name="clientId">clientId is string clientclientid</param>
    /// <param name="filterForInactive">isUserStatusActive is True or False</param>
    /// <param name="userStatus">userStatus is ACTIVE, INACTIVE, PENDING or DELETED (optional, multiple values allowed)</param>
    /// <returns>Returns the specific number of service account data for the given page.</returns>
    /// <remarks>Example: GET: api/administration/serviceaccount/owncompany/serviceaccounts</remarks>
    /// <response code="200">Returns the specific number of service account data for the given page.</response>
    [HttpGet]
    [Authorize(Roles = "view_tech_user_management")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("owncompany/serviceaccounts")]
    [ProducesResponseType(typeof(Pagination.Response<CompanyServiceAccountData>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<CompanyServiceAccountData>> GetServiceAccountsData([FromQuery] int page, [FromQuery] int size, [FromQuery] bool? isOwner, [FromQuery] string? clientId, [FromQuery] bool filterForInactive = false, [FromQuery] IEnumerable<UserStatusId>? userStatus = null) =>
        _logic.GetOwnCompanyServiceAccountsDataAsync(page, size, clientId, isOwner, filterForInactive, userStatus);

    /// <summary>
    /// Get all service account roles
    /// </summary>
    /// <param name="languageShortName">OPTIONAL: The language short name.</param>
    /// <returns>all service account roles</returns>
    /// <remarks>Example: GET: api/administration/serviceaccount/user/roles</remarks>
    /// <response code="200">returns all service account roles</response>
    [HttpGet]
    [Authorize(Roles = "technical_roles_management")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("user/roles")]
    [ProducesResponseType(typeof(List<UserRoleWithDescription>), StatusCodes.Status200OK)]
    public IAsyncEnumerable<UserRoleWithDescription> GetServiceAccountRolesAsync(string? languageShortName = null) =>
        _logic.GetServiceAccountRolesAsync(languageShortName);

    /// <summary>
    /// Get all service account roles
    /// </summary>
    /// <param name="processId">The processId that was passed as externalId with the request for creation of the technical user.</param>
    /// <param name="callbackData">Information of the technical user which was created.</param>
    /// <remarks>Example: POST: api/administration/serviceaccount/callback/{externalId}</remarks>
    /// <response code="200">returns all service account roles</response>
    [HttpPost]
    [Authorize(Roles = "technical_roles_management")]
    [Authorize(Policy = PolicyTypes.ServiceAccount)]
    [Route("callback/{processId}")]
    public async Task<OkResult> ServiceAccountCreationCallback([FromRoute] Guid processId, [FromBody] AuthenticationDetail callbackData)
    {
        await _logic.HandleServiceAccountCreationCallback(processId, callbackData).ConfigureAwait(ConfigureAwaitOptions.None);
        return Ok();
    }

    /// <summary>
    /// Callback for the successful service account deletion
    /// </summary>
    /// <param name="processId">The processId that was passed as externalId with the request for deletion of the technical user.</param>
    /// <remarks>Example: POST: api/administration/serviceaccount/callback/{externalId}/delete</remarks>
    /// <response code="200">Ok</response>
    [HttpPost]
    [Authorize(Roles = "technical_roles_management")]
    [Authorize(Policy = PolicyTypes.ServiceAccount)]
    [Route("callback/{processId}/delete")]
    public async Task<OkResult> ServiceAccountDeletionCallback([FromRoute] Guid processId)
    {
        await _logic.HandleServiceAccountDeletionCallback(processId).ConfigureAwait(ConfigureAwaitOptions.None);
        return Ok();
    }
}
