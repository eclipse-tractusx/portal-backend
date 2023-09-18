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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.Tests;

public class OnboardingServiceProviderBusinessLogicTests
{
    #region Initialization

    private readonly IFixture _fixture;
    private readonly IOnboardingServiceProviderBusinessLogic _sut;
    private readonly IOnboardingServiceProviderService _onboardingServiceProviderService;
    private readonly INetworkRepository _networkRepository;

    public OnboardingServiceProviderBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _onboardingServiceProviderService = A.Fake<IOnboardingServiceProviderService>();
        _networkRepository = A.Fake<INetworkRepository>();
        var portalRepositories = A.Fake<IPortalRepositories>();

        A.CallTo(() => portalRepositories.GetInstance<INetworkRepository>()).Returns(_networkRepository);
        
        _sut = new OnboardingServiceProviderBusinessLogic(_onboardingServiceProviderService, portalRepositories);
    }

    #endregion

    #region TriggerProviderCallback

    [Fact]
    public async Task TriggerProviderCallback_WithoutCallbackUrl_DoesntCall()
    {
        // Arrange
        var networkRegistrationId = Guid.NewGuid();
        A.CallTo(() => _networkRepository.GetCallbackData(networkRegistrationId, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED))
            .Returns(new ValueTuple<OspDetails?, Guid?, string?, Guid, IEnumerable<string?>>());

        // Act
        var result = await _sut.TriggerProviderCallback(networkRegistrationId, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _onboardingServiceProviderService.TriggerProviderCallback(A<OspDetails>._, A<OnboardingServiceProviderCallbackData>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        result.stepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
        result.processMessage.Should().Be("No callback url set");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithoutExternalId_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var networkRegistrationId = Guid.NewGuid();
        var secret = "test123";
        var details = new OspDetails("https://callback.url", "https://auth.url", "test1", secret);
        A.CallTo(() => _networkRepository.GetCallbackData(networkRegistrationId, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED))
            .Returns(new ValueTuple<OspDetails?, Guid?, string?, Guid, IEnumerable<string?>>(details, null, null, Guid.NewGuid(), Enumerable.Empty<string>()));

        // Act
        async Task Act() => await _sut.TriggerProviderCallback(networkRegistrationId, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be("No external registration found");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithoutBpn_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var networkRegistrationId = Guid.NewGuid();
        var secret = "test123";
        var details = new OspDetails("https://callback.url", "https://auth.url", "test1", secret);
        A.CallTo(() => _networkRepository.GetCallbackData(networkRegistrationId, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED))
            .Returns(new ValueTuple<OspDetails?, Guid?, string?, Guid, IEnumerable<string?>>(details, Guid.NewGuid(), null, Guid.NewGuid(), Enumerable.Empty<string>()));

        // Act
        async Task Act() => await _sut.TriggerProviderCallback(networkRegistrationId, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be("Bpn must be set");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithMultipleDeclineMessages_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var networkRegistrationId = Guid.NewGuid();
        var secret = "test123";
        const string Bpn = "BPNL00000001TEST";
        var details = new OspDetails("https://callback.url", "https://auth.url", "test1", secret);
        A.CallTo(() => _networkRepository.GetCallbackData(networkRegistrationId, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED))
            .Returns(new ValueTuple<OspDetails?, Guid?, string?, Guid, IEnumerable<string?>>(details, Guid.NewGuid(), Bpn, Guid.NewGuid(), _fixture.CreateMany<string>(2)));

        // Act
        async Task Act() => await _sut.TriggerProviderCallback(networkRegistrationId, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be("Message for decline should be set");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithWrongProcessStepTypeId_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var networkRegistrationId = Guid.NewGuid();
        var secret = "test123";
        const string Bpn = "BPNL00000001TEST";
        var details = new OspDetails("https://callback.url", "https://auth.url", "test1", secret);
        A.CallTo(() => _networkRepository.GetCallbackData(networkRegistrationId, ProcessStepTypeId.START_AUTOSETUP))
            .Returns(new ValueTuple<OspDetails?, Guid?, string?, Guid, IEnumerable<string?>>(details, Guid.NewGuid(), Bpn, Guid.NewGuid(), Enumerable.Empty<string>()));

        // Act
        async Task Act() => await _sut.TriggerProviderCallback(networkRegistrationId, ProcessStepTypeId.START_AUTOSETUP, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
        ex.Message.Should().Be($"{ProcessStepTypeId.START_AUTOSETUP} is not supported");
    }

    [Theory]
    [InlineData(ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED, "Application 2b965267-555c-4834-a323-09b7858c29ae has been submitted for further processing", CompanyApplicationStatusId.SUBMITTED)]
    [InlineData(ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED, "Application 2b965267-555c-4834-a323-09b7858c29ae has been approved", CompanyApplicationStatusId.CONFIRMED)]
    [InlineData(ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED, $"Application 2b965267-555c-4834-a323-09b7858c29ae has been declined with reason: this is a test", CompanyApplicationStatusId.DECLINED)]
    public async Task TriggerProviderCallback_WithValidData_CallsExpected(ProcessStepTypeId processStepTypeId, string message, CompanyApplicationStatusId applicationStatusId)
    {
        // Act
        const string CallbackUrl = "https://callback.url";
        const string Bpn = "BPNL00000001TEST";
        var secret = "test123";
        var externalId = Guid.NewGuid();
        var applicationId = new Guid("2b965267-555c-4834-a323-09b7858c29ae");
        var networkRegistrationId = Guid.NewGuid();
        var details = new OspDetails(CallbackUrl, "https://auth.url", "test1", secret);
        A.CallTo(() => _networkRepository.GetCallbackData(networkRegistrationId, processStepTypeId))
            .Returns(new ValueTuple<OspDetails?, Guid?, string?, Guid, IEnumerable<string?>>(details, externalId, Bpn, applicationId, processStepTypeId == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED ? Enumerable.Repeat("this is a test", 1) : Enumerable.Empty<string>()));

        // Act
       var result = await _sut.TriggerProviderCallback(networkRegistrationId, processStepTypeId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _onboardingServiceProviderService.TriggerProviderCallback(
                details,
                new OnboardingServiceProviderCallbackData(externalId, applicationId, Bpn, applicationStatusId, message),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        result.nextStepTypeIds.Should().BeEmpty();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
    }

    #endregion
}
