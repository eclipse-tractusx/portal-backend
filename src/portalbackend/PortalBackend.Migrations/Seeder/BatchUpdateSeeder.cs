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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Seeder;

/// <summary>
/// Seeder to seed the base entities (those with an id as primary key)
/// </summary>
public class BatchUpdateSeeder : ICustomSeeder
{
    private readonly PortalDbContext _context;
    private readonly ILogger<BatchUpdateSeeder> _logger;
    private readonly SeederSettings _settings;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger</param>
    /// <param name="options">The options</param>
    public BatchUpdateSeeder(PortalDbContext context, ILogger<BatchUpdateSeeder> logger, IOptions<SeederSettings> options)
    {
        _context = context;
        _logger = logger;
        _settings = options.Value;
    }

    /// <inheritdoc />
    public int Order => 2;

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start BaseEntityBatch Seeder");
        await SeedTable<LanguageLongName>(
            "language_long_names",
            x => new { x.ShortName, x.LanguageShortName },
            x => x.dbEntity.LongName != x.dataEntity.LongName,
            (dbEntity, entity) =>
            {
                dbEntity.LongName = entity.LongName;
            }, cancellationToken).ConfigureAwait(false);

        await SeedTable<CompanyRoleDescription>(
            "company_role_descriptions",
            x => new { x.CompanyRoleId, x.LanguageShortName },
            x => x.dbEntity.Description != x.dataEntity.Description,
            (dbEntry, entry) =>
            {
                dbEntry.Description = entry.Description;
            }, cancellationToken).ConfigureAwait(false);

        await SeedTable<UserRoleCollectionDescription>(
            "user_role_collection_descriptions",
            x => new { x.UserRoleCollectionId, x.LanguageShortName },
            x => x.dbEntity.Description != x.dataEntity.Description,
            (dbEntry, entry) =>
            {
                dbEntry.Description = entry.Description;
            }, cancellationToken).ConfigureAwait(false);

        await SeedTable<Country>(
            "countries",
            x => x.Alpha2Code,
            x => x.dbEntity.Alpha3Code != x.dataEntity.Alpha3Code || x.dbEntity.CountryNameDe != x.dataEntity.CountryNameDe || x.dbEntity.CountryNameEn != x.dataEntity.CountryNameEn,
            (dbEntry, entry) =>
            {
                dbEntry.Alpha3Code = entry.Alpha3Code;
                dbEntry.CountryNameDe = entry.CountryNameDe;
                dbEntry.CountryNameEn = entry.CountryNameEn;
            }, cancellationToken).ConfigureAwait(false);

        await SeedTable<CompanyIdentifier>("company_identifiers",
            x => new { x.CompanyId, x.UniqueIdentifierId },
            x => x.dataEntity.Value != x.dbEntity.Value,
            (dbEntry, entry) =>
            {
                dbEntry.Value = entry.Value;
            }, cancellationToken).ConfigureAwait(false);

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Finished BaseEntityBatch Seeder");
    }

    private async Task SeedTable<T>(string fileName, Func<T, object> keySelector, Func<(T dataEntity, T dbEntity), bool> whereClause, Action<T, T> updateEntries, CancellationToken cancellationToken) where T : class
    {
        _logger.LogInformation("Start seeding {Filename}", fileName);
        var data = await SeederHelper.GetSeedData<T>(_logger, fileName, cancellationToken, _settings.TestDataEnvironments.ToArray()).ConfigureAwait(false);
        _logger.LogInformation("Found {ElementCount} data", data.Count);
        if (data.Any())
        {
            var typeName = typeof(T).Name;
            var entriesForUpdate = data
                .Join(_context.Set<T>(), keySelector, keySelector, (dataEntry, dbEntry) => new ValueTuple<T, T>(dataEntry, dbEntry))
                .Where(whereClause.Invoke)
                .ToList();
            if (entriesForUpdate.Any())
            {
                _logger.LogInformation("Started to Update {EntryCount} entries of {TableName}", entriesForUpdate.Count, typeName);
                foreach (var dbEntry in entriesForUpdate)
                {
                    updateEntries.Invoke(dbEntry.Item1, dbEntry.Item2);
                }
                _logger.LogInformation("Updated {TableName}", typeName);
            }
        }
    }
}
