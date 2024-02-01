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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public interface ICompanyCertificateRepository
{
    /// <summary>
    /// Checks whether the given CertificateType is a <see cref="CompanyCertificateTypeId"/> Certificate
    /// </summary>
    /// <param name="CertificateTypeId">Id of the credentialTypeId</param>
    /// <returns><c>true</c> if the tpye is a certificate, otherwise <c>false</c></returns>
    Task<bool> CheckSsiCertificateType(CompanyCertificateTypeId CertificateTypeId);

    /// <summary>
    /// Creates the company certificate data
    /// </summary>
    /// <param name="companyId">Id of the company</param>
    /// <param name="companyCertificateTypeId">Id of the company certificate types</param>
    /// <param name="docId">id of the document</param>
    /// <param name="expiryDate">expiry date</param>
    /// <param name="companyCertificateStatusId">company certificate status id</param>   
    /// <returns>The created entity</returns>
    CompanyCertificate CreateCompanyCertificate(Guid companyId, CompanyCertificateTypeId companyCertificateTypeId, Guid docId, DateTimeOffset expiryDate);
}