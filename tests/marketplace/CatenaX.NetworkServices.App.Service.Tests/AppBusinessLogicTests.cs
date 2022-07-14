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
using CatenaX.NetworkServices.App.Service.BusinessLogic;
using CatenaX.NetworkServices.App.Service.ViewModels;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.Tests.Shared;
using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CatenaX.NetworkServices.App.Service.Tests
{
    public class AppBusinessLogicTests
    {
        private readonly IFixture _fixture;

        public AppBusinessLogicTests()
        {
            _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        }

        [Fact]
        public async void GetAllActiveApps_ReturnsReleasedAppsSuccessfully()
        {
            // Arrange
            var apps = _fixture.Build<PortalBackend.PortalEntities.Entities.App>()
                .With(a => a.DateReleased, DateTimeOffset.MinValue) // all are active
                .CreateMany();
            var appsDbSet = apps.AsFakeDbSet();
            var languagesDbSet = new List<Language>().AsFakeDbSet();

            var contextFake = A.Fake<PortalDbContext>();
            A.CallTo(() => contextFake.Apps).Returns(appsDbSet);
            A.CallTo(() => contextFake.Languages).Returns(languagesDbSet);
            _fixture.Inject(contextFake);

            var sut = _fixture.Create<AppsBusinessLogic>();

            // Act
            var results = await sut.GetAllActiveAppsAsync().ToListAsync();

            // Assert
            results.Should().NotBeNullOrEmpty();
            results.Should().HaveCount(apps.Count());
            results.Should().AllBeOfType<AppViewModel>();
            results.Should().AllSatisfy(a => apps.Select(app => app.Id).Contains(a.Id));
        }

        [Fact]
        public async void GetAllFavouriteAppsForUser_ReturnsAppsSuccessfully()
        {
            // Arrange
            var favouriteApps = _fixture.CreateMany<PortalBackend.PortalEntities.Entities.App>();
            var (companyUser, iamUser) = CreateTestUserPair();
            foreach (var app in favouriteApps)
            {
                companyUser.Apps.Add(app);
            }
            var iamUsersFakeDbSet = new List<IamUser>{ iamUser }.AsFakeDbSet();

            var contextFake = A.Fake<PortalDbContext>();
            A.CallTo(() => contextFake.IamUsers).Returns(iamUsersFakeDbSet);
            _fixture.Inject(contextFake);

            var sut = _fixture.Create<AppsBusinessLogic>();

            // Act
            var results = await sut.GetAllFavouriteAppsForUserAsync(iamUser.UserEntityId).ToListAsync();

            // Assert
            results.Should().NotBeNullOrEmpty();
            results.Should().HaveCount(favouriteApps.Count());
            results.Should().AllSatisfy(a => favouriteApps.Select(app => app.Id).Contains(a));
        }

        [Fact]
        public async void AddFavouriteAppForUser_ExecutesSuccessfully()
        {
            // Arrange
            var appId = _fixture.Create<Guid>();
            var appFavourites = new List<CompanyUserAssignedAppFavourite>();
            var appFavouritesFakeDbSet = appFavourites.AsFakeDbSet();

            var (companyUser, iamUser) = CreateTestUserPair();
            var companyUsersFakeDbSet = new List<CompanyUser> { companyUser }.AsFakeDbSet();

            var contextFake = A.Fake<PortalDbContext>();
            A.CallTo(() => contextFake.CompanyUsers).Returns(companyUsersFakeDbSet);
            A.CallTo(() => contextFake.CompanyUserAssignedAppFavourites).Returns(appFavouritesFakeDbSet);
            A.CallTo(() => contextFake.SaveChangesAsync(A<CancellationToken>._)).ReturnsLazily(_ => Task.FromResult(appFavourites.Count()));
            _fixture.Inject(contextFake);

            var sut = _fixture.Create<AppsBusinessLogic>();

            // Act
            await sut.AddFavouriteAppForUserAsync(appId, iamUser.UserEntityId);

            // Assert
            appFavourites.Should().NotBeNullOrEmpty();
            appFavourites.Should().HaveCount(1);
            appFavourites.Single().AppId.Should().Be(appId);
            appFavourites.Single().CompanyUserId.Should().Be(companyUser.Id);
        }

        [Fact]
        public async void RemoveFavouriteAppForUser_ExecutesSuccessfully()
        {
            // Arrange
            var (companyUser, iamUser) = CreateTestUserPair();
            var companyUsersFakeDbSet = new List<CompanyUser> { companyUser }.AsFakeDbSet();

            var appId = _fixture.Create<Guid>();
            var appFavourites = new List<CompanyUserAssignedAppFavourite> { 
                new CompanyUserAssignedAppFavourite(appId, companyUser.Id)
            };
            var appFavouritesFakeDbSet = appFavourites.AsFakeDbSet();

            var contextFake = A.Fake<PortalDbContext>();
            A.CallTo(() => contextFake.CompanyUsers).Returns(companyUsersFakeDbSet);
            A.CallTo(() => contextFake.CompanyUserAssignedAppFavourites).Returns(appFavouritesFakeDbSet);
            A.CallTo(() => contextFake.SaveChangesAsync(A<CancellationToken>._)).ReturnsLazily(_ => Task.FromResult(1));
            _fixture.Inject(contextFake);

            var sut = _fixture.Create<AppsBusinessLogic>();

            // Act
            await sut.RemoveFavouriteAppForUserAsync(appId, iamUser.UserEntityId);

            // Assert
            A.CallTo(() => appFavouritesFakeDbSet.Remove(
                A<CompanyUserAssignedAppFavourite>
                .That.Matches(f => f.AppId == appId && f.CompanyUserId == companyUser.Id)
            ))
            .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async void AddCompanyAppSubscription_ExecutesSuccessfully()
        {
            // Arrange
            var appId = _fixture.Create<Guid>();
            var appSubscriptions = new List<CompanyAssignedApp>();
            var appSubscriptionsFakeDbSet = appSubscriptions.AsFakeDbSet();

            var (companyUser, iamUser) = CreateTestUserPair();
            var companyUsersFakeDbSet = new List<CompanyUser> { companyUser }.AsFakeDbSet();

            var contextFake = A.Fake<PortalDbContext>();
            A.CallTo(() => contextFake.CompanyUsers).Returns(companyUsersFakeDbSet);
            A.CallTo(() => contextFake.CompanyAssignedApps).Returns(appSubscriptionsFakeDbSet);
            A.CallTo(() => contextFake.SaveChangesAsync(A<CancellationToken>._)).ReturnsLazily(_ => Task.FromResult(appSubscriptions.Count()));
            _fixture.Inject(contextFake);

            var sut = _fixture.Create<AppsBusinessLogic>();

            // Act
            await sut.AddCompanyAppSubscriptionAsync(appId, iamUser.UserEntityId);

            // Assert
            appSubscriptions.Should().NotBeNullOrEmpty();
            appSubscriptions.Should().HaveCount(1);
            appSubscriptions.Single().AppId.Should().Be(appId);
            appSubscriptions.Single().CompanyId.Should().Be(companyUser.CompanyId);
        }

        [Fact]
        public async void GetBusinessApps_ReturnsAppListSuccessfully()
        {
            // Arrange
            var expectedApp = _fixture.Create<PortalBackend.PortalEntities.Entities.App>();
            var (companyUser, iamUser) = CreateTestUserPair();
            companyUser.Company!.BoughtApps.Add(expectedApp);
            foreach (var app in _fixture.CreateMany<PortalBackend.PortalEntities.Entities.App>())
            {
                companyUser.Company.BoughtApps.Add(app);
            }

            var iamClient = _fixture.Create<IamClient>();
            iamClient.Apps.Add(expectedApp);
            foreach (var app in _fixture.CreateMany<PortalBackend.PortalEntities.Entities.App>())
            {
                iamClient.Apps.Add(app);
            }

            foreach (var role in _fixture.Build<UserRole>().With(r => r.IamClient, iamClient).CreateMany())
            {
                companyUser.UserRoles.Add(role);
            }

            var iamUserFakeDbSet = new List<IamUser>() { iamUser }.AsFakeDbSet();

            var contextFake = A.Fake<PortalDbContext>();
            A.CallTo(() => contextFake.IamUsers).Returns(iamUserFakeDbSet);
            _fixture.Inject(contextFake);

            var sut = _fixture.Create<AppsBusinessLogic>();

            // Act
            var result = await sut.GetAllUserUserBusinessAppsAsync(iamUser.UserEntityId).ToListAsync();

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(1);
            result.Single().Id.Should().Be(expectedApp.Id);
        }

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
    }
}
