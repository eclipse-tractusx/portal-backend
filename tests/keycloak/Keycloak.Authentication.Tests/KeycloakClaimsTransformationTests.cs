/********************************************************************************
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

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using System.Security.Claims;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication.Tests;

public class KeycloakClaimsTransformationTests
{
    private readonly KeycloakClaimsTransformation _sut;
    private readonly IIdentityRepository _identityRepository;
    private readonly IFixture _fixture;
    private readonly IMockLogger<KeycloakClaimsTransformation> _mockLogger;

    public KeycloakClaimsTransformationTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var portalRepositories = A.Fake<IPortalRepositories>();
        _identityRepository = A.Fake<IIdentityRepository>();
        A.CallTo(() => portalRepositories.GetInstance<IIdentityRepository>()).Returns(_identityRepository);

        _mockLogger = A.Fake<IMockLogger<KeycloakClaimsTransformation>>();
        ILogger<KeycloakClaimsTransformation> logger = new MockLogger<KeycloakClaimsTransformation>(_mockLogger);
        _sut = new KeycloakClaimsTransformation(Options.Create(new JwtBearerOptions()), portalRepositories, logger);
    }

    [Fact]
    public async Task TransformAsync_WithValid_ReturnsExpected()
    {
        // Arrange
        var identityId = Guid.NewGuid();
        var identity = new ClaimsIdentity(Enumerable.Repeat(new Claim(PortalClaimTypes.PreferredUserName, identityId.ToString()), 1));
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _sut.TransformAsync(principal).ConfigureAwait(false);

        // Assert
        result.Identities.Should().Contain(x => x.Claims.Any(x => x.Type == PortalClaimTypes.IdentityId));
    }

    [Fact]
    public async Task TransformAsync_WithoutIdentityIdWithValidUserEntityId_ReturnsExpected()
    {
        // Arrange
        var identityId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        A.CallTo(() => _identityRepository.GetIdentityIdByUserEntityId(userId))
            .Returns(identityId);
        var identity = new ClaimsIdentity(Enumerable.Repeat(new Claim(PortalClaimTypes.Sub, userId), 1));
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _sut.TransformAsync(principal).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception?>._, $"Preferred user name (null) couldn't be parsed to uuid for userEntityId {userId}")).MustHaveHappenedOnceExactly();
        result.Identities.Should().Contain(x => x.Claims.Any(x => x.Type == PortalClaimTypes.IdentityId));
    }

    [Fact]
    public async Task TransformAsync_WithoutInvalidIdentityIdWithValidUserEntityId_ReturnsExpected()
    {
        // Arrange
        var identityId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        A.CallTo(() => _identityRepository.GetIdentityIdByUserEntityId(userId))
            .Returns(identityId);
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(PortalClaimTypes.PreferredUserName, $"user.{identityId}"),
            new Claim(PortalClaimTypes.Sub, userId)
        });
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _sut.TransformAsync(principal).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception?>._, $"Preferred user name user.{identityId} couldn't be parsed to uuid for userEntityId {userId}")).MustHaveHappenedOnceExactly();
        result.Identities.Should().Contain(x => x.Claims.Any(x => x.Type == PortalClaimTypes.IdentityId));
    }

    [Fact]
    public async Task TransformAsync_WithoutIdentityIdAndUserEntityId_ReturnsExpected()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        A.CallTo(() => _identityRepository.GetIdentityIdByUserEntityId(userId))
            .Returns(Guid.Empty);
        var identity = new ClaimsIdentity(Enumerable.Repeat(new Claim(PortalClaimTypes.Sub, userId), 1));
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _sut.TransformAsync(principal).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception?>._, $"Preferred user name (null) couldn't be parsed to uuid for userEntityId {userId}")).MustHaveHappenedOnceExactly();
        result.Identities.Should().NotContain(x => x.Claims.Any(x => x.Type == PortalClaimTypes.IdentityId));
    }
}
