﻿/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;
using CatenaX.NetworkServices.Framework.ErrorHandling;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// Implementation of <see cref="IConnectorsRepository"/> accessing database with EF Core.
public class ConnectorsRepository : IConnectorsRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">PortalDb context.</param>
    public ConnectorsRepository(PortalDbContext portalDbContext)
    {
        this._context = portalDbContext;
    }

    /// <inheritdoc/>
    public IQueryable<Connector> GetAllCompanyConnectorsForIamUser(string iamUserId) =>
        _context.IamUsers.AsNoTracking()
            .Where(u => u.UserEntityId == iamUserId)
            .SelectMany(u => u.CompanyUser!.Company!.ProvidedConnectors);

    public Task<(ConnectorData ConnectorData, bool IsProviderUser)> GetConnectorByIdForIamUser(Guid connectorId, string iamUser) =>
        _context.Connectors
            .AsNoTracking()
            .Where(connector => connector.Id == connectorId)
            .Select(connector => ((ConnectorData ConnectorData, bool IsProviderUser)) new (
                new ConnectorData(connector.Name, connector.Location!.Alpha2Code)
                {
                    Id = connector.Id,
                    Status = connector.Status!.Id,
                    Type = connector.Type!.Id
                },
                connector.Provider!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUser)
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc/>
    public Connector CreateConnector(string name, string location, string connectorUrl, Action<Connector>? setupOptionalFields)
    {
        var connector = new Connector(Guid.NewGuid(), name, location, connectorUrl);
        setupOptionalFields?.Invoke(connector);
        return _context.Connectors.Add(connector).Entity;
    }

    /// <inheritdoc/>
    public async Task DeleteConnectorAsync(Guid connectorId)
    {
        try
        {
            var connector = new Connector(connectorId, string.Empty, string.Empty, string.Empty);
            _context.Connectors.Attach(connector);
            _context.Connectors.Remove(connector);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new NotFoundException("Connector with provided ID does not exist.");
        }
    }
}
