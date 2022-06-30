using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using CatenaX.NetworkServices.App.Service.BusinessLogic;
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
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using Xunit;

namespace CatenaX.NetworkServices.App.Service.Tests
{
    public class AppBusinessLogicTests
    {
        private readonly IFixture _fixture;
        private readonly IPortalRepositories _portalRepositories;
        private readonly IAppRepository _appRepository;
        private readonly ICompanyAssignedAppsRepository _companyAssignedAppsRepository;
        private readonly ICompanyUserAssignedAppFavouritesRepository _companyUserAssignedAppFavourites;
        private readonly IUserRepository _userRepository;

        public AppBusinessLogicTests()
        {
            _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
            _portalRepositories = A.Fake<IPortalRepositories>();
            _companyAssignedAppsRepository = A.Fake<ICompanyAssignedAppsRepository>();
            _companyUserAssignedAppFavourites = A.Fake<ICompanyUserAssignedAppFavouritesRepository>();
            _appRepository = A.Fake<IAppRepository>();
            _userRepository = A.Fake<IUserRepository>();
        }

        [Fact]
        public async void AddFavouriteAppForUser_ExecutesSuccessfully()
        {
            // Arrange
            var appId = _fixture.Create<Guid>();
            var (companyUser, iamUser) = CreateTestUserPair();
            A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(iamUser.UserEntityId))
                .Returns(companyUser.Id);
            A.CallTo(() => _portalRepositories.GetInstance<ICompanyUserAssignedAppFavouritesRepository>()).Returns(_companyUserAssignedAppFavourites);
            A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
            _fixture.Inject(_portalRepositories);
            
            var sut = _fixture.Create<AppsBusinessLogic>();

            // Act
            await sut.AddFavouriteAppForUserAsync(appId, iamUser.UserEntityId);

            // Assert
            A.CallTo(() => _companyUserAssignedAppFavourites.AddAppFavourite(A<CompanyUserAssignedAppFavourite>.That.Matches(x => x.AppId == appId && x.CompanyUserId == companyUser.Id))).MustHaveHappenedOnceExactly();
            A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async void RemoveFavouriteAppForUser_ExecutesSuccessfully()
        {
            // Arrange
            var (companyUser, iamUser) = CreateTestUserPair();

            var appId = _fixture.Create<Guid>();
            A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(iamUser.UserEntityId))
                .Returns(companyUser.Id);
            A.CallTo(() => _portalRepositories.GetInstance<ICompanyUserAssignedAppFavouritesRepository>()).Returns(_companyUserAssignedAppFavourites);
            A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
            _fixture.Inject(_portalRepositories);

            var sut = _fixture.Create<AppsBusinessLogic>();

            // Act
            await sut.RemoveFavouriteAppForUserAsync(appId, iamUser.UserEntityId);

            // Assert
            A.CallTo(() => _companyUserAssignedAppFavourites.RemoveFavouriteAppForUser(appId, companyUser.Id)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        }

        //
        // [Fact]
        // public async void RemoveFavouriteAppForUser_WithNotExistingApp_ThrowsError()
        // {
        //     // Arrange
        //     var (companyUser, iamUser) = CreateTestUserPair();
        //
        //     var appId = _fixture.Create<Guid>();
        //     A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(iamUser.UserEntityId))
        //         .Returns(companyUser.Id);
        //     A.CallTo(() => _companyUserAssignedAppFavourites.RemoveFavouriteAppForUser(appId, companyUser.Id))
        //         .Throws(() => new DbUpdateConcurrencyException());
        //     A.CallTo(() => _portalRepositories.GetInstance<ICompanyUserAssignedAppFavouritesRepository>()).Returns(_companyUserAssignedAppFavourites);
        //     A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        //     _fixture.Inject(_portalRepositories);
        //
        //     var sut = _fixture.Create<AppsBusinessLogic>();
        //
        //     // Act
        //     await sut.RemoveFavouriteAppForUserAsync(appId, iamUser.UserEntityId);
        //
        //     // Assert
        //     A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        // }

        [Fact]
        public async void AddCompanyAppSubscription_ExecutesSuccessfully()
        {
            // Arrange
            var appId = _fixture.Create<Guid>();
            var appName = "Test App";
            var providerName = "New Provider";
            var providerContactEmail = "email@provider.com";
            var (companyUser, iamUser) = CreateTestUserPair();

            A.CallTo(() => _userRepository.GetCompanyIdForIamUserUntrackedAsync(iamUser.UserEntityId))
                .Returns(companyUser.CompanyId);
            A.CallTo(() => _appRepository.GetAppProviderDetailsAsync(appId))
                .Returns(((string appName, string providerName, string providerContactEmail))new ValueTuple<string, string, string>(appName, providerName, providerContactEmail));
            A.CallTo(() => _portalRepositories.GetInstance<ICompanyAssignedAppsRepository>()).Returns(_companyAssignedAppsRepository);
            A.CallTo(() => _portalRepositories.GetInstance<IAppRepository>()).Returns(_appRepository);
            A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
            _fixture.Inject(_portalRepositories);
            var mailingService = A.Fake<IMailingService>();
            _fixture.Inject(mailingService);

            var sut = _fixture.Create<AppsBusinessLogic>();

            // Act
            await sut.AddCompanyAppSubscriptionAsync(appId, iamUser.UserEntityId);

            // Assert
            A.CallTo(() => _companyAssignedAppsRepository.AddCompanyAssignedApp(A<CompanyAssignedApp>._)).MustHaveHappened(1, Times.Exactly);
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
