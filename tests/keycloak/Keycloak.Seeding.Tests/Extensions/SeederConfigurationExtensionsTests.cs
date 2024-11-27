using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Tests.Extensions;

public class SeederConfigurationExtensionsTests
{
    [Fact]
    public void ModifyAllowed_InvalidModificationType_ThrowsException()
    {
        var defaultConfig = new SeederConfigurationModel(false, false, false, new Dictionary<string, SeederConfigurationModel>());
        var config = new KeycloakSeederConfigModel(defaultConfig, null);

        Assert.Throws<ArgumentOutOfRangeException>(() => config.ModificationAllowed((ModificationType)666));
    }

    [Theory]
    [InlineData(true, false, false, ModificationType.Create, true)]
    [InlineData(false, false, false, ModificationType.Create, false)]
    [InlineData(false, true, false, ModificationType.Update, true)]
    [InlineData(false, false, false, ModificationType.Update, false)]
    [InlineData(false, false, true, ModificationType.Delete, true)]
    [InlineData(false, false, false, ModificationType.Delete, false)]
    public void ModifyAllowed_WithExpected_ReturnsExpected(bool create, bool update, bool delete, ModificationType modificationType, bool expectedResult)
    {
        var defaultConfig = new SeederConfigurationModel(create, update, delete, new Dictionary<string, SeederConfigurationModel>());
        var config = new KeycloakSeederConfigModel(defaultConfig, null);

        var result = config.ModificationAllowed(modificationType);

        result.Should().Be(expectedResult);
    }

    [Fact]
    public void ModificationAllowed_DefaultConfigAllows_ReturnsTrue()
    {
        var settings = new KeycloakRealmSettings
        {
            Create = true,
            Update = false,
            Delete = false
        };
        var defaultSettings = settings.GetConfigurationDictionaries();
        var config = new KeycloakSeederConfigModel(defaultSettings, null);

        var result = config.ModificationAllowed(ModificationType.Create);

        result.Should().BeTrue();
    }

    [Fact]
    public void ModificationAllowed_DefaultConfigDeniesCreate_ReturnsFalse()
    {
        var settings = new KeycloakRealmSettings
        {
            Create = false,
            Update = false,
            Delete = false
        };
        var defaultSettings = settings.GetConfigurationDictionaries();
        var config = new KeycloakSeederConfigModel(defaultSettings, null);

        var result = config.ModificationAllowed(ModificationType.Create);

        result.Should().BeFalse();
    }

    [Fact]
    public void ModificationAllowed_WithSpecificConfigurationOverwrites_ReturnsSpecificConfiguration()
    {
        var settings = new KeycloakRealmSettings
        {
            Create = false,
            Update = false,
            Delete = false,
            SeederConfigurations = new SeederConfiguration[]
            {
                new()
                {
                    Key = ConfigurationKey.Users.ToString(),
                    Create = true,
                    Update = false,
                    Delete = false
                }
            }
        };
        var defaultSettings = settings.GetConfigurationDictionaries();
        var config = new KeycloakSeederConfigModel(defaultSettings, defaultSettings.SeederConfigurations["users"]);

        var result = config.ModificationAllowed(ModificationType.Create);

        result.Should().BeTrue();
    }

    [Fact]
    public void ModificationAllowed_WithEntityKeySpecificConfig_ReturnsEntitySpecificKey()
    {
        var settings = new KeycloakRealmSettings
        {
            Create = false,
            Update = false,
            Delete = false,
            SeederConfigurations = new SeederConfiguration[]
            {
                new()
                {
                    Key = ConfigurationKey.Users.ToString(),
                    Create = false,
                    Update = false,
                    Delete = false,
                    SeederConfigurations = new SeederConfiguration[]
                    {
                        new()
                        {
                            Key = "testUser", Create = true, Update = false, Delete = false
                        }
                    }
                }
            }
        };
        var defaultSettings = settings.GetConfigurationDictionaries();

        var config = new KeycloakSeederConfigModel(defaultSettings, defaultSettings.SeederConfigurations["users"]);

        var result = config.ModificationAllowed(ModificationType.Create, "testUser");

        result.Should().BeTrue();
    }

    [Fact]
    public void ModificationAllowed_EntityKeyNotInSpecificConfig_UsesSpecificConfig()
    {
        var settings = new KeycloakRealmSettings
        {
            Create = false,
            Update = false,
            Delete = false,
            SeederConfigurations = new SeederConfiguration[]
            {
                new()
                {
                    Key = ConfigurationKey.Users.ToString(),
                    Create = true,
                    Update = false,
                    Delete = false,
                    SeederConfigurations = new SeederConfiguration[]
                    {
                        new()
                        {
                            Key = "testUser", Create = false, Update = false, Delete = false
                        }
                    }
                }
            }
        };
        var defaultSettings = settings.GetConfigurationDictionaries();

        var config = new KeycloakSeederConfigModel(defaultSettings, defaultSettings.SeederConfigurations["users"]);

        var result = config.ModificationAllowed(ModificationType.Create, "nonexistent");

        result.Should().BeTrue();
    }

    [Fact]
    public void ModificationAllowed_NestedEntityConfigAllowsCreate_ReturnsTrue()
    {
        var settings = new KeycloakRealmSettings
        {
            Create = false,
            Update = false,
            Delete = false,
            SeederConfigurations = new SeederConfiguration[]
            {
                new()
                {
                    Key = ConfigurationKey.Users.ToString(),
                    Create = false,
                    Update = false,
                    Delete = false,
                    SeederConfigurations = new SeederConfiguration[]
                    {
                        new()
                        {
                            Key = "testUser", Create = false, Update = false, Delete = false,
                            SeederConfigurations = new SeederConfiguration[]
                            {
                                new()
                                {
                                    Key = ConfigurationKey.FederatedIdentities.ToString(),
                                    Create = true,
                                    Update = false,
                                    Delete = false,
                                }
                            }
                        }
                    }
                }
            }
        };
        var defaultSettings = settings.GetConfigurationDictionaries();

        var config = new KeycloakSeederConfigModel(defaultSettings, defaultSettings.SeederConfigurations["users"]);

        var result = config.ModificationAllowed("testUser", ConfigurationKey.FederatedIdentities, ModificationType.Create);

        result.Should().BeTrue();
    }

    [Fact]
    public void ModificationAllowed_NestedSpecificEntityConfigAllowsCreate_ReturnsTrue()
    {
        var settings = new KeycloakRealmSettings
        {
            Create = false,
            Update = false,
            Delete = false,
            SeederConfigurations = new SeederConfiguration[]
            {
                new()
                {
                    Key = ConfigurationKey.Users.ToString(),
                    Create = false,
                    Update = false,
                    Delete = false,
                    SeederConfigurations = new SeederConfiguration[]
                    {
                        new()
                        {
                            Key = "testUser",
                            Create = false,
                            Update = false,
                            Delete = false,
                            SeederConfigurations = new SeederConfiguration[]
                            {
                                new()
                                {
                                    Key = ConfigurationKey.FederatedIdentities.ToString(),
                                    Create = false,
                                    Update = false,
                                    Delete = false,
                                    SeederConfigurations = new SeederConfiguration[]
                                    {
                                        new()
                                        {
                                            Key = "fi1",
                                            Create = true,
                                            Update = false,
                                            Delete = false,
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
        var defaultSettings = settings.GetConfigurationDictionaries();

        var config = new KeycloakSeederConfigModel(defaultSettings, defaultSettings.SeederConfigurations["users"]);

        var result = config.ModificationAllowed("testUser", ConfigurationKey.FederatedIdentities, ModificationType.Create, "fi1");

        result.Should().BeTrue();
    }

    [Fact]
    public void ModificationAllowed_NestedSpecificEntityNotFoundConfigAllowsCreate_ReturnsNestedEntity()
    {
        var settings = new KeycloakRealmSettings
        {
            Create = false,
            Update = false,
            Delete = false,
            SeederConfigurations = new SeederConfiguration[]
            {
                new()
                {
                    Key = ConfigurationKey.Users.ToString(),
                    Create = false,
                    Update = false,
                    Delete = false,
                    SeederConfigurations = new SeederConfiguration[]
                    {
                        new()
                        {
                            Key = "testUser", Create = false, Update = false, Delete = false,
                            SeederConfigurations = new SeederConfiguration[]
                            {
                                new()
                                {
                                    Key = ConfigurationKey.FederatedIdentities.ToString(),
                                    Create = true,
                                    Update = false,
                                    Delete = false,
                                    SeederConfigurations = new SeederConfiguration[]
                                    {
                                        new()
                                        {
                                            Key = "fi1",
                                            Create = false,
                                            Update = false,
                                            Delete = false,
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
        var defaultSettings = settings.GetConfigurationDictionaries();

        var config = new KeycloakSeederConfigModel(defaultSettings, defaultSettings.SeederConfigurations["users"]);

        var result = config.ModificationAllowed("testUser", ConfigurationKey.FederatedIdentities, ModificationType.Create, "finotfound");

        result.Should().BeTrue();
    }

    [Fact]
    public void ModificationAllowed_WithoutNestedConfigAndSpecificEntry_ReturnsTopLevelSpecific()
    {
        var settings = new KeycloakRealmSettings
        {
            Create = false,
            Update = false,
            Delete = false,
            SeederConfigurations = new SeederConfiguration[]
            {
                new()
                {
                    Key = ConfigurationKey.Users.ToString(),
                    Create = false,
                    Update = false,
                    Delete = false,
                    SeederConfigurations = new SeederConfiguration[]
                    {
                        new()
                        {
                            Key = "testUser", Create = false, Update = false, Delete = false
                        }
                    }
                },
                new()
                {
                    Key = ConfigurationKey.FederatedIdentities.ToString(),
                    Create = false,
                    Update = false,
                    Delete = false,
                    SeederConfigurations = new SeederConfiguration[]
                    {
                        new()
                        {
                            Key = "fi",
                            Create = true,
                            Update = false,
                            Delete = false,
                        }
                    }
                }
            }
        };
        var defaultSettings = settings.GetConfigurationDictionaries();

        var config = new KeycloakSeederConfigModel(defaultSettings, defaultSettings.SeederConfigurations["users"]);

        var result = config.ModificationAllowed("testUser", ConfigurationKey.FederatedIdentities, ModificationType.Create, "fi");

        result.Should().BeTrue();
    }

    [Fact]
    public void ModificationAllowed_WithoutNestedConfig_ReturnsTopLevel()
    {
        var settings = new KeycloakRealmSettings
        {
            Create = false,
            Update = false,
            Delete = false,
            SeederConfigurations = new SeederConfiguration[]
            {
                new()
                {
                    Key = ConfigurationKey.Users.ToString(),
                    Create = false,
                    Update = false,
                    Delete = false,
                    SeederConfigurations = new SeederConfiguration[]
                    {
                        new()
                        {
                            Key = "testUser", Create = false, Update = false, Delete = false
                        }
                    }
                },
                new()
                {
                    Key = ConfigurationKey.FederatedIdentities.ToString(),
                    Create = true,
                    Update = false,
                    Delete = false,
                    SeederConfigurations = new SeederConfiguration[]
                    {
                        new()
                        {
                            Key = "fi",
                            Create = false,
                            Update = false,
                            Delete = false,
                        }
                    }
                }
            }
        };
        var defaultSettings = settings.GetConfigurationDictionaries();

        var config = new KeycloakSeederConfigModel(defaultSettings, defaultSettings.SeederConfigurations["users"]);

        var result = config.ModificationAllowed("testUser", ConfigurationKey.FederatedIdentities, ModificationType.Create, "missing");

        result.Should().BeTrue();
    }

    [Fact]
    public void ModificationAllowed_WithoutConfig_ReturnsDefault()
    {
        var settings = new KeycloakRealmSettings
        {
            Create = true,
            Update = false,
            Delete = false,
            SeederConfigurations = new SeederConfiguration[]
            {
                new()
                {
                    Key = ConfigurationKey.Users.ToString(),
                    Create = false,
                    Update = false,
                    Delete = false,
                    SeederConfigurations = new SeederConfiguration[]
                    {
                        new()
                        {
                            Key = "testUser", Create = false, Update = false, Delete = false
                        }
                    }
                }
            }
        };
        var defaultSettings = settings.GetConfigurationDictionaries();

        var config = new KeycloakSeederConfigModel(defaultSettings, defaultSettings.SeederConfigurations["users"]);

        var result = config.ModificationAllowed("xy", ConfigurationKey.FederatedIdentities, ModificationType.Create, "missing");

        result.Should().BeTrue();
    }
}
