/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class InvitationRepository : IInvitationRepository
{
    private readonly PortalDbContext _dbContext;

    public InvitationRepository(PortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public IAsyncEnumerable<InvitedUserDetail> GetInvitedUserDetailsUntrackedAsync(Guid applicationId) =>
        (from invitation in _dbContext.Invitations
            join invitationStatus in _dbContext.InvitationStatuses on invitation.InvitationStatusId equals invitationStatus.Id
            join companyuser in _dbContext.CompanyUsers on invitation.CompanyUserId equals companyuser.Id
            join iamuser in _dbContext.IamUsers on companyuser.Id equals iamuser.CompanyUserId
            where invitation.CompanyApplicationId == applicationId
            select new InvitedUserDetail(
                iamuser.UserEntityId,
                invitationStatus.Id,
                companyuser.Email
            ))
        .AsNoTracking()
        .AsAsyncEnumerable();

    public Task<Invitation?> GetInvitationStatusAsync(string iamUserId) =>
        _dbContext.Invitations
            .Where(invitation => invitation.CompanyUser!.IamUser!.UserEntityId == iamUserId)
            .SingleOrDefaultAsync();
}
