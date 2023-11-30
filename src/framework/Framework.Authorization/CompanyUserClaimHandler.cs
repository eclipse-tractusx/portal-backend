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

using Microsoft.AspNetCore.Authorization;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Authorization
{
    public class CompanyUserClaimRequirement : IAuthorizationRequirement
    {
    }

    public class CompanyUserClaimHandler : AuthorizationHandler<CompanyUserClaimRequirement>
    {
        private readonly IIdentityService _identityService;

        public CompanyUserClaimHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CompanyUserClaimRequirement requirement)
        {
            if (_identityService.IdentityData.IdentityType == IdentityTypeId.COMPANY_USER)
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
