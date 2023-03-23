using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

/// <summary>
/// Creates a new instance of <see cref="ProcessController"/>
/// </summary>
[ApiController]
[Route("api/administration/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public class ProcessController : ControllerBase
{
    private readonly IProcessBusinessLogic _businessLogic;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="businessLogic">Prozess business logic.</param>
    public ProcessController(IProcessBusinessLogic businessLogic)
    {
        _businessLogic = businessLogic;
    }

    /// <summary>
    /// Retriggers the client creation for the given offer subscription id
    /// </summary>
    /// <param name="offerSubscriptionId" example="22dbc488-8f90-40b4-9fbd-ea0b246e827b">Id of the offer subscription that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/process/offer-subscription/{offerSubscriptionId}/retrigger-create-client
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">Either the OfferSubscription is not in status PENDING or the next step can't automatically retriggered.</response>
    /// <response code="404">No OfferSubscription found for the offerSubscriptionId.</response>
    [HttpPost]
    [Authorize(Roles = "tobedefined")]
    [Route("offer-subscription/{offerSubscriptionId}/retrigger-create-client")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerCreateClient([FromRoute] Guid offerSubscriptionId)
    {
        await _businessLogic.TriggerProcessStep(offerSubscriptionId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION).ConfigureAwait(false);
        return NoContent();
    }
    
    /// <summary>
    /// Retriggers the client creation for the given offer subscription id
    /// </summary>
    /// <param name="offerSubscriptionId" example="22dbc488-8f90-40b4-9fbd-ea0b246e827b">Id of the offer subscription that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/process/offer-subscription/{offerSubscriptionId}/retrigger-create-client
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">Either the OfferSubscription is not in status PENDING or the next step can't automatically retriggered.</response>
    /// <response code="404">No OfferSubscription found for the offerSubscriptionId.</response>
    [HttpPost]
    [Authorize(Roles = "tobedefined")]
    [Route("offer-subscription/{offerSubscriptionId}/retrigger-create-technical-user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerCreateTechnicalUser([FromRoute] Guid offerSubscriptionId)
    {
        await _businessLogic.TriggerProcessStep(offerSubscriptionId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION).ConfigureAwait(false);
        return NoContent();
    }
}