/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for User Management on persistence layer.
/// </summary>
public interface IUserRepository
{
    IAsyncEnumerable<CompanyApplicationWithStatus> GetApplicationsWithStatusUntrackedAsync(string iamUserId);
    Task<RegistrationData?> GetRegistrationDataUntrackedAsync(Guid applicationId, string iamUserId);
    CompanyUser CreateCompanyUser(string? firstName, string? lastName, string email, Guid companyId, CompanyUserStatusId companyUserStatusId, Guid lastEditorId);
    CompanyUser AttachAndModifyCompanyUser(Guid companyUserId, Action<CompanyUser>? setOptionalParameters = null);
    IamUser CreateIamUser(Guid companyUserId, string iamUserId);
    IamUser RemoveIamUser(IamUser iamUser);
    IQueryable<CompanyUser> GetOwnCompanyUserQuery(string adminUserId, Guid? companyUserId = null, string? userEntityId = null, string? firstName = null, string? lastName = null, string? email = null);
    Task<(string UserEntityId, string? FirstName, string? LastName, string? Email)> GetUserEntityDataAsync(Guid companyUserId, Guid companyId);
    IAsyncEnumerable<(string? UserEntityId, Guid CompanyUserId)> GetMatchingCompanyIamUsersByNameEmail(string firstName, string lastName, string email, Guid companyId);
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
    Task<CompanyUserWithIdpData?> GetUserWithSharedIdpDataAsync(string iamUserId);
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

    /// <summary>
    /// Gets the company user id and email for the given iam user
    /// </summary>
    /// <remarks><b>Returns as UNTRACKED</b></remarks>
    /// <param name="userId">id of the iamUser</param>
    /// <returns>Returns the userId and email</returns>
    Task<(Guid UserId, string Email)> GetCompanyUserIdAndEmailForIamUserUntrackedAsync(string userId);

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

    Task<(string? IamClientId, string IamUserId, bool IsSameCompany)> GetAppAssignedIamClientUserDataUntrackedAsync(Guid offerId, Guid companyUserId, string iamUserId);
}
