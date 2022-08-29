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
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Service.Service.BusinessLogic;
using CatenaX.NetworkServices.Tests.Shared;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace CatenaX.NetworkServices.Service.Service.Test.BusinessLogic;

public class ServiceBusinessLogicTests
{
    private readonly CompanyUser _companyUser;
    private readonly IFixture _fixture;
    private readonly IamUser _iamUser;
    private readonly IAppRepository _appRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IUserRepository _userRepository;

    public ServiceBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var (companyUser, iamUser) = CreateTestUserPair();
        _companyUser = companyUser;
        _iamUser = iamUser;

        _portalRepositories = A.Fake<IPortalRepositories>();
        _appRepository = A.Fake<IAppRepository>();
        _userRepository = A.Fake<IUserRepository>();

        SetupRepositories(companyUser, iamUser);
    }

    #region Create Service

    [Fact]
    public async Task CreateNotification_WithValidData_ReturnsCorrectDetails()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        var apps = new List<App>();
        A.CallTo(() => _appRepository.CreateApp(A<string>._, A<AppTypeId>._, A<Action<App?>>._))
            .Invokes(x =>
            {
                var provider = x.Arguments.Get<string>("provider");
                var appTypeId = x.Arguments.Get<AppTypeId>("appType");
                var action = x.Arguments.Get<Action<App?>>("setOptionalParameters");

                var app = new App(serviceId, provider!, DateTimeOffset.UtcNow, appTypeId);
                action?.Invoke(app);
                apps.Add(app);
            })
            .Returns(new App(serviceId)
            {
                AppTypeId = AppTypeId.SERVICE 
            });
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.CreateServiceOffering(new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<ServiceDescription>()), _iamUser.UserEntityId);

        // Assert
        result.Should().Be(serviceId);
        apps.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateNotification_WithWrongIamUser_ReturnsCorrectDetails()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.CreateServiceOffering(new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<ServiceDescription>()), Guid.NewGuid().ToString());
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    #endregion

    #region Get Active Services

    [Fact]
    public async Task ServiceBusinessLogic_GetAllActiveServicesAsync_GetsExpectedEntries()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetAllActiveServicesAsync(0, 15);

        // Assert
        result.Content.Should().HaveCount(5);
    }

    
    [Fact]
    public async Task ServiceBusinessLogic_WithSmallSize_GetsExpectedEntries()
    {
        // Arrange
        const int expectedCount = 3;
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetAllActiveServicesAsync(0, expectedCount);

        // Assert
        result.Content.Should().HaveCount(expectedCount);
    }

    #endregion

    #region Setup

    private void SetupRepositories(CompanyUser companyUser, IamUser iamUser)
    {
        var serviceDetailData = new AsyncEnumerableStub<ServiceDetailData>(_fixture.CreateMany<ServiceDetailData>(5));
        
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(iamUser.UserEntityId, companyUser.Id))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName)>{new (_companyUser.Id, true, "COMPANYBPN"), new (_companyUser.Id, false, "OTHERCOMPANYBPN")}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(iamUser.UserEntityId, A<Guid>.That.Not.Matches(x => x == companyUser.Id)))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName)>{new (_companyUser.Id, true, "COMPANYBPN")}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(A<string>.That.Not.Matches(x => x == iamUser.UserEntityId), companyUser.Id))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName)>{new (_companyUser.Id, false, "OTHERCOMPANYBPN")}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(A<string>.That.Not.Matches(x => x == iamUser.UserEntityId), A<Guid>.That.Not.Matches(x => x == companyUser.Id)))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName)>().ToAsyncEnumerable());
        A.CallTo(() => _appRepository.GetActiveServices())
            .Returns(serviceDetailData.AsQueryable());

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IAppRepository>()).Returns(_appRepository);
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

    #endregion
}
