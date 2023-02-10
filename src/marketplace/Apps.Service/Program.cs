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

using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

var VERSION = "v2";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultServices<Program>(builder.Configuration, VERSION)
    .AddMailingAndTemplateManager(builder.Configuration)
    .AddPortalRepositories(builder.Configuration)
    .AddProvisioningManager(builder.Configuration);

builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddTransient<IAppsBusinessLogic, AppsBusinessLogic>()
                .AddTransient<IOfferService, OfferService>()
                .AddTransient<IOfferSubscriptionService, OfferSubscriptionService>()
                .ConfigureAppsSettings(builder.Configuration.GetSection("AppMarketPlace"));

builder.Services.AddOfferSetupService();

builder.Services.AddTransient<IAppReleaseBusinessLogic, AppReleaseBusinessLogic>();

builder.Build()
    .CreateApp<Program>("apps", VERSION, builder.Environment)
    .Run();
