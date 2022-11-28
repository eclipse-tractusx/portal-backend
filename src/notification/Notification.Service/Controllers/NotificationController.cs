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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.Keycloak.Authentication;
using Org.CatenaX.Ng.Portal.Backend.Notification.Service.BusinessLogic;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.Notification.Service.Controllers;

/// <summary>
///     Controller providing actions for creating, displaying and updating notifications.
/// </summary>
[ApiController]
[Route("api/[controller]")]
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
    ///     Creates a new notification for the given user.
    /// </summary>
    /// <param name="companyUserId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">
    ///     Id of the user to create the notification
    ///     for.
    /// </param>
    /// <param name="data">Contains the information needed to create the notification.</param>
    /// <remarks>Example: POST: /api/notification/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645</remarks>
    /// <response code="201">Notification was successfully created.</response>
    /// <response code="400">UserId not found or the NotificationType or NotificationStatus don't exist.</response>
    [HttpPost]
    [Authorize(Roles = "view_notifications")]
    [ProducesResponseType(typeof(NotificationDetailData), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<CreatedAtRouteResult> CreateNotification([FromQuery] Guid companyUserId,
        [FromBody] NotificationCreationData data)
    {
        var notificationId = await this.WithIamUserId(iamUserId => _logic.CreateNotificationAsync(iamUserId, data, companyUserId)).ConfigureAwait(false);
        return CreatedAtRoute(nameof(GetNotification), new { notificationId }, notificationId);
    }

    /// <summary>
    ///     Gets all notifications for the logged in user
    /// </summary>
    /// <param name="page">The page to get</param>
    /// <param name="size">Amount of entries</param>
    /// <param name="isRead">OPTIONAL: Filter for read or unread notifications</param>
    /// <param name="notificationTypeId">OPTIONAL: Type of the notifications</param>
    /// <param name="notificationTopicId">OPTIONAL: Topic of the notifications</param>
    /// <param name="onlyDueDate">OPTIONAL: If true only notifications with a due date will be returned</param>
    /// <param name="sorting">Defines the sorting of the list</param>
    /// <remarks>Example: Get: /api/notification/</remarks>
    /// <response code="200">Collection of the unread notifications for the user.</response>
    /// <response code="400">NotificationType or NotificationStatus don't exist.</response>
    [HttpGet]
    [Authorize(Roles = "view_notifications")]
    [Route("", Name = nameof(GetNotifications))]
    [ProducesResponseType(typeof(IEnumerable<NotificationDetailData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public Task<Pagination.Response<NotificationDetailData>> GetNotifications(
        [FromQuery] int page = 0,
        [FromQuery] int size = 15, 
        [FromQuery] bool? isRead = null,
        [FromQuery] NotificationTypeId? notificationTypeId = null,
        [FromQuery] NotificationTopicId? notificationTopicId = null,
        [FromQuery] bool onlyDueDate = false,
        [FromQuery] NotificationSorting? sorting = null) =>
        this.WithIamUserId(userId => _logic.GetNotificationsAsync(page, size, userId, isRead, notificationTypeId, notificationTopicId, sorting));

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
    [Route("{notificationId}", Name = nameof(GetNotification))]
    [ProducesResponseType(typeof(NotificationDetailData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<NotificationDetailData> GetNotification(
        [FromRoute] Guid notificationId) =>
        this.WithIamUserId(userId => _logic.GetNotificationDetailDataAsync(userId, notificationId));
    
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
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<int> NotificationCount([FromQuery] bool? isRead) =>
        this.WithIamUserId((iamUser) => _logic.GetNotificationCountAsync(iamUser, isRead));

    /// <summary>
    /// Gets the notification count for the current logged in user
    /// </summary>
    /// <returns>the count of unread notifications</returns>
    /// <remarks>Example: Get: /api/notification/count-details</remarks>
    /// <response code="200">Count of the notifications.</response>
    [HttpGet]
    [Route("count-details")]
    [Authorize(Roles = "view_notifications")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public Task<NotificationCountDetails> NotificationCountDetails() =>
        this.WithIamUserId((iamUser) => _logic.GetNotificationCountDetailsAsync(iamUser));

    /// <summary>
    /// Changes the read status of a notification
    /// </summary>
    /// <param name="notificationId" example="f22f2b57-426a-4ac3-b3af-7924a1c61590">OPTIONAL: Id of the notification status</param>
    /// <param name="isRead" example="false">OPTIONAL: <c>true</c> if the notification is read, otherwise <c>false</c></param>
    /// <returns>Return NoContent</returns>
    /// <remarks>Example: PUT: /api/notification/read/f22f2b57-426a-4ac3-b3af-7924a1c61590</remarks>
    /// <remarks>Example: PUT: /api/notification/read/f22f2b57-426a-4ac3-b3af-7924a1c61590?statusId=1</remarks>
    /// <response code="204">The Read status was updated.</response>
    /// <response code="400">NotificationStatus does not exist.</response>
    /// <response code="403">IamUserId is not assigned.</response>
    [HttpPut]
    [Route("{notificationId:guid}/read")]
    [Authorize(Roles = "view_notifications")]
    [ProducesResponseType(typeof(int), StatusCodes.Status204NoContent)]   [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> SetNotificationToRead([FromRoute] Guid notificationId, [FromQuery] bool isRead = true)
    {
        await this.WithIamUserId(userId => _logic.SetNotificationStatusAsync(userId, notificationId, isRead)).ConfigureAwait(false);
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
    [ProducesResponseType(typeof(int), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteNotification([FromRoute] Guid notificationId)
    {
        await this.WithIamUserId(userId => _logic.DeleteNotificationAsync(userId, notificationId)).ConfigureAwait(false);
        return NoContent();
    }
}
