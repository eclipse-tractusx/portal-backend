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

using Org.CatenaX.Ng.Portal.Backend.Framework.Web;
using Org.CatenaX.Ng.Portal.Backend.Notification.Library;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library;
using Org.CatenaX.Ng.Portal.Backend.Services.Service;
using Org.CatenaX.Ng.Portal.Backend.Services.Service.BusinessLogic;
using Microsoft.Extensions.FileProviders;
using Org.CatenaX.Ng.Portal.Backend.Mailing.SendMail;

var VERSION = "v2";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Kubernetes")
{
    var provider = new PhysicalFileProvider("/app/secrets");
    builder.Configuration.AddJsonFile(provider, "appsettings.json", optional: false, reloadOnChange: false);
}

builder.Services.AddDefaultServices<Program>(builder.Configuration, VERSION)
    .AddMailingAndTemplateManager(builder.Configuration)
    .AddPortalRepositories(builder.Configuration)
    .AddProvisioningManager(builder.Configuration)
    .AddHttpClient();

builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddTransient<IServiceBusinessLogic, ServiceBusinessLogic>()
    .AddTransient<IOfferService, OfferService>()
    .AddTransient<IOfferSetupService, OfferSetupService>()
    .AddTransient<IOfferSubscriptionService, OfferSubscriptionService>()
    .ConfigureServiceSettings(builder.Configuration.GetSection("Services"));

builder.Build()
    .CreateApp<Program>("services", VERSION)
    .Run();
