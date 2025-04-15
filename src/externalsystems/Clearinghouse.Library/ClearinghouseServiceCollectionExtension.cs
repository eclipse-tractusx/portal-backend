/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;

namespace Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library;

public static class ClearinghouseServiceCollectionExtension
{
    public static IServiceCollection AddClearinghouseService(this IServiceCollection services, IConfigurationSection section)
    {
        var options = services.AddOptions<ClearinghouseSettings>()
            .Bind(section);
        options
            .EnvironmentalValidation(section);
        if (!EnvironmentExtensions.SkipValidation())
        {
            options.Validate(x => x.Validate());
        }
        services.AddTransient<LoggingHandler<ClearinghouseService>>();

        var sp = services.BuildServiceProvider();
        var settings = sp.GetRequiredService<IOptions<ClearinghouseSettings>>();
        // Get settings of all available clearing houses
        var defaultClientDetails = settings.Value.DefaultClearinghouseCredentials;
        var regionalClientDetails = settings.Value.RegionalClearinghouseCredentials.Select(x => ($"{nameof(ClearinghouseService)}{x.CountryAlpha2Code}", x.BaseAddress));
        services
            // Adding all clients of available clearing houses in advance to avoid socket exhaustion issue
            .AddCustomHttpClientWithAuthentication<ClearinghouseService>($"{nameof(ClearinghouseService)}{defaultClientDetails.CountryAlpha2Code}", defaultClientDetails.BaseAddress)
            .AddCustomHttpClientWithAuthentication<ClearinghouseService>(regionalClientDetails)
            .AddTransient<IClearinghouseService, ClearinghouseService>()
            .AddTransient<IClearinghouseBusinessLogic, ClearinghouseBusinessLogic>();

        return services;
    }
}
