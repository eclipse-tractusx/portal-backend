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

namespace Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.BusinessLogic;

/// <summary>
/// Settings for the notification service
/// </summary>
public class NotificationSettings
{
    /// <summary>
    /// Max Page Size
    /// </summary>
    public int MaxPageSize { get; set; }
}

/// <summary>
/// Notification Settings extension class.
/// </summary>
public static class NotificationSettingsExtension
{
    /// <summary>
    /// configure notification settings using service collection interface
    /// </summary>
    public static IServiceCollection ConfigureNotificationSettings(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<NotificationSettings>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }
}
