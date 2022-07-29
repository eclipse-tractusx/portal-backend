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
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Tests.Shared;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the logic of the <see cref="UserRepository"/>
/// </summary>
public class UserRepositoryTests
{
    private static string CatenaXCompanyName = "Catena X";
    private static string CxAdminRolename = "CX Admin";
    private static string CompanyAdminRole = "Company Admin";
    private static readonly Guid CatenaXCompanyId = Guid.NewGuid();
    private readonly IFixture _fixture;
    private readonly PortalDbContext _contextFake;

    public UserRepositoryTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _contextFake = A.Fake<PortalDbContext>();
    }

    [Fact]
    public async Task GetAllFavouriteAppsForUser_ReturnsAppsSuccessfully()
    {
        // Arrange
        var favouriteApps = _fixture.CreateMany<App>(10).ToList();
        var (companyUser, iamUser) = CreateTestUserPair();
        foreach (var app in favouriteApps)
        {
            companyUser.Apps.Add(app);
        }
        var iamUsersFakeDbSet = new List<IamUser>{ iamUser }.AsFakeDbSet();

        A.CallTo(() => _contextFake.IamUsers).Returns(iamUsersFakeDbSet);
        _fixture.Inject(_contextFake);

        var sut = _fixture.Create<UserRepository>();

        // Act
        var results = await sut.GetAllFavouriteAppsForUserUntrackedAsync(iamUser.UserEntityId).ToListAsync();

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(favouriteApps.Count);
        var favouriteAppIds = favouriteApps.Select(app => app.Id).ToList();
        results.Should().BeEquivalentTo(favouriteAppIds);
    }
    
    [Fact]
    public async Task GetBusinessApps_ReturnsAppListSuccessfully()
    {
        // Arrange
        var expectedApp = _fixture.Create<App>();
        var (companyUser, iamUser) = CreateTestUserPair();
        companyUser.Company!.BoughtApps.Add(expectedApp);
        foreach (var app in _fixture.CreateMany<App>())
        {
            companyUser.Company.BoughtApps.Add(app);
        }

        var iamClient = _fixture.Create<IamClient>();
        iamClient.Apps.Add(expectedApp);
        foreach (var app in _fixture.CreateMany<App>())
        {
            iamClient.Apps.Add(app);
        }

        foreach (var role in _fixture.Build<UserRole>().With(r => r.IamClient, iamClient).CreateMany())
        {
            companyUser.UserRoles.Add(role);
        }

        var iamUserFakeDbSet = new List<IamUser>() { iamUser }.AsFakeDbSet();

        A.CallTo(() => _contextFake.IamUsers).Returns(iamUserFakeDbSet);
        _fixture.Inject(_contextFake);

        var sut = _fixture.Create<UserRepository>();

        // Act
        var result = await sut.GetAllBusinessAppDataForUserIdAsync(iamUser.UserEntityId).ToListAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveCount(1);
        result.Single().Id.Should().Be(expectedApp.Id);
    }

    [Fact]
    public async Task GetCatenaAndCompanyAdminIdAsync_WithMultipleCompanyAdmins_ReturnsExpectedAmount()
    {
        // Arrange
        var (companyUser, _) = CreateTestUserPair();
        CreateFakeContext(companyUser.CompanyId, false, 2);

        var sut = _fixture.Create<UserRepository>();

        // Act
        var result = await sut.GetCatenaAndCompanyAdminIdAsync(companyUser.CompanyId, CatenaXCompanyName, CxAdminRolename, CompanyAdminRole).ToListAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveCount(2);
        result.Where(x => x.RoleNames.Any(y => y == CompanyAdminRole)).Should().HaveCount(2);
        result.Where(x => x.RoleNames.Any(y => y == CxAdminRolename)).Should().HaveCount(0);
    }

    [Fact]
    public async Task GetCatenaAndCompanyAdminIdAsync_WithMultipleCompanyAdminsAndCatenaXAdmin_ReturnsExpectedAmount()
    {
        // Arrange
        var (companyUser, _) = CreateTestUserPair();
        CreateFakeContext(companyUser.CompanyId, true, 2);

        var sut = _fixture.Create<UserRepository>();

        // Act
        var result = await sut.GetCatenaAndCompanyAdminIdAsync(companyUser.CompanyId, CatenaXCompanyName, CxAdminRolename, CompanyAdminRole).ToListAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveCount(3);
        result.Where(x => x.RoleNames.Any(y => y == CompanyAdminRole)).Should().HaveCount(2);
        result.Where(x => x.RoleNames.Any(y => y == CxAdminRolename)).Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCatenaAndCompanyAdminIdAsync_WithOnlyCatenaXAdmin_ReturnsExpectedAmount()
    {
        // Arrange
        var (companyUser, _) = CreateTestUserPair();
        CreateFakeContext(companyUser.CompanyId, true, 0);

        var sut = _fixture.Create<UserRepository>();

        // Act
        var result = await sut.GetCatenaAndCompanyAdminIdAsync(companyUser.CompanyId, CatenaXCompanyName, CxAdminRolename, CompanyAdminRole).ToListAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveCount(1);
        result.Where(x => x.RoleNames.Any(y => y == CompanyAdminRole)).Should().HaveCount(0);
        result.Where(x => x.RoleNames.Any(y => y == CxAdminRolename)).Should().HaveCount(1);
    }

    #region Setup

    private (CompanyUser, IamUser) CreateTestUserPair()
    {
        var companyUser = _fixture.Build<CompanyUser>()
            .Without(u => u.IamUser)
            .Create();
        var iamUser = _fixture.Build<IamUser>()
            .With(u => u.CompanyUser, companyUser)
            .Create();
        companyUser.IamUser = iamUser;
        return (companyUser, iamUser);
    }

    private void CreateFakeContext(Guid companyId, bool withCatenaXAdmin, int companyAdminCount)
    {
        var catenaCompany = new Company(Guid.NewGuid(), CatenaXCompanyName, CompanyStatusId.ACTIVE,
            DateTimeOffset.UtcNow);

        var catenaXAdminRole = new UserRole(Guid.NewGuid(), CxAdminRolename, Guid.NewGuid());
        var companyAdminRole = new UserRole(Guid.NewGuid(), CompanyAdminRole, Guid.NewGuid());
        var rolesDbSet = new List<UserRole>
        {
            catenaXAdminRole,
            companyAdminRole
        };
        var companyUsers = companyAdminCount > 0 ?
            _fixture.Build<CompanyUser>()
                .With(x => x.CompanyId, companyId)
                .CreateMany(companyAdminCount)
                .ToList() :
            new List<CompanyUser>();
        foreach (var companyUser in companyUsers)
        {
            companyUser.UserRoles.Add(companyAdminRole);
        }

        var companyUserRoles = companyUsers
            .Select(companyUser => new CompanyUserAssignedRole(companyUser.Id, companyAdminRole.Id))
            .ToList();

        if(withCatenaXAdmin)
        {
            var catenaAdmin = new CompanyUser(Guid.NewGuid(), CatenaXCompanyId, CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow)
            {
                Lastname = CxAdminRolename,
                Company = catenaCompany,
                UserRoles = { catenaXAdminRole }
            };
            companyUsers.Add(catenaAdmin);
            companyUserRoles.Add(new CompanyUserAssignedRole(catenaAdmin.Id, catenaXAdminRole.Id));
        }

        A.CallTo(() => _contextFake.Companies).Returns(new List<Company> { catenaCompany }.AsFakeDbSet());
        A.CallTo(() => _contextFake.UserRoles).Returns(rolesDbSet.AsFakeDbSet());
        A.CallTo(() => _contextFake.CompanyUsers).Returns(companyUsers.AsFakeDbSet());
        A.CallTo(() => _contextFake.CompanyUserAssignedRoles).Returns(companyUserRoles.AsFakeDbSet());
        _fixture.Inject(_contextFake);
    }

    #endregion
}
