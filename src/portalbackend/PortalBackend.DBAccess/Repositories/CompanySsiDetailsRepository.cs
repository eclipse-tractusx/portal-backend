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
            .Where(types => types.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId == VerifiedCredentialTypeKindId.USE_CASE)
            .Select(types => new
            {
                UseCase = types.VerifiedCredentialTypeAssignedUseCase!.UseCase,
                TypeId = types.Id,
                ExternalTypeDetails = types.VerifiedCredentialTypeAssignedExternalType!.VerifiedCredentialExternalType!.VerifiedCredentialExternalTypeUseCaseDetailVersions,
            })
            .Select(x => new UseCaseParticipationTransferData(
                x.UseCase!.Name,
                x.UseCase.UseCaseDescriptions
                    .Where(ucd => ucd.LanguageShortName == language)
                    .Select(ucd => ucd.Description)
                    .SingleOrDefault(),
                x.TypeId,
                x.ExternalTypeDetails
                    .Select(external =>
                        new CompanySsiExternalTypeDetailTransferData(
                            new ExternalTypeDetailData(
                                external.Id,
                                external.VerifiedCredentialExternalTypeId,
                                external.Version,
                                external.Template,
                                external.ValidFrom,
                                external.Expiry),
                            external.CompanySsiDetails
                                .Where(ssi =>
                                    ssi.CompanyId == companyId &&
                                    ssi.VerifiedCredentialTypeId == x.TypeId &&
                                    ssi.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE &&
                                    ssi.VerifiedCredentialExternalTypeUseCaseDetailId == external.Id)
                                .Select(ssi =>
                                    new CompanySsiDetailTransferData(
                                        ssi.Id,
                                        ssi.CompanySsiDetailStatusId,
                                        ssi.ExpiryDate,
                                        ssi.Document == null
                                            ? null
                                            : new DocumentData(
                                                ssi.Document!.Id,
                                                ssi.Document.DocumentName)))
                                .Take(2)
                        ))
            ))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<SsiCertificateTransferData> GetSsiCertificates(Guid companyId) =>
        _context.VerifiedCredentialTypes
            .Where(types => types.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId == VerifiedCredentialTypeKindId.CERTIFICATE)
            .Select(types => new SsiCertificateTransferData(
                types.Id,
                types.CompanySsiDetails
                    .Where(ssi =>
                        ssi.VerifiedCredentialTypeId == types.Id &&
                        ssi.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE &&
                        ssi.CompanyId == companyId)
                    .Select(ssi =>
                        new CompanySsiDetailTransferData(
                            ssi.Id,
                            ssi.CompanySsiDetailStatusId,
                            ssi.ExpiryDate,
                            ssi.Document == null
                                ? null
                                : new DocumentData(
                                    ssi.Document!.Id,
                                    ssi.Document.DocumentName)))
                    .Take(2)
             ))
            .ToAsyncEnumerable();
}
