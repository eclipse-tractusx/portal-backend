/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Logging;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Bpn;

public static class BpnAccessCollectionExtension
{
    public static IServiceCollection AddBpnAccess(this IServiceCollection services, string baseAddress)
    {
        services.AddTransient<LoggingHandler<BpnAccess>>();
        services.AddHttpClient(nameof(BpnAccess), c =>
            {
                c.BaseAddress = new Uri(baseAddress);
            })
            .AddHttpMessageHandler<LoggingHandler<BpnAccess>>();
        services.AddTransient<IBpnAccess, BpnAccess>();

        return services;
    }
}
