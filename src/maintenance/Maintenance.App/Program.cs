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
// See https://aka.ms/new-console-template for more information

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ProcessIdentity.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Maintenance.App;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Serilog;

LoggingExtensions.EnsureInitialized();
Log.Information("Building service");
try
{
    var host = Host.CreateDefaultBuilder(args)
        .UseSystemd()
        .ConfigureServices((hostContext, services) =>
        {
            services
                .AddProcessIdentity(hostContext.Configuration.GetSection("ProcessIdentity"))
                .AddDbContext<PortalDbContext>(o =>
                    o.UseNpgsql(hostContext.Configuration.GetConnectionString("PortalDb")));
            services.AddHostedService<BatchDeleteService>();
        })
        .AddLogging()
        .Build();

    host.Run();
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
