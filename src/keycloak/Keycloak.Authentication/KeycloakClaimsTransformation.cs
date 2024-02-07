/********************************************************************************
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

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using System.Json;
using System.Security.Claims;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication
{
    public class KeycloakClaimsTransformation : IClaimsTransformation
    {
        private readonly JwtBearerOptions _options;

        public KeycloakClaimsTransformation(IOptions<JwtBearerOptions> options)
        {
            _options = options.Value;
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var claimsIdentity = new ClaimsIdentity();
            if (AddRoles(principal, claimsIdentity))
            {
                principal.AddIdentity(claimsIdentity);
            }
            return Task.FromResult(principal);
        }

        private bool AddRoles(ClaimsPrincipal principal, ClaimsIdentity claimsIdentity) =>
            principal.Claims
                .Where(claim =>
                    claim.Type == PortalClaimTypes.ResourceAccess &&
                    claim.ValueType == "JSON")
                .SelectMany(claim =>
                    JsonValue.Parse(claim.Value) is JsonObject jsonObject &&
                    jsonObject.TryGetValue(
                        _options.TokenValidationParameters.ValidAudience,
                        out var audience) &&
                    audience is JsonObject client &&
                    client.TryGetValue("roles", out var jsonRoles) &&
                    jsonRoles is JsonArray roles
                        ? roles.Where(x => x.JsonType == JsonType.String)
                               .Select(role => new Claim(ClaimTypes.Role, role))
                        : Enumerable.Empty<Claim>())
                .IfAny(claims =>
                {
                    foreach (var claim in claims)
                    {
                        claimsIdentity.AddClaim(claim);
                    }
                });
    }
}
