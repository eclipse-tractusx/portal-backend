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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class UserBusinessPartnerRepository : IUserBusinessPartnerRepository
{
    private readonly PortalDbContext _dbContext;

    public UserBusinessPartnerRepository(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public CompanyUserAssignedBusinessPartner CreateCompanyUserAssignedBusinessPartner(Guid companyUserId, string businessPartnerNumber)
    {
        if (businessPartnerNumber.Length > 20)
        {
            throw new ArgumentException($"{nameof(businessPartnerNumber)} {businessPartnerNumber} exceeds maximum length of 20 characters", nameof(businessPartnerNumber));
        }
        return _dbContext.CompanyUserAssignedBusinessPartners.Add(
            new CompanyUserAssignedBusinessPartner(
                companyUserId,
                businessPartnerNumber
            )).Entity;
    }

    public CompanyUserAssignedBusinessPartner DeleteCompanyUserAssignedBusinessPartner(Guid companyUserId, string businessPartnerNumber) =>
        _dbContext.Remove(
            new CompanyUserAssignedBusinessPartner(
                companyUserId,
                businessPartnerNumber
            )).Entity;

    public void DeleteCompanyUserAssignedBusinessPartners(IEnumerable<(Guid CompanyUserId, string BusinessPartnerNumber)> companyUserAssignedBusinessPartnerIds) =>
        _dbContext.RemoveRange(companyUserAssignedBusinessPartnerIds.Select(ids => new CompanyUserAssignedBusinessPartner(ids.CompanyUserId, ids.BusinessPartnerNumber)));

    public Task<(string? UserEntityId, bool IsAssignedBusinessPartner, bool IsValidUser)> GetOwnCompanyUserWithAssignedBusinessPartnerNumbersAsync(Guid companyUserId, Guid userCompanyId, string businessPartnerNumber) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new ValueTuple<string?, bool, bool>(
                companyUser.UserEntityId,
                companyUser.CompanyUserAssignedBusinessPartners!.Any(assignedPartner => assignedPartner.BusinessPartnerNumber == businessPartnerNumber),
                companyUser.CompanyId == userCompanyId
            ))
            .SingleOrDefaultAsync();
}
