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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class CompanySsiDetailsRepository : ICompanySsiDetailsRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">Portal DB context.</param>
    public CompanySsiDetailsRepository(PortalDbContext portalDbContext)
    {
        _context = portalDbContext;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<UseCaseParticipationData> GetUseCaseParticipationForCompany(Guid companyId, string language) =>
        _context.VerifiedCredentialTypes
            .Where(x => x.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId == VerifiedCredentialTypeKindId.USE_CASE)
            .AsSplitQuery()
            .Select(x => new
            {
                UseCase = x.VerifiedCredentialTypeAssignedUseCase!.UseCase,
                TypeId = x.Id,
                ExternalTypeDetails = x.VerifiedCredentialExternalType!.VerifiedCredentialExternalTypeUseCaseDetails,
            })
            .Select(x => new UseCaseParticipationData(
                x.UseCase!.Name,
                x.UseCase.UseCaseDescriptions
                    .Where(ucd => ucd.LanguageShortName == language).Select(ucd => ucd.Description).SingleOrDefault(),
                x.TypeId,
                x.ExternalTypeDetails.Select(y =>
                    new CompanySsiExternalTypeDetailData(
                        new ExternalTypeDetailData(
                            y.Id,
                            y.VerifiedCredentialExternalTypeId,
                            y.Version,
                            y.Template,
                            y.ValidFrom,
                            y.Expiry),
                        y.CompanySsiDetails.SingleOrDefault(ssi => ssi.CompanyId == companyId && ssi.VerifiedCredentialTypeId == x.TypeId && ssi.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE) == null ?
                            null :
                            new CompanySsiDetailData(
                                y.CompanySsiDetails.Single(ssi => ssi.CompanyId == companyId && ssi.VerifiedCredentialTypeId == x.TypeId && ssi.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE).Id,
                                y.CompanySsiDetails.Single(ssi => ssi.CompanyId == companyId && ssi.VerifiedCredentialTypeId == x.TypeId && ssi.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE).CompanySsiDetailStatusId,
                                y.CompanySsiDetails.Single(ssi => ssi.CompanyId == companyId && ssi.VerifiedCredentialTypeId == x.TypeId && ssi.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE).ExpiryDate,
                                new DocumentData(
                                    y.CompanySsiDetails.Single(ssi => ssi.CompanyId == companyId && ssi.VerifiedCredentialTypeId == x.TypeId && ssi.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE).Document!.Id,
                                    y.CompanySsiDetails.Single(ssi => ssi.CompanyId == companyId && ssi.VerifiedCredentialTypeId == x.TypeId && ssi.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE).Document!.DocumentName))
                ))
            ))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<SsiCertificateData> GetSsiCertificates(Guid companyId) =>
        _context.VerifiedCredentialTypes
            .Where(x => x.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId == VerifiedCredentialTypeKindId.CERTIFICATE)
            .AsSplitQuery()
            .Select(x => new
            {
                TypeId = x.Id,
                SsiDetails = x.CompanySsiDetails.SingleOrDefault(y => y.VerifiedCredentialTypeId == x.Id && y.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE && y.CompanyId == companyId)
            })
            .Select(x => new SsiCertificateData(
                x.TypeId,
                x.SsiDetails == null ? null : new CompanySsiDetailData(
                        x.SsiDetails.Id,
                        x.SsiDetails.CompanySsiDetailStatusId,
                        x.SsiDetails.ExpiryDate,
                        x.SsiDetails.Document == null ?
                            null :
                            new DocumentData(x.SsiDetails.Document!.Id, x.SsiDetails.Document.DocumentName)
                    )
            ))
            .ToAsyncEnumerable();
}
