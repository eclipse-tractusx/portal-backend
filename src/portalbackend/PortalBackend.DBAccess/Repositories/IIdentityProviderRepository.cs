/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for User Management on persistence layer.
/// </summary>
public interface IIdentityProviderRepository
{
    IdentityProvider CreateIdentityProvider(IdentityProviderCategoryId identityProviderCategory, IdentityProviderTypeId identityProviderTypeId, Guid owner, Action<IdentityProvider>? setOptionalFields);
    void DeleteIdentityProvider(Guid identityProviderId);
    IamIdentityProvider CreateIamIdentityProvider(Guid identityProviderId, string idpAlias);
    void DeleteIamIdentityProvider(string idpAlias);
    void AttachAndModifyIamIdentityProvider(string idpAlias, Action<IamIdentityProvider>? initialize, Action<IamIdentityProvider> modify);
    CompanyIdentityProvider CreateCompanyIdentityProvider(Guid companyId, Guid identityProviderId);
    void DeleteCompanyIdentityProvider(Guid companyId, Guid identityProviderId);
    void CreateCompanyIdentityProviders(IEnumerable<(Guid CompanyId, Guid IdentityProviderId)> companyIdIdentityProviderIds);
    Task<string?> GetSharedIdentityProviderIamAliasDataUntrackedAsync(Guid companyId);
    Task<(string? Alias, bool IsValidUser)> GetIdpCategoryIdByUserIdAsync(Guid companyUserId, Guid userCompanyId);
    Task<(string? Alias, IdentityProviderCategoryId IamIdentityProviderCategory, bool IsOwnOrOwnerCompany, IdentityProviderTypeId TypeId, string? MetadataUrl)> GetOwnCompanyIdentityProviderAliasUntrackedAsync(Guid identityProviderId, Guid companyId);
    Task<(string? Alias, IdentityProviderCategoryId IamIdentityProviderCategory, bool IsOwnerCompany, IdentityProviderTypeId TypeId, string? MetadataUrl, IEnumerable<ConnectedCompanyData> ConnectedCompanies)> GetOwnIdentityProviderWithConnectedCompanies(Guid identityProviderId, Guid companyId);
    Task<(bool IsOwner, (string? Alias, IdentityProviderCategoryId IdentityProviderCategory, IdentityProviderTypeId IdentityProviderTypeId, string? MetadataUrl) IdentityProviderData, IEnumerable<(Guid CompanyId, IEnumerable<string> Aliase)>? CompanyIdAliase, bool CompanyUsersLinked, string IdpOwnerName)> GetOwnCompanyIdentityProviderStatusUpdateData(Guid identityProviderId, Guid companyId, bool queryAliase);
    Task<(bool IsOwner, string? Alias, IdentityProviderCategoryId IdentityProviderCategory, IdentityProviderTypeId IdentityProviderTypeId, string? MetadataUrl)> GetOwnCompanyIdentityProviderUpdateData(Guid identityProviderId, Guid companyId);
    Task<(bool IsOwner, string? Alias, IdentityProviderTypeId IdentityProviderTypeId, IEnumerable<(Guid CompanyId, IEnumerable<string> Aliase)> CompanyIdAliase, string IdpOwnerName)> GetOwnCompanyIdentityProviderUpdateDataForDelete(Guid identityProviderId, Guid companyId);
    IAsyncEnumerable<(Guid IdentityProviderId, IdentityProviderCategoryId CategoryId, string? Alias, IdentityProviderTypeId TypeId, string? MetadataUrl)> GetCompanyIdentityProviderCategoryDataUntracked(Guid companyId);
    IAsyncEnumerable<(Guid IdentityProviderId, string Alias)> GetOwnCompanyIdentityProviderAliasDataUntracked(Guid companyId, IEnumerable<Guid> identityProviderIds);
    Task<(Guid IdentityProviderId, string? Alias)> GetSingleManagedIdentityProviderAliasDataUntracked(Guid companyId);
    IAsyncEnumerable<(Guid IdentityProviderId, string? Alias)> GetManagedIdentityProviderAliasDataUntracked(Guid companyId, IEnumerable<Guid> identityProviderIds);
    Task<(bool IsValidUser, string? Alias, bool IsSameCompany)> GetIamUserIsOwnCompanyIdentityProviderAliasAsync(Guid companyUserId, Guid identityProviderId, Guid companyId);

    Task<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company, (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser, IEnumerable<(Guid IdentityProviderId, string Alias)> IdpAliase)> GetCompanyNameIdpAliaseUntrackedAsync(Guid companyUserId, Guid? applicationId, IdentityProviderCategoryId identityProviderCategoryId, IdentityProviderTypeId identityProviderTypeId);

    Task<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company,
        (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser,
        (string? IdpAlias, bool IsSharedIdp) IdentityProvider)>
            GetCompanyNameIdpAliasUntrackedAsync(Guid identityProviderId, Guid companyUserId);

    IAsyncEnumerable<Guid> GetIdpLinkedCompanyUserIds(Guid identityProviderId, Guid companyId);
    IAsyncEnumerable<(Guid CompanyId, CompanyStatusId CompanyStatusId, bool HasMoreIdentityProviders, IEnumerable<(Guid IdentityId, bool IsLinkedCompanyUser, (string? UserMail, string? FirstName, string? LastName) Userdata, bool IsInUserRoles, IEnumerable<Guid> UserRoleIds)> Identities)> GetManagedIdpLinkedData(Guid identityProviderId, IEnumerable<Guid> userRoleIds);
    IAsyncEnumerable<(string Email, string? FirstName, string? LastName)> GetCompanyUserEmailForIdpWithoutOwnerAndRoleId(IEnumerable<Guid> userRoleIds, Guid identityProviderId);
    Task<(IdpData? idpData, Guid companyId, Guid companyUserId)> GetIdentityProviderDataForProcessIdAsync(Guid processId);
    IAsyncEnumerable<Guid> GetIdentityproviderIdAsync(IEnumerable<string> alias);
    void AttachAndModifyIdentityProvider(Guid identityProviderId, Action<IdentityProvider>? initialize, Action<IdentityProvider> modify);
}
