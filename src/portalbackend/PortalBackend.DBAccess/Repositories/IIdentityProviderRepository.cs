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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for User Management on persistence layer.
/// </summary>
public interface IIdentityProviderRepository
{
    IdentityProvider CreateIdentityProvider(IdentityProviderCategoryId identityProviderCategory);
    IamIdentityProvider CreateIamIdentityProvider(IdentityProvider identityProvider, string idpAlias);
    CompanyIdentityProvider CreateCompanyIdentityProvider(Guid companyId, Guid identityProviderId);
    Task<(string? SharedIdpAlias, Guid CompanyUserId)> GetSharedIdentityProviderIamAliasDataUntrackedAsync(Guid companyUserId);
    Task<IdpUser?> GetIdpCategoryIdByUserIdAsync(Guid companyUserId, Guid userCompanyId);
    Task<(string Alias, IdentityProviderCategoryId IamIdentityProviderCategory, bool IsOwnCompany)> GetOwnCompanyIdentityProviderAliasUntrackedAsync(Guid identityProviderId, Guid userCompanyId);
    Task<(bool IsSameCompany, string Alias, IdentityProviderCategoryId IdentityProviderCategory, IEnumerable<string> Aliase)> GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(Guid identityProviderId, Guid userCompanyId);
    Task<(Guid CompanyId, int LinkedCompaniesCount, string Alias, IdentityProviderCategoryId IdentityProviderCategory, IEnumerable<string> Aliase)> GetOwnCompanyIdentityProviderDeletionDataUntrackedAsync(Guid identityProviderId, string iamUserId);
    IAsyncEnumerable<(Guid IdentityProviderId, IdentityProviderCategoryId CategoryId, string Alias)> GetOwnCompanyIdentityProviderCategoryDataUntracked(Guid companyUserId);
    IAsyncEnumerable<(Guid IdentityProviderId, IdentityProviderCategoryId CategoryId, string Alias)> GetCompanyIdentityProviderCategoryDataUntracked(Guid companyId);
    IAsyncEnumerable<(Guid IdentityProviderId, string Alias)> GetOwnCompanyIdentityProviderAliasDataUntracked(Guid companyUserId, IEnumerable<Guid> identityProviderIds);
    Task<(string? UserEntityId, string? Alias, bool IsSameCompany)> GetIamUserIsOwnCompanyIdentityProviderAliasAsync(Guid companyUserId, Guid identityProviderId, Guid userCompanyId);

    Task<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company,
        (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser,
        IEnumerable<string> IdpAliase)>
            GetCompanyNameIdpAliaseUntrackedAsync(string iamUserId, Guid? applicationId, IdentityProviderCategoryId identityProviderCategoryId);

    Task<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company,
        (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser,
        (string? IdpAlias, bool IsSharedIdp) IdentityProvider)>
            GetCompanyNameIdpAliasUntrackedAsync(Guid identityProviderId, string iamUserId);
}
