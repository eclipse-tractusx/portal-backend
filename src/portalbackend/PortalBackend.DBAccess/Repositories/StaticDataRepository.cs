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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// Implementation of <see cref="IStaticDataRepository"/> accessing database with EF Core.
public class StaticDataRepository : IStaticDataRepository
{
    private readonly PortalDbContext _dbContext;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="portalDbContext">PortalDb context.</param>
    public StaticDataRepository(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<UseCaseData> GetAllUseCase() =>
        _dbContext.UseCases
            .AsNoTracking()
            .Select(s => new UseCaseData(s.Id, s.Name, s.Shortname))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<LanguageData> GetAllLanguage() =>
        _dbContext.Languages
            .AsNoTracking()
            .Select(lang => new LanguageData
                (
                    lang.ShortName,
                    new LanguageDataLongNames
                    (
                         lang.LongNameDe,
                         lang.LongNameEn
                    )
                ))
            .AsAsyncEnumerable();
    
    ///<inheritdoc />
    public Task<(IEnumerable<UniqueIdentifierData> IdentifierData, bool IsCountryCodeExist)> GetCompanyIdentifiers(string alpha2Code) =>
        _dbContext.Countries
            .AsNoTracking()
            .Where(country => country.Alpha2Code == alpha2Code)
            .Select(country => new ValueTuple<IEnumerable<UniqueIdentifierData>, bool>
                (
                   country.CountryAssignedIdentifiers.Select(countryAssignedIdentifier => new UniqueIdentifierData((int)countryAssignedIdentifier.UniqueIdentifier!.Id, countryAssignedIdentifier.UniqueIdentifier!.Label)),
                   true
                ))
            .SingleOrDefaultAsync();
    
}
