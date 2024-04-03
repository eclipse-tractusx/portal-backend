/********************************************************************************
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
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Security.Cryptography;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.Tests;

public class OnboardingServiceProviderBusinessLogicTests
{
    #region Initialization

    private readonly IFixture _fixture;
    private readonly OnboardingServiceProviderSettings _settings;
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

        _settings = new OnboardingServiceProviderSettings
        {
            EncryptionConfigs = new EncryptionModeConfig[]
            {
                new() { Index=0, EncryptionKey="2b7e151628aed2a6abf715892b7e151628aed2a6abf715892b7e151628aed2a6", CipherMode=CipherMode.ECB, PaddingMode=PaddingMode.PKCS7 },
                new() { Index=1, EncryptionKey="5892b7e151628aed2a6abf715892b7e151628aed2a62b7e151628aed2a6abf71", CipherMode=CipherMode.CBC, PaddingMode=PaddingMode.PKCS7 },
            },
            EncryptionConfigIndex = 1
        };

        _sut = new OnboardingServiceProviderBusinessLogic(_onboardingServiceProviderService, portalRepositories, Options.Create(_settings));
    }

    #endregion

    #region TriggerProviderCallback

    [Fact]
    public async Task TriggerProviderCallback_WithoutNetworkRegistration_DoesntCall()
    {
        // Arrange
        var networkRegistrationId = Guid.NewGuid();
        A.CallTo(() => _networkRepository.GetCallbackData(networkRegistrationId, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED))
            .Returns<(OspDetails?, string, string?, Guid, IEnumerable<string>)>(default);

        // Act
        var Act = () => _sut.TriggerProviderCallback(networkRegistrationId, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED, CancellationToken.None);

        // Assert
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        A.CallTo(() => _onboardingServiceProviderService.TriggerProviderCallback(A<OspTriggerDetails>._, A<OnboardingServiceProviderCallbackData>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        result.Message.Should().Be($"data should never be default here (networkRegistrationId: {networkRegistrationId})");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithoutCallbackUrl_DoesntCall()
    {
        // Arrange
        var networkRegistrationId = Guid.NewGuid();
        A.CallTo(() => _networkRepository.GetCallbackData(networkRegistrationId, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED))
            .Returns((null, _fixture.Create<string>(), null, Guid.NewGuid(), _fixture.CreateMany<string>()));

        // Act
        var result = await _sut.TriggerProviderCallback(networkRegistrationId, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED, CancellationToken.None);

        // Assert
        A.CallTo(() => _onboardingServiceProviderService.TriggerProviderCallback(A<OspTriggerDetails>._, A<OnboardingServiceProviderCallbackData>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        result.stepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
        result.processMessage.Should().Be("No callback url set");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithMultipleDeclineMessages_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var networkRegistrationId = Guid.NewGuid();
        var secret = Encoding.UTF8.GetBytes(_fixture.Create<string>());
        const string Bpn = "BPNL00000001TEST";
        var details = new OspDetails("https://callback.url", "https://auth.url", "test1", secret, null, 0);
        A.CallTo(() => _networkRepository.GetCallbackData(networkRegistrationId, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED))
            .Returns((details, Guid.NewGuid().ToString(), Bpn, Guid.NewGuid(), _fixture.CreateMany<string>(2)));

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
        var secret = Encoding.UTF8.GetBytes(_fixture.Create<string>());
        const string Bpn = "BPNL00000001TEST";
        var details = new OspDetails("https://callback.url", "https://auth.url", "test1", secret, null, 0);
        A.CallTo(() => _networkRepository.GetCallbackData(networkRegistrationId, ProcessStepTypeId.START_AUTOSETUP))
            .Returns((details, Guid.NewGuid().ToString(), Bpn, Guid.NewGuid(), Enumerable.Empty<string>()));

        // Act
        async Task Act() => await _sut.TriggerProviderCallback(networkRegistrationId, ProcessStepTypeId.START_AUTOSETUP, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
        ex.Message.Should().Be($"{ProcessStepTypeId.START_AUTOSETUP} is not supported");
    }

    [Theory]
    [InlineData("/UJ0wr5w1HiXaLo25QfxqXWhyq6Pa9w+CvBFNs1782s=", null, 0, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED, "Application 2b965267-555c-4834-a323-09b7858c29ae has been submitted for further processing", CompanyApplicationStatusId.SUBMITTED)]
    [InlineData("/UJ0wr5w1HiXaLo25QfxqXWhyq6Pa9w+CvBFNs1782s=", null, 0, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED, "Application 2b965267-555c-4834-a323-09b7858c29ae has been approved", CompanyApplicationStatusId.CONFIRMED)]
    [InlineData("/UJ0wr5w1HiXaLo25QfxqXWhyq6Pa9w+CvBFNs1782s=", null, 0, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED, "Application 2b965267-555c-4834-a323-09b7858c29ae has been declined with reason: this is a test", CompanyApplicationStatusId.DECLINED)]
    [InlineData("hzl/2shJlzl64Y4FGNYtuFjR2c4VKXsfBz4UeQKDovQ=", "7hFxEXvfoiRTrHYMA+vkug==", 1, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED, "Application 2b965267-555c-4834-a323-09b7858c29ae has been submitted for further processing", CompanyApplicationStatusId.SUBMITTED)]
    [InlineData("hzl/2shJlzl64Y4FGNYtuFjR2c4VKXsfBz4UeQKDovQ=", "7hFxEXvfoiRTrHYMA+vkug==", 1, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED, "Application 2b965267-555c-4834-a323-09b7858c29ae has been approved", CompanyApplicationStatusId.CONFIRMED)]
    [InlineData("hzl/2shJlzl64Y4FGNYtuFjR2c4VKXsfBz4UeQKDovQ=", "7hFxEXvfoiRTrHYMA+vkug==", 1, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED, "Application 2b965267-555c-4834-a323-09b7858c29ae has been declined with reason: this is a test", CompanyApplicationStatusId.DECLINED)]
    public async Task TriggerProviderCallback_WithValidData_CallsExpected(string clientSecret, string? initialVector, int index, ProcessStepTypeId processStepTypeId, string message, CompanyApplicationStatusId applicationStatusId)
    {
        // Act
        const string CallbackUrl = "https://callback.url";
        const string Bpn = "BPNL00000001TEST";
        var externalId = Guid.NewGuid().ToString();
        var applicationId = new Guid("2b965267-555c-4834-a323-09b7858c29ae");
        var networkRegistrationId = Guid.NewGuid();
        var details = new OspDetails(CallbackUrl, "https://auth.url", "test1", Convert.FromBase64String(clientSecret), initialVector == null ? null : Convert.FromBase64String(initialVector), index);
        A.CallTo(() => _networkRepository.GetCallbackData(networkRegistrationId, processStepTypeId))
            .Returns((details, externalId, Bpn, applicationId, processStepTypeId == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED ? Enumerable.Repeat("this is a test", 1) : Enumerable.Empty<string>()));

        // Act
        var result = await _sut.TriggerProviderCallback(networkRegistrationId, processStepTypeId, CancellationToken.None);

        // Assert
        A.CallTo(() => _onboardingServiceProviderService.TriggerProviderCallback(
                new OspTriggerDetails(CallbackUrl, "https://auth.url", "test1", "Sup3rS3cureTest!"),
                new OnboardingServiceProviderCallbackData(externalId, applicationId, Bpn, applicationStatusId, message),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        result.nextStepTypeIds.Should().BeEmpty();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
    }

    #endregion
}
