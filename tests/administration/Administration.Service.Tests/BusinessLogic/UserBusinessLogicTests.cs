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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Models;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Models;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class UserBusinessLogicTests
{
    #region Initialization
    
    private const string IamClientId = "Cl1-CX-Registration";
    private const string AdminIamUser = "9aae7a3b-b188-4a42-b46b-fb2ea5f47664";
    private const string IamUserWithoutCompanyName = "3812d567-5e12-4b38-90ac-2a8d56ad8862";
    private const string IamUserWithoutIdpAlias = "3bf112ea-e2f8-45ae-b4a6-aaeef8e1d76d";
    private const string CreatedCentralUserId = "a30cebe9-53db-4dcd-a3e8-8dc7aa49fb73";
    private const string IdpName = "Company-1";
    private readonly Guid _companyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");
    private readonly Guid _adminRoleId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
    private readonly Guid _buyerRoleId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47662");
    private readonly Guid _supplierRoleId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47663");
    private readonly Guid _validOfferId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47665");
    private readonly Guid _noTargetIamUserSet = new("9b486e95-4a23-4667-ad1a-de16ec44c21c");
    
    private readonly CompanyUser _companyUser;
    private readonly IFixture _fixture;
    private readonly IamUser _iamUser;
    private readonly IOfferRepository _offerRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly ICollection<CompanyUserAssignedRole> _companyUserAssignedRole = new HashSet<CompanyUserAssignedRole>();
    private readonly IProvisioningManager _provisioningManager;

    public UserBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var (companyUser, iamUser) = CreateTestUserPair();
        _companyUser = companyUser;
        _iamUser = iamUser;

        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerRepository = A.Fake<IOfferRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _provisioningManager = A.Fake<IProvisioningManager>();

        _fixture.Inject(Options.Create(new UserSettings
        {
            Portal = new UserSetting
            {
                BasePortalAddress = "https://base-porta-address.com",
                KeyCloakClientID = "CatenaX"   
            },
            PasswordReset = new PasswordReset
            {
                NoOfHours = 2,
                MaxNoOfReset = 2
            },
            ApplicationsMaxPageSize = 15
        }));
        
        SetupRepositories();
        SetupServices();
        _fixture.Inject(_portalRepositories);
    }
    
    #endregion

    #region CreateOwnCompanyUsers
    
    [Fact]
    public async Task CreateOwnCompanyUsersAsync_WithValidData_ReturnsExceptedResult()
    {
        // Arrange
        var sut = _fixture.Create<UserBusinessLogic>();
        var iamUserId = Guid.NewGuid().ToString();

        // Act
        var results = await sut.CreateOwnCompanyUsersAsync(new []
        {
            new UserCreationInfo("ironMan", "tony@stark.com", "Tony", "Stark", new List<string>
                {
                    "Company Admin"
                }, 
                iamUserId)
        }, _iamUser.UserEntityId).ToListAsync().ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateOwnCompanyUsersAsync_WithUnassignedUser_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var sut = _fixture.Create<UserBusinessLogic>();
        var iamUserId = Guid.NewGuid().ToString();

        // Act
        var results = sut.CreateOwnCompanyUsersAsync(new[]
        {
            new UserCreationInfo("ironMan", "tony@stark.com", "Tony", "Stark", new List<string>
                {
                    "Company Admin"
                },
                iamUserId)
        }, iamUserId);

        // Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await results.ToListAsync().ConfigureAwait(false));
    }
    
    [Fact]
    public async Task CreateOwnCompanyUsersAsync_WithUserWithouCompany_ThrowsException()
    {
        // Arrange
        var sut = _fixture.Create<UserBusinessLogic>();

        // Act
        var results = sut.CreateOwnCompanyUsersAsync(new[]
        {
            new UserCreationInfo("ironMan", "tony@stark.com", "Tony", "Stark", new List<string>
                {
                    "Company Admin"
                },
                IamUserWithoutCompanyName)
        }, IamUserWithoutCompanyName);

        // Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await results.ToListAsync().ConfigureAwait(false));
        ex.Message.Should().Be($"assertion failed: companyName of company {_companyUserCompanyId} should never be null here");
    }

    [Fact]
    public async Task CreateOwnCompanyUsersAsync_WithUserWithouIdAlias_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var sut = _fixture.Create<UserBusinessLogic>();

        // Act
        var results = sut.CreateOwnCompanyUsersAsync(new[]
        {
            new UserCreationInfo("ironMan", "tony@stark.com", "Tony", "Stark", new List<string>
                {
                    "Company Admin"
                },
                IamUserWithoutIdpAlias)
        }, IamUserWithoutIdpAlias);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await results.ToListAsync().ConfigureAwait(false));
        ex.Message.Should().Be($"Specified argument was out of the range of valid values. (Parameter 'user {IamUserWithoutIdpAlias} is not associated with any shared idp')");
    }

    [Fact]
    public async Task CreateOwnCompanyUsersAsync_WitInvalidRole_ThrowsArgumentException()
    {
        // Arrange
        var sut = _fixture.Create<UserBusinessLogic>();
        var invalidRole = "invalidRole";

        // Act
        var results = sut.CreateOwnCompanyUsersAsync(new[]
        {
            new UserCreationInfo("ironMan", "tony@stark.com", "Tony", "Stark", new List<string>
                {
                    invalidRole
                },
                IamUserWithoutIdpAlias)
        }, _iamUser.UserEntityId);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await results.ToListAsync().ConfigureAwait(false));
        ex.Message.Should().Be($"invalid Role: {invalidRole}");
    }

    #endregion
    
    #region Modify UserRole Async

    [Fact]
    public async Task ModifyUserRoleAsync_WithTwoNewRoles_AddsTwoRolesToTheDatabase()
    {
        // Arrange
        var sut = _fixture.Create<UserBusinessLogic>();

        // Act
        var userRoleInfo = new UserRoleInfo(_companyUser.Id, new []
        {
            "Company Admin",
            "Buyer",
            "Supplier"
        });
        await sut.ModifyUserRoleAsync(_validOfferId, userRoleInfo, AdminIamUser).ConfigureAwait(false);

        // Assert
        _companyUserAssignedRole.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task ModifyUserRoleAsync_WithOneRoleToDelete_DeletesTheRole()
    {
        // Arrange
        var sut = _fixture.Create<UserBusinessLogic>();

        // Act
        var userRoleInfo = new UserRoleInfo(_companyUser.Id, new []
        {
            "Company Admin",
            "Buyer"
        });
        await sut.ModifyUserRoleAsync(_validOfferId, userRoleInfo, AdminIamUser).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalRepositories.RemoveRange(A<IEnumerable<CompanyUserAssignedRole>>.That.Matches(x => x.Count() == 1))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateServiceOffering_WithNotFoundCompanyUser_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<UserBusinessLogic>();

        // Act
        var userRoleInfo = new UserRoleInfo(Guid.NewGuid(), new []
        {
            "Company Admin",
            "Buyer",
            "Supplier"
        });
        async Task Action() => await sut.ModifyUserRoleAsync(_validOfferId, userRoleInfo, AdminIamUser).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Cannot identify companyId or shared idp : companyUserId {userRoleInfo.CompanyUserId} is not associated with the same company as adminUserId {AdminIamUser}");
    }

    [Fact]
    public async Task CreateServiceOffering_WithoutTargetUser_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<UserBusinessLogic>();

        // Act
        var userRoleInfo = new UserRoleInfo(_noTargetIamUserSet, new []
        {
            "Company Admin",
            "Buyer",
            "Supplier"
        });
        async Task Action() => await sut.ModifyUserRoleAsync(_validOfferId, userRoleInfo, AdminIamUser).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task CreateServiceOffering_WithInvalidOfferId_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<UserBusinessLogic>();
        var invalidAppId = Guid.NewGuid();

        // Act
        var userRoleInfo = new UserRoleInfo(_companyUser.Id, new []
        {
            "Company Admin",
            "Buyer",
            "Supplier"
        });
        async Task Action() => await sut.ModifyUserRoleAsync(invalidAppId, userRoleInfo, AdminIamUser).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Action);
        ex.ParamName.Should().Be("appId");
    }

    #endregion

    #region Setup

    private void SetupRepositories()
    {
        A.CallTo(() =>
                _userRepository.GetIdpUserByIdUntrackedAsync(A<Guid>.That.Matches(x => x == _companyUser.Id), A<string>._))
            .ReturnsLazily(() => new CompanyIamUser(_companyUser.CompanyId, new[]
            {
                _adminRoleId,
                _buyerRoleId,
                _supplierRoleId
            })
            {
                TargetIamUserId = _iamUser.UserEntityId,
                IdpName = IdpName
            });
        A.CallTo(() =>
                _userRepository.GetIdpUserByIdUntrackedAsync(A<Guid>.That.Matches(x => x == _noTargetIamUserSet), A<string>._))
            .ReturnsLazily(() => new CompanyIamUser(_companyUser.CompanyId, new[]
            {
                _adminRoleId,
                _buyerRoleId,
                _supplierRoleId
            })
            {
                IdpName = IdpName
            });
        A.CallTo(() => _userRepository.GetIdpUserByIdUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _noTargetIamUserSet || x == _companyUser.Id), A<string>._))
            .ReturnsLazily(() => (CompanyIamUser?)null);

        A.CallTo(() => _userRepository.GetCompanyNameIdpAliaseUntrackedAsync(
                A<string>.That.Matches(x => x == _iamUser.UserEntityId),
                A<IdentityProviderCategoryId>.That.Matches(x => x == IdentityProviderCategoryId.KEYCLOAK_SHARED)))
            .ReturnsLazily(() => new ValueTuple<Guid, string?, string?, IEnumerable<string>>(_companyUserCompanyId,
                "Catena", "Catena X", new List<string> {"central"}));
        A.CallTo(() => _userRepository.GetCompanyNameIdpAliaseUntrackedAsync(
                A<string>.That.Matches(x => x == IamUserWithoutCompanyName),
                A<IdentityProviderCategoryId>.That.Matches(x => x == IdentityProviderCategoryId.KEYCLOAK_SHARED)))
            .ReturnsLazily(() => new ValueTuple<Guid, string?, string?, IEnumerable<string>>(_companyUserCompanyId, null, "1234", new List<string>()));
        A.CallTo(() => _userRepository.GetCompanyNameIdpAliaseUntrackedAsync(
                A<string>.That.Matches(x => x == IamUserWithoutIdpAlias),
                A<IdentityProviderCategoryId>.That.Matches(x => x == IdentityProviderCategoryId.KEYCLOAK_SHARED)))
            .ReturnsLazily(() => new ValueTuple<Guid, string?, string?, IEnumerable<string>>(_companyUserCompanyId, "CompanyName", "1234", new List<string>()));

        A.CallTo(() => _userRolesRepository.GetUserRoleWithIdsUntrackedAsync(A<string>._, A<IEnumerable<string>>.That.Matches(x => x.Any(y => y == "Company Admin"))))
            .ReturnsLazily(() => new List<UserRoleWithId> { new("Company Admin", Guid.NewGuid()) }.ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetUserRoleWithIdsUntrackedAsync(A<string>._, A<IEnumerable<string>>.That.Not.Matches(x => x.Any(y => y == "Company Admin"))))
            .ReturnsLazily(() => new List<UserRoleWithId>().ToAsyncEnumerable());

        A.CallTo(() => _userRepository.GetCompanyUserIdAndEmailForIamUserUntrackedAsync(A<string>.That.Matches(x => x == _iamUser.UserEntityId)))
            .ReturnsLazily(() => (_companyUser.Id, "test@mail.de"));
        A.CallTo(() => _userRepository.GetCompanyUserIdAndEmailForIamUserUntrackedAsync(A<string>.That.Not.Matches(x => x == _iamUser.UserEntityId)))
            .ReturnsLazily(() => new ValueTuple<Guid, string>());
        
        A.CallTo(() => _offerRepository.GetAppAssignedClientIdUntrackedAsync(A<Guid>.That.Matches(x => x == _validOfferId), A<Guid>._))
            .ReturnsLazily(() => IamClientId);
        A.CallTo(() => _offerRepository.GetAppAssignedClientIdUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _validOfferId), A<Guid>._))
            .ReturnsLazily(() => (string?)null);
        A.CallTo(() => _userRolesRepository.GetRolesToAdd(A<Guid>._, A<IEnumerable<string>>.That.Matches(x => x.Contains("Buyer") && x.Contains("Company Admin")), A<Guid>._))
            .ReturnsLazily(() => new List<UserRoleWithId> { new("Buyer", _buyerRoleId), new("Company Admin", _adminRoleId) }.ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetRolesToAdd(A<Guid>._, A<IEnumerable<string>>.That.Matches(x => x.Contains("Buyer") && !x.Contains("Company Admin")), A<Guid>._))
            .ReturnsLazily(() => new List<UserRoleWithId> { new("Buyer", _buyerRoleId) }.ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetRolesToAdd(A<Guid>._, A<IEnumerable<string>>.That.Matches(x => !x.Contains("Buyer") && x.Contains("Company Admin")), A<Guid>._))
            .ReturnsLazily(() => new List<UserRoleWithId> { new("Company Admin", _adminRoleId) }.ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetRolesToAdd(A<Guid>._, A<IEnumerable<string>>.That.Matches(x => !x.Contains("Buyer") && !x.Contains("Company Admin")), A<Guid>._))
            .ReturnsLazily(() => new List<UserRoleWithId>().ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetAssignedRolesForDeletion(A<Guid>._, A<IEnumerable<string>>.That.Matches(x => !x.Contains("Supplier")), A<Guid>._))
            .ReturnsLazily(() => new List<UserRoleWithId> { new("Supplier", _buyerRoleId) }.ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetAssignedRolesForDeletion(A<Guid>._, A<IEnumerable<string>>.That.Matches(x => x.Contains("Supplier")), A<Guid>._))
            .ReturnsLazily(() => new List<UserRoleWithId>().ToAsyncEnumerable());

        A.CallTo(() => _userRolesRepository.CreateCompanyUserAssignedRole(A<Guid>._, A<Guid>._))
            .Invokes(x =>
            {
                var companyUserId = x.Arguments.Get<Guid>("companyUserId");
                var companyUserRoleId = x.Arguments.Get<Guid>("companyUserRoleId");

                var companyUserAssignedRole = new CompanyUserAssignedRole(companyUserId, companyUserRoleId);
                _companyUserAssignedRole.Add(companyUserAssignedRole);
            });

        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
    }

    private void SetupServices()
    {
        A.CallTo(() => _provisioningManager.CreateSharedUserLinkedToCentralAsync(A<string>.That.Matches(x => x == "central"), A<UserProfile>._, A<IEnumerable<(string, IEnumerable<string>)>>._))
            .ReturnsLazily(() => CreatedCentralUserId);
        A.CallTo(() => _provisioningManager.CreateSharedUserLinkedToCentralAsync(A<string>.That.Not.Matches(x => x == "central"), A<UserProfile>._, A<IEnumerable<(string, IEnumerable<string>)>>._))
            .ReturnsLazily(() => Guid.NewGuid().ToString());
        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(A<string>.That.Matches(x => x == CreatedCentralUserId), A<IDictionary<string, IEnumerable<string>>>._))
            .ReturnsLazily(() => new Dictionary<string, IEnumerable<string>>
            {
                {"central", new List<string> {"Company Admin"}}
            });
        A.CallTo(() => _provisioningManager.AssignClientRolesToCentralUserAsync(
                A<string>.That.Not.Matches(x => x == CreatedCentralUserId), A<IDictionary<string, IEnumerable<string>>>._))
            .ReturnsLazily(() => new Dictionary<string, IEnumerable<string>>());
        _fixture.Inject(_provisioningManager);
    }

    private (CompanyUser, IamUser) CreateTestUserPair()
    {
        var companyUser = _fixture.Build<CompanyUser>()
            .Without(u => u.IamUser)
            .With(u => u.CompanyId, _companyUserCompanyId)
            .Create();
        var iamUser = _fixture.Build<IamUser>()
            .With(u => u.CompanyUser, companyUser)
            .Create();
        companyUser.IamUser = iamUser;
        companyUser.Company = new Company(Guid.NewGuid(), "The Company", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow);
        return (companyUser, iamUser);
    }

    #endregion
}
