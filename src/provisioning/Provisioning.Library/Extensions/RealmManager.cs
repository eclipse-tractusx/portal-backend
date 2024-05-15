/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

public partial class ProvisioningManager
{
    private static async ValueTask UpdateSharedRealmAsync(KeycloakClient keycloak, string alias, string? displayName, string? loginTheme)
    {
        var realm = await keycloak.GetRealmAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        realm.DisplayName = displayName;
        realm.LoginTheme = loginTheme;
        await keycloak.UpdateRealmAsync(alias, realm).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private static async ValueTask SetSharedRealmStatusAsync(KeycloakClient keycloak, string alias, bool enabled)
    {
        var realm = await keycloak.GetRealmAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None);
        realm.Enabled = enabled;
        await keycloak.UpdateRealmAsync(alias, realm).ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
