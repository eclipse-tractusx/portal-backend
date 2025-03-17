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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;

public class GeneralHttpExceptionFilter(
    ILogger<GeneralHttpExceptionFilter> logger,
    IErrorMessageService errorMessageService)
    : BaseHttpExceptionHandler(errorMessageService), IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var errorId = Guid.NewGuid().ToString();
        var error = context.Exception;
        var details = GetErrorDetails(error);
        var message = GetErrorMessage(error);
        LogErrorInformation(errorId, error);
        var (statusCode, messageFunc, logLevel) = GetErrorInformation(error);
        logger.Log(logLevel, error, "GeneralErrorHandler caught {Error} with errorId: {ErrorId} resulting in response status code {StatusCode}, message '{Message}'", error.GetType().Name, errorId, (int)statusCode, message);
        context.Result = new ContentResult
        {
            Content = JsonSerializer.Serialize(CreateErrorResponse(statusCode, error, errorId, message, details, messageFunc), Options),
            StatusCode = (int)statusCode
        };
    }
}
