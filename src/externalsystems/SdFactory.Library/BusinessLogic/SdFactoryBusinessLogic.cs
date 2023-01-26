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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;

public class SdFactoryBusinessLogic : ISdFactoryBusinessLogic
{
    private readonly ISdFactoryService _sdFactoryService;
    private readonly IPortalRepositories _portalRepositories;

    public SdFactoryBusinessLogic(ISdFactoryService sdFactoryService, IPortalRepositories portalRepositories)
    {
        _sdFactoryService = sdFactoryService;
        _portalRepositories = portalRepositories;
    }

    /// <inheritdoc />
    public Task<Guid> RegisterConnectorAsync(
        string connectorUrl,
        string businessPartnerNumber,
        CancellationToken cancellationToken) =>
        _sdFactoryService.RegisterConnectorAsync(connectorUrl, businessPartnerNumber, cancellationToken);

    /// <inheritdoc />
    public async Task RegisterSelfDescriptionAsync(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var result = await _portalRepositories.GetInstance<IApplicationRepository>()
            .GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(applicationId)
            .ConfigureAwait(false);
        if (result == default)
        {
            throw new ConflictException($"CompanyApplication {applicationId} is not in status SUBMITTED");
        }
        var (companyId, businessPartnerNumber, countryCode, uniqueIdentifiers) = result;

        if (string.IsNullOrWhiteSpace(businessPartnerNumber))
        {
            throw new ConflictException($"BusinessPartnerNumber (bpn) for CompanyApplications {applicationId} company {companyId} is empty");
        }

        Guid? documentId = await _sdFactoryService.RegisterSelfDescriptionAsync(uniqueIdentifiers, countryCode, businessPartnerNumber, cancellationToken).ConfigureAwait(false);
        _portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(companyId, null, c =>
        {
            c.SelfDescriptionDocumentId = documentId;
        });
    }
}
