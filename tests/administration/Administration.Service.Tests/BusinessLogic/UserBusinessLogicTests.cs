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
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class UserBusinessLogicTests
{
    private const string IamClientId = "Cl1-CX-Registration";
    private const string AdminIamUser = "9aae7a3b-b188-4a42-b46b-fb2ea5f47664";
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
        _fixture.Inject(_portalRepositories);
    }

    #region Modify UserRole Async

    [Fact(Skip = "Will be implemented")]
    public async Task ModifyUserRoleAsync_WithValidDataAndEmptyDescriptions_ReturnsCorrectDetails()
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
        true.Should().BeFalse();
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

        A.CallTo(() => _offerRepository.GetAppAssignedClientIdUntrackedAsync(A<Guid>.That.Matches(x => x == _validOfferId), A<Guid>._))
            .ReturnsLazily(() => IamClientId);
        A.CallTo(() => _offerRepository.GetAppAssignedClientIdUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _validOfferId), A<Guid>._))
            .ReturnsLazily(() => (string?)null);

        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
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
