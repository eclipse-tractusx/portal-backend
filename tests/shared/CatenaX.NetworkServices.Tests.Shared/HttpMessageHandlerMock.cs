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

using System.Net;

namespace CatenaX.NetworkServices.Tests.Shared;

public class HttpMessageHandlerMock : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly HttpContent? _httpContent;

    public HttpMessageHandlerMock(HttpStatusCode statusCode, HttpContent? httpContent = null)
    {
        _statusCode = statusCode;
        _httpContent = httpContent;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpResponseMessage = new HttpResponseMessage(_statusCode);
        if (_httpContent != null)
        {
            httpResponseMessage.Content = _httpContent;
        }

        return Task.FromResult(httpResponseMessage);
    }
}