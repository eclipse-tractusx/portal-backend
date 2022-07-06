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

using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <inheritdoc />
public class NotificationRepository : INotificationRepository
{
    private readonly PortalDbContext _dbContext;

    /// <summary>
    /// Creates a new instance of <see cref="NotificationRepository"/>
    /// </summary>
    /// <param name="dbContext">Access to the database</param>
    public NotificationRepository(PortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    /// <inheritdoc />
    public void Add(Notification notification)
    {
        _dbContext.Add(notification);
    }

    /// <inheritdoc />
    public async Task<int> GetNotificationCountAsync(Guid companyUserId, NotificationStatusId? statusId) =>
        await _dbContext.Notifications.CountAsync(x =>
            x.ReceiverUserId == companyUserId &&
            statusId.HasValue ? x.ReadStatusId == statusId.Value : true);
}
