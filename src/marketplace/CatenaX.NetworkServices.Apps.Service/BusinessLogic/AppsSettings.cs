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

using CatenaX.NetworkServices.Framework.ErrorHandling;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.Apps.Service.BusinessLogic;

/// <summary>
/// settings used in business logic concerning apps.
/// </summary>
public class AppsSettings
{

    /// <summary>
    /// BasePortalAddress url required for subscription email 
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string BasePortalAddress { get; init; } = null!;
}

/// <summary>
/// app settings extension method.
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
