/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using Serilog.Context;
using System.Collections.Immutable;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;

public class BaseHttpExceptionHandler(IErrorMessageService errorMessageService)
{
    protected static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    protected static readonly IReadOnlyDictionary<HttpStatusCode, MetaData> Metadata = ImmutableDictionary.CreateRange<HttpStatusCode, MetaData>(
    [
        new(HttpStatusCode.BadRequest, new MetaData("https://tools.ietf.org/html/rfc7231#section-6.5.1", "One or more validation errors occurred.")),
        new(HttpStatusCode.Conflict, new MetaData("https://tools.ietf.org/html/rfc7231#section-6.5.8", "The resorce is in conflict with the current request.")),
        new(HttpStatusCode.NotFound, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4", "Cannot find representation of target resource.")),
        new(HttpStatusCode.Forbidden, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3", "Access to requested resource is not permitted.")),
        new(HttpStatusCode.UnsupportedMediaType, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.13", "The server cannot process this type of content")),
        new(HttpStatusCode.BadGateway, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.3", "Error accessing external resource.")),
        new(HttpStatusCode.ServiceUnavailable, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.4", "Service is currently unavailable.")),
        new(HttpStatusCode.InternalServerError, new MetaData("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1", "The server encountered an unexpected condition.")),
    ]);

    protected static KeyValuePair<Type, (HttpStatusCode HttpStatusCode, Func<Exception, (string?, IEnumerable<string>)>? MessageFunc, LogLevel LogLevel)> CreateErrorEntry<T>(
            HttpStatusCode httpStatusCode,
            Func<T, (string?, IEnumerable<string>)>? messageFunc = null,
            LogLevel logLevel = LogLevel.Information
        ) where T : class =>
            KeyValuePair.Create<Type, (HttpStatusCode, Func<Exception, (string?, IEnumerable<string>)>?, LogLevel)>(
                typeof(T),
                (httpStatusCode,
                messageFunc == null
                    ? null
                    : e => messageFunc.Invoke(e as T ?? throw new UnexpectedConditionException($"Exception type {e.GetType()} should always be of type {typeof(T)} here")),
                logLevel));

    protected static readonly IReadOnlyDictionary<Type, (HttpStatusCode StatusCode, Func<Exception, (string?, IEnumerable<string>)>? MessageFunc, LogLevel LogLevel)> ErrorTypes = ImmutableDictionary.CreateRange(
    [
        CreateErrorEntry<ArgumentException>(HttpStatusCode.BadRequest, argumentException => (argumentException.ParamName, Enumerable.Repeat(argumentException.Message, 1))),
        CreateErrorEntry<ControllerArgumentException>(HttpStatusCode.BadRequest, caException => (caException.ParamName, Enumerable.Repeat(caException.Message, 1))),
        CreateErrorEntry<NotFoundException>(HttpStatusCode.NotFound),
        CreateErrorEntry<ConflictException>(HttpStatusCode.Conflict),
        CreateErrorEntry<ForbiddenException>(HttpStatusCode.Forbidden),
        CreateErrorEntry<ServiceException>(HttpStatusCode.BadGateway, serviceException => (serviceException.Source, new[] { serviceException.StatusCode == null ? "remote service call failed" : $"remote service returned status code: {(int)serviceException.StatusCode} {serviceException.StatusCode}", serviceException.Message })),
        CreateErrorEntry<UnsupportedMediaTypeException>(HttpStatusCode.UnsupportedMediaType),
        CreateErrorEntry<ConfigurationException>(HttpStatusCode.InternalServerError, configurationException => (configurationException.Source, new[] { $"Invalid service configuration: {configurationException.Message}" }))
    ]);

    protected static (HttpStatusCode StatusCode, Func<Exception, (string?, IEnumerable<string>)>? MessageFunc, LogLevel LogLevel) GetErrorInformation(Exception error) =>
        ErrorTypes.TryGetValue(error.GetType(), out var mapping)
            ? mapping
            : (HttpStatusCode.InternalServerError, null, LogLevel.Error);

    protected ErrorResponse CreateErrorResponse(HttpStatusCode statusCode, Exception error, string errorId, string message, IEnumerable<ErrorDetails>? details, Func<Exception, (string?, IEnumerable<string>)>? getSourceAndMessages)
    {
        var meta = Metadata.GetValueOrDefault(statusCode, Metadata[HttpStatusCode.InternalServerError]);
        var (source, messages) = getSourceAndMessages?.Invoke(error) ?? (error.Source, Enumerable.Repeat(message, 1));

        var messageMap = new Dictionary<string, IEnumerable<string>> { { source ?? "unknown", messages } };
        while (error.InnerException != null)
        {
            error = error.InnerException;
            source = error.Source ?? "inner";

            messageMap[source] = messageMap.TryGetValue(source, out messages)
                ? messages.Append(GetErrorMessage(error))
                : Enumerable.Repeat(GetErrorMessage(error), 1);
        }

        return new ErrorResponse(
            meta.Url,
            meta.Description,
            (int)statusCode,
            messageMap,
            errorId,
            details
        );
    }

    protected string GetErrorMessage(Exception exception) =>
        exception is DetailException { HasDetails: true } detail
            ? detail.GetErrorMessage(errorMessageService)
            : exception.Message;

    protected IEnumerable<ErrorDetails> GetErrorDetails(Exception exception) =>
        exception is DetailException { HasDetails: true } detail
            ? detail.GetErrorDetails(errorMessageService)
            : [];

    protected static void LogErrorInformation(string errorId, Exception exception)
    {
        LogContext.PushProperty("ErrorId", errorId);
        LogContext.PushProperty("StackTrace", exception.StackTrace);
    }

    protected sealed record MetaData(string Url, string Description);
}
