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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Diagnostics;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class TechnicalUserRepository(PortalDbContext portalDbContext) : ITechnicalUserRepository
{
    public TechnicalUser CreateTechnicalUser(
        Guid identityId,
        string name,
        string description,
        string? clientClientId,
        TechnicalUserTypeId technicalUserTypeId,
        TechnicalUserKindId technicalUserKindId,
        Action<TechnicalUser>? setOptionalParameters = null)
    {
        var entity = new TechnicalUser(
            identityId,
            Guid.NewGuid(),
            name,
            description,
            technicalUserTypeId,
            technicalUserKindId)
        {
            ClientClientId = clientClientId
        };
        setOptionalParameters?.Invoke(entity);
        return portalDbContext.TechnicalUsers.Add(entity).Entity;
    }

    public void AttachAndModifyTechnicalUser(
        Guid id,
        Guid version,
        Action<TechnicalUser>? initialize,
        Action<TechnicalUser> modify)
    {
        var technicalUser = new TechnicalUser(
            id,
            version,
            null!,
            null!,
            default,
            default);
        initialize?.Invoke(technicalUser);
        portalDbContext.Attach(technicalUser);
        modify(technicalUser);
        technicalUser.UpdateVersion();
    }

    public Task<TechnicalUserWithRoleDataClientId?> GetTechnicalUserWithRoleDataClientIdAsync(Guid technicalUserId, Guid userCompanyId) =>
        portalDbContext.TechnicalUsers
            .Where(tu =>
                tu.Id == technicalUserId
                && tu.Identity!.UserStatusId == UserStatusId.ACTIVE
                && tu.Identity.CompanyId == userCompanyId)
            .Select(tu => new TechnicalUserWithRoleDataClientId(
                    tu.Version,
                    tu.Name,
                    tu.Description,
                    tu.ClientClientId,
                    tu.TechnicalUserTypeId,
                    tu.TechnicalUserKindId,
                    tu.OfferSubscriptionId,
                    tu.Identity!.UserStatusId,
                    tu.Identity!.IdentityAssignedRoles
                        .Select(assignedRole => assignedRole.UserRole)
                        .Select(userRole => new UserRoleData(
                            userRole!.Id,
                            userRole.Offer!.AppInstances.First().IamClient!.ClientClientId,
                            userRole.UserRoleText))))
            .SingleOrDefaultAsync();

    public Task<OwnTechnicalUserData?> GetOwnTechnicalUserWithIamUserRolesAsync(Guid technicalUserId, Guid companyId, IEnumerable<ProcessStepTypeId> processStepsToFilter) =>
        portalDbContext.TechnicalUsers
            .Where(tu =>
                tu.Id == technicalUserId)
            .Select(tu => new OwnTechnicalUserData(
                tu.Identity!.IdentityAssignedRoles.Select(r => r.UserRoleId),
                tu.Id,
                tu.Identity!.UserStatusId,
                tu.CompaniesLinkedTechnicalUser!.Owners == companyId || tu.CompaniesLinkedTechnicalUser!.Provider == companyId,
                tu.Version,
                tu.Connector!.Id,
                tu.ClientClientId,
                tu.Connector!.StatusId,
                tu.OfferSubscription!.OfferSubscriptionStatusId,
                tu.TechnicalUserKindId == TechnicalUserKindId.EXTERNAL,
                tu.ExternalTechnicalUserCreationData!.Process!.ProcessSteps
                        .Any(ps =>
                            ps.ProcessStepStatusId == ProcessStepStatusId.TODO &&
                            processStepsToFilter.Contains(ps.ProcessStepTypeId)),
                tu.ExternalTechnicalUserCreationData == null ? null : tu.ExternalTechnicalUserCreationData!.ProcessId))
            .SingleOrDefaultAsync();

    public Task<TechnicalUserDetailedData?> GetOwnTechnicalUserDataUntrackedAsync(Guid technicalUserId, Guid companyId) =>
        portalDbContext.TechnicalUsers
            .AsNoTracking()
            .Select(tu => new
            {
                TechnicalUser = tu,
                tu.Identity,
                tu.Connector,
                tu.OfferSubscription,
                tu.Identity!.LastEditor,
                tu.ExternalTechnicalUser,
                tu.CompaniesLinkedTechnicalUser
            })
            .Where(x =>
                x.TechnicalUser.Id == technicalUserId &&
                x.Identity!.UserStatusId != UserStatusId.DELETED &&
                (x.CompaniesLinkedTechnicalUser!.Owners == companyId || x.CompaniesLinkedTechnicalUser!.Provider == companyId))
            .Select(x => new TechnicalUserDetailedData(
                x.TechnicalUser.Id,
                x.TechnicalUser.ClientClientId,
                x.TechnicalUser.Name,
                x.TechnicalUser.Description,
                x.Identity!.UserStatusId,
                x.Identity.IdentityAssignedRoles
                    .Select(assignedRole => assignedRole.UserRole)
                    .Select(userRole => new UserRoleData(
                        userRole!.Id,
                        userRole.Offer!.AppInstances.First().IamClient!.ClientClientId,
                        userRole.UserRoleText)),
                x.TechnicalUser.TechnicalUserTypeId,
                x.TechnicalUser.TechnicalUserKindId,
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
                            : x.LastEditor.TechnicalUser!.Name,
                        x.LastEditor.Company!.Name),
                x.TechnicalUser.ExternalTechnicalUser == null
                    ? null
                    : new ExternalTechnicalUserData(
                        x.ExternalTechnicalUser!.AuthenticationServiceUrl,
                        x.ExternalTechnicalUser!.ClientSecret,
                        x.ExternalTechnicalUser.InitializationVector,
                        x.ExternalTechnicalUser.EncryptionMode)))
            .SingleOrDefaultAsync();

    public Func<int, int, Task<Pagination.Source<CompanyServiceAccountData>?>> GetOwnTechnicalUsersUntracked(Guid userCompanyId, string? clientId, bool? isOwner, IEnumerable<UserStatusId> userStatusIds) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            portalDbContext.TechnicalUsers
                .AsNoTracking()
                .Select(tu => new
                {
                    TechnicalUser = tu,
                    IsOwner = tu.CompaniesLinkedTechnicalUser!.Owners == userCompanyId,
                    IsProvider = tu.CompaniesLinkedTechnicalUser!.Provider == userCompanyId
                })
                .Where(x =>
                    (isOwner.HasValue
                        ? isOwner.Value && x.IsOwner || !isOwner.Value && x.IsProvider
                        : x.IsOwner || x.IsProvider) &&
                    userStatusIds.Contains(x.TechnicalUser.Identity!.UserStatusId) &&
                    (clientId == null || EF.Functions.ILike(x.TechnicalUser.ClientClientId!, $"%{clientId.EscapeForILike()}%")))
                .GroupBy(x => x.TechnicalUser.Identity!.IdentityTypeId),
            x => x.OrderBy(x => x.TechnicalUser.Name),
            x => new CompanyServiceAccountData(
                x.TechnicalUser.Id,
                x.TechnicalUser.ClientClientId,
                x.TechnicalUser.Name,
                x.TechnicalUser.TechnicalUserKindId,
                x.TechnicalUser.TechnicalUserTypeId,
                x.TechnicalUser.Identity!.UserStatusId,
                x.IsOwner,
                x.IsProvider,
                x.TechnicalUser.OfferSubscriptionId,
                x.TechnicalUser.Connector == null
                    ? null
                    : new ConnectorResponseData(
                        x.TechnicalUser.Connector.Id,
                        x.TechnicalUser.Connector.Name),
                x!.TechnicalUser.OfferSubscription == null
                    ? null
                    : new OfferResponseData(
                        x.TechnicalUser.OfferSubscription.OfferId,
                        x.TechnicalUser.OfferSubscription.Offer!.OfferTypeId,
                        x.TechnicalUser.OfferSubscription.Offer.Name,
                        x.TechnicalUser.OfferSubscription.Id)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<bool> CheckActiveServiceAccountExistsForCompanyAsync(Guid technicalUserId, Guid companyId) =>
        portalDbContext.TechnicalUsers
            .Where(tu =>
                tu.Id == technicalUserId &&
                (tu.Identity!.UserStatusId == UserStatusId.ACTIVE || tu.Identity!.UserStatusId == UserStatusId.PENDING) &&
                tu.Identity.CompanyId == companyId)
            .AnyAsync();

    public Task<(Guid IdentityId, Guid CompanyId)> GetTechnicalUserDataByClientId(string clientId) =>
        portalDbContext.TechnicalUsers
            .Where(tu => tu.ClientClientId == clientId)
            .Select(tu => new ValueTuple<Guid, Guid>(
                tu.Id,
                tu.Identity!.CompanyId))
            .SingleOrDefaultAsync();

    public void CreateExternalTechnicalUser(Guid technicalUserId, string authenticationServiceUrl, byte[] secret, byte[] initializationVector, int encryptionMode) =>
        portalDbContext.ExternalTechnicalUsers.Add(new ExternalTechnicalUser(technicalUserId, authenticationServiceUrl, secret, initializationVector, encryptionMode));

    public void CreateExternalTechnicalUserCreationData(Guid technicalUserId, Guid processId) =>
         portalDbContext.ExternalTechnicalUserCreationData.Add(new ExternalTechnicalUserCreationData(Guid.NewGuid(), technicalUserId, processId));

    public Task<(bool IsValid, string? Bpn, string Name)> GetExternalTechnicalUserData(Guid externalTechnicalUserId) =>
        portalDbContext.ExternalTechnicalUserCreationData
            .Where(x => x.Id == externalTechnicalUserId)
            .Select(x => new ValueTuple<bool, string?, string>(
                true,
                x.TechnicalUser!.Identity!.Company!.BusinessPartnerNumber,
                x.TechnicalUser!.Name))
            .SingleOrDefaultAsync();

    public Task<Guid> GetExternalTechnicalUserIdForProcess(Guid processId) =>
        portalDbContext.ExternalTechnicalUserCreationData
            .Where(x => x.ProcessId == processId)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();

    public Task<(ProcessTypeId ProcessTypeId, VerifyProcessData<ProcessTypeId, ProcessStepTypeId> ProcessData, Guid? TechnicalUserId, Guid? TechnicalUserVersion)> GetProcessDataForTechnicalUserCallback(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds) =>
        portalDbContext.ExternalTechnicalUserCreationData
            .AsNoTracking()
            .Where(x => x.ProcessId == processId)
            .Select(x => new ValueTuple<ProcessTypeId, VerifyProcessData<ProcessTypeId, ProcessStepTypeId>, Guid?, Guid?>(
                x.Process!.ProcessTypeId,
                new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(
                    x.Process,
                    x.Process.ProcessSteps
                        .Where(step =>
                            processStepTypeIds.Contains(step.ProcessStepTypeId) &&
                            step.ProcessStepStatusId == ProcessStepStatusId.TODO)),
                x.TechnicalUserId,
                x.TechnicalUser!.Version)
            )
            .SingleOrDefaultAsync();

    public Task<(ProcessTypeId ProcessTypeId, VerifyProcessData<ProcessTypeId, ProcessStepTypeId> ProcessData, Guid? TechnicalUserId)> GetProcessDataForTechnicalUserDeletionCallback(Guid processId, IEnumerable<ProcessStepTypeId>? processStepTypeIds) =>
        portalDbContext.ExternalTechnicalUserCreationData
            .AsNoTracking()
            .Where(x => x.ProcessId == processId && x.Process!.ProcessTypeId == ProcessTypeId.DIM_TECHNICAL_USER)
            .Select(x => new ValueTuple<ProcessTypeId, VerifyProcessData<ProcessTypeId, ProcessStepTypeId>, Guid?>(
                x.Process!.ProcessTypeId,
                new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(
                    x.Process,
                    x.Process!.ProcessSteps
                        .Where(step =>
                            (processStepTypeIds == null || processStepTypeIds.Contains(step.ProcessStepTypeId)) &&
                            step.ProcessStepStatusId == ProcessStepStatusId.TODO)),
                x.TechnicalUserId)
            )
            .SingleOrDefaultAsync();
}
