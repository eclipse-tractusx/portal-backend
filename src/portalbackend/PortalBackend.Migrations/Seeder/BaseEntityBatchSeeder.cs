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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace  Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Seeder;

/// <summary>
/// Seeder to seed the base entities (those with an id as primary key)
/// </summary>
public class BaseEntityBatchSeeder : ICustomSeeder
{
    private readonly PortalDbContext _context;
    private readonly ILogger<BaseEntityBatchSeeder> _logger;
    private readonly SeederSettings _settings;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger</param>
    /// <param name="options">The options</param>
    public BaseEntityBatchSeeder(PortalDbContext context, ILogger<BaseEntityBatchSeeder> logger, IOptions<SeederSettings> options)
    {
        _context = context;
        _logger = logger;
        _settings = options.Value;
    }

    /// <inheritdoc />
    public int Order => 2;

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start BaseEntityBatch Seeder");
        await SeedTable<Address>("addresses", cancellationToken).ConfigureAwait(false);
        await SeedTable<Company>("companies", cancellationToken).ConfigureAwait(false);
        await SeedTable<Agreement>("agreements", cancellationToken).ConfigureAwait(false);
        await SeedTable<CompanyUser>("company_users", cancellationToken).ConfigureAwait(false);
        await SeedTable<Document>("documents", cancellationToken).ConfigureAwait(false);
        await SeedTable<Offer>("offers", cancellationToken).ConfigureAwait(false);
        await SeedTable<IamClient>("iam_clients", cancellationToken).ConfigureAwait(false);
        await SeedTable<CompanyApplication>("company_applications", cancellationToken).ConfigureAwait(false);
        await SeedTable<IdentityProvider>("identity_providers", cancellationToken).ConfigureAwait(false);
        await SeedTable<UserRoleCollection>("user_role_collections", cancellationToken).ConfigureAwait(false);
        await SeedTable<CompanyServiceAccount>("company_service_accounts", cancellationToken).ConfigureAwait(false);
        await SeedTable<UserRole>("user_roles", cancellationToken).ConfigureAwait(false);
        await SeedTable<Connector>("connectors", cancellationToken).ConfigureAwait(false);
        await SeedTable<Consent>("consents", cancellationToken).ConfigureAwait(false);
        await SeedTable<Invitation>("invitations", cancellationToken).ConfigureAwait(false);
        await SeedTable<Notification>("notifications", cancellationToken).ConfigureAwait(false);
        await SeedTable<OfferLicense>("offer_licenses", cancellationToken).ConfigureAwait(false);
        await SeedTable<ProviderCompanyDetail>("provider_company_details", cancellationToken).ConfigureAwait(false);
        await SeedTable<OfferSubscription>("offer_subscriptions", cancellationToken).ConfigureAwait(false);
        await SeedTable<AppInstance>("app_instances", cancellationToken).ConfigureAwait(false);
        await SeedTable<AppSubscriptionDetail>("app_subscription_details", cancellationToken).ConfigureAwait(false);
        await SeedTable<OfferDetailImage>("offer_detail_images", cancellationToken).ConfigureAwait(false);
        await SeedTable<UseCase>("use_cases", cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Finished BaseEntityBatch Seeder");
    }

    private async Task SeedTable<T>(string fileName, CancellationToken cancellationToken) where T : class, IBaseEntity
    {
        _logger.LogInformation("Start seeding {Filename}", fileName);
        var data = await SeederHelper.GetSeedData<T>(_logger, fileName, cancellationToken, _settings.TestDataEnvironments.ToArray()).ConfigureAwait(false);
        _logger.LogInformation("Found {ElementCount} data", data.Count);
        if (data.Any())
        {
            var typeName = typeof(T).Name;
            _logger.LogInformation("Started to Seed {TableName}", typeName);
            data = (from d in data
                join dbData in _context.Set<T>() on d.Id equals dbData.Id into t
                from x in t.DefaultIfEmpty()
                where x == null
                select d).ToList();
            _logger.LogInformation("Seeding {DataCount} {TableName}", data.Count, typeName);
            await _context.Set<T>().AddRangeAsync(data, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Seeded {TableName}", typeName);
        }
    }
}