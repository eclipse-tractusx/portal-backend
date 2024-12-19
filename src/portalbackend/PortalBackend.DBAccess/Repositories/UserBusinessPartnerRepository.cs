/********************************************************************************
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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class UserBusinessPartnerRepository(PortalDbContext dbContext)
    : IUserBusinessPartnerRepository
{
    public CompanyUserAssignedBusinessPartner CreateCompanyUserAssignedBusinessPartner(Guid companyUserId, string businessPartnerNumber) =>
        dbContext.CompanyUserAssignedBusinessPartners.Add(
            new CompanyUserAssignedBusinessPartner(
                companyUserId,
                businessPartnerNumber
            )).Entity;

    public void CreateCompanyUserAssignedBusinessPartners(IEnumerable<(Guid CompanyUserId, string BusinessPartnerNumber, Action<CompanyUserAssignedBusinessPartner> SetOptional)> companyUserIdBpns) =>
        dbContext.CompanyUserAssignedBusinessPartners.AddRange(companyUserIdBpns.Select(x =>
        {
            var ub = new CompanyUserAssignedBusinessPartner(x.CompanyUserId, x.BusinessPartnerNumber);
            x.SetOptional(ub);
            return ub;
        }));

    public void AttachAndModifyCompanyUserAssignedBusinessPartner(Guid companyUserId, string businessPartnerNumber, Action<CompanyUserAssignedBusinessPartner> modify)
    {
        var companyUserAssignedBusinessPartner = new CompanyUserAssignedBusinessPartner(companyUserId, businessPartnerNumber);
        dbContext.CompanyUserAssignedBusinessPartners.Attach(companyUserAssignedBusinessPartner);
        modify(companyUserAssignedBusinessPartner);
    }

    public Task<(Guid UserId, string? Bpn)> GetForProcessIdAsync(Guid processId) =>
        dbContext.CompanyUserAssignedBusinessPartners
            .Where(x => x.ProcessId == processId)
            .Select(x => new ValueTuple<Guid, string?>(x.CompanyUserId, x.BusinessPartnerNumber))
            .SingleOrDefaultAsync();

    public CompanyUserAssignedBusinessPartner DeleteCompanyUserAssignedBusinessPartner(Guid companyUserId, string businessPartnerNumber) =>
        dbContext.Remove(
            new CompanyUserAssignedBusinessPartner(
                companyUserId,
                businessPartnerNumber
            )).Entity;

    public void DeleteCompanyUserAssignedBusinessPartners(IEnumerable<(Guid CompanyUserId, string BusinessPartnerNumber)> companyUserAssignedBusinessPartnerIds) =>
        dbContext.RemoveRange(companyUserAssignedBusinessPartnerIds.Select(ids => new CompanyUserAssignedBusinessPartner(ids.CompanyUserId, ids.BusinessPartnerNumber)));

    public Task<(bool IsValidUser, bool IsAssignedBusinessPartner, bool IsSameCompany)> GetOwnCompanyUserWithAssignedBusinessPartnerNumbersAsync(Guid companyUserId, Guid userCompanyId, string businessPartnerNumber) =>
        dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new ValueTuple<bool, bool, bool>(
                true,
                companyUser.CompanyUserAssignedBusinessPartners!.Any(assignedPartner => assignedPartner.BusinessPartnerNumber == businessPartnerNumber),
                companyUser.Identity!.CompanyId == userCompanyId
            ))
            .SingleOrDefaultAsync();
}
