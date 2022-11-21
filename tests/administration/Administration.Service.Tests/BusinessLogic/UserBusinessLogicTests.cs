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
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Models;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.Mailing.SendMail;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Service;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic.Tests;

public class UserBusinessLogicTests
{
    private readonly IFixture _fixture;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityProviderRepository _identityProviderRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IUserBusinessPartnerRepository _userBusinessPartnerRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IMailingService _mailingService;
    private readonly IMockLogger<UserBusinessLogic> _mockLogger;
    private readonly ILogger<UserBusinessLogic> _logger;
    private readonly IOptions<UserSettings> _options;
    private readonly CompanyUser _companyUser;
    private readonly Guid _identityProviderId;
    private readonly string _iamUserId;
    private readonly string _adminIamUser;
    private readonly Guid _companyUserId;
    private readonly Guid _companyId;
    private readonly Guid _validOfferId;
    private readonly Guid _noTargetIamUserSet;
    private readonly string _createdCentralUserId;
    private readonly string _displayName;
    private readonly ICollection<CompanyUserAssignedRole> _companyUserAssignedRole = new HashSet<CompanyUserAssignedRole>();
    private readonly Func<UserCreationRoleDataIdpInfo,(Guid CompanyUserId, string UserName, string? Password, Exception? Error)> _processLine;
    private readonly Func<CompanyUserAccountData,CompanyUserAccountData> _companyUserSelectFunction;
    private readonly Exception _error;
    private readonly Random _random;

    public UserBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _random = new Random();

        _provisioningManager = A.Fake<IProvisioningManager>();
        _userProvisioningService = A.Fake<IUserProvisioningService>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _companyUser = A.Fake<CompanyUser>();
        _offerRepository = A.Fake<IOfferRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _userBusinessPartnerRepository = A.Fake<IUserBusinessPartnerRepository>();
        _applicationRepository = A.Fake<IApplicationRepository>();

        _mailingService = A.Fake<IMailingService>();
        _mockLogger = A.Fake<IMockLogger<UserBusinessLogic>>();
        _logger = new MockLogger<UserBusinessLogic>(_mockLogger);
        _options = _fixture.Create<IOptions<UserSettings>>();

        _identityProviderId = _fixture.Create<Guid>();
        _iamUserId = _fixture.Create<string>();
        _adminIamUser = _fixture.Create<string>();
        _companyUserId = _fixture.Create<Guid>();
        _companyId = _fixture.Create<Guid>();
        _validOfferId = _fixture.Create<Guid>();
        _noTargetIamUserSet = _fixture.Create<Guid>();
        _createdCentralUserId = _fixture.Create<string>();
        _displayName = _fixture.Create<string>();

        _processLine = A.Fake<Func<UserCreationRoleDataIdpInfo,(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>>();
        _companyUserSelectFunction = A.Fake<Func<CompanyUserAccountData,CompanyUserAccountData>>();

        _error = _fixture.Create<TestException>();
    }

    #region CreateOwnCompanyUsersAsync

    [Fact]
    public async void TestUserCreationAllSuccess()
    {
        SetupFakesForUserCreation(true);

        var userList = _fixture.Create<IEnumerable<UserCreationInfo>>();

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            _portalRepositories,
            _mailingService,
            _logger,
            _options);

        var result = await sut.CreateOwnCompanyUsersAsync(userList, _iamUserId).ToListAsync().ConfigureAwait(false);

        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Error), A<Exception?>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<IDictionary<string,string>>._,A<List<string>>._)).MustHaveHappenedANumberOfTimesMatching(times => times == userList.Count());

        result.Should().NotBeNull();
        result.Should().HaveSameCount(userList);
        result.Should().Match(r => r.SequenceEqual(userList.Select(u => u.eMail)));
    }

    [Fact]
    public async void TestUserCreationCreationError()
    {
        SetupFakesForUserCreation(true);

        var userCreationInfo = _fixture.Create<UserCreationInfo>();
        var error = _fixture.Create<TestException>();

        var userList = new [] {
            _fixture.Create<UserCreationInfo>(),
            _fixture.Create<UserCreationInfo>(),
            userCreationInfo,
            _fixture.Create<UserCreationInfo>(),
            _fixture.Create<UserCreationInfo>()
        };

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Matches(u =>
            u.FirstName == userCreationInfo.firstName &&
            u.LastName == userCreationInfo.lastName &&
            u.Email == userCreationInfo.eMail            
        ))).ReturnsLazily(
            (UserCreationRoleDataIdpInfo creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                .With(x => x.UserName, creationInfo.UserName)
                .With(x => x.Error, error)
                .Create());

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            _portalRepositories,
            _mailingService,
            _logger,
            _options);

        var result = await sut.CreateOwnCompanyUsersAsync(userList, _iamUserId).ToListAsync().ConfigureAwait(false);

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Matches(u =>
            u.FirstName == userCreationInfo.firstName &&
            u.LastName == userCreationInfo.lastName &&
            u.Email == userCreationInfo.eMail            
        ))).MustHaveHappenedOnceExactly();

        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Error), A<Exception?>._, A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<IDictionary<string,string>>._,A<List<string>>._)).MustHaveHappenedANumberOfTimesMatching(times => times == 4);

        result.Should().NotBeNull();
        result.Should().HaveCount(4);
    }

    [Fact]
    public async void TestUserCreationCreationThrows()
    {
        SetupFakesForUserCreation(true);

        var userCreationInfo = _fixture.Create<UserCreationInfo>();
        var expected = _fixture.Create<TestException>();

        var userList = new [] {
            _fixture.Create<UserCreationInfo>(),
            _fixture.Create<UserCreationInfo>(),
            userCreationInfo,
            _fixture.Create<UserCreationInfo>(),
            _fixture.Create<UserCreationInfo>()
        };

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Matches(u =>
            u.FirstName == userCreationInfo.firstName &&
            u.LastName == userCreationInfo.lastName &&
            u.Email == userCreationInfo.eMail            
        ))).Throws(() => expected);

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            _portalRepositories,
            _mailingService,
            _logger,
            _options);

        async Task Act() => await sut.CreateOwnCompanyUsersAsync(userList, _iamUserId).ToListAsync().ConfigureAwait(false);

        var error = await Assert.ThrowsAsync<TestException>(Act).ConfigureAwait(false);

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Matches(u =>
            u.FirstName == userCreationInfo.firstName &&
            u.LastName == userCreationInfo.lastName &&
            u.Email == userCreationInfo.eMail            
        ))).MustHaveHappenedOnceExactly();

        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Error), A<Exception?>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<IDictionary<string,string>>._,A<List<string>>._)).MustHaveHappenedANumberOfTimesMatching(times => times == 2);

        error.Should().BeSameAs(expected);
    }

    [Fact]
    public async void TestUserCreationSendMailError()
    {
        SetupFakesForUserCreation(true);

        var userCreationInfo = _fixture.Create<UserCreationInfo>();
        var error = _fixture.Create<TestException>();

        var userList = new [] {
            _fixture.Create<UserCreationInfo>(),
            _fixture.Create<UserCreationInfo>(),
            userCreationInfo,
            _fixture.Create<UserCreationInfo>(),
            _fixture.Create<UserCreationInfo>()
        };

        A.CallTo(() => _mailingService.SendMails(A<string>.That.IsEqualTo(userCreationInfo.eMail),A<IDictionary<string,string>>._,A<List<string>>._))
            .Throws(error);

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            _portalRepositories,
            _mailingService,
            _logger,
            _options);

        var result = await sut.CreateOwnCompanyUsersAsync(userList, _iamUserId).ToListAsync().ConfigureAwait(false);

        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Error), A<Exception?>._, A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<IDictionary<string,string>>._,A<List<string>>._)).MustHaveHappenedANumberOfTimesMatching(times => times == 5);

        result.Should().NotBeNull();
        result.Should().HaveCount(5);
    }

    #endregion

    #region CreateOwnCompanyIdpUserAsync

    [Fact]
    public async void TestCreateOwnCompanyIdpUserAsyncSuccess()
    {
        SetupFakesForUserCreation(false);

        var userCreationInfoIdp = _fixture.Create<UserCreationInfoIdp>();

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            null!,
            _mailingService,
            _logger,
            _options);

        var result = await sut.CreateOwnCompanyIdpUserAsync(_identityProviderId, userCreationInfoIdp, _iamUserId).ConfigureAwait(false);
        result.Should().NotBe(Guid.Empty);
        A.CallTo(() => _mailingService.SendMails(A<string>.That.IsEqualTo(userCreationInfoIdp.Email),A<IDictionary<string,string>>.That.Matches(x => x["companyName"] == _displayName),A<IEnumerable<string>>._)).MustHaveHappened();
        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Error), A<Exception?>._, A<string>._)).MustNotHaveHappened();        
    }

    [Fact]
    public async void TestCreateOwnCompanyIdpUserAsyncError()
    {
        SetupFakesForUserCreation(false);

        var userCreationInfoIdp = _fixture.Create<UserCreationInfoIdp>();

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Matches(u => u.FirstName == userCreationInfoIdp.FirstName))).ReturnsLazily(
            (UserCreationRoleDataIdpInfo creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                .With(x => x.UserName, creationInfo.UserName)
                .With(x => x.Error, _error)
                .Create());

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            null!,
            _mailingService,
            _logger,
            _options);

        Task Act() => sut.CreateOwnCompanyIdpUserAsync(_identityProviderId, userCreationInfoIdp, _iamUserId);

        var error = await Assert.ThrowsAsync<TestException>(Act).ConfigureAwait(false);
        error.Message.Should().Be(_error.Message);
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<IDictionary<string,string>>._,A<IEnumerable<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async void TestCreateOwnCompanyIdpUserAsyncThrows()
    {
        SetupFakesForUserCreation(false);

        var userCreationInfoIdp = _fixture.Create<UserCreationInfoIdp>();

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Matches(u => u.FirstName == userCreationInfoIdp.FirstName))).Throws(_error);

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            null!,
            _mailingService,
            _logger,
            _options);

        Task Act() => sut.CreateOwnCompanyIdpUserAsync(_identityProviderId, userCreationInfoIdp, _iamUserId);

        var error = await Assert.ThrowsAsync<TestException>(Act).ConfigureAwait(false);
        error.Message.Should().Be(_error.Message);
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<IDictionary<string,string>>._,A<IEnumerable<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async void TestCreateOwnCompanyIdpUserAsyncMailingErrorLogs()
    {
        SetupFakesForUserCreation(false);

        var userCreationInfoIdp = _fixture.Create<UserCreationInfoIdp>();

        A.CallTo(() => _mailingService.SendMails(A<string>._,A<IDictionary<string,string>>._,A<IEnumerable<string>>._)).Throws(_error);

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            null!,
            _mailingService,
            _logger,
            _options);

        var result = await sut.CreateOwnCompanyIdpUserAsync(_identityProviderId, userCreationInfoIdp, _iamUserId).ConfigureAwait(false);
        result.Should().NotBe(Guid.Empty);
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<IDictionary<string,string>>._,A<IEnumerable<string>>._)).MustHaveHappened();
        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Error), A<Exception?>._, A<string>._)).MustHaveHappened();        
    }

    #endregion

    #region DeleteOwnUserAsync

    [Fact]
    public async void TestDeleteOwnUserSuccess()
    {
        SetupFakesForUserDeletion();

        var sut = new UserBusinessLogic(
            _provisioningManager,
            null!,
            null!,
            _portalRepositories,
            null!,
            _logger,
            _options
        );

        var result = await sut.DeleteOwnUserAsync(_companyUserId, _iamUserId).ConfigureAwait(false);

        A.CallTo(() => _provisioningManager.GetProviderUserIdForCentralUserIdAsync(A<string>._,A<string>._)).MustHaveHappened();
        A.CallTo(() => _provisioningManager.DeleteSharedRealmUserAsync(A<string>._,A<string>._)).MustHaveHappened();
        A.CallTo(() => _provisioningManager.DeleteCentralRealmUserAsync(A<string>._)).MustHaveHappened();
        A.CallTo(() => _userBusinessPartnerRepository.DeleteCompanyUserAssignedBusinessPartners(A<IEnumerable<(Guid,string)>>._)).MustHaveHappened();
        A.CallTo(() => _offerRepository.DeleteAppFavourites(A<IEnumerable<(Guid,Guid)>>._)).MustHaveHappened();
        A.CallTo(() => _userRolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid,Guid)>>._)).MustHaveHappened();
        A.CallTo(() => _applicationRepository.DeleteInvitations(A<IEnumerable<Guid>>._)).MustHaveHappened();
        A.CallTo(() => _userRepository.DeleteIamUser(A<string>._)).MustHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened();

        _companyUser.CompanyUserStatusId.Should().Be(CompanyUserStatusId.DELETED);
        _companyUser.LastEditorId.Should().Be(_companyUserId);
    }

    [Fact]
    public async void TestDeleteOwnUserInvalidUserThrows()
    {
        SetupFakesForUserDeletion();

        var iamUserId = _fixture.Create<string>();

        A.CallTo(() => _userRepository.GetSharedIdentityProviderUserAccountDataUntrackedAsync(A<string>.That.IsEqualTo(iamUserId))).Returns(((string?,CompanyUserAccountData))default!);
        
        var sut = new UserBusinessLogic(
            _provisioningManager,
            null!,
            null!,
            _portalRepositories,
            null!,
            _logger,
            _options
        );

        Task Act() => sut.DeleteOwnUserAsync(_companyUserId, iamUserId);

        var error = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"iamUser {iamUserId} is not associated with any companyUser");

        A.CallTo(() => _provisioningManager.DeleteSharedRealmUserAsync(A<string>._,A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.DeleteCentralRealmUserAsync(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _userRolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid,Guid)>>._)).MustNotHaveHappened();
        A.CallTo(() => _userRepository.DeleteIamUser(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async void TestDeleteOwnUserInvalidCompanyUserThrows()
    {
        SetupFakesForUserDeletion();

        var iamUserId = _fixture.Create<string>();

        var sut = new UserBusinessLogic(
            _provisioningManager,
            null!,
            null!,
            _portalRepositories,
            null!,
            _logger,
            _options
        );

        Task Act() => sut.DeleteOwnUserAsync(_companyUserId, iamUserId);

        var error = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"invalid companyUserId {_companyUserId} for user {iamUserId}");

        A.CallTo(() => _provisioningManager.DeleteSharedRealmUserAsync(A<string>._,A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.DeleteCentralRealmUserAsync(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _userRolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid,Guid)>>._)).MustNotHaveHappened();        
        A.CallTo(() => _userRepository.DeleteIamUser(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async void TestDeleteOwnUserErrorDeletingThrows()
    {
        SetupFakesForUserDeletion();
        
        A.CallTo(() => _userRepository.DeleteIamUser(A<string>._)).Throws(_error);

        var sut = new UserBusinessLogic(
            _provisioningManager,
            null!,
            null!,
            _portalRepositories,
            null!,
            _logger,
            _options
        );

        Task Act() => sut.DeleteOwnUserAsync(_companyUserId, _iamUserId);

        var error = await Assert.ThrowsAsync<TestException>(Act).ConfigureAwait(false);
        error.Message.Should().Be(_error.Message);

        A.CallTo(() => _provisioningManager.DeleteSharedRealmUserAsync(A<string>._,A<string>._)).MustHaveHappened();
        A.CallTo(() => _provisioningManager.DeleteCentralRealmUserAsync(A<string>._)).MustHaveHappened();
        A.CallTo(() => _userRolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid,Guid)>>._)).MustNotHaveHappened();        
        A.CallTo(() => _userRepository.DeleteIamUser(A<string>._)).MustHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    #endregion

    #region Modify UserRole Async

    [Fact]
    public async Task ModifyUserRoleAsync_WithTwoNewRoles_AddsTwoRolesToTheDatabase()
    {
        // Arrange
        SetupFakesForUserRoleModification();

        var sut = new UserRolesBusinessLogic(
            _portalRepositories,
            _provisioningManager
        );

        // Act
        var userRoleInfo = new UserRoleInfo(_companyUserId, new []
        {
            "Existing Role",
            "Company Admin",
            "Buyer",
            "Supplier"
        });
        await sut.ModifyUserRoleAsync(_validOfferId, userRoleInfo, _createdCentralUserId).ConfigureAwait(false);

        // Assert
        _companyUserAssignedRole.Should().HaveCount(2);
    }

    [Fact]
    public async Task ModifyUserRoleAsync_WithOneRoleToDelete_DeletesTheRole()
    {
        // Arrange
        SetupFakesForUserRoleModification();

        var sut = new UserRolesBusinessLogic(
            _portalRepositories,
            _provisioningManager
        );

        // Act
        var userRoleInfo = new UserRoleInfo(_companyUserId, new []
        {
            "Company Admin"
        });
        await sut.ModifyUserRoleAsync(_validOfferId, userRoleInfo, _adminIamUser).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalRepositories.RemoveRange(A<IEnumerable<CompanyUserAssignedRole>>.That.Matches(x => x.Count() == 1))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ModifyUserRoleAsync_WithNotExistingRole_ThrowsException()
    {
        // Arrange
        SetupFakesForUserRoleModification();

        var sut = new UserRolesBusinessLogic(
            _portalRepositories,
            _provisioningManager
        );

        // Act
        var userRoleInfo = new UserRoleInfo(_companyUserId, new []
        {
            "NotExisting",
            "Buyer"
        });
        async Task Action() => await sut.ModifyUserRoleAsync(_validOfferId, userRoleInfo, _adminIamUser).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("roles");
    }

    [Fact]
    public async Task ModifyUserRoleAsync_WithNotFoundCompanyUser_ThrowsException()
    {
        // Arrange
        SetupFakesForUserRoleModification();

        var sut = new UserRolesBusinessLogic(
            _portalRepositories,
            _provisioningManager
        );

        // Act
        var userRoleInfo = new UserRoleInfo(Guid.NewGuid(), new []
        {
            "Company Admin",
            "Buyer",
            "Supplier"
        });
        async Task Action() => await sut.ModifyUserRoleAsync(_validOfferId, userRoleInfo, _adminIamUser).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Action);
        ex.Message.Should().Be($"CompanyUserId {userRoleInfo.CompanyUserId} is not associated with the same company as adminUserId {_adminIamUser}");
    }

    [Fact]
    public async Task ModifyUserRoleAsync_WithInvalidOfferId_ThrowsException()
    {
        // Arrange
        SetupFakesForUserRoleModification();

        var sut = new UserRolesBusinessLogic(
            _portalRepositories,
            _provisioningManager
        );
        var invalidAppId = Guid.NewGuid();

        // Act
        var userRoleInfo = new UserRoleInfo(_companyUserId, new []
        {
            "Company Admin",
            "Buyer",
            "Supplier"
        });
        async Task Action() => await sut.ModifyUserRoleAsync(invalidAppId, userRoleInfo, _adminIamUser).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().StartWith("offerId");
    }

    #endregion

    #region ModifyAppUserRolesAsync

    [Fact]
    public async Task ModifyAppUserRolesAsync_WithTwoNewRoles_AddsTwoRolesToTheDatabase()
    {
        // Arrange
        SetupFakesForUserRoleModification();

        var sut = new UserRolesBusinessLogic(
            _portalRepositories,
            _provisioningManager
        );

        // Act
        var userRoles = new []
        {
            "Existing Role",
            "Company Admin",
            "Buyer",
            "Supplier"
        };
        await sut.ModifyAppUserRolesAsync(_validOfferId, _companyUserId, userRoles, _createdCentralUserId).ConfigureAwait(false);

        // Assert
        _companyUserAssignedRole.Should().HaveCount(2);
    }

    [Fact]
    public async Task ModifyAppUserRolesAsync_WithOneRoleToDelete_DeletesTheRole()
    {
        // Arrange
        SetupFakesForUserRoleModification();

        var sut = new UserRolesBusinessLogic(
            _portalRepositories,
            _provisioningManager
        );

        // Act
        var userRoles = new []
        {
            "Company Admin"
        };
        await sut.ModifyAppUserRolesAsync(_validOfferId, _companyUserId, userRoles, _adminIamUser).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalRepositories.RemoveRange(A<IEnumerable<CompanyUserAssignedRole>>.That.Matches(x => x.Count() == 1))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ModifyAppUserRolesAsync_WithNotExistingRole_ThrowsException()
    {
        // Arrange
        SetupFakesForUserRoleModification();

        var sut = new UserRolesBusinessLogic(
            _portalRepositories,
            _provisioningManager
        );

        // Act
        var userRoles = new []
        {
            "NotExisting",
            "Buyer"
        };
        async Task Action() => await sut.ModifyAppUserRolesAsync(_validOfferId, _companyUserId, userRoles, _adminIamUser).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("roles");
    }

    [Fact]
    public async Task ModifyAppUserRolesAsync_WithNotFoundCompanyUser_ThrowsException()
    {
        // Arrange
        SetupFakesForUserRoleModification();

        var sut = new UserRolesBusinessLogic(
            _portalRepositories,
            _provisioningManager
        );

        var companyUserId = _fixture.Create<Guid>();
        // Act
        var userRoles = new []
        {
            "Company Admin",
            "Buyer",
            "Supplier"
        };
        async Task Action() => await sut.ModifyAppUserRolesAsync(_validOfferId, companyUserId, userRoles, _adminIamUser).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Action);
        ex.Message.Should().Be($"CompanyUserId {companyUserId} is not associated with the same company as adminUserId {_adminIamUser}");
    }

    [Fact]
    public async Task ModifyAppUserRolesAsync_WithInvalidOfferId_ThrowsException()
    {
        // Arrange
        SetupFakesForUserRoleModification();

        var sut = new UserRolesBusinessLogic(
            _portalRepositories,
            _provisioningManager
        );
        var invalidAppId = Guid.NewGuid();

        // Act
        var userRoles = new []
        {
            "Company Admin",
            "Buyer",
            "Supplier"
        };
        async Task Action() => await sut.ModifyAppUserRolesAsync(invalidAppId, _companyUserId, userRoles, _adminIamUser).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().StartWith("offerId");
    }

    #endregion

    #region DeleteOwnCompanyUsersAsync

    [Fact]
    public async void TestDeleteOwnCompanyUsersAsyncSuccess()
    {
        SetupFakesForUserDeletion();

        var companyUserIds = _fixture.CreateMany<Guid>(5);

        var sut = new UserBusinessLogic(
            _provisioningManager,
            null!,
            null!,
            _portalRepositories,
            null!,
            _logger,
            _options
        );

        var result = await sut.DeleteOwnCompanyUsersAsync(companyUserIds,_iamUserId).ToListAsync().ConfigureAwait(false);

        var expectedCount = companyUserIds.Count();
        result.Should().HaveCount(expectedCount);
        result.Should().Match(r => Enumerable.SequenceEqual(r,companyUserIds));

        A.CallTo(() => _provisioningManager.DeleteSharedRealmUserAsync(A<string>._,A<string>._)).MustHaveHappenedANumberOfTimesMatching(n => n == expectedCount);
        A.CallTo(() => _provisioningManager.DeleteCentralRealmUserAsync(A<string>._)).MustHaveHappenedANumberOfTimesMatching(n => n == expectedCount);
        A.CallTo(() => _userRolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid,Guid)>>._)).MustHaveHappenedANumberOfTimesMatching(n => n == expectedCount);
        A.CallTo(() => _userRepository.DeleteIamUser(A<string>._)).MustHaveHappenedANumberOfTimesMatching(n => n == expectedCount);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened();
    }

    [Fact]
    public async void TestDeleteOwnCompanyUsersAsyncNoSharedIdpSuccess()
    {
        SetupFakesForUserDeletion();

        A.CallTo(() => _identityProviderRepository.GetSharedIdentityProviderIamAliasDataUntrackedAsync(A<string>._))
            .Returns((null,_companyUserId));

        var companyUserIds = _fixture.CreateMany<Guid>(5);

        var sut = new UserBusinessLogic(
            _provisioningManager,
            null!,
            null!,
            _portalRepositories,
            null!,
            _logger,
            _options
        );

        var result = await sut.DeleteOwnCompanyUsersAsync(companyUserIds,_iamUserId).ToListAsync().ConfigureAwait(false);

        var expectedCount = companyUserIds.Count();
        result.Should().HaveCount(expectedCount);
        result.Should().Match(r => Enumerable.SequenceEqual(r,companyUserIds));

        A.CallTo(() => _provisioningManager.GetProviderUserIdForCentralUserIdAsync(A<string>._,A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.DeleteSharedRealmUserAsync(A<string>._,A<string>._)).MustNotHaveHappened();

        A.CallTo(() => _provisioningManager.DeleteCentralRealmUserAsync(A<string>._)).MustHaveHappenedANumberOfTimesMatching(n => n == expectedCount);
        A.CallTo(() => _userRolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid,Guid)>>._)).MustHaveHappenedANumberOfTimesMatching(n => n == expectedCount);
        A.CallTo(() => _userRepository.DeleteIamUser(A<string>._)).MustHaveHappenedANumberOfTimesMatching(n => n == expectedCount);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened();
    }

    [Fact]
    public async void TestDeleteOwnCompanyUsersInvalidAdminUserThrows()
    {
        SetupFakesForUserDeletion();

        A.CallTo(() => _identityProviderRepository.GetSharedIdentityProviderIamAliasDataUntrackedAsync(A<string>._))
            .Returns(((string?,Guid))default);

        var companyUserIds = _fixture.CreateMany<Guid>(5);

        var sut = new UserBusinessLogic(
            _provisioningManager,
            null!,
            null!,
            _portalRepositories,
            null!,
            _logger,
            _options
        );

        async Task Act() => await sut.DeleteOwnCompanyUsersAsync(companyUserIds,_iamUserId).ToListAsync().ConfigureAwait(false);

        var error = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"iamUser {_iamUserId} is not associated with any companyUser");

        A.CallTo(() => _provisioningManager.DeleteSharedRealmUserAsync(A<string>._,A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.DeleteCentralRealmUserAsync(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _userRolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid,Guid)>>._)).MustNotHaveHappened();
        A.CallTo(() => _userRepository.DeleteIamUser(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async void TestDeleteOwnCompanyUsersAsyncError()
    {
        SetupFakesForUserDeletion();

        var sharedIdpAlias = _fixture.Create<string>();

        A.CallTo(() => _identityProviderRepository.GetSharedIdentityProviderIamAliasDataUntrackedAsync(A<string>._))
            .Returns((sharedIdpAlias,_companyUserId));

        var invalidUserId = _fixture.Create<Guid>();
        var invalidUserEntityId = _fixture.Create<string>();

        var companyUserIds = new [] {
            _fixture.Create<Guid>(),
            _fixture.Create<Guid>(),
            invalidUserId,
            _fixture.Create<Guid>(),
            _fixture.Create<Guid>()
        };

        A.CallTo(() => _companyUserSelectFunction(A<CompanyUserAccountData>.That.Matches(u => u.CompanyUserId == invalidUserId))).ReturnsLazily(
                (CompanyUserAccountData u) =>
                    new CompanyUserAccountData(
                        u.CompanyUserId,
                        invalidUserEntityId,
                        u.BusinessPartnerNumbers,
                        u.RoleIds,
                        u.OfferIds,
                        u.InvitationIds
                    ));

        A.CallTo(() => _provisioningManager.DeleteCentralRealmUserAsync(A<string>.That.IsEqualTo(invalidUserEntityId))).Throws(_error);

        var sut = new UserBusinessLogic(
            _provisioningManager,
            null!,
            null!,
            _portalRepositories,
            null!,
            _logger,
            _options
        );

        var result = await sut.DeleteOwnCompanyUsersAsync(companyUserIds,_iamUserId).ToListAsync().ConfigureAwait(false);

        result.Should().HaveCount(companyUserIds.Length-1);
        result.Should().Match(r => Enumerable.SequenceEqual(r,companyUserIds.Take(2).Concat(companyUserIds.Skip(3))));

        A.CallTo(() => _provisioningManager.DeleteSharedRealmUserAsync(A<string>._,A<string>._)).MustHaveHappenedANumberOfTimesMatching(n => n == companyUserIds.Length);
        A.CallTo(() => _provisioningManager.DeleteCentralRealmUserAsync(A<string>._)).MustHaveHappenedANumberOfTimesMatching(n => n == companyUserIds.Length);
        A.CallTo(() => _userRolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid,Guid)>>._)).MustHaveHappenedANumberOfTimesMatching(n => n == companyUserIds.Length-1);
        A.CallTo(() => _userRepository.DeleteIamUser(A<string>._)).MustHaveHappenedANumberOfTimesMatching(n => n == companyUserIds.Length-1);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened();

        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Error),_error,A<string>.That.IsEqualTo($"Error while deleting companyUser {invalidUserId} from shared idp {sharedIdpAlias}"))).MustHaveHappened();
    }

    [Fact]
    public async void TestDeleteOwnCompanyUsersAsyncNoSharedIdpError()
    {
        SetupFakesForUserDeletion();

        A.CallTo(() => _identityProviderRepository.GetSharedIdentityProviderIamAliasDataUntrackedAsync(A<string>._))
            .Returns((null,_companyUserId));

        var invalidUserId = _fixture.Create<Guid>();
        var invalidUserEntityId = _fixture.Create<string>();

        var companyUserIds = new [] {
            _fixture.Create<Guid>(),
            _fixture.Create<Guid>(),
            invalidUserId,
            _fixture.Create<Guid>(),
            _fixture.Create<Guid>()
        };

        A.CallTo(() => _companyUserSelectFunction(A<CompanyUserAccountData>.That.Matches(u => u.CompanyUserId == invalidUserId))).ReturnsLazily(
                (CompanyUserAccountData u) => new CompanyUserAccountData(
                    u.CompanyUserId,
                    invalidUserEntityId,
                    u.BusinessPartnerNumbers,
                    u.RoleIds,
                    u.OfferIds,
                    u.InvitationIds
                ));

        A.CallTo(() => _provisioningManager.DeleteCentralRealmUserAsync(A<string>.That.IsEqualTo(invalidUserEntityId))).Throws(_error);

        var sut = new UserBusinessLogic(
            _provisioningManager,
            null!,
            null!,
            _portalRepositories,
            null!,
            _logger,
            _options
        );

        var result = await sut.DeleteOwnCompanyUsersAsync(companyUserIds,_iamUserId).ToListAsync().ConfigureAwait(false);

        result.Should().HaveCount(companyUserIds.Length-1);
        result.Should().Match(r => Enumerable.SequenceEqual(r,companyUserIds.Take(2).Concat(companyUserIds.Skip(3))));

        A.CallTo(() => _provisioningManager.GetProviderUserIdForCentralUserIdAsync(A<string>._,A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.DeleteSharedRealmUserAsync(A<string>._,A<string>._)).MustNotHaveHappened();

        A.CallTo(() => _provisioningManager.DeleteCentralRealmUserAsync(A<string>._)).MustHaveHappenedANumberOfTimesMatching(n => n == companyUserIds.Length);
        A.CallTo(() => _userRolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid,Guid)>>._)).MustHaveHappenedANumberOfTimesMatching(n => n == companyUserIds.Length-1);
        A.CallTo(() => _userRepository.DeleteIamUser(A<string>._)).MustHaveHappenedANumberOfTimesMatching(n => n == companyUserIds.Length-1);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened();

        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Error),_error,A<string>.That.IsEqualTo($"Error while deleting companyUser {invalidUserId}"))).MustHaveHappened();
    }

    #endregion

    #region GetOwnCompanyAppUsers

    [Fact]
    public async Task GetOwnCompanyAppUsersAsync_ReturnsExpectedResult()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var iamUserId = _fixture.Create<string>();
        var companyUsers = _fixture.CreateMany<CompanyAppUserDetails>(5);

        A.CallTo(() => _userRepository.GetOwnCompanyAppUsersPaginationSourceAsync(A<Guid>._, A<string>._, A<IEnumerable<OfferSubscriptionStatusId>>._, A<IEnumerable<CompanyUserStatusId>>._, A<CompanyUserFilter>._))
            .Returns((int skip, int take) => Task.FromResult((Pagination.Source<CompanyAppUserDetails>?)new Pagination.Source<CompanyAppUserDetails>(companyUsers.Count(), companyUsers.Skip(skip).Take(take))));
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        var sut = new UserBusinessLogic(null!, null!, null!, _portalRepositories, null!, null!, A.Fake<IOptions<UserSettings>>());
        
        // Act
        var results = await sut.GetOwnCompanyAppUsersAsync(
            appId,
            iamUserId, 
            0,
            10,
            new CompanyUserFilter(null, null, null, null, null)).ConfigureAwait(false);
        
        // Assert
        results.Should().NotBeNull();
        results.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetOwnCompanyAppUsersAsync_SecondPage_ReturnsExpectedResult()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var iamUserId = _fixture.Create<string>();
        var companyUsers = _fixture.CreateMany<CompanyAppUserDetails>(5);

        A.CallTo(() => _userRepository.GetOwnCompanyAppUsersPaginationSourceAsync(A<Guid>._, A<string>._, A<IEnumerable<OfferSubscriptionStatusId>>._, A<IEnumerable<CompanyUserStatusId>>._, A<CompanyUserFilter>._))
            .Returns((int skip, int take) => Task.FromResult((Pagination.Source<CompanyAppUserDetails>?)new Pagination.Source<CompanyAppUserDetails>(companyUsers.Count(), companyUsers.Skip(skip).Take(take))));
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        var sut = new UserBusinessLogic(null!, null!, null!, _portalRepositories, null!, null!, A.Fake<IOptions<UserSettings>>());
        
        // Act
        var results = await sut.GetOwnCompanyAppUsersAsync(
            appId,
            iamUserId, 
            1,
            3,
            new CompanyUserFilter(null, null, null, null, null)).ConfigureAwait(false);
        
        // Assert
        results.Should().NotBeNull();
        results.Content.Should().HaveCount(2);
    }

    #endregion
    
    #region DeleteOwnUserBusinessPartnerNumbers
    
    [Fact]
    public async Task GetOwnCompanyAppUsersAsync_WithNonExistingCompanyUser_ThrowsNotFoundException()
    {
        // Arrange
        var companyUserId = _fixture.Create<Guid>();
        var businessPartnerNumber = _fixture.Create<string>();
        var adminUserId = _fixture.Create<string>();
        A.CallTo(() => _userBusinessPartnerRepository.GetOwnCompanyUserWithAssignedBusinessPartnerNumbersAsync(companyUserId, adminUserId, businessPartnerNumber))
            .ReturnsLazily(() => new ValueTuple<string?, bool, bool>());
        A.CallTo(() => _portalRepositories.GetInstance<IUserBusinessPartnerRepository>()).Returns(_userBusinessPartnerRepository);
        var sut = new UserBusinessLogic(null!, null!, null!, _portalRepositories, null!, null!, A.Fake<IOptions<UserSettings>>());
        
        // Act
        async Task Act() => await sut.DeleteOwnUserBusinessPartnerNumbersAsync(companyUserId, businessPartnerNumber, adminUserId).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Contain("does not exist");
    }

    [Fact]
    public async Task GetOwnCompanyAppUsersAsync_WithUnassignedBusinessPartner_ThrowsForbiddenEception()
    {
        // Arrange
        var companyUserId = _fixture.Create<Guid>();
        var businessPartnerNumber = _fixture.Create<string>();
        var adminUserId = _fixture.Create<string>();
        A.CallTo(() => _userBusinessPartnerRepository.GetOwnCompanyUserWithAssignedBusinessPartnerNumbersAsync(companyUserId, adminUserId, businessPartnerNumber))
            .ReturnsLazily(() => new ValueTuple<string?, bool, bool>(string.Empty, false, false));
        A.CallTo(() => _portalRepositories.GetInstance<IUserBusinessPartnerRepository>()).Returns(_userBusinessPartnerRepository);
        var sut = new UserBusinessLogic(null!, null!, null!, _portalRepositories, null!, null!, A.Fake<IOptions<UserSettings>>());
        
        // Act
        async Task Act() => await sut.DeleteOwnUserBusinessPartnerNumbersAsync(companyUserId, businessPartnerNumber, adminUserId).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Contain("is not assigned to user");
    }

    [Fact]
    public async Task GetOwnCompanyAppUsersAsync_WithoutUserForBpn_ThrowsArgumentException()
    {
        // Arrange
        var companyUserId = _fixture.Create<Guid>();
        var businessPartnerNumber = _fixture.Create<string>();
        var adminUserId = _fixture.Create<string>();
        A.CallTo(() => _userBusinessPartnerRepository.GetOwnCompanyUserWithAssignedBusinessPartnerNumbersAsync(companyUserId, adminUserId, businessPartnerNumber))
            .ReturnsLazily(() => new ValueTuple<string?, bool, bool>(null, true, false));
        A.CallTo(() => _portalRepositories.GetInstance<IUserBusinessPartnerRepository>()).Returns(_userBusinessPartnerRepository);
        var sut = new UserBusinessLogic(null!, null!, null!, _portalRepositories, null!, null!, A.Fake<IOptions<UserSettings>>());
        
        // Act
        async Task Act() => await sut.DeleteOwnUserBusinessPartnerNumbersAsync(companyUserId, businessPartnerNumber, adminUserId).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Contain("is not associated with a user in keycloak");
    }
    
    [Fact]
    public async Task GetOwnCompanyAppUsersAsync_WithInvalidUser_ThrowsForbiddenException()
    {
        // Arrange
        var companyUserId = _fixture.Create<Guid>();
        var businessPartnerNumber = _fixture.Create<string>();
        var adminUserId = _fixture.Create<string>();
        A.CallTo(() => _userBusinessPartnerRepository.GetOwnCompanyUserWithAssignedBusinessPartnerNumbersAsync(companyUserId, adminUserId, businessPartnerNumber))
            .ReturnsLazily(() => new ValueTuple<string?, bool, bool>(Guid.NewGuid().ToString(), true, false));
        A.CallTo(() => _portalRepositories.GetInstance<IUserBusinessPartnerRepository>()).Returns(_userBusinessPartnerRepository);
        var sut = new UserBusinessLogic(null!, null!, null!, _portalRepositories, null!, null!, A.Fake<IOptions<UserSettings>>());
        
        // Act
        async Task Act() => await sut.DeleteOwnUserBusinessPartnerNumbersAsync(companyUserId, businessPartnerNumber, adminUserId).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Contain("do not belong to same company");
    }

    [Fact]
    public async Task GetOwnCompanyAppUsersAsync_WithValidData_ThrowsForbiddenException()
    {
        // Arrange
        var companyUserId = _fixture.Create<Guid>();
        var businessPartnerNumber = _fixture.Create<string>();
        var adminUserId = _fixture.Create<string>();
        A.CallTo(() => _userBusinessPartnerRepository.GetOwnCompanyUserWithAssignedBusinessPartnerNumbersAsync(companyUserId, adminUserId, businessPartnerNumber))
            .ReturnsLazily(() => new ValueTuple<string?, bool, bool>(Guid.NewGuid().ToString(), true, true));
        A.CallTo(() => _portalRepositories.GetInstance<IUserBusinessPartnerRepository>()).Returns(_userBusinessPartnerRepository);
        var sut = new UserBusinessLogic(_provisioningManager, null!, null!, _portalRepositories, null!, null!, A.Fake<IOptions<UserSettings>>());
        
        // Act
        await sut.DeleteOwnUserBusinessPartnerNumbersAsync(companyUserId, businessPartnerNumber, adminUserId).ConfigureAwait(false);
        
        // Assert
        A.CallTo(() => _provisioningManager.DeleteCentralUserBusinessPartnerNumberAsync(A<string>._, A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    #endregion
    
    #region Setup

    private void SetupFakesForUserCreation(bool isBulkUserCreation)
    {
        if (isBulkUserCreation)
        {
            A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);
        }
        else
        {
            A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
            A.CallTo(() => _userRepository.GetSharedIdentityProviderUserAccountDataUntrackedAsync(A<string>._)).Returns(_fixture.Create<(string? SharedIdpAlias, CompanyUserAccountData AccountData)>());

            A.CallTo(() => _userProvisioningService.GetCompanyNameIdpAliasData(A<Guid>._,A<string>._)).Returns((_fixture.Create<CompanyNameIdpAliasData>(),_fixture.Create<string>()));
        }

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._,A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._,A<CancellationToken>._))
            .ReturnsLazily((CompanyNameIdpAliasData companyNameIdpAliasData, IAsyncEnumerable<UserCreationRoleDataIdpInfo> userCreationInfos, CancellationToken cancellationToken) =>
                userCreationInfos.Select(userCreationInfo => _processLine(userCreationInfo)));

        A.CallTo(() => _userProvisioningService.GetIdentityProviderDisplayName(A<string>._)).Returns(_displayName);

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>._)).ReturnsLazily(
            (UserCreationRoleDataIdpInfo creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                .With(x => x.UserName, creationInfo.UserName)
                .With(x => x.Error, (Exception?)null)
                .Create());
    }

    private void SetupFakesForUserDeletion()
    {
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserBusinessPartnerRepository>()).Returns(_userBusinessPartnerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);

        A.CallTo(() => _userRepository.GetSharedIdentityProviderUserAccountDataUntrackedAsync(A<string>._)).Returns(_fixture.Create<(string? SharedIdpAlias, CompanyUserAccountData AccountData)>());
        A.CallTo(() => _userRepository.GetSharedIdentityProviderUserAccountDataUntrackedAsync(A<string>.That.IsEqualTo(_iamUserId))).ReturnsLazily(() =>
            {
                var data = (
                    SharedIdpAlias: (string?)_fixture.Create<string>(),
                    AccountData: _fixture.Build<CompanyUserAccountData>()
                        .With(x => x.CompanyUserId, _companyUserId)
                        .Create());
                return data;
            });

        A.CallTo(() => _identityProviderRepository.GetSharedIdentityProviderIamAliasDataUntrackedAsync(A<string>._))
            .Returns(_fixture.Create<(string? SharedIdpAlias, Guid CompanyUserId)>());
        A.CallTo(() => _identityProviderRepository.GetSharedIdentityProviderIamAliasDataUntrackedAsync(A<string>.That.IsEqualTo(_iamUserId)))
            .Returns(_fixture.Build<(string? SharedIdpAlias, Guid CompanyUserId)>()
                .With(x => x.CompanyUserId, _companyUserId)
                .Create());

        A.CallTo(() => _userRepository.GetCompanyUserAccountDataUntrackedAsync(A<IEnumerable<Guid>>._,A<Guid>._))
            .Returns(Enumerable.Empty<CompanyUserAccountData>().ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserAccountDataUntrackedAsync(A<IEnumerable<Guid>>._,A<Guid>.That.IsEqualTo(_companyUserId)))
            .ReturnsLazily((IEnumerable<Guid> companyUserIds, Guid adminId) =>
                companyUserIds.Select(id => _fixture.Build<CompanyUserAccountData>().With(x => x.CompanyUserId, id).Create())
                    .Select(u => _companyUserSelectFunction(u))
                    .ToAsyncEnumerable());

        A.CallTo(() => _userRepository.AttachAndModifyCompanyUser(A<Guid>._,A<Action<CompanyUser>>._))
            .ReturnsLazily((Guid companyUserId, Action<CompanyUser> setOptionalParameters) =>
            {
                setOptionalParameters.Invoke(_companyUser);
                return _companyUser;
            });

        A.CallTo(() => _companyUserSelectFunction(A<CompanyUserAccountData>._)).ReturnsLazily((CompanyUserAccountData u) => u);

        A.CallTo(() => _provisioningManager.GetProviderUserIdForCentralUserIdAsync(A<string>._,A<string>._)).Returns(_fixture.Create<string>());
    }

    private void SetupFakesForUserRoleModification()
    {
        var iamClientId = "Cl1-CX-Registration";
        var adminRoleId = new Guid("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
        var buyerRoleId = new Guid("9aae7a3b-b188-4a42-b46b-fb2ea5f47662");
        var supplierRoleId = new Guid("9aae7a3b-b188-4a42-b46b-fb2ea5f47663");
        A.CallTo(() => _userRepository.GetAppAssignedIamClientUserDataUntrackedAsync(A<Guid>.That.Matches(x => x == _validOfferId), A<Guid>.That.Matches(x => x == _companyUserId), A<string>.That.Matches(x => x == _adminIamUser || x == _createdCentralUserId)))
            .ReturnsLazily(() => new OfferIamUserData(true, Enumerable.Repeat(iamClientId,1), _iamUserId, true));
        A.CallTo(() => _userRepository.GetAppAssignedIamClientUserDataUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _validOfferId), A<Guid>.That.Matches(x => x == _companyUserId), A<string>.That.Matches(x => x == _adminIamUser)))
            .ReturnsLazily(() => new OfferIamUserData(false, Enumerable.Empty<string>(), _iamUserId, true));
        A.CallTo(() => _userRepository.GetAppAssignedIamClientUserDataUntrackedAsync(A<Guid>.That.Matches(x => x == _validOfferId), A<Guid>.That.Not.Matches(x => x == _companyUserId), A<string>.That.Matches(x => x == _adminIamUser)))
            .ReturnsLazily(() => new OfferIamUserData(true, Enumerable.Repeat(iamClientId,1), _iamUserId, false));
        
        A.CallTo(() => _userRolesRepository.GetAssignedAndMatchingAppRoles(A<Guid>._, A<IEnumerable<string>>._, A<Guid>._))
            .ReturnsLazily(() => new List<UserRoleModificationData>
            {
                new("Existing Role", Guid.NewGuid(), false), 
                new("Buyer", buyerRoleId, true), 
                new("Company Admin", adminRoleId, true),
                new("Supplier", supplierRoleId, false),
            }.ToAsyncEnumerable());

        A.CallTo(() => _userRolesRepository.CreateCompanyUserAssignedRole(A<Guid>._, A<Guid>._))
            .Invokes(x =>
            {
                var companyUserId = x.Arguments.Get<Guid>("companyUserId");
                var companyUserRoleId = x.Arguments.Get<Guid>("companyUserRoleId");

                var companyUserAssignedRole = new CompanyUserAssignedRole(companyUserId, companyUserRoleId);
                _companyUserAssignedRole.Add(companyUserAssignedRole);
            });

        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(A<string>.That.Matches(x => x == _iamUserId), A<IDictionary<string, IEnumerable<string>>>._))
            .ReturnsLazily(() => new [] { (Client: iamClientId, Roles: new [] { "Existing Role", "Supplier" }.AsEnumerable()) }.ToAsyncEnumerable());

        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(A<string>.That.Matches(x => x == _createdCentralUserId), A<IDictionary<string, IEnumerable<string>>>._))
            .ReturnsLazily(() => new [] { (Client: iamClientId, Roles: new [] { "Company Admin" }.AsEnumerable()) }.ToAsyncEnumerable());

        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(
                A<string>.That.Not.Matches(x => x == _createdCentralUserId || x == _iamUserId), A<IDictionary<string, IEnumerable<string>>>._))
            .ReturnsLazily(() => new [] { (Client: iamClientId, Roles: Enumerable.Empty<string>()) }.ToAsyncEnumerable());

        _fixture.Inject(_provisioningManager);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
    }

    #endregion

    [Serializable]
    public class TestException : Exception
    {
        public TestException() { }
        public TestException(string message) : base(message) { }
        public TestException(string message, Exception inner) : base(message, inner) { }
        protected TestException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public interface IMockLogger<T>
    {
        void Log(LogLevel logLevel, Exception? exception, string logMessage);
    }

    public class MockLogger<T> : ILogger<T>
    {
        private readonly IMockLogger<T> _logger;

        public MockLogger(IMockLogger<T> logger)
        {
            _logger = logger;
        }

        public IDisposable BeginScope<TState>(TState state) => new TestDisposable();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState,Exception?,string> formatter) =>
            _logger.Log(logLevel,exception,formatter(state,exception));
        
        public class TestDisposable : IDisposable
        {
            public void Dispose() {
                GC.SuppressFinalize(this);
            }
        }
    }
}
