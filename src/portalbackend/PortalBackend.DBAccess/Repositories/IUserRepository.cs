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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for User Management on persistence layer.
/// </summary>
public interface IUserRepository
{
    IAsyncEnumerable<CompanyApplicationWithStatus> GetApplicationsWithStatusUntrackedAsync(Guid userCompanyId);
    CompanyUser CreateCompanyUser(Guid identityId, string? firstName, string? lastName, string email, Guid lastEditorId);
    Identity CreateIdentity(Guid companyId, UserStatusId userStatusId);
    void AttachAndModifyCompanyUser(Guid companyUserId, Action<CompanyUser>? initialize, Action<CompanyUser> setOptionalParameters);
    IQueryable<CompanyUser> GetOwnCompanyUserQuery(Guid companyId, Guid? companyUserId = null, string? userEntityId = null, string? firstName = null, string? lastName = null, string? email = null, IEnumerable<UserStatusId>? statusIds = null);
    Task<(string UserEntityId, string? FirstName, string? LastName, string? Email)> GetUserEntityDataAsync(Guid companyUserId, Guid companyId);
    IAsyncEnumerable<(string? UserEntityId, Guid CompanyUserId)> GetMatchingCompanyIamUsersByNameEmail(string firstName, string lastName, string email, Guid companyId, IEnumerable<UserStatusId> companyUserStatusIds);
    Task<(CompanyInformationData companyInformation, string? userEmail)> GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(Guid companyUserId);
    Task<bool> IsOwnCompanyUserWithEmailExisting(string email, Guid companyUserId);
    Task<CompanyUserDetails?> GetOwnCompanyUserDetailsUntrackedAsync(Guid companyUserId, Guid userCompanyId);
    Task<CompanyUserBusinessPartners?> GetOwnCompanyUserWithAssignedBusinessPartnerNumbersUntrackedAsync(Guid companyUserId, Guid userId);
    Task<(Guid CompanyId, string? Bpn, IEnumerable<Guid> TechnicalUserRoleIds)> GetCompanyIdAndBpnRolesForIamUserUntrackedAsync(Guid companyUserId, string technicalUserClientId);

    Task<CompanyOwnUserDetails?> GetUserDetailsUntrackedAsync(Guid companyUserId, IEnumerable<Guid> userRoleIds);
    Task<CompanyUserWithIdpBusinessPartnerData?> GetUserWithCompanyIdpAsync(Guid companyUserId);

    /// <summary>
    /// GGets all apps for the give user from the persistence layer.
    /// </summary>
    /// <param name="companyUserId">Id of the user which apps should be selected.</param>
    /// <returns>Returns an IAsyncEnumerable of GUIDs</returns>
    IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserUntrackedAsync(Guid companyUserId);

    /// <summary>
    /// Gets all company user ids which have the any given user role assigned
    /// </summary>
    /// <param name="userRoleIds">User role ids</param>
    /// <param name="companyId">Id of the company for the users to select</param>
    /// <returns>Returns a list of the company user ids</returns>
    IAsyncEnumerable<Guid> GetCompanyUserWithRoleIdForCompany(IEnumerable<Guid> userRoleIds, Guid companyId);

    /// <summary>
    /// Gets all company user ids which have the any given user role assigned
    /// </summary>
    /// <param name="userRoleIds">User role ids</param>
    /// <returns>Returns a list of the company user ids</returns>
    IAsyncEnumerable<Guid> GetCompanyUserWithRoleId(IEnumerable<Guid> userRoleIds);

    /// <summary>
    /// Gets all company user emails which have the given user role assigned
    /// </summary>
    /// <param name="userRoleIds">User role ids</param>
    /// <param name="companyId">Id of the company for the users to select</param>
    /// <returns>Returns a list of the company user emails</returns>
    IAsyncEnumerable<(string Email, string? FirstName, string? LastName)> GetCompanyUserEmailForCompanyAndRoleId(IEnumerable<Guid> userRoleIds, Guid companyId);

    Task<OfferIamUserData?> GetAppAssignedIamClientUserDataUntrackedAsync(Guid offerId, Guid companyUserId, Guid userCompanyId);

    Task<CoreOfferIamUserData?> GetCoreOfferAssignedIamClientUserDataUntrackedAsync(Guid offerId, Guid companyUserId, Guid userCompanyId);

    IAsyncEnumerable<Guid> GetServiceProviderCompanyUserWithRoleIdAsync(Guid offerId, List<Guid> userRoleIds);

    Func<int, int, Task<Pagination.Source<CompanyAppUserDetails>?>> GetOwnCompanyAppUsersPaginationSourceAsync(Guid appId, Guid companyUserId, IEnumerable<OfferSubscriptionStatusId> subscriptionStatusIds, IEnumerable<UserStatusId> companyUserStatusIds, CompanyUserFilter filter);

    /// <summary>
    /// User account data for deletion of own userId
    /// </summary>
    /// <param name="companyUserId"></param>
    /// <returns>SharedIdpAlias, CompanyUserId, UserEntityId, BusinessPartnerNumbers, RoleIds, OfferIds, InvitationIds</returns>
    Task<(string? SharedIdpAlias, CompanyUserAccountData AccountData)> GetSharedIdentityProviderUserAccountDataUntrackedAsync(Guid companyUserId);

    /// <summary>
    /// User account data for deletion of own company userIds
    /// </summary>
    /// <param name="companyUserIds"></param>
    /// <param name="companyUserId"></param>
    /// <returns>CompanyUserId, UserEntityId, BusinessPartnerNumbers, RoleIds, OfferIds, InvitationIds</returns>
    IAsyncEnumerable<CompanyUserAccountData> GetCompanyUserAccountDataUntrackedAsync(IEnumerable<Guid> companyUserIds, Guid companyUserId);

    /// <summary>
    /// Validate CompanyUser is Member of all roleIds and belongs to same company as executing user 
    /// </summary>
    /// <param name="userCompanyId"></param>
    /// <param name="roleIds"></param>
    /// <param name="companyUserId"></param>
    /// <returns></returns>
    Task<(IEnumerable<Guid> RoleIds, bool IsSameCompany, Guid UserCompanyId)> GetRolesAndCompanyMembershipUntrackedAsync(Guid userCompanyId, IEnumerable<Guid> roleIds, Guid companyUserId);

    /// <summary>
    /// Retrieve BPN for applicationId and Logged In User Company
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="businessPartnerNumber"></param>
    /// <returns></returns>
    IAsyncEnumerable<(bool IsApplicationCompany, bool IsApplicationPending, string? BusinessPartnerNumber, Guid CompanyId)> GetBpnForIamUserUntrackedAsync(Guid applicationId, string businessPartnerNumber);

    /// <summary>
    /// Gets the users company bpn for the user
    /// </summary>
    /// <param name="companyUserId">Id of the user</param>
    /// <returns>The bpn of the company for the user</returns>
    Task<string?> GetCompanyBpnForIamUserAsync(Guid companyUserId);

    Task<IdentityData?> GetActiveUserDataByUserEntityId(string userEntityId);
    Identity AttachAndModifyIdentity(Guid identityId, Action<Identity>? initialize, Action<Identity> modify);

    /// <summary>
    /// Gets the company user ids and checks if its the given iamUser
    /// </summary>
    /// <param name="userId">Id of the user</param>
    /// <param name="companyUserId">The id of the company user to check in the persistence layer.</param>
    /// <returns><c>true</c> if the user exists, otherwise <c>false</c></returns>
    IAsyncEnumerable<(Guid CompanyUserId, bool IsIamUser)> GetCompanyUserWithIamUserCheck(Guid userId, Guid companyUserId);
}
