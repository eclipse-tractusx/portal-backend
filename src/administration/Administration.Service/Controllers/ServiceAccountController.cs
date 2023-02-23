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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

[ApiController]
[Route("api/administration/serviceaccount")]
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
    /// <response code="201">The service account was created.</response>
    /// <response code="400">Missing mandatory input values (e.g. name) or not supported authenticationType selected.</response>
    /// <response code="404">Record was not found. Possible reason: invalid user role, requester user invalid.</response>
    [HttpPost]
    [Authorize(Roles = "add_tech_user_management")]
    [Route("owncompany/serviceaccounts")]
    [ProducesResponseType(typeof(ServiceAccountDetails), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceAccountDetails>> ExecuteCompanyUserCreation([FromBody] ServiceAccountCreationInfo serviceAccountCreationInfo)
    {
        var serviceAccountDetails = await this.WithIamUserId(createdByName => _logic.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfo, createdByName).ConfigureAwait(false));
        return CreatedAtRoute("GetServiceAccountDetails", new { serviceAccountId = serviceAccountDetails.ServiceAccountId }, serviceAccountDetails);
    }

    /// <summary>
    /// Deletes the service account with the given id
    /// </summary>
    /// <param name="serviceAccountId" example="7e85a0b8-0001-ab67-10d1-0ef508201000">Id of the service account that should be deleted.</param>
    /// <returns></returns>
    /// <remarks>Example: DELETE: api/administration/owncompany/serviceaccounts/7e85a0b8-0001-ab67-10d1-0ef508201000</remarks>
    /// <response code="200">Successful if the service account was deleted.</response>
    /// <response code="404">Record was not found. Service account is either not existing or not connected to the respective company.</response>
    [HttpDelete]
    [Authorize(Roles = "delete_tech_user_management")]
    [Route("owncompany/serviceaccounts/{serviceAccountId}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<int> DeleteServiceAccount([FromRoute] Guid serviceAccountId) =>
        this.WithIamUserId(adminId => _logic.DeleteOwnCompanyServiceAccountAsync(serviceAccountId, adminId));

    /// <summary>
    /// Gets the service account details for the given id
    /// </summary>
    /// <param name="serviceAccountId">Id to get the service account details for.</param>
    /// <returns>Returns a list of service account details.</returns>
    /// <remarks>Example: GET: api/administration/serviceaccount/owncompany/serviceaccounts/7e85a0b8-0001-ab67-10d1-0ef508201000</remarks>
    /// <response code="200">Returns a list of service account details.</response>
    /// <response code="404">Record was not found. Service account is either not existing or not connected to the respective company.</response>
    [HttpGet]
    [Authorize(Roles = "view_tech_user_management")]
    [Route("owncompany/serviceaccounts/{serviceAccountId}", Name="GetServiceAccountDetails")]
    [ProducesResponseType(typeof(ServiceAccountDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<ServiceAccountDetails> GetServiceAccountDetails([FromRoute] Guid serviceAccountId) =>
        this.WithIamUserId(adminId => _logic.GetOwnCompanyServiceAccountDetailsAsync(serviceAccountId, adminId));

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
    [HttpPut]
    [Authorize(Roles = "add_tech_user_management")] // TODO check whether we also want an edit role
    [Route("owncompany/serviceaccounts/{serviceAccountId}")]
    [ProducesResponseType(typeof(ServiceAccountDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<ServiceAccountDetails> PutServiceAccountDetails([FromRoute] Guid serviceAccountId, [FromBody] ServiceAccountEditableDetails serviceAccountDetails) =>
        this.WithIamUserId(adminId => _logic.UpdateOwnCompanyServiceAccountDetailsAsync(serviceAccountId, serviceAccountDetails, adminId));

    /// <summary>
    /// Resets the service account credentials for the given service account Id.
    /// 
    /// </summary>
    /// <param name="serviceAccountId">Id of the service account.</param>
    /// <returns>Returns the service account details.</returns>
    /// <remarks>Example: PUT: api/administration/serviceaccount/owncompany/serviceaccounts/7e85a0b8-0001-ab67-10d1-0ef508201000/resetCredentials</remarks>
    /// <response code="200">Returns the service account details.</response>
    /// <response code="404">Record was not found. Service account is either not existing or not connected to the respective company.</response>
    [HttpPost]
    [Authorize(Roles = "add_tech_user_management")]
    [Route("owncompany/serviceaccounts/{serviceAccountId}/resetCredentials")]
    [ProducesResponseType(typeof(ServiceAccountDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<ServiceAccountDetails> ResetServiceAccountCredentials([FromRoute] Guid serviceAccountId) =>
        this.WithIamUserId(adminId => _logic.ResetOwnCompanyServiceAccountSecretAsync(serviceAccountId, adminId));

    /// <summary>
    /// Gets the service account data as pagination
    /// </summary>
    /// <param name="page">the page of service account data</param>
    /// <param name="size">number of service account data</param>
    /// <returns>Returns the specific number of service account data for the given page.</returns>
    /// <remarks>Example: GET: api/administration/serviceaccount/owncompany/serviceaccounts</remarks>
    /// <response code="200">Returns the specific number of service account data for the given page.</response>
    [HttpGet]
    [Authorize(Roles = "view_tech_user_management")]
    [Route("owncompany/serviceaccounts")]
    [ProducesResponseType(typeof(Pagination.Response<CompanyServiceAccountData>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<CompanyServiceAccountData>> GetServiceAccountsData([FromQuery] int page, [FromQuery] int size) =>
        this.WithIamUserId(adminId => _logic.GetOwnCompanyServiceAccountsDataAsync(page, size, adminId));

    /// <summary>
    /// Get all service account roles
    /// </summary>
    /// <param name="languageShortName">OPTIONAL: The language short name.</param>
    /// <returns>all service account roles</returns>
    /// <remarks>Example: Get: api/administration/serviceaccount/user/roles</remarks>
    /// <response code="200">returns all service account roles</response>
    [HttpGet]
    [Authorize(Roles = "technical_roles_management")]
    [Route("user/roles")]
    [ProducesResponseType(typeof(List<UserRoleWithDescription>), StatusCodes.Status200OK)]
    public IAsyncEnumerable<UserRoleWithDescription> GetServiceAccountRolesAsync(string? languageShortName = null) =>
        _logic.GetServiceAccountRolesAsync(languageShortName);        
}
