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

using System.ComponentModel.DataAnnotations;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.Services.Service;

/// <summary>
/// Settings for the service service
/// </summary>
public class ServiceSettings
{
    [Required]
    public int ApplicationsMaxPageSize { get; init; }

    [Required]
    public IDictionary<string,IEnumerable<string>> CompanyAdminRoles { get; init; } = null!;
    
    [Required]
    public IDictionary<string,IEnumerable<string>> ServiceAccountRoles { get; init; } = null!;

    [Required]
    public IDictionary<string,IEnumerable<string>> ServiceManagerRoles { get; init; } = null!;

    [Required]
    public IDictionary<string,IEnumerable<string>> SalesManagerRoles { get; init; } = null!;
    
    /// <summary>
    /// Notification Type Id
    /// </summary>
    /// <value></value>
    [Required]
    public IEnumerable<NotificationTypeId> SubmitServiceNotificationTypeIds { get; set; } = null!;

    /// <summary>
    /// BasePortalAddress url required for subscription email 
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string BasePortalAddress { get; init; } = null!;
}

public static class ServiceSettingsExtension
{
    public static IServiceCollection ConfigureServiceSettings(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<ServiceSettings>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }
}
