/********************************************************************************
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

using CatenaX.NetworkServices.App.Service.BusinessLogic;
using CatenaX.NetworkServices.App.Service.InputModels;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Keycloak.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.App.Service.Controllers;

/// <summary>
/// Controller providing actions for updating applications.
/// </summary>
[Route("api/apps/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public class AppReleaseProcessController : ControllerBase
{
    private readonly IAppReleaseBusinessLogic _appReleaseBusinessLogic;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="appReleaseBusinessLogic"></param>
    public AppReleaseProcessController(IAppReleaseBusinessLogic appReleaseBusinessLogic)
    {
        _appReleaseBusinessLogic = appReleaseBusinessLogic;
    }
    
    /// <summary>
    /// Update active apps in the marketplace for given appId for same company as user
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="updateModel"></param>
    /// <remarks>Example: PUT: /api/apps/appreleaseprocess/updateapp/74BA5AEF-1CC7-495F-ABAA-CF87840FA6E2</remarks>
    /// <response code="204">App was successfully updated.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist, or any other parameters are invalid.</response>
    /// <response code="404">App does not exist.</response>
    [HttpPut]
    [Route("updateapp/{appId}")]
    [Authorize(Roles = "app_management")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateApp([FromRoute] Guid appId, [FromBody] AppEditableDetail updateModel) 
    {
        await this.WithIamUserId(userId => _appReleaseBusinessLogic.UpdateAppAsync(appId, updateModel, userId)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Upload document for active apps in the marketplace for given appId for same company as user
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="documentTypeId"></param>
    /// <param name="document"></param>
    /// <remarks>Example: PUT: /api/apps/appreleaseprocess/apprelease/{appId}/documentType/{documentTypeId}/documents</remarks>
    /// <response code="200">Successfully uploaded the document</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist, or any other parameters are invalid.</response>
    /// <response code="404">App does not exist.</response>
    /// <response code="403">The user is not assigned with the app.</response>
    /// <response code="415">Only PDF files are supported.</response>
    [HttpPut]
    [Route("updateappdoc/{appId}/documentType/{documentTypeId}/documents")]
    [Authorize(Roles = "app_management")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(ValueLengthLimit = 819200, MultipartBodyLengthLimit = 819200)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    public Task<int> UpdateAppDocumentAsync([FromRoute] Guid appId, [FromRoute] DocumentTypeId documentTypeId, [FromForm(Name = "document")] IFormFile document, CancellationToken cancellationToken) =>
         this.WithIamUserId(userId => _appReleaseBusinessLogic.UpdateAppDocumentAsync(appId, documentTypeId, document, userId, cancellationToken));
       
    
    
    /// <summary>
    /// Add role and role description for App 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="appAssignedDesc"></param>
    /// <remarks>Example: PUT: /api/apps/appreleaseprocess/addappuserrole/{appId}</remarks>
    /// <response code="400">If sub claim is empty/invalid or user does not exist, or any other parameters are invalid.</response>
    /// <response code="404">App does not exist.</response>
    [HttpPost]
    [Route("addappuserrole/{appId}")]
    [Authorize(Roles = "edit_apps")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddAppUserRole([FromRoute] Guid appId, [FromBody] IEnumerable<AppUserRole> appAssignedDesc)
    {
        await this.WithIamUserId(userId => _appReleaseBusinessLogic.AddAppUserRoleAsync(appId, appAssignedDesc, userId)).ConfigureAwait(false);
        return NoContent();
    }
    
}