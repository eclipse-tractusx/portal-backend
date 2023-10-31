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

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Authorization;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Web.Initialization;

public static class WebAppHelper
{
    public static void BuildAndRunWebApplication<TProgram>(string[] args, string path, string version, Action<WebApplicationBuilder> configureBuilder) =>
        WebApplicationBuildRunner
            .BuildAndRunWebApplication<TProgram, KeycloakClaimsTransformation>(args, path, version, configureBuilder,
                (app, environment) =>
                {
                    if (environment.IsDevelopment())
                    {
                        app.UseDeveloperExceptionPage();
                        var urlsToTrust = app.Configuration.GetSection("Keycloak").Get<KeycloakSettingsMap>()?.Values
                            .Where(config => config.ConnectionString.StartsWith("https://"))
                            .Select(config => config.ConnectionString)
                            .Distinct();
                        if (urlsToTrust != null)
                        {
                            FlurlUntrustedCertExceptionHandler.ConfigureExceptions(urlsToTrust);
                        }
                    }

                    FlurlErrorHandler.ConfigureErrorHandler(app.Services.GetRequiredService<ILogger<TProgram>>(), environment.IsDevelopment());
                },
                builder =>
                {
                    builder.AddPolicy(PolicyTypes.CompanyUser, policy =>
                    {
                        policy.Requirements.Add(new MandatoryEnumTypeClaimRequirement(PortalClaimTypes.IdentityType, IdentityTypeId.COMPANY_USER));
                        policy.Requirements.Add(new MandatoryGuidClaimRequirement(PortalClaimTypes.IdentityId));
                    });
                    builder.AddPolicy(PolicyTypes.ServiceAccount, policy =>
                    {
                        policy.Requirements.Add(new MandatoryEnumTypeClaimRequirement(PortalClaimTypes.IdentityType, IdentityTypeId.COMPANY_SERVICE_ACCOUNT));
                        policy.Requirements.Add(new MandatoryGuidClaimRequirement(PortalClaimTypes.IdentityId));
                    });
                });
}
