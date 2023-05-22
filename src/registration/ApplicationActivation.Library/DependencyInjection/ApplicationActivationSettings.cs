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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.ApplicationActivation.Library.DependencyInjection;

public class ApplicationActivationSettings
{
    [Required]
    public IEnumerable<UserRoleConfig> ApplicationApprovalInitialRoles { get; set; } = null!;

    [Required]
    public IEnumerable<UserRoleConfig> CompanyAdminRoles { get; set; } = null!;

    /// <summary>
    /// IDs of the notification types that should be created as welcome notifications
    /// </summary>
    [Required]
    public IEnumerable<NotificationTypeId> WelcomeNotificationTypeIds { get; set; } = null!;

    /// <summary>
    /// ClientIds to remove the roles on company activation
    /// </summary>
    [Required]
    public IEnumerable<string> ClientToRemoveRolesOnActivation { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string BasePortalAddress { get; set; } = null!;

    /// <summary>
    /// Earliest time to start the activation process
    /// </summary>
    public TimeSpan? StartTime { get; set; }

    /// <summary>
    /// Latest time to start the activation process
    /// </summary>
    public TimeSpan? EndTime { get; set; }

    /// <summary>
    /// The login theme for the shared idp 
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string LoginTheme { get; set; } = null!;

    public static bool Validate(ApplicationActivationSettings settings)
    {
        var startSet = settings.StartTime.HasValue;
        var endSet = settings.EndTime.HasValue;
        switch (startSet)
        {
            case false when !endSet:
                return true;
            case true when !endSet:
            case false when endSet:
                return false;
            case true when endSet:
                return settings.StartTime!.Value.TotalDays < 1 &&
                       settings.EndTime!.Value.TotalDays < 1 &&
                       settings.StartTime.Value >= TimeSpan.Zero &&
                       settings.EndTime.Value >= TimeSpan.Zero;
        }

        return false;
    }
}
