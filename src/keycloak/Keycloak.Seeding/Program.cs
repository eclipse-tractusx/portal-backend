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
using Microsoft.Extensions.Hosting;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;
using Serilog;

LoggingExtensions.EnsureInitialized();
Log.Information("Building keycloak-seeder");
var isDevelopment = false;
try
{
    var host = Host
        .CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services
                .AddLogging()
                .AddScoped<ISeedDataHandler, SeedDataHandler>()
                .AddTransient<IRealmUpdater, RealmUpdater>()
                .AddTransient<IRolesUpdater, RolesUpdater>()
                .AddTransient<IClientsUpdater, ClientsUpdater>()
                .AddTransient<IIdentityProvidersUpdater, IdentityProvidersUpdater>()
                .AddTransient<IUsersUpdater, UsersUpdater>()
                .AddTransient<IClientScopesUpdater, ClientScopesUpdater>()
                .AddTransient<IAuthenticationFlowsUpdater, AuthenticationFlowsUpdater>()
                .AddTransient<IKeycloakFactory, KeycloakFactory>()
                .ConfigureKeycloakSettingsMap(hostContext.Configuration.GetSection("Keycloak"))
                .AddTransient<IKeycloakSeeder, KeycloakSeeder>()
                .ConfigureKeycloakSeederSettings(hostContext.Configuration.GetSection("KeycloakSeeding"));

            if (hostContext.HostingEnvironment.IsDevelopment())
            {
                var urlsToTrust = hostContext.Configuration.GetSection("Keycloak").Get<KeycloakSettingsMap>().Values
                    .Where(config => config.ConnectionString.StartsWith("https://"))
                    .Select(config => config.ConnectionString)
                    .Distinct();
                FlurlUntrustedCertExceptionHandler.ConfigureExceptions(urlsToTrust);
                isDevelopment = true;
            }
        })
        .UseSerilog()
        .Build();

    FlurlErrorHandler.ConfigureErrorHandler(host.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>(), isDevelopment);

    Log.Information("Building keycloak-seeder completed");

    var tokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += (s, e) =>
    {
        Log.Information("Canceling...");
        tokenSource.Cancel();
        e.Cancel = true;
    };

    using var scope = host.Services.CreateScope();
    Log.Information("Start seeding");
    var seederInstance = scope.ServiceProvider.GetRequiredService<IKeycloakSeeder>();
    await seederInstance.Seed(tokenSource.Token).ConfigureAwait(false);
    Log.Information("Execution finished shutting down");
}
catch (Exception ex) when (!ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
{
    Log.Fatal(ex, "Unhandled exception");
    Environment.ExitCode = 1;
}
finally
{
    Log.Information("Server Shutting down");
    Log.CloseAndFlush();
}
