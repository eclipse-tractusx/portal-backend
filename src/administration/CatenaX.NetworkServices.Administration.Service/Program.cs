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

using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Custodian;
using CatenaX.NetworkServices.Framework.Web;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.Notification.Library;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.Provisioning.DBAccess;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Service;
using CatenaX.NetworkServices.Provisioning.ProvisioningEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

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
                .AddProvisioningManager(builder.Configuration);

builder.Services.AddTransient<IUserProvisioningService, UserProvisioningService>();

builder.Services.AddTransient<IInvitationBusinessLogic, InvitationBusinessLogic>()
                .ConfigureInvitationSettings(builder.Configuration.GetSection("Invitation"));

builder.Services.AddTransient<IUserBusinessLogic, UserBusinessLogic>()
                .ConfigureUserSettings(builder.Configuration.GetSection("UserManagement"));

builder.Services.AddTransient<IRegistrationBusinessLogic, RegistrationBusinessLogic>()
                .ConfigureRegistrationSettings(builder.Configuration.GetSection("Registration"));

builder.Services.AddTransient<IServiceAccountBusinessLogic, ServiceAccountBusinessLogic>()
                .ConfigureServiceAccountSettings(builder.Configuration.GetSection("ServiceAccount"));
                
builder.Services.AddTransient<IDocumentsBusinessLogic, DocumentsBusinessLogic>();
builder.Services.AddTransient<IStaticDataBusinessLogic, StaticDataBusinessLogic>();
builder.Services.AddTransient<IPartnerNetworkBusinessLogic, PartnerNetworkBusinessLogic>();
builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddTransient<ICompanyDataBusinessLogic, CompanyDataBusinessLogic>();

builder.Services.AddTransient<IIdentityProviderBusinessLogic, IdentityProviderBusinessLogic>()
                .ConfigureIdentityProviderSettings(builder.Configuration.GetSection("IdentityProviderAdmin"));

builder.Services.AddTransient<IProvisioningDBAccess, ProvisioningDBAccess>();

builder.Services.AddCustodianService(builder.Configuration.GetSection("Custodian"));

builder.Services.AddTransient<IConnectorsSdFactoryService, ConnectorsSdFactoryService>()
                .AddTransient<IConnectorsBusinessLogic, ConnectorsBusinessLogic>()
                .ConfigureConnectorsSettings(builder.Configuration.GetSection("Connectors"));

builder.Services.AddTransient<IServiceProviderBusinessLogic, ServiceProviderBusinessLogic>();

builder.Services.AddDbContext<ProvisioningDBContext>(options =>
                    options.UseNpgsql(builder.Configuration.GetConnectionString("ProvisioningDB")));

builder.Build()
    .CreateApp<Program>("administration", VERSION)
    .Run();
