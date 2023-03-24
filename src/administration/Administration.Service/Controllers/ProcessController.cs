using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
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
    /// Gets the process steps for the given offer subscription id
    /// </summary>
    /// <param name="offerSubscriptionId" example="22dbc488-8f90-40b4-9fbd-ea0b246e827b">Id of the offer subscription that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/process/offer-subscription/{offerSubscriptionId}
    /// <response code="200">Returns the process steps.</response>
    /// <response code="400">Either the OfferSubscription is not in status PENDING or the next step can't automatically retriggered.</response>
    /// <response code="404">No OfferSubscription found for the offerSubscriptionId.</response>
    [HttpGet]
    [Authorize(Roles = "tobedefined")]
    [Route("offer-subscription/{offerSubscriptionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IAsyncEnumerable<ProcessStepData> GetProcessStepsForSubscription([FromRoute] Guid offerSubscriptionId) => 
        _businessLogic.GetProcessStepsForSubscription(offerSubscriptionId);

    /// <summary>
    /// Retriggers the provider for the given offer subscription id
    /// </summary>
    /// <param name="offerSubscriptionId" example="22dbc488-8f90-40b4-9fbd-ea0b246e827b">Id of the offer subscription that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/process/offer-subscription/{offerSubscriptionId}/retrigger-provider
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">Either the OfferSubscription is not in status PENDING or the next step can't automatically retriggered.</response>
    /// <response code="404">No OfferSubscription found for the offerSubscriptionId.</response>
    [HttpPost]
    [Authorize(Roles = "tobedefined")]
    [Route("offer-subscription/{offerSubscriptionId}/retrigger-provider")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerProvider([FromRoute] Guid offerSubscriptionId)
    {
        await _businessLogic.TriggerProcessStep(offerSubscriptionId, ProcessStepTypeId.RETRIGGER_PROVIDER).ConfigureAwait(false);
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
    /// Retriggers the single instance details for the given offer subscription id
    /// </summary>
    /// <param name="offerSubscriptionId" example="22dbc488-8f90-40b4-9fbd-ea0b246e827b">Id of the offer subscription that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/process/offer-subscription/{offerSubscriptionId}/retrigger-single-instance-details
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">Either the OfferSubscription is not in status PENDING or the next step can't automatically retriggered.</response>
    /// <response code="404">No OfferSubscription found for the offerSubscriptionId.</response>
    [HttpPost]
    [Authorize(Roles = "tobedefined")]
    [Route("offer-subscription/{offerSubscriptionId}/retrigger-single-instance-details")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerSingleInstanceDetailsCreation([FromRoute] Guid offerSubscriptionId)
    {
        await _businessLogic.TriggerProcessStep(offerSubscriptionId, ProcessStepTypeId.RETRIGGER_SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retriggers the technical user creation for the given offer subscription id
    /// </summary>
    /// <param name="offerSubscriptionId" example="22dbc488-8f90-40b4-9fbd-ea0b246e827b">Id of the offer subscription that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/process/offer-subscription/{offerSubscriptionId}/retrigger-create-technical-user
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
    
    /// <summary>
    /// Retriggers the activation for the given offer subscription id
    /// </summary>
    /// <param name="offerSubscriptionId" example="22dbc488-8f90-40b4-9fbd-ea0b246e827b">Id of the offer subscription that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/process/offer-subscription/{offerSubscriptionId}/retrigger-activation
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">Either the OfferSubscription is not in status PENDING or the next step can't automatically retriggered.</response>
    /// <response code="404">No OfferSubscription found for the offerSubscriptionId.</response>
    [HttpPost]
    [Authorize(Roles = "tobedefined")]
    [Route("offer-subscription/{offerSubscriptionId}/retrigger-activation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerActivation([FromRoute] Guid offerSubscriptionId)
    {
        await _businessLogic.TriggerProcessStep(offerSubscriptionId, ProcessStepTypeId.RETRIGGER_ACTIVATE_SUBSCRIPTION).ConfigureAwait(false);
        return NoContent();
    }
    
    /// <summary>
    /// Retriggers the provider callback for the given offer subscription id
    /// </summary>
    /// <param name="offerSubscriptionId" example="22dbc488-8f90-40b4-9fbd-ea0b246e827b">Id of the offer subscription that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/process/offer-subscription/{offerSubscriptionId}/retrigger-provider-callback
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">Either the OfferSubscription is not in status ACTIVE or the next step can't automatically retriggered.</response>
    /// <response code="404">No OfferSubscription found for the offerSubscriptionId.</response>
    [HttpPost]
    [Authorize(Roles = "tobedefined")]
    [Route("offer-subscription/{offerSubscriptionId}/retrigger-provider-callback")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerProviderCallback([FromRoute] Guid offerSubscriptionId)
    {
        await _businessLogic.TriggerProcessStep(offerSubscriptionId, ProcessStepTypeId.RETRIGGER_PROVIDER_CALLBACK, false).ConfigureAwait(false);
        return NoContent();
    }
}