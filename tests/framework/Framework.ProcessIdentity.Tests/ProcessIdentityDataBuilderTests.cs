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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ProcessIdentity;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ProcessIdentity.DependencyInjection;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ProcessIdentity.Tests;

public class ProcessIdentityDataBuilderTests
{
    private readonly IFixture _fixture;
    private readonly Guid _identityId = Guid.NewGuid();
    private readonly ProcessIdentityDataBuilder _sut;

    public ProcessIdentityDataBuilderTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var settings = _fixture.Build<ProcessIdentitySettings>().With(x => x.ProcessUserId, _identityId).Create();
        var options = Options.Create(settings);

        _sut = new ProcessIdentityDataBuilder(options);
    }

    [Fact]
    public void IdentityId_ReturnsExpected()
    {
        // Act
        var result = _sut.IdentityId;

        // Assert
        result.Should().Be(_identityId);
    }

    [Fact]
    public void AddIdentityData_ReturnsExpected()
    {
        // Arrange
        var identityType = _fixture.Create<IdentityTypeId>();
        var companyId = Guid.NewGuid();

        // Act
        _sut.AddIdentityData(identityType, companyId);
        var identityTypeResult = _sut.IdentityTypeId;
        var companyIdResult = _sut.CompanyId;

        // Assert
        identityTypeResult.Should().Be(identityType);
        companyIdResult.Should().Be(companyId);
    }

    [Fact]
    public void IdentityTypeId_WithoutCallToAddIdentityData_Throws()
    {
        // Act
        var error = Assert.Throws<UnexpectedConditionException>(() => _sut.IdentityTypeId);

        // Assert
        error.Message.Should().Be("identityType should never be null here (GetIdentityData must be called before)");
    }

    [Fact]
    public void CompanyId_WithoutCallToAddIdentityData_Throws()
    {
        // Act
        var error = Assert.Throws<UnexpectedConditionException>(() => _sut.CompanyId);

        // Assert
        error.Message.Should().Be("companyId should never be null here (GetIdentityData must be called before)");
    }

    [Fact]
    public void IdentityType_WithoutGetIdentitDataCalled_Throws()
    {
        // Act
        var error = Assert.Throws<UnexpectedConditionException>(() => _sut.IdentityTypeId);

        // Assert
        error.Message.Should().Be("identityType should never be null here (GetIdentityData must be called before)");
    }
}
