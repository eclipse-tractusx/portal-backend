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
using FakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
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
        var iamUsersFakeDbSet = new IamUser[] { iamUser }.AsFakeDbSet();

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
