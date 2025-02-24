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

using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Web.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Web.Initialization;
using Org.Eclipse.TractusX.Portal.Backend.Web.PublicInfos.DependencyInjection;

var version = AssemblyExtension.GetApplicationVersion();

await WebAppHelper
    .BuildAndRunWebApplicationAsync<Program>(args, "apps", version, builder =>
    {
        builder.Services
            .AddPublicInfos();

        builder.Services
            .AddPortalRepositories(builder.Configuration)
            .AddProvisioningManager(builder.Configuration);

        builder.Services.AddTransient<INotificationService, NotificationService>();
        builder.Services.AddTransient<IAppsBusinessLogic, AppsBusinessLogic>()
            .AddTransient<IAppReleaseBusinessLogic, AppReleaseBusinessLogic>()
            .AddTransient<IAppChangeBusinessLogic, AppChangeBusinessLogic>()
            .AddTransient<IOfferService, OfferService>()
            .AddTransient<IOfferSubscriptionService, OfferSubscriptionService>()
            .AddTransient<IOfferSetupService, OfferSetupService>()
            .AddTransient<ITechnicalUserProfileService, TechnicalUserProfileService>()
            .AddSingleton<IErrorMessageService, ErrorMessageService>()
            .AddSingleton<IErrorMessageContainer, AppChangeErrorMessageContainer>()
            .AddSingleton<IErrorMessageContainer, AppExtensionErrorMessageContainer>()
            .AddSingleton<IErrorMessageContainer, AppReleaseErrorMessageContainer>()
            .AddSingleton<IErrorMessageContainer, AppErrorMessageContainer>()
            .AddSingleton<IErrorMessageContainer, ValidationExpressionErrorMessageContainer>()
            .AddTechnicalUserProfile()
            .ConfigureAppsSettings(builder.Configuration.GetSection("AppMarketPlace"))
            .AddOfferDocumentServices();

        builder.Services
            .AddDimService(builder.Configuration.GetSection("Dim"))
            .AddOfferServices()
            .AddProvisioningDBAccess(builder.Configuration);

        builder.Services.AddMailingProcessCreation(builder.Configuration.GetSection("MailingProcessCreation"));
    }).ConfigureAwait(ConfigureAwaitOptions.None);
