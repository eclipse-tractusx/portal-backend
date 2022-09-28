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


using Microsoft.Extensions.Options;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.Custodian;

public static class CustodianServiceCollectionExtension
{
    public static IServiceCollection AddCustodianService(this IServiceCollection services, IConfigurationSection section)
    {
        services.Configure<CustodianSettings>(x =>
            {
                section.Bind(x);
                if(String.IsNullOrWhiteSpace(x.Username))
                {
                    throw new Exception($"{nameof(CustodianSettings)}: {nameof(x.Username)} must not be null or empty");
                }
                if(String.IsNullOrWhiteSpace(x.Password))
                {
                    throw new Exception($"{nameof(CustodianSettings)}: {nameof(x.Password)} must not be null or empty");
                }
                if(String.IsNullOrWhiteSpace(x.ClientId))
                {
                    throw new Exception($"{nameof(CustodianSettings)}: {nameof(x.ClientId)} must not be null or empty");
                }
                if(String.IsNullOrWhiteSpace(x.GrantType))
                {
                    throw new Exception($"{nameof(CustodianSettings)}: {nameof(x.GrantType)} must not be null or empty");
                }
                if(String.IsNullOrWhiteSpace(x.ClientSecret))
                {
                    throw new Exception($"{nameof(CustodianSettings)}: {nameof(x.ClientSecret)} must not be null or empty");
                }
                if(String.IsNullOrWhiteSpace(x.Scope))
                {
                    throw new Exception($"{nameof(CustodianSettings)}: {nameof(x.Scope)} must not be null or empty");
                }
                if(String.IsNullOrWhiteSpace(x.KeyCloakTokenAdress))
                {
                    throw new Exception($"{nameof(CustodianSettings)}: {nameof(x.KeyCloakTokenAdress)} must not be null or empty");
                }
                if(String.IsNullOrWhiteSpace(x.BaseAdress))
                {
                    throw new Exception($"{nameof(CustodianSettings)}: {nameof(x.BaseAdress)} must not be null or empty");
                }
            });
        var sp = services.BuildServiceProvider();
        var settings = sp.GetRequiredService<IOptions<CustodianSettings>>();
        services.AddHttpClient("custodian", c =>
        {
            c.BaseAddress = new Uri(settings.Value.BaseAdress);
        }); 
        services.AddHttpClient("custodianAuth", c =>
        {
            c.BaseAddress = new Uri(settings.Value.KeyCloakTokenAdress);
        });
        services.AddTransient<ICustodianService, CustodianService>();

        return services;
    }
}
