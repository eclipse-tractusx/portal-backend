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

public interface ITechnicalUserProfileRepository
{
    /// <summary>
    /// Gets the profile offer data for the given offer id and user
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="offerTypeId">OfferTypeId</param>
    /// <param name="userCompanyId">The id of the users company</param>
    /// <returns>Returns the offer profile data</returns>
    Task<OfferProfileData?> GetOfferProfileData(Guid offerId, OfferTypeId offerTypeId, Guid userCompanyId);

    /// <summary>
    /// Creates the technical user profile for an offer
    /// </summary>
    /// <param name="id">Id of the technical user profile</param>
    /// <param name="offerId">Id of the offer</param>
    TechnicalUserProfile CreateTechnicalUserProfile(Guid id, Guid offerId);

    /// <summary>
    /// Creates and deletes the technical user profile assigned roles
    /// </summary>
    /// <param name="initialTechnicalUserProfileIdRoles">the initial tuples of technicalUserProfileId and UserRoleId</param>
    /// <param name="modifyTechnicalUserProfileIdRoles">the new set of tuples of technicalUserProfileId and UserRoleId</param>
    void CreateDeleteTechnicalUserProfileAssignedRoles(IEnumerable<(Guid TechnicalUserProfileId, Guid UserRoleId)> initialTechnicalUserProfileIdRoles, IEnumerable<(Guid TechnicalUserProfileId, Guid UserRoleId)> modifyTechnicalUserProfileIdRoles);

    /// <summary>
    /// Removes the technical user profiles
    /// </summary>
    /// <param name="technicalUserProfilesIds">The ids of the profiles to delete</param>
    void RemoveTechnicalUserProfiles(IEnumerable<Guid> technicalUserProfileIds);

    /// <summary>
    /// Removes the technical user profiles and the assigned roles
    /// </summary>
    /// <param name="offerId">The id of the offer to remove the technical user profiles for</param>
    void RemoveTechnicalUserProfilesForOffer(Guid offerId);

    /// <summary>
    /// Gets the technical user profiles for a given offer
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="usersCompanyId"></param>
    /// <param name="offerTypeId">Id of the offertype</param>
    /// <returns>List of the technical user profile information</returns>
    Task<(bool IsUserOfProvidingCompany, IEnumerable<TechnicalUserProfileInformation> Information)> GetTechnicalUserProfileInformation(Guid offerId, Guid usersCompanyId, OfferTypeId offerTypeId);
}
