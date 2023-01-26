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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Org.Eclipse.TractusX.Portal.Backend.ApplicationActivation.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Worker;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using System.Reflection;

var host = Host.CreateDefaultBuilder(args)
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
         .AddUserSecrets(Assembly.GetExecutingAssembly())
         .AddEnvironmentVariables();
      })
   .ConfigureServices((hostContext, services) =>
      services
         .AddTransient<ChecklistExecutionService>()
         .AddPortalRepositories(hostContext.Configuration)
         .AddChecklist(hostContext.Configuration.GetSection("Checklist"))
         .AddChecklistCreation()
         .AddApplicationActivation(hostContext.Configuration)).Build();

   var cts = new CancellationTokenSource();
   Console.CancelKeyPress += (s, e) =>
   {
      Console.WriteLine("Canceling...");
      cts.Cancel();
      e.Cancel = true;
   };

var workerInstance = host.Services.GetRequiredService<ChecklistExecutionService>();
await workerInstance.ExecuteAsync(cts.Token).ConfigureAwait(false);
