# Seeding Configuration

The Keycloak seeder has the possibility to be configured to only create, update and delete specific types or even specific entities for each realm.
The settings for the seeding can be made via the configuration. In each role config there is the possibility to set the SeederConfiguration.

## Default Configuration

In the Seeder configuration you must have one Default entry where the following values needs to be set:

**Example**:

```json
    "Realms": [
        "Create": true,
        "Update": true,
        "Delete": true,
        "SeederConfigurations": []
    ]
```

with this the general logic to create, update, delete entries can either be enabled or disabled.

## Type Specific Configuration

To be able to enable or disable the functionality for specific types the SeederConfigurations array in the seeder configuration can be used.

**Example**:

```json
    "SeederConfigurations": [
      {
        "Key": "Localizations",
        "Create": false,
        "Update": false,
        "Delete": false,
      }
    ]
```

with this example configuration all entities would be created, updated and deleted, but for all entities that are `Localization` the seeding wouldn't do anything.

### Possible Types

The following types can be configured:

- `Roles`
- `Localizations`
- `UserProfile`
- `ClientScopes`
- `Clients`
- `IdentityProviders`
- `IdentityProviderMappers`
- `Users`
- `FederatedIdentities`
- `ClientScopeMappers`
- `ProtocolMappers`
- `AuthenticationFlows`
- `ClientProtocolMapper`
- `ClientRoles`
- `AuthenticationFlowExecution`
- `AuthenticatorConfig`

## Entry Specific Configuration

To be able to enable or disable the seeding for specific values the configuration can be adjusted as follows:

**Example**

```json
    "SeederConfigurations": [
      {
        "Key": "Localizations",
        "Create": true,
        "Update": false,
        "Delete": true,
        "SeederConfigurations": [
          {
            "Key": "profile.attributes.organisation",
            "Create": true,
            "Update": true,
            "Delete": true
          }
        ]
      }
    ]
```

In the example above you can see that the default settings as well as the specific type settings for update are disabled.
But for localizations with the key `profile.attributes.organisation` the update is enabled. With this option you can enable the modification specifically for only the entities you want to modify with the seeding.

**Note**: The key defers for the specific types e.g. for `Localization` it is a string for `User` it is a uuid. Keys are case-sensitive.

## Entity Specific Type Configurations

For some entities there is a specific entry type configuration in place. E.g. FederatedIdentities can be configured for a specific user.

**Example**

```json
    "SeederConfigurations": [
      {
        "Key": "Users",
        "Create": true,
        "Update": false,
        "Delete": false,
        "SeederConfigurations": [
          {
            "Key": "e69c1397-eee8-434a-b83b-dc7944bb9bdd",
            "Create": true,
            "Update": true,
            "Delete": false,
            "SeederConfigurations": [
              {
                "Key": "FederatedIdentities",
                "Create": false,
                "Update": false,
                "Delete": false,
                "SeederConfigurations": [
                  {
                    "Key": "CX-Operator",
                    "Create": true,
                    "Update": true,
                    "Delete": true
                  }
                ]
              }
            ]
          }
        ]
      }
    ]
```

## Example Configuration

For further reference you can have a look at the [example appsettings](./appsettings.example.json)

## Not supported modifications

- UserProfiles can only be updated. The deletion and creation of userProfiles isn't supported
- Clients can't be deleted since it isn't supported by the api
- IdentityProviders can't be deleted yet
- Users can't be deleted yet

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 Contributors to the Eclipse Foundation
- Source URL: <https://github.com/eclipse-tractusx/portal-backend>
