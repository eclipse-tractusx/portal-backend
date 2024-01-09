/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;
using System.Diagnostics.CodeAnalysis;
using IdentityProvider = Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders.IdentityProvider;

namespace Org.Eclipse.TractusX.Portal.Backend.ExternalSystems.Provisioning.Library.DependencyInjection;

[ExcludeFromCodeCoverage]
public static class ProvisioningLibraryExtensions
{
    public static IServiceCollection AddIdpManagement(this IServiceCollection services, IConfiguration configuration) =>
        services
            .ConfigureIdpCreationSettings(configuration.GetSection("Provisioning"))
            .AddTransient<IKeycloakFactory, KeycloakFactory>()
            .ConfigureKeycloakSettingsMap(configuration.GetSection("Keycloak"))
            .AddScoped<IIdpManagement, IdpManagement>();
}

public class IdpCreationSettings
{
    public string CentralRealm { get; init; } = null!;
    public string IdpPrefix { get; init; } = null!;
    public IdentityProvider CentralIdentityProvider { get; init; } = null!;
    public Realm SharedRealm { get; init; } = null!;
    public Client ServiceAccountClient { get; init; } = null!;
    public Client SharedRealmClient { get; init; } = null!;
    public string MappedCompanyAttribute { get; init; } = null!;
    public string ServiceAccountClientPrefix { get; init; } = null!;

    public IdpCreationSettings ValidateIdpCreationSettings()
    {
        new ConfigurationValidation<IdpCreationSettings>()
            .NotNullOrWhiteSpace(CentralRealm, () => nameof(CentralRealm))
            .NotNullOrWhiteSpace(IdpPrefix, () => nameof(IdpPrefix))
            .NotNull(CentralIdentityProvider, () => nameof(CentralIdentityProvider))
            .NotNull(SharedRealm, () => nameof(SharedRealm))
            .NotNull(ServiceAccountClient, () => nameof(ServiceAccountClient))
            .NotNull(SharedRealmClient, () => nameof(SharedRealmClient))
            .NotNullOrWhiteSpace(MappedCompanyAttribute, () => nameof(MappedCompanyAttribute))
            .NotNullOrWhiteSpace(ServiceAccountClientPrefix, () => nameof(ServiceAccountClientPrefix));
        return this;
    }
}

public static class IdpCreationSettingsExtensions
{
    public static IServiceCollection ConfigureIdpCreationSettings(
        this IServiceCollection services,
        IConfigurationSection section) =>
        services.Configure<IdpCreationSettings>(x =>
        {
            section.Bind(x);
            x.ValidateIdpCreationSettings();
        });
}
