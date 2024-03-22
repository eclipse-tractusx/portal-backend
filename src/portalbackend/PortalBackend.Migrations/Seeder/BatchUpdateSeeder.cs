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
        if (!_settings.DataPaths.Any())
        {
            _logger.LogInformation("There a no data paths configured, therefore the {SeederName} will be skipped", nameof(BatchUpdateSeeder));
            return;
        }

        _logger.LogInformation("Start BaseEntityBatch Seeder");
        await SeedTable<LanguageLongName>(
            "language_long_names",
            x => new { x.ShortName, x.LanguageShortName },
            x => x.dbEntity.LongName != x.dataEntity.LongName,
            (dbEntity, entity) =>
            {
                dbEntity.LongName = entity.LongName;
            }, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        await SeedTable<CompanyRoleDescription>(
            "company_role_descriptions",
            x => new { x.CompanyRoleId, x.LanguageShortName },
            x => x.dbEntity.Description != x.dataEntity.Description,
            (dbEntry, entry) =>
            {
                dbEntry.Description = entry.Description;
            }, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        await SeedTable<UserRoleCollectionDescription>(
            "user_role_collection_descriptions",
            x => new { x.UserRoleCollectionId, x.LanguageShortName },
            x => x.dbEntity.Description != x.dataEntity.Description,
            (dbEntry, entry) =>
            {
                dbEntry.Description = entry.Description;
            }, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        await SeedTable<CompanyCertificateTypeAssignedStatus>(
            "company_certificate_type_assigned_statuses",
            x => new { x.CompanyCertificateTypeId, x.CompanyCertificateTypeStatusId },
            x => x.dbEntity.CompanyCertificateTypeStatusId != x.dataEntity.CompanyCertificateTypeStatusId,
            (dbEntry, entry) =>
            {
                dbEntry.CompanyCertificateTypeStatusId = entry.CompanyCertificateTypeStatusId;
            }, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        await SeedTable<CompanyCertificateTypeDescription>(
            "company_certificate_type_descriptions",
            x => new { x.CompanyCertificateTypeId, x.LanguageShortName },
            x => x.dbEntity.Description != x.dataEntity.Description,
            (dbEntry, entry) =>
            {
                dbEntry.Description = entry.Description;
            }, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        await SeedTable<Country>(
            "countries",
            x => x.Alpha2Code,
            x => x.dbEntity.Alpha3Code != x.dataEntity.Alpha3Code,
            (dbEntry, entry) =>
            {
                dbEntry.Alpha3Code = entry.Alpha3Code;
            }, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        await SeedTable<CompanyIdentifier>("company_identifiers",
            x => new { x.CompanyId, x.UniqueIdentifierId },
            x => x.dataEntity.Value != x.dbEntity.Value,
            (dbEntry, entry) =>
            {
                dbEntry.Value = entry.Value;
            }, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        await SeedTable<VerifiedCredentialExternalTypeUseCaseDetailVersion>("verified_credential_external_type_use_case_detail_versions",
            x => x.Id,
            x => x.dataEntity.Template != x.dbEntity.Template || x.dataEntity.Expiry != x.dbEntity.Expiry || x.dataEntity.ValidFrom != x.dbEntity.ValidFrom,
            (dbEntry, entry) =>
            {
                dbEntry.Template = entry.Template;
                dbEntry.Expiry = entry.Expiry;
                dbEntry.ValidFrom = entry.ValidFrom;
            }, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        await SeedTable<CompanyServiceAccount>("company_service_accounts",
            x => x.Id,
            x => x.dataEntity.Description != x.dbEntity.Description || x.dataEntity.Name != x.dbEntity.Name || x.dataEntity.ClientClientId != x.dbEntity.ClientClientId,
            (dbEntry, entry) =>
            {
                dbEntry.Description = entry.Description;
                dbEntry.Name = entry.Name;
                dbEntry.ClientClientId = entry.ClientClientId;
            }, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        await SeedTable<Company>("companies",
            x => x.Id,
            x => x.dbEntity.SelfDescriptionDocumentId == null && x.dataEntity.SelfDescriptionDocumentId != x.dbEntity.SelfDescriptionDocumentId,
            (dbEntry, entry) =>
            {
                dbEntry.SelfDescriptionDocumentId = entry.SelfDescriptionDocumentId;
            }, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        _logger.LogInformation("Finished BaseEntityBatch Seeder");
    }

    private async Task SeedTable<T>(string fileName, Func<T, object> keySelector, Func<(T dataEntity, T dbEntity), bool> whereClause, Action<T, T> updateEntries, CancellationToken cancellationToken) where T : class
    {
        _logger.LogInformation("Start seeding {Filename}", fileName);
        var additionalEnvironments = _settings.TestDataEnvironments ?? Enumerable.Empty<string>();
        var data = await SeederHelper.GetSeedData<T>(_logger, fileName, _settings.DataPaths, cancellationToken, additionalEnvironments.ToArray()).ConfigureAwait(ConfigureAwaitOptions.None);
        _logger.LogInformation("Found {ElementCount} data", data.Count);
        if (data.Any())
        {
            var typeName = typeof(T).Name;
            var entriesForUpdate = data
                .Join(_context.Set<T>(), keySelector, keySelector, (dataEntry, dbEntry) => (DataEntry: dataEntry, DbEntry: dbEntry))
                .Where(whereClause.Invoke)
                .ToList();
            if (entriesForUpdate.Any())
            {
                _logger.LogInformation("Started to Update {EntryCount} entries of {TableName}", entriesForUpdate.Count, typeName);
                foreach (var entry in entriesForUpdate)
                {
                    updateEntries.Invoke(entry.DbEntry, entry.DataEntry);
                }
                _logger.LogInformation("Updated {TableName}", typeName);
            }
        }
    }
}
