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

using System.Net;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Offers.Library.Tests.Service;

public class OfferSetupServiceTests
{
    private readonly Guid _companyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");
    private readonly Guid _existingServiceId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
    private readonly Guid _existingServiceOfferId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47662");
    private readonly string _accessToken = "THISISAACCESSTOKEN";
    private readonly IFixture _fixture;
    private readonly IamUser _iamUser;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IHttpClientFactory _httpClientFactory;

    public OfferSetupServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _iamUser = CreateTestUserPair();

        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _httpClientFactory = A.Fake<IHttpClientFactory>();

        SetupRepositories();
    }

    #region Get Service Agreement

    [Fact]
    public async Task AutoSetupOffer_WithSuccessfullyCall_ReturnsTrue()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK);
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _httpClientFactory.CreateClient(A<string>._)).Returns(httpClient);
        _fixture.Inject(_httpClientFactory);
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<OfferSetupService>();

        // Act
        await sut.AutoSetupOffer(_existingServiceId, _iamUser.UserEntityId, _accessToken, "https://www.superservice.com").ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerSubscriptionsRepository.GetThirdPartyAutoSetupDataAsync(
            A<Guid>.That.Matches(x => x == _existingServiceId),
            A<string>.That.Matches(x => x == _iamUser.UserEntityId))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AutoSetupOffer_WithNotExistingServiceId_ThrowsArgumentException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _httpClientFactory.CreateClient(A<string>._)).Returns(httpClient);
        _fixture.Inject(_httpClientFactory);
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<OfferSetupService>();

        // Act
        async Task Action() => await sut.AutoSetupOffer(Guid.NewGuid(), _iamUser.UserEntityId, _accessToken, "https://www.superservice.com").ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
    }

    [Fact]
    public async Task AutoSetupOffer_WithNotAssociatedUserId_ThrowsArgumentException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _httpClientFactory.CreateClient(A<string>._)).Returns(httpClient);
        _fixture.Inject(_httpClientFactory);
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<OfferSetupService>();

        // Act
        async Task Action() => await sut.AutoSetupOffer(_existingServiceId, "not existing userid", _accessToken, "https://www.superservice.com").ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Action);
    }

    [Fact]
    public async Task AutoSetupOffer_WithNonSuccessfullyClientCall_ThrowsServiceException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _httpClientFactory.CreateClient(A<string>._)).Returns(httpClient);
        _fixture.Inject(_httpClientFactory);
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<OfferSetupService>();

        // Act
        async Task Action() => await sut.AutoSetupOffer(_existingServiceId, _iamUser.UserEntityId, _accessToken, "https://www.superservice.com").ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Action);
        ex.Message.Should().Be("Request failed");
    }

    [Fact]
    public async Task AutoSetupOffer_WithDnsError_ReturnsServiceException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest, ex: new HttpRequestException ("DNS Error"));
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _httpClientFactory.CreateClient(A<string>._)).Returns(httpClient);
        _fixture.Inject(_httpClientFactory);
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<OfferSetupService>();

        // Act
        async Task Action() => await sut.AutoSetupOffer(_existingServiceId, _iamUser.UserEntityId, _accessToken, "https://www.superservice.com").ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Action);
        ex.Message.Should().Be("The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.");
    }

    [Fact]
    public async Task AutoSetupOffer_WithTimeout_ReturnsServiceException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest, ex: new TaskCanceledException("Timed out"));
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _httpClientFactory.CreateClient(A<string>._)).Returns(httpClient);
        _fixture.Inject(_httpClientFactory);
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<OfferSetupService>();

        // Act
        async Task Action() => await sut.AutoSetupOffer(_existingServiceId, _iamUser.UserEntityId, _accessToken, "https://www.superservice.com").ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Action);
        ex.Message.Should().Be("The request failed due to timeout.");
    }

    #endregion

    #region Setup

    private void SetupRepositories()
    {
        A.CallTo(() => _offerSubscriptionsRepository.GetThirdPartyAutoSetupDataAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == _iamUser.UserEntityId)))
            .ReturnsLazily(() => new ValueTuple<OfferThirdPartyAutoSetupData,bool>(
                new OfferThirdPartyAutoSetupData(
                    new OfferThirdPartyAutoSetupCustomerData("Test Provider", "de", "tony@stark.com"),
                    new OfferThirdPartyAutoSetupPropertyData("BPNL000000000009", _existingServiceOfferId, _existingServiceId))
                ,true));
        A.CallTo(() => _offerSubscriptionsRepository.GetThirdPartyAutoSetupDataAsync(
                A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == _iamUser.UserEntityId)))
            .ReturnsLazily(() => ((OfferThirdPartyAutoSetupData,bool))default);

        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
    }

    private IamUser CreateTestUserPair()
    {
        var companyUser = _fixture.Build<CompanyUser>()
            .Without(u => u.IamUser)
            .With(u => u.CompanyId, _companyUserCompanyId)
            .Create();
        var iamUser = _fixture.Build<IamUser>()
            .With(u => u.CompanyUser, companyUser)
            .Create();
        companyUser.IamUser = iamUser;
        return iamUser;
    }

    #endregion
}
