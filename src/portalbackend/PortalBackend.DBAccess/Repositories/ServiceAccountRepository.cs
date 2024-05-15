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
        string? clientClientId,
        CompanyServiceAccountTypeId companyServiceAccountTypeId,
        CompanyServiceAccountKindId companyServiceAccountKindId,
        Action<CompanyServiceAccount>? setOptionalParameters = null)
    {
        var entity = new CompanyServiceAccount(
            identityId,
            name,
            description,
            companyServiceAccountTypeId,
            companyServiceAccountKindId)
        {
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
            default,
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
                    serviceAccount.CompanyServiceAccountKindId,
                    serviceAccount.OfferSubscriptionId,
                    serviceAccount.ClientClientId,
                    serviceAccount.Identity!.IdentityAssignedRoles
                        .Select(assignedRole => assignedRole.UserRole)
                        .Select(userRole => new UserRoleData(
                            userRole!.Id,
                            userRole.Offer!.AppInstances.First().IamClient!.ClientClientId,
                            userRole.UserRoleText))))
            .SingleOrDefaultAsync();

    public Task<(IEnumerable<Guid> UserRoleIds, Guid? ConnectorId, string? ClientClientId, ConnectorStatusId? statusId, OfferSubscriptionStatusId? OfferStatusId)> GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(Guid serviceAccountId, Guid companyId) =>
        _dbContext.CompanyServiceAccounts
            .Where(serviceAccount =>
                serviceAccount.Id == serviceAccountId &&
                serviceAccount.Identity!.UserStatusId == UserStatusId.ACTIVE &&
                (serviceAccount.CompaniesLinkedServiceAccount!.Owners == companyId || serviceAccount.CompaniesLinkedServiceAccount!.Provider == companyId))
            .Select(sa => new ValueTuple<IEnumerable<Guid>, Guid?, string?, ConnectorStatusId?, OfferSubscriptionStatusId?>(
                sa.Identity!.IdentityAssignedRoles.Select(r => r.UserRoleId),
                sa.Connector!.Id,
                sa.ClientClientId,
                sa.Connector!.StatusId,
                sa.OfferSubscription!.OfferSubscriptionStatusId))
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
                    serviceAccount.ClientClientId,
                    serviceAccount.Name,
                    serviceAccount.Description,
                    serviceAccount.Identity!.UserStatusId,
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
                            serviceAccount.OfferSubscription.Id),
                    serviceAccount.Identity.LastEditorId == null
                        ? null
                        : new CompanyLastEditorData(
                            serviceAccount.Identity.LastEditor!.IdentityTypeId == IdentityTypeId.COMPANY_USER
                                ? serviceAccount.Identity.LastEditor.CompanyUser!.Lastname
                                : serviceAccount.Identity.LastEditor.CompanyServiceAccount!.Name,
                            serviceAccount.Identity.LastEditor.Company!.Name),
                    serviceAccount.DimCompanyServiceAccount == null
                        ? null
                        : new DimServiceAccountData(
                            serviceAccount.DimCompanyServiceAccount.ClientSecret,
                            serviceAccount.DimCompanyServiceAccount.InitializationVector,
                            serviceAccount.DimCompanyServiceAccount.EncryptionMode)))
            .SingleOrDefaultAsync();

    public Func<int, int, Task<Pagination.Source<CompanyServiceAccountData>?>> GetOwnCompanyServiceAccountsUntracked(Guid userCompanyId, string? clientId, bool? isOwner, UserStatusId userStatusId) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            _dbContext.CompanyServiceAccounts
                .AsNoTracking()
                .Select(serviceAccount => new
                {
                    ServiceAccount = serviceAccount,
                    IsOwner = serviceAccount.CompaniesLinkedServiceAccount!.Owners == userCompanyId,
                    IsProvider = serviceAccount.CompaniesLinkedServiceAccount!.Provider == userCompanyId
                })
                .Where(x =>
                    (isOwner.HasValue
                        ? isOwner.Value && x.IsOwner || !isOwner.Value && x.IsProvider
                        : x.IsOwner || x.IsProvider) &&
                    x.ServiceAccount.Identity!.UserStatusId == userStatusId &&
                    (clientId == null || EF.Functions.ILike(x.ServiceAccount.ClientClientId!, $"%{clientId.EscapeForILike()}%")))
                .GroupBy(x => x.ServiceAccount.Identity!.UserStatusId),
            x => x.OrderBy(x => x.ServiceAccount.Name),
            x => new CompanyServiceAccountData(
                x.ServiceAccount.Id,
                x.ServiceAccount.ClientClientId,
                x.ServiceAccount.Name,
                x.ServiceAccount.CompanyServiceAccountTypeId,
                x.IsOwner,
                x.IsProvider,
                x.ServiceAccount.OfferSubscriptionId,
                x.ServiceAccount.Connector == null
                    ? null
                    : new ConnectorResponseData(
                        x.ServiceAccount.Connector.Id,
                        x.ServiceAccount.Connector.Name),
                x!.ServiceAccount.OfferSubscription == null
                    ? null
                    : new OfferResponseData(
                        x.ServiceAccount.OfferSubscription.OfferId,
                        x.ServiceAccount.OfferSubscription.Offer!.OfferTypeId,
                        x.ServiceAccount.OfferSubscription.Offer.Name,
                        x.ServiceAccount.OfferSubscription.Id)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<bool> CheckActiveServiceAccountExistsForCompanyAsync(Guid technicalUserId, Guid companyId) =>
        _dbContext.CompanyServiceAccounts
            .Where(sa =>
                sa.Id == technicalUserId &&
                sa.Identity!.UserStatusId == UserStatusId.ACTIVE &&
                sa.Identity.CompanyId == companyId)
            .AnyAsync();

    public Task<(Guid IdentityId, Guid CompanyId)> GetServiceAccountDataByClientId(string clientId) =>
        _dbContext.CompanyServiceAccounts
            .Where(sa => sa.ClientClientId == clientId)
            .Select(sa => new ValueTuple<Guid, Guid>(
                sa.Id,
                sa.Identity!.CompanyId))
            .SingleOrDefaultAsync();

    public void CreateDimCompanyServiceAccount(Guid serviceAccountId, string authenticationServiceUrl, byte[] secret, byte[] initializationVector, int encryptionMode) =>
        _dbContext.DimCompanyServiceAccounts.Add(new DimCompanyServiceAccount(serviceAccountId, authenticationServiceUrl, secret, initializationVector, encryptionMode));

    public void CreateDimUserCreationData(Guid serviceAccountId, Guid processId) =>
         _dbContext.DimUserCreationData.Add(new DimUserCreationData(Guid.NewGuid(), serviceAccountId, processId));

    public Task<(bool IsValid, string? Bpn, string? ClientClientId)> GetDimServiceAccountData(Guid dimServiceAccountId) =>
        _dbContext.DimUserCreationData
            .Where(x => x.Id == dimServiceAccountId)
            .Select(x => new ValueTuple<bool, string?, string?>(
                true,
                x.ServiceAccount!.Identity!.Company!.BusinessPartnerNumber,
                x.ServiceAccount!.ClientClientId))
            .SingleOrDefaultAsync();

    public Task<Guid> GetDimServiceAccountIdForProcess(Guid processId) =>
        _dbContext.DimUserCreationData
            .Where(x => x.ProcessId == processId)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
}
