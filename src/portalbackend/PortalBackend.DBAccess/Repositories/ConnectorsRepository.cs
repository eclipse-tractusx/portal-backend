/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

/// <inheritdoc />
public class ConnectorsRepository(PortalDbContext dbContext) : IConnectorsRepository
{
    /// <inheritdoc/>
    public Func<int, int, Task<Pagination.Source<ConnectorData>?>> GetAllCompanyConnectorsForCompanyId(Guid companyId) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            dbContext.Connectors.AsNoTracking()
                .Where(x => x.ProviderId == companyId &&
                       x.StatusId != ConnectorStatusId.INACTIVE &&
                       x.TypeId == ConnectorTypeId.COMPANY_CONNECTOR)
                .GroupBy(c => c.ProviderId),
            connector => connector.OrderByDescending(c => c.Name),
            con => new ConnectorData(
                con.Name,
                con.Location!.Alpha2Code,
                con.Id,
                con.TypeId,
                con.StatusId,
                con.HostId,
                con.Host!.Name,
                con.SelfDescriptionDocumentId,
                con.CompanyServiceAccountId == null ? null : new TechnicalUserData(
                    con.CompanyServiceAccount!.Id,
                    con.CompanyServiceAccount.Name,
                    con.CompanyServiceAccount.ClientClientId,
                    con.CompanyServiceAccount.Description),
                con.ConnectorUrl)
        ).SingleOrDefaultAsync();

    /// <inheritdoc/>
    public Func<int, int, Task<Pagination.Source<ManagedConnectorData>?>> GetManagedConnectorsForCompany(Guid companyId) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            dbContext.Connectors.AsNoTracking()
                .Where(c => c.HostId == companyId &&
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
                    c.Provider!.Name,
                    c.SelfDescriptionDocumentId,
                    c.CompanyServiceAccountId == default ? null : new TechnicalUserData(
                        c.CompanyServiceAccount!.Id,
                        c.CompanyServiceAccount.Name,
                        c.CompanyServiceAccount.ClientClientId,
                        c.CompanyServiceAccount.Description),
                    c.ConnectorUrl)
        ).SingleOrDefaultAsync();

    public Task<(ConnectorData ConnectorData, bool IsProviderCompany)> GetConnectorByIdForCompany(Guid connectorId, Guid companyId) =>
        dbContext.Connectors
            .AsNoTracking()
            .Where(connector => connector.Id == connectorId && connector.StatusId != ConnectorStatusId.INACTIVE)
            .Select(connector => new ValueTuple<ConnectorData, bool>(
                new ConnectorData(
                    connector.Name,
                    connector.Location!.Alpha2Code,
                    connector.Id,
                    connector.TypeId,
                    connector.StatusId,
                    connector.HostId,
                    connector.Host!.Name,
                    connector.SelfDescriptionDocumentId,
                    connector.CompanyServiceAccountId == default ? null : new TechnicalUserData(
                        connector.CompanyServiceAccount!.Id,
                        connector.CompanyServiceAccount.Name,
                        connector.CompanyServiceAccount.ClientClientId,
                        connector.CompanyServiceAccount.Description),
                    connector.ConnectorUrl),
                connector.ProviderId == companyId
            ))
            .SingleOrDefaultAsync();

    public Task<(ConnectorInformationData ConnectorInformationData, bool IsProviderUser)> GetConnectorInformationByIdForIamUser(Guid connectorId, Guid userCompanyId) =>
        dbContext.Connectors
            .AsNoTracking()
            .Where(connector => connector.Id == connectorId && connector.StatusId != ConnectorStatusId.INACTIVE)
            .Select(connector => new ValueTuple<ConnectorInformationData, bool>(
                new ConnectorInformationData(connector.Name, connector.Provider!.BusinessPartnerNumber!, connector.Id, connector.ConnectorUrl),
                connector.ProviderId == userCompanyId
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc/>
    public Connector CreateConnector(string name, string location, string connectorUrl, Action<Connector>? setupOptionalFields)
    {
        var connector = new Connector(Guid.NewGuid(), name, location, connectorUrl);
        setupOptionalFields?.Invoke(connector);
        return dbContext.Connectors.Add(connector).Entity;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<(string BusinessPartnerNumber, string ConnectorEndpoint)> GetConnectorEndPointDataAsync(IEnumerable<string> bpns) =>
        dbContext.Connectors
            .AsNoTracking()
            .Where(connector => connector.StatusId == ConnectorStatusId.ACTIVE && (!bpns.Any() || bpns.Contains(connector.Provider!.BusinessPartnerNumber)))
            .OrderBy(connector => connector.ProviderId)
            .Select(connector => new ValueTuple<string, string>
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
        dbContext.Attach(connector);
        setOptionalParameters(connector);
        return connector;
    }

    /// <inheritdoc />
    public Task<(Guid ConnectorId, Guid? SelfDescriptionDocumentId)> GetConnectorDataById(Guid connectorId) =>
        dbContext.Connectors
            .Where(x => x.Id == connectorId && x.StatusId != ConnectorStatusId.INACTIVE)
            .Select(x => new ValueTuple<Guid, Guid?>(x.Id, x.SelfDescriptionDocumentId))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<DeleteConnectorData?> GetConnectorDeleteDataAsync(Guid connectorId, Guid companyId, IEnumerable<ProcessStepTypeId> processStepsToFilter) =>
        dbContext.Connectors
            .AsSplitQuery()
            .Where(x => x.Id == connectorId)
            .Select(connector => new DeleteConnectorData(
                connector.ProviderId == companyId || connector.HostId == companyId,
                connector.SelfDescriptionDocumentId,
                connector.SelfDescriptionDocument!.DocumentStatusId,
                connector.StatusId,
                connector.ConnectorAssignedOfferSubscriptions.Select(x => new ConnectorOfferSubscription(
                    x.OfferSubscriptionId,
                    x.OfferSubscription!.OfferSubscriptionStatusId
                )),
                connector.CompanyServiceAccount!.Identity!.UserStatusId,
                connector.CompanyServiceAccountId,
                new DeleteServiceAccountData(
                    connector.CompanyServiceAccount!.Identity!.IdentityAssignedRoles.Select(r => r.UserRoleId),
                    connector.CompanyServiceAccount.ClientClientId,
                    connector.CompanyServiceAccount.CompanyServiceAccountKindId == CompanyServiceAccountKindId.EXTERNAL,
                    connector.CompanyServiceAccount.DimUserCreationData!.Process!.ProcessSteps
                        .Any(ps =>
                            ps.ProcessStepStatusId == ProcessStepStatusId.TODO &&
                            processStepsToFilter.Contains(ps.ProcessStepTypeId)),
                    connector.CompanyServiceAccount.DimUserCreationData == null ? null : connector.CompanyServiceAccount.DimUserCreationData!.ProcessId)
            )).SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<ConnectorUpdateInformation?> GetConnectorUpdateInformation(Guid connectorId, Guid companyId) =>
        dbContext.Connectors
            .Where(c => c.Id == connectorId)
            .Select(c => new ConnectorUpdateInformation(
                c.StatusId,
                c.TypeId,
                c.HostId == companyId,
                c.ConnectorUrl,
                c.Provider!.BusinessPartnerNumber
            ))
            .SingleOrDefaultAsync();

    public void DeleteConnector(Guid connectorId) =>
        dbContext.Connectors.Remove(new Connector(connectorId, null!, null!, null!));

    /// <inheritdoc />
    public ConnectorAssignedOfferSubscription CreateConnectorAssignedSubscriptions(Guid connectorId, Guid subscriptionId) =>
        dbContext.ConnectorAssignedOfferSubscriptions.Add(new ConnectorAssignedOfferSubscription(connectorId, subscriptionId)).Entity;

    /// <inheritdoc />
    public void DeleteConnectorAssignedSubscriptions(Guid connectorId, IEnumerable<Guid> assignedOfferSubscriptions) =>
        dbContext.ConnectorAssignedOfferSubscriptions.RemoveRange(assignedOfferSubscriptions.Select(x => new ConnectorAssignedOfferSubscription(connectorId, x)));

    public Func<int, int, Task<Pagination.Source<ConnectorMissingSdDocumentData>?>> GetConnectorsWithMissingSdDocument() =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            dbContext.Connectors.AsNoTracking()
                .Where(x => x.StatusId == ConnectorStatusId.ACTIVE && x.SelfDescriptionDocumentId == null)
                .GroupBy(c => c.StatusId),
            connector => connector.OrderByDescending(c => c.Name),
            con => new ConnectorMissingSdDocumentData(
                con.Id,
                con.Name,
                con.HostId ?? con.ProviderId,
                con.HostId != null ? con.Host!.Name : con.Provider!.Name)
        ).SingleOrDefaultAsync();

    public IAsyncEnumerable<Guid> GetConnectorIdsWithMissingSelfDescription() =>
        dbContext.Connectors
            .Where(c => c.StatusId == ConnectorStatusId.ACTIVE && c.SelfDescriptionDocumentId == null && c.Provider!.SelfDescriptionDocumentId != null)
            .Select(c => c.Id)
            .ToAsyncEnumerable();

    public Task<(Guid Id, string? BusinessPartnerNumber, Guid SelfDescriptionDocumentId)> GetConnectorForProcessId(Guid processId) =>
        dbContext.Connectors
            .Where(c => c.SdCreationProcessId == processId)
            .Select(c => new ValueTuple<Guid, string?, Guid>(c.Id, c.Provider!.BusinessPartnerNumber, c.Provider.SelfDescriptionDocumentId!.Value))
            .SingleOrDefaultAsync();
}
