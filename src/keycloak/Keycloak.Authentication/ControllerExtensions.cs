/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;

/// <summary>
/// Extension methods for API controllers.
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Determines IamUserId from request claims and subsequently calls a provided function with iamUserId as parameter.
    /// </summary>
    /// <typeparam name="T">Return type of the controller function.</typeparam>
    /// <param name="controller">Controller to extend.</param>
    /// <param name="idConsumingFunction">Function that is called with iamUserId parameter.</param>
    /// <returns>Result of inner function.</returns>
    /// <exception cref="ArgumentException">If expected claim value is not provided.</exception>
    public static T WithIamUserId<T>(this ControllerBase controller, Func<string, T> idConsumingFunction)
    {
        var sub = controller.GetIamUserId();
        return idConsumingFunction(sub);
    }

    public static T WithBearerToken<T>(this ControllerBase controller, Func<string, T> tokenConsumingFunction)
    {
        var bearer = controller.GetBearerToken();
        return tokenConsumingFunction(bearer);
    }
    
    public static T WithIamUserAndBearerToken<T>(this ControllerBase controller, Func<(string iamUserId, string bearerToken), T> tokenConsumingFunction)
    {
        var bearerToken = controller.GetBearerToken();
        var iamUserId = controller.GetIamUserId();
        return tokenConsumingFunction((iamUserId, bearerToken));
    }

    private static string GetIamUserId(this ControllerBase controller)
    {
        var sub = controller.User.Claims.SingleOrDefault(x => x.Type == "sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub))
        {
            throw new ControllerArgumentException("Claim 'sub' must not be null or empty.", nameof(sub));
        }

        return sub;
    }

    private static string GetBearerToken(this ControllerBase controller)
    {
        var authorization = controller.Request.Headers.Authorization.FirstOrDefault();
        if (authorization == null || !authorization.StartsWith("Bearer "))
        {
            throw new ControllerArgumentException("Request does not contain a Bearer-token in authorization-header",
                nameof(authorization));
        }

        var bearer = authorization.Substring("Bearer ".Length);
        if (string.IsNullOrWhiteSpace(bearer))
        {
            throw new ControllerArgumentException("Bearer-token in authorization-header must not be empty", nameof(bearer));
        }

        return bearer;
    }
}
