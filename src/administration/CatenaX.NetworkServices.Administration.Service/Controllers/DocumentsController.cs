using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
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
    /// <param name="connectorsBusinessLogic">Connectors business logic.</param>
    public DocumentsController(IDocumentsBusinessLogic connectorsBusinessLogic)
    {
        _businessLogic = connectorsBusinessLogic;
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
    public async Task<ActionResult> GetCompanyConnectorsForCurrentUserAsync([FromQuery] Guid documentId)
    {
        var (fileName, content) = await this.WithIamUserId(adminId => _businessLogic.GetDocumentAsync(documentId, adminId));
        return File(content, "application/pdf", fileName);
    }
        

}