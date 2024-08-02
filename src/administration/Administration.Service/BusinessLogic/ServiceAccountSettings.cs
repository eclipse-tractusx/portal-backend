/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Settings used in business logic concerning service account.
/// </summary>
public class ServiceAccountSettings
{
    /// <summary>
    /// Service account clientId.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ClientId { get; set; } = null!;

    [Required]
    public int EncryptionConfigIndex { get; set; }

    [Required]
    [DistinctValues("x => x.Index")]
    public IEnumerable<EncryptionModeConfig> EncryptionConfigs { get; set; } = null!;

    public int LockExpirySeconds { get; set; }
}

public static class ServiceAccountSettingsExtensions
{
    public static IServiceCollection ConfigureServiceAccountSettings(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<ServiceAccountSettings>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateDistinctValues(section)
            .ValidateOnStart();
        return services;
    }
}
