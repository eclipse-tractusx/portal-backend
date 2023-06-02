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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Logging;
using Serilog.Context;
using System.Net;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;

public class GeneralHttpErrorHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    private static readonly IReadOnlyDictionary<HttpStatusCode, MetaData> Metadata = new Dictionary<HttpStatusCode, MetaData>()
    {
        { HttpStatusCode.BadRequest, new MetaData("https://tools.ietf.org/html/rfc7231#section-6.5.1", "One or more validation errors occurred.") },
        { HttpStatusCode.Conflict, new MetaData("https://tools.ietf.org/html/rfc7231#section-6.5.8", "The resorce is in conflict with the current request.") },
        { HttpStatusCode.NotFound, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4", "Cannot find representation of target resource.") },
        { HttpStatusCode.Forbidden, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3", "Access to requested resource is not permitted.") },
        { HttpStatusCode.UnsupportedMediaType, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.13", "The server cannot process this type of content") },
        { HttpStatusCode.BadGateway, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.3", "Error accessing external resource.") },
        { HttpStatusCode.ServiceUnavailable, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.4", "Service is currently unavailable.") },
        { HttpStatusCode.InternalServerError, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1", "The server encountered an unexpected condition.") }
    };

    public GeneralHttpErrorHandler(RequestDelegate next, ILogger<GeneralHttpErrorHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception error)
        {
            var errorId = Guid.NewGuid().ToString();
            LogErrorInformation(errorId, error);
            var (statusCode, messageFunc, logLevel) = GetErrorInformation(error);

            _logger.Log(logLevel, error, "GeneralErrorHandler caught {Error} with errorId: {ErrorId} resulting in response status code {StatusCode}, message '{Message}'", error.GetType().Name, errorId, (int)statusCode, error.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(CreateErrorResponse(statusCode, error, errorId, messageFunc))).ConfigureAwait(false);
        }
    }

    private static (HttpStatusCode StatusCode, Func<Exception, (string?, IEnumerable<string>)>? MessageFunc, LogLevel LogLevel) GetErrorInformation(Exception error)
    {
        HttpStatusCode statusCode;
        var logLevel = LogLevel.Information;
        Func<Exception, (string?, IEnumerable<string>)>? messageFunc = null;

        if (error is ArgumentException argumentException)
        {
            statusCode = HttpStatusCode.BadRequest;
            messageFunc = _ => (argumentException.ParamName, Enumerable.Repeat(argumentException.Message, 1));
        }
        else if (error is ControllerArgumentException caException)
        {
            statusCode = HttpStatusCode.BadRequest;
            messageFunc = _ => (caException.ParamName, Enumerable.Repeat(caException.Message, 1));
        }
        else if (error is NotFoundException)
        {
            statusCode = HttpStatusCode.NotFound;
        }
        else if (error is ConflictException)
        {
            statusCode = HttpStatusCode.Conflict;
        }
        else if (error is ForbiddenException)
        {
            statusCode = HttpStatusCode.Forbidden;
        }
        else if (error is ServiceException serviceException)
        {
            statusCode = HttpStatusCode.BadGateway;
            var serviceStatus = serviceException.StatusCode;
            messageFunc = ex => (ex.Source, new[]
            {
                serviceStatus == null
                    ? "remote service call failed"
                    : $"remote service returned status code: {(int) serviceStatus} {serviceStatus}",
                error.Message
            });
        }
        else if (error is UnsupportedMediaTypeException)
        {
            statusCode = HttpStatusCode.UnsupportedMediaType;
        }
        else if (error is ConfigurationException)
        {
            statusCode = HttpStatusCode.InternalServerError;
            messageFunc = ex => (ex.Source, new[] { $"Invalid service configuration: {ex.Message}" });
        }
        else
        {
            statusCode = HttpStatusCode.InternalServerError;
            logLevel = LogLevel.Error;
        }

        return (statusCode, messageFunc, logLevel);
    }

    private static ErrorResponse CreateErrorResponse(HttpStatusCode statusCode, Exception error, string errorId, Func<Exception, (string?, IEnumerable<string>)>? getSourceAndMessages = null)
    {
        var meta = Metadata.GetValueOrDefault(statusCode, Metadata[HttpStatusCode.InternalServerError]);
        var (source, messages) = getSourceAndMessages?.Invoke(error) ?? (error.Source, Enumerable.Repeat(error.Message, 1));

        var messageMap = new Dictionary<string, IEnumerable<string>> { { source ?? "unknown", messages } };
        while (error.InnerException != null)
        {
            error = error.InnerException;
            source = error.Source ?? "inner";

            messageMap[source] = messageMap.TryGetValue(source, out messages)
                ? messages.Append(error.Message)
                : Enumerable.Repeat(error.Message, 1);
        }

        return new ErrorResponse(
            meta.Url,
            meta.Description,
            (int)statusCode,
            messageMap,
            errorId
        );
    }

    private static void LogErrorInformation(string errorId, Exception exception)
    {
        LogContext.PushProperty("ErrorId", errorId);
        LogContext.PushProperty("StackTrace", exception.StackTrace);
    }

    private sealed record MetaData(string Url, string Description);
}
