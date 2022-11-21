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
using System.ComponentModel.DataAnnotations;

namespace Org.CatenaX.Ng.Portal.Backend.Apps.Service.BusinessLogic;

/// <summary>
/// Config Settings for Apps
/// </summary>
public class AppsSettings
{
    /// <summary>
    /// Company Admin Roles
    /// </summary>
    /// <value></value>
    [Required]
    public IDictionary<string,IEnumerable<string>> CompanyAdminRoles { get; set; } = null!;

    /// <summary>
    /// Notification Type Id
    /// </summary>
    /// <value></value>
    [Required]
    public IEnumerable<NotificationTypeId> NotificationTypeIds { get; set; } = null!;
    
    /// <summary>
    /// Document Type Id
    /// </summary>
    /// <value></value>
    [Required]
    public IEnumerable<DocumentTypeId> DocumentTypeIds { get; set; } = null!;

    /// <summary>
    /// BasePortalAddress url required for subscription email 
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string BasePortalAddress { get; init; } = null!;
    
    /// <summary>
    /// Service account roles
    /// </summary>
    [Required]
    public IDictionary<string,IEnumerable<string>> ServiceAccountRoles { get; set; } = null!;

    /// <summary>
    /// Sales Manager roles
    /// </summary>
    [Required]
    public IDictionary<string,IEnumerable<string>> SalesManagerRoles { get; set; } = null!;
    
    /// <summary>
    /// Roles to notify when a new subscription was created
    /// </summary>
    [Required]
    public IDictionary<string, IEnumerable<string>> ServiceManagerRoles { get; set; } = null!;
    
    /// <summary>
    /// Document Content Type Settings
    /// </summary>
    /// <value></value>
    [Required]
    public IEnumerable<string> ContentTypeSettings { get; set; } = null!;

    /// <summary>
    /// Document Type Id
    /// </summary>
    /// <value></value>
    [Required]
    public IEnumerable<OfferStatusId> OfferStatusIds { get; set; } = null!;
     /// <summary>
    /// Active App Company Admin Roles
    /// </summary>
    /// <value></value>
    [Required]
    public IDictionary<string,IEnumerable<string>> ActiveAppCompanyAdminRoles { get; set; } = null!;

    /// <summary>
    /// Active App Notification Type Id
    /// </summary>
    /// <value></value>
    [Required]
    public IEnumerable<NotificationTypeId> ActiveAppNotificationTypeIds { get; set; } = null!;
}

/// <summary>
/// App Settings extension class.
/// </summary>
public static class AppsSettingsExtension
{
    /// <summary>
    /// configure apps settings using service collection interface
    /// </summary>
    public static IServiceCollection ConfigureAppsSettings(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<AppsSettings>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }
}
