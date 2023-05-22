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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.Tests.BusinessLogic;

public class OfferProviderBusinessLogicTests
{
    private readonly Guid _subscriptionId = Guid.NewGuid();
    private readonly Guid _singleInstanceSubscriptionId = Guid.NewGuid();
    private readonly Guid _offerId = Guid.NewGuid();
    private readonly Guid _companyUserId = Guid.NewGuid();
    private readonly Guid _salesManagerId = Guid.NewGuid();
    private readonly Guid _receiverId = Guid.NewGuid();

    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly INotificationRepository _notificationRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOfferProviderService _offerProviderService;
    private readonly IProvisioningManager _provisioningManager;
    private readonly OfferProviderSettings _settings;
    private readonly OfferProviderBusinessLogic _sut;

    public OfferProviderBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _portalRepositories = A.Fake<IPortalRepositories>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _offerSubscriptionRepository = A.Fake<IOfferSubscriptionsRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _userRepository = A.Fake<IUserRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>())
            .Returns(_offerSubscriptionRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>())
            .Returns(_notificationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>())
            .Returns(_userRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>())
            .Returns(_userRepository);

        _offerProviderService = A.Fake<IOfferProviderService>();
        _provisioningManager = A.Fake<IProvisioningManager>();

        _settings = new OfferProviderSettings
        {
            Password = "test",
            ClientId = "123",
            ClientSecret = "test",
            ServiceManagerRoles = Enumerable.Repeat(new UserRoleConfig("Portal", new[] { "Manager" }), 1)
        };

        _sut = new OfferProviderBusinessLogic(
            _portalRepositories,
            _offerProviderService,
            _provisioningManager,
            Options.Create(_settings));
    }

    #region TriggerProvider

    [Fact]
    public async Task TriggerProvider_InvalidProcessId_Throws()
    {
        // Arrange
        SetupTriggerProvider();
        var fakeId = Guid.NewGuid();
        async Task Act() => await _sut.TriggerProvider(fakeId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be($"OfferSubscription {fakeId} does not exist");
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task TriggerProvider_ValidMultiInstanceApp_ReturnsExpected(OfferTypeId offerTypeId)
    {
        // Arrange
        SetupTriggerProvider(offerTypeId);

        // Act
        var result = await _sut.TriggerProvider(_subscriptionId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.nextStepTypeIds.Should().ContainSingle()
            .And.Satisfy(x => x == ProcessStepTypeId.START_AUTOSETUP);
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        A.CallTo(() =>
                _offerProviderService.TriggerOfferProvider(A<OfferThirdPartyAutoSetupData>._, A<string>._,
                    A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _notificationRepository.CreateNotification(_salesManagerId,
                offerTypeId == OfferTypeId.APP
                    ? NotificationTypeId.APP_SUBSCRIPTION_REQUEST
                    : NotificationTypeId.SERVICE_REQUEST, false, A<Action<Notification>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _notificationRepository.CreateNotification(_receiverId,
                offerTypeId == OfferTypeId.APP
                    ? NotificationTypeId.APP_SUBSCRIPTION_REQUEST
                    : NotificationTypeId.SERVICE_REQUEST, false, A<Action<Notification>>._))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task TriggerProvider_ValidSingleInstanceApp_ReturnsExpected(OfferTypeId offerTypeId)
    {
        // Arrange
        SetupTriggerProvider(offerTypeId);

        // Act
        var result = await _sut.TriggerProvider(_singleInstanceSubscriptionId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.nextStepTypeIds.Should().ContainSingle()
            .And.Satisfy(x => x == ProcessStepTypeId.SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION);
        result.stepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
        A.CallTo(() =>
                _offerProviderService.TriggerOfferProvider(A<OfferThirdPartyAutoSetupData>._, A<string>._,
                    A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _notificationRepository.CreateNotification(_salesManagerId,
                offerTypeId == OfferTypeId.APP
                    ? NotificationTypeId.APP_SUBSCRIPTION_REQUEST
                    : NotificationTypeId.SERVICE_REQUEST, false, A<Action<Notification>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _notificationRepository.CreateNotification(_receiverId,
                offerTypeId == OfferTypeId.APP
                    ? NotificationTypeId.APP_SUBSCRIPTION_REQUEST
                    : NotificationTypeId.SERVICE_REQUEST, false, A<Action<Notification>>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region TriggerProvider

    [Fact]
    public async Task TriggerProviderCallback_InvalidSubscriptionId_Throws()
    {
        // Arrange
        var fakeId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderCallbackInformation(fakeId))
            .Returns(((IEnumerable<(Guid, string?)>, string, string?, OfferSubscriptionStatusId))default);
        async Task Act() => await _sut.TriggerProviderCallback(fakeId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be($"OfferSubscription {fakeId} does not exist");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithPendingSubscription_Throws()
    {
        // Arrange
        var fakeId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderCallbackInformation(fakeId))
            .Returns((Enumerable.Empty<(Guid, string?)>(), string.Empty, null, OfferSubscriptionStatusId.PENDING));
        async Task Act() => await _sut.TriggerProviderCallback(fakeId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be("offer subscription should be active");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithClientIdNotSet_Throws()
    {
        // Arrange
        var fakeId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderCallbackInformation(fakeId))
            .Returns((Enumerable.Empty<(Guid, string?)>(), null, null, OfferSubscriptionStatusId.ACTIVE));
        async Task Act() => await _sut.TriggerProviderCallback(fakeId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be("Client should be set");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithCallbackUrlNotSet_Throws()
    {
        // Arrange
        var fakeId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderCallbackInformation(fakeId))
            .Returns((Enumerable.Empty<(Guid, string?)>(), "cl1", null, OfferSubscriptionStatusId.ACTIVE));
        async Task Act() => await _sut.TriggerProviderCallback(fakeId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be("Callback Url should be set here");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithNoServiceAccountSet_CallsExpected()
    {
        // Arrange
        var fakeId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderCallbackInformation(fakeId))
            .Returns((Enumerable.Empty<(Guid, string?)>(), "cl1", "https://callback.com", OfferSubscriptionStatusId.ACTIVE));

        // Act
        var result = await _sut.TriggerProviderCallback(fakeId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.nextStepTypeIds.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.modified.Should().BeTrue();
        A.CallTo(() => _offerProviderService.TriggerOfferProviderCallback(A<OfferProviderCallbackData>.That.Matches(x => x.TechnicalUserInfo == null), A<string>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

    }

    [Fact]
    public async Task TriggerProviderCallback_WithMultipleServiceAccountSet_Throws()
    {
        // Arrange
        var fakeId = Guid.NewGuid();
        var serviceAccounts = new (Guid, string?)[]
        {
            new(Guid.NewGuid(), "sa1"),
            new(Guid.NewGuid(), "sa2")
        };
        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderCallbackInformation(fakeId))
            .Returns((serviceAccounts, "cl1", "https://callback.com", OfferSubscriptionStatusId.ACTIVE));
        async Task Act() => await _sut.TriggerProviderCallback(fakeId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be("There should be not more than one service account for the offer subscription");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithValidData_ReturnsExpected()
    {
        // Arrange
        var technicalUserId = Guid.NewGuid();
        var technicalUserClientId = "sa1";
        var serviceAccounts = new (Guid, string?)[]
        {
            new(technicalUserId, technicalUserClientId)
        };
        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderCallbackInformation(_subscriptionId))
            .Returns((serviceAccounts, "cl1", "https://callback.com", OfferSubscriptionStatusId.ACTIVE));
        A.CallTo(() => _provisioningManager.GetCentralClientAuthDataAsync(technicalUserClientId))
            .Returns(new ClientAuthData(IamClientAuthMethod.SECRET) { Secret = "test123" });

        // Act
        var result = await _sut.TriggerProviderCallback(_subscriptionId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.nextStepTypeIds.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.modified.Should().BeTrue();
        A.CallTo(() => _offerProviderService.TriggerOfferProviderCallback(A<OfferProviderCallbackData>.That.Matches(x => x.TechnicalUserInfo!.TechnicalUserSecret == "test123"), A<string>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Setup

    private void SetupTriggerProvider(OfferTypeId offerTypeId = OfferTypeId.APP)
    {
        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderInformation(_subscriptionId))
            .Returns(new TriggerProviderInformation(
                _offerId,
                "Test App",
                "https://www.test.com",
                new CompanyInformationData(Guid.NewGuid(), "Stark", "DE", "BPNL0000123TEST"),
                "test@email.com",
                offerTypeId,
                _salesManagerId,
                _companyUserId,
                false
            ));

        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderInformation(_singleInstanceSubscriptionId))
            .Returns(new TriggerProviderInformation(
                _offerId,
                "Single Test App",
                "https://www.test.com",
                new CompanyInformationData(Guid.NewGuid(), "Stark", "DE", "BPNL0000123TEST"),
                "test@email.com",
                offerTypeId,
                _salesManagerId,
                _companyUserId,
                true
            ));

        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderInformation(A<Guid>.That.Not.Matches(x => x == _subscriptionId || x == _singleInstanceSubscriptionId)))
            .Returns((TriggerProviderInformation?)null);

        var userRoleId = Guid.NewGuid();
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>._))
            .Returns(new[] { userRoleId }.ToAsyncEnumerable());

        A.CallTo(() => _userRepository.GetServiceProviderCompanyUserWithRoleIdAsync(_offerId, A<List<Guid>>.That.Matches(x => x.Count() == 1 && x.Single() == userRoleId)))
            .Returns(new[] { _receiverId }.ToAsyncEnumerable());
    }

    #endregion
}
