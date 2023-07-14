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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Security.Claims;

namespace Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.IntegrationTests;

public class FakePolicyEvaluator : IPolicyEvaluator
{
    public async Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var testScheme = "FakeScheme";
        var principal = new ClaimsPrincipal();
        principal.AddIdentity(new ClaimsIdentity(new[] {
            new Claim(PortalClaimTypes.Sub, "3d8142f1-860b-48aa-8c2b-1ccb18699f65"),
            new Claim(ClaimTypes.Role, "Administrator"),
            new Claim(ClaimTypes.NameIdentifier, "John"),
            new Claim(PortalClaimTypes.IdentityId, "ac1cf001-7fbc-1f2f-817f-bce058020001"),
            new Claim(PortalClaimTypes.IdentityType, IdentityTypeId.COMPANY_USER.ToString()),
            new Claim(PortalClaimTypes.CompanyId, "2dc4249f-b5ca-4d42-bef1-7a7a950a4f87")
        }, testScheme));

        return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal,
            new AuthenticationProperties(), testScheme)));
    }

    public async Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy,
        AuthenticateResult authenticationResult, HttpContext context, object? resource)
    {
        return await Task.FromResult(PolicyAuthorizationResult.Success());
    }
}
