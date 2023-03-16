/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
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

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;

public class RegistrationSettings
{
    public RegistrationSettings()
    {
        KeycloakClientID = null!;
        BasePortalAddress = null!;
    }

    [Required(AllowEmptyStrings = false)]
    public string KeycloakClientID { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    public string BasePortalAddress { get; set; }

    /// <summary>
    /// Document Type Id
    /// </summary>
    /// <value></value>
    [Required]
    public IEnumerable<DocumentTypeId> DocumentTypeIds { get; set; } = null!;

    /// <summary>
    /// Document Type Id
    /// </summary>
    /// <value></value>
    [Required]
    public IEnumerable<CompanyApplicationStatusId> ApplicationStatusIds { get; set; } = null!;
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
            .ValidateOnStart();
        return services;
    }
}
