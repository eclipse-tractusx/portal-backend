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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;

public class KeycloakFactory : IKeycloakFactory
{
    private readonly KeycloakSettingsMap _settings;

    public KeycloakFactory(IOptions<KeycloakSettingsMap> settings)
    {
        _settings = settings.Value;
    }

    public KeycloakClient CreateKeycloakClient(string instance)
    {
        if (!_settings.Keys.Contains(instance, StringComparer.InvariantCultureIgnoreCase))
        {
            throw new ConfigurationException($"undefined keycloak instance '{instance}'");
        }

        var settings = _settings.Single(x => x.Key.Equals(instance, StringComparison.InvariantCultureIgnoreCase)).Value;
        return settings.ClientSecret == null
            ? new KeycloakClient(settings.ConnectionString, settings.User, settings.Password, settings.AuthRealm)
            : KeycloakClient.CreateWithClientId(settings.ConnectionString, settings.ClientId, settings.ClientSecret, settings.AuthRealm);
    }

    public KeycloakClient CreateKeycloakClient(string instance, string clientId, string secret)
    {
        if (!_settings.Keys.Contains(instance, StringComparer.InvariantCultureIgnoreCase))
        {
            throw new ConfigurationException($"undefined keycloak instance '{instance}'");
        }

        var settings = _settings.Single(x => x.Key.Equals(instance, StringComparison.InvariantCultureIgnoreCase)).Value;
        return KeycloakClient.CreateWithClientId(settings.ConnectionString, clientId, secret, settings.AuthRealm);
    }
}
