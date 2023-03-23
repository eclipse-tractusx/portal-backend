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
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Executor;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Executor.DependencyInjection;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Executor.Tests;

public class OfferSubscriptionProcessTypeExecutorTests
{
    private readonly Guid _processId = Guid.NewGuid();
    private readonly Guid _subscriptionId = Guid.NewGuid();

    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionRepository;
    private readonly IOfferProviderBusinessLogic _offerProviderBusinessLogic;
    private readonly IOfferSetupService _offerSetupService;
    private readonly IFixture _fixture;
    private readonly OfferSubscriptionsProcessSettings _settings;
    private readonly OfferSubscriptionProcessTypeExecutor _executor;

    public OfferSubscriptionProcessTypeExecutorTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerProviderBusinessLogic = A.Fake<IOfferProviderBusinessLogic>();
        _offerSetupService = A.Fake<IOfferSetupService>();

        _offerSubscriptionRepository = A.Fake<IOfferSubscriptionsRepository>();

        _settings = new OfferSubscriptionsProcessSettings
        {
            BasePortalAddress = "https://test.com",
            ItAdminRoles = new Dictionary<string, IEnumerable<string>>
            {
                { "Portal", new []{ "ItAdmin", "Admin" }}
            },
            ServiceAccountRoles = new Dictionary<string, IEnumerable<string>>
            {
                { "Portal", new []{ "Service Account", "User" }}
            }
        };
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>())
            .Returns(_offerSubscriptionRepository);

        _executor = new OfferSubscriptionProcessTypeExecutor(
            _offerProviderBusinessLogic,
            _offerSetupService,
            _portalRepositories,
            Options.Create(_settings));

        SetupFakes();
    }

    #region InitializeProcess

    [Fact]
    public async Task InitializeProcess_InvalidProcessId_Throws()
    {
        // Arrange
        var processId = Guid.NewGuid();

        async Task Act() => await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be($"process {processId} does not exist or is not associated with an offer subscription");
    }

    [Fact]
    public async Task InitializeProcess_ValidProcessId_ReturnsExpected()
    {
        // Arrange
        var result = await _executor.InitializeProcess(_processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);
        ;

        // Assert
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    #endregion

    #region ExecuteProcessStep

    [Fact]
    public async Task ExecuteProcessStep_InitializeNotCalled_Throws()
    {
        // Arrange
        var processStepTypeId = _fixture.Create<ProcessStepTypeId>();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();

        var Act = async () => await _executor.ExecuteProcessStep(processStepTypeId, processStepTypeIds, CancellationToken.None).ConfigureAwait(false);

        // Act
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);

        // Assert
        result.Message.Should().Be("offerSubscriptionId should never be empty here");
    }

    [Fact]
    public async Task ExecuteProcessStep_ValidSubscription_ReturnsExpected()
    {
        // Act initialize
        var initializationResult = await _executor.InitializeProcess(_processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange execute
        var executeProcessStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>();
        A.CallTo(() => _offerProviderBusinessLogic.TriggerProvider(_subscriptionId, A<CancellationToken>._))
            .ReturnsLazily(() => new ValueTuple<IEnumerable<ProcessStepTypeId>, IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(new[] { ProcessStepTypeId.START_AUTOSETUP }, null, ProcessStepStatusId.DONE, true, null));

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.TRIGGER_PROVIDER, executeProcessStepTypeIds, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeTrue();
        result.ScheduleStepTypeIds.Should().ContainSingle().And.Satisfy(x => x == ProcessStepTypeId.START_AUTOSETUP);
    }

    #endregion

    #region GetProcessTypeId

    [Fact]
    public void GetProcessTypeId_ReturnsExpected()
    {
        // Act
        var result = _executor.GetProcessTypeId();

        // Assert
        result.Should().Be(ProcessTypeId.OFFER_SUBSCRIPTION);
    }

    #endregion

    #region IsExecutableStepTypeId

    [Theory]
    [InlineData(ProcessStepTypeId.TRIGGER_PROVIDER, true)]
    [InlineData(ProcessStepTypeId.SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION, true)]
    [InlineData(ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION, true)]
    [InlineData(ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION, true)]
    [InlineData(ProcessStepTypeId.ACTIVATE_SUBSCRIPTION, true)]
    [InlineData(ProcessStepTypeId.START_AUTOSETUP, false)]
    [InlineData(ProcessStepTypeId.END_CLEARING_HOUSE, false)]
    [InlineData(ProcessStepTypeId.START_CLEARING_HOUSE, false)]
    [InlineData(ProcessStepTypeId.DECLINE_APPLICATION, false)]
    [InlineData(ProcessStepTypeId.CREATE_IDENTITY_WALLET, false)]
    public void IsExecutableProcessStep_ReturnsExpected(ProcessStepTypeId processStepTypeId, bool expectedResult)
    {
        // Act
        var result = _executor.IsExecutableStepTypeId(processStepTypeId);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region GetExecutableStepTypeIds

    [Fact]
    public void GetExecutableStepTypeIds_ReturnsExpected()
    {
        //Act
        var result = _executor.GetExecutableStepTypeIds();

        // Assert
        result.Should().HaveCount(5)
            .And.Satisfy(
                x => x == ProcessStepTypeId.TRIGGER_PROVIDER,
                x => x == ProcessStepTypeId.SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION,
                x => x == ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION,
                x => x == ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION,
                x => x == ProcessStepTypeId.ACTIVATE_SUBSCRIPTION
            );
    }

    #endregion

    #region Setup

    private void SetupFakes()
    {
        A.CallTo(() => _offerSubscriptionRepository.GetOfferSubscriptionDataForProcessIdAsync(_processId))
            .ReturnsLazily(() => _subscriptionId);
        A.CallTo(() => _offerSubscriptionRepository.GetOfferSubscriptionDataForProcessIdAsync(A<Guid>.That.Not.Matches(x => x == _processId)))
            .ReturnsLazily(() => Guid.Empty);

        A.CallTo(() => _offerProviderBusinessLogic.TriggerProvider(A<Guid>.That.Not.Matches(x => x == _subscriptionId), A<CancellationToken>._))
            .ThrowsAsync(() => new TestException("Test"));
        A.CallTo(() => _offerSetupService.CreateSingleInstanceSubscriptionDetail(A<Guid>.That.Not.Matches(x => x == _subscriptionId)))
            .ThrowsAsync(() => new TestException("Test"));
        A.CallTo(() => _offerSetupService.CreateClient(A<Guid>.That.Not.Matches(x => x == _subscriptionId)))
            .ThrowsAsync(() => new TestException("Test"));
        A.CallTo(() => _offerSetupService.CreateTechnicalUser(A<Guid>.That.Not.Matches(x => x == _subscriptionId), A<IDictionary<string, IEnumerable<string>>>._, A<IDictionary<string, IEnumerable<string>>>._))
            .ThrowsAsync(() => new TestException("Test"));
        A.CallTo(() => _offerSetupService.ActivateSubscription(A<Guid>.That.Not.Matches(x => x == _subscriptionId), A<IDictionary<string, IEnumerable<string>>>._, A<string>._))
            .ThrowsAsync(() => new TestException("Test"));
        A.CallTo(() => _offerProviderBusinessLogic.TriggerProviderCallback(A<Guid>.That.Not.Matches(x => x == _subscriptionId), A<CancellationToken>._))
            .ThrowsAsync(() => new TestException("Test"));
    }

    #endregion

    [Serializable]
    public class TestException : Exception
    {
        public TestException() { }
        public TestException(string message) : base(message) { }
        public TestException(string message, Exception inner) : base(message, inner) { }
        protected TestException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
