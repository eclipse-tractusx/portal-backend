/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Tests.Extensions;

using Xunit;

public class KeycloakRealmSettingsTests
{
    [Fact]
    public void GetFlatDictionary_WithInDepthConfiguration_DeeperOneIsTaken()
    {
        // Arrange
        var realmSettings = new KeycloakRealmSettings
        {
            Create = false,
            Update = false,
            Delete = false,
            SeederConfigurations =
            [
                new()
                {
                    Key = "Roles",
                    Create = true,
                    Update = false,
                    Delete = false
                },
                new()
                {
                    Key = "Localizations",
                    Create = false,
                    Update = true,
                    Delete = false
                },
                new()
                {
                    Key = "UserProfile",
                    Create = false,
                    Update = false,
                    Delete = true
                },
                new()
                {
                    Key = "FederatedIdentities",
                    Create = false,
                    Update = false,
                    Delete = false
                },
                new()
                {
                    Key = "FEDERATEDIdentities",
                    Create = true,
                    Update = true,
                    Delete = true
                },
                new()
                {
                    Key = "Users",
                    Create = false,
                    Update = false,
                    Delete = false,
                    SeederConfigurations =
                    [
                        new()
                        {
                            Key = "testUser1",
                            Create = false,
                            Update = false,
                            Delete = false
                        },
                        new()
                        {
                            Key = "testUser2",
                            Create = false,
                            Update = false,
                            Delete = false,
                            SeederConfigurations =
                            [
                                new()
                                {
                                    Key = "FederatedIdentities",
                                    Create = true,
                                    Update = true,
                                    Delete = false
                                }
                            ]
                        }
                    ]
                },
                new()
                {
                    Key = "Clients",
                    Create = false,
                    Update = false,
                    Delete = false,
                    SeederConfigurations =
                    [
                        new()
                        {
                            Key = "testClient",
                            Create = false,
                            Update = false,
                            Delete = false
                        }
                    ]
                }
            ]
        };

        // Act
        var result = realmSettings.GetFlatDictionary();

        // Assert
        result.Should().HaveCount(6).And.Satisfy(
            x => x.Key == ConfigurationKey.Roles && x.Value,
            x => x.Key == ConfigurationKey.Localizations && x.Value,
            x => x.Key == ConfigurationKey.UserProfile && x.Value,
            x => x.Key == ConfigurationKey.FederatedIdentities && !x.Value,
            x => x.Key == ConfigurationKey.Users && x.Value,
            x => x.Key == ConfigurationKey.Clients && !x.Value
        );
    }

    [Fact]
    public void GetConfigurationDictionaries_WithNestedConfigurations_ReturnsExpected()
    {
        // Arrange
        var realmSettings = new KeycloakRealmSettings
        {
            Create = true,
            Update = false,
            Delete = true,
            SeederConfigurations =
            [
                new()
                {
                    Key = "Users",
                    Create = true,
                    Update = false,
                    Delete = true,
                    SeederConfigurations =
                    [
                        new()
                        {
                            Key = "testUser",
                            Create = false,
                            Update = true,
                            Delete = false
                        }
                    ]
                }
            ]
        };

        // Act
        var result = realmSettings.GetConfigurationDictionaries();

        // Assert
        result.Create.Should().BeTrue();
        result.Update.Should().BeFalse();
        result.Delete.Should().BeTrue();
        result.SeederConfigurations.Should().ContainSingle().And.Satisfy(
            x => x.Key == "Users" && x.Value.Create && !x.Value.Update && x.Value.Delete &&
                 x.Value.SeederConfigurations != null && x.Value.SeederConfigurations.Count == 1 && x.Value.SeederConfigurations.ContainsKey("testUser") &&
                 !x.Value.SeederConfigurations.Single().Value.Create &&
                 x.Value.SeederConfigurations.Single().Value.Update &&
                 !x.Value.SeederConfigurations.Single().Value.Delete);
    }
}
