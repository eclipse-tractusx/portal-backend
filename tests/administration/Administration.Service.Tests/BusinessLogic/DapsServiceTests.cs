/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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
using Microsoft.Extensions.Options;
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared;
using System.Net;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class DapsServiceTests
{
    #region Initialization
    
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<DapsSettings> _options;

    public DapsServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _options = Options.Create(new DapsSettings
        {
            DapsUrl = "https://www.api.daps-not-existing.com",
        });
        _httpClientFactory = A.Fake<IHttpClientFactory>();
    }

    #endregion
    
    #region EnableDapsAuth
    
    [Fact]
    public async Task EnableDapsAuthAsync_WithValidCall_ReturnsExpected()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pem", "application/x-pem-file");

        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK);
        CreateHttpClient(httpMessageHandlerMock);
        const string clientName = "Connec Tor";
        const string referringConnector = "https://connect-tor.com";
        const string businessPartnerNumber = "BPNL000000000009";
        const string accessToken = "this-is-a-super-secret-secret-not";
        var service = new DapsService(_httpClientFactory);

        // Act
        var result = await service.EnableDapsAuthAsync(clientName, accessToken, referringConnector, businessPartnerNumber, file, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public async Task EnableDapsAuthAsync_WithUnsuccessfulStatusCode_ThrowsException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pem", "application/x-pem-file");

        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        CreateHttpClient(httpMessageHandlerMock);
        const string clientName = "Connec Tor";
        const string referringConnector = "https://connect-tor.com";
        const string businessPartnerNumber = "BPNL000000000009";
        const string accessToken = "this-is-a-super-secret-secret-not";
        var service = new DapsService(_httpClientFactory);

        // Act
        async Task Act() => await service.EnableDapsAuthAsync(clientName, accessToken, referringConnector, businessPartnerNumber, file, CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);
    }

    [Fact]
    public async Task EnableDapsAuthAsync_WithException_ThrowsException()
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("Content of the super secure certificate", "test.pem", "application/x-pem-file");

        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest, ex:  new HttpRequestException ("DNS Error"));
        CreateHttpClient(httpMessageHandlerMock);
        const string clientName = "Connec Tor";
        const string referringConnector = "https://connect-tor.com";
        const string businessPartnerNumber = "BPNL000000000009";
        const string accessToken = "this-is-a-super-secret-secret-not";
        var service = new DapsService(_httpClientFactory);

        // Act
        async Task Act() => await service.EnableDapsAuthAsync(clientName, accessToken, referringConnector, businessPartnerNumber, file, CancellationToken.None).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);
    }

    #endregion

    #region Setup

    private void CreateHttpClient(HttpMessageHandler httpMessageHandlerMock)
    {
        var httpClient = new HttpClient(httpMessageHandlerMock) { BaseAddress = new Uri(_options.Value.DapsUrl) };
        A.CallTo(() => _httpClientFactory.CreateClient(A<string>._)).Returns(httpClient);
    }

    #endregion
}
