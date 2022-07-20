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
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.Tests.Shared;
using FluentAssertions;
using Xunit;

namespace CatenaX.NetworkServices.App.Service.Tests
{
    public class AppBusinessLogicTests
    {
        private readonly IFixture _fixture;
        private readonly IPortalRepositories _portalRepositories;
        private readonly IAppRepository _appRepository;
        private readonly ICompanyAssignedAppsRepository _companyAssignedAppsRepository;
        private readonly IUserRepository _userRepository;

        public AppBusinessLogicTests()
        {
            _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
            _portalRepositories = A.Fake<IPortalRepositories>();
            _companyAssignedAppsRepository = A.Fake<ICompanyAssignedAppsRepository>();
            _appRepository = A.Fake<IAppRepository>();
            _userRepository = A.Fake<IUserRepository>();
        }

        [Fact]
        public async Task AddFavouriteAppForUser_ExecutesSuccessfully()
        {
            // Arrange
            var appId = _fixture.Create<Guid>();
            var (companyUser, iamUser) = CreateTestUserPair();
            A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(iamUser.UserEntityId))
                .Returns(companyUser.Id);
            A.CallTo(() => _portalRepositories.GetInstance<IAppRepository>()).Returns(_appRepository);
            A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
            _fixture.Inject(_portalRepositories);

            var sut = _fixture.Create<AppsBusinessLogic>();

            // Act
            await sut.AddFavouriteAppForUserAsync(appId, iamUser.UserEntityId);

            // Assert
            A.CallTo(() => _appRepository.CreateAppFavourite(A<Guid>.That.Matches(x => x == appId), A<Guid>.That.Matches(x => x == companyUser.Id))).MustHaveHappenedOnceExactly();
            A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task RemoveFavouriteAppForUser_ExecutesSuccessfully()
        {
            // Arrange
            var (companyUser, iamUser) = CreateTestUserPair();

            var appId = _fixture.Create<Guid>();
            A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(iamUser.UserEntityId))
                .Returns(companyUser.Id);
            A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
            _fixture.Inject(_portalRepositories);

            var sut = _fixture.Create<AppsBusinessLogic>();

            // Act
            await sut.RemoveFavouriteAppForUserAsync(appId, iamUser.UserEntityId);

            // Assert
            A.CallTo(() => _portalRepositories.Remove(A<CompanyUserAssignedAppFavourite>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task AddCompanyAppSubscription_ExecutesSuccessfully()
        {
            // Arrange
            var appId = _fixture.Create<Guid>();
            var appName = "Test App";
            var providerName = "New Provider";
            var providerContactEmail = "email@provider.com";
            var (companyUser, iamUser) = CreateTestUserPair();

            A.CallTo(() => _companyAssignedAppsRepository.GetCompanyIdWithAssignedAppForCompanyUserAsync(appId, iamUser.UserEntityId))
                .Returns(((Guid companyId, CompanyAssignedApp? companyAssignedApp)) new (companyUser.CompanyId, null));
            A.CallTo(() => _companyAssignedAppsRepository.CreateCompanyAssignedApp(appId, companyUser.CompanyId, AppSubscriptionStatusId.PENDING))
                .Returns(new CompanyAssignedApp(appId, companyUser.CompanyId, AppSubscriptionStatusId.PENDING));
            A.CallTo(() => _appRepository.GetAppProviderDetailsAsync(appId))
                .Returns(((string appName, string providerName, string providerContactEmail)) new (appName, providerName, providerContactEmail));
            A.CallTo(() => _portalRepositories.GetInstance<ICompanyAssignedAppsRepository>()).Returns(_companyAssignedAppsRepository);
            A.CallTo(() => _portalRepositories.GetInstance<IAppRepository>()).Returns(_appRepository);
            _fixture.Inject(_portalRepositories);
            var mailingService = A.Fake<IMailingService>();
            _fixture.Inject(mailingService);

            var sut = _fixture.Create<AppsBusinessLogic>();

            // Act
            await sut.AddOwnCompanyAppSubscriptionAsync(appId, iamUser.UserEntityId);

            // Assert
            A.CallTo(() => _companyAssignedAppsRepository.CreateCompanyAssignedApp(A<Guid>._, A<Guid>._, A<AppSubscriptionStatusId>._)).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => mailingService.SendMails(providerContactEmail, A<Dictionary<string, string>>._, A<List<string>>._)).MustHaveHappened(1, Times.Exactly);
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
