﻿/********************************************************************************
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

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

/// <summary>
/// Settings used in business logic concerning service account.
/// </summary>
public class ServiceAccountSettings
{
    public ServiceAccountSettings() 
    {
        ClientId = null!;
    }

    /// <summary>
    /// Service account clientId.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ClientId { get; set; }
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
            .ValidateOnStart();
        return services;
    }
}
