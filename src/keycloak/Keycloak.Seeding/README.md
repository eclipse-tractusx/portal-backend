# Seeding Configuration

The Keycloak seeder has the possibility to be configured to only create, update and delete specific types or even specific entities for each realm.
The settings for the seeding can be made via the configuration. In each role config there is the possibility to set the SeederConfiguration.

## Default Configuration

In the Seeder configuration you must have one Default entry where the following values needs to be set:

**Example**:

```json
    "SeederConfiguration": {
        "Key": "Default",
        "Create": true,
        "Update": true,
        "Delete": true,
        "Entities": []
    }
```

with this the general logic to create, update, delete entries can either be enabled or disabled.

## Type Specific Configuration

To be able to enable or disable the functionality for specific types the Entities array in the seeder configuration can be used.

**Example**:

```json
    "SeederConfiguration": {
        "Key": "Default",
        "Create": true,
        "Update": true,
        "Delete": true,
        "Entities": [
          {
            "Key": "Localizations",
            "Create": false,
            "Update": false,
            "Delete": false,
          }
        ]
    }
```

with this example configuration all entities would be created, updated and deleted, but for all entities that are roles the seeding wouldn't do anything.

### Possible Types

The following types can be configured:

- `ROLES`
- `LOCALIZATIONS`
- `USERPROFILE`
- `CLIENTSCOPES`
- `CLIENTS`
- `IDENTITYPROVIDERS`
- `IDENTITYPROVIDERMAPPERS`
- `USERS`
- `FEDERATEDIDENTITIES`
- `CLIENTSCOPEMAPPERS`
- `PROTOCOLMAPPERS`
- `AUTHENTICATIONFLOWS`
- `CLIENTPROTOCOLMAPPER`
- `CLIENTROLES`
- `AUTHENTICATIONFLOWEXECUTION`
- `AUTHENTICATORCONFIG`

## Entry Specific Configuration

To be able to enable or disable the seeding for specific values the configuration can be adjusted as follows:

**Example**

```json
    "SeederConfiguration": {
        "Key": "Default",
        "Create": true,
        "Update": false,
        "Delete": true,
        "Entities": [
          {
            "Key": "Localizations",
            "Create": true,
            "Update": false,
            "Delete": true,
            "Entities": [
              {
                "Key": "profile.attributes.organisation",
                "Create": true,
                "Update": true,
                "Delete": true
              }
            ]
          }
        ]
    }
```

In the example above you can see that the default settings as well as the specific type settings for update are disabled.
But for localizations with the key `profile.attributes.organisation` the update is enabled. With this option you can enable the modification specifically for only the entities you want to modify with the seeding.

**Note**: The key defers for the specific types e.g. for `Localization` it is a string for `User` it is a uuid.

## Example Configuration

For further reference you can have a look at the [example appsettings](./appsettings.example.json)

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 Contributors to the Eclipse Foundation
- Source URL: https://github.com/eclipse-tractusx/portal-backend
