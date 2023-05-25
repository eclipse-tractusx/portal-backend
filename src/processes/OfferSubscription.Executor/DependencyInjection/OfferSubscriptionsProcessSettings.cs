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

using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Executor.DependencyInjection;

public class OfferSubscriptionsProcessSettings
{
    /// <summary>
    /// IT Admin Roles
    /// </summary>
    [Required]
    public IDictionary<string, IEnumerable<string>> ItAdminRoles { get; set; } = null!;

    /// <summary>
    /// Service Manager Roles
    /// </summary>
    /// <param name="identity">Identity of the user</param>
    /// <returns>The detail data</returns>
    [Required]
    public IDictionary<string, IEnumerable<string>> ServiceManagerRoles { get; set; } = null!;

    /// <summary>
    /// BasePortalAddress url required for subscription email 
    /// </summary>
    /// <param name="data">Detail data for the service provider</param>
    /// <param name="identity">Identity of the user</param>
    [Required(AllowEmptyStrings = false)]
    public string BasePortalAddress { get; init; } = null!;
}
