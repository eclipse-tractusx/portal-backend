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
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Web.Identity;
using System.Security.Claims;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication.Tests;

public class KeycloakClaimsTransformationTests
{
    private readonly IFixture _fixture;

    public KeycloakClaimsTransformationTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task TransformAsync_RurnsExpected()
    {
        // Arrange
        var resource_access =
        """
        {
            "client1": {
                "roles": [
                    "client1_role1",
                    "client1_role2"
                ]
            },
            "client2": {
                "roles": [
                    "client2_role1",
                    "client2_role2"
                ]
            },
            "client3": {
                "roles": [
                    "client3_role1",
                    "client3_role2"
                ]
            }
        }
        """;

        var identity = new ClaimsIdentity(new[] { new Claim(PortalClaimTypes.ResourceAccess, resource_access, "JSON") });
        var principal = new ClaimsPrincipal(identity);

        var options = Options.Create(new JwtBearerOptions()
        {
            TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
            {
                ValidAudience = "client2"
            }
        });

        var sut = new KeycloakClaimsTransformation(options);

        // Act
        var result = await sut.TransformAsync(principal).ConfigureAwait(false);

        // Assert
        result.Identities.Should().Contain(x =>
            x.Claims.Any(x =>
                x.Type == ClaimTypes.Role &&
                x.Value == "client2_role1") &&
            x.Claims.Any(x =>
                x.Type == ClaimTypes.Role &&
                x.Value == "client2_role2"));

        result.Identities.Should().NotContain(x =>
            x.Claims.Any(x =>
                x.Type == ClaimTypes.Role &&
                x.Value == "client1_role1") ||
            x.Claims.Any(x =>
                x.Type == ClaimTypes.Role &&
                x.Value == "client1_role2") ||
            x.Claims.Any(x =>
                x.Type == ClaimTypes.Role &&
                x.Value == "client3_role1") ||
            x.Claims.Any(x =>
                x.Type == ClaimTypes.Role &&
                x.Value == "client3_role2"));
    }
}
