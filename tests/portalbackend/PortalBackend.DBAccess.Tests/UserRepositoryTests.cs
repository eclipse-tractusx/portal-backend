/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="UserRepository"/>
/// </summary>
public class UserRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private const string ValidIamUserId = "623770c5-cf38-4b9f-9a35-f8b9ae972e2d";
    private readonly Guid ValidCompanyUser = new ("ac1cf001-7fbc-1f2f-817f-bce058020001");
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
    public async Task GetOwnCompanyAppUsersPaginationSourceAsync_WithValidIamUser_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyAppUsersPaginationSourceAsync(
            _validOfferId,
            ValidIamUserId,
            new [] { OfferSubscriptionStatusId.ACTIVE },
            new [] { CompanyUserStatusId.ACTIVE, CompanyUserStatusId.INACTIVE },
            new CompanyUserFilter(null, null, null, null, null))(0,15).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().NotBeEmpty();
        result!.Data.Should().HaveCount(7);
    }

    [Fact]
    public async Task GetOwnCompanyAppUsersPaginationSourceAsync_Inactive_WithValidIamUser_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyAppUsersPaginationSourceAsync(
            _validOfferId,
            ValidIamUserId,
            new [] { OfferSubscriptionStatusId.ACTIVE },
            new [] { CompanyUserStatusId.INACTIVE },
            new CompanyUserFilter(null, null, null, null, null))(0,15).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().NotBeEmpty();
        result!.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOwnCompanyAppUsersPaginationSourceAsync_Active_Inactive_Deleted_WithValidIamUser_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyAppUsersPaginationSourceAsync(
            _validOfferId,
            ValidIamUserId,
            new [] { OfferSubscriptionStatusId.ACTIVE },
            new [] { CompanyUserStatusId.ACTIVE, CompanyUserStatusId.INACTIVE, CompanyUserStatusId.DELETED },
            new CompanyUserFilter(null, null, null, null, null))(0,15).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.Data.Should().NotBeEmpty();
        result!.Data.Should().HaveCount(8);
    }

    [Fact]
    public async Task GetOwnCompanyAppUsersPaginationSourceAsync_WithNotExistingIamUser_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOwnCompanyAppUsersPaginationSourceAsync(
            _validOfferId,
            Guid.NewGuid().ToString(),
            new [] { OfferSubscriptionStatusId.ACTIVE },
            new [] { CompanyUserStatusId.ACTIVE, CompanyUserStatusId.INACTIVE },
            new CompanyUserFilter(null, null, null, null, null))(0,15).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
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

    #region GetCompanyUserWithIamUserCheckAndCompanyName

    [Fact]
    public async Task GetCompanyUserWithIamUserCheckAndCompanyName_WithoutSalesManager_ReturnsOnlyOneUser()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);
        
        // Act        
        var result = await sut.GetCompanyUserWithIamUserCheckAndCompanyName(ValidIamUserId, null).ToListAsync().ConfigureAwait(false);
        
        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCompanyUserWithIamUserCheckAndCompanyName_WithoutUserAndWithoutSalesmanager_ReturnsNoUsers()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);
        
        // Act        
        var result = await sut.GetCompanyUserWithIamUserCheckAndCompanyName(string.Empty, null).ToListAsync().ConfigureAwait(false);
        
        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCompanyUserWithIamUserCheckAndCompanyName_WithoutUserAndWithSalesmanager_ReturnsOneUsers()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);
        
        // Act        
        var result = await sut.GetCompanyUserWithIamUserCheckAndCompanyName(string.Empty, ValidCompanyUser).ToListAsync().ConfigureAwait(false);
        
        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCompanyUserWithIamUserCheckAndCompanyName_WithUserAndWithSalesmanager_ReturnsTwoUsers()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);
        
        // Act        
        var result = await sut.GetCompanyUserWithIamUserCheckAndCompanyName(ValidIamUserId, ValidCompanyUser).ToListAsync().ConfigureAwait(false);
        
        // Assert
        result.Should().HaveCount(2);
        result.Select(x => x.CompanyName).Should().Contain("Catena-X");
    }

    #endregion

    #region GetCompanyUserWithRoleIdForCompany

    [Fact]
    public async Task GetCompanyUserWithRoleIdForCompany_WithExistingUserForRole_ReturnsExpectedUserId()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);
        
        // Act
        var result = await sut
            .GetCompanyUserWithRoleIdForCompany(new[] {new Guid("607818be-4978-41f4-bf63-fa8d2de51154")}, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"))
            .ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(1);
        result.First().Should().Be(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019993"));
    }

    #endregion
    
    #region GetCompanyUserWithRoleIdForCompany

    [Fact]
    public async Task GetCompanyUserEmailForCompanyAndRoleId_WithExistingUserForRole_ReturnsExpectedEmail()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);
        
        // Act
        var result = await sut
            .GetCompanyUserEmailForCompanyAndRoleId(new[] {new Guid("607818be-4978-41f4-bf63-fa8d2de51154")}, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"))
            .ToListAsync().ConfigureAwait(false);
        
        // Assert
        result.Should().HaveCount(1);
        result.First().Email.Should().Be("tester.user4@test.de");
    }

    #endregion

    private async Task<(UserRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new UserRepository(context);
        return (sut, context);
    }
}
