/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;

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

    public CompanyUserAssignedBusinessPartner RemoveCompanyUserAssignedBusinessPartner(CompanyUserAssignedBusinessPartner companyUserAssignedBusinessPartner) =>
        _dbContext.Remove(companyUserAssignedBusinessPartner).Entity;

    public Task<(string? UserEntityId, CompanyUserAssignedBusinessPartner? AssignedBusinessPartner, bool IsValidUser)> GetOwnCompanyUserWithAssignedBusinessPartnerNumbersAsync(Guid companyUserId,string adminUserId, string businessPartnerNumber) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => ((string? UserEntityId, CompanyUserAssignedBusinessPartner? AssignedBusinessPartner, bool IsValidUser)) new (
                companyUser.IamUser!.UserEntityId,
                companyUser.CompanyUserAssignedBusinessPartners!.SingleOrDefault(assignedPartner => assignedPartner.BusinessPartnerNumber == businessPartnerNumber),
                companyUser.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId)
            ))
            .SingleOrDefaultAsync();
}
