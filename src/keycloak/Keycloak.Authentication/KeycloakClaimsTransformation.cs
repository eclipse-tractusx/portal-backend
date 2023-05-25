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

using Framework.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using System.Json;
using System.Security.Claims;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication
{
    public class KeycloakClaimsTransformation : IClaimsTransformation
    {
        private readonly IPortalRepositories _portalDbRepositories;
        private readonly JwtBearerOptions _options;

        public KeycloakClaimsTransformation(IOptions<JwtBearerOptions> options, IPortalRepositories portalDbRepositories)
        {
            _portalDbRepositories = portalDbRepositories;
            _options = options.Value;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var claimsIdentity = new ClaimsIdentity();
            var rolesAdded = AddRoles(principal, claimsIdentity);

            var sub = principal.Claims.SingleOrDefault(x => x.Type == PortalClaimTypes.Sub)?.Value;
            if (string.IsNullOrWhiteSpace(sub))
            {
                if (rolesAdded)
                {
                    principal.AddIdentity(claimsIdentity);
                }
                return principal;
            }

            var result = await _portalDbRepositories.GetInstance<IUserRepository>().GetUserDataByUserEntityId(sub).ConfigureAwait(false);
            if (result == null)
            {
                if (rolesAdded)
                {
                    principal.AddIdentity(claimsIdentity);
                }
                return principal;
            }

            claimsIdentity.AddClaim(new Claim(PortalClaimTypes.IdentityId, result.IdentityId.ToString()));
            claimsIdentity.AddClaim(new Claim(PortalClaimTypes.IdentityType, Enum.GetName(result.IdentityType) ?? throw new ConflictException($"IdentityType {(int)result.IdentityType} is out of range")));
            claimsIdentity.AddClaim(new Claim(PortalClaimTypes.CompanyId, result.CompanyId.ToString()));
            principal.AddIdentity(claimsIdentity);
            return principal;
        }

        private bool AddRoles(ClaimsPrincipal principal, ClaimsIdentity claimsIdentity)
        {
            var resource_access = principal.Claims
                .FirstOrDefault(claim => claim.Type == "resource_access" && claim.ValueType == "JSON")?.Value;
            if (resource_access == null ||
                !((JsonValue.Parse(resource_access) as JsonObject)?.TryGetValue(
                    _options.TokenValidationParameters.ValidAudience,
                    out var audience) ?? false) ||
                !((audience as JsonObject)?.TryGetValue("roles", out var roles) ?? false) ||
                roles is not JsonArray)
            {
                return false;
            }

            var rolesAdded = false;
            foreach (JsonValue role in roles)
            {
                if (role.JsonType != JsonType.String)
                {
                    continue;
                }

                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                rolesAdded = true;
            }

            return rolesAdded;
        }
    }
}
