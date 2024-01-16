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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Security.Claims;

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
        private readonly IIdentityRepository _identityRepository;
        private readonly IServiceAccountRepository _serviceAccountRepository;
        private readonly IClaimsIdentityDataBuilder _identityDataBuilder;
        private readonly ILogger<MandatoryIdentityClaimHandler> _logger;

        public MandatoryIdentityClaimHandler(IClaimsIdentityDataBuilder claimsIdentityDataBuilder, IPortalRepositories portalRepositories, ILogger<MandatoryIdentityClaimHandler> logger)
        {
            _identityDataBuilder = claimsIdentityDataBuilder;
            _identityRepository = portalRepositories.GetInstance<IIdentityRepository>();
            _serviceAccountRepository = portalRepositories.GetInstance<IServiceAccountRepository>();
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MandatoryIdentityClaimRequirement requirement)
        {
            if (_identityDataBuilder.Status == IClaimsIdentityDataBuilderStatus.Initial)
            {
                await InitializeClaims(context.User).ConfigureAwait(false);
            }
            if (_identityDataBuilder.Status == IClaimsIdentityDataBuilderStatus.Empty)
            {
                context.Fail();
                return;
            }
            if (requirement.PolicyTypeId switch
            {
                PolicyTypeId.ValidIdentity => _identityDataBuilder.IdentityId != Guid.Empty,
                PolicyTypeId.ValidCompany => (await GetCompanyId().ConfigureAwait(false)) != Guid.Empty,
                PolicyTypeId.CompanyUser => _identityDataBuilder.IdentityTypeId == IdentityTypeId.COMPANY_USER,
                PolicyTypeId.ServiceAccount => _identityDataBuilder.IdentityTypeId == IdentityTypeId.COMPANY_SERVICE_ACCOUNT,
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

        private async ValueTask InitializeClaims(ClaimsPrincipal principal)
        {
            var preferredUserName = principal.Claims.SingleOrDefault(x => x.Type == PortalClaimTypes.PreferredUserName)?.Value;
            if (Guid.TryParse(preferredUserName, out var identityId))
            {
                _identityDataBuilder.AddIdentityId(identityId);
                _identityDataBuilder.AddIdentityTypeId(IdentityTypeId.COMPANY_USER);
                _identityDataBuilder.Status = IClaimsIdentityDataBuilderStatus.Initialized;
                return;
            }

            (Guid IdentityId, Guid CompanyId) serviceAccountData;
            var clientId = principal.Claims.SingleOrDefault(x => x.Type == PortalClaimTypes.ClientId)?.Value;
            if (!string.IsNullOrWhiteSpace(clientId) && (serviceAccountData = await _serviceAccountRepository.GetServiceAccountDataByClientId(clientId).ConfigureAwait(false)) != default)
            {
                _identityDataBuilder.AddIdentityId(serviceAccountData.IdentityId);
                _identityDataBuilder.AddIdentityTypeId(IdentityTypeId.COMPANY_SERVICE_ACCOUNT);
                _identityDataBuilder.AddCompanyId(serviceAccountData.CompanyId);
                _identityDataBuilder.Status = IClaimsIdentityDataBuilderStatus.Complete;
                return;
            }

            var sub = principal.Claims.SingleOrDefault(x => x.Type == PortalClaimTypes.Sub)?.Value;
            _logger.LogInformation("Preferred user name {PreferredUserName} couldn't be parsed to uuid for sub {Sub}", preferredUserName, sub);

            (Guid IdentityId, IdentityTypeId IdentityTypeId, Guid CompanyId) identityData;
            if (!string.IsNullOrWhiteSpace(sub) && (identityData = await _identityRepository.GetActiveIdentityDataByUserEntityId(sub).ConfigureAwait(false)) != default)
            {
                _identityDataBuilder.AddIdentityId(identityData.IdentityId);
                _identityDataBuilder.AddIdentityTypeId(identityData.IdentityTypeId);
                _identityDataBuilder.AddCompanyId(identityData.CompanyId);
                _identityDataBuilder.Status = IClaimsIdentityDataBuilderStatus.Complete;
                return;
            }

            _logger.LogWarning("No identity found for userEntityId {Sub}", sub);
            _identityDataBuilder.Status = IClaimsIdentityDataBuilderStatus.Empty;
        }

        private async ValueTask<Guid> GetCompanyId()
        {
            if (_identityDataBuilder.Status == IClaimsIdentityDataBuilderStatus.Initialized)
            {
                _identityDataBuilder.AddCompanyId(await _identityRepository.GetActiveCompanyIdByIdentityId(_identityDataBuilder.IdentityId).ConfigureAwait(false));
                _identityDataBuilder.Status = IClaimsIdentityDataBuilderStatus.Complete;
            }
            return _identityDataBuilder.CompanyId;
        }
    }
}
