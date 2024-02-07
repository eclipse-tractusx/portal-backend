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
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class CompanyCertificateRepository : ICompanyCertificateRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">Portal DB context.</param>
    public CompanyCertificateRepository(PortalDbContext portalDbContext)
    {
        _context = portalDbContext;
    }

    public Task<bool> CheckCompanyCertificateType(CompanyCertificateTypeId certificateTypeId) =>
     _context.CompanyCertificateTypes
            .AnyAsync(x =>
                x.Id == certificateTypeId);

    public Task<bool> CheckCompanyCertificateId(Guid id) =>
    _context.CompanyCertificates
            .AnyAsync(x =>
                x.Id == id);

    /// <inheritdoc />
    public CompanyCertificate CreateCompanyCertificateData(Guid companyId, CompanyCertificateTypeId companyCertificateTypeId, Guid docId, DateTimeOffset? expiryDate, Action<CompanyCertificate>? setOptionalFields)
    {
        var companyCertificate = new CompanyCertificate(Guid.NewGuid(), DateTimeOffset.UtcNow, companyCertificateTypeId, CompanyCertificateStatusId.ACTIVE, companyId, docId, expiryDate);
        setOptionalFields?.Invoke(companyCertificate);
        return _context.CompanyCertificates.Add(companyCertificate).Entity;
    }

}