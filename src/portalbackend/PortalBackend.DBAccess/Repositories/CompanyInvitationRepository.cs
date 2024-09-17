/********************************************************************************
* Copyright (c) 2024 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class CompanyInvitationRepository(PortalDbContext context)
    : ICompanyInvitationRepository
{
    CompanyInvitation ICompanyInvitationRepository.CreateCompanyInvitation(Guid applicationId, string firstName, string lastName, string email, Guid processId, Action<CompanyInvitation>? setOptionalFields)
    {
        var entity = new CompanyInvitation(Guid.NewGuid(), applicationId, firstName, lastName, email, processId);
        setOptionalFields?.Invoke(entity);
        return context.Add(entity).Entity;
    }

    public Task<Guid> GetCompanyInvitationForProcessId(Guid processId) =>
        context.CompanyInvitations
            .AsNoTracking()
            .Where(i => i.ProcessId == processId)
            .Select(i => i.Id)
            .SingleOrDefaultAsync();

    public Task<string?> GetOrganisationNameForInvitation(Guid invitationId) =>
        context.CompanyInvitations
            .Where(x => x.Id == invitationId)
            .Select(x => x.Application!.Company!.Name)
            .SingleOrDefaultAsync();

    public Task<(bool Exists, Guid? ApplicationId, Guid? CompanyId, string CompanyName, IEnumerable<(Guid IdpId, string IdpName)> IdpInformation, UserInvitationInformation UserInformation)> GetInvitationUserData(Guid companyInvitationId) =>
        context.CompanyInvitations
            .Where(x => x.Id == companyInvitationId)
            .Select(x => new ValueTuple<bool, Guid?, Guid?, string, IEnumerable<ValueTuple<Guid, string>>, UserInvitationInformation>(
                true,
                x.ApplicationId,
                x.Application!.CompanyId,
                x.Application!.Company!.Name,
                x.Application!.Company!.IdentityProviders.Select(x => new ValueTuple<Guid, string>(x.Id, x.IamIdentityProvider!.IamIdpAlias)),
                new UserInvitationInformation(x.FirstName, x.LastName, x.Email, x.UserName)
            ))
            .SingleOrDefaultAsync();

    public Task<(bool Exists, string OrgName, string? IdpName)> GetIdpAndOrgName(Guid invitationId) =>
        context.CompanyInvitations
            .Where(x => x.Id == invitationId)
            .Select(x => new ValueTuple<bool, string, string?>(
                true,
                x.Application!.Company!.Name,
                x.IdpName
            ))
            .SingleOrDefaultAsync();

    public Task<(bool Exists, Guid CompanyId, string? IdpName)> GetIdpAndCompanyId(Guid invitationId) =>
        context.CompanyInvitations
            .Where(x => x.Id == invitationId)
            .Select(x => new ValueTuple<bool, Guid, string?>(
                true,
                x.Application!.CompanyId,
                x.IdpName
            ))
            .SingleOrDefaultAsync();

    public void AttachAndModifyCompanyInvitation(Guid invitationId, Action<CompanyInvitation>? initialize, Action<CompanyInvitation> modify)
    {
        var entity = new CompanyInvitation(invitationId, Guid.Empty, null!, null!, null!, Guid.Empty);
        initialize?.Invoke(entity);
        context.Attach(entity);
        modify(entity);
    }

    public Task<string?> GetIdpNameForInvitationId(Guid invitationId) =>
        context.CompanyInvitations
            .Where(x => x.Id == invitationId)
            .Select(x => x.IdpName)
            .SingleOrDefaultAsync();

    public Task<(string OrgName, string? IdpName, string? ClientId, byte[]? ClientSecret, byte[]? InitializationVector, int? EncryptionMode)> GetUpdateCentralIdpUrlData(Guid invitationId) =>
        context.CompanyInvitations
            .Where(x => x.Id == invitationId)
            .Select(x => new ValueTuple<string, string?, string?, byte[]?, byte[]?, int?>(
                x.Application!.Company!.Name,
                x.IdpName,
                x.ClientId,
                x.ClientSecret,
                x.InitializationVector,
                x.EncryptionMode))
            .SingleOrDefaultAsync();

    public Task<string?> GetServiceAccountUserIdForInvitation(Guid invitationId) =>
        context.CompanyInvitations
            .Where(x => x.Id == invitationId)
            .Select(x => x.ServiceAccountUserId)
            .SingleOrDefaultAsync();
}
