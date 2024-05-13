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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;

public class TechnicalUserProfileService : ITechnicalUserProfileService
{
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    public TechnicalUserProfileService(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ServiceAccountCreationInfo>> GetTechnicalUserProfilesForOffer(Guid offerId, OfferTypeId offerTypeId)
    {
        var data = await _portalRepositories.GetInstance<IOfferRepository>()
            .GetServiceAccountProfileData(offerId, offerTypeId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (data == default)
        {
            throw new NotFoundException($"Offer {offerTypeId} {offerId} does not exists");
        }

        return CheckTechnicalUserData(data)
            ? data.ServiceAccountProfiles.Select(x => GetServiceAccountData(data.OfferName!, x))
            : Enumerable.Empty<ServiceAccountCreationInfo>();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ServiceAccountCreationInfo>> GetTechnicalUserProfilesForOfferSubscription(Guid subscriptionId)
    {
        var data = await _portalRepositories.GetInstance<IOfferRepository>()
            .GetServiceAccountProfileDataForSubscription(subscriptionId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (data == default)
        {
            throw new NotFoundException($"Offer Subscription {subscriptionId} does not exists");
        }

        return CheckTechnicalUserData(data)
            ? data.ServiceAccountProfiles.Select(x => GetServiceAccountData(data.OfferName!, x))
            : Enumerable.Empty<ServiceAccountCreationInfo>();
    }

    private static bool CheckTechnicalUserData((bool IsSingleInstance, IEnumerable<IEnumerable<UserRoleData>> ServiceAccountProfiles, string? OfferName) data)
    {
        if (string.IsNullOrWhiteSpace(data.OfferName))
        {
            throw new ConflictException("Offer name needs to be set here");
        }

        return !data.IsSingleInstance && data.ServiceAccountProfiles.Any();
    }

    private static ServiceAccountCreationInfo GetServiceAccountData(string offerName, IEnumerable<UserRoleData> serviceAccountUserRoles) =>
        new(
            offerName,
            $"Technical User for app {offerName} - {string.Join(",", serviceAccountUserRoles.Select(x => x.UserRoleText))}",
            IamClientAuthMethod.SECRET,
            serviceAccountUserRoles.Select(x => x.UserRoleId));
}
