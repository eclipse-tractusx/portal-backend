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

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System.Json;
using System.Security.Claims;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication
{
    public class KeycloakClaimsTransformation : IClaimsTransformation
    {
        readonly JwtBearerOptions _Options;
        public KeycloakClaimsTransformation(IOptions<JwtBearerOptions> options)
        {
            _Options = options.Value;
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var resource_access = principal.Claims.FirstOrDefault(claim => claim.Type == "resource_access" && claim.ValueType == "JSON")?.Value;
            if ((resource_access != null) &&
                ((JsonValue.Parse(resource_access) as JsonObject)?.TryGetValue(_Options.TokenValidationParameters.ValidAudience, out JsonValue audience) ?? false) &&
                ((audience as JsonObject)?.TryGetValue("roles", out JsonValue roles) ?? false) &&
                roles is JsonArray)
            {
                ClaimsIdentity claimsIdentity = new ClaimsIdentity();
                bool rolesAdded = false;
                foreach(JsonValue role in roles)
                {
                    if (role.JsonType == JsonType.String)
                    {
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role,role));
                        rolesAdded = true;
                    }
                }
                if (rolesAdded)
                {
                    principal.AddIdentity(claimsIdentity);
                }
            }
            return Task.FromResult(principal);
        }
    }
}
