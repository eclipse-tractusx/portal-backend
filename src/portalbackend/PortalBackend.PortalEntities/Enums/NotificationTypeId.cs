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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

/// <summary>
/// Possible types of a notification
/// </summary>
public enum NotificationTypeId
{
    /// <summary>
    /// Notification is just an information for the user
    /// </summary>
    INFO = 1,

    /// <summary>
    /// Notification requires the user to take some kind of action
    /// </summary>
    ACTION = 2,

    /// <summary>
    /// Welcome message
    /// </summary>
    WELCOME = 3,

    /// <summary>
    /// Welcome use case explination
    /// </summary>
    WELCOME_USE_CASES = 4,

    /// <summary>
    /// Welcome - link to service provider marketplace
    /// </summary>
    WELCOME_SERVICE_PROVIDER = 5,

    /// <summary>
    /// Welcome - link to register connector
    /// </summary>
    WELCOME_CONNECTOR_REGISTRATION = 6,

    /// <summary>
    /// Welcome - link to apps
    /// </summary>
    WELCOME_APP_MARKETPLACE = 7,

    /// <summary>
    /// New App Subscription was requested
    /// </summary>
    APP_SUBSCRIPTION_REQUEST = 8,

    /// <summary>
    /// New App Subscription was activated
    /// </summary>
    APP_SUBSCRIPTION_ACTIVATION = 9,

    /// <summary>
    /// Connector was registered
    /// </summary>
    CONNECTOR_REGISTERED = 10,

    /// <summary>
    /// App Release was requested
    /// </summary>
    APP_RELEASE_REQUEST = 11,

    /// <summary>
    /// Technical user was created
    /// </summary>
    TECHNICAL_USER_CREATION = 12,

    /// <summary>
    /// Service request
    /// </summary>
    SERVICE_REQUEST = 13,

    /// <summary>
    /// Activation of a service
    /// </summary>
    SERVICE_ACTIVATION = 14,

    /// <summary>
    /// Role Added for Active App
    /// </summary>
    APP_ROLE_ADDED = 15,

    /// <summary>
    /// Approve App to change status from IN_REVIEW to ACTIVE
    /// </summary>
    APP_RELEASE_APPROVAL = 16,

    /// <summary>
    /// Service Release was requested
    /// </summary>
    SERVICE_RELEASE_REQUEST = 17,

    /// <summary>
    /// Approve Service to change status from IN_REVIEW to ACTIVE
    /// </summary>
    SERVICE_RELEASE_APPROVAL = 18,

    /// <summary>
    /// Notification when a app is rejected
    /// </summary>
    APP_RELEASE_REJECTION = 19,

    /// <summary>
    /// Notification when a service is rejected
    /// </summary>
    SERVICE_RELEASE_REJECTION = 20,

    /// <summary>
    /// Notification when the user roles are updated
    /// </summary>
    ROLE_UPDATE_CORE_OFFER = 21,

    /// <summary>
    /// Notification when the user roles are updated for an offer
    /// </summary>
    ROLE_UPDATE_APP_OFFER = 22,

    /// <summary>
    /// Notification when the url of a subscription is changed
    /// </summary>
    SUBSCRIPTION_URL_UPDATE = 23,

    /// <summary>
    /// Notification when a credential got approved
    /// </summary>
    CREDENTIAL_APPROVAL = 24,

    /// <summary>
    /// Notification when a credential got rejected
    /// </summary>
    CREDENTIAL_REJECTED = 25,

    /// <summary>
    /// Notification when a credential got rejected
    /// </summary>
    CREDENTIAL_EXPIRY = 26
}
