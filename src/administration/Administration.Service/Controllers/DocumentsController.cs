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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

/// <summary>
/// Controller providing actions for displaying, filtering and updating connectors for documents.
/// </summary>
[Route("api/administration/[controller]")]
[ApiController]
[Produces("application/json")]
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
    [HttpGet]
    [Route("{documentId}")]
    [Authorize(Roles = "view_documents")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetDocumentContentFileAsync([FromRoute] Guid documentId)
    {
        var (fileName, content) = await this.WithIamUserId(adminId => _businessLogic.GetDocumentAsync(documentId, adminId)).ConfigureAwait(false);
        return File(content, "application/pdf", fileName);
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
    [Route("{documentId}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<bool> DeleteDocumentAsync([FromRoute] Guid documentId) => 
        this.WithIamUserId(userId => _businessLogic.DeleteDocumentAsync(documentId, userId));
}
