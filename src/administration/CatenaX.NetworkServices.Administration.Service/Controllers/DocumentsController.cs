using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Keycloak.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Administration.Service.Controllers;

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
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetDocumentContentFileAsync([FromQuery] Guid documentId)
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
