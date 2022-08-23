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

using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using CatenaX.NetworkServices.Framework.Cors;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.Framework.Swagger;
using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.Keycloak.Factory;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.Mailing.Template;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatenaX.NetworkServices.Framework.Web;

public static class StartupServiceExtensions
{
    public static IServiceCollection AddDefaultServices(this IServiceCollection services, IConfigurationRoot configuration, string version, string? tag, bool addAuthorization = true)
    {
        services.AddCors(options => options.SetupCors(configuration));

        services.AddControllers()
            .AddJsonOptions(options => {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
            });

        services.AddSwaggerGen(c => SwaggerGenConfiguration.SetupSwaggerGen(c, version, tag));

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options => {
            configuration.Bind("JwtBearerOptions", options);
            if (!options.RequireHttpsMetadata)
            {
                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = ServerCertificateValidationExtensions.ServerCertificateCustomValidation
                };
            }
        });

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        if(addAuthorization)
        {
            services.AddTransient<IAuthorizationHandler, ClaimRequestPathHandler>()
            .AddAuthorization(option =>
            {
                option.AddPolicy("CheckTenant",
                    policy => { policy.AddRequirements(new ClaimRequestPathRequirement("tenant", "tenant")); });
            })
            .AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            
            services.AddTransient<IKeycloakFactory, KeycloakFactory>()
                .ConfigureKeycloakSettingsMap(configuration.GetSection("Keycloak"));
        }

        services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>()
            .Configure<JwtBearerOptions>(options => configuration.Bind("JwtBearerOptions", options));

        services.AddTransient<IPortalRepositories, PortalRepositories>();
        services.AddDbContext<PortalDbContext>(o => o.UseNpgsql(configuration.GetConnectionString("PortalDB")));

        return services;
    }

    public static IServiceCollection AddMailingAndTemplateManager(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.AddTransient<IMailingService, MailingService>()
            .AddTransient<ISendMail, SendMail>()
            .AddTransient<ITemplateManager, TemplateManager>()
            .ConfigureTemplateSettings(configuration.GetSection(Constants.MailingTemplates))
            .ConfigureMailSettings(configuration.GetSection(MailSettings.Position));

        return services;
    }
}