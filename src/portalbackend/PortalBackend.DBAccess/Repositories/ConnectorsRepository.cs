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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

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
            .SelectMany(u => u.CompanyUser!.Company!.ProvidedConnectors.Where(c => c.StatusId != ConnectorStatusId.INACTIVE));

    /// <inheritdoc/>
    public Func<int,int,Task<Pagination.Source<ManagedConnectorData>?>> GetManagedConnectorsForIamUser(string iamUserId) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            _context.Connectors.AsNoTracking()
                .Where(c => c.Host!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId) &&
                            c.StatusId != ConnectorStatusId.INACTIVE &&
                            c.TypeId == ConnectorTypeId.CONNECTOR_AS_A_SERVICE)
                .GroupBy(c => c.HostId),
            connectors => connectors.OrderByDescending(connector => connector.Name),
            c => new ManagedConnectorData(
                    c.Name,
                    c.Location!.Alpha2Code,
                    c.Id,
                    c.TypeId,
                    c.StatusId,
                    c.DapsRegistrationSuccessful,
                    c.Provider!.Name,
                    c.SelfDescriptionDocumentId)
        ).SingleOrDefaultAsync();

    public Task<(ConnectorData ConnectorData, bool IsProviderUser)> GetConnectorByIdForIamUser(Guid connectorId, string iamUser) =>
        _context.Connectors
            .AsNoTracking()
            .Where(connector => connector.Id == connectorId && connector.StatusId != ConnectorStatusId.INACTIVE)
            .Select(connector => new ValueTuple<ConnectorData, bool>(
                new ConnectorData(
                    connector.Name,
                    connector.Location!.Alpha2Code,
                    connector.Id,
                    connector.TypeId,
                    connector.StatusId,
                    connector.DapsRegistrationSuccessful,
                    connector.HostId,
                    connector.Host!.Name,
                    connector.SelfDescriptionDocumentId,
                    connector.SelfDescriptionDocument!.DocumentName
                ),
                connector.Provider!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUser)
            ))
            .SingleOrDefaultAsync();

    public Task<(ConnectorInformationData ConnectorInformationData, bool IsProviderUser)> GetConnectorInformationByIdForIamUser(Guid connectorId, string iamUser) =>
        _context.Connectors
            .AsNoTracking()
            .Where(connector => connector.Id == connectorId && connector.StatusId != ConnectorStatusId.INACTIVE)
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
    public IAsyncEnumerable<(string BusinessPartnerNumber, string ConnectorEndpoint)> GetConnectorEndPointDataAsync(IEnumerable<string> bpns) =>
        _context.Connectors
            .AsNoTracking()
            .Where(connector => connector.StatusId==ConnectorStatusId.ACTIVE && (!bpns.Any() || bpns.Contains(connector.Provider!.BusinessPartnerNumber)))
            .OrderBy(connector => connector.ProviderId)
            .Select(connector => new ValueTuple<string,string>
            (
                connector.Provider!.BusinessPartnerNumber!,
                connector.ConnectorUrl
            ))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public Connector AttachAndModifyConnector(Guid connectorId, Action<Connector>? initialize, Action<Connector> setOptionalParameters)
    {
        var connector = new Connector(connectorId, null!, null!, null!);
        initialize?.Invoke(connector);
        _context.Attach(connector);
        setOptionalParameters(connector);
        return connector;
    }

    /// <inheritdoc />
    public Task<(Guid ConnectorId, Guid? SelfDescriptionDocumentId)> GetConnectorDataById(Guid connectorId) =>
        _context.Connectors
            .Where(x => x.Id == connectorId && x.StatusId != ConnectorStatusId.INACTIVE)
            .Select(x => new ValueTuple<Guid, Guid?>(x.Id, x.SelfDescriptionDocumentId))
            .SingleOrDefaultAsync();
    
    /// <inheritdoc />
    public Task<(bool IsConnectorIdExist, string? DapsClientId, Guid? SelfDescriptionDocumentId, DocumentStatusId? DocumentStatusId, ConnectorStatusId ConnectorStatus)> GetConnectorDeleteDataAsync(Guid connectorId) =>
        _context.Connectors
            .Where(x => x.Id == connectorId)
            .Select(connector => new ValueTuple<bool, string?, Guid?, DocumentStatusId?, ConnectorStatusId>(
                true,
                connector.ClientDetails == null ? null : connector.ClientDetails!.ClientId,
                connector.SelfDescriptionDocumentId,
                connector.SelfDescriptionDocument!.DocumentStatusId,
                connector.StatusId
            )).SingleOrDefaultAsync();

    /// <inheritdoc />
    public void CreateConnectorClientDetails(Guid connectorId, string dapsClientId) =>
        _context.ConnectorClientDetails.Add(new ConnectorClientDetail(connectorId, dapsClientId));

    /// <inheritdoc />
    public void DeleteConnectorClientDetails(Guid connectorId) => 
        _context.ConnectorClientDetails.Remove(new ConnectorClientDetail(connectorId, null!));

    /// <inheritdoc />
    public Task<ConnectorUpdateInformation?> GetConnectorUpdateInformation(Guid connectorId, string iamUser) =>
        _context.Connectors
            .Where(c => c.Id == connectorId)
            .Select(c => new ConnectorUpdateInformation(
                c.StatusId,
                c.TypeId,
                c.Host!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUser),
                c.ConnectorUrl,
                c.Provider!.BusinessPartnerNumber,
                c.ClientDetails!.ClientId
            ))
            .SingleOrDefaultAsync();
}
