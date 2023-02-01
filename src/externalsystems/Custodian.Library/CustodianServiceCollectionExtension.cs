/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Logging;

namespace Org.Eclipse.TractusX.Portal.Backend.Custodian.Library;

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
        services.AddHttpClient(nameof(CustodianService), c =>
        {
            c.BaseAddress = new Uri(settings.Value.BaseAddress);
        }).AddHttpMessageHandler<LoggingHandler<CustodianService>>();
        services.AddHttpClient($"{nameof(CustodianService)}Auth", c =>
        {
            c.BaseAddress = new Uri(settings.Value.KeycloakTokenAddress);
        }).AddHttpMessageHandler<LoggingHandler<CustodianService>>();
        services
            .AddTransient<ICustodianService, CustodianService>()
            .AddTransient<ICustodianBusinessLogic, CustodianBusinessLogic>();
        
        return services;
    }
}
