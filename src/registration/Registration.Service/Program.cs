/********************************************************************************
 * Copyright (c) 2022 Microsoft and BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Config;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Web.Initialization;
using Org.Eclipse.TractusX.Portal.Backend.Web.PublicInfos.DependencyInjection;

var version = AssemblyExtension.GetApplicationVersion();

await WebAppHelper
    .BuildAndRunWebApplicationAsync<Program>(args, "registration", version, builder =>
    {
        builder.Services
            .AddPublicInfos();

        builder.Services
            .AddPortalRepositories(builder.Configuration)
            .AddProvisioningManager(builder.Configuration);

        builder.Services.AddTransient<IUserProvisioningService, UserProvisioningService>();
        builder.Services.AddTransient<IStaticDataBusinessLogic, StaticDataBusinessLogic>();
        builder.Services.AddTransient<IRegistrationBusinessLogic, RegistrationBusinessLogic>()
            .AddTransient<IIdentityProviderProvisioningService, IdentityProviderProvisioningService>()
            .ConfigureRegistrationSettings(builder.Configuration.GetSection("Registration"))
            .AddTransient<INetworkBusinessLogic, NetworkBusinessLogic>();

        builder.Services.AddApplicationChecklistCreation(builder.Configuration.GetSection("ApplicationCreation"));
        builder.Services.AddBpnAccess(builder.Configuration.GetSection("BpnAccess"));
        builder.Services.AddMailingProcessCreation(builder.Configuration.GetSection("MailingProcessCreation"));
    }).ConfigureAwait(ConfigureAwaitOptions.None);
