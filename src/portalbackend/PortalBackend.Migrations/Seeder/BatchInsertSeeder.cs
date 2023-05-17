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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Seeder;

/// <summary>
/// Seeder to seed the base entities (those with an id as primary key)
/// </summary>
public class BatchInsertSeeder : ICustomSeeder
{
	private readonly PortalDbContext _context;
	private readonly ILogger<BatchInsertSeeder> _logger;
	private readonly SeederSettings _settings;

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="context">The database context</param>
	/// <param name="logger">The logger</param>
	/// <param name="options">The options</param>
	public BatchInsertSeeder(PortalDbContext context, ILogger<BatchInsertSeeder> logger, IOptions<SeederSettings> options)
	{
		_context = context;
		_logger = logger;
		_settings = options.Value;
	}

	/// <inheritdoc />
	public int Order => 1;

	/// <inheritdoc />
	public async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Start BaseEntityBatch Seeder");
		await SeedTable<Language>("languages", x => x.ShortName, cancellationToken).ConfigureAwait(false);
		await SeedBaseEntity(cancellationToken);

		await SeedTable<AgreementAssignedCompanyRole>("agreement_assigned_company_roles", x => new { x.AgreementId, x.CompanyRoleId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<AgreementAssignedOfferType>("agreement_assigned_offer_types", x => new { x.AgreementId, x.OfferTypeId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<AgreementAssignedOffer>("agreement_assigned_offers", x => new { x.AgreementId, x.OfferId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<AppAssignedUseCase>("app_assigned_use_cases", x => new { x.AppId, x.UseCaseId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<AppLanguage>("app_languages", x => new { x.AppId, x.LanguageShortName }, cancellationToken).ConfigureAwait(false);
		await SeedTable<ApplicationChecklistEntry>("application_checklist", x => new { x.ApplicationId, x.ApplicationChecklistEntryTypeId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<CompanyAssignedRole>("company_assigned_roles", x => new { x.CompanyId, x.CompanyRoleId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<CompanyAssignedUseCase>("company_assigned_use_cases", x => new { x.CompanyId, x.UseCaseId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<CompanyIdentityProvider>("company_identity_providers", x => new { x.CompanyId, x.IdentityProviderId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<IamIdentityProvider>("IamIdentityProvider", x => x.IamIdpAlias, cancellationToken).ConfigureAwait(false);
		await SeedTable<CompanyRoleAssignedRoleCollection>("company_role_assigned_role_collections", x => x.CompanyRoleId, cancellationToken).ConfigureAwait(false);
		await SeedTable<CompanyRoleRegistrationData>("company_role_registration_data", x => x.CompanyRoleId, cancellationToken).ConfigureAwait(false);
		await SeedTable<CompanyRoleDescription>("company_role_descriptions", x => new { x.CompanyRoleId, x.LanguageShortName }, cancellationToken).ConfigureAwait(false);
		await SeedTable<CompanyUserAssignedAppFavourite>("company_user_assigned_app_favourites", x => new { x.CompanyUserId, x.AppId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<CompanyUserAssignedBusinessPartner>("company_user_assigned_business_partners", x => new { x.CompanyUserId, x.BusinessPartnerNumber }, cancellationToken).ConfigureAwait(false);
		await SeedTable<CompanyUserAssignedRole>("company_user_assigned_roles", x => new { x.CompanyUserId, x.UserRoleId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<CompanyServiceAccountAssignedRole>("company_service_account_assigned_roles", x => new { x.CompanyServiceAccountId, x.UserRoleId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<ConsentAssignedOffer>("consent_assigned_offers", x => new { x.OfferId, x.ConsentId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<ConsentAssignedOfferSubscription>("consent_assigned_offer_subscriptions", x => new { x.OfferSubscriptionId, x.ConsentId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<Country>("countries", x => x.Alpha2Code, cancellationToken).ConfigureAwait(false);
		await SeedTable<OfferAssignedDocument>("offer_assigned_documents", x => new { x.OfferId, x.DocumentId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<IamIdentityProvider>("iam_identity_providers", x => x.IamIdpAlias, cancellationToken).ConfigureAwait(false);
		await SeedTable<IamServiceAccount>("iam_service_accounts", x => x.ClientId, cancellationToken).ConfigureAwait(false);
		await SeedTable<IamUser>("iam_users", x => x.UserEntityId, cancellationToken).ConfigureAwait(false);
		await SeedTable<OfferAssignedLicense>("offer_assigned_licenses", x => new { x.OfferId, x.OfferLicenseId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<OfferDescription>("offer_descriptions", x => new { AppId = x.OfferId, x.LanguageShortName }, cancellationToken).ConfigureAwait(false);
		await SeedTable<OfferTag>("offer_tags", x => new { AppId = x.OfferId, x.Name }, cancellationToken).ConfigureAwait(false);
		await SeedTable<ServiceDetail>("service_details", x => new { x.ServiceId, x.ServiceTypeId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<UserRoleAssignedCollection>("user_role_assigned_collections", x => new { x.UserRoleId, x.UserRoleCollectionId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<UserRoleCollectionDescription>("user_role_collection_descriptions", x => new { x.UserRoleCollectionId, x.LanguageShortName }, cancellationToken).ConfigureAwait(false);
		await SeedTable<UserRoleDescription>("user_role_descriptions", x => new { x.UserRoleId, x.LanguageShortName }, cancellationToken).ConfigureAwait(false);
		await SeedTable<NotificationTypeAssignedTopic>("notification_type_assigned_topic", x => new { x.NotificationTypeId, x.NotificationTopicId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<CompanyIdentifier>("company_identifiers", x => new { x.CompanyId, x.UniqueIdentifierId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<CountryAssignedIdentifier>("country_assigned_identifiers", x => new { x.CountryAlpha2Code, x.UniqueIdentifierId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<OfferAssignedPrivacyPolicy>("offer_assigned_privacy_policies", x => new { x.OfferId, x.PrivacyPolicyId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<AppInstanceAssignedCompanyServiceAccount>("app_instance_assigned_company_service_accounts", x => new { x.AppInstanceId, x.CompanyServiceAccountId }, cancellationToken).ConfigureAwait(false);
		await SeedTable<TechnicalUserProfileAssignedUserRole>("technical_user_profile_assigned_user_roles", x => new { x.TechnicalUserProfileId, x.UserRoleId }, cancellationToken).ConfigureAwait(false);

		await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		_logger.LogInformation("Finished BaseEntityBatch Seeder");
	}

	private async Task SeedBaseEntity(CancellationToken cancellationToken)
	{
		await SeedTableForBaseEntity<Address>("addresses", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<Company>("companies", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<Agreement>("agreements", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<CompanyUser>("company_users", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<Document>("documents", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<Offer>("offers", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<IamClient>("iam_clients", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<CompanyApplication>("company_applications", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<IdentityProvider>("identity_providers", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<UserRoleCollection>("user_role_collections", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<CompanyServiceAccount>("company_service_accounts", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<UserRole>("user_roles", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<Connector>("connectors", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<Consent>("consents", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<Invitation>("invitations", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<Notification>("notifications", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<OfferLicense>("offer_licenses", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<ProviderCompanyDetail>("provider_company_details", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<OfferSubscription>("offer_subscriptions", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<AppInstance>("app_instances", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<AppSubscriptionDetail>("app_subscription_details", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<UseCase>("use_cases", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<ProcessStep>("process_steps", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<Process>("processes", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<AppInstanceSetup>("app_instance_setups", cancellationToken).ConfigureAwait(false);
		await SeedTableForBaseEntity<TechnicalUserProfile>("technical_user_profiles", cancellationToken).ConfigureAwait(false);

	}

	private async Task SeedTableForBaseEntity<T>(string fileName, CancellationToken cancellationToken) where T : class, IBaseEntity
	{
		await SeedTable<T>(fileName, x => x.Id, cancellationToken).ConfigureAwait(false);
	}

	private async Task SeedTable<T>(string fileName, Func<T, object> keySelector, CancellationToken cancellationToken) where T : class
	{
		_logger.LogInformation("Start seeding {Filename}", fileName);
		var data = await SeederHelper.GetSeedData<T>(_logger, fileName, cancellationToken, _settings.TestDataEnvironments.ToArray()).ConfigureAwait(false);
		_logger.LogInformation("Found {ElementCount} data", data.Count);
		if (data.Any())
		{
			var typeName = typeof(T).Name;
			_logger.LogInformation("Started to Seed {TableName}", typeName);
			data = data.GroupJoin(_context.Set<T>(), keySelector, keySelector, (d, dbEntry) => new { d, dbEntry })
				.SelectMany(t => t.dbEntry.DefaultIfEmpty(), (t, x) => new { t, x })
				.Where(t => t.x == null)
				.Select(t => t.t.d).ToList();
			_logger.LogInformation("Seeding {DataCount} {TableName}", data.Count, typeName);
			await _context.Set<T>().AddRangeAsync(data, cancellationToken).ConfigureAwait(false);

			_logger.LogInformation("Seeded {TableName}", typeName);
		}
	}
}
