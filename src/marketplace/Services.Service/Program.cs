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

using Asp.Versioning;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Web.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.DependencyInjection;

var VERSION = "v2";

WebApplicationBuildRunner
    .BuildAndRunWebApplication<Program>(args, "services", builder =>
    {
        builder.Services
            .AddMailingAndTemplateManager(builder.Configuration)
            .AddPortalRepositories(builder.Configuration)
            .AddProvisioningManager(builder.Configuration);

        builder.Services.AddTransient<INotificationService, NotificationService>();
        builder.Services
            .AddServiceBusinessLogic(builder.Configuration)
            .AddTransient<IServiceReleaseBusinessLogic, ServiceReleaseBusinessLogic>()
            .AddTransient<IServiceChangeBusinessLogic, ServiceChangeBusinessLogic>()
            .AddTechnicalUserProfile()
            .AddOfferDocumentServices();

        builder.Services
            .AddOfferServices()
            .AddProvisioningDBAccess(builder.Configuration);
    }, new ApiVersion(1, 0));
