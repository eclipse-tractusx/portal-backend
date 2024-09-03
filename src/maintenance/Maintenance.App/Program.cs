/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Laraue.EfCoreTriggers.PostgreSql.Extensions;
using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.ProcessIdentity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.Services;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ProcessIdentity;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ProcessIdentity.DependencyInjection;
using Serilog;

LoggingExtensions.EnsureInitialized();
Log.Information("Building service");
try
{
    var host = Host
        .CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services
                .AddMaintenanceService()
                .AddConfigurationProcessIdentityIdDetermination(hostContext.Configuration.GetSection("ProcessIdentity"))
                .AddBatchDelete(hostContext.Configuration.GetSection("BatchDelete"))
                .AddTransient<ITokenService, TokenService>()
                .AddTransient<ICustodianBusinessLogic, CustodianBusinessLogic>()
                .AddTransient<ICustodianService, CustodianService>()
                .AddTransient<IApplicationChecklistService, ApplicationChecklistService>()
                .AddTransient<IProcessIdentityDataDetermination, ProcessIdentityDataDetermination>()
                .AddClearinghouseService(hostContext.Configuration.GetSection("Clearinghouse"))
                .AddDbAuditing()
                .AddPortalRepositories(hostContext.Configuration)
                .AddDbContext<PortalDbContext>(o =>
                    o.UseNpgsql(hostContext.Configuration.GetConnectionString("PortalDb"))
                        .UsePostgreSqlTriggers());
        })
        .AddLogging()
        .Build();
    Log.Information("Building worker completed");

    using var tokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += (s, e) =>
    {
        Log.Information("Canceling...");
        tokenSource.Cancel();
        e.Cancel = true;
    };

    Log.Information("Start processing");
    var workerInstance = host.Services.GetRequiredService<MaintenanceService>();
    await workerInstance.ExecuteAsync(tokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.None);
    Log.Information("Execution finished shutting down");
}
catch (Exception ex) when (!ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Server Shutting down");
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}
