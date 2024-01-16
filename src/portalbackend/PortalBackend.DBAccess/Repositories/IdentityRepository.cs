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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class IdentityRepository : IIdentityRepository
{
    private readonly PortalDbContext _context;

    public IdentityRepository(PortalDbContext context)
    {
        _context = context;
    }

    public Task<Guid> GetActiveCompanyIdByIdentityId(Guid identityId) =>
        _context.Identities.Where(x => x.Id == identityId && x.UserStatusId == UserStatusId.ACTIVE)
            .Select(x => x.CompanyId)
            .SingleOrDefaultAsync();

    public Task<(IdentityTypeId IdentityTypeId, Guid CompanyId)> GetActiveIdentityDataByIdentityId(Guid identityId) =>
        _context.Identities.Where(x => x.Id == identityId && x.UserStatusId == UserStatusId.ACTIVE)
            .Select(x => new ValueTuple<IdentityTypeId, Guid>(
                x.IdentityTypeId,
                x.CompanyId))
            .SingleOrDefaultAsync();

    public Task<(Guid IdentityId, IdentityTypeId IdentityTypeId, Guid CompanyId)> GetActiveIdentityDataByUserEntityId(string userEntityId) =>
        _context.Identities.Where(x => x.UserEntityId == userEntityId && x.UserStatusId == UserStatusId.ACTIVE)
            .Select(x => new ValueTuple<Guid, IdentityTypeId, Guid>(
                x.Id,
                x.IdentityTypeId,
                x.CompanyId))
            .SingleOrDefaultAsync();
}
