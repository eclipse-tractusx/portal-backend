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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class CompanyDataBusinessLogic : ICompanyDataBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="portalRepositories"></param>
    public CompanyDataBusinessLogic(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    /// <inheritdoc/>
    public async Task<CompanyAddressDetailData> GetOwnCompanyDetailsAsync(string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<ICompanyRepository>().GetOwnCompanyDetailsAsync(iamUserId).ConfigureAwait(false);
        if (result == null)
        {
            throw new ConflictException($"user {iamUserId} is not associated with any company");
        }
        return result;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<CompanyRoleConsentData> GetCompanyRoleAndConsentAgreementDetailsAsync(string iamUserId)
    {
        var result =  _portalRepositories.GetInstance<ICompanyRepository>().GetCompanyRoleAndConsentAgreementDetailsAsync(iamUserId);
        if (!await result.AnyAsync())
        {
            throw new ConflictException($"user {iamUserId} is not associated with any company or Incorrect Status");
        }
        await foreach(var data in result)
        {
            yield return data;
        }
    }

    /// <inheritdoc/>
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync(string iamUserId,IEnumerable<CompanyRoleConsentDetails> companyRoleConsentDetails)
    {
        var companyRole = await _portalRepositories.GetInstance<ICompanyRepository>().GetCompanyRolesDataAsync(iamUserId).ConfigureAwait(false);
        if(companyRole == default)
        {
            throw new NotFoundException($"user {iamUserId} is not associated with any company");
        }
        if(!companyRole.isCompanyActive)
        {
            throw new ConflictException("Company Status is Incorrect");
        }
        foreach(var data in  companyRoleConsentDetails)
        {
            if(!companyRole.agreementAssignedRole.Contains(data.companyRoles) || data.agreements.Any(x => x.consentStatus != ConsentStatusId.ACTIVE))
            {
                throw new ConflictException("All agreement need to get signed");
            }
            if(companyRole.companyRoleId.Contains(data.companyRoles))
            {
                throw new ConflictException("companyRole already exists");   
            }
            _portalRepositories.GetInstance<ICompanyRolesRepository>().CreateCompanyAssignedRole(companyRole.companyId, data.companyRoles);
            foreach(var agreementData in data.agreements)
            {
                _portalRepositories.GetInstance<IConsentRepository>().CreateConsent(agreementData.agreementId, companyRole.companyId, companyRole.companyUserId, agreementData.consentStatus);
            }
        }
        await _portalRepositories.SaveAsync();
    }
}
