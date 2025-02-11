/********************************************************************************
 * Copyright (c) 2022 BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Web.Initialization;
using Org.Eclipse.TractusX.Portal.Backend.Web.PublicInfos.DependencyInjection;

var version = AssemblyExtension.GetApplicationVersion();

await WebAppHelper
    .BuildAndRunWebApplicationAsync<Program>(args, "notification", version, builder =>
    {
        builder.Services
            .AddPublicInfos();

        builder.Services
            .AddPortalRepositories(builder.Configuration);

        builder.Services
            .AddSingleton<IErrorMessageService, ErrorMessageService>()
            .AddTransient<INotificationBusinessLogic, NotificationBusinessLogic>()
            .ConfigureNotificationSettings(builder.Configuration.GetSection("Notifications"));
    }).ConfigureAwait(ConfigureAwaitOptions.None);
