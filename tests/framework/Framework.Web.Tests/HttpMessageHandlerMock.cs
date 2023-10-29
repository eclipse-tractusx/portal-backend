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

using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Web.Tests;

public class HttpMessageHandlerMock : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly Exception? _ex;
    private readonly HttpContent? _httpContent;
    private readonly bool _isRequestUri;

    public HttpMessageHandlerMock(HttpStatusCode statusCode, HttpContent? httpContent = null, Exception? ex = null, bool IsRequestUri = false)
    {
        _statusCode = statusCode;
        _httpContent = httpContent;
        _ex = ex;
        _isRequestUri = IsRequestUri;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        RequestMessage = request;

        if (_ex != null)
        {
            throw _ex;
        }

        var httpResponseMessage = new HttpResponseMessage(_statusCode)
        {
            RequestMessage = _isRequestUri ? request : null
        };
        if (_httpContent != null)
        {
            httpResponseMessage.Content = _httpContent;
        }

        return Task.FromResult(httpResponseMessage);
    }

    public HttpRequestMessage? RequestMessage { get; private set; } = null;
}
