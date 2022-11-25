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

using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for User Management on persistence layer.
/// </summary>
public interface IUserRepository
{
    IAsyncEnumerable<CompanyApplicationWithStatus> GetApplicationsWithStatusUntrackedAsync(string iamUserId);
    Task<RegistrationData?> GetRegistrationDataUntrackedAsync(Guid applicationId, string iamUserId);
    CompanyUser CreateCompanyUser(string? firstName, string? lastName, string email, Guid companyId, CompanyUserStatusId companyUserStatusId, Guid lastEditorId);
    void AttachAndModifyCompanyUser(Guid companyUserId, Action<CompanyUser> setOptionalParameters);
    IamUser CreateIamUser(Guid companyUserId, string iamUserId);
    IamUser DeleteIamUser(string iamUserId);
    IQueryable<CompanyUser> GetOwnCompanyUserQuery(string adminUserId, Guid? companyUserId = null, string? userEntityId = null, string? firstName = null, string? lastName = null, string? email = null, IEnumerable<CompanyUserStatusId>? statusIds = null);
    Task<(string UserEntityId, string? FirstName, string? LastName, string? Email)> GetUserEntityDataAsync(Guid companyUserId, Guid companyId);
    IAsyncEnumerable<(string? UserEntityId, Guid CompanyUserId)> GetMatchingCompanyIamUsersByNameEmail(string firstName, string lastName, string email, Guid companyId, IEnumerable<CompanyUserStatusId> companyUserStatusIds);
    Task<(Guid companyId, Guid companyUserId)> GetOwnCompanyAndCompanyUserId(string iamUserId);
    Task<Guid> GetOwnCompanyId(string iamUserId);
    Task<(CompanyInformationData companyInformation, Guid companyUserId, string? userEmail)> GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(string iamUserId);
    Task<bool> IsOwnCompanyUserWithEmailExisting(string email, string adminUserId);
    Task<CompanyUserDetails?> GetOwnCompanyUserDetailsUntrackedAsync(Guid companyUserId, string iamUserId);
    Task<CompanyUserBusinessPartners?> GetOwnCompanyUserWithAssignedBusinessPartnerNumbersUntrackedAsync(Guid companyUserId, string adminUserId);
    Task<Guid> GetCompanyIdForIamUserUntrackedAsync(string iamUserId);
    Task<(Guid CompanyId, string Bpn)> GetCompanyIdAndBpnForIamUserUntrackedAsync(string iamUserId);
    
    /// <summary>
    /// Gets the CompanyUser Id for the given IamUser Id
    /// </summary>
    /// <param name="userId">the iam userid the company user should be searched for.</param>
    /// <returns>Returns the id of the CompanyUser</returns>
    Task<Guid> GetCompanyUserIdForIamUserUntrackedAsync(string userId);

    Task<CompanyUserDetails?> GetUserDetailsUntrackedAsync(string iamUserId);
    Task<CompanyUserWithIdpBusinessPartnerData?> GetUserWithCompanyIdpAsync(string iamUserId);
    Task<Guid> GetCompanyUserIdForUserApplicationUntrackedAsync(Guid applicationId, string iamUserId);

    /// <summary>
    /// GGets all apps for the give user from the persistence layer.
    /// </summary>
    /// <param name="userId">Id of the user which apps should be selected.</param>
    /// <returns>Returns an IAsyncEnumerable of GUIDs</returns>
    IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserUntrackedAsync(string userId);

    /// <summary>
    /// Gets the company user ids and checks if its the given iamUser
    /// </summary>
    /// <param name="iamUserId">Id of the iamUser</param>
    /// <param name="companyUserId">The id of the company user to check in the persistence layer.</param>
    /// <returns><c>true</c> if the user exists, otherwise <c>false</c></returns>
    IAsyncEnumerable<(Guid CompanyUserId, bool IsIamUser)> GetCompanyUserWithIamUserCheck(string iamUserId,
        Guid companyUserId);

    /// <summary>
    /// Gets the company user ids and checks if its the given iamUser
    /// </summary>
    /// <param name="iamUserId">Id of the iamUser</param>
    /// <param name="salesManagerId">The id of the company user to check in the persistence layer.</param>
    /// <returns><c>true</c> if the user exists, otherwise <c>false</c></returns>
    IAsyncEnumerable<(Guid CompanyUserId, bool IsIamUser, string CompanyShortName, Guid CompanyId)> GetCompanyUserWithIamUserCheckAndCompanyShortName(string iamUserId, Guid salesManagerId);

    /// <summary>
    /// Gets all company user ids which have the any given user role assigned
    /// </summary>
    /// <param name="userRoleIds">User role ids</param>
    /// <returns>Returns a list of the company user ids</returns>
    IAsyncEnumerable<Guid> GetCompanyUserWithRoleId(IEnumerable<Guid> userRoleIds);

    /// <summary>
    /// Gets a company Id for the given service account
    /// </summary>
    /// <param name="iamUserId">Id of the service account</param>
    /// <returns>The Id of the company</returns>
    Task<Guid> GetServiceAccountCompany(string iamUserId);

    Task<OfferIamUserData?> GetAppAssignedIamClientUserDataUntrackedAsync(Guid offerId, Guid companyUserId, string iamUserId);
    Task<OfferIamUserData?> GetCoreOfferAssignedIamClientUserDataUntrackedAsync(Guid offerId, Guid companyUserId, string iamUserId);
    
    IAsyncEnumerable<Guid> GetServiceProviderCompanyUserWithRoleIdAsync(Guid offerId, List<Guid> userRoleIds);

    Func<int,int,Task<Pagination.Source<CompanyAppUserDetails>?>> GetOwnCompanyAppUsersPaginationSourceAsync(Guid appId, string iamUserId, IEnumerable<OfferSubscriptionStatusId> subscriptionStatusIds, IEnumerable<CompanyUserStatusId> companyUserStatusIds, CompanyUserFilter filter);

    /// <summary>
    /// User account data for deletion of own userId
    /// </summary>
    /// <param name="iamUserId"></param>
    /// <returns>SharedIdpAlias, CompanyUserId, UserEntityId, BusinessPartnerNumbers, RoleIds, OfferIds, InvitationIds</returns>
    Task<(string? SharedIdpAlias, CompanyUserAccountData AccountData)> GetSharedIdentityProviderUserAccountDataUntrackedAsync(string iamUserId);

    /// <summary>
    /// User account data for deletion of own company userIds
    /// </summary>
    /// <param name="iamUserId"></param>
    /// <returns>CompanyUserId, UserEntityId, BusinessPartnerNumbers, RoleIds, OfferIds, InvitationIds</returns>
    IAsyncEnumerable<CompanyUserAccountData> GetCompanyUserAccountDataUntrackedAsync(IEnumerable<Guid> companyUserIds, Guid companyUserId);
    
    /// <summary>
    /// Validate CompanyUser is Member of all roleIds and belongs to same company as executing user 
    /// </summary>
    /// <param name="iamUserId"></param>
    /// <param name="roleIds"></param>
    /// <param name="companyUserIdId"></param>
    /// <returns></returns>
    Task<(IEnumerable<Guid> RoleIds, bool IsSameCompany, Guid UserCompanyId)> GetRolesAndCompanyMembershipUntrackedAsync(string iamUserId, IEnumerable<Guid> roleIds, Guid companyUserId);
}
