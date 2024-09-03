/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using System.Security.Claims;

namespace Org.Eclipse.TractusX.Portal.Backend.Web.Identity;

public class MandatoryIdentityClaimRequirement : IAuthorizationRequirement
{
    public MandatoryIdentityClaimRequirement(PolicyTypeId policyTypeId)
    {
        PolicyTypeId = policyTypeId;
    }

    public PolicyTypeId PolicyTypeId { get; }
}

public class MandatoryIdentityClaimHandler(
    IClaimsIdentityDataBuilder claimsIdentityDataBuilder,
    IPortalRepositories portalRepositories,
    ILogger<MandatoryIdentityClaimHandler> logger)
    : AuthorizationHandler<MandatoryIdentityClaimRequirement>
{
    private readonly IIdentityRepository _identityRepository = portalRepositories.GetInstance<IIdentityRepository>();
    private readonly ITechnicalUserRepository _technicalUserRepository = portalRepositories.GetInstance<ITechnicalUserRepository>();

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MandatoryIdentityClaimRequirement requirement)
    {
        if (claimsIdentityDataBuilder.Status == IClaimsIdentityDataBuilderStatus.Initial)
        {
            await InitializeClaims(context.User).ConfigureAwait(false);
        }
        if (claimsIdentityDataBuilder.Status == IClaimsIdentityDataBuilderStatus.Empty)
        {
            context.Fail();
            return;
        }
        if (requirement.PolicyTypeId switch
        {
            PolicyTypeId.ValidIdentity => claimsIdentityDataBuilder.IdentityId != Guid.Empty,
            PolicyTypeId.ValidCompany => (await GetCompanyId().ConfigureAwait(false)) != Guid.Empty,
            PolicyTypeId.CompanyUser => claimsIdentityDataBuilder.IdentityTypeId == Framework.Identity.IdentityTypeId.COMPANY_USER,
            PolicyTypeId.ServiceAccount => claimsIdentityDataBuilder.IdentityTypeId == Framework.Identity.IdentityTypeId.COMPANY_SERVICE_ACCOUNT,
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
            claimsIdentityDataBuilder.AddIdentityId(identityId);
            claimsIdentityDataBuilder.AddIdentityTypeId(Framework.Identity.IdentityTypeId.COMPANY_USER);
            claimsIdentityDataBuilder.Status = IClaimsIdentityDataBuilderStatus.Initialized;
            return;
        }

        (Guid IdentityId, Guid CompanyId) serviceAccountData;
        var clientId = principal.Claims.SingleOrDefault(x => x.Type == PortalClaimTypes.ClientId)?.Value;
        if (!string.IsNullOrWhiteSpace(clientId) && (serviceAccountData = await _technicalUserRepository.GetTechnicalUserDataByClientId(clientId).ConfigureAwait(ConfigureAwaitOptions.None)) != default)
        {
            claimsIdentityDataBuilder.AddIdentityId(serviceAccountData.IdentityId);
            claimsIdentityDataBuilder.AddIdentityTypeId(Framework.Identity.IdentityTypeId.COMPANY_SERVICE_ACCOUNT);
            claimsIdentityDataBuilder.AddCompanyId(serviceAccountData.CompanyId);
            claimsIdentityDataBuilder.Status = IClaimsIdentityDataBuilderStatus.Complete;
            return;
        }

        var sub = principal.Claims.SingleOrDefault(x => x.Type == PortalClaimTypes.Sub)?.Value;
        logger.LogInformation("Preferred user name {PreferredUserName} couldn't be parsed to uuid for sub {Sub}", preferredUserName, sub);

        (Guid IdentityId, Framework.Identity.IdentityTypeId IdentityTypeId, Guid CompanyId) identityData;
        if (!string.IsNullOrWhiteSpace(sub) && (identityData = await _identityRepository.GetActiveIdentityDataByUserEntityId(sub).ConfigureAwait(ConfigureAwaitOptions.None)) != default)
        {
            claimsIdentityDataBuilder.AddIdentityId(identityData.IdentityId);
            claimsIdentityDataBuilder.AddIdentityTypeId(identityData.IdentityTypeId);
            claimsIdentityDataBuilder.AddCompanyId(identityData.CompanyId);
            claimsIdentityDataBuilder.Status = IClaimsIdentityDataBuilderStatus.Complete;
            return;
        }

        logger.LogWarning("No identity found for userEntityId {Sub}", sub);
        claimsIdentityDataBuilder.Status = IClaimsIdentityDataBuilderStatus.Empty;
    }

    private async ValueTask<Guid> GetCompanyId()
    {
        if (claimsIdentityDataBuilder.Status == IClaimsIdentityDataBuilderStatus.Initialized)
        {
            claimsIdentityDataBuilder.AddCompanyId(await _identityRepository.GetActiveCompanyIdByIdentityId(claimsIdentityDataBuilder.IdentityId).ConfigureAwait(ConfigureAwaitOptions.None));
            claimsIdentityDataBuilder.Status = IClaimsIdentityDataBuilderStatus.Complete;
        }
        return claimsIdentityDataBuilder.CompanyId;
    }
}
