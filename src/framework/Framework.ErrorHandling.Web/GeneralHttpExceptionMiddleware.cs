/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;

public class GeneralHttpExceptionMiddleware(ILogger<GeneralHttpExceptionMiddleware> logger, IErrorMessageService errorMessageService) :
    BaseHttpExceptionHandler(errorMessageService), IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context).ConfigureAwait(ConfigureAwaitOptions.None);
        }
        catch (Exception exception)
        {
            var errorId = Guid.NewGuid().ToString();
            var details = GetErrorDetails(exception);
            var message = GetErrorMessage(exception);
            LogErrorInformation(errorId, exception);
            var (statusCode, messageFunc, logLevel) = GetErrorInformation(exception);
            logger.Log(logLevel, exception, "GeneralErrorHandler caught {Error} with errorId: {ErrorId} resulting in response status code {StatusCode}, message '{Message}'", exception.GetType().Name, errorId, (int)statusCode, message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(CreateErrorResponse(statusCode, exception, errorId, message, details, messageFunc), Options)).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }
}
