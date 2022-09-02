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

using CatenaX.NetworkServices.Keycloak.Factory;
using CatenaX.NetworkServices.Provisioning.DBAccess;
using CatenaX.NetworkServices.Provisioning.ProvisioningEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.Provisioning.Library;

public static class ProvisioningManagerStartupServiceExtensions
{
    public static IServiceCollection AddProvisioningManager(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IKeycloakFactory, KeycloakFactory>()
            .ConfigureKeycloakSettingsMap(configuration.GetSection("Keycloak"))
            .AddTransient<IProvisioningManager, ProvisioningManager>()
            .ConfigureProvisioningSettings(configuration.GetSection("Provisioning"));
        
        var connectionString = configuration.GetConnectionString("ProvisioningDB");
        if (connectionString != null)
        {
            services.AddTransient<IProvisioningDBAccess, ProvisioningDBAccess>()
                .AddDbContext<ProvisioningDBContext>(options =>
                    options.UseNpgsql(connectionString));
        }
        return services;
    }
}
