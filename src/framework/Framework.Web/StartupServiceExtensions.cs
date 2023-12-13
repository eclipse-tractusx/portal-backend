/********************************************************************************
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

using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Cors;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.PublicInfos.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Swagger;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Web;

public static class StartupServiceExtensions
{
    public static IServiceCollection AddDefaultServices<TProgram>(this IServiceCollection services, IConfigurationRoot configuration, ApiVersion defaultApiVersion)
    {
        services.AddCors(options => options.SetupCors(configuration));

        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.Cookie.Name = ".Portal";
            options.IdleTimeout = TimeSpan.FromMinutes(10);
        });

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
            });

        services
            .AddApiVersioning(setup =>
            {
                setup.DefaultApiVersion = defaultApiVersion;
                setup.AssumeDefaultVersionWhenUnspecified = true;
                setup.ReportApiVersions = true;
                setup.UnsupportedApiVersionStatusCode = (int)HttpStatusCode.NotFound;
            })
            .AddApiExplorer(options =>
            {
                // the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VV";
                options.SubstituteApiVersionInUrl = true;
            });

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen(options =>
        {
            try
            {

                var filePath = Path.Combine(AppContext.BaseDirectory, $"{typeof(TProgram).Assembly.FullName?.Split(',')[0]}.xml");
                options.IncludeXmlComments(filePath);
            }
            catch (Exception e)
            {
                throw new ConfigurationException("error configuring swagger xmldocumentation", e);
            }
        });

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
        services.AddTransient<IAuthorizationHandler, MandatoryIdentityClaimHandler>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyTypes.ValidIdentity, policy => policy.Requirements.Add(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidIdentity)));
            options.AddPolicy(PolicyTypes.ValidCompany, policy => policy.Requirements.Add(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidCompany)));
            options.AddPolicy(PolicyTypes.CompanyUser, policy => policy.Requirements.Add(new MandatoryIdentityClaimRequirement(PolicyTypeId.CompanyUser)));
            options.AddPolicy(PolicyTypes.ServiceAccount, policy => policy.Requirements.Add(new MandatoryIdentityClaimRequirement(PolicyTypeId.ServiceAccount)));
        });

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>()
            .AddOptions<JwtBearerOptions>()
            .Bind(configuration.GetSection("JwtBearerOptions"))
            .ValidateOnStart();

        services.AddHealthChecks()
            .AddCheck<JwtBearerConfigurationHealthCheck>("JwtBearerConfiguration", tags: new[] { "keycloak" });

        services.AddHttpContextAccessor();
        services.AddClaimsIdentityIdDetermination();

        services.AddDateTimeProvider();
        services.AddPublicInfos();
        return services;
    }
}
