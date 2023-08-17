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
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.ProvisioningEntities;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess.Tests;

public class ProvisioningDBAccessTests : IAssemblyFixture<TestDbFixture>
{
    private readonly IFixture _fixture;
    private readonly TestDbFixture _dbTestDbFixture;

    public ProvisioningDBAccessTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetNextIdentityProviderSequence

    [Fact]
    public async Task GetNextIdentityProviderSequenceAsync_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result1 = await sut.GetNextIdentityProviderSequenceAsync().ConfigureAwait(false);
        var result2 = await sut.GetNextIdentityProviderSequenceAsync().ConfigureAwait(false);

        result1.Should().Be(1);
        result2.Should().Be(2);
    }

    #endregion

    #region GetNextClientSequence

    [Fact]
    public async Task GetNextClientSequenceAsync_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result1 = await sut.GetNextClientSequenceAsync().ConfigureAwait(false);
        var result2 = await sut.GetNextClientSequenceAsync().ConfigureAwait(false);

        result1.Should().Be(1);
        result2.Should().Be(2);
    }

    #endregion

    #region Setup

    private async Task<(IProvisioningDBAccess, ProvisioningDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new ProvisioningDBAccess(context);
        return (sut, context);
    }

    #endregion
}
