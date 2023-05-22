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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;

public class KeycloakSettingData
{
    public KeycloakSettingData()
    {
        ConnectionString = null!;
    }

    public string ConnectionString { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? AuthRealm { get; set; }

    public void Validate()
    {
        if (ConnectionString == null)
        {
            throw new ConfigurationException($"{nameof(KeycloakSettingData)}: {nameof(ConnectionString)} must not be null");
        }

        if ((User != null && Password != null) ||
            (ClientId != null && ClientSecret != null))
        {
            return;
        }

        new ConfigurationValidation<KeycloakSettingData>()
            .NotNullOrWhiteSpace(User, () => nameof(User))
            .NotNullOrWhiteSpace(Password, () => nameof(Password))
            .NotNullOrWhiteSpace(ClientId, () => nameof(ClientId))
            .NotNullOrWhiteSpace(ClientSecret, () => nameof(ClientSecret));
    }
}

public class KeycloakSettingsMap
{
    public KeycloakSettingsMap()
    {
        Name = null!;
        Settings = null!;
    }

    public string Name { get; set; }

    public KeycloakSettingData Settings { get; set; }

    public void Validate()
    {
        Settings.Validate();
    }
}

public class KeycloakSettings
{
    public IEnumerable<KeycloakSettingsMap> SettingMap { get; set; } = null!;

    public bool Validate()
    {
        if (!SettingMap.Any())
        {
            throw new ConfigurationException();
        }

        foreach (var settings in SettingMap)
        {
            settings.Validate();
        }

        return true;
    }
}

public static class KeycloakSettingsExtention
{
    public static IServiceCollection ConfigureKeycloakSettingsMap(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<KeycloakSettings>()
            .Bind(section)
            .Validate(x => x.Validate())
            .ValidateOnStart();
        return services;
    }
}
