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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class NetworkRepository : INetworkRepository
{
    private readonly PortalDbContext _context;

    public NetworkRepository(PortalDbContext context)
    {
        _context = context;
    }

    public NetworkRegistration CreateNetworkRegistration(Guid externalId, Guid companyId, Guid processId, Guid ospId, Guid applicationId) =>
        _context.NetworkRegistrations.Add(new NetworkRegistration(Guid.NewGuid(), externalId, companyId, processId, ospId, applicationId, DateTimeOffset.UtcNow)).Entity;

    public Task<bool> CheckExternalIdExists(Guid externalId, Guid onboardingServiceProviderId) =>
        _context.NetworkRegistrations
            .AnyAsync(x =>
                x.OnboardingServiceProviderId == onboardingServiceProviderId &&
                x.ExternalId == externalId);

    /// <inheritdoc />
    public Task<Guid> GetNetworkRegistrationDataForProcessIdAsync(Guid processId) =>
        _context.Processes
            .AsNoTracking()
            .Where(process => process.Id == processId)
            .Select(process => process.NetworkRegistration!.Id)
            .SingleOrDefaultAsync();

    public Task<(bool RegistrationIdExists, VerifyProcessData processData)> IsValidRegistration(Guid externalId, IEnumerable<ProcessStepTypeId> processStepTypeIds) =>
        _context.NetworkRegistrations
            .Where(x => x.ExternalId == externalId)
            .Select(x => new ValueTuple<bool, VerifyProcessData>(
                    true,
                    new VerifyProcessData(
                        x.Process,
                        x.Process!.ProcessSteps
                            .Where(step =>
                                processStepTypeIds.Contains(step.ProcessStepTypeId) &&
                                step.ProcessStepStatusId == ProcessStepStatusId.TODO))
                ))
            .SingleOrDefaultAsync();

    public Task<(bool Exists, IEnumerable<(Guid Id, CompanyApplicationStatusId StatusId)> CompanyApplications, bool IsUserInRole, IEnumerable<(CompanyRoleId CompanyRoleId, IEnumerable<Guid> AgreementIds)> CompanyRoleIds, string? CallbackUrl, string? Bpn, Guid? ExternalId)> GetSubmitData(Guid companyId, Guid userId, IEnumerable<Guid> roleIds) =>
        _context.Companies
            .Where(x => x.Id == companyId)
            .Select(x => new ValueTuple<bool, IEnumerable<(Guid, CompanyApplicationStatusId)>, bool, IEnumerable<(CompanyRoleId, IEnumerable<Guid>)>, string?, string?, Guid?>(
                true,
                x.CompanyApplications.Where(ca => ca.CompanyApplicationTypeId == CompanyApplicationTypeId.EXTERNAL).Select(ca => new ValueTuple<Guid, CompanyApplicationStatusId>(ca.Id, ca.ApplicationStatusId)),
                x.Identities.Any(i => i.Id == userId && i.IdentityAssignedRoles.Any(roles => roleIds.Any(r => r == roles.UserRoleId))),
                x.CompanyAssignedRoles.Select(assigned => new ValueTuple<CompanyRoleId, IEnumerable<Guid>>(
                        assigned.CompanyRoleId,
                        assigned.CompanyRole!.AgreementAssignedCompanyRoles.Select(a => a.AgreementId))),
                x.OnboardingServiceProviderDetail!.CallbackUrl,
                x.BusinessPartnerNumber,
                x.NetworkRegistration!.ExternalId
                ))
            .SingleOrDefaultAsync();
}
