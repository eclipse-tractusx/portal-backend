/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Flurl.Http.Testing;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.FlurlSetup;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests;

public class SharedMultiKeycloakManagerTests
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityProviderRepository _idpRepository;
    private readonly ISharedMultiKeycloakResolver _resolver;

    public SharedMultiKeycloakManagerTests()
    {
        _portalRepositories = A.Fake<IPortalRepositories>();
        _idpRepository = A.Fake<IIdentityProviderRepository>();
        _resolver = A.Fake<ISharedMultiKeycloakResolver>();

        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>())
            .Returns(_idpRepository);
    }

    [Fact]
    public async Task SyncMultiSharedIdpAsync_WithInstances_CallsExpected()
    {
        // Arrange
        var instance = new SharedIdpInstanceDetail(
            Guid.NewGuid(),
            "https://shared.example.org",
            "clientX",
            new byte[16],
            new byte[16],
            1,
            DateTimeOffset.UtcNow
        );

        var instances = new List<SharedIdpInstanceDetail> { instance };

        A.CallTo(() => _idpRepository.GetAllSharedIdpInstanceDetails())
            .Returns(instances);
        var fakeClient = new KeycloakClient("http://fake", "u", "p", "r", false);
        A.CallTo(() => _resolver.GetKeycloakClient(instance))
            .Returns(fakeClient);

        var fakeRealms = new List<Realm>
        {
            new() { _Realm = "realm1" },
            new() { _Realm = "realm2" }
        };
        using var httpTest = new HttpTest();
        httpTest.WithAuthorization()
            .WithGetRealmsAsync("master", fakeRealms);

        var sut = new SharedMultiKeycloakManager(_portalRepositories, _resolver);

        // Act
        await sut.SyncMultiSharedIdpAsync();

        // Assert
        A.CallTo(() => _resolver.GetKeycloakClient(instance))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _idpRepository.SyncSharedIdpRealmMappings(
                A<List<(Guid SharedIdpId, string RealmName)>>.That.Matches(mappings =>
                    mappings.Count == 2 &&
                    mappings.Any(m => m.RealmName == "realm1") &&
                    mappings.Any(m => m.RealmName == "realm2")
                )))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SyncMultiSharedIdpAsync_WithNoInstances_DoesNothing()
    {
        // Arrange
        A.CallTo(() => _idpRepository.GetAllSharedIdpInstanceDetails())
            .Returns(new List<SharedIdpInstanceDetail>());

        var sut = new SharedMultiKeycloakManager(_portalRepositories, _resolver);

        // Act
        await sut.SyncMultiSharedIdpAsync();

        // Assert
        A.CallTo(() => _resolver.GetKeycloakClient(A<SharedIdpInstanceDetail>._))
            .MustNotHaveHappened();

        A.CallTo(() => _idpRepository.SyncSharedIdpRealmMappings(A<List<(Guid SharedIdpId, string RealmName)>>._))
            .MustHaveHappenedOnceExactly();
    }
}
