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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;

/// <summary>
/// Extension methods for API controllers.
/// </summary>
public static class ControllerExtensions
{
    private static readonly string BusinessPartnerNumberHeader = "Business-Partner-Number";

    public static T WithBearerToken<T>(this ControllerBase controller, Func<string, T> tokenConsumingFunction) =>
        tokenConsumingFunction(controller.GetBearerToken());

    public static T WithBpn<T>(this ControllerBase controller, Func<string, T> bpnConsumingFunction) =>
        bpnConsumingFunction(controller.GetBpn());

    private static string GetBearerToken(this ControllerBase controller)
    {
        var authorization = controller.Request.Headers.Authorization.FirstOrDefault();
        if (authorization == null || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
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

    private static string GetBpn(this ControllerBase controller)
    {
        if (!controller.Request.Headers.TryGetValue(BusinessPartnerNumberHeader, out var values))
        {
            throw new ControllerArgumentException("Request does not contain Business-Partner-Number header", BusinessPartnerNumberHeader);
        }

        var bpn = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(bpn))
        {
            throw new ControllerArgumentException("Business-Partner-Number in header must not be empty", BusinessPartnerNumberHeader);
        }

        return bpn;
    }
}
