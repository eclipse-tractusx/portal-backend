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
    private static readonly Guid ExistingCompanyId = new("857b93b1-8fcb-4141-81b0-ae81950d489e");
    private static readonly Guid ExistingServiceProviderCompanyDetailId = new("5f68fdb2-991d-4222-ac31-d8ef2e42e8d0");
    private static readonly Guid ExistingDetailId = new("80a5491e-1189-4e35-99b6-6495641d06ef");

    private readonly ICompanyRepository _companyRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IFixture _fixture;
    private readonly ICollection<ProviderCompanyDetail> _serviceProviderDetails;

    public ServiceProviderBusinessLogicTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _serviceProviderDetails = new HashSet<ProviderCompanyDetail>();
            
        _portalRepositories = A.Fake<IPortalRepositories>();
        _companyRepository = A.Fake<ICompanyRepository>();
            
        SetupRepositories();
    }
        
    #region Create ServiceProviderCompanyDetails
        
    [Fact]
    public async Task CreateServiceProviderCompanyDetailsAsync_WithValidData_ReturnsExpectedResult()
    {
        //Arrange
        var serviceProviderDetailData = new ServiceProviderDetailData("https://www.service-url.com");
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        await sut.CreateServiceProviderCompanyDetailsAsync(serviceProviderDetailData, IamUserId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened(1, Times.OrMore);
        _serviceProviderDetails.Should().ContainSingle();
    }

    [Fact]
    public async Task CreateServiceProviderCompanyDetailsAsync_WithUnknownUser_ThrowsException()
    {
        //Arrange
        var serviceProviderDetailData = new ServiceProviderDetailData("https://www.service-url.com");
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        async Task Action() => await sut.CreateServiceProviderCompanyDetailsAsync(serviceProviderDetailData, Guid.NewGuid().ToString()).ConfigureAwait(false);

        //Assert
        await Assert.ThrowsAsync<ConflictException>(Action);
        _serviceProviderDetails.Should().BeEmpty();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateServiceProviderCompanyDetailsAsync_WithHttpUrl_ThrowsException()
    {
        //Arrange
        var serviceProviderDetailData = new ServiceProviderDetailData("http://www.service-url.com");
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        async Task Action() => await sut.CreateServiceProviderCompanyDetailsAsync(serviceProviderDetailData, IamUserId).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Url");
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        _serviceProviderDetails.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateServiceProviderCompanyDetailsAsync_WithEmptyUrl_ThrowsException()
    {
        //Arrange
        var serviceProviderDetailData = new ServiceProviderDetailData(string.Empty);
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        async Task Action() => await sut.CreateServiceProviderCompanyDetailsAsync(serviceProviderDetailData, IamUserId).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Url");
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        _serviceProviderDetails.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateServiceProviderCompanyDetailsAsync_WithToLongUrl_ThrowsException()
    {
        //Arrange
        var serviceProviderDetailData = new ServiceProviderDetailData("https://www.super-duper-long-url-which-is-actually-to-long-to-be-valid-but-it-is-not-long-enough-yet-so-add-a-few-words.com");
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        async Task Action() => await sut.CreateServiceProviderCompanyDetailsAsync(serviceProviderDetailData, IamUserId).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Url");
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        _serviceProviderDetails.Should().BeEmpty();
    }

    #endregion

    #region Get ServiceProviderCompanyDetails
        
    [Fact]
    public async Task GetServiceProviderCompanyDetailsAsync_WithValidIdAndUser_ReturnsDetails()
    {
        //Arrange
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        var result = await sut.GetServiceProviderCompanyDetailsAsync(ExistingDetailId, IamUserId).ConfigureAwait(false);

        //Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetServiceProviderCompanyDetailsAsync_WithInvalidServiceProviderDetailDataId_ThrowsException()
    {
        //Arrange
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        async Task Action() => await sut.GetServiceProviderCompanyDetailsAsync(Guid.NewGuid(), IamUserId).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
    }

    #endregion

    #region Update ServiceProviderCompanyDetails
        
    [Fact]
    public async Task UpdateServiceProviderCompanyDetailsAsync_WithValidData_ReturnsExpectedResult()
    {
        //Arrange
        var serviceProviderDetailData = new ServiceProviderDetailData("https://www.service-url.com");
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        await sut.UpdateServiceProviderCompanyDetailsAsync(ExistingServiceProviderCompanyDetailId, serviceProviderDetailData, IamUserId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened(1, Times.OrMore);
    }

    [Fact]
    public async Task UpdateServiceProviderCompanyDetailsAsync_WithUnknownUser_ThrowsException()
    {
        //Arrange
        var serviceProviderDetailData = new ServiceProviderDetailData("https://www.service-url.com");
        var serviceProviderDetailDataId = Guid.NewGuid();
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        async Task Action() => await sut.UpdateServiceProviderCompanyDetailsAsync(serviceProviderDetailDataId, serviceProviderDetailData, Guid.NewGuid().ToString()).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"ServiceProviderDetailData {serviceProviderDetailDataId} does not exists.");
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateServiceProviderCompanyDetailsAsync_WithNotExistingServiceProviderCompanyDetails_ThrowsNotFoundException()
    {
        //Arrange
        var serviceProviderDetailData = new ServiceProviderDetailData("https://www.service-url.com");
        var serviceProviderDetailDataId = Guid.NewGuid();
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        async Task Action() => await sut.UpdateServiceProviderCompanyDetailsAsync(serviceProviderDetailDataId, serviceProviderDetailData, Guid.NewGuid().ToString()).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"ServiceProviderDetailData {serviceProviderDetailDataId} does not exists.");
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateServiceProviderCompanyDetailsAsync_WithHttpUrl_ThrowsException()
    {
        //Arrange
        var serviceProviderDetailData = new ServiceProviderDetailData("http://www.service-url.com");
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        async Task Action() => await sut.UpdateServiceProviderCompanyDetailsAsync(ExistingServiceProviderCompanyDetailId, serviceProviderDetailData, IamUserId).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Url");
        _serviceProviderDetails.Should().BeEmpty();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateServiceProviderCompanyDetailsAsync_WithEmptyUrl_ThrowsException()
    {
        //Arrange
        var serviceProviderDetailData = new ServiceProviderDetailData(string.Empty);
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        async Task Action() => await sut.UpdateServiceProviderCompanyDetailsAsync(ExistingServiceProviderCompanyDetailId, serviceProviderDetailData, IamUserId).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Url");
        _serviceProviderDetails.Should().BeEmpty();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateServiceProviderCompanyDetailsAsync_WithToLongUrl_ThrowsException()
    {
        //Arrange
        var serviceProviderDetailData = new ServiceProviderDetailData("https://www.super-duper-long-url-which-is-actually-to-long-to-be-valid-but-it-is-not-long-enough-yet-so-add-a-few-words.com");
        var sut = _fixture.Create<ServiceProviderBusinessLogic>();
            
        //Act
        async Task Action() => await sut.UpdateServiceProviderCompanyDetailsAsync(ExistingServiceProviderCompanyDetailId, serviceProviderDetailData, IamUserId).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Url");
        _serviceProviderDetails.Should().BeEmpty();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    #endregion

    #region Setup

    private void SetupRepositories()
    {
        A.CallTo(() => _companyRepository.GetCompanyIdMatchingRoleAndIamUserOrTechnicalUserAsync(A<string>.That.Matches(x => x == IamUserId), A<CompanyRoleId>._))
            .ReturnsLazily(() => (ExistingCompanyId,true));
        A.CallTo(() => _companyRepository.GetCompanyIdMatchingRoleAndIamUserOrTechnicalUserAsync(A<string>.That.Not.Matches(x => x == IamUserId), A<CompanyRoleId>._))
            .ReturnsLazily(() => ((Guid,bool))default);

        A.CallTo(() => _companyRepository.CreateProviderCompanyDetail(A<Guid>._, A<string>._))
            .Invokes(x =>
            {
                var companyId = x.Arguments.Get<Guid>("companyId");
                var dataUrl = x.Arguments.Get<string>("dataUrl")!;
                var providerCompanyDetail = new ProviderCompanyDetail(Guid.NewGuid(), companyId, dataUrl, DateTimeOffset.UtcNow);
                _serviceProviderDetails.Add(providerCompanyDetail);
            });
        
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailAsync(A<Guid>.That.Matches(x => x == ExistingDetailId), A<CompanyRoleId>.That.Matches(x => x == CompanyRoleId.SERVICE_PROVIDER), A<string>.That.Matches(x => x == IamUserId)))
            .ReturnsLazily(() => (new ProviderDetailReturnData(Guid.NewGuid(), Guid.NewGuid(), "https://new-test-service.de"),true,true));
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailAsync(A<Guid>.That.Not.Matches(x => x == ExistingDetailId), A<CompanyRoleId>.That.Matches(x => x == CompanyRoleId.SERVICE_PROVIDER), A<string>.That.Matches(x => x == IamUserId)))
            .ReturnsLazily(() => ((ProviderDetailReturnData,bool,bool))default);
        
        A.CallTo(() => _companyRepository.CheckProviderCompanyDetailsExistsForUser(A<string>.That.Matches(x => x == IamUserId), A<Guid>.That.Matches(x => x == ExistingServiceProviderCompanyDetailId)))
            .ReturnsLazily(() => (true,true));
        A.CallTo(() => _companyRepository.CheckProviderCompanyDetailsExistsForUser(A<string>.That.Not.Matches(x => x == IamUserId), A<Guid>.That.Matches(x => x == ExistingServiceProviderCompanyDetailId)))
            .ReturnsLazily(() => (true,false));
        A.CallTo(() => _companyRepository.CheckProviderCompanyDetailsExistsForUser(A<string>.That.Matches(x => x == IamUserId), A<Guid>.That.Not.Matches(x => x == ExistingServiceProviderCompanyDetailId)))
            .ReturnsLazily(() => ((bool,bool))default);
        A.CallTo(() => _companyRepository.CheckProviderCompanyDetailsExistsForUser(A<string>.That.Not.Matches(x => x == IamUserId), A<Guid>.That.Not.Matches(x => x == ExistingServiceProviderCompanyDetailId)))
            .ReturnsLazily(() => ((bool,bool))default);

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        _fixture.Inject(_portalRepositories);
    }

    #endregion
}
