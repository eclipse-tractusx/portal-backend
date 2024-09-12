/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.BpnDidResolver.Library.Tests;

public class BpnDidResolveServiceTests
{
    private const string BPN = "BPNL0000000000XX";
    private readonly IBpnDidResolverService _sut;
    private readonly IHttpClientFactory _clientFactory;

    public BpnDidResolveServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.ConfigureFixture();

        _clientFactory = A.Fake<IHttpClientFactory>();

        _sut = new BpnDidResolverService(_clientFactory);
    }

    #region ValidateDid

    [Fact]
    public async Task ValidateDid_WithoutError_ReturnsTrue()
    {
        // Arrange
        const string did = "did:web:123";
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK);
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _clientFactory.CreateClient(nameof(BpnDidResolverService))).Returns(httpClient);

        // Act
        var result = await _sut.TransmitDidAndBpn(did, BPN, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateDid_WithError_ReturnsFalse()
    {
        // Arrange
        const string did = "did:web:123";
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _clientFactory.CreateClient(nameof(BpnDidResolverService))).Returns(httpClient);

        // Act
        var result = await _sut.TransmitDidAndBpn(did, BPN, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
