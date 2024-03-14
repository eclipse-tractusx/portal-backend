/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Config.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Web.Initialization;
using Org.Eclipse.TractusX.Portal.Backend.Web.PublicInfos.DependencyInjection;

var VERSION = "v2";

WebAppHelper
    .BuildAndRunWebApplication<Program>(args, "administration", VERSION, builder =>
    {
        builder.Services
            .AddPublicInfos();

        builder.Services
            .AddPortalRepositories(builder.Configuration)
            .AddProvisioningManager(builder.Configuration);

        builder.Services.AddTransient<IUserProvisioningService, UserProvisioningService>();

        builder.Services.AddTransient<IInvitationBusinessLogic, InvitationBusinessLogic>();

        builder.Services.AddTransient<IUserBusinessLogic, UserBusinessLogic>()
            .AddTransient<IUserUploadBusinessLogic, UserUploadBusinessLogic>()
            .AddTransient<IUserRolesBusinessLogic, UserRolesBusinessLogic>()
            .ConfigureUserSettings(builder.Configuration.GetSection("UserManagement"));

        builder.Services.AddTransient<IRegistrationBusinessLogic, RegistrationBusinessLogic>()
            .ConfigureRegistrationSettings(builder.Configuration.GetSection("Registration"));

        builder.Services.AddTransient<IServiceAccountBusinessLogic, ServiceAccountBusinessLogic>()
            .ConfigureServiceAccountSettings(builder.Configuration.GetSection("ServiceAccount"));

        builder.Services.AddTransient<IDocumentsBusinessLogic, DocumentsBusinessLogic>()
            .ConfigureDocumentSettings(builder.Configuration.GetSection("Document"));
        builder.Services.AddTransient<IStaticDataBusinessLogic, StaticDataBusinessLogic>();
        builder.Services.AddTransient<IPartnerNetworkBusinessLogic, PartnerNetworkBusinessLogic>();
        builder.Services.AddTransient<INotificationService, NotificationService>();
        builder.Services.AddCompanyDataService(builder.Configuration.GetSection("CompanyData"));

        builder.Services.AddTransient<IIdentityProviderBusinessLogic, IdentityProviderBusinessLogic>()
            .ConfigureIdentityProviderSettings(builder.Configuration.GetSection("IdentityProviderAdmin"));

        builder.Services.AddApplicationChecklist(builder.Configuration.GetSection("ApplicationChecklist"))
                        .AddOfferSubscriptionProcess();

        builder.Services.AddTransient<IConnectorsBusinessLogic, ConnectorsBusinessLogic>()
            .ConfigureConnectorsSettings(builder.Configuration.GetSection("Connectors"));

        builder.Services.AddMailingProcessCreation(builder.Configuration.GetSection("MailingProcessCreation"));

        builder.Services
            .AddTransient<ISubscriptionConfigurationBusinessLogic, SubscriptionConfigurationBusinessLogic>()
            .AddPartnerRegistration(builder.Configuration)
            .AddNetworkRegistrationProcessHelper();

        builder.Services
            .AddSingleton<IErrorMessageService, ErrorMessageService>()
            .AddSingleton<IErrorMessageContainer, AdministrationConnectorErrorMessageContainer>()
            .AddSingleton<IErrorMessageContainer, AdministrationRegistrationErrorMessageContainer>()
            .AddSingleton<IErrorMessageContainer, AdministrationServiceAccountErrorMessageContainer>()
            .AddSingleton<IErrorMessageContainer, ProvisioningServiceErrorMessageContainer>();

        builder.Services.AddProvisioningDBAccess(builder.Configuration);
    });
