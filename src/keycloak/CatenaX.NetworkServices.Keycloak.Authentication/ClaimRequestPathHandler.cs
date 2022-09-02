/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CatenaX.NetworkServices.Keycloak.Authentication
{
    public class ClaimRequestPathRequirement : IAuthorizationRequirement
    {
        private readonly string _claim;
        private readonly string _parameter;
        public ClaimRequestPathRequirement(string claim, string parameter)
        {
            _claim = claim;
            _parameter = parameter;
        }
        public bool IsSuccess(IDictionary<string,object> routeValues, IEnumerable<Claim> claims)
        {
            var routeValue = routeValues[_parameter];
            if (routeValue == null) return false;
            var claim = claims.SingleOrDefault( x => x.Type == _claim );
            if (claim == null) return false;
            return claim.Value.Equals(routeValue);
        }
    }

    public class ClaimRequestPathHandler : AuthorizationHandler<ClaimRequestPathRequirement>
    {
        private IHttpContextAccessor _contextAccessor;

        public ClaimRequestPathHandler (IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ClaimRequestPathRequirement requirement)
        {
            if (_contextAccessor.HttpContext?.Request.RouteValues != null &&
                requirement.IsSuccess(_contextAccessor.HttpContext.Request.RouteValues!, context.User.Claims))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
            return Task.CompletedTask;
        }
    }
}
