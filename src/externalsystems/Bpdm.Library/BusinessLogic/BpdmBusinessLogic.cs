/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.BusinessLogic;

public class BpdmBusinessLogic : IBpdmBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IBpdmService _bpdmService;

    public BpdmBusinessLogic(IPortalRepositories portalRepositories, IBpdmService bpdmService)
    {
        _portalRepositories = portalRepositories;
        _bpdmService = bpdmService;
    }

    public async Task<bool> TriggerBpnDataPush(Guid applicationId, string iamUserId, CancellationToken cancellationToken)
    {
        var data = await _portalRepositories.GetInstance<ICompanyRepository>().GetBpdmDataForApplicationAsync(iamUserId, applicationId).ConfigureAwait(false);
        if (data is null)
        {
            throw new NotFoundException($"Application {applicationId} does not exists.");
        }

        if (!data.IsUserInCompany)
        {
            throw new ForbiddenException($"User is not allowed to trigger Bpn Data Push for the application {applicationId}");
        }
        
        if (data.ApplicationStatusId != CompanyApplicationStatusId.SUBMITTED)
        {
            throw new ArgumentException($"CompanyApplication {applicationId} is not in status SUBMITTED", nameof(applicationId));
        }

        if (string.IsNullOrWhiteSpace(data.ZipCode))
        {
            throw new ConflictException("ZipCode must not be empty");
        }

        var bpdmTransferData = new BpdmTransferData(data.CompanyName, data.AlphaCode2, data.ZipCode, data.City, data.Street);
        return await _bpdmService.TriggerBpnDataPush(bpdmTransferData, cancellationToken).ConfigureAwait(false);
    }
}
