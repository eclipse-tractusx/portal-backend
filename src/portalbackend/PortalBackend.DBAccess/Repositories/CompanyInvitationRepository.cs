/********************************************************************************
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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class CompanyInvitationRepository : ICompanyInvitationRepository
{
    private readonly PortalDbContext _context;

    public CompanyInvitationRepository(PortalDbContext context) => _context = context;

    public CompanyInvitation CreateCompanyInvitation(string firstName, string lastName, string email, string organisationName, Guid processId, Action<CompanyInvitation>? setOptionalFields)
    {
        var entity = new CompanyInvitation(Guid.NewGuid(), firstName, lastName, email, organisationName, processId);
        setOptionalFields?.Invoke(entity);
        return _context.Add(entity).Entity;
    }

    public Task<Guid> GetCompanyInvitationForProcessId(Guid processId) =>
        _context.Processes
            .Where(process => process.Id == processId)
            .Select(process => process.CompanyInvitation!.Id)
            .SingleOrDefaultAsync();

    public Task<string?> GetOrganisationNameForInvitation(Guid invitationId) =>
        _context.CompanyInvitations
            .Where(x => x.Id == invitationId)
            .Select(x => x.OrganisationName)
            .SingleOrDefaultAsync();

    public Task<(bool Exists, Guid? ApplicationId, Guid? CompanyId, string CompanyName, IEnumerable<(Guid IdpId, string IdpName)> IdpInformation, UserInvitationInformation UserInformation)> GetInvitationUserData(Guid companyInvitationId) =>
        _context.CompanyInvitations
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
        _context.CompanyInvitations
            .Where(x => x.Id == invitationId)
            .Select(x => new ValueTuple<bool, string, string?>(
                true,
                x.OrganisationName,
                x.IdpName
            ))
            .SingleOrDefaultAsync();

    public void AttachAndModifyCompanyInvitation(Guid invitationId, Action<CompanyInvitation>? initialize, Action<CompanyInvitation> modify)
    {
        var entity = new CompanyInvitation(invitationId, null!, null!, null!, null!, Guid.Empty);
        initialize?.Invoke(entity);
        _context.Attach(entity);
        modify(entity);
    }

    public Task<string?> GetIdpNameForInvitationId(Guid invitationId) =>
        _context.CompanyInvitations
            .Where(x => x.Id == invitationId)
            .Select(x => x.IdpName)
            .SingleOrDefaultAsync();

    public Task<(string OrgName, string? IdpName, string? ClientId, byte[]? ClientSecret, byte[]? InitializationVector, int? EncryptionMode)> GetUpdateCentralIdpUrlData(Guid invitationId) =>
        _context.CompanyInvitations
            .Where(x => x.Id == invitationId)
            .Select(x => new ValueTuple<string, string?, string?, byte[]?, byte[]?, int?>(
                x.OrganisationName,
                x.IdpName,
                x.ClientId,
                x.ClientSecret,
                x.ClientIdInitializationVector,
                x.ClientIdEncryptionMode))
            .SingleOrDefaultAsync();

    public Task<string?> GetServiceAccountUserIdForInvitation(Guid invitationId) =>
        _context.CompanyInvitations
            .Where(x => x.Id == invitationId)
            .Select(x => x.ServiceAccountUserId)
            .SingleOrDefaultAsync();
}
