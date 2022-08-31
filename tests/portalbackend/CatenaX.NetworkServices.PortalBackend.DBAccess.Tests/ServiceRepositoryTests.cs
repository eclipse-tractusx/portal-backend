/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Tests.Setup;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Tests.Shared.TestSeeds;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="ServiceRepositoryTests"/>
/// </summary>
// [Collection("Database collection")]
public class ServiceRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly IFixture _fixture;
    private readonly TestDbFixture _dbTestDbFixture;

    public ServiceRepositoryTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetActiveServices

    [Fact]
    public async Task GetActiveServices_ReturnsExpectedAppCount()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var results = await sut.GetActiveServices().ToListAsync();

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(1);
    }

    #endregion

    #region GetServiceDetailByIdUntrackedAsync

    [Fact]
    public async Task GetServiceDetailByIdUntrackedAsync_WithNotExistingService_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var results = await sut.GetServiceDetailByIdUntrackedAsync(Guid.NewGuid(), "en");

        // Assert
        results.Should().BeNull();
    }

    [Fact]
    public async Task GetServiceDetailByIdUntrackedAsync_ReturnsServiceDetailData()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetServiceDetailByIdUntrackedAsync(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), "en");

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Newest Service");
        result.ContactEmail.Should().Be("service-test@mail.com");
        result.Provider.Should<string>().Be("Catena X");
    }

    #endregion

    private async Task<AppRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        _fixture.Inject(context);
        var sut = _fixture.Create<AppRepository>();
        return sut;
    }
}
