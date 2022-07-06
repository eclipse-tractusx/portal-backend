// /********************************************************************************
//  * Copyright (c) 2021,2022 BMW Group AG
//  * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
//  *
//  * See the NOTICE file(s) distributed with this work for additional
//  * information regarding copyright ownership.
//  *
//  * This program and the accompanying materials are made available under the
//  * terms of the Apache License, Version 2.0 which is available at
//  * https://www.apache.org/licenses/LICENSE-2.0.
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
//  * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
//  * License for the specific language governing permissions and limitations
//  * under the License.
//  *
//  * SPDX-License-Identifier: Apache-2.0
//  ********************************************************************************/

#region

using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.Keycloak.Factory;
using CatenaX.NetworkServices.Keycloak.Factory.Utils;
using CatenaX.NetworkServices.Notification.Service.BusinessLogic;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

#endregion

var VERSION = "v2";
var TAG = typeof(Program).Namespace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Kubernetes")
{
    var provider = new PhysicalFileProvider("/app/secrets");
    builder.Configuration.AddJsonFile(provider, "appsettings.json", false, false);
}

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

builder.Services.AddSwaggerGen(c => { c.SwaggerDoc(VERSION, new OpenApiInfo {Title = TAG, Version = VERSION}); });

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    builder.Configuration.Bind("JwtBearerOptions", options);
    if (!options.RequireHttpsMetadata)
        options.BackchannelHttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (a, b, c, d) => true
        };
});

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddTransient<IAuthorizationHandler, ClaimRequestPathHandler>()
    .AddAuthorization(option =>
    {
        option.AddPolicy("CheckTenant",
            policy => { policy.AddRequirements(new ClaimRequestPathRequirement("tenant", "tenant")); });
    })
    .AddTransient<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>()
    .Configure<JwtBearerOptions>(options => builder.Configuration.Bind("JwtBearerOptions", options));

builder.Services.AddTransient<IKeycloakFactory, KeycloakFactory>()
    .ConfigureKeycloakSettingsMap(builder.Configuration.GetSection("Keycloak"));

builder.Services.AddTransient<INotificationBusinessLogic, NotificationBusinessLogic>();
builder.Services.AddTransient<IPortalRepositories, PortalRepositories>();

builder.Services.AddDbContext<PortalDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PortalDB")));

var app = builder.Build();

var debugEnabled = app.Configuration.GetValue<bool?>("DebugEnabled") != null &&
                   app.Configuration.GetValue<bool>("DebugEnabled");
if (debugEnabled)
{
    app.UseDeveloperExceptionPage();
    KeycloakUntrustedCertExceptionHandler.ConfigureExceptions(app.Configuration.GetSection("Keycloak"));
}

FlurlErrorHandler.ConfigureErrorHandler(app.Services.GetRequiredService<ILogger<Program>>(), debugEnabled);

if (app.Configuration.GetValue<bool?>("SwaggerEnabled") != null && app.Configuration.GetValue<bool>("SwaggerEnabled"))
{
    app.UseSwagger(c => c.RouteTemplate = "/api/notification/swagger/{documentName}/swagger.{json|yaml}");
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint(string.Format("/api/notification/swagger/{0}/swagger.json", VERSION),
            string.Format("{0} {1}", TAG, VERSION));
        c.RoutePrefix = "api/notification/swagger";
    });
}

app.UseRouting();

app.UseMiddleware<GeneralHttpErrorHandler>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();