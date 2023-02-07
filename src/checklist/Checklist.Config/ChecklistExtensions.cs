﻿/********************************************************************************
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Config.DependencyInjection;

public static class ChecklistExtensions
{
    public static IServiceCollection AddChecklist(this IServiceCollection services, IConfigurationSection section)
    {
        return services
            .AddTransient<ITokenService, TokenService>()
            .AddTransient<IChecklistService, ChecklistService>()
            .AddBpdmService(section.GetSection("Bpdm"))
            .AddCustodianService(section.GetSection("Custodian"))
            .AddClearinghouseService(section.GetSection("Clearinghouse"))
            .AddSdFactoryService(section.GetSection("SdFactory"));
    }

    public static IServiceCollection AddChecklistCreation(this IServiceCollection services)
    {
        return services
            .AddScoped<IChecklistCreationService, ChecklistCreationService>();
    }
}
