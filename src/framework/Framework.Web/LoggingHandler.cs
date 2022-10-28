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

using Microsoft.Extensions.Logging;

namespace Org.CatenaX.Ng.Portal.Backend.Framework.Web;

public class LoggingHandler<TLogger> : DelegatingHandler 
    where TLogger : class
{
    private readonly ILogger<TLogger> _logger;

    public LoggingHandler(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TLogger>();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Request: {Request}", request.ToString());
        if (request.Content is { } content)
        {
            _logger.LogDebug("Request Content: {Content}", await content.ReadAsStringAsync(cancellationToken));
        }
        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogInformation("Response: {Response}", response.ToString());
        _logger.LogDebug("Responded with status code: {StatusCode} and data: {Data}", response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));

        return response;
    }
}
