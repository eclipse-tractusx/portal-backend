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
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.Tests;

public class AppBusinessLogicTests
{
    private const string IamUserId = "3e8343f7-4fe5-4296-8312-f33aa6dbde5d";
    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferRepository _offerRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly IUserRepository _userRepository;

    public AppBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _offerRepository = A.Fake<IOfferRepository>();
        _userRepository = A.Fake<IUserRepository>();
    }

    [Fact]
    public async Task AddFavouriteAppForUser_ExecutesSuccessfully()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var (companyUser, iamUser) = CreateTestUserPair();
        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(iamUser.UserEntityId))
            .Returns(companyUser.Id);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        _fixture.Inject(_portalRepositories);

        var sut = _fixture.Create<AppsBusinessLogic>();

        // Act
        await sut.AddFavouriteAppForUserAsync(appId, iamUser.UserEntityId);

        // Assert
        A.CallTo(() => _offerRepository.CreateAppFavourite(A<Guid>.That.Matches(x => x == appId), A<Guid>.That.Matches(x => x == companyUser.Id))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RemoveFavouriteAppForUser_ExecutesSuccessfully()
    {
        // Arrange
        var (companyUser, iamUser) = CreateTestUserPair();

        var appId = _fixture.Create<Guid>();
        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(iamUser.UserEntityId))
            .Returns(companyUser.Id);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        _fixture.Inject(_portalRepositories);

        var sut = _fixture.Create<AppsBusinessLogic>();

        // Act
        await sut.RemoveFavouriteAppForUserAsync(appId, iamUser.UserEntityId);

        // Assert
        A.CallTo(() => _portalRepositories.Remove(A<CompanyUserAssignedAppFavourite>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AddCompanyAppSubscription_ExecutesSuccessfully()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var appName = "Test App";
        var providerName = "New Provider";
        var providerContactEmail = "email@provider.com";
        var (companyUser, iamUser) = CreateTestUserPair();

        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAsync(appId, iamUser.UserEntityId, OfferTypeId.APP))
            .Returns(new ValueTuple<Guid, OfferSubscription?, string, Guid>(companyUser.CompanyId, null, "umbrella corporation", companyUser.Id));
        A.CallTo(() => _offerSubscriptionsRepository.CreateOfferSubscription(appId, companyUser.CompanyId, OfferSubscriptionStatusId.PENDING, companyUser.Id, companyUser.Id))
            .Returns(new OfferSubscription(Guid.NewGuid(), appId, companyUser.CompanyId, OfferSubscriptionStatusId.PENDING, companyUser.Id, companyUser.Id));
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(appId, OfferTypeId.APP))
            .Returns(new OfferProviderDetailsData(appName, providerName, providerContactEmail, Guid.NewGuid(), null));
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        _fixture.Inject(_portalRepositories);
        var mailingService = A.Fake<IMailingService>();
        _fixture.Inject(mailingService);

        var sut = _fixture.Create<AppsBusinessLogic>();

        // Act
        await sut.AddOwnCompanyAppSubscriptionAsync(appId, iamUser.UserEntityId);

        // Assert
        A.CallTo(() => _offerSubscriptionsRepository.CreateOfferSubscription(A<Guid>._, A<Guid>._, A<OfferSubscriptionStatusId>._, A<Guid>._, A<Guid>._)).MustHaveHappened(1, Times.Exactly);
        A.CallTo(() => mailingService.SendMails(providerContactEmail, A<Dictionary<string, string>>._, A<List<string>>._)).MustHaveHappened(1, Times.Exactly);
    }

    #region Auto setup service

    [Fact]
    public async Task AutoSetupService_ReturnsExcepted()
    {
        var offerAutoSetupResponseData = _fixture.Create<OfferAutoSetupResponseData>();
        // Arrange
        var offerService = A.Fake<IOfferService>();
        A.CallTo(() => offerService.AutoSetupServiceAsync(A<OfferAutoSetupData>._, A<IDictionary<string, IEnumerable<string>>>._, A<IDictionary<string, IEnumerable<string>>>._, A<string>._, A<OfferTypeId>._))
            .ReturnsLazily(() => offerAutoSetupResponseData);
        var data = new OfferAutoSetupData(Guid.NewGuid(), "https://www.offer.com");

        var sut = new AppsBusinessLogic(null!, null!, null!, offerService, _fixture.Create<IOptions<AppsSettings>>());

        // Act
        var result = await sut.AutoSetupAppAsync(data, IamUserId).ConfigureAwait(false);

        // Assert
        result.Should().Be(offerAutoSetupResponseData);
    }

    #endregion

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
}
