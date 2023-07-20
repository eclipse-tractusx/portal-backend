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

public class InvitationRepository : IInvitationRepository
{
    private readonly PortalDbContext _dbContext;

    public InvitationRepository(PortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IAsyncEnumerable<InvitedUserDetail> GetInvitedUserDetailsUntrackedAsync(Guid applicationId) =>
        _dbContext.Invitations
            .Join(_dbContext.InvitationStatuses,
                i => i.InvitationStatusId,
                invitationStatus => invitationStatus.Id,
                (i, invitationStatus) => new { Invitation = i, InvitationStatus = invitationStatus })
            .Join(_dbContext.CompanyUsers,
                i => i.Invitation.CompanyUserId,
                cu => cu.Id,
                (i, cu) => new { i.Invitation, i.InvitationStatus, CompanyUser = cu })
            .Where(i => i.Invitation.CompanyApplicationId == applicationId && i.CompanyUser.Identity!.UserStatusId != UserStatusId.DELETED)
            .Select(i => new InvitedUserDetail(
                i.CompanyUser.Identity!.UserEntityId,
                i.InvitationStatus.Id,
                i.CompanyUser.Email))
        .AsNoTracking()
        .AsAsyncEnumerable();

    public Task<Invitation?> GetInvitationStatusAsync(Guid companyUserId) =>
        _dbContext.Invitations
            .Where(invitation => invitation.CompanyUserId == companyUserId)
            .SingleOrDefaultAsync();
}
