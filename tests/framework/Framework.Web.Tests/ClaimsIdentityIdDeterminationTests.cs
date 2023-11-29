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

using Microsoft.AspNetCore.Http;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using System.Security.Claims;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Web.Tests;

public class ClaimsIdentityIdDeterminationTests
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpContext _httpContext;
    private readonly ClaimsPrincipal _user;

    public ClaimsIdentityIdDeterminationTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _httpContextAccessor = A.Fake<IHttpContextAccessor>();
        _httpContext = A.Fake<HttpContext>();
        _user = A.Fake<ClaimsPrincipal>();
        A.CallTo(() => _httpContextAccessor.HttpContext).Returns(_httpContext);
    }

    [Fact]
    public void IdentityData_ReturnsExpected()
    {
        // Arrange
        var identityId = Guid.NewGuid();

        var sut = CreateSut(identityId.ToString());

        // Act
        var first = sut.IdentityId;
        var second = sut.IdentityId;

        // Assert
        first.Should().NotBeEmpty()
            .And.Be(second)
            .And.Be(identityId);

        A.CallTo(() => _httpContext.User).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IdentityData_EmptyIdentityId_Throws()
    {
        // Arrange
        var sut = CreateSut("");

        // Act
        var error = Assert.Throws<ControllerArgumentException>(() => sut.IdentityId);

        // Assert
        error.Message.Should().Be("Claim https://catena-x.net//schema/2023/05/identity/claims/identity_id must not be null or empty (Parameter 'claims')");
        A.CallTo(() => _httpContext.User).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IdentityData_NonGuidIdentityId_Throws()
    {
        // Arrange
        var sut = CreateSut("deadbeef");

        // Act
        var error = Assert.Throws<ControllerArgumentException>(() => sut.IdentityId);

        // Assert
        error.Message.Should().Be("Claim https://catena-x.net//schema/2023/05/identity/claims/identity_id must contain a Guid (Parameter 'claims')");
        A.CallTo(() => _httpContext.User).MustHaveHappenedOnceExactly();
    }

    private ClaimsIdentityIdDetermination CreateSut(string identityId)
    {
        var claims = new Claim[] {
            new(PortalClaimTypes.IdentityId, identityId)
        };

        A.CallTo(() => _user.Claims).Returns(claims);
        A.CallTo(() => _httpContext.User).Returns(_user);

        return new ClaimsIdentityIdDetermination(_httpContextAccessor);
    }
}
