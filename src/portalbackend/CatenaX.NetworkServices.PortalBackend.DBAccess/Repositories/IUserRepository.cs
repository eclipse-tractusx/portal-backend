/********************************************************************************
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

using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for User Management on persistence layer.
/// </summary>
public interface IUserRepository
{
    CompanyUser CreateCompanyUser(string? firstName, string? lastName, string email, Guid companyId, CompanyUserStatusId companyUserStatusId, Guid lastEditorId);
    IamUser CreateIamUser(CompanyUser companyUser, string iamUserId);
    IQueryable<CompanyUser> GetOwnCompanyUserQuery(string adminUserId, Guid? companyUserId = null, string? userEntityId = null, string? firstName = null, string? lastName = null, string? email = null);
    Task<bool> IsOwnCompanyUserWithEmailExisting(string email, string adminUserId);
    Task<CompanyUserDetails?> GetOwnCompanyUserDetailsUntrackedAsync(Guid companyUserId, string iamUserId);
    Task<CompanyUserBusinessPartners?> GetOwnCompanyUserWithAssignedBusinessPartnerNumbersUntrackedAsync(Guid companyUserId, string adminUserId);
    Task<Guid> GetCompanyIdForIamUserUntrackedAsync(string iamUserId);
    Task<(Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber, string? IdpAlias)> GetCompanyNameIdpAliasUntrackedAsync(string iamUserId);

    /// <summary>
    /// Gets the CompanyUser Id for the given IamUser Id
    /// </summary>
    /// <param name="userId">the iam userid the company user should be searched for.</param>
    /// <returns>Returns the id of the CompanyUser</returns>
    Task<Guid> GetCompanyUserIdForIamUserUntrackedAsync(string userId);

    /// <summary>
    /// Get the IdpName ,UserId and Role Ids by CompanyUser and AdminUser Id
    /// </summary>
    /// <param name="companyUserId"></param>
    /// <param name="adminUserId"></param>
    /// <returns>Company and IamUser</returns>
    Task<CompanyIamUser?> GetIdpUserByIdUntrackedAsync(Guid companyUserId, string adminUserId);

    Task<CompanyUserDetails?> GetUserDetailsUntrackedAsync(string iamUserId);
    Task<CompanyUserWithIdpBusinessPartnerData?> GetUserWithCompanyIdpAsync(string iamUserId);
    Task<CompanyUserWithIdpData?> GetUserWithIdpAsync(string iamUserId);
    Task<Guid> GetCompanyUserIdForUserApplicationUntrackedAsync(Guid applicationId, string iamUserId);

    /// <summary>
    /// GGets all apps for the give user from the persistence layer.
    /// </summary>
    /// <param name="userId">Id of the user which apps should be selected.</param>
    /// <returns>Returns an IAsyncEnumerable of GUIDs</returns>
    IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserUntrackedAsync(string userId);

    /// <summary>
    /// Gets all business app data for the given userId
    /// </summary>
    /// <param name="userId">Id of the user to get the app data for.</param>
    /// <returns>Returns an IAsyncEnumerable of <see cref="BusinessAppData"/></returns>
    IAsyncEnumerable<BusinessAppData> GetAllBusinessAppDataForUserIdAsync(string userId);

    /// <summary>
    /// Gets the company user ids and checks if its the given iamUser
    /// </summary>
    /// <param name="iamUserId">Id of the iamUser</param>
    /// <param name="companyUserId">The id of the company user to check in the persistence layer.</param>
    /// <returns><c>true</c> if the user exists, otherwise <c>false</c></returns>
    IAsyncEnumerable<(Guid CompanyUserId, bool IsIamUser)> GetCompanyUserWithIamUserCheck(string iamUserId,
        Guid companyUserId);
}
