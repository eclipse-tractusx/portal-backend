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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

public class SharedMultiKeycloakManager : ISharedMultiKeycloakManager
{

    private readonly ISharedMultiKeycloakResolver _sharedMultiKeycloakResolver;
    private readonly IPortalRepositories _portalRepositories;
    private static readonly string MasterRealm = "master";
    public SharedMultiKeycloakManager(IPortalRepositories portalRepositories, ISharedMultiKeycloakResolver sharedMultiKeycloakResolver)
    {

        _portalRepositories = portalRepositories;
        _sharedMultiKeycloakResolver = sharedMultiKeycloakResolver;
    }

    public async Task SyncMultiSharedIdpAsync()
    {
        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();
        var allInstances = await identityProviderRepository.GetAllSharedIdpInstanceDetails().ConfigureAwait(false);
        // Run all realm fetches in parallel
        var tasks = allInstances.Select(async instance =>
        {
            var keycloakClient = _sharedMultiKeycloakResolver.GetKeycloakClient(instance);
            Console.WriteLine($"Syncing shared IDP instance: {instance.SharedIdpUrl}");

            var realms = await keycloakClient.GetRealmsAsync(MasterRealm).ConfigureAwait(false);
            Console.WriteLine($"Fetched {realms.Count()} realms from shared IDP instance: {instance.SharedIdpUrl}");
            return realms.Select(realm => (instance.Id, realm._Realm!));
        });

        // Await all tasks in parallel
        var results = await Task.WhenAll(tasks);

        // Flatten results into a single list
        var mappings = results.SelectMany(r => r).ToList();
        identityProviderRepository.SyncSharedIdpRealmMappings(mappings);
    }
}
