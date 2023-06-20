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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class CountryRepository : ICountryRepository
{
    private readonly PortalDbContext _portalDbContext;

    /// <summary>
    /// Creates an instance of <see cref="CountryRepository"/>
    /// </summary>
    /// <param name="portalDbContext">The database</param>
    public CountryRepository(PortalDbContext portalDbContext)
    {
        _portalDbContext = portalDbContext;
    }

    /// <inheritdoc />
    public Task<bool> CheckCountryExistsByAlpha2CodeAsync(string alpha2Code) =>
        _portalDbContext.Countries.AnyAsync(x => x.Alpha2Code == alpha2Code);

    public Task<(bool IsValidCountry, IEnumerable<UniqueIdentifierId> UniqueIdentifierIds)> GetCountryAssignedIdentifiers(string alpha2Code, IEnumerable<UniqueIdentifierId> uniqueIdentifierIds) =>
        _portalDbContext.Countries
            .AsNoTracking()
            .Where(country => country.Alpha2Code == alpha2Code)
            .Select(country => new ValueTuple<bool, IEnumerable<UniqueIdentifierId>>(
                true,
                country.CountryAssignedIdentifiers
                    .Where(assignedIdentifier => uniqueIdentifierIds.Contains(assignedIdentifier.UniqueIdentifierId))
                    .Select(assignedIdentifier => assignedIdentifier.UniqueIdentifierId)
            ))
            .SingleOrDefaultAsync();
}
