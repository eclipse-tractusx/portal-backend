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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using FakeItEasy;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests;

public class UserProvisioningServiceCreateUsersTests
{
    private readonly IFixture _fixture;
    private readonly Random _random;
    private readonly int _numUsers;
    private readonly int _numRoles;
    private readonly int _indexSpecialUser;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IUserRepository _userRepository;
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
        _numRoles = 5;

        _companyNameIdpAliasData = _fixture.Build<CompanyNameIdpAliasData>().With(x => x.IsSharedIdp, false).Create();
        _companyNameIdpAliasDataSharedIdp = _fixture.Build<CompanyNameIdpAliasData>().With(x => x.IsSharedIdp, true).Create();

        _clientId = _fixture.Create<string>();
        _cancellationTokenSource = new CancellationTokenSource();
        _userRolesWithId = _fixture.CreateMany<(string,Guid)>(_numRoles).ToList();

        _portalRepositories = A.Fake<IPortalRepositories>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();

        SetupRepositories();
        SetupProvisioningManager();
    }

    #region CreateOwnCompanyIdpUsersAsync

    [Fact]
    public async void TestFixtureSetup()
    {
        var sut = new UserProvisioningService(_provisioningManager,_portalRepositories);

        var userCreationInfoIdp = CreateUserCreationInfoIdp().ToAsyncEnumerable();
        
        var result = await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasData,
            _clientId,
            userCreationInfoIdp,
            _cancellationTokenSource.Token
        ).ToListAsync().ConfigureAwait(false);

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).MustHaveHappened();
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).MustHaveHappened();
        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>._, A<IEnumerable<(string,IEnumerable<string>)>>._)).MustHaveHappened();
        A.CallTo(() => _provisioningManager.AddProviderUserLinkToCentralUserAsync(A<string>._, A<IdentityProviderLink>._)).MustHaveHappened();
        A.CallTo(() => _provisioningManager.CreateSharedRealmUserAsync(A<string>._, A<UserProfile>._)).MustNotHaveHappened();
        A.CallTo(() => _userRepository.CreateIamUser(A<Guid>._, A<string>._)).MustHaveHappened();
    }

    [Fact]
    public async void TestSharedIdpFixtureSetup()
    {
        var sut = new UserProvisioningService(_provisioningManager,_portalRepositories);

        var userCreationInfoIdp = CreateUserCreationInfoIdp().ToAsyncEnumerable();
        
        var result = await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasDataSharedIdp,
            _clientId,
            userCreationInfoIdp,
            _cancellationTokenSource.Token
        ).ToListAsync().ConfigureAwait(false);

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).MustHaveHappened();
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).MustHaveHappened();
        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>._, A<IEnumerable<(string,IEnumerable<string>)>>._)).MustHaveHappened();
        A.CallTo(() => _provisioningManager.AddProviderUserLinkToCentralUserAsync(A<string>._, A<IdentityProviderLink>._)).MustHaveHappened();
        A.CallTo(() => _provisioningManager.CreateSharedRealmUserAsync(A<string>._, A<UserProfile>._)).MustHaveHappened();
        A.CallTo(() => _userRepository.CreateIamUser(A<Guid>._, A<string>._)).MustHaveHappened();
    }

    [Fact]
    public async void TestCreateUsersAllSuccess()
    {
        var sut = new UserProvisioningService(_provisioningManager,_portalRepositories);

        var userCreationInfoIdp = CreateUserCreationInfoIdp().ToList();
        
        var result = await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasData,
            _clientId,
            userCreationInfoIdp.ToAsyncEnumerable(),
            _cancellationTokenSource.Token
        ).ToListAsync().ConfigureAwait(false);

        result.Should().HaveCount(_numUsers);
        result.Select(r => r.UserName).Should().ContainInOrder(userCreationInfoIdp.Select(u => u.UserName));
        result.Should().AllSatisfy(r => r.Error.Should().BeNull());
    }

    [Fact]
    public async void TestCreateUsersInvalidRolesError()
    {
        var sut = new UserProvisioningService(_provisioningManager,_portalRepositories);

        var userWithInvalidRoles = _fixture.Create<UserCreationInfoIdp>();
        var userCreationInfoIdp = CreateUserCreationInfoIdp(
            () => userWithInvalidRoles).ToList();

        var result = await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasData,
            _clientId,
            userCreationInfoIdp.ToAsyncEnumerable(),
            _cancellationTokenSource.Token
        ).ToListAsync().ConfigureAwait(false);

        result.Should().HaveCount(_numUsers);
        result.Where((r,index) => index != _indexSpecialUser).Should().AllSatisfy(r => r.Error.Should().BeNull());

        var error = result.ElementAt(_indexSpecialUser).Error;
        error.Should().NotBeNull();
        error.Should().BeOfType(typeof(ControllerArgumentException));
        error!.Message.Should().Be($"invalid Roles: [{string.Join(", ",userWithInvalidRoles.Roles)}]");
    }

    [Fact]
    public async void TestCreateUsersRolesAssignmentError()
    {
        var sut = new UserProvisioningService(_provisioningManager,_portalRepositories);

        var userCreationInfoIdp = CreateUserCreationInfoIdp().ToList();

        var roles = userCreationInfoIdp.ElementAt(_indexSpecialUser).Roles;
        var assignedRoles = roles.Take(roles.Count()-1);
        var centralUserName = _companyNameIdpAliasData.IdpAlias + "." + userCreationInfoIdp.ElementAt(_indexSpecialUser).UserId;
        var iamUserId = _fixture.Create<string>();

        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>.That.Matches(p => p.UserName ==  centralUserName), A<IEnumerable<(string,IEnumerable<string>)>>._))
            .Returns(iamUserId);

        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(A<string>.That.IsEqualTo(iamUserId), A<IDictionary<string, IEnumerable<string>>>._))
            .ReturnsLazily((string _, IDictionary<string, IEnumerable<string>> clientRoles) => new [] { (_clientId, assignedRoles) }.ToDictionary(x => x._clientId, x => x.assignedRoles));

        var result = await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasData,
            _clientId,
            userCreationInfoIdp.ToAsyncEnumerable(),
            _cancellationTokenSource.Token
        ).ToListAsync().ConfigureAwait(false);

        result.Should().HaveCount(_numUsers);
        result.Where((r,index) => index != _indexSpecialUser).Should().AllSatisfy(r => r.Error.Should().BeNull());

        var error = result.ElementAt(_indexSpecialUser).Error;
        error.Should().NotBeNull();
        error.Should().BeOfType(typeof(ConflictException));
        error!.Message.Should().Be($"invalid role data, client: {_clientId}, [{String.Join(", ", roles.Except(assignedRoles))}] has not been assigned in keycloak");
    }

    [Fact]
    public async void TestCreateUsersDuplicateKeycloakUserError()
    {
        var sut = new UserProvisioningService(_provisioningManager,_portalRepositories);

        var userCreationInfoIdp = CreateUserCreationInfoIdp().ToList();

        var userInfo = userCreationInfoIdp.ElementAt(_indexSpecialUser);
        var iamUserId = _fixture.Create<string>();

        A.CallTo(() => _userRepository.GetMatchingCompanyIamUsersByNameEmail(A<string>.That.IsEqualTo(userInfo.FirstName), A<string>._, A<string>._, A<Guid>._))
            .Returns(new [] { (UserEntityId: (string?)iamUserId, CompanyUserId: Guid.Empty) }.ToAsyncEnumerable());

        A.CallTo(() => _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(A<string>.That.IsEqualTo(iamUserId)))
            .Returns(new [] { new IdentityProviderLink(_companyNameIdpAliasData.IdpAlias,userInfo.UserId,userInfo.UserName) }.ToAsyncEnumerable());

        var result = await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasData,
            _clientId,
            userCreationInfoIdp.ToAsyncEnumerable(),
            _cancellationTokenSource.Token
        ).ToListAsync().ConfigureAwait(false);

        A.CallTo(() => _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(A<string>._)).MustHaveHappened();

        result.Should().HaveCount(_numUsers);
        result.Where((r,index) => index != _indexSpecialUser).Should().AllSatisfy(r => r.Error.Should().BeNull());

        var error = result.ElementAt(_indexSpecialUser).Error;
        error.Should().NotBeNull();
        error.Should().BeOfType(typeof(ConflictException));
        error!.Message.Should().Be($"existing user {iamUserId} in keycloak for provider userid {userInfo.UserId}, {userInfo.UserName}");
    }

    [Fact]
    public async void TestCreateUsersExistingCompanyUserWithoutKeycloakUserSuccess()
    {
        var sut = new UserProvisioningService(_provisioningManager,_portalRepositories);

        var userCreationInfoIdp = CreateUserCreationInfoIdp().ToList();

        var userInfo = userCreationInfoIdp.ElementAt(_indexSpecialUser);
        var companyUserId = _fixture.Create<Guid>();

        A.CallTo(() => _userRepository.GetMatchingCompanyIamUsersByNameEmail(A<string>.That.IsEqualTo(userInfo.FirstName), A<string>._, A<string>._, A<Guid>._))
            .Returns(new [] { (UserEntityId: (string?)null, CompanyUserId: companyUserId) }.ToAsyncEnumerable());

        var result = await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasData,
            _clientId,
            userCreationInfoIdp.ToAsyncEnumerable(),
            _cancellationTokenSource.Token
        ).ToListAsync().ConfigureAwait(false);

        A.CallTo(() => _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>.That.Matches(u => u.FirstName == userInfo.FirstName), A<IEnumerable<(string,IEnumerable<string>)>>._)).MustHaveHappened();
        A.CallTo(() => _userRepository.CreateIamUser(A<Guid>.That.IsEqualTo(companyUserId), A<string>._)).MustHaveHappened();

        result.Should().HaveCount(_numUsers);
        result.Should().AllSatisfy(r => r.Error.Should().BeNull());
    }

    #endregion

    #region Setup

    private void SetupRepositories()
    {
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);

        A.CallTo(() => _userRepository.CreateCompanyUser(A<string>._, A<string>._, A<string>._,  A<Guid>._, A<CompanyUserStatusId>._, A<Guid>._))
            .ReturnsLazily((string firstName, string lastName, string email, Guid companyId, CompanyUserStatusId companyUserStatusId, Guid lastEditorId) =>
                new CompanyUser(_fixture.Create<Guid>(), companyId, companyUserStatusId, DateTimeOffset.UtcNow, lastEditorId));

        A.CallTo(() => _userRepository.CreateIamUser(A<Guid>._, A<string>._))
            .ReturnsLazily((Guid companyUserId, string iamUserId) => new IamUser(iamUserId, companyUserId));

        A.CallTo(() => _userRolesRepository.GetUserRolesWithIdAsync(A<string>.Ignored)).Returns(_userRolesWithId.ToAsyncEnumerable());
    }

    private void SetupProvisioningManager()
    {
        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>._, A<IEnumerable<(string,IEnumerable<string>)>>._))
            .ReturnsLazily(() => _fixture.Create<string>());

        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(A<string>._, A<IDictionary<string, IEnumerable<string>>>._))
            .ReturnsLazily((string _, IDictionary<string, IEnumerable<string>> clientRoles) => clientRoles);
    }

    private IEnumerable<UserCreationInfoIdp> CreateUserCreationInfoIdp(Func<UserCreationInfoIdp>? createInvalidUser = null)
    {
        var indexUser = 0;
        while (indexUser < _numUsers)
        {
            yield return (indexUser == _indexSpecialUser && createInvalidUser != null)
                ? createInvalidUser()
                : _fixture.Build<UserCreationInfoIdp>().With(x => x.Roles, PickValidRoles().ToList()).Create();
            indexUser++;
        }
    }

    private IEnumerable<string> PickValidRoles()
    {
        var maxRoles = _userRolesWithId.Count();
        var numRoles = _random.Next(1,maxRoles);
        while(numRoles > 0)
        {
            yield return _userRolesWithId.ElementAt(_random.Next(maxRoles)).Role;
            numRoles--;
        }
    }

    #endregion
}
