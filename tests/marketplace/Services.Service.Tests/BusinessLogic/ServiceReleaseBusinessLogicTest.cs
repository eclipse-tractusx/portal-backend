/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.ViewModels;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.Tests.BusinessLogic;

public class ServiceReleaseBusinessLogicTest
{
    private readonly IFixture _fixture;
    private readonly IOfferRepository _offerRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferService _offerService;
    private readonly IStaticDataRepository _staticDataRepository;
    private readonly ServiceReleaseBusinessLogic _sut;

    public ServiceReleaseBusinessLogicTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerRepository = A.Fake<IOfferRepository>();
        _offerService = A.Fake<IOfferService>();
        _staticDataRepository = A.Fake<IStaticDataRepository>();

        SetupRepositories();
        _sut = new ServiceReleaseBusinessLogic(_portalRepositories, _offerService);
    }

    [Fact]
    public async Task GetServiceAgreementData_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.CreateMany<AgreementDocumentData>(5).ToAsyncEnumerable();
        A.CallTo(() => _offerService.GetOfferTypeAgreements(OfferTypeId.SERVICE))
            .Returns(data);

        //Act
        var result = await _sut.GetServiceAgreementDataAsync().ToListAsync().ConfigureAwait(false);
        
        // Assert 
        A.CallTo(() => _offerService.GetOfferTypeAgreements(OfferTypeId.SERVICE))
            .MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetServiceDetailsByIdAsync_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.Build<ServiceDetailsData>()
                           .With(x=>x.OfferStatusId, OfferStatusId.IN_REVIEW)
                           .With(x=>x.Title, "ServiceTest")
                           .With(x=>x.Provider, "TestProvider")
                           .With(x=>x.ProviderUri, "TestProviderUri")
                           .With(x=>x.ContactEmail, "test@gmail.com")
                           .With(x=>x.ContactNumber, "6754321786")
                           .With(x=>x.ServiceTypeIds, new []{ServiceTypeId.CONSULTANCE_SERVICE.ToString(),ServiceTypeId.DATASPACE_SERVICE.ToString()})
                           .Create();
        var serviceId = _fixture.Create<Guid>();
       
        A.CallTo(() => _offerRepository.GetServiceDetailsByIdAsync(serviceId))
            .ReturnsLazily(() => data);

        //Act
        var result = await _sut.GetServiceDetailsByIdAsync(serviceId).ConfigureAwait(false);
        
        // Assert 
        A.CallTo(() => _offerRepository.GetServiceDetailsByIdAsync(A<Guid>._))
            .MustHaveHappenedOnceExactly();
        result.Should().BeOfType<ServiceData>();
        result.Title.Should().Be("ServiceTest");
        result.Provider.Should().Be("TestProvider");
        result.ProviderUri.Should().Be("TestProviderUri");
        result.ContactEmail.Should().Be("test@gmail.com");
        result.ContactNumber.Should().Be("6754321786");
    }

    [Fact]
    public async Task GetServiceDetailsByIdAsync_WithInvalidOfferStatus_ThrowsException()
    {
        // Arrange
        var data = _fixture.Build<ServiceDetailsData>()
                            .With(x=>x.OfferStatusId, OfferStatusId.CREATED)
                            .Create();
        var serviceId = _fixture.Create<Guid>();
       
        A.CallTo(() => _offerRepository.GetServiceDetailsByIdAsync(serviceId))
            .ReturnsLazily(() => data);

        // Act
        async Task Act() => await _sut.GetServiceDetailsByIdAsync(serviceId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"serviceId {serviceId} is incorrect status");
    }
    
    [Fact]
    public async Task GetServiceDetailsByIdAsync_WithInvalidServiceId_ThrowsException()
    {
        // Arrange
        var invalidServiceId = Guid.NewGuid();
        A.CallTo(() => _offerRepository.GetServiceDetailsByIdAsync(invalidServiceId))
           .ReturnsLazily(() => (ServiceDetailsData?)null);

        // Act
        async Task Act() => await _sut.GetServiceDetailsByIdAsync(invalidServiceId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"serviceId {invalidServiceId} does not exist");
    }

    [Fact]
    public async Task GetServiceTypeData_ReturnExpectedResult()
    {
        // Arrange
        var data = _fixture.Build<ServiceTypeData>()
                            .With(x=>x.ServiceTypeId, 1)
                            .With(x=>x.Name, ServiceTypeId.CONSULTANCE_SERVICE.ToString())
                            .CreateMany()
                            .ToAsyncEnumerable();
       
        A.CallTo(() => _staticDataRepository.GetServiceTypeData())
            .Returns(data);
        var sut = _fixture.Create<ServiceReleaseBusinessLogic>();

        // Act
        var result = await sut.GetServiceTypeDataAsync().ToListAsync().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _staticDataRepository.GetServiceTypeData())
            .MustHaveHappenedOnceExactly();
        result.Should().BeOfType<List<ServiceTypeData>>();
        result.FirstOrDefault()!.ServiceTypeId.Should().Be(1);
        result.FirstOrDefault()!.Name.Should().Be(ServiceTypeId.CONSULTANCE_SERVICE.ToString());
    }
    
    [Fact]
    public async Task GetServiceAgreementConsentAsync_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.Create<OfferAgreementConsent>();
        var serviceId = Guid.NewGuid();
        var iamUserId = _fixture.Create<string>();
        var offerService = A.Fake<IOfferService>();
        _fixture.Inject(offerService);
        A.CallTo(() => offerService.GetProviderOfferAgreementConsentById(A<Guid>._, A<string>._, OfferTypeId.SERVICE))
            .Returns(data);

        //Act
        var sut = _fixture.Create<ServiceReleaseBusinessLogic>();
        var result = await sut.GetServiceAgreementConsentAsync(serviceId, iamUserId).ConfigureAwait(false);
        
        // Assert 
        A.CallTo(() => offerService.GetProviderOfferAgreementConsentById(serviceId, iamUserId, OfferTypeId.SERVICE))
            .MustHaveHappenedOnceExactly();
        result.Should().BeOfType<OfferAgreementConsent>();
    } 

    [Fact]
    public async Task GetServiceTypeDataAsync_ReturnsExpected()
    {
        var serviceId = Guid.NewGuid();
        var iamUserId = Guid.NewGuid().ToString();
        var data = _fixture.Build<OfferProviderResponse>()
            .With(x => x.Title, "test title")
            .With(x => x.ContactEmail, "info@test.de")
            .Create();

        A.CallTo(() => _offerService.GetProviderOfferDetailsForStatusAsync(serviceId, iamUserId, OfferTypeId.SERVICE))
            .ReturnsLazily(() => data);

        var result = await _sut.GetServiceDetailsForStatusAsync(serviceId, iamUserId).ConfigureAwait(false);

        result.Should().NotBeNull();
        result.Title.Should().Be("test title");
        result.ContactEmail.Should().Be("info@test.de");
    }

    private void SetupRepositories()
    {
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IStaticDataRepository>()).Returns(_staticDataRepository);
        _fixture.Inject(_portalRepositories);
    }
}
