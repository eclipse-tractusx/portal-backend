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

using Org.Eclipse.TractusX.Portal.Backend.Provisioning.ProvisioningEntities;
using Microsoft.EntityFrameworkCore;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;

public class ProvisioningDBAccess : IProvisioningDBAccess
{
    private readonly ProvisioningDbContext _dbContext;

    public ProvisioningDBAccess(ProvisioningDbContext provisioningDBContext)
    {
        _dbContext = provisioningDBContext;
    }

    public async Task<int> GetNextClientSequenceAsync()
    {
        var nextSequence = _dbContext.ClientSequences.Add(new ClientSequence()).Entity;
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return nextSequence.SequenceId;
    }

    public async Task<int> GetNextIdentityProviderSequenceAsync()
    {
        var nextSequence = _dbContext.IdentityProviderSequences.Add(new IdentityProviderSequence()).Entity;
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return nextSequence.SequenceId;
    }

    public UserPasswordReset CreateUserPasswordResetInfo(string userEntityId, DateTimeOffset passwordModifiedAt, int resetCount) =>
        _dbContext.UserPasswordResets.Add(
            new UserPasswordReset(
                userEntityId,
                passwordModifiedAt,
                resetCount
            )
        ).Entity;

    public Task<UserPasswordReset?> GetUserPasswordResetInfo(string userEntityId)
    {
        return _dbContext.UserPasswordResets
            .Where(x => x.UserEntityId == userEntityId)
            .SingleOrDefaultAsync();
    }

    public Task<int> SaveAsync() =>
        _dbContext.SaveChangesAsync();
}
