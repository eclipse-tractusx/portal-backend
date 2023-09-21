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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

public partial class ProvisioningSettings
{
    public string CentralRealm { get; set; } = null!;
    public string IdpPrefix { get; set; }
    public string ClientPrefix { get; set; }
    public string MappedCompanyAttribute { get; set; }
    public string MappedBpnAttribute { get; set; }

    public string UserNameMapperTemplate { get; set; }

    public bool UseAuthTrail { get; set; }
}

public static class ProvisioningSettingsExtension
{
    public static IServiceCollection ConfigureProvisioningSettings(
        this IServiceCollection services,
        IConfigurationSection section) =>
        services.Configure<ProvisioningSettings>(x =>
            {
                section.Bind(x);
                if (string.IsNullOrWhiteSpace(x.CentralRealm))
                {
                    throw new ConfigurationException($"{nameof(ProvisioningSettings)}: {nameof(x.CentralRealm)} must not be null or empty");
                }
            });
}
