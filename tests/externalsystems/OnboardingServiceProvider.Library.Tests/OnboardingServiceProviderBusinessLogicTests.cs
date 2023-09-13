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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.Tests;

public class OnboardingServiceProviderBusinessLogicTests
{
    #region Initialization

    private readonly IFixture _fixture;
    private readonly IOnboardingServiceProviderBusinessLogic _sut;
    private readonly IOnboardingServiceProviderService _onboardingServiceProviderService;

    public OnboardingServiceProviderBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _onboardingServiceProviderService = A.Fake<IOnboardingServiceProviderService>();
        
        _sut = new OnboardingServiceProviderBusinessLogic(_onboardingServiceProviderService);
    }

    #endregion

    #region TriggerProviderCallback

    [Fact]
    public async Task TriggerProviderCallback_WithoutCallbackUrl_DoesntCall()
    {
        // Act
        await _sut.TriggerProviderCallback(null, null, null, Guid.NewGuid(), "test", CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _onboardingServiceProviderService.TriggerProviderCallback(A<string>._, A<OnboardingServiceProviderCallbackData>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task TriggerProviderCallback_WithoutExternalId_ThrowsUnexpectedConditionException()
    {
        // Act
        async Task Act() => await _sut.TriggerProviderCallback("https://callback.url", null, null, Guid.NewGuid(), "test", CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be("No external registration found");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithoutBpn_ThrowsUnexpectedConditionException()
    {
        // Act
        async Task Act() => await _sut.TriggerProviderCallback("https://callback.url", null, Guid.NewGuid(), Guid.NewGuid(), "test", CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be("Bpn must be set");
    }

    [Fact]
    public async Task TriggerProviderCallback_WithValidData_CallsExpected()
    {
        // Act
        var externalId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        const string Bpn = "BPNL00000001TEST";
        const string CallbackUrl = "https://callback.url";
        await _sut.TriggerProviderCallback(CallbackUrl, Bpn, externalId, applicationId, "test", CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _onboardingServiceProviderService.TriggerProviderCallback(
                CallbackUrl,
                new OnboardingServiceProviderCallbackData(externalId, applicationId, Bpn, CompanyApplicationStatusId.DECLINED, "test"),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion
}
