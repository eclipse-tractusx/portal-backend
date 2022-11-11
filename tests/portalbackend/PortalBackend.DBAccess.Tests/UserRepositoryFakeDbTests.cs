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
using FakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the logic of the <see cref="UserRepository"/>
/// </summary>
public class UserRepositoryFakeDbTests
{
    private readonly IFixture _fixture;
    private readonly PortalDbContext _contextFake;

    public UserRepositoryFakeDbTests()
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
        var favouriteApps = _fixture.CreateMany<Offer>(10).ToList();
        var (companyUser, iamUser) = CreateTestUserPair();
        foreach (var app in favouriteApps)
        {
            companyUser.Offers.Add(app);
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
        var expectedApp = _fixture.Create<Offer>();
        var (companyUser, iamUser) = CreateTestUserPair();
        companyUser.Company!.BoughtOffers.Add(expectedApp);
        foreach (var app in _fixture.CreateMany<Offer>())
        {
            companyUser.Company.BoughtOffers.Add(app);
        }

        var iamClient = _fixture.Create<IamClient>();
        var expectedAppInstance = new AppInstance(Guid.NewGuid(), expectedApp.Id, iamClient.Id);
        iamClient.AppInstances.Add(expectedAppInstance);
        foreach (var appInstance in _fixture.CreateMany<AppInstance>())
        {
            iamClient.AppInstances.Add(appInstance);
        }

        foreach (var role in _fixture.Build<UserRole>().With(r => r.Offer, expectedApp).With(x => x.OfferId, expectedApp.Id).CreateMany())
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
    public async Task GetCompanyUserWithRoleId_ReturnsExpectedCompanyUsers()
    {
        // Arrange
        var userRoles = _fixture.CreateMany<UserRole>(5);
        userRoles.First().UserRoleText = "CX Admin";

        var companyUser1 = _fixture.Build<CompanyUser>()
            .With(x => x.CompanyUserStatusId, CompanyUserStatusId.ACTIVE)
            .Create();
        companyUser1.UserRoles.Add(userRoles.First());
        var companyUser2 = _fixture.Build<CompanyUser>()
            .With(x => x.CompanyUserStatusId, CompanyUserStatusId.ACTIVE)
            .Create();
        companyUser2.UserRoles.Add(userRoles.Last());
        var companyUser3 = _fixture.Build<CompanyUser>()
            .With(x => x.CompanyUserStatusId, CompanyUserStatusId.ACTIVE)
            .Create();
        companyUser3.UserRoles.Add(userRoles.Last());

        var companyUsersDbSet = new List<CompanyUser>
        {
            companyUser1,
            companyUser2,
            companyUser3
        }.AsFakeDbSet();

        var userRolesDbSet = new List<CompanyUserAssignedRole>
        {
            new(companyUser1.Id, userRoles.First().Id),
            new(companyUser2.Id, userRoles.Last().Id),
            new(companyUser3.Id, userRoles.Last().Id),
        }.AsFakeDbSet();
        
        A.CallTo(() => _contextFake.UserRoles).Returns(userRoles.AsFakeDbSet());
        A.CallTo(() => _contextFake.CompanyUsers).Returns(companyUsersDbSet);
        A.CallTo(() => _contextFake.CompanyUserAssignedRoles).Returns(userRolesDbSet);
        
        _fixture.Inject(_contextFake);

        var sut = _fixture.Create<UserRepository>();

        // Act
        var result = await sut.GetCompanyUserWithRoleId(new List<Guid>{ userRoles.First().Id }).ToListAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveCount(1);
        result.Single().Should().Be(companyUser1.Id);
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

    #endregion
}
