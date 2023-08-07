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
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class ServiceAccountRepository : IServiceAccountRepository
{
    private readonly PortalDbContext _dbContext;

    public ServiceAccountRepository(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public CompanyServiceAccount CreateCompanyServiceAccount(
        Guid identityId,
        string name,
        string description,
        string clientId,
        string clientClientId,
        CompanyServiceAccountTypeId companyServiceAccountTypeId,
        Action<CompanyServiceAccount>? setOptionalParameters = null)
    {
        var entity = new CompanyServiceAccount(
            identityId,
            name,
            description,
            companyServiceAccountTypeId)
        {
            ClientId = clientId,
            ClientClientId = clientClientId
        };
        setOptionalParameters?.Invoke(entity);
        return _dbContext.CompanyServiceAccounts.Add(entity).Entity;
    }

    public void AttachAndModifyCompanyServiceAccount(
        Guid id,
        Action<CompanyServiceAccount>? initialize,
        Action<CompanyServiceAccount> modify)
    {
        var companyServiceAccount = new CompanyServiceAccount(
            id,
            null!,
            null!,
            default);
        initialize?.Invoke(companyServiceAccount);
        _dbContext.Attach(companyServiceAccount);
        modify(companyServiceAccount);
    }

    public Task<CompanyServiceAccountWithRoleDataClientId?> GetOwnCompanyServiceAccountWithIamClientIdAsync(Guid serviceAccountId, Guid userCompanyId) =>
        _dbContext.CompanyServiceAccounts
            .Where(serviceAccount =>
                serviceAccount.Id == serviceAccountId
                && serviceAccount.Identity!.UserStatusId == UserStatusId.ACTIVE
                && serviceAccount.Identity.CompanyId == userCompanyId)
            .Select(serviceAccount => new CompanyServiceAccountWithRoleDataClientId(
                    serviceAccount.Id,
                    serviceAccount.Identity!.UserStatusId,
                    serviceAccount.Name,
                    serviceAccount.Description,
                    serviceAccount.CompanyServiceAccountTypeId,
                    serviceAccount.OfferSubscriptionId,
                    serviceAccount.ClientId,
                    serviceAccount.ClientClientId,
                    serviceAccount.Identity!.IdentityAssignedRoles
                        .Select(assignedRole => assignedRole.UserRole)
                        .Select(userRole => new UserRoleData(
                            userRole!.Id,
                            userRole.Offer!.AppInstances.First().IamClient!.ClientClientId,
                            userRole.UserRoleText))))
            .SingleOrDefaultAsync();

    public Task<(IEnumerable<Guid> UserRoleIds, Guid? ConnectorId, string? ClientId, ConnectorStatusId? statusId)> GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(Guid serviceAccountId, Guid companyId) =>
        _dbContext.CompanyServiceAccounts
            .Where(serviceAccount =>
                serviceAccount.Id == serviceAccountId &&
                serviceAccount.Identity!.UserStatusId == UserStatusId.ACTIVE &&
                (serviceAccount.CompaniesLinkedServiceAccount!.Owners == companyId || serviceAccount.CompaniesLinkedServiceAccount!.Provider == companyId))
            .Select(sa => new ValueTuple<IEnumerable<Guid>, Guid?, string?, ConnectorStatusId?>(
                sa.Identity!.IdentityAssignedRoles.Select(r => r.UserRoleId),
                sa.Connector!.Id,
                sa.ClientId,
                sa.Connector!.StatusId))
            .SingleOrDefaultAsync();

    public Task<CompanyServiceAccountDetailedData?> GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(Guid serviceAccountId, Guid companyId) =>
        _dbContext.CompanyServiceAccounts
            .AsNoTracking()
            .Where(serviceAccount =>
                serviceAccount.Id == serviceAccountId &&
                serviceAccount.Identity!.UserStatusId == UserStatusId.ACTIVE &&
                (serviceAccount.CompaniesLinkedServiceAccount!.Owners == companyId || serviceAccount.CompaniesLinkedServiceAccount!.Provider == companyId))
            .Select(serviceAccount => new CompanyServiceAccountDetailedData(
                    serviceAccount.Id,
                    serviceAccount.ClientId,
                    serviceAccount.ClientClientId,
                    serviceAccount.Identity!.UserEntityId,
                    serviceAccount.Name,
                    serviceAccount.Description,
                    serviceAccount.Identity.IdentityAssignedRoles
                        .Select(assignedRole => assignedRole.UserRole)
                        .Select(userRole => new UserRoleData(
                            userRole!.Id,
                            userRole.Offer!.AppInstances.First().IamClient!.ClientClientId,
                            userRole.UserRoleText)),
                    serviceAccount.CompanyServiceAccountTypeId,
                    serviceAccount.OfferSubscriptionId,
                    serviceAccount.Connector == null
                        ? null
                        : new ConnectorResponseData(
                            serviceAccount.Connector.Id,
                            serviceAccount.Connector.Name),
                    serviceAccount!.OfferSubscription == null
                        ? null
                        : new OfferResponseData(
                            serviceAccount.OfferSubscription.OfferId,
                            serviceAccount.OfferSubscription.Offer!.OfferTypeId,
                            serviceAccount.OfferSubscription.Offer.Name,
                            serviceAccount.OfferSubscription.Id)))
            .SingleOrDefaultAsync();

    public Func<int, int, Task<Pagination.Source<CompanyServiceAccountData>?>> GetOwnCompanyServiceAccountsUntracked(Guid userCompanyId, string? clientId, bool? isOwner) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            _dbContext.CompanyServiceAccounts
                .AsNoTracking()
                .Where(serviceAccount =>
                    serviceAccount.Identity!.CompanyId == userCompanyId &&
                    serviceAccount.Identity.UserStatusId == UserStatusId.ACTIVE &&
                    (!isOwner.HasValue || (isOwner.Value && serviceAccount.CompaniesLinkedServiceAccount!.Provider == null) || (!isOwner.Value && serviceAccount.CompaniesLinkedServiceAccount!.Provider != null)) &&
                    (clientId == null || EF.Functions.ILike(serviceAccount.ClientClientId!, $"%{clientId.EscapeForILike()}%")))
                .GroupBy(serviceAccount => serviceAccount.Identity!.CompanyId),
            serviceAccounts => serviceAccounts.OrderBy(serviceAccount => serviceAccount.Name),
            serviceAccount => new CompanyServiceAccountData(
                serviceAccount.Id,
                serviceAccount.ClientClientId,
                serviceAccount.Name,
                serviceAccount.CompanyServiceAccountTypeId,
                serviceAccount.CompaniesLinkedServiceAccount!.Provider == null,
                serviceAccount.OfferSubscriptionId,
                serviceAccount.Connector == null
                    ? null
                    : new ConnectorResponseData(
                        serviceAccount.Connector.Id,
                        serviceAccount.Connector.Name),
                serviceAccount!.OfferSubscription == null
                    ? null
                    : new OfferResponseData(
                        serviceAccount.OfferSubscription.OfferId,
                        serviceAccount.OfferSubscription.Offer!.OfferTypeId,
                        serviceAccount.OfferSubscription.Offer.Name,
                        serviceAccount.OfferSubscription.Id)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<bool> CheckActiveServiceAccountExistsForCompanyAsync(Guid technicalUserId, Guid companyId) =>
        _dbContext.CompanyServiceAccounts
            .Where(sa =>
                sa.Id == technicalUserId &&
                sa.Identity!.UserStatusId == UserStatusId.ACTIVE &&
                sa.Identity.CompanyId == companyId)
            .AnyAsync();
}
