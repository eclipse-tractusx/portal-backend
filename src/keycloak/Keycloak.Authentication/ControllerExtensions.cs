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

using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using System.Security.Claims;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;

/// <summary>
/// Extension methods for API controllers.
/// </summary>
public static class ControllerExtensions
{
    public static T WithBearerToken<T>(this ControllerBase controller, Func<string, T> tokenConsumingFunction) =>
        tokenConsumingFunction(controller.GetBearerToken());
 
    public static IdentityData GetIdentityData(this ClaimsPrincipal user)
    {
        var sub = user.Claims.GetStringFromClaim(PortalClaimTypes.Sub);
        var identityId = user.Claims.GetGuidFromClaim(PortalClaimTypes.IdentityId);
        var identityType = user.Claims.GetEnumFromClaim<IdentityTypeId>(PortalClaimTypes.IdentityType);
        var companyId = user.Claims.GetGuidFromClaim(PortalClaimTypes.CompanyId);
        return new IdentityData(sub, identityId, identityType, companyId);
    }

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

    private static string GetStringFromClaim(this IEnumerable<Claim> claims, string claimType)
    {
        var claimValue = claims.SingleOrDefault(x => x.Type == claimType)?.Value;
        if (string.IsNullOrWhiteSpace(claimValue))
        {
            throw new ControllerArgumentException($"Claim {claimType} must not be null or empty.", nameof(claims));
        }

        return claimValue;
    }

    private static Guid GetGuidFromClaim(this IEnumerable<Claim> claims, string claimType)
    {
        var claimValue = claims.SingleOrDefault(x => x.Type == claimType)?.Value;
        if (string.IsNullOrWhiteSpace(claimValue))
        {
            throw new ControllerArgumentException($"Claim '{claimType} must not be null or empty.");
        }

        if (!Guid.TryParse(claimValue, out var result) || Guid.Empty == result)
        {
            throw new ControllerArgumentException($"Claim {claimType} must contain a Guid");
        }

        return result;
    }

    private static T GetEnumFromClaim<T>(this IEnumerable<Claim> claims, string claimType) where T : struct, Enum
    {
        var claimValue = claims.SingleOrDefault(x => x.Type == claimType)?.Value;
        if (string.IsNullOrWhiteSpace(claimValue))
        {
            throw new ControllerArgumentException($"Claim '{claimType} must not be null or empty.");
        }

        if (!Enum.TryParse(claimValue, true, out T result))
        {
            throw new ControllerArgumentException($"Claim {claimType} must contain a {typeof(T)}");
        }

        return result;
    }
}
