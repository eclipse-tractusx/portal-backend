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

using Microsoft.AspNetCore.Http;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;

public class CustomAuthorizationMiddleware(IErrorMessageService errorMessageService) :
    BaseHttpExceptionHandler(errorMessageService), IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await next(context).ConfigureAwait(ConfigureAwaitOptions.None);

        switch (context.Response.StatusCode)
        {
            case (int)HttpStatusCode.Unauthorized:
                context.Response.ContentType = "application/json";
                const string UnauthorizedAccess = "Unauthorized access";
                await context.Response.WriteAsJsonAsync(
                    CreateErrorResponse(
                        HttpStatusCode.Unauthorized,
                        new UnauthorizedAccessException(),
                        Guid.NewGuid().ToString(),
                        UnauthorizedAccess,
                        Enumerable.Repeat(new ErrorDetails("UnauthorizedAccess", nameof(UnauthorizedAccessException), UnauthorizedAccess, []), 1),
                        null),
                    Options).ConfigureAwait(ConfigureAwaitOptions.None);
                break;

            case (int)HttpStatusCode.Forbidden:
                context.Response.ContentType = "application/json";
                const string ForbiddenAccess = "Access forbidden";
                await context.Response.WriteAsJsonAsync(
                    CreateErrorResponse(
                        HttpStatusCode.Forbidden,
                        new ForbiddenException(),
                        Guid.NewGuid().ToString(),
                        ForbiddenAccess,
                        Enumerable.Repeat(new ErrorDetails("ForbiddenAccess", nameof(ForbiddenException), ForbiddenAccess, []), 1),
                        null),
                    Options).ConfigureAwait(ConfigureAwaitOptions.None);
                break;

            default:
                break;
        }
    }
}
