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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class TechnicalUserProfileRepository : ITechnicalUserProfileRepository
{
    private readonly PortalDbContext _context;

    public TechnicalUserProfileRepository(PortalDbContext dbContext)
    {
        _context = dbContext;
    }

    /// <inheritdoc />
    public Task<OfferProfileData?> GetOfferProfileData(Guid offerId, OfferTypeId offerTypeId, Guid userCompanyId) =>
        _context.Offers
            .Where(x => x.Id == offerId && x.OfferTypeId == offerTypeId)
            .Select(o => new OfferProfileData(
                o.ProviderCompanyId == userCompanyId,
                offerTypeId == OfferTypeId.SERVICE ? o.ServiceDetails.Select(sd => sd.ServiceTypeId) : null,
                o.TechnicalUserProfiles.Select(tup => new ValueTuple<Guid, IEnumerable<Guid>>(tup.Id, tup.TechnicalUserProfileAssignedUserRoles.Select(ur => ur.UserRoleId)))))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public TechnicalUserProfile CreateTechnicalUserProfile(Guid id, Guid offerId) =>
        _context.TechnicalUserProfiles.Add(new TechnicalUserProfile(id, offerId)).Entity;

    ///<inheritdoc/>
    public void CreateDeleteTechnicalUserProfileAssignedRoles(IEnumerable<(Guid TechnicalUserProfileId, Guid UserRoleId)> initialTechnicalUserProfileIdRoles, IEnumerable<(Guid TechnicalUserProfileId, Guid UserRoleId)> modifyTechnicalUserProfileIdRoles) =>
        _context.AddRemoveRange(
            initialTechnicalUserProfileIdRoles,
            modifyTechnicalUserProfileIdRoles,
            x => new TechnicalUserProfileAssignedUserRole(x.TechnicalUserProfileId, x.UserRoleId));

    /// <inheritdoc />
    public void RemoveTechnicalUserProfiles(IEnumerable<Guid> technicalUserProfileIds) =>
        _context.TechnicalUserProfiles.RemoveRange(technicalUserProfileIds.Select(profileId => new TechnicalUserProfile(profileId, Guid.Empty)));

    /// <inheritdoc />
    public void RemoveTechnicalUserProfilesForOffer(Guid offerId)
    {
        _context.TechnicalUserProfileAssignedUserRoles.RemoveRange(_context.TechnicalUserProfileAssignedUserRoles.AsNoTracking().Where(x => x.TechnicalUserProfile!.OfferId == offerId));
        _context.TechnicalUserProfiles.RemoveRange(_context.TechnicalUserProfiles.AsNoTracking().Where(x => x.OfferId == offerId));
    }

    /// <inheritdoc />
    public Task<(bool IsUserOfProvidingCompany, IEnumerable<TechnicalUserProfileInformationTransferData> Information)> GetTechnicalUserProfileInformation(Guid offerId, Guid usersCompanyId, OfferTypeId offerTypeId, IEnumerable<UserRoleConfig> externalRoles, IEnumerable<UserRoleConfig> providerOnlyRoles)
    {
        var externalRoleIds = _context.UserRoles
            .SelectMany(ur => ur.Offer!.AppInstances.Select(ai => new
            {
                ai.IamClient!.ClientClientId,
                ur.UserRoleText,
                UserRoleId = ur.Id
            }))
            .JoinTuples(
                externalRoles.SelectMany(cr => cr.UserRoleNames.Select(urn => (cr.ClientId, urn))),
                x => x.ClientClientId,
                x => x.UserRoleText)
            .Select(x => x.UserRoleId);

        var providerOnlyRoleIds = _context.UserRoles
            .SelectMany(ur => ur.Offer!.AppInstances.Select(ai => new
            {
                ai.IamClient!.ClientClientId,
                ur.UserRoleText,
                UserRoleId = ur.Id
            }))
            .JoinTuples(
                providerOnlyRoles.SelectMany(cr => cr.UserRoleNames.Select(urn => (cr.ClientId, urn))),
                x => x.ClientClientId,
                x => x.UserRoleText)
            .Select(x => x.UserRoleId);

        return _context.Offers
            .Where(x => x.Id == offerId && x.OfferTypeId == offerTypeId)
            .Select(x => new ValueTuple<bool, IEnumerable<TechnicalUserProfileInformationTransferData>>(
                x.ProviderCompanyId == usersCompanyId,
                x.TechnicalUserProfiles.Select(tup => new TechnicalUserProfileInformationTransferData(
                    tup.Id,
                    tup.TechnicalUserProfileAssignedUserRoles
                        .Select(ur => new UserRoleInformationTransferData(
                            ur.UserRole!.Id,
                            ur.UserRole.UserRoleText,
                            externalRoleIds.Contains(ur.UserRole.Id),
                            providerOnlyRoleIds.Contains(ur.UserRole.Id)
                            ))))))
            .SingleOrDefaultAsync();
    }
}
