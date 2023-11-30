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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Microsoft.Extensions.Logging;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Authorization
{
    public class MandatoryIdentityClaimRequirement : IAuthorizationRequirement
    {
        private readonly PolicyTypeId _policyTypeId;

        public MandatoryIdentityClaimRequirement(PolicyTypeId policyTypeId)
        {
            _policyTypeId = policyTypeId;
        }

        public PolicyTypeId PolicyTypeId { get => _policyTypeId; }
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

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MandatoryIdentityClaimRequirement requirement)
        {
            try
            {
                if (requirement.PolicyTypeId switch
                {
                    PolicyTypeId.ValidIdentity => _identityService.IdentityId != Guid.Empty,
                    PolicyTypeId.ValidCompany => _identityService.IdentityData.CompanyId != Guid.Empty,
                    PolicyTypeId.CompanyUser => _identityService.IdentityId != Guid.Empty && _identityService.IdentityData.IdentityType == IdentityTypeId.COMPANY_USER,
                    PolicyTypeId.ServiceAccount => _identityService.IdentityId != Guid.Empty && _identityService.IdentityData.IdentityType == IdentityTypeId.COMPANY_SERVICE_ACCOUNT,
                    var policyTypeId => throw new UnexpectedConditionException($"unexpected PolicyTypeId {policyTypeId}")
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
                _logger.LogInformation("unable to retrieve IdentityData", e);
                context.Fail();
            }
            return Task.CompletedTask;
        }
    }
}
