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
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Service;
using FakeItEasy;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Tests;

public class UserProvisioningServiceTests
{
    private readonly IFixture _fixture;
    private readonly Random _random;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly CompanyNameIdpAliasData _companyNameIdpAliasData;
    private readonly CompanyNameIdpAliasData _companyNameIdpAliasDataSharedIdp;
    private readonly string _clientId;
    private readonly IEnumerable<UserCreationInfoIdp> _userCreationInfoIdp;
    private readonly IEnumerable<(string Role, Guid Id)> _userRolesWithId;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public UserProvisioningServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _random = new Random();

        _companyNameIdpAliasData = _fixture.Build<CompanyNameIdpAliasData>().With(x => x.IsSharedIdp, false).Create();
        _companyNameIdpAliasDataSharedIdp = _fixture.Build<CompanyNameIdpAliasData>().With(x => x.IsSharedIdp, true).Create();

        _clientId = _fixture.Create<string>();
        _cancellationTokenSource = new CancellationTokenSource();
        _userRolesWithId = _fixture.CreateMany<(string,Guid)>(5).ToList();
        _userCreationInfoIdp = CreateUserCreationInfoIdp(_userRolesWithId.Select(u => u.Role),10);

        _portalRepositories = A.Fake<IPortalRepositories>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();

        SetupRepositories();
        SetupProvisioningManager();

        _fixture.Inject(_portalRepositories);
        _fixture.Inject(_provisioningManager);
    }

    [Fact]
    public async void TestFixtureSetup()
    {
        var sut = _fixture.Create<UserProvisioningService>();

        var result = await sut.CreateOwnCompanyIdpUsersAsync(
            _companyNameIdpAliasData,
            _clientId,
            _userCreationInfoIdp.ToAsyncEnumerable(),
            _cancellationTokenSource.Token
        ).ToListAsync().ConfigureAwait(false);

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).MustHaveHappenedOnceOrMore();
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).MustHaveHappenedOnceOrMore();
        A.CallTo(() => _provisioningManager.CreateCentralUserAsync(A<UserProfile>._, A<IEnumerable<(string,IEnumerable<string>)>>._)).MustHaveHappenedOnceOrMore();
        A.CallTo(() => _provisioningManager.AddProviderUserLinkToCentralUserAsync(A<string>._, A<IdentityProviderLink>._)).MustHaveHappenedOnceOrMore();
        A.CallTo(() => _userRepository.CreateIamUser(A<Guid>._, A<string>._)).MustHaveHappenedOnceOrMore();
    }

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

    private IEnumerable<UserCreationInfoIdp> CreateUserCreationInfoIdp(IEnumerable<string> roles, int numUsers)
    {
        var numUsersToCreate = numUsers;
        while (numUsersToCreate > 0)
        {
            yield return _fixture.Build<UserCreationInfoIdp>().With(x => x.Roles, PickRoles(roles)).Create();
            numUsersToCreate--;
        }
    }

    private IEnumerable<string> PickRoles(IEnumerable<string> roles)
    {
        var maxRoles = roles.Count();
        var numRoles = _random.Next(maxRoles);
        while(numRoles > 0)
        {
            yield return roles.ElementAt(_random.Next(maxRoles));
            numRoles--;
        }
    }
}
