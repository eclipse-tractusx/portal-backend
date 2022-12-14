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
// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Laraue.EfCoreTriggers.PostgreSql.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding.DependencyInjection;

Console.WriteLine("Starting process");
try
{
    var builder = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(cfg =>
        {
            // Read configuration for configuring logger.
            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

            // Build a config object, using env vars and JSON providers.
            if (environmentName == "Kubernetes")
            {
                var provider = new PhysicalFileProvider("/app/secrets");
                cfg.AddJsonFile(provider, "appsettings.json", optional: false, reloadOnChange: true);
            }

            cfg
                .AddEnvironmentVariables()
                .AddUserSecrets(Assembly.GetExecutingAssembly());
        })
        .ConfigureServices((hostContext, services) =>
        {
            services.AddDbContext<PortalDbContext>(o =>
                    o.UseNpgsql(hostContext.Configuration.GetConnectionString("PortalDb"),
                        x => x.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name)
                            .MigrationsHistoryTable("__efmigrations_history_portal"))
                        .UsePostgreSqlTriggers())
                .AddDatabaseInitializer<PortalDbContext>(hostContext.Configuration.GetSection("Seeding"));
        });
    
    var host = builder.Build();

    await host.Services.InitializeDatabasesAsync();
    
    // We don't actually run anything here. The magic happens in InitializeDatabasesAsync
}
catch (Exception ex) when (!ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
{
    // Should be replaced with Serilog as soon as we have it.
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Unhandled exception: {0}", ex);
    Console.ResetColor();
}
finally
{
    // Should be replaced with Serilog as soon as we have it.
    Console.WriteLine("Process Shutting down...");
}
