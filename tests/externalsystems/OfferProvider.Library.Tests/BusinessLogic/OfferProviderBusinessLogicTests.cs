/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using System.Security.Cryptography;

namespace Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.Tests.BusinessLogic;

public class OfferProviderBusinessLogicTests
{
    private readonly Guid _subscriptionId = Guid.NewGuid();
    private readonly Guid _singleInstanceSubscriptionId = Guid.NewGuid();
    private readonly Guid _offerId = Guid.NewGuid();
    private readonly Guid _companyUserId = Guid.NewGuid();
    private readonly Guid _salesManagerId = Guid.NewGuid();
    private readonly Guid _receiverId = Guid.NewGuid();
    private readonly ProviderAuthInformation _providerAuth = new("https://auth.url", "test1", Convert.FromBase64String("hzl/2shJlzl64Y4FGNYtuFjR2c4VKXsfBz4UeQKDovQ="), Convert.FromBase64String("7hFxEXvfoiRTrHYMA+vkug=="), 1);

    private readonly IOfferSubscriptionsRepository _offerSubscriptionRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOfferProviderService _offerProviderService;
    private readonly IProvisioningManager _provisioningManager;
    private readonly OfferProviderSettings _settings;
    private readonly OfferProviderBusinessLogic _sut;

    public OfferProviderBusinessLogicTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        var portalRepositories = A.Fake<IPortalRepositories>();
        _offerSubscriptionRepository = A.Fake<IOfferSubscriptionsRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _userRepository = A.Fake<IUserRepository>();

        _settings = new OfferProviderSettings
        {
            EncryptionConfigs =
                    [
                new() { Index = 0, EncryptionKey = "2b7e151628aed2a6abf715892b7e151628aed2a6abf715892b7e151628aed2a6", CipherMode = CipherMode.CFB, PaddingMode = PaddingMode.PKCS7 },
                        new() { Index = 1, EncryptionKey = "5892b7e151628aed2a6abf715892b7e151628aed2a62b7e151628aed2a6abf71", CipherMode = CipherMode.CBC, PaddingMode = PaddingMode.PKCS7 },
                    ],
            EncryptionConfigIndex = 1
        };
        A.CallTo(() => portalRepositories.GetInstance<IOfferSubscriptionsRepository>())
            .Returns(_offerSubscriptionRepository);
        A.CallTo(() => portalRepositories.GetInstance<IUserRolesRepository>())
            .Returns(_userRolesRepository);
        A.CallTo(() => portalRepositories.GetInstance<IUserRepository>())
            .Returns(_userRepository);

        _offerProviderService = A.Fake<IOfferProviderService>();
        _provisioningManager = A.Fake<IProvisioningManager>();

        _sut = new OfferProviderBusinessLogic(
            portalRepositories,
            _offerProviderService,
            _provisioningManager, Options.Create(_settings));
    }

    #region TriggerProvider

    [Fact]
    public async Task TriggerProvider_InvalidProcessId_Throws()
    {
        // Arrange
        SetupTriggerProvider();
        var fakeId = Guid.NewGuid();
        async Task Act() => await _sut.TriggerProvider(fakeId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

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
        var result = await _sut.TriggerProvider(_subscriptionId, CancellationToken.None);

        // Assert
        result.nextStepTypeIds.Should().ContainSingle()
            .And.Satisfy(x => x == ProcessStepTypeId.AWAIT_START_AUTOSETUP);
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        A.CallTo(() =>
                _offerProviderService.TriggerOfferProvider(A<OfferThirdPartyAutoSetupData>._, A<string>._, A<string>._, A<string>._, A<string>._,
                    A<CancellationToken>._))
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
        var result = await _sut.TriggerProvider(_singleInstanceSubscriptionId, CancellationToken.None);

        // Assert
        result.nextStepTypeIds.Should().ContainSingle()
            .And.Satisfy(x => x == ProcessStepTypeId.SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION);
        result.stepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
        A.CallTo(() =>
                _offerProviderService.TriggerOfferProvider(A<OfferThirdPartyAutoSetupData>._, A<string>._, A<string>._, A<string>._, A<string>._,
                    A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    #endregion

    #region TriggerProvider

    [Fact]
    public async Task TriggerProviderCallback_InvalidSubscriptionId_Throws()
    {
        // Arrange
        var fakeId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderCallbackInformation(fakeId))
            .Returns<(IEnumerable<(Guid, string?, TechnicalUserKindId)>, string?, string?, ProviderAuthInformation, OfferSubscriptionStatusId)>(default);
        async Task Act() => await _sut.TriggerProviderCallback(fakeId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be($"OfferSubscription {fakeId} does not exist");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithPendingSubscription_Throws()
    {
        // Arrange
        var fakeId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderCallbackInformation(fakeId))
            .Returns((Enumerable.Empty<(Guid, string?, TechnicalUserKindId)>(), string.Empty, "callback", _providerAuth, OfferSubscriptionStatusId.PENDING));
        async Task Act() => await _sut.TriggerProviderCallback(fakeId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("offer subscription should be active");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithClientIdNotSet_Throws()
    {
        // Arrange
        var fakeId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderCallbackInformation(fakeId))
            .Returns((Enumerable.Empty<(Guid, string?, TechnicalUserKindId)>(), null, "callback", _providerAuth, OfferSubscriptionStatusId.ACTIVE));
        async Task Act() => await _sut.TriggerProviderCallback(fakeId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("Client should be set");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithTechnicalClientIdNotSet_Throws()
    {
        // Arrange
        var technicalUserId = Guid.NewGuid();
        var serviceAccounts = new (Guid, string?, TechnicalUserKindId)[]
        {
            new(technicalUserId, null, TechnicalUserKindId.INTERNAL)
        };
        var fakeId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderCallbackInformation(fakeId))
            .Returns((serviceAccounts, "cl1", "callback", _providerAuth, OfferSubscriptionStatusId.ACTIVE));
        async Task Act() => await _sut.TriggerProviderCallback(fakeId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be($"ClientId of serviceAccount {technicalUserId} should be set");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithCallbackUrlNotSet_Skips()
    {
        // Arrange
        var fakeId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderCallbackInformation(fakeId))
            .Returns((Enumerable.Empty<(Guid, string?, TechnicalUserKindId)>(), "cl1", null, _providerAuth, OfferSubscriptionStatusId.ACTIVE));

        // Act
        var result = await _sut.TriggerProviderCallback(fakeId, CancellationToken.None);

        // Assert
        result.nextStepTypeIds.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
    }

    [Fact]
    public async Task TriggerProviderCallback_WithNoServiceAccountSet_CallsExpected()
    {
        // Arrange
        var fakeId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderCallbackInformation(fakeId))
            .Returns((Enumerable.Empty<(Guid, string?, TechnicalUserKindId)>(), "cl1", "https://callback.com", _providerAuth, OfferSubscriptionStatusId.ACTIVE));

        // Act
        var result = await _sut.TriggerProviderCallback(fakeId, CancellationToken.None);

        // Assert
        result.nextStepTypeIds.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.modified.Should().BeTrue();
        A.CallTo(() => _offerProviderService.TriggerOfferProviderCallback(A<OfferProviderCallbackData>.That.Matches(x => x.TechnicalUsersInfo == null), A<string>._, A<string>._, A<string>._, A<string>._, A<CancellationToken>._))
             .MustHaveHappenedOnceExactly();

    }

    [Fact]
    public async Task TriggerProviderCallback_WithValidData_InternalSA_ReturnsExpected()
    {
        // Arrange
        var technicalUserId = Guid.NewGuid();
        var technicalUserClientId = "sa1";
        var technicalUserInternalClientId = Guid.NewGuid().ToString();
        var serviceAccounts = new (Guid, string?, TechnicalUserKindId)[]
        {
            new(technicalUserId, technicalUserClientId, TechnicalUserKindId.INTERNAL)
        };
        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderCallbackInformation(_subscriptionId))
            .Returns((serviceAccounts, "cl1", "https://callback.com", _providerAuth, OfferSubscriptionStatusId.ACTIVE));
        A.CallTo(() => _provisioningManager.GetIdOfCentralClientAsync(technicalUserClientId))
            .Returns(technicalUserInternalClientId);
        A.CallTo(() => _provisioningManager.GetCentralClientAuthDataAsync(technicalUserInternalClientId))
            .Returns(new ClientAuthData(IamClientAuthMethod.SECRET) { Secret = "test123" });

        // Act
        var result = await _sut.TriggerProviderCallback(_subscriptionId, CancellationToken.None);

        // Assert
        result.nextStepTypeIds.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.modified.Should().BeTrue();
        A.CallTo(() => _offerProviderService.TriggerOfferProviderCallback(A<OfferProviderCallbackData>.That.Matches(x => x.TechnicalUsersInfo != null && x.TechnicalUsersInfo.Count() > 0 && x.TechnicalUsersInfo.First().TechnicalUserSecret == "test123"), A<string>._, A<string>._, A<string>._, A<string>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task TriggerProviderCallback_WithValidData_InternalSA_MultipleTechnicalUsers_ReturnsExpected()
    {
        // Arrange
        var technicalUsers = new[]
        {
            new { Id = Guid.NewGuid(), ClientId = "sa1", InternalClientId = Guid.NewGuid().ToString(), Secret = "test123" },
            new { Id = Guid.NewGuid(), ClientId = "sa2", InternalClientId = Guid.NewGuid().ToString(), Secret = "test456" }
        };

        var serviceAccounts = technicalUsers
            .Select(tu => (tu.Id, tu.ClientId, TechnicalUserKindId.INTERNAL))
            .ToArray();

        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderCallbackInformation(_subscriptionId))
            .Returns((serviceAccounts, "cl1", "https://callback.com", _providerAuth, OfferSubscriptionStatusId.ACTIVE));

        foreach (var user in technicalUsers)
        {
            A.CallTo(() => _provisioningManager.GetIdOfCentralClientAsync(user.ClientId))
                .Returns(user.InternalClientId);
            A.CallTo(() => _provisioningManager.GetCentralClientAuthDataAsync(user.InternalClientId))
                .Returns(new ClientAuthData(IamClientAuthMethod.SECRET) { Secret = user.Secret });
        }

        // Act
        var result = await _sut.TriggerProviderCallback(_subscriptionId, CancellationToken.None);

        // Assert
        result.nextStepTypeIds.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.modified.Should().BeTrue();

        var expectedTechnicalUsers = technicalUsers.Select(tu =>
          new CallbackTechnicalUserInfoData(tu.Id, tu.Secret, tu.ClientId)).ToList();

        // Verify the callback method was called with the expected data
        A.CallTo(() => _offerProviderService.TriggerOfferProviderCallback(
            A<OfferProviderCallbackData>.That.Matches(x =>
                x.TechnicalUsersInfo != null &&
                x.TechnicalUsersInfo.SequenceEqual(expectedTechnicalUsers)
            ),
            A<string>._,
            A<string>._,
            A<string>._,
            A<string>._,
            A<CancellationToken>._
        )).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task TriggerProviderCallback_WithValidData_ExternalSA_ReturnsExpected()
    {
        // Arrange
        var technicalUserId = Guid.NewGuid();
        var technicalUserClientId = "sa1";
        var serviceAccounts = new (Guid, string?, TechnicalUserKindId)[]
        {
            new(technicalUserId, technicalUserClientId, TechnicalUserKindId.EXTERNAL)
        };
        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderCallbackInformation(_subscriptionId))
            .Returns((serviceAccounts, "cl1", "https://callback.com", _providerAuth, OfferSubscriptionStatusId.ACTIVE));

        // Act
        var result = await _sut.TriggerProviderCallback(_subscriptionId, CancellationToken.None);

        // Assert
        result.nextStepTypeIds.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.modified.Should().BeTrue();
        A.CallTo(() => _provisioningManager.GetIdOfCentralClientAsync(A<string>._))
            .MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.GetCentralClientAuthDataAsync(A<string>._))
            .MustNotHaveHappened();
        A.CallTo(() => _offerProviderService.TriggerOfferProviderCallback(A<OfferProviderCallbackData>.That.Matches(x => x.TechnicalUsersInfo!.All(x => x.TechnicalUserSecret == null)), A<string>._, A<string>._, A<string>._, A<string>._, A<CancellationToken>._))
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
                _providerAuth,
                new CompanyInformationData(Guid.NewGuid(), "Stark", "DE", "BPNL0000123TEST", "test@email.com"),
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
                _providerAuth,
                new CompanyInformationData(Guid.NewGuid(), "Stark", "DE", "BPNL0000123TEST",
                    "test@email.com"),
                offerTypeId,
                _salesManagerId,
                _companyUserId,
                true
            ));

        A.CallTo(() => _offerSubscriptionRepository.GetTriggerProviderInformation(A<Guid>.That.Not.Matches(x => x == _subscriptionId || x == _singleInstanceSubscriptionId)))
            .Returns<TriggerProviderInformation?>(null);

        var userRoleId = Guid.NewGuid();
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>._))
            .Returns(new[] { userRoleId }.ToAsyncEnumerable());

        A.CallTo(() => _userRepository.GetServiceProviderCompanyUserWithRoleIdAsync(_offerId, A<List<Guid>>.That.IsSameSequenceAs(new[] { userRoleId })))
            .Returns(new[] { _receiverId }.ToAsyncEnumerable());
    }

    #endregion
}
