/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Net;
using System.Text.Json;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Bpn;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Bpn.Model;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests
{
    public class BPNAccessTest
    {
        private readonly IFixture _fixture;

        public BPNAccessTest()
        {
            _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        }

        private void ConfigureHttpClientFactoryFixture(HttpResponseMessage httpResponseMessage)
        {
            var messageHandler = A.Fake<HttpMessageHandler>();
            A.CallTo(messageHandler) // mock protected method
                .Where(x => x.Method.Name == "SendAsync")
                .WithReturnType<Task<HttpResponseMessage>>()
                .Returns(httpResponseMessage);
            var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://localhost") };
            _fixture.Inject(httpClient);

            var httpClientFactory = _fixture.Freeze<Fake<IHttpClientFactory>>();
            A.CallTo(() => httpClientFactory.FakedObject.CreateClient("bpn")).Returns(httpClient);
        }

        [Fact]
        public async Task FetchBusinessPartner_Success()
        {
            var resultSet = _fixture.Create<FetchBusinessPartnerDto>();
            ConfigureHttpClientFactoryFixture(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(resultSet))
            });

            var httpClient = _fixture.Create<HttpClient>();
            var sut = _fixture.Create<BpnAccess>();

            var result = await sut.FetchBusinessPartner("testpbn", "token", CancellationToken.None).ToListAsync().ConfigureAwait(false);
            Assert.Equal(resultSet.Bpn, result.First().Bpn);
            Assert.Equal("token", httpClient.DefaultRequestHeaders.Authorization?.Parameter);
        }

        [Fact]
        public async Task FetchBusinessPartner_Failure()
        {
            ConfigureHttpClientFactoryFixture(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

            var httpClient = _fixture.Create<HttpClient>();
            var sut = _fixture.Create<BpnAccess>();

            async Task Act() => await sut.FetchBusinessPartner("testpbn", "token", CancellationToken.None).ToListAsync().ConfigureAwait(false);

            await Assert.ThrowsAsync<ServiceException>(Act);
            Assert.Equal("token", httpClient.DefaultRequestHeaders.Authorization?.Parameter);
        }
    }
}
