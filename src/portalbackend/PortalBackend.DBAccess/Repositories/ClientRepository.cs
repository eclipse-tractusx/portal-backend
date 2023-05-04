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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <inheritdoc />
public class ClientRepository : IClientRepository
{
    private readonly PortalDbContext _dbContext;

    /// <summary>
    /// Creates an instance of <see cref="ClientRepository"/>
    /// </summary>
    /// <param name="dbContext">Access to the database</param>
    public ClientRepository(PortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public IamClient CreateClient(string clientId) => 
        _dbContext.IamClients.Add(new IamClient(Guid.NewGuid(), clientId)).Entity;

    /// <inheritdoc />
    public void RemoveClient(Guid clientId) =>
        _dbContext.IamClients.Remove(new IamClient(clientId, null!));
}
