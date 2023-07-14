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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.PublicInfos;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

/// <summary>
/// Controller providing actions for displaying, filtering and updating connectors for documents.
/// </summary>
[Route("api/administration/[controller]")]
[ApiController]
[Consumes("application/json")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentsBusinessLogic _businessLogic;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="documentsBusinessLogic">documents business logic.</param>
    public DocumentsController(IDocumentsBusinessLogic documentsBusinessLogic)
    {
        _businessLogic = documentsBusinessLogic;
    }

    /// <summary>
    /// Retrieves a specific document for the given id.
    /// </summary>
    /// <param name="documentId" example="4ad087bb-80a1-49d3-9ba9-da0b175cd4e3">Id of the document to get.</param>
    /// <returns>Returns the file.</returns>
    /// <remarks>Example: GET: /api/administration/documents/4ad087bb-80a1-49d3-9ba9-da0b175cd4e3</remarks>
    /// <response code="200">Returns the file.</response>
    /// <response code="403">The user is not assigned with the Company.</response>
    /// <response code="404">The document was not found.</response>
    /// <response code="503">document Content is null.</response>
    [HttpGet]
    [Route("{documentId}")]
    [Authorize(Roles = "view_documents")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult> GetDocumentContentFileAsync([FromRoute] Guid documentId)
    {
        var (fileName, content, mediaType) = await this.WithCompanyId(companyId => _businessLogic.GetDocumentAsync(documentId, companyId).ConfigureAwait(false));
        return File(content, mediaType, fileName);
    }

    /// <summary>
    /// Retrieves a specific document for the given id.
    /// </summary>
    /// <param name="documentId" example="4ad087bb-80a1-49d3-9ba9-da0b175cd4e3">Id of the document to get.</param>
    /// <returns>Returns the file.</returns>
    /// <remarks>Example: GET: /api/administration/documents/selfDescription/4ad087bb-80a1-49d3-9ba9-da0b175cd4e3</remarks>
    /// <response code="200">Returns the file.</response>
    /// <response code="404">The document was not found.</response>
    [HttpGet]
    [Route("selfDescription/{documentId}")]
    [Authorize(Roles = "view_documents")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [PublicUrl(CompanyRoleId.SERVICE_PROVIDER, CompanyRoleId.APP_PROVIDER)]
    public async Task<ActionResult> GetSelfDescriptionDocumentsAsync([FromRoute] Guid documentId)
    {
        var (fileName, content, mediaType) = await _businessLogic.GetSelfDescriptionDocumentAsync(documentId).ConfigureAwait(false);
        return File(content, mediaType, fileName);
    }

    /// <summary>
    /// Deletes the document with the given id
    /// </summary>
    /// <param name="documentId" example="4ad087bb-80a1-49d3-9ba9-da0b175cd4e3"></param>
    /// <returns></returns>
    /// <remarks>Example: Delete: /api/registration/documents/{documentId}</remarks>
    /// <response code="200">Successfully deleted the document</response>
    /// <response code="400">Incorrect document state</response>
    /// <response code="403">The user is not assigned with the Company.</response>
    /// <response code="404">The document was not found.</response>
    [HttpDelete]
    [Authorize(Roles = "delete_documents")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [Route("{documentId}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<bool> DeleteDocumentAsync([FromRoute] Guid documentId) =>
        this.WithUserId(userId => _businessLogic.DeleteDocumentAsync(documentId, userId));

    /// <summary>
    /// Gets the json the seed data for a specific document
    /// </summary>
    /// <param name="documentId" example="4ad087bb-80a1-49d3-9ba9-da0b175cd4e3"></param>
    /// <returns></returns>
    /// <remarks>
    /// Example: GET: /api/registration/documents/{documentId}/seeddata
    /// <br /> <b>this endpoint can only be used in the dev environment!</b>
    /// </remarks>
    /// <response code="200">Successfully deleted the document</response>
    /// <response code="403">Call was made from a non dev environment</response>
    /// <response code="404">The document was not found.</response>
    [HttpGet]
    [Authorize(Roles = "debug_download_documents")]
    [Route("{documentId}/seeddata")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<DocumentSeedData> GetDocumentSeedData([FromRoute] Guid documentId) =>
        _businessLogic.GetSeedData(documentId);

    /// <summary>
    /// Retrieve  document of type CX_FRAME_CONTRACT
    /// </summary>
    /// <param name="documentId"></param>
    /// <response code="200">Successfully fetched the document</response>
    /// <response code="404">No document with the given id was found.</response>
    /// <remarks>Example: Get: /api/administration/documents/frameDocuments/4ad087bb-80a1-49d3-9ba9-da0b175cd4e3</remarks>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Roles = "view_documents")]
    [Route("frameDocuments/{documentId}")]
    [Produces("application/pdf", "application/json")]
    [ProducesResponseType(typeof(File), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetFrameDocumentAsync([FromRoute] Guid documentId)
    {
        var (fileName, content) = await _businessLogic.GetFrameDocumentAsync(documentId);
        return File(content, "application/pdf", fileName);
    }
}
