﻿/********************************************************************************
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

using CatenaX.NetworkServices.Framework.Cors;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Keycloak.ErrorHandling;
using CatenaX.NetworkServices.Keycloak.Factory;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CatenaX.NetworkServices.Framework.Web;

public static class StartupServiceWebApplicationExtensions
{
    
    public static WebApplication CreateApp<TProgram>(this WebApplication app, string apiPath, string version)
    {
        var debugEnabled = app.Configuration.GetValue<bool?>("DebugEnabled") != null &&
                           app.Configuration.GetValue<bool>("DebugEnabled");
        if (debugEnabled)
        {
            app.UseDeveloperExceptionPage();
            var urlsToTrust = app.Configuration.GetSection("Keycloak").Get<KeycloakSettingsMap>().Values
                .Select(config => new Uri(config.ConnectionString))
                .Where(uri => uri.Scheme == "https")
                .Select(uri => uri.Scheme + "://" + uri.Host)
                .Distinct();
            FlurlUntrustedCertExceptionHandler.ConfigureExceptions(urlsToTrust);
        }

        var assemblyName = typeof(TProgram).Assembly.FullName?.Split(',')[0];

        FlurlErrorHandler.ConfigureErrorHandler(app.Services.GetRequiredService<ILogger<TProgram>>(),
            debugEnabled);

        if (app.Configuration.GetValue<bool?>("SwaggerEnabled") != null &&
            app.Configuration.GetValue<bool>("SwaggerEnabled"))
        {
            app.UseSwagger(c =>
                c.RouteTemplate = $"/api/{apiPath}/swagger/{{documentName}}/swagger.{{json|yaml}}");
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/api/{apiPath}/swagger/{version}/swagger.json",
                    $"{assemblyName} {version}");
                c.RoutePrefix = $"api/{apiPath}/swagger";
            });
        }

        app.UseRouting();

        app.UseCors(CorsExtensions.AllowSpecificOrigins);

        app.UseMiddleware<GeneralHttpErrorHandler>();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
