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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Logic;

public class KeycloakSeeder
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, IncludeFields = true, PropertyNameCaseInsensitive = false };
    private readonly KeycloakSeederSettings _settings;
    private readonly IKeycloakFactory _factory;
    public KeycloakSeeder(IKeycloakFactory keycloakFactory, IOptions<KeycloakSeederSettings> options)
    {
        _settings = options.Value;
        _factory = keycloakFactory;
    }

    public async Task Seed()
    {
        KeycloakRealm jsonRealm;
        using (var stream = File.OpenRead(_settings.DataPath))
        {
            jsonRealm = await JsonSerializer.DeserializeAsync<KeycloakRealm>(stream, Options).ConfigureAwait(false) ?? throw new ConfigurationException($"cannot deserialize realm from {_settings.DataPath}");
        }

        if (jsonRealm.Realm == null)
            throw new ConflictException("realm must not be null");

        var keycloak = _factory.CreateKeycloakClient("master");
        Library.Models.RealmsAdmin.Realm realm;
        try
        {
            realm = await keycloak.GetRealmAsync(jsonRealm.Realm).ConfigureAwait(false);
        }
        catch (KeycloakEntityNotFoundException)
        {
            realm = new Library.Models.RealmsAdmin.Realm {
                Id = jsonRealm.Id,
                _Realm = jsonRealm.Realm
            };
            await keycloak.ImportRealmAsync(jsonRealm.Realm, realm).ConfigureAwait(false);
        }
        var updater = new RealmUpdater(keycloak, realm, jsonRealm);
        await updater.Update().ConfigureAwait(false);
    }
}
