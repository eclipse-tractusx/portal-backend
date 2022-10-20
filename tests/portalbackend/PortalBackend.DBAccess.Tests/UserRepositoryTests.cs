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
using FluentAssertions;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="UserRepository"/>
/// </summary>
public class UserRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private const string ValidIamUserId = "623770c5-cf38-4b9f-9a35-f8b9ae972e2d";
    private readonly Guid _validOfferId = new("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4");

    public UserRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetOwnCompanAndCompanyUseryIdWithCompanyNameAndUserEmailAsync

    [Fact]
    public async Task GetOwnCompanAndCompanyUseryIdWithCompanyNameAndUserEmailAsync_WithValidIamUser_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(ValidIamUserId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.companyUserId.Should().Be(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020000"));
        result.companyInformation.OrganizationName.Should().Be("Catena-X");
    }

    [Fact]
    public async Task GetOwnCompanAndCompanyUseryIdWithCompanyNameAndUserEmailAsync_WithNotExistingIamUser_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(Guid.NewGuid().ToString()).ConfigureAwait(false);

        // Assert
        (result == default).Should().BeTrue();
    }

    #endregion

    #region GetOwnCompanAndCompanyUseryId

    [Fact]
    public async Task GetOwnCompanAndCompanyUseryId_WithValidIamUser_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyAndCompanyUserId(ValidIamUserId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.companyUserId.Should().Be(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020000"));
    }

    [Fact]
    public async Task GetOwnCompanAndCompanyUseryId_WithNotExistingIamUser_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyAndCompanyUserId(Guid.NewGuid().ToString()).ConfigureAwait(false);

        // Assert
        (result == default).Should().BeTrue();
    }

    #endregion

    #region Get own company app users

    [Fact]
    public async Task GetOwnCompanyAppUsersUntrackedAsync_WithValidIamUser_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyAppUsersUntrackedAsync(_validOfferId, ValidIamUserId).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(6);
    }

    [Fact]
    public async Task GetOwnCompanyAppUsersUntrackedAsync_WithNotExistingIamUser_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyAppUsersUntrackedAsync(_validOfferId, Guid.NewGuid().ToString()).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetServiceProviderCompanyUserWithRoleId

    [Fact]
    public async Task GetServiceProviderCompanyUserWithRoleIdAsync_WithValidData_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetServiceProviderCompanyUserWithRoleIdAsync(
            new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new List<Guid>
            {
                new ("7410693c-c893-409e-852f-9ee886ce94a6") // Company Admin
            }).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetServiceProviderCompanyUserWithRoleIdAsync_WithMultipleUsers_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetServiceProviderCompanyUserWithRoleIdAsync(
            new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new List<Guid>
            {
                new ("58f897ec-0aad-4588-8ffa-5f45d6638632") // CX Admin
            }).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetServiceProviderCompanyUserWithRoleIdAsync_WithNotExistingRole_ReturnsEmptyList()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetServiceProviderCompanyUserWithRoleIdAsync(
            new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new List<Guid>
            {
                Guid.NewGuid()
            }).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    private async Task<(UserRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new UserRepository(context);
        return (sut, context);
    }
}
