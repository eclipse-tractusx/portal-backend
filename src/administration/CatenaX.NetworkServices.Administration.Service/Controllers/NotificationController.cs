using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Administration.Service.Controllers;

/// <summary>
/// Controller providing actions for creating, displaying and updating notifications.
/// </summary>
[ApiController]
[Route("api/administration/[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
public class NotificationController : ControllerBase
{
    private readonly INotificationBusinessLogic _logic;

    /// <summary>
    /// Creates a new instance of <see cref="NotificationController"/>
    /// </summary>
    /// <param name="logic">The business logic for the notifications</param>
    public NotificationController(INotificationBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    /// Creates a new notification for the given user.
    /// </summary>
    /// <param name="companyUserId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id of the user to create the notification for.</param>
    /// <param name="data">Contains the information needed to create the notification.</param>
    /// <remarks>Example: POST: /api/administration/notification/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645</remarks>
    /// <response code="201">Notification was successfully created.</response>
    /// <response code="400">UserId not found.</response>
    /// <response code="404">The NotificationType or NotificationStatus does not exist.</response>
    [HttpPost]
    [Route("{companyUserId}", Name = nameof(CreateNotification))]
    [Authorize(Roles = "view_notifications")]
    [ProducesResponseType(typeof(NotificationDetailData), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationDetailData>> CreateNotification([FromRoute] Guid companyUserId, [FromBody] NotificationCreationData data)
    {
        var notificationDetailData = await _logic.CreateNotification(data, companyUserId).ConfigureAwait(false);
        return CreatedAtRoute(nameof(CreateNotification), new { notificationId = notificationDetailData.Id }, notificationDetailData); // TODO - change the createdAtRoute as soon the get method exists
    }
}
