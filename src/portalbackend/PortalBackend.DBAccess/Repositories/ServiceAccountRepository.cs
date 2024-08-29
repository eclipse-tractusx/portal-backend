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

public class ServiceAccountRepository(PortalDbContext portalDbContext) : IServiceAccountRepository
{
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
            Guid.NewGuid(),
            name,
            description,
            companyServiceAccountTypeId,
            companyServiceAccountKindId)
        {
            ClientClientId = clientClientId
        };
        setOptionalParameters?.Invoke(entity);
        return portalDbContext.CompanyServiceAccounts.Add(entity).Entity;
    }

    public void AttachAndModifyCompanyServiceAccount(
        Guid id,
        Guid version,
        Action<CompanyServiceAccount>? initialize,
        Action<CompanyServiceAccount> modify)
    {
        var companyServiceAccount = new CompanyServiceAccount(
            id,
            version,
            null!,
            null!,
            default,
            default);
        initialize?.Invoke(companyServiceAccount);
        portalDbContext.Attach(companyServiceAccount);
        modify(companyServiceAccount);
        companyServiceAccount.UpdateVersion();
    }

    public Task<CompanyServiceAccountWithRoleDataClientId?> GetOwnCompanyServiceAccountWithIamClientIdAsync(Guid serviceAccountId, Guid userCompanyId) =>
        portalDbContext.CompanyServiceAccounts
            .Where(serviceAccount =>
                serviceAccount.Id == serviceAccountId
                && serviceAccount.Identity!.UserStatusId == UserStatusId.ACTIVE
                && serviceAccount.Identity.CompanyId == userCompanyId)
            .Select(serviceAccount => new CompanyServiceAccountWithRoleDataClientId(
                    serviceAccount.Version,
                    serviceAccount.Name,
                    serviceAccount.Description,
                    serviceAccount.ClientClientId,
                    serviceAccount.CompanyServiceAccountTypeId,
                    serviceAccount.CompanyServiceAccountKindId,
                    serviceAccount.OfferSubscriptionId,
                    serviceAccount.Identity!.UserStatusId,
                    serviceAccount.Identity!.IdentityAssignedRoles
                        .Select(assignedRole => assignedRole.UserRole)
                        .Select(userRole => new UserRoleData(
                            userRole!.Id,
                            userRole.Offer!.AppInstances.First().IamClient!.ClientClientId,
                            userRole.UserRoleText))))
            .SingleOrDefaultAsync();

    public Task<OwnServiceAccountData?> GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(Guid serviceAccountId, Guid companyId, IEnumerable<ProcessStepTypeId> processStepsToFilter) =>
        portalDbContext.CompanyServiceAccounts
            .Where(serviceAccount =>
                serviceAccount.Id == serviceAccountId &&
                serviceAccount.Identity!.UserStatusId == UserStatusId.ACTIVE &&
                (serviceAccount.CompaniesLinkedServiceAccount!.Owners == companyId || serviceAccount.CompaniesLinkedServiceAccount!.Provider == companyId))
            .Select(sa => new OwnServiceAccountData(
                sa.Identity!.IdentityAssignedRoles.Select(r => r.UserRoleId),
                sa.Id,
                sa.Version,
                sa.Connector!.Id,
                sa.ClientClientId,
                sa.Connector!.StatusId,
                sa.OfferSubscription!.OfferSubscriptionStatusId,
                sa.CompanyServiceAccountKindId == CompanyServiceAccountKindId.EXTERNAL,
                sa.DimUserCreationData!.Process!.ProcessSteps
                        .Any(ps =>
                            ps.ProcessStepStatusId == ProcessStepStatusId.TODO &&
                            processStepsToFilter.Contains(ps.ProcessStepTypeId)),
                sa.DimUserCreationData == null ? null : sa.DimUserCreationData!.ProcessId))
            .SingleOrDefaultAsync();

    public Task<CompanyServiceAccountDetailedData?> GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(Guid serviceAccountId, Guid companyId) =>
        portalDbContext.CompanyServiceAccounts
            .AsNoTracking()
            .Select(serviceAccount => new
            {
                ServiceAccount = serviceAccount,
                serviceAccount.Identity,
                serviceAccount.Connector,
                serviceAccount.OfferSubscription,
                serviceAccount.Identity!.LastEditor,
                serviceAccount.DimCompanyServiceAccount,
                serviceAccount.CompaniesLinkedServiceAccount
            })
            .Where(x =>
                x.ServiceAccount.Id == serviceAccountId &&
                x.Identity!.UserStatusId != UserStatusId.DELETED &&
                (x.CompaniesLinkedServiceAccount!.Owners == companyId || x.CompaniesLinkedServiceAccount!.Provider == companyId))
            .Select(x => new CompanyServiceAccountDetailedData(
                x.ServiceAccount.Id,
                x.ServiceAccount.ClientClientId,
                x.ServiceAccount.Name,
                x.ServiceAccount.Description,
                x.Identity!.UserStatusId,
                x.Identity.IdentityAssignedRoles
                    .Select(assignedRole => assignedRole.UserRole)
                    .Select(userRole => new UserRoleData(
                        userRole!.Id,
                        userRole.Offer!.AppInstances.First().IamClient!.ClientClientId,
                        userRole.UserRoleText)),
                x.ServiceAccount.CompanyServiceAccountTypeId,
                x.ServiceAccount.OfferSubscriptionId,
                x.Connector == null
                    ? null
                    : new ConnectorResponseData(
                        x.Connector.Id,
                        x.Connector.Name),
                x.OfferSubscription == null
                    ? null
                    : new OfferResponseData(
                        x.OfferSubscription.OfferId,
                        x.OfferSubscription.Offer!.OfferTypeId,
                        x.OfferSubscription.Offer.Name,
                        x.OfferSubscription.Id),
                x.Identity.LastEditorId == null
                    ? null
                    : new CompanyLastEditorData(
                        x.LastEditor!.IdentityTypeId == IdentityTypeId.COMPANY_USER
                            ? x.LastEditor.CompanyUser!.Lastname
                            : x.LastEditor.CompanyServiceAccount!.Name,
                        x.LastEditor.Company!.Name),
                x.ServiceAccount.DimCompanyServiceAccount == null
                    ? null
                    : new DimServiceAccountData(
                        x.DimCompanyServiceAccount!.ClientSecret,
                        x.DimCompanyServiceAccount.InitializationVector,
                        x.DimCompanyServiceAccount.EncryptionMode)))
            .SingleOrDefaultAsync();

    public Func<int, int, Task<Pagination.Source<CompanyServiceAccountData>?>> GetOwnCompanyServiceAccountsUntracked(Guid userCompanyId, string? clientId, bool? isOwner, IEnumerable<UserStatusId> userStatusIds) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            portalDbContext.CompanyServiceAccounts
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
                    userStatusIds.Contains(x.ServiceAccount.Identity!.UserStatusId) &&
                    (clientId == null || EF.Functions.ILike(x.ServiceAccount.ClientClientId!, $"%{clientId.EscapeForILike()}%")))
                .GroupBy(x => x.ServiceAccount.Identity!.IdentityTypeId),
            x => x.OrderBy(x => x.ServiceAccount.Name),
            x => new CompanyServiceAccountData(
                x.ServiceAccount.Id,
                x.ServiceAccount.ClientClientId,
                x.ServiceAccount.Name,
                x.ServiceAccount.CompanyServiceAccountTypeId,
                x.ServiceAccount.Identity!.UserStatusId,
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
        portalDbContext.CompanyServiceAccounts
            .Where(sa =>
                sa.Id == technicalUserId &&
                sa.Identity!.UserStatusId == UserStatusId.ACTIVE &&
                sa.Identity.CompanyId == companyId)
            .AnyAsync();

    public Task<(Guid IdentityId, Guid CompanyId)> GetServiceAccountDataByClientId(string clientId) =>
        portalDbContext.CompanyServiceAccounts
            .Where(sa => sa.ClientClientId == clientId)
            .Select(sa => new ValueTuple<Guid, Guid>(
                sa.Id,
                sa.Identity!.CompanyId))
            .SingleOrDefaultAsync();

    public void CreateDimCompanyServiceAccount(Guid serviceAccountId, string authenticationServiceUrl, byte[] secret, byte[] initializationVector, int encryptionMode) =>
        portalDbContext.DimCompanyServiceAccounts.Add(new DimCompanyServiceAccount(serviceAccountId, authenticationServiceUrl, secret, initializationVector, encryptionMode));

    public void CreateDimUserCreationData(Guid serviceAccountId, Guid processId) =>
         portalDbContext.DimUserCreationData.Add(new DimUserCreationData(Guid.NewGuid(), serviceAccountId, processId));

    public Task<(bool IsValid, string? Bpn, string Name)> GetDimServiceAccountData(Guid dimServiceAccountId) =>
        portalDbContext.DimUserCreationData
            .Where(x => x.Id == dimServiceAccountId)
            .Select(x => new ValueTuple<bool, string?, string>(
                true,
                x.ServiceAccount!.Identity!.Company!.BusinessPartnerNumber,
                x.ServiceAccount!.Name))
            .SingleOrDefaultAsync();

    public Task<Guid> GetDimServiceAccountIdForProcess(Guid processId) =>
        portalDbContext.DimUserCreationData
            .Where(x => x.ProcessId == processId)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
}
