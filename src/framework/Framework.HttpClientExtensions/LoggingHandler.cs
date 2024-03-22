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

using Microsoft.Extensions.Logging;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;

public class LoggingHandler<TLogger> : DelegatingHandler
    where TLogger : class
{
    private readonly ILogger<TLogger> _logger;

    public LoggingHandler(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TLogger>();
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
        {
            return base.SendAsync(request, cancellationToken);
        }
        return SendAsyncInternal(request, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsyncInternal(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Request: {@Request}", request);
        if (request.Content is { } content)
        {
            _logger.LogDebug("Request Content: {@Content}", await content.ReadAsStringAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None));
        }
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        _logger.LogInformation("Response: {@Response}", response);
        _logger.LogDebug("Responded with status code: {@StatusCode} and data: {@Data}", response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None));
        return response;
    }
}
