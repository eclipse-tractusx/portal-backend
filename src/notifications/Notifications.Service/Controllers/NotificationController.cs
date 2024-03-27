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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Web.Identity;

namespace Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.Controllers;

/// <summary>
///     Controller providing actions for creating, displaying and updating notifications.
/// </summary>
[ApiController]
[EnvironmentRoute("MVC_ROUTING_BASEPATH")]
[Consumes("application/json")]
[Produces("application/json")]
public class NotificationController : ControllerBase
{
    private readonly INotificationBusinessLogic _logic;

    /// <summary>
    ///     Creates a new instance of <see cref="NotificationController" />
    /// </summary>
    /// <param name="logic">The business logic for the notifications</param>
    public NotificationController(INotificationBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    ///     Gets all notifications for the logged in user
    /// </summary>
    /// <remarks>Example: Get: /api/notification/</remarks>
    /// <param name="searchTypeIds">OPTIONAL: types for the search</param>
    /// <param name="page">The page to get</param>
    /// <param name="size">Amount of entries</param>
    /// <param name="searchSemantic">OPTIONAL: choose AND or OR semantics (defaults to AND)</param>
    /// <param name="isRead">OPTIONAL: Filter for read or unread notifications</param>
    /// <param name="notificationTypeId">OPTIONAL: Type of the notifications</param>
    /// <param name="notificationTopicId">OPTIONAL: Topic of the notifications</param>
    /// <param name="onlyDueDate">OPTIONAL: If true only notifications with a due date will be returned</param>
    /// <param name="sorting">Defines the sorting of the list</param>
    /// <param name="doneState">OPTIONAL: Defines the done state</param>
    /// <param name="searchQuery">OPTIONAL: a search query</param>
    /// <response code="200">Collection of the unread notifications for the user.</response>
    /// <response code="400">NotificationType or NotificationStatus don't exist.</response>
    [HttpGet]
    [Authorize(Roles = "view_notifications")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Route("", Name = nameof(GetNotifications))]
    [ProducesResponseType(typeof(IEnumerable<NotificationDetailData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public Task<Pagination.Response<NotificationDetailData>> GetNotifications(
        [FromQuery] IEnumerable<NotificationTypeId> searchTypeIds,
        [FromQuery] int page = 0,
        [FromQuery] int size = 15,
        [FromQuery] SearchSemanticTypeId searchSemantic = SearchSemanticTypeId.AND,
        [FromQuery] bool? isRead = null,
        [FromQuery] NotificationTypeId? notificationTypeId = null,
        [FromQuery] NotificationTopicId? notificationTopicId = null,
        [FromQuery] bool onlyDueDate = false,
        [FromQuery] NotificationSorting? sorting = null,
        [FromQuery] bool? doneState = null,
        [FromQuery] string? searchQuery = null
        ) =>
        _logic.GetNotificationsAsync(page, size, new NotificationFilters(isRead, notificationTypeId, notificationTopicId, onlyDueDate, sorting, doneState, searchTypeIds, searchQuery), searchSemantic);

    /// <summary>
    ///     Gets a notification for the logged in user
    /// </summary>
    /// <param name="notificationId">is of the notification</param>
    /// <remarks>Example: Get: /api/notification/f22f2b57-426a-4ac3-b3af-7924a1c61590</remarks>
    /// <response code="200">notifications for the user.</response>
    /// <response code="400">notification doesn't exist.</response>
    /// <response code="403">User is not assigned.</response>
    [HttpGet]
    [Authorize(Roles = "view_notifications")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Route("{notificationId}", Name = nameof(GetNotification))]
    [ProducesResponseType(typeof(NotificationDetailData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<NotificationDetailData> GetNotification([FromRoute] Guid notificationId) =>
        _logic.GetNotificationDetailDataAsync(notificationId);

    /// <summary>
    /// Gets the notification count for the current logged in user
    /// </summary>
    /// <param name="isRead" example="true">OPTIONAL: Filter for read or unread notifications</param>
    /// <returns>the count of unread notifications</returns>
    /// <remarks>Example: Get: /api/notification/count</remarks>
    /// <remarks>Example: Get: /api/notification/count?isRead=true</remarks>
    /// <response code="200">Count of the notifications.</response>
    /// <response code="400">NotificationStatus does not exist.</response>
    /// <response code="403">IamUserId is not assigned.</response>
    [HttpGet]
    [Route("count")]
    [Authorize(Roles = "view_notifications")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<int> NotificationCount([FromQuery] bool? isRead) =>
        _logic.GetNotificationCountAsync(isRead);

    /// <summary>
    /// Gets the notification count for the current logged in user
    /// </summary>
    /// <returns>the count of unread notifications</returns>
    /// <remarks>Example: Get: /api/notification/count-details</remarks>
    /// <response code="200">Count of the notifications.</response>
    [HttpGet]
    [Route("count-details")]
    [Authorize(Roles = "view_notifications")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [ProducesResponseType(typeof(NotificationCountDetails), StatusCodes.Status200OK)]
    public Task<NotificationCountDetails> NotificationCountDetails() =>
        _logic.GetNotificationCountDetailsAsync();

    /// <summary>
    /// Changes the read status of a notification
    /// </summary>
    /// <param name="notificationId" example="f22f2b57-426a-4ac3-b3af-7924a1c61590">OPTIONAL: Id of the notification status</param>
    /// <param name="isRead" example="false">OPTIONAL: <c>true</c> if the notification is read, otherwise <c>false</c></param>
    /// <returns>Return NoContent</returns>
    /// <remarks>Example: PUT: /api/notification/read/f22f2b57-426a-4ac3-b3af-7924a1c61590/read</remarks>
    /// <remarks>Example: PUT: /api/notification/read/f22f2b57-426a-4ac3-b3af-7924a1c61590/read?isRead=false</remarks>
    /// <response code="204">The Read status was updated.</response>
    /// <response code="400">NotificationStatus does not exist.</response>
    /// <response code="403">IamUserId is not assigned.</response>
    [HttpPut]
    [Route("{notificationId:guid}/read")]
    [Authorize(Roles = "view_notifications")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> SetNotificationToRead([FromRoute] Guid notificationId, [FromQuery] bool isRead = true)
    {
        await _logic.SetNotificationStatusAsync(notificationId, isRead).ConfigureAwait(ConfigureAwaitOptions.None);
        return NoContent();
    }

    /// <summary>
    /// Delete the
    /// </summary>
    /// <param name="notificationId" example="f22f2b57-426a-4ac3-b3af-7924a1c615901">Id of the notification</param>
    /// <returns>Return NoContent</returns>
    /// <remarks>Example: DELETE: /api/notification/f22f2b57-426a-4ac3-b3af-7924a1c615901</remarks>
    /// <response code="204">Count of the notifications.</response>
    /// <response code="400">NotificationStatus does not exist.</response>
    /// <response code="403">IamUserId is not assigned.</response>
    [HttpDelete]
    [Route("{notificationId:guid}")]
    [Authorize(Roles = "view_notifications")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [ProducesResponseType(typeof(int), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteNotification([FromRoute] Guid notificationId)
    {
        await _logic.DeleteNotificationAsync(notificationId).ConfigureAwait(ConfigureAwaitOptions.None);
        return NoContent();
    }
}
