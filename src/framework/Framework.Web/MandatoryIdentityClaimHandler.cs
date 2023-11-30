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

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Web
{
    public class MandatoryIdentityClaimRequirement : IAuthorizationRequirement
    {
        public MandatoryIdentityClaimRequirement(PolicyTypeId policyTypeId)
        {
            PolicyTypeId = policyTypeId;
        }

        public PolicyTypeId PolicyTypeId { get; }
    }

    public class MandatoryIdentityClaimHandler : AuthorizationHandler<MandatoryIdentityClaimRequirement>
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<MandatoryIdentityClaimHandler> _logger;

        public MandatoryIdentityClaimHandler(IIdentityService identityService, ILogger<MandatoryIdentityClaimHandler> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MandatoryIdentityClaimRequirement requirement)
        {
            try
            {
                if (requirement.PolicyTypeId switch
                {
                    PolicyTypeId.ValidIdentity => _identityService.IdentityId != Guid.Empty,
                    PolicyTypeId.ValidCompany => (await _identityService.GetIdentityData().ConfigureAwait(false)).CompanyId != Guid.Empty,
                    PolicyTypeId.CompanyUser => _identityService.IdentityId != Guid.Empty && (await _identityService.GetIdentityData().ConfigureAwait(false)).IdentityType == IdentityTypeId.COMPANY_USER,
                    PolicyTypeId.ServiceAccount => _identityService.IdentityId != Guid.Empty && (await _identityService.GetIdentityData().ConfigureAwait(false)).IdentityType == IdentityTypeId.COMPANY_SERVICE_ACCOUNT,
                    _ => throw new UnexpectedConditionException($"unexpected PolicyTypeId {requirement.PolicyTypeId}")
                })
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, "unable to retrieve IdentityData");
                context.Fail();
            }
        }
    }
}
