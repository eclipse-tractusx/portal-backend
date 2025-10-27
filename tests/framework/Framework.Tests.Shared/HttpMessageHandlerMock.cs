/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;

public class HttpMessageHandlerMock(
    HttpStatusCode statusCode,
    HttpContent? httpContent = null,
    Exception? ex = null,
    bool isRequestUri = false)
    : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        RequestMessage = request;

        if (ex != null)
        {
            throw ex;
        }

        var httpResponseMessage = new HttpResponseMessage(statusCode)
        {
            RequestMessage = isRequestUri ? request : null
        };
        if (httpContent != null)
        {
            httpResponseMessage.Content = httpContent;
        }

        return Task.FromResult(httpResponseMessage);
    }

    public HttpRequestMessage? RequestMessage { get; private set; } = null;
}
