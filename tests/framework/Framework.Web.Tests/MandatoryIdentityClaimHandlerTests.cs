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

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using System.Security.Claims;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Web.Tests;

public class MandatoryIdentityClaimHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IIdentityService _identityService;
    private readonly IMockLogger<MandatoryIdentityClaimHandler> _mockLogger;
    private readonly ILogger<MandatoryIdentityClaimHandler> _logger;

    public MandatoryIdentityClaimHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _identityService = A.Fake<IIdentityService>();
        _mockLogger = A.Fake<IMockLogger<MandatoryIdentityClaimHandler>>();
        _logger = new MockLogger<MandatoryIdentityClaimHandler>(_mockLogger);
    }

    [Theory]
    [InlineData(IdentityTypeId.COMPANY_USER, PolicyTypeId.CompanyUser)]
    [InlineData(IdentityTypeId.COMPANY_SERVICE_ACCOUNT, PolicyTypeId.ServiceAccount)]
    public async Task HandleRequirementAsync_WithValidIdentityType_ReturnsExpected(IdentityTypeId identityTypeId, PolicyTypeId policyTypeId)
    {
        // Arrange
        var identity = _fixture.Build<IdentityData>().With(x => x.IdentityType, identityTypeId).Create();
        var principal = _fixture.Create<ClaimsPrincipal>();
        A.CallTo(() => _identityService.GetIdentityData()).Returns(identity);
        A.CallTo(() => _identityService.IdentityId).Returns(Guid.NewGuid());
        var sut = new MandatoryIdentityClaimHandler(_identityService, _logger);
        var ctx = new AuthorizationHandlerContext(Enumerable.Repeat(new MandatoryIdentityClaimRequirement(policyTypeId), 1), principal, null);

        // Act
        await sut.HandleAsync(ctx).ConfigureAwait(false);

        // Assert
        ctx.HasSucceeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(IdentityTypeId.COMPANY_USER, PolicyTypeId.ServiceAccount)]
    [InlineData(IdentityTypeId.COMPANY_SERVICE_ACCOUNT, PolicyTypeId.CompanyUser)]
    public async Task HandleRequirementAsync_WithInvalidIdentityType_ReturnsExpected(IdentityTypeId identityTypeId, PolicyTypeId policyTypeId)
    {
        // Arrange
        var identity = _fixture.Build<IdentityData>().With(x => x.IdentityType, identityTypeId).Create();
        var principal = _fixture.Create<ClaimsPrincipal>();
        A.CallTo(() => _identityService.GetIdentityData()).Returns(identity);
        var sut = new MandatoryIdentityClaimHandler(_identityService, _logger);
        var ctx = new AuthorizationHandlerContext(Enumerable.Repeat(new MandatoryIdentityClaimRequirement(policyTypeId), 1), principal, null);

        // Act
        await sut.HandleAsync(ctx).ConfigureAwait(false);

        // Assert
        ctx.HasSucceeded.Should().BeFalse();
    }

    [Theory]
    [InlineData(PolicyTypeId.ValidCompany)]
    [InlineData(PolicyTypeId.ValidIdentity)]
    public async Task HandleRequirementAsync_WithValid_ReturnsExpected(PolicyTypeId policyTypeId)
    {
        // Arrange
        var identity = _fixture.Build<IdentityData>().With(x => x.CompanyId, Guid.NewGuid).Create();
        var principal = _fixture.Create<ClaimsPrincipal>();
        A.CallTo(() => _identityService.GetIdentityData()).Returns(identity);
        A.CallTo(() => _identityService.IdentityId).Returns(Guid.NewGuid());
        var sut = new MandatoryIdentityClaimHandler(_identityService, _logger);
        var ctx = new AuthorizationHandlerContext(Enumerable.Repeat(new MandatoryIdentityClaimRequirement(policyTypeId), 1), principal, null);

        // Act
        await sut.HandleAsync(ctx).ConfigureAwait(false);

        // Assert
        ctx.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithInvalidCompany_ReturnsExpected()
    {
        // Arrange
        var identity = _fixture.Build<IdentityData>().With(x => x.CompanyId, Guid.Empty).Create();
        var principal = _fixture.Create<ClaimsPrincipal>();
        A.CallTo(() => _identityService.GetIdentityData()).Returns(identity);
        var sut = new MandatoryIdentityClaimHandler(_identityService, _logger);
        var ctx = new AuthorizationHandlerContext(Enumerable.Repeat(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidCompany), 1), principal, null);

        // Act
        await sut.HandleAsync(ctx).ConfigureAwait(false);

        // Assert
        ctx.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithInvalidIdentity_ReturnsExpected()
    {
        // Arrange
        var principal = _fixture.Create<ClaimsPrincipal>();
        A.CallTo(() => _identityService.IdentityId).Returns(Guid.Empty);
        var sut = new MandatoryIdentityClaimHandler(_identityService, _logger);
        var ctx = new AuthorizationHandlerContext(Enumerable.Repeat(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidIdentity), 1), principal, null);

        // Act
        await sut.HandleAsync(ctx).ConfigureAwait(false);

        // Assert
        ctx.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithFailingIdentityService_LogsException()
    {
        // Arrange
        var principal = _fixture.Create<ClaimsPrincipal>();
        var identityId = Guid.NewGuid();
        var exception = new ConflictException($"Identity {identityId} could not be found");
        A.CallTo(() => _identityService.GetIdentityData()).Throws(exception);
        var sut = new MandatoryIdentityClaimHandler(_identityService, _logger);
        var ctx = new AuthorizationHandlerContext(Enumerable.Repeat(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidCompany), 1), principal, null);

        // Act
        await sut.HandleAsync(ctx).ConfigureAwait(false);

        // Assert
        ctx.HasFailed.Should().BeTrue();
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, exception, "unable to retrieve IdentityData: {Exception}")).MustNotHaveHappened();
    }
}
