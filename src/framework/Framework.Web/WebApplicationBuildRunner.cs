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

using Asp.Versioning;
using Flurl.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Logging;
using Serilog;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Web;

public static class WebApplicationBuildRunner
{
    public static void BuildAndRunWebApplication<TProgram>(
        string[] args,
        string path,
        Action<WebApplicationBuilder> configureBuilder,
        ApiVersion defaultApiVersion)
    {
        LoggingExtensions.EnsureInitialized();
        Log.Information("Starting the application");
        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddLogging((configuration, config) =>
            {
                configuration.Enrich.WithCorrelationIdHeader("X-Request-Id");
                var healthCheckPaths = config.GetSection("HealthChecks").Get<IEnumerable<HealthCheckSettings>>()?.Select(x => x.Path);
                if (healthCheckPaths != null)
                {
                    configuration
                        .Filter.ByExcluding(le =>
                        {
                            return le.Properties.TryGetValue("RequestPath", out var logProperty) &&
                                   logProperty.ToKeyValuePairs().Any(x => healthCheckPaths.Contains(x.Value));
                        });
                }
            });
            builder.Services
                .AddDefaultServices<TProgram>(builder.Configuration, defaultApiVersion);

            configureBuilder(builder);

            builder.Build().CreateApp<TProgram>(path, builder.Environment).Run();
        }
        catch (Exception ex) when (!ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
        {
            Log.Fatal("Unhandled exception {Exception}", ex);
        }
        finally
        {
            Log.Information("Server Shutting down");
            Log.CloseAndFlush();
        }
    }
}
