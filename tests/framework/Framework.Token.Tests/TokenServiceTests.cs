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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Net;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Token.Tests;

public class TokenServiceTests
{
    private readonly string _accessToken;
    private readonly CancellationToken _cancellationToken;
    private readonly TestException _testException;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IFixture _fixture;
    private readonly Uri _validBaseAddress = new("https://validurl.com");

    public TokenServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _accessToken = _fixture.Create<string>();
        _cancellationToken = new CancellationToken();
        _testException = _fixture.Create<TestException>();

        _httpClientFactory = A.Fake<IHttpClientFactory>();
    }

    #region GetAuthorizedClient

    [Fact]
    public async Task GetAuthorizedClient_Success()
    {
        var authResponse = JsonSerializer.Serialize(_fixture.Build<AuthResponse>().With(x => x.AccessToken, _accessToken).Create());
        SetupForGetAuthorized<TokenService>(new HttpMessageHandlerMock(HttpStatusCode.OK, authResponse.ToFormContent("application/json")));

        var settings = _fixture.Create<KeyVaultAuthSettings>();

        var sut = new TokenService(_httpClientFactory);

        var result = await sut.GetAuthorizedClient<TokenService>(settings, _cancellationToken).ConfigureAwait(false);

        result.Should().NotBeNull();
        result.BaseAddress.Should().Be(_validBaseAddress);
    }

    [Fact]
    public async Task GetAuthorizedClient_HttpClientError500_Throws()
    {
        var errorResponse = JsonSerializer.Serialize(_fixture.Create<ErrorResponse>());
        SetupForGetAuthorized<TokenService>(new HttpMessageHandlerMock(HttpStatusCode.InternalServerError, errorResponse.ToFormContent("application/json")));
        var settings = _fixture.Create<KeyVaultAuthSettings>();

        var sut = new TokenService(_httpClientFactory);

        var act = () => sut.GetAuthorizedClient<TokenService>(settings, _cancellationToken);

        var error = await Assert.ThrowsAsync<ServiceException>(act).ConfigureAwait(false);

        error.Should().NotBeNull();
        error.Message.Should().Be($"call to external system token-post failed with statuscode 500 - Message: {errorResponse}");
        error.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetAuthorizedClient_HttpClientThrows_Throws()
    {
        SetupForGetAuthorized<TokenService>(new HttpMessageHandlerMock(HttpStatusCode.InternalServerError, ex: _testException));
        var settings = _fixture.Create<KeyVaultAuthSettings>();

        var sut = new TokenService(_httpClientFactory);

        var act = () => sut.GetAuthorizedClient<TokenService>(settings, _cancellationToken);

        var error = await Assert.ThrowsAsync<ServiceException>(act).ConfigureAwait(false);

        error.Should().NotBeNull();
        error.InnerException.Should().Be(_testException);
        error.Message.Should().Be($"call to external system token-post failed");
    }

    #endregion

    #region Setup

    private void SetupForGetAuthorized<T>(HttpMessageHandler httpMessageHandler)
    {
        var httpClientAuth = new HttpClient(httpMessageHandler)
        {
            BaseAddress = _fixture.Create<Uri>()
        };
        var httpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = _validBaseAddress
        };
        A.CallTo(() => _httpClientFactory.CreateClient($"{typeof(T).Name}Auth"))
            .Returns(httpClientAuth);
        A.CallTo(() => _httpClientFactory.CreateClient(typeof(T).Name))
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
