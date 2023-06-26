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
    public IAsyncEnumerable<UseCaseParticipationTransferData> GetUseCaseParticipationForCompany(Guid companyId, string language) =>
        _context.VerifiedCredentialTypes
            .Where(x => x.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId == VerifiedCredentialTypeKindId.USE_CASE)
            .Select(x => new
            {
                UseCase = x.VerifiedCredentialTypeAssignedUseCase!.UseCase,
                TypeId = x.Id,
                ExternalTypeDetails = x.VerifiedCredentialTypeAssignedExternalType!.VerifiedCredentialExternalType!.VerifiedCredentialExternalTypeUseCaseDetailVersions,
            })
            .Select(x => new UseCaseParticipationTransferData(
                x.UseCase!.Name,
                x.UseCase.UseCaseDescriptions
                    .Where(ucd => ucd.LanguageShortName == language).Select(ucd => ucd.Description).SingleOrDefault(),
                x.TypeId,
                x.ExternalTypeDetails.Select(y => new
                {
                    Version = y,
                    Detail = y.CompanySsiDetails.Where(ssi => ssi.CompanyId == companyId && ssi.VerifiedCredentialTypeId == x.TypeId && ssi.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE && ssi.VerifiedCredentialExternalTypeUseCaseDetailId == y.Id).Take(2)
                })
                    .Select(y =>
                    new CompanySsiExternalTypeDetailTransferData(
                        new ExternalTypeDetailData(
                            y.Version.Id,
                            y.Version.VerifiedCredentialExternalTypeId,
                            y.Version.Version,
                            y.Version.Template,
                            y.Version.ValidFrom,
                            y.Version.Expiry),
                        y.Detail.Select(d =>
                            new CompanySsiDetailTransferData(
                                d.Id,
                                d.CompanySsiDetailStatusId,
                                d.ExpiryDate,
                                new DocumentData(
                                    d.Document!.Id,
                                    d.Document!.DocumentName))
                            )
                    ))
            ))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<SsiCertificateTransferData> GetSsiCertificates(Guid companyId) =>
        _context.VerifiedCredentialTypes
            .Where(x => x.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId == VerifiedCredentialTypeKindId.CERTIFICATE)
            .Select(x => new
            {
                TypeId = x.Id,
                SsiDetails = x.CompanySsiDetails.Where(y => y.VerifiedCredentialTypeId == x.Id && y.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE && y.CompanyId == companyId).Take(2)
            })
            .Select(x => new SsiCertificateTransferData(
                x.TypeId,
                x.SsiDetails.Select(d =>
                    new CompanySsiDetailTransferData(
                       d.Id,
                        d.CompanySsiDetailStatusId,
                        d.ExpiryDate,
                        d.Document == null ?
                            null :
                            new DocumentData(d.Document!.Id, d.Document.DocumentName)
                    )
            )))
            .ToAsyncEnumerable();
}
