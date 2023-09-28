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

    public Task<(bool Exists, IEnumerable<(Guid CompanyApplicationId, CompanyApplicationStatusId CompanyApplicationStatusId, string? CallbackUrl)> CompanyApplications, IEnumerable<(CompanyRoleId CompanyRoleId, IEnumerable<Guid> AgreementIds)> CompanyRoleAgreementIds, Guid? ProcessId)> GetSubmitData(Guid companyId) =>
        _context.Companies
            .AsSplitQuery()
            .Where(x => x.Id == companyId)
            .Select(x => new ValueTuple<bool, IEnumerable<(Guid, CompanyApplicationStatusId, string?)>, IEnumerable<(CompanyRoleId, IEnumerable<Guid>)>, Guid?>(
                true,
                x.CompanyApplications
                    .Where(ca => ca.CompanyApplicationTypeId == CompanyApplicationTypeId.EXTERNAL)
                    .Select(ca => new ValueTuple<Guid, CompanyApplicationStatusId, string?>(
                        ca.Id,
                        ca.ApplicationStatusId,
                        ca.OnboardingServiceProvider!.OnboardingServiceProviderDetail!.CallbackUrl)),
                x.CompanyAssignedRoles.Select(assigned => new ValueTuple<CompanyRoleId, IEnumerable<Guid>>(
                        assigned.CompanyRoleId,
                        assigned.CompanyRole!.AgreementAssignedCompanyRoles.Select(a => a.AgreementId))),
                x.NetworkRegistration!.ProcessId
                ))
            .SingleOrDefaultAsync();

    public Task<(OspDetails? OspDetails, Guid? ExternalId, string? Bpn, Guid ApplicationId, IEnumerable<string> Comments)> GetCallbackData(Guid networkRegistrationId, ProcessStepTypeId processStepTypeId) =>
        _context.NetworkRegistrations
            .Where(x => x.Id == networkRegistrationId)
            .Select(x => new ValueTuple<OspDetails?, Guid?, string?, Guid, IEnumerable<string>>(
                x.OnboardingServiceProvider!.OnboardingServiceProviderDetail == null
                    ? null
                    : new OspDetails(
                        x.OnboardingServiceProvider.OnboardingServiceProviderDetail.CallbackUrl,
                        x.OnboardingServiceProvider.OnboardingServiceProviderDetail.AuthUrl,
                        x.OnboardingServiceProvider.OnboardingServiceProviderDetail.ClientId,
                        x.OnboardingServiceProvider.OnboardingServiceProviderDetail.ClientSecret),
                x.ExternalId,
                x.OnboardingServiceProvider.BusinessPartnerNumber,
                x.ApplicationId,
                processStepTypeId == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED
                    ? x.Process!.ProcessSteps
                        .Where(p =>
                            p.ProcessStepTypeId == ProcessStepTypeId.VERIFY_REGISTRATION &&
                            p.ProcessStepStatusId == ProcessStepStatusId.FAILED &&
                            p.Message != null)
                        .Select(step => step.Message!)
                    : new List<string>()))
            .SingleOrDefaultAsync();
}
