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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic.Tests;

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
    private readonly IMailingService _mailingService;
    private readonly IMockLogger<UserBusinessLogic> _mockLogger;
    private readonly ILogger<UserBusinessLogic> _logger;
    private readonly IOptions<UserSettings> _options;
    private readonly Guid _identityProviderId;
    private readonly string _iamUserId;
    private readonly string _adminIamUser;
    private readonly Guid _companyUserId;
    private readonly Guid _companyId;
    private readonly Guid _validOfferId;
    private readonly Guid _noTargetIamUserSet;
    private readonly string _createdCentralUserId;
    private readonly ICollection<CompanyUserAssignedRole> _companyUserAssignedRole = new HashSet<CompanyUserAssignedRole>();
    private readonly Func<UserCreationInfoIdp,(Guid CompanyUserId, string UserName, string? Password, Exception? Error)> _processLine;
    private readonly Func<CompanyUser,CompanyUser> _companyUserSelectFunction;
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
        _offerRepository = A.Fake<IOfferRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
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

        _processLine = A.Fake<Func<UserCreationInfoIdp,(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>>();
        _companyUserSelectFunction = A.Fake<Func<CompanyUser,CompanyUser>>();

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
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<Dictionary<string,string>>._,A<List<string>>._)).MustHaveHappenedANumberOfTimesMatching(times => times == userList.Count());

        result.Should().NotBeNull();
        result.Should().HaveSameCount(userList);
        result.Should().Match(r => r.SequenceEqual(userList.Select(u => u.eMail)));
    }

    [Fact]
    public async void TestUserCreationInvalidUserThrows()
    {
        SetupFakesForUserCreation(true);

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<string>._, A<IdentityProviderCategoryId>._))
            .ReturnsLazily(() => (((Guid,string?,string?),(Guid,string?,string?,string?),IEnumerable<string>))default);

        var userList = _fixture.Create<IEnumerable<UserCreationInfo>>();        

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            _portalRepositories,
            _mailingService,
            _logger,
            _options);

        async Task Act() => await sut.CreateOwnCompanyUsersAsync(userList, _iamUserId).ToListAsync().ConfigureAwait(false);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);

        error.Message.Should().Be($"user {_iamUserId} is not associated with any company");

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._,A<string>._,A<IAsyncEnumerable<UserCreationInfoIdp>>._,A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async void TestUserCreationCompanyNameNullThrows()
    {
        SetupFakesForUserCreation(true);

        var companyId = _fixture.Create<Guid>();

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<string>._, A<IdentityProviderCategoryId>._))
            .Returns(_fixture.Build<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company,
                (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser,
                IEnumerable<string> IdpAliase)>()
                .With(x => x.Company, _fixture.Build<(Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber)>()
                    .With(x => x.CompanyId, companyId)
                    .With(x => x.CompanyName, (string?)null)
                    .Create())
                .Create());

        var userList = _fixture.Create<IEnumerable<UserCreationInfo>>();        

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            _portalRepositories,
            _mailingService,
            _logger,
            _options);

        async Task Act() => await sut.CreateOwnCompanyUsersAsync(userList, _iamUserId).ToListAsync().ConfigureAwait(false);

        var error = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);

        error.Message.Should().Be($"assertion failed: companyName of company {companyId} should never be null here");

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._,A<string>._,A<IAsyncEnumerable<UserCreationInfoIdp>>._,A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async void TestUserCreationNoIdpAliasThrows()
    {
        SetupFakesForUserCreation(true);

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<string>._, A<IdentityProviderCategoryId>._))
            .Returns(_fixture.Build<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company,
                (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser,
                IEnumerable<string> IdpAliase)>()
                .With(x => x.IdpAliase, Enumerable.Empty<string>())
                .Create());

        var userList = _fixture.Create<IEnumerable<UserCreationInfo>>();        

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            _portalRepositories,
            _mailingService,
            _logger,
            _options);

        async Task Act() => await sut.CreateOwnCompanyUsersAsync(userList, _iamUserId).ToListAsync().ConfigureAwait(false);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);

        error.Message.Should().Be($"user {_iamUserId} is not associated with any shared idp");

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._,A<string>._,A<IAsyncEnumerable<UserCreationInfoIdp>>._,A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async void TestUserCreationMultipleIdpAliaseThrows()
    {
        SetupFakesForUserCreation(true);

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<string>._, A<IdentityProviderCategoryId>._))
            .Returns(_fixture.Build<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company,
                (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser,
                IEnumerable<string> IdpAliase)>()
                .With(x => x.IdpAliase, _fixture.CreateMany<string>(2))
                .Create());

        var userList = _fixture.Create<IEnumerable<UserCreationInfo>>();        

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            _portalRepositories,
            _mailingService,
            _logger,
            _options);

        async Task Act() => await sut.CreateOwnCompanyUsersAsync(userList, _iamUserId).ToListAsync().ConfigureAwait(false);

        var error = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        error.Message.Should().Be($"user {_iamUserId} is associated with more than one shared idp");

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._,A<string>._,A<IAsyncEnumerable<UserCreationInfoIdp>>._,A<CancellationToken>._))
            .MustNotHaveHappened();
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

        A.CallTo(() => _processLine(A<UserCreationInfoIdp>.That.Matches(u =>
            u.FirstName == userCreationInfo.firstName &&
            u.LastName == userCreationInfo.lastName &&
            u.Email == userCreationInfo.eMail &&
            u.Roles == userCreationInfo.Roles            
        ))).ReturnsLazily(
            (UserCreationInfoIdp creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
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

        A.CallTo(() => _processLine(A<UserCreationInfoIdp>.That.Matches(u =>
            u.FirstName == userCreationInfo.firstName &&
            u.LastName == userCreationInfo.lastName &&
            u.Email == userCreationInfo.eMail &&
            u.Roles == userCreationInfo.Roles            
        ))).MustHaveHappenedOnceExactly();

        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Error), A<Exception?>._, A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<Dictionary<string,string>>._,A<List<string>>._)).MustHaveHappenedANumberOfTimesMatching(times => times == 4);

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

        A.CallTo(() => _processLine(A<UserCreationInfoIdp>.That.Matches(u =>
            u.FirstName == userCreationInfo.firstName &&
            u.LastName == userCreationInfo.lastName &&
            u.Email == userCreationInfo.eMail &&
            u.Roles == userCreationInfo.Roles            
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

        A.CallTo(() => _processLine(A<UserCreationInfoIdp>.That.Matches(u =>
            u.FirstName == userCreationInfo.firstName &&
            u.LastName == userCreationInfo.lastName &&
            u.Email == userCreationInfo.eMail &&
            u.Roles == userCreationInfo.Roles            
        ))).MustHaveHappenedOnceExactly();

        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Error), A<Exception?>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<Dictionary<string,string>>._,A<List<string>>._)).MustHaveHappenedANumberOfTimesMatching(times => times == 2);

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

        A.CallTo(() => _mailingService.SendMails(A<string>.That.IsEqualTo(userCreationInfo.eMail),A<Dictionary<string,string>>._,A<List<string>>._))
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
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<Dictionary<string,string>>._,A<List<string>>._)).MustHaveHappenedANumberOfTimesMatching(times => times == 5);

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
            null!,
            null!,
            _options);

        var result = await sut.CreateOwnCompanyIdpUserAsync(_identityProviderId, userCreationInfoIdp, _iamUserId).ConfigureAwait(false);
        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async void TestCreateOwnCompanyIdpUserAsyncError()
    {
        SetupFakesForUserCreation(false);

        var userCreationInfoIdp = _fixture.Create<UserCreationInfoIdp>();

        A.CallTo(() => _processLine(A<UserCreationInfoIdp>.That.Matches(u => u.FirstName == userCreationInfoIdp.FirstName))).ReturnsLazily(
            (UserCreationInfoIdp creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                .With(x => x.UserName, creationInfo.UserName)
                .With(x => x.Error, _error)
                .Create());

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            null!,
            null!,
            null!,
            _options);

        Task Act() => sut.CreateOwnCompanyIdpUserAsync(_identityProviderId, userCreationInfoIdp, _iamUserId);

        var error = await Assert.ThrowsAsync<TestException>(Act).ConfigureAwait(false);
        error.Message.Should().Be(_error.Message);
    }

    [Fact]
    public async void TestCreateOwnCompanyIdpUserAsyncThrows()
    {
        SetupFakesForUserCreation(false);

        var userCreationInfoIdp = _fixture.Create<UserCreationInfoIdp>();

        A.CallTo(() => _processLine(A<UserCreationInfoIdp>.That.Matches(u => u.FirstName == userCreationInfoIdp.FirstName))).Throws(_error);

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            null!,
            null!,
            null!,
            _options);

        Task Act() => sut.CreateOwnCompanyIdpUserAsync(_identityProviderId, userCreationInfoIdp, _iamUserId);

        var error = await Assert.ThrowsAsync<TestException>(Act).ConfigureAwait(false);
        error.Message.Should().Be(_error.Message);
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
        A.CallTo(() => _userRolesRepository.RemoveCompanyUserAssignedRole(A<CompanyUserAssignedRole>._)).MustHaveHappened();
        A.CallTo(() => _userRepository.RemoveIamUser(A<IamUser>._)).MustHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened();
    }

    [Fact]
    public async void TestDeleteOwnUserInvalidUserThrows()
    {
        SetupFakesForUserDeletion();

        var iamUserId = _fixture.Create<string>();

        A.CallTo(() => _userRepository.GetUserWithSharedIdpDataAsync(A<string>.That.IsEqualTo(iamUserId))).Returns((CompanyUserWithIdpData)null!);
        
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
        error.Message.Should().Be($"iamUser {iamUserId} is not associated to any companyUser");

        A.CallTo(() => _provisioningManager.DeleteSharedRealmUserAsync(A<string>._,A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.DeleteCentralRealmUserAsync(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _userRolesRepository.RemoveCompanyUserAssignedRole(A<CompanyUserAssignedRole>._)).MustNotHaveHappened();
        A.CallTo(() => _userRepository.RemoveIamUser(A<IamUser>._)).MustNotHaveHappened();
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
        A.CallTo(() => _userRolesRepository.RemoveCompanyUserAssignedRole(A<CompanyUserAssignedRole>._)).MustNotHaveHappened();
        A.CallTo(() => _userRepository.RemoveIamUser(A<IamUser>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async void TestDeleteOwnUserErrorDeletingThrows()
    {
        SetupFakesForUserDeletion();
        
        A.CallTo(() => _userRepository.RemoveIamUser(A<IamUser>._)).Throws(_error);

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
        A.CallTo(() => _userRolesRepository.RemoveCompanyUserAssignedRole(A<CompanyUserAssignedRole>._)).MustHaveHappened();
        A.CallTo(() => _userRepository.RemoveIamUser(A<IamUser>._)).MustHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    #endregion

    #region Modify UserRole Async

    [Fact]
    public async Task ModifyUserRoleAsync_WithTwoNewRoles_AddsTwoRolesToTheDatabase()
    {
        // Arrange
        SetupFakesForUserRoleModification();

        var sut = new UserBusinessLogic(
            _provisioningManager,
            null!,
            null!,
            _portalRepositories,
            null!,
            _logger,
            _options
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

        var sut = new UserBusinessLogic(
            _provisioningManager,
            null!,
            null!,
            _portalRepositories,
            null!,
            _logger,
            _options
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

        var sut = new UserBusinessLogic(
            _provisioningManager,
            null!,
            null!,
            _portalRepositories,
            null!,
            _logger,
            _options
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
        ex.ParamName.Should().Be("Roles");
    }

    [Fact]
    public async Task CreateServiceOffering_WithNotFoundCompanyUser_ThrowsException()
    {
        // Arrange
        SetupFakesForUserRoleModification();

        var sut = new UserBusinessLogic(
            _provisioningManager,
            null!,
            null!,
            _portalRepositories,
            null!,
            _logger,
            _options
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
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"CompanyUserId {userRoleInfo.CompanyUserId} is not associated with the same company as adminUserId {_adminIamUser}");
    }

    [Fact]
    public async Task ModifyUserRoleAsync_WithInvalidOfferId_ThrowsException()
    {
        // Arrange
        SetupFakesForUserRoleModification();

        var sut = new UserBusinessLogic(
            _provisioningManager,
            null!,
            null!,
            _portalRepositories,
            null!,
            _logger,
            _options
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
        var ex = await Assert.ThrowsAsync<ArgumentException>(Action);
        ex.ParamName.Should().Be("appId");
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
        A.CallTo(() => _userRolesRepository.RemoveCompanyUserAssignedRole(A<CompanyUserAssignedRole>._)).MustHaveHappenedANumberOfTimesMatching(n => n == 3 * expectedCount);
        A.CallTo(() => _userRepository.RemoveIamUser(A<IamUser>._)).MustHaveHappenedANumberOfTimesMatching(n => n == expectedCount);
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
        A.CallTo(() => _userRolesRepository.RemoveCompanyUserAssignedRole(A<CompanyUserAssignedRole>._)).MustHaveHappenedANumberOfTimesMatching(n => n == 3 * expectedCount);
        A.CallTo(() => _userRepository.RemoveIamUser(A<IamUser>._)).MustHaveHappenedANumberOfTimesMatching(n => n == expectedCount);
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
        error.Message.Should().Be($"iamUser {_iamUserId} is not assigned to any companyUser");

        A.CallTo(() => _provisioningManager.DeleteSharedRealmUserAsync(A<string>._,A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.DeleteCentralRealmUserAsync(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _userRolesRepository.RemoveCompanyUserAssignedRole(A<CompanyUserAssignedRole>._)).MustNotHaveHappened();
        A.CallTo(() => _userRepository.RemoveIamUser(A<IamUser>._)).MustNotHaveHappened();
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

        A.CallTo(() => _companyUserSelectFunction(A<CompanyUser>.That.Matches(u => u.Id == invalidUserId))).ReturnsLazily(
                (CompanyUser u) => {
                u.IamUser = new IamUser(invalidUserEntityId, u.Id); 
                return u;
            });

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
        A.CallTo(() => _userRolesRepository.RemoveCompanyUserAssignedRole(A<CompanyUserAssignedRole>._)).MustHaveHappenedANumberOfTimesMatching(n => n == 3 * (companyUserIds.Length-1));
        A.CallTo(() => _userRepository.RemoveIamUser(A<IamUser>._)).MustHaveHappenedANumberOfTimesMatching(n => n == companyUserIds.Length-1);
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

        A.CallTo(() => _companyUserSelectFunction(A<CompanyUser>.That.Matches(u => u.Id == invalidUserId))).ReturnsLazily(
                (CompanyUser u) => {
                u.IamUser = new IamUser(invalidUserEntityId, u.Id); 
                return u;
            });

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
        A.CallTo(() => _userRolesRepository.RemoveCompanyUserAssignedRole(A<CompanyUserAssignedRole>._)).MustHaveHappenedANumberOfTimesMatching(n => n == 3 * (companyUserIds.Length-1));
        A.CallTo(() => _userRepository.RemoveIamUser(A<IamUser>._)).MustHaveHappenedANumberOfTimesMatching(n => n == companyUserIds.Length-1);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened();

        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Error),_error,A<string>.That.IsEqualTo($"Error while deleting companyUser {invalidUserId}"))).MustHaveHappened();
    }

    #endregion

    #region Setup

    private void SetupFakesForUserCreation(bool isBulkUserCreation)
    {
        if (isBulkUserCreation)
        {
            A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);

            A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<string>._, A<IdentityProviderCategoryId>._))
                .Returns(_fixture.Build<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company,
                    (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser,
                    IEnumerable<string> IdpAliase)>()
                    .With(x => x.IdpAliase, new [] { _fixture.Create<string>() })
                    .Create());
        }
        else
        {
            A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
            A.CallTo(() => _userRepository.GetUserWithSharedIdpDataAsync(A<string>._)).Returns(_fixture.Create<CompanyUserWithIdpData>());

            A.CallTo(() => _userProvisioningService.GetCompanyNameIdpAliasData(A<Guid>._,A<string>._)).Returns(_fixture.Create<CompanyNameIdpAliasData>());
        }

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._,A<string>._,A<IAsyncEnumerable<UserCreationInfoIdp>>._,A<CancellationToken>._))
            .ReturnsLazily((CompanyNameIdpAliasData companyNameIdpAliasData, string keycloakClientId, IAsyncEnumerable<UserCreationInfoIdp> userCreationInfos, CancellationToken cancellationToken) =>
                userCreationInfos.Select(userCreationInfo => _processLine(userCreationInfo)));

        A.CallTo(() => _processLine(A<UserCreationInfoIdp>._)).ReturnsLazily(
            (UserCreationInfoIdp creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                .With(x => x.UserName, creationInfo.UserName)
                .With(x => x.Error, (Exception?)null)
                .Create());
    }

    private void SetupFakesForUserDeletion()
    {
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);

        A.CallTo(() => _userRepository.GetUserWithSharedIdpDataAsync(A<string>._)).Returns(_fixture.Create<CompanyUserWithIdpData>());
        A.CallTo(() => _userRepository.GetUserWithSharedIdpDataAsync(A<string>.That.IsEqualTo(_iamUserId))).ReturnsLazily(() =>
            {
                var data = new CompanyUserWithIdpData(
                    _fixture.Build<CompanyUser>()
                        .With(x => x.Id,_companyUserId)
                        .Create(),
                    _fixture.Create<string>());
                data.CompanyUser.CompanyUserAssignedRoles.AddMany(() => _fixture.Create<CompanyUserAssignedRole>(),5);
                return data;
            });

        A.CallTo(() => _identityProviderRepository.GetSharedIdentityProviderIamAliasDataUntrackedAsync(A<string>._))
            .Returns(_fixture.Create<(string? SharedIdpAlias, Guid CompanyUserId)>());
        A.CallTo(() => _identityProviderRepository.GetSharedIdentityProviderIamAliasDataUntrackedAsync(A<string>.That.IsEqualTo(_iamUserId)))
            .Returns(_fixture.Build<(string? SharedIdpAlias, Guid CompanyUserId)>()
                .With(x => x.CompanyUserId, _companyUserId)
                .Create());

        A.CallTo(() => _userRolesRepository.GetCompanyUserRolesIamUsersAsync(A<IEnumerable<Guid>>._,A<Guid>._))
            .Returns(Enumerable.Empty<CompanyUser>().ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetCompanyUserRolesIamUsersAsync(A<IEnumerable<Guid>>._,A<Guid>.That.IsEqualTo(_companyUserId)))
            .ReturnsLazily((IEnumerable<Guid> companyUserIds, Guid adminId) =>
                companyUserIds.Select(id => _fixture.Build<CompanyUser>().With(x => x.Id, id).Create())
                    .Select(u => _companyUserSelectFunction(u))
                    .ToAsyncEnumerable());
        A.CallTo(() => _companyUserSelectFunction(A<CompanyUser>._)).ReturnsLazily(
                (CompanyUser u) => {
                u.CompanyUserAssignedRoles.AddMany(() => _fixture.Create<CompanyUserAssignedRole>(),3);
                return u;
            });

        A.CallTo(() => _provisioningManager.GetProviderUserIdForCentralUserIdAsync(A<string>._,A<string>._)).Returns(_fixture.Create<string>());
    }

    private void SetupFakesForUserRoleModification()
    {
        var iamClientId = "Cl1-CX-Registration";
        var adminRoleId = new Guid("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
        var buyerRoleId = new Guid("9aae7a3b-b188-4a42-b46b-fb2ea5f47662");
        var supplierRoleId = new Guid("9aae7a3b-b188-4a42-b46b-fb2ea5f47663");
        A.CallTo(() => _userRepository.GetAppAssignedIamClientUserDataUntrackedAsync(A<Guid>.That.Matches(x => x == _validOfferId), A<Guid>.That.Matches(x => x == _companyUserId), A<string>.That.Matches(x => x == _adminIamUser || x == _createdCentralUserId)))
            .ReturnsLazily(() => new ValueTuple<string?, string, bool>(iamClientId, _iamUserId, true));
        A.CallTo(() => _userRepository.GetAppAssignedIamClientUserDataUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _validOfferId), A<Guid>.That.Matches(x => x == _companyUserId), A<string>.That.Matches(x => x == _adminIamUser)))
            .ReturnsLazily(() => new ValueTuple<string?, string, bool>(null, _iamUserId, true));
        A.CallTo(() => _userRepository.GetAppAssignedIamClientUserDataUntrackedAsync(A<Guid>.That.Matches(x => x == _validOfferId), A<Guid>.That.Not.Matches(x => x == _companyUserId), A<string>.That.Matches(x => x == _adminIamUser)))
            .ReturnsLazily(() => new ValueTuple<string?, string, bool>(iamClientId, _iamUserId, false));
        
        A.CallTo(() => _userRolesRepository.GetAssignedAndMatchingRoles(A<Guid>._, A<IEnumerable<string>>._, A<Guid>._))
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
            .ReturnsLazily(() => new Dictionary<string, IEnumerable<string>>
            {
                {iamClientId, new List<string> {"Existing Role", "Supplier"}}
            });
        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(A<string>.That.Matches(x => x == _createdCentralUserId), A<IDictionary<string, IEnumerable<string>>>._))
            .ReturnsLazily(() => new Dictionary<string, IEnumerable<string>>
            {
                {iamClientId, new List<string> {"Company Admin"}}
            });
        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(
                A<string>.That.Not.Matches(x => x == _createdCentralUserId || x == _iamUserId), A<IDictionary<string, IEnumerable<string>>>._))
            .ReturnsLazily(() => new Dictionary<string, IEnumerable<string>>());

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
