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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class UserProfileUpdater(IKeycloakFactory keycloakFactory, ISeedDataHandler seedDataHandler)
    : IUserProfileUpdater
{
    private const string UserProfileType = "org.keycloak.userprofile.UserProfileProvider";
    private const string UserProfileConfig = "kc.user.profile.config";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task UpdateUserProfile(string keycloakInstanceName, CancellationToken cancellationToken)
    {
        var keycloak = keycloakFactory.CreateKeycloakClient(keycloakInstanceName);
        var realm = seedDataHandler.Realm;
        var userProfiles = seedDataHandler.RealmComponents.Where(x => x.ProviderType == UserProfileType);
        var defaultConfig = seedDataHandler.GetSpecificConfiguration(ConfigurationKey.UserProfile);
        if (!defaultConfig.ModificationAllowed(ModificationType.Update))
        {
            return;
        }

        var userProfile = await keycloak.GetUsersProfile(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        if (userProfiles.Count() != 1 || !(userProfiles.Single().ComponentModel.Config?.TryGetValue(UserProfileConfig, out var configs) ?? false) || configs?.Count() != 1)
        {
            throw new ConflictException("There must be exactly one user profile");
        }

        var update = JsonSerializer.Deserialize<UserProfileConfig>(configs.Single(), JsonOptions);
        if (update is null)
        {
            return;
        }

        if (!userProfile.Equals(update))
        {
            await keycloak.UpdateUsersProfile(realm, update, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }
}
