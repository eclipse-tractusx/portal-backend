/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests;

public class UserProvisioningServiceCreateUsersTests
{
    private readonly IFixture _fixture;
    private readonly Random _random;
    private readonly int _numUsers;
    private readonly int _numRoles;
    private readonly int _indexSpecialUser;
    private readonly string _firstNameSpecialUser;
    private readonly Guid _companyUserIdSpecialUser;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IUserRepository _userRepository;
    private readonly IUserBusinessPartnerRepository _businessPartnerRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly CompanyNameIdpAliasData _companyNameIdpAliasData;
    private readonly CompanyNameIdpAliasData _companyNameIdpAliasDataSharedIdp;
    private readonly string _clientId;
    private readonly IEnumerable<(string Role, Guid Id)> _userRolesWithId;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public UserProvisioningServiceCreateUsersTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _random = new Random();
        _numUsers = 10;
        _indexSpecialUser = 5;
        _firstNameSpecialUser = _fixture.Create<string>();
        _companyUserIdSpecialUser = Guid.NewGuid();
        _numRoles = 5;

        _companyNameIdpAliasData = _fixture.Build<CompanyNameIdpAliasData>().With(x => x.IsSharedIdp, false).Create();
        _companyNameIdpAliasDataSharedIdp = _fixture.Build<CompanyNameIdpAliasData>().With(x => x.IsSharedIdp, true).Create();

        _clientId = _fixture.Create<string>();
        _cancellationTokenSource = new CancellationTokenSource();
        _userRolesWithId = _fixture.CreateMany<(string, Guid)>(_numRoles).ToList();

        _portalRepositories = A.Fake<IPortalRepositories>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _userRepository = A.Fake<IUserRepository>();
        _businessPartnerRepository = A.Fake<IUserBusinessPartnerRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();

        SetupRepositories();
        SetupProvisioningManager();
    }

    #region CreateOwnCompanyIdpUsersAsync

    [Fact]
    public async Task TestFixtureSetup()
    {
        var sut = new UserProvisioningService(_provisioningManager, _portalRepositories);

        var userCreationInfoIdp = CreateUserCreationInfoIdp().ToAsyncEnumerable();

        await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasData,
            userCreationInfoIdp,
            _cancellationTokenSource.Token
        ).ToListAsync();

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).MustHaveHappened();
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).MustHaveHappened();
        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>._, A<IEnumerable<(string, IEnumerable<string>)>>._)).MustHaveHappened();
        A.CallTo(() => _provisioningManager.AddProviderUserLinkToCentralUserAsync(A<string>._, A<IdentityProviderLink>._)).MustHaveHappened();
        A.CallTo(() => _provisioningManager.CreateSharedRealmUserAsync(A<string>._, A<UserProfile>._)).MustNotHaveHappened();
        A.CallTo(() => _userRepository.AttachAndModifyIdentity(A<Guid>._, null, A<Action<Identity>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestSharedIdpFixtureSetup()
    {
        var sut = new UserProvisioningService(_provisioningManager, _portalRepositories);

        var userCreationInfoIdp = CreateUserCreationInfoIdp().ToAsyncEnumerable();

        await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasDataSharedIdp,
            userCreationInfoIdp,
            _cancellationTokenSource.Token
        ).ToListAsync();

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).MustHaveHappened();
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).MustHaveHappened();
        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>._, A<IEnumerable<(string, IEnumerable<string>)>>._)).MustHaveHappened();
        A.CallTo(() => _provisioningManager.AddProviderUserLinkToCentralUserAsync(A<string>._, A<IdentityProviderLink>._)).MustHaveHappened();
        A.CallTo(() => _provisioningManager.CreateSharedRealmUserAsync(A<string>._, A<UserProfile>._)).MustHaveHappened();
        A.CallTo(() => _userRepository.AttachAndModifyIdentity(A<Guid>._, null, A<Action<Identity>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestCreateUsersAllSuccess()
    {
        var sut = new UserProvisioningService(_provisioningManager, _portalRepositories);

        var userCreationInfoIdp = CreateUserCreationInfoIdp().ToList();

        var result = await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasData,
            userCreationInfoIdp.ToAsyncEnumerable(),
            _cancellationTokenSource.Token
        ).ToListAsync();

        result.Should().HaveCount(_numUsers);
        result.Select(r => r.UserName).Should().ContainInOrder(userCreationInfoIdp.Select(u => u.UserName));
        result.Should().AllSatisfy(r => r.Error.Should().BeNull());
    }

    [Fact]
    public async Task TestCreateUsersRolesAssignmentError()
    {
        var sut = new UserProvisioningService(_provisioningManager, _portalRepositories);

        var specialUser = _fixture.Build<UserCreationRoleDataIdpInfo>()
            .With(x => x.FirstName, _firstNameSpecialUser)
            .With(x => x.RoleDatas, PickValidRoles().DistinctBy(role => role.UserRoleText).ToList())
            .Create();

        var userCreationInfoIdp = CreateUserCreationInfoIdp(() => specialUser).ToList();

        var centralUserName = _companyUserIdSpecialUser.ToString();
        var iamUserId = _fixture.Create<string>();

        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>.That.Matches(p => p.UserName == centralUserName), A<IEnumerable<(string, IEnumerable<string>)>>._))
            .Returns(iamUserId);

        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(iamUserId, A<IDictionary<string, IEnumerable<string>>>._))
            .Returns(new (string, IEnumerable<string>, Exception?)[] { (_clientId, Enumerable.Empty<string>(), new Exception("some error")) }.ToAsyncEnumerable());

        var result = await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasData,
            userCreationInfoIdp.ToAsyncEnumerable(),
            _cancellationTokenSource.Token
        ).ToListAsync();

        result.Should().HaveCount(_numUsers);
        result.Where((r, index) => index != _indexSpecialUser).Should().AllSatisfy(r => r.Error.Should().BeNull());

        var error = result.ElementAt(_indexSpecialUser).Error;
        error.Should().NotBeNull();
        error.Should().BeOfType(typeof(ConflictException));
        error!.Message.Should().Be($"invalid role data [{string.Join(", ", specialUser.RoleDatas.Select(roleData => $"clientId: {roleData.ClientClientId}, role: {roleData.UserRoleText}, error: some error"))}] has not been assigned in keycloak");
    }

    [Fact]
    public async Task TestCreateUsersRolesAssignmentNoRolesAssignedSuccess()
    {
        var sut = new UserProvisioningService(_provisioningManager, _portalRepositories);

        var specialUser = _fixture.Build<UserCreationRoleDataIdpInfo>()
            .With(x => x.FirstName, _firstNameSpecialUser)
            .With(x => x.RoleDatas, Enumerable.Empty<UserRoleData>().ToList())
            .Create();

        var userCreationInfoIdp = CreateUserCreationInfoIdp(() => specialUser).ToList();

        var centralUserName = _companyUserIdSpecialUser.ToString();
        var iamUserId = _fixture.Create<string>();

        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>.That.Matches(p => p.UserName == centralUserName), A<IEnumerable<(string, IEnumerable<string>)>>._))
            .Returns(iamUserId);

        var result = await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasData,
            userCreationInfoIdp.ToAsyncEnumerable(),
            _cancellationTokenSource.Token
        ).ToListAsync();

        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(A<string>._, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappened();
        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(A<string>.That.IsEqualTo(iamUserId), A<IDictionary<string, IEnumerable<string>>>._)).MustNotHaveHappened();

        result.Should().HaveCount(_numUsers);
        result.Should().AllSatisfy(r => r.Error.Should().BeNull());
    }

    [Fact]
    public async Task TestCreateUsersCentralUserDuplicateKeycloakUserError()
    {
        var sut = new UserProvisioningService(_provisioningManager, _portalRepositories);

        var userCreationInfoIdp = CreateUserCreationInfoIdp().ToList();

        var userInfo = userCreationInfoIdp.ElementAt(_indexSpecialUser);
        var iamUserId = _fixture.Create<string>();
        var companyUserId = Guid.NewGuid();

        A.CallTo(() => _userRepository.GetMatchingCompanyIamUsersByNameEmail(A<string>.That.IsEqualTo(userInfo.FirstName), A<string>._, A<string>._, A<Guid>._, A<IEnumerable<UserStatusId>>._))
            .Returns(new[] { (CompanyUserId: companyUserId, IsFullMatch: false) }.ToAsyncEnumerable());

        A.CallTo(() => _provisioningManager.GetUserByUserName(companyUserId.ToString()))
            .Returns(iamUserId);

        A.CallTo(() => _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(iamUserId))
            .Returns(new[] { new IdentityProviderLink(_companyNameIdpAliasData.IdpAlias, userInfo.UserId, userInfo.UserName) }.ToAsyncEnumerable());

        var result = await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasData,
            userCreationInfoIdp.ToAsyncEnumerable(),
            _cancellationTokenSource.Token
        ).ToListAsync();

        A.CallTo(() => _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(A<string>._)).MustHaveHappened();

        result.Should().HaveCount(_numUsers);
        result.Where((r, index) => index != _indexSpecialUser).Should().AllSatisfy(r => r.Error.Should().BeNull());

        var error = result.ElementAt(_indexSpecialUser).Error;
        error.Should().NotBeNull();
        error.Should().BeOfType(typeof(ConflictException));
        error!.Message.Should().Be($"existing user {companyUserId} in keycloak for provider userid {userInfo.UserId}, {userInfo.UserName}");
    }

    [Fact]
    public async Task TestCreateUsersCentralUserPotentialMatchWithoutMatchingKeycloakUserSuccess()
    {
        var sut = new UserProvisioningService(_provisioningManager, _portalRepositories);

        var userCreationInfoIdp = CreateUserCreationInfoIdp().ToList();

        var userInfo = userCreationInfoIdp.ElementAt(_indexSpecialUser);
        var iamUserId = _fixture.Create<string>();
        var companyUserId = Guid.NewGuid();

        A.CallTo(() => _userRepository.GetMatchingCompanyIamUsersByNameEmail(A<string>.That.IsEqualTo(userInfo.FirstName), A<string>._, A<string>._, A<Guid>._, A<IEnumerable<UserStatusId>>._))
            .Returns(new[] { (CompanyUserId: companyUserId, IsFullMatch: false) }.ToAsyncEnumerable());

        A.CallTo(() => _provisioningManager.GetUserByUserName(companyUserId.ToString()))
            .Returns(iamUserId);

        A.CallTo(() => _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(iamUserId))
            .Returns(_fixture.CreateMany<IdentityProviderLink>(3).ToAsyncEnumerable());

        var result = await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasData,
            userCreationInfoIdp.ToAsyncEnumerable(),
            _cancellationTokenSource.Token
        ).ToListAsync();

        A.CallTo(() => _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(A<string>._)).MustHaveHappenedOnceExactly();

        result.Should().HaveCount(_numUsers)
            .And.AllSatisfy(r => r.Error.Should().BeNull());
    }

    [Fact]
    public async Task TestCreateUsersKeycloakConflictError()
    {
        var sut = new UserProvisioningService(_provisioningManager, _portalRepositories);

        var userCreationInfoIdp = CreateUserCreationInfoIdp().ToList();

        var userInfo = userCreationInfoIdp.ElementAt(_indexSpecialUser);

        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>.That.Matches(x => x.FirstName == userInfo.FirstName), A<IEnumerable<(string Name, IEnumerable<string> Values)>>._))
            .Throws(ConflictException.Create(ProvisioningServiceErrors.USER_CREATION_CONFLICT, new ErrorParameter[] { new("userName", "foo"), new("realm", "bar") }));

        var result = await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasData,
            userCreationInfoIdp.ToAsyncEnumerable(),
            _cancellationTokenSource.Token
        ).ToListAsync();

        result.Should().HaveCount(_numUsers);
        result.Where((r, index) => index != _indexSpecialUser).Should().AllSatisfy(r => r.Error.Should().BeNull());

        result.ElementAt(_indexSpecialUser).Error.Should().NotBeNull()
            .And.BeOfType<ConflictException>()
            .Which.Should().Match<ConflictException>(x =>
                x.HasDetails &&
                x.ErrorType == typeof(ProvisioningServiceErrors) &&
                x.Parameters.Count() == 2 &&
                x.Parameters.First(p => p.Name == "userName").Value == "foo" &&
                x.Parameters.First(p => p.Name == "realm").Value == "bar");
    }

    [Fact]
    public async Task TestCreateUsersNotExistingCompanyUserWithoutKeycloakUserSuccess()
    {
        var sut = new UserProvisioningService(_provisioningManager, _portalRepositories);

        var userCreationInfoIdp = CreateUserCreationInfoIdp().ToList();

        var userInfo = userCreationInfoIdp.ElementAt(_indexSpecialUser);
        var identityId = Guid.NewGuid();
        var centralUserId = _fixture.Create<string>();

        A.CallTo(() => _userRepository.GetMatchingCompanyIamUsersByNameEmail(userInfo.FirstName, A<string>._, A<string>._, A<Guid>._, A<IEnumerable<UserStatusId>>._))
            .Returns(Enumerable.Empty<(Guid, bool)>().ToAsyncEnumerable());

        A.CallTo(() => _userRepository.CreateIdentity(A<Guid>._, A<UserStatusId>._, A<IdentityTypeId>._, A<Action<Identity>>._))
            .ReturnsLazily((Guid companyId, UserStatusId userStatusId, IdentityTypeId identityTypeId, Action<Identity>? setOptionalFields) =>
            {
                var identity = new Identity(Guid.NewGuid(), DateTimeOffset.UtcNow, companyId, userStatusId, identityTypeId);
                setOptionalFields?.Invoke(identity);
                return identity;
            });
        A.CallTo(() => _userRepository.CreateCompanyUser(A<Guid>._, userInfo.FirstName, A<string?>._, A<string>._))
            .ReturnsLazily((Guid _, string? firstName, string? lastName, string email) => new CompanyUser(identityId) { Firstname = firstName, Lastname = lastName, Email = email });

        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>.That.Matches(u => u.FirstName == userInfo.FirstName), A<IEnumerable<(string, IEnumerable<string>)>>._))
            .Returns(centralUserId);

        var result = await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasData,
            userCreationInfoIdp.ToAsyncEnumerable(),
            _cancellationTokenSource.Token
        ).ToListAsync();

        A.CallTo(() => _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>._, A<IEnumerable<(string, IEnumerable<string>)>>._)).MustHaveHappened(userCreationInfoIdp.Count, Times.Exactly);
        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>.That.Matches(u => u.FirstName == userInfo.FirstName), A<IEnumerable<(string, IEnumerable<string>)>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.CreateIdentity(_companyNameIdpAliasData.CompanyId, UserStatusId.ACTIVE, IdentityTypeId.COMPANY_USER, null)).MustHaveHappened(userCreationInfoIdp.Count, Times.Exactly);
        A.CallTo(() => _userRepository.AddCompanyUserAssignedIdentityProvider(A<Guid>._, A<Guid>._, A<string>._, A<string>._)).MustHaveHappened(userCreationInfoIdp.Count, Times.Exactly);
        A.CallTo(() => _userRepository.AddCompanyUserAssignedIdentityProvider(identityId, A<Guid>._, A<string>._, A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.CreateCompanyUser(A<Guid>._, A<string?>._, A<string?>._, A<string>._)).MustHaveHappened(userCreationInfoIdp.Count, Times.Exactly);
        A.CallTo(() => _userRepository.CreateCompanyUser(A<Guid>._, userInfo.FirstName, userInfo.LastName, userInfo.Email)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(A<Guid>._, _companyNameIdpAliasData.BusinessPartnerNumber!)).MustHaveHappened(userCreationInfoIdp.Count, Times.Exactly);
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(identityId, _companyNameIdpAliasData.BusinessPartnerNumber!)).MustHaveHappenedOnceExactly();

        result.Should().HaveCount(_numUsers);
        result.Should().AllSatisfy(r => r.Error.Should().BeNull());
    }

    [Fact]
    public async Task TestCreateUsersExistingCompanyUserWithoutKeycloakUserSuccess()
    {
        var sut = new UserProvisioningService(_provisioningManager, _portalRepositories);

        var userCreationInfoIdp = CreateUserCreationInfoIdp().ToList();

        var userInfo = userCreationInfoIdp.ElementAt(_indexSpecialUser);
        var companyUserId = Guid.NewGuid();
        var existingUserId = _fixture.Create<string>();
        var centralUserId = _fixture.Create<string>();

        A.CallTo(() => _userRepository.GetMatchingCompanyIamUsersByNameEmail(userInfo.FirstName, A<string>._, A<string>._, A<Guid>._, A<IEnumerable<UserStatusId>>._))
            .Returns(new[] { (CompanyUserId: companyUserId, IsFullMatch: true) }.ToAsyncEnumerable());

        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>.That.Matches(u => u.FirstName == userInfo.FirstName), A<IEnumerable<(string, IEnumerable<string>)>>._))
            .Returns(centralUserId);

        var result = await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasData,
            userCreationInfoIdp.ToAsyncEnumerable(),
            _cancellationTokenSource.Token
        ).ToListAsync();

        A.CallTo(() => _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(existingUserId)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>._, A<IEnumerable<(string, IEnumerable<string>)>>._)).MustHaveHappened(userCreationInfoIdp.Count, Times.Exactly);
        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>.That.Matches(u => u.FirstName == userInfo.FirstName), A<IEnumerable<(string, IEnumerable<string>)>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.CreateIdentity(A<Guid>._, UserStatusId.ACTIVE, IdentityTypeId.COMPANY_USER, A<Action<Identity>>._)).MustHaveHappened(userCreationInfoIdp.Count - 1, Times.Exactly);
        A.CallTo(() => _userRepository.CreateCompanyUser(A<Guid>._, A<string?>._, A<string?>._, A<string>._)).MustHaveHappened(userCreationInfoIdp.Count - 1, Times.Exactly);
        A.CallTo(() => _userRepository.CreateCompanyUser(A<Guid>._, userInfo.FirstName, A<string?>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(A<Guid>._, _companyNameIdpAliasData.BusinessPartnerNumber!)).MustHaveHappened(userCreationInfoIdp.Count - 1, Times.Exactly);
        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(companyUserId, A<string?>._)).MustNotHaveHappened();

        result.Should().HaveCount(_numUsers);
        result.Should().AllSatisfy(r => r.Error.Should().BeNull());
    }

    #endregion

    #region GetRoleDatas

    [Fact]
    public async Task TestGetRoleDatasSuccess()
    {
        var clientRoles = _fixture.Create<IEnumerable<UserRoleConfig>>();

        var sut = new UserProvisioningService(_provisioningManager, _portalRepositories);

        var result = await sut.GetRoleDatas(clientRoles).ToListAsync();

        result.Should().HaveSameCount(clientRoles.SelectMany(r => r.UserRoleNames));
    }

    [Fact]
    public async Task TestGetRoleDatasThrows()
    {
        var clientRoles = _fixture.Create<IEnumerable<UserRoleConfig>>();

        A.CallTo(() => _userRolesRepository.GetUserRoleDataUntrackedAsync(A<IEnumerable<UserRoleConfig>>._))
            .ReturnsLazily((IEnumerable<UserRoleConfig> clientRoles) =>
                clientRoles.SelectMany(r => r.UserRoleNames.Take(r.UserRoleNames.Count() - 1).Select(role => _fixture.Build<UserRoleData>()
                    .With(x => x.ClientClientId, r.ClientId)
                    .With(x => x.UserRoleText, role).Create())).ToAsyncEnumerable());

        var sut = new UserProvisioningService(_provisioningManager, _portalRepositories);

        async Task Act() => await sut.GetRoleDatas(clientRoles).ToListAsync();

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        error.Message.Should().StartWith("invalid roles: clientId:");
    }

    #endregion

    #region GetOwnCompanyPortalRoleDatas

    [Fact]
    public async Task TestGetOwnCompanyPortalRoleDatasSuccess()
    {
        var client = _fixture.Create<string>();
        var roles = _fixture.CreateMany<string>();
        var companyId = Guid.NewGuid();

        var sut = new UserProvisioningService(_provisioningManager, _portalRepositories);

        var result = await sut.GetOwnCompanyPortalRoleDatas(client, roles, companyId);

        result.Should().HaveSameCount(roles);
    }

    [Fact]
    public async Task TestGetOwnCompanyPortalRoleDatasThrows()
    {
        var client = _fixture.Create<string>();
        var roles = _fixture.CreateMany<string>();
        var companyId = Guid.NewGuid();

        A.CallTo(() => _userRolesRepository.GetOwnCompanyPortalUserRoleDataUntrackedAsync(A<string>._, A<IEnumerable<string>>._, A<Guid>._))
            .ReturnsLazily((string clientId, IEnumerable<string> roles, Guid _) =>
                roles.Take(roles.Count() - 1).Select(role => _fixture.Build<UserRoleData>()
                    .With(x => x.ClientClientId, clientId)
                    .With(x => x.UserRoleText, role).Create()).ToAsyncEnumerable());

        var sut = new UserProvisioningService(_provisioningManager, _portalRepositories);

        async Task Act() => await sut.GetOwnCompanyPortalRoleDatas(client, roles, companyId);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        error.Message.Should().StartWith("invalid roles: clientId:");
    }

    #endregion

    #region Setup

    private void SetupRepositories()
    {
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserBusinessPartnerRepository>()).Returns(_businessPartnerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);

        A.CallTo(() => _userRepository.CreateIdentity(A<Guid>._, A<UserStatusId>._, IdentityTypeId.COMPANY_USER, null))
            .ReturnsLazily((Guid companyId, UserStatusId userStatusId, IdentityTypeId identityTypeId, Action<Identity>? setOptionalFields) =>
            {
                var identity = new Identity(
                    Guid.NewGuid(),
                    DateTimeOffset.UtcNow,
                    companyId,
                    userStatusId,
                    identityTypeId);
                setOptionalFields?.Invoke(identity);
                return identity;
            });

        A.CallTo(() => _userRepository.CreateCompanyUser(A<Guid>._, A<string>._, A<string>._, A<string>._))
            .ReturnsLazily((Guid _, string firstName, string _, string _) =>
                new CompanyUser(
                    firstName == _firstNameSpecialUser ? _companyUserIdSpecialUser : Guid.NewGuid()));

        A.CallTo(() => _businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(A<Guid>._, A<string>._))
            .ReturnsLazily((Guid companyUserId, string businessPartnerNumber) => new CompanyUserAssignedBusinessPartner(companyUserId, businessPartnerNumber));

        A.CallTo(() => _userRolesRepository.GetUserRoleDataUntrackedAsync(A<IEnumerable<UserRoleConfig>>._))
            .ReturnsLazily((IEnumerable<UserRoleConfig> clientRoles) =>
                clientRoles.SelectMany(role => role.UserRoleNames.Select(r => _fixture.Build<UserRoleData>()
                    .With(x => x.ClientClientId, role.ClientId)
                    .With(x => x.UserRoleText, r).Create())).ToAsyncEnumerable());

        A.CallTo(() => _userRolesRepository.GetOwnCompanyPortalUserRoleDataUntrackedAsync(A<string>._, A<IEnumerable<string>>._, A<Guid>._))
            .ReturnsLazily((string client, IEnumerable<string> roles, Guid _) =>
                roles.Select(role => _fixture.Build<UserRoleData>()
                    .With(x => x.ClientClientId, client)
                    .With(x => x.UserRoleText, role).Create()).ToAsyncEnumerable());
    }

    private void SetupProvisioningManager()
    {
        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>._, A<IEnumerable<(string, IEnumerable<string>)>>._))
            .ReturnsLazily(() => _fixture.Create<string>());

        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(A<string>._, A<IDictionary<string, IEnumerable<string>>>._))
            .ReturnsLazily((string _, IDictionary<string, IEnumerable<string>> clientRoles) => clientRoles.Select(x => (Client: x.Key, Roles: x.Value, Error: default(Exception?))).ToAsyncEnumerable());
    }

    private IEnumerable<UserCreationRoleDataIdpInfo> CreateUserCreationInfoIdp(Func<UserCreationRoleDataIdpInfo>? createInvalidUser = null)
    {
        var indexUser = 0;
        while (indexUser < _numUsers)
        {
            yield return (indexUser == _indexSpecialUser && createInvalidUser != null)
                ? createInvalidUser()
                : _fixture.Build<UserCreationRoleDataIdpInfo>()
                    .With(x => x.RoleDatas, PickValidRoles().DistinctBy(role => role.UserRoleText).ToList())
                    .With(x => x.UserStatusId, UserStatusId.ACTIVE)
                    .Create();
            indexUser++;
        }
    }

    private IEnumerable<UserRoleData> PickValidRoles()
    {
        var maxRoles = _userRolesWithId.Count();
        var numRoles = _random.Next(1, maxRoles);
        while (numRoles > 0)
        {
            var roleWithId = _userRolesWithId.ElementAt(_random.Next(maxRoles));
            yield return new UserRoleData(roleWithId.Id, _clientId, roleWithId.Role);
            numRoles--;
        }
    }

    #endregion
}
