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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.Tests;

public class OnboardingServiceProviderServiceTests
{
    #region Initialization

    private readonly IFixture _fixture;
    private readonly ITokenService _tokenService;

    public OnboardingServiceProviderServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _tokenService = A.Fake<ITokenService>();
    }

    #endregion

    #region TriggerProviderCallback

    [Fact]
    public async Task TriggerProviderCallback_WithValidCall_ReturnsExpected()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK);
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _tokenService.GetAuthorizedClient<OnboardingServiceProviderService>(A<KeyVaultAuthSettings>._, A<CancellationToken>._))
            .Returns(httpClient);
        var ospDetails = new OspTriggerDetails("https://trigger.com", "https://auth.com", "test1", "ZKU7jbfe9ZUNBVYxdXgrjqtihXfR2aRr");
        var data = _fixture.Create<OnboardingServiceProviderCallbackData>();
        var service = new OnboardingServiceProviderService(_tokenService);

        // Act
        var result = await service.TriggerProviderCallback(ospDetails, data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public async Task TriggerProviderCallback_WithUnsuccessfulStatusCode_ThrowsException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _tokenService.GetAuthorizedClient<OnboardingServiceProviderService>(A<KeyVaultAuthSettings>._, A<CancellationToken>._))
            .Returns(httpClient);
        var ospDetails = new OspTriggerDetails("https://callback.com", "https://auth.com", "test1", "ZKU7jbfe9ZUNBVYxdXgrjqtihXfR2aRr");
        var data = _fixture.Create<OnboardingServiceProviderCallbackData>();
        var service = new OnboardingServiceProviderService(_tokenService);

        // Act
        async Task Act() => await service.TriggerProviderCallback(ospDetails, data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);
    }

    [Fact]
    public async Task TriggerProviderCallback_WithException_ThrowsException()
    {
        // Arrange
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.BadRequest, ex: new HttpRequestException("DNS Error"));
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _tokenService.GetAuthorizedClient<OnboardingServiceProviderService>(A<KeyVaultAuthSettings>._, A<CancellationToken>._))
            .Returns(httpClient);
        var ospDetails = new OspTriggerDetails("https://callback.com", "https://auth.com", "test1", "ZKU7jbfe9ZUNBVYxdXgrjqtihXfR2aRr");
        var data = _fixture.Create<OnboardingServiceProviderCallbackData>();
        var service = new OnboardingServiceProviderService(_tokenService);

        // Act
        async Task Act() => await service.TriggerProviderCallback(ospDetails, data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);
    }

    #endregion
}
