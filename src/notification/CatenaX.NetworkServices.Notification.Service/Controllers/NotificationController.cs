﻿// /********************************************************************************
//  * Copyright (c) 2021,2022 BMW Group AG
//  * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
//  *
//  * See the NOTICE file(s) distributed with this work for additional
//  * information regarding copyright ownership.
//  *
//  * This program and the accompanying materials are made available under the
//  * terms of the Apache License, Version 2.0 which is available at
//  * https://www.apache.org/licenses/LICENSE-2.0.
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
//  * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
//  * License for the specific language governing permissions and limitations
//  * under the License.
//  *
//  * SPDX-License-Identifier: Apache-2.0
//  ********************************************************************************/

using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.Notification.Service.BusinessLogic;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Notification.Service.Controllers;

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
    [Route("{companyUserId}", Name = nameof(CreateNotification))]
    [Authorize(Roles = "view_notifications")]
    [ProducesResponseType(typeof(NotificationDetailData), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationDetailData>> CreateNotification([FromRoute] Guid companyUserId,
        [FromBody] NotificationCreationData data)
    {
        var notificationDetailData = await _logic.CreateNotification(data, companyUserId).ConfigureAwait(false);
        return CreatedAtRoute(nameof(CreateNotification), new {notificationId = notificationDetailData.Id},
            notificationDetailData); // TODO - change the createdAtRoute as soon the get method exists
    }

    /// <summary>
    /// Gets the notification count for the current logged in user
    /// </summary>
    /// <param name="statusId" example="1">OPTIONAL: Id of the notification status</param>
    /// <returns>the count of unread notifications</returns>
    /// <response code="200">Count of the notifications.</response>
    /// <response code="400">NotificationStatus does not exist.</response>
    /// <response code="403">IamUserId is not assigned.</response>
    [HttpGet]
    [Route("count")]
    [Authorize(Roles = "view_notifications")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public Task<int> NotificationCount([FromQuery] NotificationStatusId? statusId) =>
        this.WithIamUserId((iamUser) => _logic.GetNotificationCount(iamUser, statusId));
}
