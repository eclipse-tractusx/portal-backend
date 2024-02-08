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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public interface ICompanyDataBusinessLogic
{
    Task<CompanyAddressDetailData> GetCompanyDetailsAsync();

    IAsyncEnumerable<CompanyAssignedUseCaseData> GetCompanyAssigendUseCaseDetailsAsync();

    Task<bool> CreateCompanyAssignedUseCaseDetailsAsync(Guid useCaseId);

    Task RemoveCompanyAssignedUseCaseDetailsAsync(Guid useCaseId);

    IAsyncEnumerable<CompanyRoleConsentViewData> GetCompanyRoleAndConsentAgreementDetailsAsync(string? languageShortName);

    Task CreateCompanyRoleAndConsentAgreementDetailsAsync(IEnumerable<CompanyRoleConsentDetails> companyRoleConsentDetails);

    Task<IEnumerable<UseCaseParticipationData>> GetUseCaseParticipationAsync(string? language);

    Task<IEnumerable<SsiCertificateData>> GetSsiCertificatesAsync();

    Task CreateUseCaseParticipation(UseCaseParticipationCreationData data, CancellationToken cancellationToken);
    Task CreateSsiCertificate(SsiCertificateCreationData data, CancellationToken cancellationToken);

    Task<Pagination.Response<CredentialDetailData>> GetCredentials(int page, int size, CompanySsiDetailStatusId? companySsiDetailStatusId, VerifiedCredentialTypeId? credentialTypeId, string? companyName, CompanySsiDetailSorting? sorting);

    Task ApproveCredential(Guid credentialId, CancellationToken cancellationToken);

    Task RejectCredential(Guid credentialId);

    IAsyncEnumerable<VerifiedCredentialTypeId> GetCertificateTypes();

    // Task<IEnumerable<CompanyCertificateBpnData>> GetCompanyCertificatesBpnOthers(string businessPartnerNumber);
    Task CreateCompanyCertificate(CompanyCertificateCreationData data, CancellationToken cancellationToken);

    Task<Pagination.Response<CompanyCertificateData>> GetAllCompanyCertificatesAsync(int page, int size, CertificateSorting? sorting, CompanyCertificateStatusId? certificateStatus, CompanyCertificateTypeId? certificateType);
}
