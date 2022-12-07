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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Token.Tests;

public class TokenServiceTests
{
    private readonly string _accessToken;
    private readonly string _httpClientName;
    private readonly CancellationToken _cancellationToken;
    private readonly TestException _testException;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IFixture _fixture;

    public TokenServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _accessToken = _fixture.Create<string>();
        _httpClientName  = _fixture.Create<string>();
        _cancellationToken = new CancellationToken();
        _testException = _fixture.Create<TestException>();

        _httpClientFactory = A.Fake<IHttpClientFactory>();
    }

    #region GetTokenAsync

    [Fact]
    public async void GetTokenAsyncSuccess()
    {
        var authResponse = JsonSerializer.Serialize(_fixture.Build<AuthResponse>().With(x => x.AccessToken, _accessToken).Create());
        SetupHttpClient(new HttpMessageHandlerMock(HttpStatusCode.OK, authResponse.ToFormContent("application/json")));

        var settings = _fixture.Build<GetTokenSettings>().With(x => x.HttpClientName, _httpClientName).Create();

        var sut = new TokenService(_httpClientFactory);

        var result = await sut.GetTokenAsync(settings, _cancellationToken).ConfigureAwait(false);

        result.Should().NotBeNull();
        result.Should().Be(_accessToken);
    }

    [Fact]
    public async void GetTokenAsyncHttpClientError500_Throws()
    {
        var errorResponse = JsonSerializer.Serialize(_fixture.Create<ErrorResponse>());
        SetupHttpClient(new HttpMessageHandlerMock(HttpStatusCode.InternalServerError, errorResponse.ToFormContent("application/json")));

        var settings = _fixture.Build<GetTokenSettings>().With(x => x.HttpClientName, _httpClientName).Create();

        var sut = new TokenService(_httpClientFactory);

        var act = () => sut.GetTokenAsync(settings, _cancellationToken);

        var error = await Assert.ThrowsAsync<ServiceException>(act).ConfigureAwait(false);

        error.Should().NotBeNull();
        error.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        error.Message.Should().Be($"Get Token Call for {_httpClientName} was not successful");
    }

    [Fact]
    public async void GetTokenAsyncHttpClientThrows_Throws()
    {
        SetupHttpClient(new HttpMessageHandlerMock(HttpStatusCode.InternalServerError, ex: _testException));

        var settings = _fixture.Build<GetTokenSettings>().With(x => x.HttpClientName, _httpClientName).Create();

        var sut = new TokenService(_httpClientFactory);

        var act = () => sut.GetTokenAsync(settings, _cancellationToken);

        var error = await Assert.ThrowsAsync<ServiceException>(act).ConfigureAwait(false);

        error.Should().NotBeNull();
        error.InnerException.Should().Be(_testException);
        error.Message.Should().Be($"Get Token Call for {_httpClientName} threw exception");
    }

    #endregion

    #region Setup

    private void SetupHttpClient(HttpMessageHandler httpMessageHandler)
    {
        var httpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = _fixture.Create<Uri>()
        };
        A.CallTo(() => _httpClientFactory.CreateClient(_httpClientName))
            .Returns(httpClient);
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
