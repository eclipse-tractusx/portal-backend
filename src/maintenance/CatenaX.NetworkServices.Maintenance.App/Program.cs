/********************************************************************************
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
// See https://aka.ms/new-console-template for more information

using System.Reflection;
using CatenaX.NetworkServices.Maintenance.App;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
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

        cfg.AddUserSecrets(Assembly.GetExecutingAssembly());
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDbContext<PortalDbContext>(o =>
            o.UseNpgsql(hostContext.Configuration.GetConnectionString("PortalDb")));
        services.AddHostedService<BatchDeleteService>();
    }).Build();

host.Run();
