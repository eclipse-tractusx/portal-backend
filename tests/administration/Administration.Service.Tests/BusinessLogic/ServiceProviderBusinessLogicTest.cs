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
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class ServiceProviderBusinessLogicTest
{
    private static readonly string IamUserId = new Guid("4C1A6851-D4E7-4E10-A011-3732CD045E8A").ToString();
    private static readonly Guid _existingCompanyId = new("857b93b1-8fcb-4141-81b0-ae81950d489e");
    private static readonly Guid _notServiceProviderCompanyId = new("857b93b1-8fcb-4141-81b0-ae81950d9487");

    private readonly ICompanyRepository _companyRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IFixture _fixture;
    private ICollection<ServiceProviderCompanyDetail> _serviceProviderDetails;

    public ServiceProviderBusinessLogicTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _serviceProviderDetails = new HashSet<ServiceProviderCompanyDetail>();
            
        _portalRepositories = A.Fake<IPortalRepositories>();
        _companyRepository = A.Fake<ICompanyRepository>();
            
        SetupRepositories();
    }
        
    #region Create ServiceProviderCompanyDetails
        
    [Fact]
    public async Task ApprovePartnerRequest_WithCompanyAdminUser_ApprovesRequestAndCreatesNotifications()
    {
        //Arrange
        var serviceProviderDetailData = new ServiceProviderDetailData("https://www.service-url.com");
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        await sut.CreateServiceProviderCompanyDetails(_existingCompanyId, serviceProviderDetailData, IamUserId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened(1, Times.OrMore);
        _serviceProviderDetails.Should().ContainSingle();
    }

    [Fact]
    public async Task ApprovePartnerRequest_WithCompanyThatIsntServiceProvider_ThrowsException()
    {
        //Arrange
        var serviceProviderDetailData = new ServiceProviderDetailData("https://www.service-url.com");
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        async Task Action() => await sut.CreateServiceProviderCompanyDetails(_notServiceProviderCompanyId, serviceProviderDetailData, IamUserId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
        _serviceProviderDetails.Should().BeEmpty();
    }

    [Fact]
    public async Task ApprovePartnerRequest_WithHttpUrl_ThrowsException()
    {
        //Arrange
        var serviceProviderDetailData = new ServiceProviderDetailData("http://www.service-url.com");
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        async Task Action() => await sut.CreateServiceProviderCompanyDetails(_existingCompanyId, serviceProviderDetailData, IamUserId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Url");
        _serviceProviderDetails.Should().BeEmpty();
    }

    [Fact]
    public async Task ApprovePartnerRequest_WithEmptyUrl_ThrowsException()
    {
        //Arrange
        var serviceProviderDetailData = new ServiceProviderDetailData(string.Empty);
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        async Task Action() => await sut.CreateServiceProviderCompanyDetails(_existingCompanyId, serviceProviderDetailData, IamUserId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Url");
        _serviceProviderDetails.Should().BeEmpty();
    }

    [Fact]
    public async Task ApprovePartnerRequest_WithToLongUrl_ThrowsException()
    {
        //Arrange
        var serviceProviderDetailData = new ServiceProviderDetailData("https://www.super-duper-long-url-which-is-actually-to-long-to-be-valid-but-it-is-not-long-enough-yet-so-add-a-few-words.com");
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        async Task Action() => await sut.CreateServiceProviderCompanyDetails(_existingCompanyId, serviceProviderDetailData, IamUserId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Url");
        _serviceProviderDetails.Should().BeEmpty();
    }

    #endregion
        
    #region Setup

    private void SetupRepositories()
    {
        A.CallTo(() => _companyRepository.CheckCompanyIsServiceProviderAndExistsForIamUser(A<Guid>.That.Matches(x => x == _existingCompanyId), A<string>._, A<CompanyRoleId>._))
            .ReturnsLazily(() => true);
        A.CallTo(() => _companyRepository.CheckCompanyIsServiceProviderAndExistsForIamUser(A<Guid>.That.Matches(x => x == _notServiceProviderCompanyId), A<string>._, A<CompanyRoleId>._))
            .ReturnsLazily(() => false);

        A.CallTo(() => _companyRepository.CreateServiceProviderCompanyDetail(A<Guid>._, A<string>._))
            .Invokes(x =>
            {
                var companyId = x.Arguments.Get<Guid>("companyId");
                var dataUrl = x.Arguments.Get<string>("dataUrl")!;
                var serviceProviderCompanyDetail = new ServiceProviderCompanyDetail(Guid.NewGuid(), companyId, dataUrl, DateTimeOffset.UtcNow);
                _serviceProviderDetails.Add(serviceProviderCompanyDetail);
            });
            
            
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
    }

    #endregion
}