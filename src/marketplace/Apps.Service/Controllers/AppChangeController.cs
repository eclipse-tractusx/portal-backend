﻿/********************************************************************************
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
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.Controllers;

/// <summary>
/// Controller providing actions for updating applications.
/// </summary>
[Route("api/apps/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public class AppChangeController : ControllerBase
{
    private readonly IAppChangeBusinessLogic _businessLogic;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="businessLogic"></param>
    public AppChangeController(IAppChangeBusinessLogic businessLogic)
    {
        _businessLogic = businessLogic;
    }
    
    /// <summary>
    /// update app roles and related description for "active" owned app offers
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="userRoles"></param>
    /// <remarks>Example: POST: /api/apps/appchange/{appId}/role/activeapp</remarks>
    /// <response code="400">If sub claim is empty/invalid or user does not exist, or any other parameters are invalid.</response>
    /// <response code="404">App does not exist.</response>
    /// <response code="200">created role and role description successfully.</response>
    /// <response code="403">User not associated with provider company.</response>
    /// <response code="409">App provider company not set.</response>
    [HttpPost]
    [Route("{appId}/role/activeapp")]
    [Authorize(Roles = "edit_apps")]
    [ProducesResponseType(typeof(IEnumerable<AppRoleData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IEnumerable<AppRoleData>> AddActiveAppUserRole([FromRoute] Guid appId, [FromBody] IEnumerable<AppUserRole> userRoles)=>
        await this.WithIamUserId(iamUserId => _businessLogic.AddActiveAppUserRoleAsync(appId, userRoles, iamUserId)).ConfigureAwait(false);

    /// <summary>
    /// Get description of the app by Id.
    /// </summary>
    /// <param name="appId" example="092bdae3-a044-4314-94f4-85c65a09e31b">Id for the app description to retrieve.</param>
    /// <returns>collection of descriptions of app by Id that are provided by the calling users company</returns>
    /// <remarks>Example: Get: /api/apps/appchange/092bdae3-a044-4314-94f4-85c65a09e31b/appupdate/description</remarks>
    /// <response code="200">returns list of app descriptions</response>
    [HttpGet]
    [Route("{appId}/appupdate/description")]
    [Authorize(Roles = "edit_apps")]
    [ProducesResponseType(typeof(IEnumerable<LocalizedDescription>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IEnumerable<LocalizedDescription>> GetAppUpdateDescriptionsAsync([FromRoute] Guid appId) =>
        await this.WithIamUserId(userId => _businessLogic.GetAppUpdateDescriptionByIdAsync(appId, userId)).ConfigureAwait(false);

    /// <summary>
    /// Create or Update description of the app by Id.
    /// </summary>
    /// <param name="appId" example="092bdae3-a044-4314-94f4-85c65a09e31b">Id for the app description to create or update.</param>
    /// <param name="offerDescriptionDatas">app description data to create or update.</param>
    /// <remarks>Example: Put: /api/apps/appchanges/092bdae3-a044-4314-94f4-85c65a09e31b/appupdate/description</remarks>
    /// <response code="204">The app description succesFully created or updated</response>
    [HttpPut]
    [Route("{appId}/appupdate/description")]
    [Authorize(Roles = "edit_apps")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NoContentResult> CreateOrUpdateAppDescriptionsByIdAsync([FromRoute] Guid appId, [FromBody] IEnumerable<LocalizedDescription> offerDescriptionDatas)
    {
        await this.WithIamUserId(userId => _businessLogic.CreateOrUpdateAppDescriptionByIdAsync(appId, userId, offerDescriptionDatas)).ConfigureAwait(false);
        return NoContent();
    }
    /// <summary>
    /// Upload offerassigned AppLeadImage document for active apps for given appId for same company as user
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="document"></param>
    /// <param name="cancellationToken"></param>
    /// <remarks>Example: POST: /api/apps/appchanges/{appId}/appLeadImage</remarks>
    /// <response code="204">Successfully uploaded the document</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist, or any other parameters are invalid.</response>
    /// <response code="403">The user is not assigned with the app.</response>
    /// <response code="415">Only PNG and JPEG files are supported.</response>
    [HttpPost]
    [Route("{appId}/appLeadImage")]
    [Authorize(Roles = "edit_apps")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(ValueLengthLimit = 819200, MultipartBodyLengthLimit = 819200)]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<NoContentResult> UploadOfferAssignedAppLeadImageDocumentByIdAsync([FromRoute] Guid appId,  [FromForm(Name = "document")] IFormFile document, CancellationToken cancellationToken)
    {
        await this.WithIamUserId(iamUserId => _businessLogic.UploadOfferAssignedAppLeadImageDocumentByIdAsync(appId, iamUserId, document, cancellationToken));
        return NoContent();
    }
}
