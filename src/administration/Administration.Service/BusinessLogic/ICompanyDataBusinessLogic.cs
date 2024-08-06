/********************************************************************************
 * Copyright (c) 2022 BMW Group AG
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
    Task<Guid> CreateUseCaseParticipation(UseCaseParticipationCreationData data, string token, CancellationToken cancellationToken);

    IAsyncEnumerable<CompanyCertificateBpnData> GetCompanyCertificatesByBpn(string businessPartnerNumber);

    Task CreateCompanyCertificate(CompanyCertificateCreationData data, CancellationToken cancellationToken);

    Task<(string FileName, byte[] Content, string MediaType)> GetCompanyCertificateDocumentByCompanyIdAsync(Guid documentId);

    Task<(string FileName, byte[] Content, string MediaType)> GetCompanyCertificateDocumentAsync(Guid documentId);

    Task<int> DeleteCompanyCertificateAsync(Guid documentId);

    Task<Pagination.Response<CompanyCertificateData>> GetAllCompanyCertificatesAsync(int page, int size, CertificateSorting? sorting, CompanyCertificateStatusId? certificateStatus, CompanyCertificateTypeId? certificateType);
    Task<DimUrlsResponse> GetDimServiceUrls();
    Task<Pagination.Response<CompanyMissingSdDocumentData>> GetCompaniesWithMissingSdDocument(int page, int size);
    Task TriggerSelfDescriptionCreation();
}
