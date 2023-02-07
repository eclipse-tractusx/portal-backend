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
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Worker;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Config.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;

try
{
    Console.WriteLine("Building worker");
    var host = Host.CreateDefaultBuilder(args)
     .ConfigureServices((hostContext, services) =>
     {
         services
             .AddTransient<ChecklistExecutionService>()
             .AddTransient<IChecklistProcessor, ChecklistProcessor>()            
             .AddTransient<IChecklistHandlerService, ChecklistHandlerService>()
             .AddPortalRepositories(hostContext.Configuration)
             .AddChecklist(hostContext.Configuration.GetSection("Checklist"))
             .AddChecklistCreation()
             .AddApplicationActivation(hostContext.Configuration);

         if (hostContext.HostingEnvironment.IsDevelopment())
         {
             var urlsToTrust = hostContext.Configuration.GetSection("Keycloak").Get<KeycloakSettingsMap>().Values
                 .Where(config => config.ConnectionString.StartsWith("https://"))
                 .Select(config => config.ConnectionString)
                 .Distinct();
             FlurlUntrustedCertExceptionHandler.ConfigureExceptions(urlsToTrust);    
         }
     }).Build();
    Console.WriteLine("Building worker completed");

    var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (s, e) =>
    {
        Console.WriteLine("Canceling...");
        cts.Cancel();
        e.Cancel = true;
    };

    Console.WriteLine("Start processing Checklist");
    var workerInstance = host.Services.GetRequiredService<ChecklistExecutionService>();
    await workerInstance.ExecuteAsync(cts.Token).ConfigureAwait(false);
    Console.WriteLine("Execution finished shutting down");
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}
