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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

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
    public Task<(IEnumerable<UniqueIdentifierId> IdentifierIds, bool IsValidCountryCode)> GetCompanyIdentifiers(string alpha2Code) =>
        _dbContext.Countries
            .AsNoTracking()
            .Where(country => country.Alpha2Code == alpha2Code)
            .Select(country => new ValueTuple<IEnumerable<UniqueIdentifierId>, bool>
                (
                   country.CountryAssignedIdentifiers.Select(countryAssignedIdentifier => countryAssignedIdentifier.UniqueIdentifierId),
                   true
                ))
            .SingleOrDefaultAsync();

    public Task<(bool IsValidCountry, IEnumerable<(BpdmIdentifierId BpdmIdentifierId, UniqueIdentifierId UniqueIdentifierId)> Identifiers)> GetCountryAssignedIdentifiers(IEnumerable<BpdmIdentifierId> bpdmIdentifierIds, string countryAlpha2Code) =>
        _dbContext.Countries
            .AsNoTracking()
            .Where(country => country.Alpha2Code == countryAlpha2Code)
            .Select(country => new ValueTuple<bool, IEnumerable<(BpdmIdentifierId, UniqueIdentifierId)>>(
                true,
                country.CountryAssignedIdentifiers
                    .Where(identifier => identifier.BpdmIdentifierId != null && bpdmIdentifierIds.Contains(identifier.BpdmIdentifierId.Value))
                    .Select(identifier => new ValueTuple<BpdmIdentifierId, UniqueIdentifierId>(identifier.BpdmIdentifierId!.Value, identifier.UniqueIdentifierId))))
            .SingleOrDefaultAsync();

    ///<inheritdoc />
    public IAsyncEnumerable<ServiceTypeData> GetServiceTypeData() =>
        _dbContext.ServiceTypes.AsNoTracking()
            .Select(serviceType => new ServiceTypeData((int)serviceType.Id, serviceType.Label))
            .AsAsyncEnumerable();

    ///<inheritdoc />
    public IAsyncEnumerable<LicenseTypeData> GetLicenseTypeData() =>
        _dbContext.LicenseTypes.AsNoTracking()
            .Select(licenseType => new LicenseTypeData((int)licenseType.Id, licenseType.Label))
            .AsAsyncEnumerable();
}
