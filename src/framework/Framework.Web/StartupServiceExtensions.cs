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

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Cors;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Swagger;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Web;

public static class StartupServiceExtensions
{
    public static IServiceCollection AddDefaultServices<TProgram>(this IServiceCollection services, IConfigurationRoot configuration, string version, string cookieName, bool useMinimalApi)
    {
        services.AddCors(options => options.SetupCors(configuration));
        if (useMinimalApi)
        {
            services.AddScoped<GeneralHttpExceptionMiddleware>();
        }
        else
        {
            services.AddScoped<CustomAuthorizationMiddleware>();
        }

        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.Cookie.Name = cookieName;
            options.IdleTimeout = TimeSpan.FromMinutes(10);
        });

        services.AddControllers(options =>
            {
                if (!useMinimalApi)
                    options.Filters.Add<GeneralHttpExceptionFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
            });

        services.AddSwaggerGen(c => SwaggerGenConfiguration.SetupSwaggerGen<TProgram>(c, version));

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            configuration.Bind("JwtBearerOptions", options);
            if (!options.RequireHttpsMetadata)
            {
                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                };
            }
        });
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        var options = services
            .AddOptions<JwtBearerOptions>()
            .Bind(configuration.GetSection("JwtBearerOptions"));

        if (!EnvironmentExtensions.SkipValidation())
        {
            options.ValidateOnStart();
        }

        services.AddHealthChecks()
            .AddCheck<JwtBearerConfigurationHealthCheck>("JwtBearerConfiguration", tags: ["keycloak"]);

        services.AddHttpContextAccessor();

        services.AddDateTimeProvider();
        services.AutoRegister();
        return services;
    }
}
