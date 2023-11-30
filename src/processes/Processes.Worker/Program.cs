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
using Org.Eclipse.TractusX.Portal.Backend.ApplicationActivation.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ProcessIdentity.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Identities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Config.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Executor;
using Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Executor.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Executor.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ServiceAccountSync.Executor.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Worker.Library;
using Serilog;

LoggingExtensions.EnsureInitialized();
Log.Information("Building worker");
try
{
    var host = Host
        .CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services
                .AddProcessExecutionService(hostContext.Configuration.GetSection("Processes"))
                .AddTransient<IProcessTypeExecutor, ApplicationChecklistProcessTypeExecutor>()
                .AddOfferSubscriptionProcessExecutor(hostContext.Configuration)
                .AddTechnicalUserProfile()
                .AddTransient<IApplicationChecklistHandlerService, ApplicationChecklistHandlerService>()
                .AddPortalRepositories(hostContext.Configuration)
                .AddApplicationChecklist(hostContext.Configuration.GetSection("ApplicationChecklist"))
                .AddApplicationChecklistCreation()
                .AddApplicationActivation(hostContext.Configuration)
                .AddConfigurationIdentityService(hostContext.Configuration.GetSection("ProcessIdentity"))
                .AddNetworkRegistrationProcessExecutor(hostContext.Configuration)
                .AddServiceAccountSyncProcessExecutor(hostContext.Configuration);

            if (hostContext.HostingEnvironment.IsDevelopment())
            {
                var urlsToTrust = hostContext.Configuration.GetSection("Keycloak")?.Get<KeycloakSettingsMap>()?.Values
                    .Where(config => config.ConnectionString.StartsWith("https://"))
                    .Select(config => config.ConnectionString)
                    .Distinct();
                if (urlsToTrust != null)
                {
                    FlurlUntrustedCertExceptionHandler.ConfigureExceptions(urlsToTrust);
                }
            }
        })
        .AddLogging()
        .Build();
    Log.Information("Building worker completed");

    var tokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += (s, e) =>
    {
        Log.Information("Canceling...");
        tokenSource.Cancel();
        e.Cancel = true;
    };

    Log.Information("Start processing");
    var workerInstance = host.Services.GetRequiredService<ProcessExecutionService>();
    await workerInstance.ExecuteAsync(tokenSource.Token).ConfigureAwait(false);
    Log.Information("Execution finished shutting down");
}
catch (Exception ex) when (!ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Server Shutting down");
    Log.CloseAndFlush();
}
