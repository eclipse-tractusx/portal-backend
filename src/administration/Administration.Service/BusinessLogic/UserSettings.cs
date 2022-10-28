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

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;

public class UserSettings
{
    public UserSettings()
    {
        Portal = null!;
        PasswordReset = null!;
    }

    [Required]
    public UserSetting Portal { get; set; }
    public PasswordReset PasswordReset { get; set; }
    public int ApplicationsMaxPageSize { get; set; }

    /// <summary>
    /// Company User Status Id
    /// </summary>
    /// <value></value>
    [Required]
    public IEnumerable<CompanyUserStatusId> CompanyUserStatusIds { get; set; } = null!;
}

public class UserSetting
{
    public UserSetting()
    {
        KeyCloakClientID = null!;
        BasePortalAddress = null!;
    }

    [Required(AllowEmptyStrings = false)]
    public string KeyCloakClientID { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string BasePortalAddress { get; set; }
}

public class PasswordReset
{
    public int NoOfHours { get; set; }
    public int MaxNoOfReset { get; set; }
}

public static class UserSettingsExtension
{
    public static IServiceCollection ConfigureUserSettings(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<UserSettings>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }
}
