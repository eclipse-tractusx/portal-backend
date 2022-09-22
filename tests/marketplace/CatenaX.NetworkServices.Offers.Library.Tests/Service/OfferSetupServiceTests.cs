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
using System.Net.Http.Json;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using CatenaX.NetworkServices.Offers.Library.Service;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.Tests.Shared;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace CatenaX.NetworkServices.Offers.Library.Tests.Service;

public class OfferSetupServiceTests
{
    private readonly Guid _companyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");
    private readonly Guid _existingServiceId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
    private readonly Guid _existingServiceOfferId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47662");
    private readonly CompanyUser _companyUser;
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

        var (companyUser, iamUser) = CreateTestUserPair();
        _companyUser = companyUser;
        _iamUser = iamUser;

        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _httpClientFactory = A.Fake<IHttpClientFactory>();

        SetupRepositories(iamUser);
    }

    #region Get Service Agreement

    [Fact]
    public async Task AutoSetupOffer_WithSuccessfullCall_ReturnsTrue()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK);
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _httpClientFactory.CreateClient(A<string>._)).Returns(httpClient);
        _fixture.Inject(_httpClientFactory);
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<OfferSetupService>();

        // Act
        var result = await sut.AutoSetupOffer(_existingServiceId, _iamUser.UserEntityId, "https://www.superservice.com").ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AutoSetupOffer_WithNonSuccessfullClientCall_ReturnsFalse()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _httpClientFactory.CreateClient(A<string>._)).Returns(httpClient);
        _fixture.Inject(_httpClientFactory);
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<OfferSetupService>();

        // Act
        var result = await sut.AutoSetupOffer(_existingServiceId, _iamUser.UserEntityId, "https://www.superservice.com").ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Setup

    private void SetupRepositories(IamUser iamUser)
    {
        A.CallTo(() => _offerSubscriptionsRepository.GetAutoSetupDataAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == iamUser.UserEntityId)))
            .ReturnsLazily(() => new OfferThirdPartyAutoSetupData(new CustomerData("Test Provider", "de", "tony@stark.com"), new PropertyData("BPNL000000000009", _existingServiceOfferId, _existingServiceId)));
        A.CallTo(() => _offerSubscriptionsRepository.GetAutoSetupDataAsync(
                A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == iamUser.UserEntityId)))
            .ReturnsLazily(() => (OfferThirdPartyAutoSetupData?)null);

        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
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
        return (companyUser, iamUser);
    }

    #endregion
}
