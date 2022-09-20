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

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

/// <summary>
/// Settings used in business logic concerning connectors.
/// </summary>
public class ConnectorsSettings
{
    public ConnectorsSettings() 
    {
        SdFactoryUrl = string.Empty;
    }

    public ConnectorsSettings(string sdFactoryUrl)
    {
        SdFactoryUrl = sdFactoryUrl;
    }

    /// <summary>
    /// Maximum amount of entries per page in paginated connector lists.
    /// </summary>
    public int MaxPageSize { get; set; }

    /// <summary>
    /// SD Factory endpoint for registering connectors.
    /// </summary>
    public string SdFactoryUrl { get; set; }
}

public static class ConnectorsSettingsExtensions
{
    public static IServiceCollection ConfigureConnectorsSettings(
        this IServiceCollection services,
        IConfigurationSection section
        )
    {
        services.AddOptions<ConnectorsSettings>()
            .Bind(section)
            .ValidateOnStart();
        return services;
    }
}
