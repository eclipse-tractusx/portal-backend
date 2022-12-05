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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

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
            .Select(connector => new ValueTuple<ConnectorData, bool>(
                new ConnectorData(connector.Name, connector.Location!.Alpha2Code, connector.Id, connector.TypeId, connector.StatusId),
                connector.Provider!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUser)
            ))
            .SingleOrDefaultAsync();

    public Task<(ConnectorInformationData ConnectorInformationData, bool IsProviderUser)> GetConnectorInformationByIdForIamUser(Guid connectorId, string iamUser) =>
        _context.Connectors
            .AsNoTracking()
            .Where(connector => connector.Id == connectorId)
            .Select(connector => new ValueTuple<ConnectorInformationData, bool>(
                new ConnectorInformationData(connector.Name, connector.Provider!.BusinessPartnerNumber!, connector.Id, connector.ConnectorUrl),
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
    
    /// <inheritdoc/>
    public IAsyncEnumerable<(string BusinessPartnerNumber, string ConnectorEndpoint)> GetConnectorEndPointDataAsync(IEnumerable<string> bpns) =>
        _context.Connectors
            .AsNoTracking()
            .Where(connector => bpns.Contains(connector.Provider!.BusinessPartnerNumber))
            .OrderBy(connector => connector.ProviderId)
            .Select(connector => new ValueTuple<string,string>
            (
                connector.Provider!.BusinessPartnerNumber!,
                connector.ConnectorUrl
            ))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public Connector AttachAndModifyConnector(Guid connectorId, Action<Connector>? setOptionalParameters = null)
    {
        var connector = _context.Connectors.Attach(new Connector(connectorId, null!, null!, null!)).Entity;
        setOptionalParameters?.Invoke(connector);
        return connector;
    }
}
