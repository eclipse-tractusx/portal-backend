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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;

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
    [DistinctValues("x => x.ClientId")]
    public IEnumerable<UserRoleConfig> CatenaAdminRoles { get; set; } = null!;

    /// <summary>
    /// Notification Type Id
    /// </summary>
    /// <value></value>
    [Required]
    [EnumEnumeration]
    [DistinctValues]
    public IEnumerable<NotificationTypeId> SubmitAppNotificationTypeIds { get; set; } = null!;

    /// <summary>
    /// BasePortalAddress url required for subscription email 
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string BasePortalAddress { get; init; } = null!;

    /// <summary>
    /// AppOverview url required for the decline request email 
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string AppOverviewAddress { get; init; } = null!;

    /// <summary>
    /// Sales Manager roles
    /// </summary>
    [Required]
    [DistinctValues("x => x.ClientId")]
    public IEnumerable<UserRoleConfig> SalesManagerRoles { get; set; } = null!;

    /// <summary>
    /// Roles to notify when a new subscription was created
    /// </summary>
    [Required]
    [DistinctValues("x => x.ClientId")]
    public IEnumerable<UserRoleConfig> ServiceManagerRoles { get; set; } = null!;

    /// <summary>
    /// Offer Status Id
    /// </summary>
    /// <value></value>
    [Required]
    [EnumEnumeration]
    [DistinctValues]
    public IEnumerable<OfferStatusId> OfferStatusIds { get; set; } = null!;

    /// <summary>
    /// Active App Company Admin Roles
    /// </summary>
    /// <value></value>
    [Required]
    [DistinctValues("x => x.ClientId")]
    public IEnumerable<UserRoleConfig> ActiveAppCompanyAdminRoles { get; set; } = null!;

    /// <summary>
    /// Active App Notification Type Id
    /// </summary>
    /// <value></value>
    [Required]
    [EnumEnumeration]
    [DistinctValues]
    public IEnumerable<NotificationTypeId> ActiveAppNotificationTypeIds { get; set; } = null!;

    /// <summary>
    /// Approve App Notification Type Id
    /// </summary>
    /// <value></value>
    [EnumEnumeration]
    [DistinctValues]
    public IEnumerable<NotificationTypeId> ApproveAppNotificationTypeIds { get; set; } = null!;

    /// <summary>
    /// Roles to notify when a new subscription was created for sales and App Manager
    /// </summary>
    [Required]
    [DistinctValues("x => x.ClientId")]
    public IEnumerable<UserRoleConfig> ApproveAppUserRoles { get; set; } = null!;

    /// <summary>
    /// Max page size for pagination
    /// </summary>
    public int ApplicationsMaxPageSize { get; set; }

    /// <summary>
    /// Document Type Id for App Image
    /// </summary>
    /// <value></value>
    [Required]
    [EnumEnumeration]
    [DistinctValues]
    public IEnumerable<DocumentTypeId> AppImageDocumentTypeIds { get; set; } = null!;

    /// <summary>
    /// IT Admin Roles
    /// </summary>
    [Required]
    [DistinctValues("x => x.ClientId")]
    public IEnumerable<UserRoleConfig> ITAdminRoles { get; set; } = null!;

    /// <summary>
    /// UserManagementAddress url required for subscription email 
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string UserManagementAddress { get; init; } = null!;

    /// <summary>
    /// Document Type Id to be deleted
    /// </summary>
    [Required]
    [EnumEnumeration]
    [DistinctValues]
    public IEnumerable<DocumentTypeId> DeleteDocumentTypeIds { get; set; } = null!;

    /// <summary>
    /// Document Type Id to be deleted
    /// </summary>
    [Required]
    [EnumEnumeration]
    [DistinctValues]
    public IEnumerable<DocumentTypeId> SubmitAppDocumentTypeIds { get; set; } = null!;

    /// <summary>
    /// Document Type Id and ContentType to be uploaded
    /// </summary>
    [Required]
    [DistinctValues("x => x.DocumentTypeId")]
    public IEnumerable<UploadDocumentConfig> UploadAppDocumentTypeIds { get; set; } = null!;

    /// <summary>
    /// Client to get the technical user profile client
    /// </summary>
    public string TechnicalUserProfileClient { get; set; } = null!;

    /// <summary>
    /// Company Admin Roles
    /// </summary>
    [Required]
    [DistinctValues("x => x.ClientId")]
    public IEnumerable<UserRoleConfig> CompanyAdminRoles { get; set; } = null!;

    public static bool Validate(AppsSettings settings)
    {
        if (settings.UploadAppDocumentTypeIds.Select(x => x.DocumentTypeId).Distinct().Count() !=
            settings.UploadAppDocumentTypeIds.Count())
        {
            throw new ConfigurationException($"{nameof(UploadAppDocumentTypeIds)}: The document type id of the app documents must be unique");
        }

        return true;
    }
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
            .Validate(AppsSettings.Validate)
            .ValidateEnumEnumeration(section)
            .ValidateDistinctValues()
            .ValidateOnStart();
        return services;
    }
}
