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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class RegistrationSettings
{
    public int ApplicationsMaxPageSize { get; set; }

    /// <summary>
    /// Document Type Id
    /// </summary>
    /// <value></value>
    [Required]
    [EnumEnumeration]
    [DistinctValues]
    public IEnumerable<DocumentTypeId> DocumentTypeIds { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string HelpAddress { get; set; } = null!;

    public bool UseDimWallet { get; set; }

    /// <summary>
    ///  If <c>true</c> all sd factory calls are disabled and won't be called. The respective process steps will be skipped.
    /// </summary>
    public bool ClearinghouseConnectDisabled { get; set; }
}

public static class RegistrationSettingsExtension
{
    public static IServiceCollection ConfigureRegistrationSettings(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<RegistrationSettings>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateEnumEnumeration(section)
            .ValidateDistinctValues(section)
            .ValidateOnStart();
        return services;
    }
}
