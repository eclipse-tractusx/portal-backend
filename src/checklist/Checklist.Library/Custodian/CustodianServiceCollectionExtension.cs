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


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Custodian;

public static class CustodianServiceCollectionExtension
{
    public static IServiceCollection AddCustodianService(this IServiceCollection services, IConfigurationSection section)
    {
        services.AddOptions<CustodianSettings>()
            .Bind(section)
            .ValidateOnStart();
        services.AddTransient<LoggingHandler<CustodianService>>();

        var sp = services.BuildServiceProvider();
        var settings = sp.GetRequiredService<IOptions<CustodianSettings>>();
        services.AddHttpClient("custodian", c =>
        {
            c.BaseAddress = new Uri(settings.Value.BaseAdress);
        }).AddHttpMessageHandler<LoggingHandler<CustodianService>>();
        services.AddHttpClient("custodianAuth", c =>
        {
            c.BaseAddress = new Uri(settings.Value.KeyCloakTokenAdress);
        }).AddHttpMessageHandler<LoggingHandler<CustodianService>>();
        services
            .AddTransient<ITokenService, TokenService>()
            .AddTransient<ICustodianService, CustodianService>();

        return services;
    }
}
