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
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.Tests;

public class OnboardingServiceProviderServiceTests
{
    #region Initialization

    private readonly IFixture _fixture;
    private readonly ITokenService _tokenService;
    private readonly IOptions<OnboardingServiceProviderSettings> _options;

    public OnboardingServiceProviderServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _options = Options.Create(new OnboardingServiceProviderSettings
        {
            Password = "passWord",
            Scope = "test",
            Username = "user@name",
            ClientId = "CatenaX",
            ClientSecret = "pass@Secret",
            GrantType = "cred",
            KeycloakTokenAddress = "https://key.cloak.com",
        });
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
        A.CallTo(() => _tokenService.GetAuthorizedClient<OnboardingServiceProviderService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        const string url = "https://trigger.com";
        var data = _fixture.Create<OnboardingServiceProviderCallbackData>();
        var service = new OnboardingServiceProviderService(_tokenService, _options);

        // Act
        var result = await service.TriggerProviderCallback(url, data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public async Task TriggerProviderCallback_WithUnsuccessfulStatusCode_ThrowsException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _tokenService.GetAuthorizedClient<OnboardingServiceProviderService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var data = _fixture.Create<OnboardingServiceProviderCallbackData>();
        var service = new OnboardingServiceProviderService(_tokenService, _options);

        // Act
        async Task Act() => await service.TriggerProviderCallback("https://callback.com", data, CancellationToken.None).ConfigureAwait(false);

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
        A.CallTo(() => _tokenService.GetAuthorizedClient<OnboardingServiceProviderService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var data = _fixture.Create<OnboardingServiceProviderCallbackData>();
        var service = new OnboardingServiceProviderService(_tokenService, _options);

        // Act
        async Task Act() => await service.TriggerProviderCallback("https://callback.com", data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);
    }

    #endregion
}
