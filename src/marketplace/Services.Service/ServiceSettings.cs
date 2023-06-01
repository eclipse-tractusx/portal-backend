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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service;

/// <summary>
/// Settings for the service service
/// </summary>
public class ServiceSettings
{
    [Required]
    public int ApplicationsMaxPageSize { get; init; }

    [Required]
    public IDictionary<string, IEnumerable<string>> CatenaAdminRoles { get; init; } = null!;

    [Required]
    public IDictionary<string, IEnumerable<string>> ServiceManagerRoles { get; init; } = null!;

    [Required]
    public IDictionary<string, IEnumerable<string>> SalesManagerRoles { get; init; } = null!;

    /// <summary>
    /// Notification Type Id
    /// </summary>
    /// <value></value>
    [Required]
    public IEnumerable<NotificationTypeId> SubmitServiceNotificationTypeIds { get; init; } = null!;

    /// <summary>
    /// BasePortalAddress url required for subscription email 
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string BasePortalAddress { get; init; } = null!;

    /// <summary>
    /// ServiceMarketplaceAddress url required for the rejection email 
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ServiceMarketplaceAddress { get; init; } = null!;

    /// <summary>
    /// Approve Service Notification Type Id
    /// </summary>
    /// <value></value>
    [Required]
    public IEnumerable<NotificationTypeId> ApproveServiceNotificationTypeIds { get; init; } = null!;

    /// <summary>
    /// Roles to notify when a new subscription was created for sales and App Manager
    /// </summary>
    [Required]
    public IDictionary<string, IEnumerable<string>> ApproveServiceUserRoles { get; init; } = null!;

    [Required]
    public IDictionary<string, IEnumerable<string>> ITAdminRoles { get; init; } = null!;

    /// <summary>
    /// UserManagementAddress url required for subscription email 
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string UserManagementAddress { get; init; } = null!;

    /// <summary>
    /// Service Document Type Id
    /// </summary>
    /// <value></value>
    [Required]
    public IEnumerable<DocumentTypeId> ServiceImageDocumentTypeIds { get; init; } = null!;

    /// <summary>
    /// Offer Status Ids
    /// </summary>
    /// <value></value>
    [Required]
    public IEnumerable<OfferStatusId> OfferStatusIds { get; set; } = null!;

    /// <summary>
    /// Service Document Type Id
    /// </summary>
    /// <value></value>
    [Required]
    public IEnumerable<DocumentTypeId> DeleteDocumentTypeIds { get; init; } = null!;

    /// <summary>
    /// Client to get the technical user profile client
    /// </summary>
    public string TechnicalUserProfileClient { get; set; } = null!;

    /// <summary>
    /// Document Type Id and ContentType to be uploaded
    /// </summary>
    /// <value></value>
    [Required]
    public IDictionary<DocumentTypeId, IEnumerable<string>> UploadServiceDocumentTypeIds { get; set; } = null!;

    /// <summary>
    /// Company Admin Roles
    /// </summary>
    [Required]
    public IDictionary<string, IEnumerable<string>> CompanyAdminRoles { get; set; } = null!;
}
