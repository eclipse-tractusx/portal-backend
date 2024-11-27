using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Tests.Extensions;

using System.Collections.Generic;
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
            SeederConfigurations = new List<SeederConfiguration>
            {
                new()
                {
                    Key = "FederatedIdentities",
                    Create = false,
                    Update = false,
                    Delete = false
                },
                new()
                {
                    Key = "Users",
                    Create = false,
                    Update = true,
                    Delete = true,
                    SeederConfigurations = new List<SeederConfiguration>
                    {
                        new()
                        {
                            Key = "testUser",
                            Create = false,
                            Update = true,
                            Delete = false,
                            SeederConfigurations = new List<SeederConfiguration>
                            {
                                new()
                                {
                                    Key = "FederatedIdentities",
                                    Create = true,
                                    Update = true,
                                    Delete = false
                                }
                            }
                        }
                    }
                },
                new()
                {
                    Key = "Clients",
                    Create = false,
                    Update = false,
                    Delete = false
                }
            }
        };

        // Act
        var result = realmSettings.GetFlatDictionary();

        // Assert
        result.Should().HaveCount(4).And.Satisfy(
            x => x.Key == "federatedidentities" && x.Value.Create == true && x.Value.Update == true && x.Value.Delete == false,
            x => x.Key == "testuser" && x.Value.Create == true && x.Value.Update == true && x.Value.Delete == false,
            x => x.Key == "users" && x.Value.Create == true && x.Value.Update == true && x.Value.Delete == true,
            x => x.Key == "clients" && x.Value.Create == false && x.Value.Update == false && x.Value.Delete == false
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
            SeederConfigurations = new List<SeederConfiguration>
            {
                new()
                {
                    Key = "Users",
                    Create = true,
                    Update = false,
                    Delete = true,
                    SeederConfigurations = new List<SeederConfiguration>
                    {
                        new()
                        {
                            Key = "testUser",
                            Create = false,
                            Update = true,
                            Delete = false
                        }
                    }
                }
            }
        };

        // Act
        var result = realmSettings.GetConfigurationDictionaries();

        // Assert
        result.Create.Should().BeTrue();
        result.Update.Should().BeFalse();
        result.Delete.Should().BeTrue();
        result.SeederConfigurations.Should().ContainSingle().And.Satisfy(
            x => x.Key == "users" && x.Value.Create == true && x.Value.Update == false && x.Value.Delete == true &&
                 x.Value.SeederConfigurations.Count == 1 && x.Value.SeederConfigurations.ContainsKey("testuser") == true &&
                 x.Value.SeederConfigurations.Single().Value.Create == false &&
                 x.Value.SeederConfigurations.Single().Value.Update == true &&
                 x.Value.SeederConfigurations.Single().Value.Delete == false);
    }
}
