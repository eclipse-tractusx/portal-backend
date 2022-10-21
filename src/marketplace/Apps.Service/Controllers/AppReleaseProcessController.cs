/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Microsoft.AspNetCore.Authorization;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.Controllers;

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
    /// Add app details to a newly created owned app under the app release/publishing process.
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
    /// <param name="cancellationToken"></param>
    /// <remarks>Example: PUT: /api/apps/appreleaseprocess/updateappdoc/{appId}/documentType/{documentTypeId}/documents</remarks>
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
         this.WithIamUserId(iamUserId => _appReleaseBusinessLogic.CreateAppDocumentAsync(appId, documentTypeId, document, iamUserId, cancellationToken));
    
    /// <summary>
    /// Add role and role description for App 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="appAssignedDesc"></param>
    /// <remarks>Example: POST: /api/apps/appreleaseprocess/{appId}/role</remarks>
    /// <response code="400">If sub claim is empty/invalid or user does not exist, or any other parameters are invalid.</response>
    /// <response code="404">App does not exist.</response>
    /// <response code="200">created role and role description successfully.</response>
    [HttpPost]
    [Route("{appId}/role")]
    [Authorize(Roles = "edit_apps")]
    [ProducesResponseType(typeof(IEnumerable<AppRoleData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IEnumerable<AppRoleData>> AddAppUserRole([FromRoute] Guid appId, [FromBody] IEnumerable<AppUserRole> appAssignedDesc)=>
         await this.WithIamUserId(iamUserId => _appReleaseBusinessLogic.AddAppUserRoleAsync(appId, appAssignedDesc, iamUserId)).ConfigureAwait(false);

    /// <summary>
    /// Return Agreement Data for offer_type_id App
    /// </summary>
    /// <remarks>Example: GET: /api/apps/appreleaseprocess/agreementData</remarks>
    /// <response code="200">Returns the Cpllection of agreement data</response>
    [HttpGet]
    [Route("agreementData")]
    [Authorize(Roles = "edit_apps")]
    [ProducesResponseType(typeof(IAsyncEnumerable<AgreementData>), StatusCodes.Status200OK)]
    public IAsyncEnumerable<AgreementData> GetOfferAgreementDataAsync() =>
        _appReleaseBusinessLogic.GetOfferAgreementDataAsync();
    
    /// <summary>
    /// Gets the agreement consent status for the given app id
    /// </summary>
    /// <param name="appId"></param>
    /// <remarks>Example: GET: /api/apps/appreleaseprocess/consent/{appId}</remarks>
    /// <response code="200">Returns the Offer Agreement Consent data</response>
    /// <response code="404">App does not exist.</response>
    [HttpGet]
    [Route("consent/{appId}")]
    [Authorize(Roles = "edit_apps")]
    [ProducesResponseType(typeof(OfferAgreementConsent), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<OfferAgreementConsent> GetOfferAgreementConsentById([FromRoute] Guid appId) =>
        this.WithIamUserId(iamUserId => _appReleaseBusinessLogic.GetOfferAgreementConsentById(appId, iamUserId));

    /// <summary>
    /// Update or Insert Consent
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="offerAgreementConsents"></param>
    /// <remarks>Example: POST: /api/apps/appreleaseprocess/consent/{appId}/agreementConsents</remarks>
    /// <response code="200">Successfully submitted consent to agreements</response>
    /// <response code="403">Either the user was not found or the user is not assignable to the given application.</response>
    /// <response code="404">App does not exist.</response>
    [HttpPost]
    [Authorize(Roles = "edit_apps")]
    [Route("consent/{appId}/agreementConsents")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<int> SubmitOfferConsentToAgreementsAsync([FromRoute] Guid appId, [FromBody] OfferAgreementConsent offerAgreementConsents) =>
        this.WithIamUserId(iamUserId =>
            _appReleaseBusinessLogic.SubmitOfferConsentAsync(appId, offerAgreementConsents, iamUserId));
    
    /// <summary>
    /// Return app detail with status
    /// </summary>
    /// <param name="appId"></param>
    /// <remarks>Example: GET: /api/apps/appreleaseprocess/{appId}/appStatus</remarks>
    /// <response code="200">Return the Offer and status data</response>
    /// <response code="404">App does not exist.</response>
    [HttpGet]
    [Route("{appId}/appStatus")]
    [Authorize(Roles = "app_management")]
    [ProducesResponseType(typeof(OfferProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<OfferProviderResponse> GetAppDetailsForStatusAsync([FromRoute] Guid appId) =>
        this.WithIamUserId(iamUserId => _appReleaseBusinessLogic.GetAppDetailsForStatusAsync(appId, iamUserId));
}
