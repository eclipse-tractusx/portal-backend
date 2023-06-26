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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class CompanyCredentialDetailsRepository : ICompanyCredentialDetailsRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">Portal DB context.</param>
    public CompanyCredentialDetailsRepository(PortalDbContext portalDbContext)
    {
        _context = portalDbContext;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<UseCaseParticipation> GetUseCaseParticipationForCompany(Guid companyId, string language) =>
        _context.CompanyCredentialDetails
            .Where(x =>
                x.UseCaseParticipationStatusId != UseCaseParticipationStatusId.INACTIVE &&
                x.CompanyId == companyId &&
                x.CredentialType!.CredentialTypeAssignedKind!.CredentialTypeKindId == CredentialTypeKindId.USE_CASE)
            .Select(x => new UseCaseParticipation(
                    x.Id,
                    x.CredentialTypeId,
                    x.CredentialAssignedUseCase!.UseCase!.Name,
                    x.CredentialAssignedUseCase!.UseCase.UseCaseDescriptions.Where(ucd => ucd.LanguageShortName == language).Select(ucd => ucd.Description).SingleOrDefault(),
                    x.UseCaseParticipationStatusId,
                    x.ExpiryDate,
                    new DocumentData(x.Document!.Id, x.Document!.DocumentName)
                ))
            .ToAsyncEnumerable();
}
