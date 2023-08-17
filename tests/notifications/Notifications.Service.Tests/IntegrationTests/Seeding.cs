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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.TestSeeds;

namespace Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.Tests;

public class Seeding : IBaseSeeding
{
    public Action<PortalDbContext> SeedData() => dbContext =>
    {
        BaseSeed.SeedBaseData().Invoke(dbContext);

        dbContext.NotificationTypeAssignedTopics.AddRange(new List<NotificationTypeAssignedTopic>()
        {
            new(NotificationTypeId.INFO, NotificationTopicId.INFO),
            new(NotificationTypeId.TECHNICAL_USER_CREATION, NotificationTopicId.INFO),
            new(NotificationTypeId.CONNECTOR_REGISTERED, NotificationTopicId.INFO),
            new(NotificationTypeId.WELCOME_SERVICE_PROVIDER, NotificationTopicId.INFO),
            new(NotificationTypeId.WELCOME_CONNECTOR_REGISTRATION, NotificationTopicId.INFO),
            new(NotificationTypeId.WELCOME, NotificationTopicId.INFO),
            new(NotificationTypeId.WELCOME_USE_CASES, NotificationTopicId.INFO),
            new(NotificationTypeId.WELCOME_APP_MARKETPLACE, NotificationTopicId.INFO),
            new(NotificationTypeId.ACTION, NotificationTopicId.ACTION),
            new(NotificationTypeId.APP_SUBSCRIPTION_REQUEST, NotificationTopicId.ACTION),
            new(NotificationTypeId.SERVICE_REQUEST, NotificationTopicId.ACTION),
            new(NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION, NotificationTopicId.OFFER),
            new(NotificationTypeId.APP_RELEASE_REQUEST, NotificationTopicId.OFFER),
            new(NotificationTypeId.SERVICE_ACTIVATION, NotificationTopicId.OFFER),
            new(NotificationTypeId.APP_ROLE_ADDED, NotificationTopicId.OFFER),
            new(NotificationTypeId.APP_RELEASE_APPROVAL, NotificationTopicId.OFFER),
            new(NotificationTypeId.SERVICE_RELEASE_REQUEST, NotificationTopicId.OFFER),
            new(NotificationTypeId.SERVICE_RELEASE_APPROVAL, NotificationTopicId.OFFER),
            new(NotificationTypeId.APP_RELEASE_REJECTION, NotificationTopicId.OFFER),
            new(NotificationTypeId.SERVICE_RELEASE_REJECTION, NotificationTopicId.OFFER)
        });

        dbContext.Notifications.AddRange(new List<Notification>
        {
            new (new Guid("94F22922-04F6-4A4E-B976-1BF2FF3DE973"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), DateTimeOffset.UtcNow, NotificationTypeId.ACTION, false),
            new (new Guid("5FCBA636-E0F6-4C86-B5CC-7711A55669B6"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), DateTimeOffset.UtcNow, NotificationTypeId.ACTION, true),
            new (new Guid("8bdaada7-4885-4aa7-87ce-1a325492a485"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), DateTimeOffset.UtcNow, NotificationTypeId.ACTION, true),
            new (new Guid("9D03FE54-3581-4399-84DD-D606E9A2B3D5"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), DateTimeOffset.UtcNow, NotificationTypeId.ACTION, false),
            new (new Guid("34782A2E-7B54-4E78-85BA-419AF534837F"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), DateTimeOffset.UtcNow, NotificationTypeId.INFO, true),
            new (new Guid("19AFFED7-13F0-4868-9A23-E77C23D8C889"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), DateTimeOffset.UtcNow, NotificationTypeId.INFO, false),
        });
    };
}
