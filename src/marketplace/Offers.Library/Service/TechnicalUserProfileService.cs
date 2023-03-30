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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;

public class TechnicalUserProfileService : ITechnicalUserProfileService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly TechnicalUserProfileSettings _settings;

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="options">Access to the settings</param>
    public TechnicalUserProfileService(IPortalRepositories portalRepositories,
        IOptions<TechnicalUserProfileSettings> options)
    {
        _portalRepositories = portalRepositories;
        _settings = options.Value;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ServiceAccountCreationInfo> GetTechnicalUserProfilesForOffer(Guid offerId)
    {
        // TODO (PS): refactor, request technical user profile from database for a specific offer 
        var data = await _portalRepositories.GetInstance<IOfferRepository>()
            .GetServiceAccountProfileData(offerId)
            .ConfigureAwait(false);
        if (data == default)
        {
            throw new NotFoundException($"Offer {offerId} does not exists");
        }

        var serviceAccountData = await GetServiceAccountData(data).ConfigureAwait(false);
        if (serviceAccountData != null)
            yield return serviceAccountData;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ServiceAccountCreationInfo> GetTechnicalUserProfilesForOfferSubscription(Guid subscriptionId)
    {
        // TODO (PS): refactor, request technical user profile from database for a specific offer 
        var data = await _portalRepositories.GetInstance<IOfferRepository>()
            .GetServiceAccountProfileDataForSubscription(subscriptionId)
            .ConfigureAwait(false);
        if (data == default)
        {
            throw new NotFoundException($"Offer Subscription {subscriptionId} does not exists");
        }

        var serviceAccountData = await GetServiceAccountData(data).ConfigureAwait(false);
        if (serviceAccountData != null)
            yield return serviceAccountData;
    }

    private async ValueTask<ServiceAccountCreationInfo?> GetServiceAccountData((bool IsSingleInstance, bool TechnicalUserNeeded, string? OfferName) data)
    {
        if (string.IsNullOrWhiteSpace(data.OfferName))
        {
            throw new ConflictException("Offer name needs to be set here");
        }

        if (data is {IsSingleInstance: false, TechnicalUserNeeded: false})
        {
            return null;
        }

        var serviceAccountUserRoles = await _portalRepositories.GetInstance<IUserRolesRepository>()
            .GetUserRoleDataUntrackedAsync(_settings.ServiceAccountRoles)
            .ToListAsync()
            .ConfigureAwait(false);

        var description =
            $"Technical User for app {data.OfferName} - {string.Join(",", serviceAccountUserRoles.Select(x => x.UserRoleText))}";
        var serviceAccountCreationData = new ServiceAccountCreationInfo(
            data.OfferName,
            description,
            IamClientAuthMethod.SECRET,
            serviceAccountUserRoles.Select(x => x.UserRoleId));
        return serviceAccountCreationData;
    }
}
