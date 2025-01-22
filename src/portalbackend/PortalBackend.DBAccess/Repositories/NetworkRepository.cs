/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
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

    public NetworkRegistration CreateNetworkRegistration(string externalId, Guid companyId, Guid processId, Guid ospId, Guid applicationId) =>
        _context.NetworkRegistrations.Add(new NetworkRegistration(Guid.NewGuid(), externalId, companyId, processId, ospId, applicationId, DateTimeOffset.UtcNow)).Entity;

    public Task<bool> CheckExternalIdExists(string externalId, Guid onboardingServiceProviderId) =>
        _context.NetworkRegistrations
            .AnyAsync(x =>
                x.OnboardingServiceProviderId == onboardingServiceProviderId &&
                x.ExternalId == externalId);

    /// <inheritdoc />
    public Task<Guid> GetNetworkRegistrationDataForProcessIdAsync(Guid processId) =>
        _context.NetworkRegistrations
            .AsNoTracking()
            .Where(nr => nr.ProcessId == processId)
            .Select(nr => nr.Id)
            .SingleOrDefaultAsync();

    public Task<(bool RegistrationIdExists, VerifyProcessData<ProcessTypeId, ProcessStepTypeId> processData)> IsValidRegistration(string externalId, IEnumerable<ProcessStepTypeId> processStepTypeIds) =>
        _context.NetworkRegistrations
            .Where(x => x.ExternalId == externalId)
            .Select(x => new ValueTuple<bool, VerifyProcessData<ProcessTypeId, ProcessStepTypeId>>(
                    true,
                    new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(
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
                        assigned.CompanyRole!.AgreementAssignedCompanyRoles.Where(a => a.Agreement!.AgreementStatusId == AgreementStatusId.ACTIVE).Select(a => a.AgreementId))),
                x.NetworkRegistration!.ProcessId
                ))
            .SingleOrDefaultAsync();

    public Task<(OspDetails? OspDetails, string ExternalId, string? Bpn, Guid ApplicationId, IEnumerable<string> Comments)> GetCallbackData(Guid networkRegistrationId, ProcessStepTypeId processStepTypeId) =>
        _context.NetworkRegistrations
            .Where(x => x.Id == networkRegistrationId)
            .Select(x => new ValueTuple<OspDetails?, string, string?, Guid, IEnumerable<string>>(
                x.OnboardingServiceProvider!.OnboardingServiceProviderDetail == null
                    ? null
                    : new OspDetails(
                        x.OnboardingServiceProvider.OnboardingServiceProviderDetail.CallbackUrl,
                        x.OnboardingServiceProvider.OnboardingServiceProviderDetail.AuthUrl,
                        x.OnboardingServiceProvider.OnboardingServiceProviderDetail.ClientId,
                        x.OnboardingServiceProvider.OnboardingServiceProviderDetail.ClientSecret,
                        x.OnboardingServiceProvider.OnboardingServiceProviderDetail.InitializationVector,
                        x.OnboardingServiceProvider.OnboardingServiceProviderDetail.EncryptionMode),
                x.ExternalId,
                x.OnboardingServiceProvider.BusinessPartnerNumber,
                x.ApplicationId,
                processStepTypeId == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED
                    ? x.Process!.ProcessSteps
                        .Where(p =>
                            p.ProcessStepTypeId == ProcessStepTypeId.MANUAL_VERIFY_REGISTRATION &&
                            p.ProcessStepStatusId == ProcessStepStatusId.FAILED &&
                            p.Message != null)
                        .Select(step => step.Message!)
                    : new List<string>()))
            .SingleOrDefaultAsync();

    public Task<string?> GetOspCompanyName(Guid networkRegistrationId) =>
        _context.NetworkRegistrations.Where(x => x.Id == networkRegistrationId)
            .Select(x => x.OnboardingServiceProvider!.Name)
            .SingleOrDefaultAsync();

    public Task<(
       bool Exists,
       bool IsValidTypeId,
       bool IsValidStatusId,
       bool IsValidCompany,
       (
           (CompanyStatusId CompanyStatusId, IEnumerable<(Guid IdentityId, UserStatusId UserStatus)> Identities) CompanyData,
           IEnumerable<(Guid InvitationId, InvitationStatusId StatusId)> InvitationData,
           VerifyProcessData<ProcessTypeId, ProcessStepTypeId> ProcessData
       )? Data)> GetDeclineDataForApplicationId(Guid applicationId, CompanyApplicationTypeId validTypeId, IEnumerable<CompanyApplicationStatusId> validStatusIds, Guid companyId) =>
   _context.NetworkRegistrations
       .AsSplitQuery()
       .Where(registration => registration.CompanyApplication!.Id == applicationId)
       .Select(registration => new
       {
           IsValidType = validTypeId == registration.CompanyApplication!.CompanyApplicationTypeId,
           IsValidStatus = validStatusIds.Contains(registration.CompanyApplication.ApplicationStatusId),
           IsValidCompany = registration.CompanyId == companyId,
           Invitations = registration.CompanyApplication.Invitations,
           Company = registration.Company,
           Process = registration.Process
       })
       .Select(x => new ValueTuple<
               bool,
               bool,
               bool,
               bool,
               ValueTuple<
                   ValueTuple<CompanyStatusId, IEnumerable<ValueTuple<Guid, UserStatusId>>>,
                   IEnumerable<ValueTuple<Guid, InvitationStatusId>>,
                   VerifyProcessData<ProcessTypeId, ProcessStepTypeId>
               >?>(
           true,
           x.IsValidType,
           x.IsValidStatus,
           x.IsValidCompany,
           x.IsValidType && x.IsValidStatus && x.IsValidCompany
               ? new ValueTuple<
                   ValueTuple<CompanyStatusId, IEnumerable<ValueTuple<Guid, UserStatusId>>>,
                   IEnumerable<ValueTuple<Guid, InvitationStatusId>>,
                   VerifyProcessData<ProcessTypeId, ProcessStepTypeId>>(
                       new ValueTuple<CompanyStatusId, IEnumerable<ValueTuple<Guid, UserStatusId>>>(
                           x.Company!.CompanyStatusId,
                           x.Company.Identities.Select(i => new ValueTuple<Guid, UserStatusId>(i.Id, i.UserStatusId))),
                       x.Invitations.Select(i => new ValueTuple<Guid, InvitationStatusId>(
                           i.Id,
                           i.InvitationStatusId)),
                       new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(
                           x.Process,
                           x.Process!.ProcessSteps.Where(ps => ps.ProcessStepStatusId == ProcessStepStatusId.TODO)))
               : null
       ))
       .SingleOrDefaultAsync();
}
