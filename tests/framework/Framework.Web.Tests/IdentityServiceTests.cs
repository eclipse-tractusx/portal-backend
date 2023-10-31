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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using System.Security.Claims;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Web.Tests;

public class IdentityServiceTests
{
    private readonly IFixture _fixture;

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpContext _httpContext;
    private readonly ClaimsPrincipal _user;

    public IdentityServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _httpContextAccessor = A.Fake<IHttpContextAccessor>();
        _httpContext = A.Fake<HttpContext>();
        _user = A.Fake<ClaimsPrincipal>();
        A.CallTo(() => _httpContextAccessor.HttpContext).Returns(_httpContext);
    }

    [Fact]
    public void IdentityData_ReturnsExpected()
    {
        // Arrange
        var sub = _fixture.Create<string>();
        var identityId = Guid.NewGuid();
        var identityType = _fixture.Create<IdentityTypeId>();
        var companyId = Guid.NewGuid();

        var sut = CreateSut(sub, identityId.ToString(), identityType.ToString(), companyId.ToString());

        // Act
        var first = sut.IdentityData;
        var second = sut.IdentityData;

        // Assert
        first.Should().NotBeNull()
            .And.BeSameAs(second)
            .And.Match<IdentityData>(x =>
                x.UserEntityId == sub &&
                x.UserId == identityId &&
                x.IdentityType == identityType &&
                x.CompanyId == companyId);

        A.CallTo(() => _httpContext.User).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IdentityData_EmptySub_Throws()
    {
        // Arrange
        var identityId = Guid.NewGuid();
        var identityType = _fixture.Create<IdentityTypeId>();
        var companyId = Guid.NewGuid();

        var sut = CreateSut("", identityId.ToString(), identityType.ToString(), companyId.ToString());

        // Act
        var error = Assert.Throws<ControllerArgumentException>(() => sut.IdentityData);

        // Assert
        error.Message.Should().Be("Claim sub must not be null or empty (Parameter 'claims')");
        A.CallTo(() => _httpContext.User).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IdentityData_EmptyIdentityId_Throws()
    {
        // Arrange
        var sub = _fixture.Create<string>();
        var identityType = _fixture.Create<IdentityTypeId>();
        var companyId = Guid.NewGuid();

        var sut = CreateSut(sub, "", identityType.ToString(), companyId.ToString());

        // Act
        var error = Assert.Throws<ControllerArgumentException>(() => sut.IdentityData);

        // Assert
        error.Message.Should().Be("Claim https://catena-x.net//schema/2023/05/identity/claims/identity_id must not be null or empty (Parameter 'claims')");
        A.CallTo(() => _httpContext.User).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IdentityData_NonGuidIdentityId_Throws()
    {
        // Arrange
        var sub = _fixture.Create<string>();
        var identityType = _fixture.Create<IdentityTypeId>();
        var companyId = Guid.NewGuid();

        var sut = CreateSut(sub, "deadbeef", identityType.ToString(), companyId.ToString());

        // Act
        var error = Assert.Throws<ControllerArgumentException>(() => sut.IdentityData);

        // Assert
        error.Message.Should().Be("Claim https://catena-x.net//schema/2023/05/identity/claims/identity_id must contain a Guid (Parameter 'claims')");
        A.CallTo(() => _httpContext.User).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IdentityData_EmptyIdentityType_Throws()
    {
        // Arrange
        var sub = _fixture.Create<string>();
        var identityId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var sut = CreateSut(sub, identityId.ToString(), "", companyId.ToString());

        // Act
        var error = Assert.Throws<ControllerArgumentException>(() => sut.IdentityData);

        // Assert
        error.Message.Should().Be("Claim https://catena-x.net//schema/2023/05/identity/claims/identity_type must not be null or empty (Parameter 'claims')");
        A.CallTo(() => _httpContext.User).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IdentityData_NonEnumIdentityType_Throws()
    {
        // Arrange
        var sub = _fixture.Create<string>();
        var identityId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var sut = CreateSut(sub, identityId.ToString(), "deadbeef", companyId.ToString());

        // Act
        var error = Assert.Throws<ControllerArgumentException>(() => sut.IdentityData);

        // Assert
        error.Message.Should().Be("Claim https://catena-x.net//schema/2023/05/identity/claims/identity_type must contain a Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums.IdentityTypeId (Parameter 'claims')");
        A.CallTo(() => _httpContext.User).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IdentityData_EmptyCompanyId_Throws()
    {
        // Arrange
        var sub = _fixture.Create<string>();
        var identityId = Guid.NewGuid();
        var identityType = _fixture.Create<IdentityTypeId>();

        var sut = CreateSut(sub, identityId.ToString(), identityType.ToString(), "");

        // Act
        var error = Assert.Throws<ControllerArgumentException>(() => sut.IdentityData);

        // Assert
        error.Message.Should().Be("Claim https://catena-x.net//schema/2023/05/identity/claims/company_id must not be null or empty (Parameter 'claims')");
        A.CallTo(() => _httpContext.User).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IdentityData_NonGuidCompanyId_Throws()
    {
        // Arrange
        var sub = _fixture.Create<string>();
        var identityId = Guid.NewGuid();
        var identityType = _fixture.Create<IdentityTypeId>();

        var sut = CreateSut(sub, identityId.ToString(), identityType.ToString(), "deadbeef");

        // Act
        var error = Assert.Throws<ControllerArgumentException>(() => sut.IdentityData);

        // Assert
        error.Message.Should().Be("Claim https://catena-x.net//schema/2023/05/identity/claims/company_id must contain a Guid (Parameter 'claims')");
        A.CallTo(() => _httpContext.User).MustHaveHappenedOnceExactly();
    }

    private IIdentityService CreateSut(string sub, string identityId, string identityType, string companyId)
    {
        var claims = new Claim[] {
            new(PortalClaimTypes.Sub, sub),
            new(PortalClaimTypes.IdentityId, identityId),
            new(PortalClaimTypes.IdentityType, identityType),
            new(PortalClaimTypes.CompanyId, companyId)
        };

        A.CallTo(() => _user.Claims).Returns(claims);
        A.CallTo(() => _httpContext.User).Returns(_user);

        return new IdentityService(_httpContextAccessor);
    }
}
