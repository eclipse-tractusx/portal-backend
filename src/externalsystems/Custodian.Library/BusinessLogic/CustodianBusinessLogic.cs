/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Custodian.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.BusinessLogic;

public class CustodianBusinessLogic : ICustodianBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly ICustodianService _custodianService;

    public CustodianBusinessLogic(IPortalRepositories portalRepositories, ICustodianService custodianService)
    {
        _portalRepositories = portalRepositories;
        _custodianService = custodianService;
    }
    
    /// <inheritdoc />
    public async Task<string> CreateWalletAsync(Guid applicationId, CancellationToken cancellationToken)
    {
        var result = await _portalRepositories.GetInstance<IApplicationRepository>().GetCompanyAndApplicationDetailsForCreateWalletAsync(applicationId).ConfigureAwait(false);
        if (result == default)
        {
            throw new ConflictException($"CompanyApplication {applicationId} is not in status SUBMITTED");
        }
        var (companyId, companyName, businessPartnerNumber) = result;

        if (string.IsNullOrWhiteSpace(businessPartnerNumber))
        {
            throw new ConflictException($"BusinessPartnerNumber (bpn) for CompanyApplications {applicationId} company {companyId} is empty");
        }
        
        return await _custodianService.CreateWalletAsync(businessPartnerNumber, companyName, cancellationToken).ConfigureAwait(false);
    }
    
    /// <inheritdoc />
    public async Task<WalletData?> GetWalletByBpnAsync(Guid applicationId, CancellationToken cancellationToken)
    {
        var bpn = await _portalRepositories.GetInstance<IApplicationRepository>()
            .GetBpnForApplicationIdAsync(applicationId).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(bpn))
        {
            throw new ConflictException("BusinessPartnerNumber is not set");
        }

        var walletData = await _custodianService.GetWalletByBpnAsync(bpn, cancellationToken)
            .ConfigureAwait(false);
        
        return walletData;
    }
}
