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

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public interface ICompanyUserAssignedAppFavouritesRepository
{
    /// <summary>
    /// Removes the app with the given appId for the companyUser from the database
    /// </summary>
    /// <param name="appId">Id of the app that should be removed</param>
    /// <param name="companyUserId">Id of the company user</param>
    void RemoveFavouriteAppForUser(Guid appId, Guid companyUserId);

    /// <summary>
    /// Adds the given app favourite to the database
    /// </summary>
    /// <param name="appFavourite">The appFavourite that should be added to the database</param>
    void AddAppFavourite(CompanyUserAssignedAppFavourite appFavourite);
}
