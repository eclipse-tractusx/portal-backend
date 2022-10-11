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

using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Org.CatenaX.Ng.Portal.Backend.Tests.Shared.Extensions;

/// <summary>
/// Extension methods for the controller
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Creates a claum for the identity user and adds it to the controller context
    /// </summary>
    /// <param name="controller">The controller that should be enriched</param>
    /// <param name="iamUserId">Id of the iamUser</param>
    public static void AddControllerContextWithClaim(this ControllerBase controller, string iamUserId)
    {
        var claimsIdentity = new ClaimsIdentity();
        claimsIdentity.AddClaims(new[] {new Claim("sub", iamUserId)});
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(claimsIdentity)
        };

        var controllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        controller.ControllerContext = controllerContext;
    }

    /// <summary>
    /// Creates a claum for the identity user and adds it to the controller context
    /// </summary>
    /// <param name="controller">The controller that should be enriched</param>
    /// <param name="iamUserId">Id of the iamUser</param>
    public static void AddControllerContextWithClaimAndBearer(this ControllerBase controller, string iamUserId, string accessToken)
    {
        var claimsIdentity = new ClaimsIdentity();
        claimsIdentity.AddClaims(new[] {new Claim("sub", iamUserId)});
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(claimsIdentity)
        };

        httpContext.Request.Headers.Authorization = $"Bearer {accessToken}";
        var controllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        controller.ControllerContext = controllerContext;
    }
}