/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Cors;

/// <summary>
/// Provides Extension methods for cors
/// </summary>
public static class CorsExtensions
{
    public const string AllowSpecificOrigins = "_catenaXAllowSpecificOrigins";

    /// <summary>
    /// Setup for the cors configuration
    /// </summary>
    /// <param name="corsOption">Cors options</param>
    /// <param name="configuration">configuration to access the allowed domains</param>
    public static void SetupCors(this CorsOptions corsOption, IConfigurationRoot configuration)
    {
        var corsConfig = configuration.Get<CorsConfiguration>();
        corsOption.AddPolicy(AllowSpecificOrigins, policy =>
        {
            policy.WithOrigins(corsConfig.Cors.AllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    }
}
