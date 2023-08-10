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
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Logging;

public static class LoggingExtensions
{
    /// <summary>
    /// Adds the default Serilog Configuration to the log
    /// </summary>
    /// <param name="host">The applications host</param>
    /// <param name="extendLogging">The possibility to extend the configuration</param>
    public static IHostBuilder AddLogging(this IHostBuilder host, Action<LoggerConfiguration, IConfiguration>? extendLogging = null) =>
        host.UseSerilog((context, configuration) =>
        {
            configuration
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .ReadFrom.Configuration(context.Configuration)                
                .WriteTo.Console(new JsonFormatter(renderMessage: true));
            extendLogging?.Invoke(configuration, context.Configuration);
        });

    /// <summary>
    /// Creates a static logger
    /// </summary>
    /// <remarks>
    /// This should only be used for logging in the Program.cs
    /// For all other logging use ILogger
    /// </remarks>
    public static void EnsureInitialized()
    {
        if (Log.Logger is Logger)
            return;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(new JsonFormatter(renderMessage: true))
            .CreateBootstrapLogger();
    }
}
