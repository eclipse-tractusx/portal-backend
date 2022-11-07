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

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Extensions;

public static class NotificationTopicExtensions
{
    public static NotificationTopicId GetNotificationTopic(this NotificationTypeId typeId) =>
        typeId switch
        {
            NotificationTypeId.INFO => NotificationTopicId.INFO,
            NotificationTypeId.TECHNICAL_USER_CREATION => NotificationTopicId.INFO,
            NotificationTypeId.CONNECTOR_REGISTERED => NotificationTopicId.INFO,
            NotificationTypeId.WELCOME_SERVICE_PROVIDER => NotificationTopicId.INFO,
            NotificationTypeId.WELCOME_CONNECTOR_REGISTRATION => NotificationTopicId.INFO,
            NotificationTypeId.WELCOME => NotificationTopicId.INFO,
            NotificationTypeId.WELCOME_USE_CASES => NotificationTopicId.INFO,
            NotificationTypeId.WELCOME_APP_MARKETPLACE => NotificationTopicId.INFO,
            NotificationTypeId.ACTION => NotificationTopicId.ACTION,
            NotificationTypeId.APP_SUBSCRIPTION_REQUEST => NotificationTopicId.ACTION,
            NotificationTypeId.SERVICE_REQUEST => NotificationTopicId.ACTION,
            NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION => NotificationTopicId.OFFER,
            NotificationTypeId.APP_RELEASE_REQUEST => NotificationTopicId.OFFER,
            NotificationTypeId.SERVICE_ACTIVATION => NotificationTopicId.OFFER,
            _ => throw new ArgumentOutOfRangeException(nameof(typeId), typeId, "No NotificationTopicId defined for the given type")
        };
}