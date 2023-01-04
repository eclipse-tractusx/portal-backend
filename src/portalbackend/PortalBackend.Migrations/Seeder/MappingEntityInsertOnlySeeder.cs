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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace  Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Seeder;

/// <summary>
/// Seeder to seed the mapping entities (those with a combined primary key)
/// </summary>
public class MappingEntityInsertOnlySeeder : ICustomSeeder
{
    private readonly PortalDbContext _context;
    private readonly ILogger<MappingEntityInsertOnlySeeder> _logger;
    private readonly SeederSettings _settings;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger</param>
    /// <param name="options">The options</param>
    public MappingEntityInsertOnlySeeder(PortalDbContext context, ILogger<MappingEntityInsertOnlySeeder> logger, IOptions<SeederSettings> options)
    {
        _context = context;
        _logger = logger;
        _settings = options.Value;
    }

    /// <inheritdoc />
    public int Order => 3;

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await SeedData<AgreementAssignedCompanyRole>("agreement_assigned_company_roles", cancellationToken).ConfigureAwait(false);
        await SeedData<AgreementAssignedDocument>("agreement_assigned_documents", cancellationToken).ConfigureAwait(false);
        await SeedData<AgreementAssignedOfferType>("agreement_assigned_offer_types", cancellationToken).ConfigureAwait(false);
        await SeedData<AgreementAssignedOffer>("agreement_assigned_offers", cancellationToken).ConfigureAwait(false);
        await SeedData<AppAssignedUseCase>("app_assigned_use_cases", cancellationToken).ConfigureAwait(false);
        await SeedData<AppLanguage>("app_languages", cancellationToken).ConfigureAwait(false);
        await SeedData<CompanyAssignedRole>("company_assigned_roles", cancellationToken).ConfigureAwait(false);
        await SeedData<CompanyAssignedUseCase>("company_assigned_use_cases", cancellationToken).ConfigureAwait(false);
        await SeedData<CompanyIdentityProvider>("company_identity_providers", cancellationToken).ConfigureAwait(false);
        await SeedData<IamIdentityProvider>("IamIdentityProvider", cancellationToken).ConfigureAwait(false);
        await SeedData<CompanyRoleAssignedRoleCollection>("company_role_assigned_role_collections", cancellationToken).ConfigureAwait(false);
        await SeedData<CompanyRoleRegistrationData>("company_role_registration_data", cancellationToken).ConfigureAwait(false);
        await SeedData<CompanyRoleDescription>("company_role_descriptions", cancellationToken).ConfigureAwait(false);
        await SeedData<CompanyUserAssignedAppFavourite>("company_user_assigned_app_favourites", cancellationToken).ConfigureAwait(false);
        await SeedData<CompanyUserAssignedBusinessPartner>("company_user_assigned_business_partners", cancellationToken).ConfigureAwait(false);
        await SeedData<CompanyUserAssignedRole>("company_user_assigned_roles", cancellationToken).ConfigureAwait(false);
        await SeedData<CompanyServiceAccountAssignedRole>("company_service_account_assigned_roles", cancellationToken).ConfigureAwait(false);
        await SeedData<ConsentAssignedOffer>("consent_assigned_offers", cancellationToken).ConfigureAwait(false);
        await SeedData<ConsentAssignedOfferSubscription>("consent_assigned_offer_subscriptions", cancellationToken).ConfigureAwait(false);
        await SeedData<Country>("countries", cancellationToken).ConfigureAwait(false);
        await SeedData<OfferAssignedDocument>("offer_assigned_documents", cancellationToken).ConfigureAwait(false);
        await SeedData<IamIdentityProvider>("iam_identity_providers", cancellationToken).ConfigureAwait(false);
        await SeedData<IamServiceAccount>("iam_service_accounts", cancellationToken).ConfigureAwait(false);
        await SeedData<IamUser>("iam_users", cancellationToken).ConfigureAwait(false);
        await SeedData<OfferAssignedLicense>("offer_assigned_licenses", cancellationToken).ConfigureAwait(false);
        await SeedData<OfferDescription>("offer_descriptions", cancellationToken).ConfigureAwait(false);
        await SeedData<OfferTag>("offer_tags", cancellationToken).ConfigureAwait(false);
        await SeedData<ServiceAssignedServiceType>("service_assigned_service_types", cancellationToken).ConfigureAwait(false);
        await SeedData<UserRoleAssignedCollection>("user_role_assigned_collections", cancellationToken).ConfigureAwait(false);
        await SeedData<UserRoleCollectionDescription>("user_role_collection_descriptions", cancellationToken).ConfigureAwait(false);
        await SeedData<UserRoleDescription>("user_role_descriptions", cancellationToken).ConfigureAwait(false);
        await SeedData<NotificationTypeAssignedTopic>("notification_type_assigned_topic", cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task SeedData<T>(string filename, CancellationToken cancellationToken) where T : class
    {
        var data = await SeederHelper.GetSeedData<T>(_logger, filename, cancellationToken, _settings.TestDataEnvironments.ToArray()).ConfigureAwait(false);
        if (data.Any())
        {
            var typeName = typeof(T).Name;
            _logger.LogInformation("Started to Seed {TableName}", typeName);
            if (!await _context.Set<T>().AnyAsync(cancellationToken))
            {
                await _context.Set<T>().AddRangeAsync(data, cancellationToken).ConfigureAwait(false);
            }

            _logger.LogInformation("Seeded {TableName}", typeName);
        }
    }
}