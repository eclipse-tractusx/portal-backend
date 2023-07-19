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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Tests;

public class KeycloakRealmModelTests
{
    [Fact]
    public async Task Foo()
    {
        // Arrange
        var keycloakSettingsMap = new KeycloakSettingsMap {
            { "central", new() {
                ConnectionString = "https://wsl:8443/iamcentral",
                User = "admin",
                Password = "admin",
                AuthRealm = "master"
            }}
        };
        var seederSettings = new KeycloakSeederSettings
        {
            DataPath = "TestSeeds/CX-Central-realm.json",
            KeycloakInstanceName = "central"
        };

        var logger = A.Fake<ILogger>();
        FlurlUntrustedCertExceptionHandler.ConfigureExceptions(new[] { "https://wsl:8443/iamcentral" });
        FlurlErrorHandler.ConfigureErrorHandler(logger, false);

        var factory = new KeycloakFactory(Options.Create(keycloakSettingsMap));
        var seedDataHandler = new SeedDataHandler();

        var sut = new KeycloakSeeder(
            seedDataHandler,
            new RealmUpdater(factory, seedDataHandler),
            new RolesUpdater(factory, seedDataHandler),
            new ClientsUpdater(factory, seedDataHandler),
            new IdentityProvidersUpdater(factory, seedDataHandler),
            new UsersUpdater(factory, seedDataHandler),
            new AuthenticationFlowsUpdater(factory, seedDataHandler),
            Options.Create(seederSettings));

        // Act
        await sut.Seed().ConfigureAwait(false);
    }
}
