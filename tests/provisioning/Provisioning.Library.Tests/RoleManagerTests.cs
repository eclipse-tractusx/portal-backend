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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Flurl.Http.Testing;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests;

public class RoleManagerTests
{
    private const string ValidClientName = "valid";
    private readonly ProvisioningManager _sut;

    public RoleManagerTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var keycloakFactory = A.Fake<IKeycloakFactory>();
        A.CallTo(() => keycloakFactory.CreateKeycloakClient("central"))
            .Returns(new KeycloakClient("https://test.de", "test", "test", "test"));
        var settings = new ProvisioningSettings
        {
            CentralRealm = "test"
        };

        _sut = new ProvisioningManager(keycloakFactory, null, Options.Create(settings));
    }

    [Fact]
    public async Task AddRolesToClientAsync_WithValidData_CallsExpected()
    {
        // Arrange
        const string clientId = "clientId";
        var roles = new[]
        {
            "123",
            "test"
        };
        using var httpTest = new HttpTest();
        httpTest.RespondWithJson(new { access_token = "123"})
            .RespondWithJson(new []{new Client { Id = clientId}})
            .RespondWithJson(new { access_token = "123"})
            .RespondWithJson(new List<Role>())
            .RespondWithJson(new { access_token = "123"});
        
        // Act
        await _sut.AddRolesToClientAsync(ValidClientName, roles).ConfigureAwait(false);

        // Assert
        httpTest.ShouldHaveCalled($"*/admin/realms/test/clients/{clientId}/roles").WithVerb(HttpMethod.Post).Times(2);
    }

    [Fact]
    public async Task AddRolesToClientAsync_WithAlreadyExistingRole_CallsExpected()
    {
        // Arrange
        const string clientId = "clientId";
        var roles = new[]
        {
            "123",
            "test"
        };
        using var httpTest = new HttpTest();
        httpTest.RespondWithJson(new { access_token = "123"})
            .RespondWithJson(new []{new Client { Id = clientId}})
            .RespondWithJson(new { access_token = "123"})
            .RespondWithJson(new []{ new Role{ Name = "123" }})
            .RespondWithJson(new { access_token = "123"});
        
        // Act
        await _sut.AddRolesToClientAsync(ValidClientName, roles).ConfigureAwait(false);

        // Assert
        httpTest.ShouldHaveCalled($"*/admin/realms/test/clients/{clientId}/roles").WithVerb(HttpMethod.Post).Times(1);
    }

    [Fact]
    public async Task AddRolesToClientAsync_WithInvalidClient_NothingGetsCalled()
    {
        // Arrange
        var roles = new[]
        {
            "123",
            "test"
        };
        var client = "notvalid";
        var httpTest = new HttpTest();
        httpTest.RespondWithJson(new { access_token = "123"}).RespondWithJson(new List<Client>());

        // Act
        async Task Act() => await _sut.AddRolesToClientAsync("notvalid", roles).ConfigureAwait(false);
        

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Client {client} does not exist");
    }
}