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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.AuditEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Views;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;

/// <summary>
/// Db Context
/// </summary>
/// <remarks>
/// The Trigger Framework requires new Guid() to convert it to gen_random_uuid(),
/// for the Id field we'll use a randomly set UUID to satisfy SonarCloud.
/// </remarks>
public class PortalDbContext : DbContext
{
    protected PortalDbContext()
    {
    }

    public PortalDbContext(DbContextOptions<PortalDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; } = default!;
    public virtual DbSet<Agreement> Agreements { get; set; } = default!;
    public virtual DbSet<AgreementAssignedCompanyRole> AgreementAssignedCompanyRoles { get; set; } = default!;
    public virtual DbSet<AgreementAssignedOffer> AgreementAssignedOffers { get; set; } = default!;
    public virtual DbSet<AgreementAssignedOfferType> AgreementAssignedOfferTypes { get; set; } = default!;
    public virtual DbSet<AgreementCategory> AgreementCategories { get; set; } = default!;
    public virtual DbSet<AppInstance> AppInstances { get; set; } = default!;

    public virtual DbSet<AppInstanceAssignedCompanyServiceAccount> AppInstanceAssignedServiceAccounts { get; set; } = default!;
    public virtual DbSet<AppInstanceSetup> AppInstanceSetups { get; set; } = default!;
    public virtual DbSet<AppAssignedUseCase> AppAssignedUseCases { get; set; } = default!;
    public virtual DbSet<AppLanguage> AppLanguages { get; set; } = default!;
    public virtual DbSet<ApplicationChecklistEntry> ApplicationChecklist { get; set; } = default!;
    public virtual DbSet<ApplicationChecklistEntryStatus> ApplicationChecklistStatuses { get; set; } = default!;
    public virtual DbSet<ApplicationChecklistEntryType> ApplicationChecklistTypes { get; set; } = default!;
    public virtual DbSet<AppSubscriptionDetail> AppSubscriptionDetails { get; set; } = default!;
    public virtual DbSet<AuditAppSubscriptionDetail20221118> AuditAppSubscriptionDetail20221118 { get; set; } = default!;
    public virtual DbSet<AuditOffer20230119> AuditOffer20230119 { get; set; } = default!;
    public virtual DbSet<AuditOffer20230406> AuditOffer20230406 { get; set; } = default!;
    public virtual DbSet<AuditOfferSubscription20221005> AuditOfferSubscription20221005 { get; set; } = default!;
    public virtual DbSet<AuditOfferSubscription20230317> AuditOfferSubscription20230317 { get; set; } = default!;
    public virtual DbSet<AuditCompanyApplication20221005> AuditCompanyApplication20221005 { get; set; } = default!;
    public virtual DbSet<AuditCompanyApplication20230214> AuditCompanyApplication20230214 { get; set; } = default!;
    public virtual DbSet<AuditCompanySsiDetail20230621> AuditCompanySsiDetail20230621 { get; set; } = default!;
    public virtual DbSet<AuditCompanyUser20221005> AuditCompanyUser20221005 { get; set; } = default!;
    public virtual DbSet<AuditCompanyUser20230522> AuditCompanyUser20230523 { get; set; } = default!;
    public virtual DbSet<AuditConnector20230405> AuditConnector20230405 { get; set; } = default!;
    public virtual DbSet<AuditConnector20230503> AuditConnector20230503 { get; set; } = default!;
    public virtual DbSet<AuditIdentity20230526> AuditIdentity20230526 { get; set; } = default!;
    public virtual DbSet<AuditUserRole20221017> AuditUserRole20221017 { get; set; } = default!;
    public virtual DbSet<AuditCompanyUserAssignedRole20221018> AuditCompanyUserAssignedRole20221018 { get; set; } = default!;
    public virtual DbSet<AuditCompanyAssignedRole2023316> AuditCompanyAssignedRole2023316 { get; set; } = default!;
    public virtual DbSet<AuditConsent20230412> AuditConsent20230412 { get; set; } = default!;
    public virtual DbSet<AuditIdentityAssignedRole20230522> AuditIdentityAssignedRole20230522 { get; set; } = default!;
    public virtual DbSet<AuditProviderCompanyDetail20230614> AuditProviderCompanyDetail20230614 { get; set; } = default!;
    public virtual DbSet<BpdmIdentifier> BpdmIdentifiers { get; set; } = default!;
    public virtual DbSet<Company> Companies { get; set; } = default!;
    public virtual DbSet<CompanyApplication> CompanyApplications { get; set; } = default!;
    public virtual DbSet<CompanyApplicationStatus> CompanyApplicationStatuses { get; set; } = default!;
    public virtual DbSet<CompanyAssignedRole> CompanyAssignedRoles { get; set; } = default!;
    public virtual DbSet<CompanyAssignedUseCase> CompanyAssignedUseCases { get; set; } = default!;
    public virtual DbSet<CompanyIdentifier> CompanyIdentifiers { get; set; } = default!;
    public virtual DbSet<CompanyIdentityProvider> CompanyIdentityProviders { get; set; } = default!;
    public virtual DbSet<CompanyRoleAssignedRoleCollection> CompanyRoleAssignedRoleCollections { get; set; } = default!;
    public virtual DbSet<CompanyRoleDescription> CompanyRoleDescriptions { get; set; } = default!;
    public virtual DbSet<CompanyRole> CompanyRoles { get; set; } = default!;
    public virtual DbSet<CompanyServiceAccount> CompanyServiceAccounts { get; set; } = default!;
    public virtual DbSet<CompanyServiceAccountType> CompanyServiceAccountTypes { get; set; } = default!;
    public virtual DbSet<CompanySsiDetail> CompanySsiDetails { get; set; } = default!;
    public virtual DbSet<CompanySsiDetailStatus> CompanySsiDetailStatuses { get; set; } = default!;
    public virtual DbSet<CompanyStatus> CompanyStatuses { get; set; } = default!;
    public virtual DbSet<CompanyUser> CompanyUsers { get; set; } = default!;
    public virtual DbSet<CompanyUserAssignedAppFavourite> CompanyUserAssignedAppFavourites { get; set; } = default!;
    public virtual DbSet<CompanyUserAssignedBusinessPartner> CompanyUserAssignedBusinessPartners { get; set; } = default!;
    public virtual DbSet<Connector> Connectors { get; set; } = default!;
    public virtual DbSet<ConnectorAssignedOfferSubscription> ConnectorAssignedOfferSubscriptions { get; set; } = default!;
    public virtual DbSet<ConnectorClientDetail> ConnectorClientDetails { get; set; } = default!;
    public virtual DbSet<ConnectorStatus> ConnectorStatuses { get; set; } = default!;
    public virtual DbSet<ConnectorType> ConnectorTypes { get; set; } = default!;
    public virtual DbSet<Consent> Consents { get; set; } = default!;
    public virtual DbSet<ConsentAssignedOffer> ConsentAssignedOffers { get; set; } = default!;
    public virtual DbSet<ConsentAssignedOfferSubscription> ConsentAssignedOfferSubscriptions { get; set; } = default!;
    public virtual DbSet<ConsentStatus> ConsentStatuses { get; set; } = default!;
    public virtual DbSet<Country> Countries { get; set; } = default!;
    public virtual DbSet<CountryAssignedIdentifier> CountryAssignedIdentifiers { get; set; } = default!;
    public virtual DbSet<Document> Documents { get; set; } = default!;
    public virtual DbSet<DocumentType> DocumentTypes { get; set; } = default!;
    public virtual DbSet<DocumentStatus> DocumentStatus { get; set; } = default!;
    public virtual DbSet<IamClient> IamClients { get; set; } = default!;
    public virtual DbSet<IamIdentityProvider> IamIdentityProviders { get; set; } = default!;
    public virtual DbSet<Identity> Identities { get; set; } = default!;
    public virtual DbSet<IdentityAssignedRole> IdentityAssignedRoles { get; set; } = default!;
    public virtual DbSet<IdentityProvider> IdentityProviders { get; set; } = default!;
    public virtual DbSet<IdentityProviderCategory> IdentityProviderCategories { get; set; } = default!;
    public virtual DbSet<IdentityUserStatus> IdentityUserStatuses { get; set; } = default!;
    public virtual DbSet<Invitation> Invitations { get; set; } = default!;
    public virtual DbSet<InvitationStatus> InvitationStatuses { get; set; } = default!;
    public virtual DbSet<Language> Languages { get; set; } = default!;
    public virtual DbSet<LanguageLongName> LanguageLongNames { get; set; } = default!;
    public virtual DbSet<LicenseType> LicenseTypes { get; set; } = default!;
    public virtual DbSet<MediaType> MediaTypes { get; set; } = default!;
    public virtual DbSet<Notification> Notifications { get; set; } = default!;
    public virtual DbSet<NotificationTypeAssignedTopic> NotificationTypeAssignedTopics { get; set; } = default!;
    public virtual DbSet<Offer> Offers { get; set; } = default!;
    public virtual DbSet<OfferAssignedDocument> OfferAssignedDocuments { get; set; } = default!;
    public virtual DbSet<OfferAssignedLicense> OfferAssignedLicenses { get; set; } = default!;
    public virtual DbSet<OfferAssignedPrivacyPolicy> OfferAssignedPrivacyPolicies { get; set; } = default!;
    public virtual DbSet<OfferDescription> OfferDescriptions { get; set; } = default!;
    public virtual DbSet<OfferLicense> OfferLicenses { get; set; } = default!;
    public virtual DbSet<OfferStatus> OfferStatuses { get; set; } = default!;
    public virtual DbSet<OfferTag> OfferTags { get; set; } = default!;
    public virtual DbSet<OfferType> OfferTypes { get; set; } = default!;
    public virtual DbSet<OfferSubscription> OfferSubscriptions { get; set; } = default!;
    public virtual DbSet<OfferSubscriptionStatus> OfferSubscriptionStatuses { get; set; } = default!;
    public virtual DbSet<OfferSubscriptionProcessData> OfferSubscriptionsProcessDatas { get; set; } = default!;
    public virtual DbSet<Process> Processes { get; set; } = default!;
    public virtual DbSet<ProcessStep> ProcessSteps { get; set; } = default!;
    public virtual DbSet<ProcessStepStatus> ProcessStepStatuses { get; set; } = default!;
    public virtual DbSet<ProcessStepType> ProcessStepTypes { get; set; } = default!;
    public virtual DbSet<ProcessType> ProcessTypes { get; set; } = default!;
    public virtual DbSet<ProviderCompanyDetail> ProviderCompanyDetails { get; set; } = default!;
    public virtual DbSet<PrivacyPolicy> PrivacyPolicies { get; set; } = default!;
    public virtual DbSet<ServiceDetail> ServiceDetails { get; set; } = default!;
    public virtual DbSet<ServiceType> ServiceTypes { get; set; } = default!;
    public virtual DbSet<TechnicalUserProfile> TechnicalUserProfiles { get; set; } = default!;
    public virtual DbSet<TechnicalUserProfileAssignedUserRole> TechnicalUserProfileAssignedUserRoles { get; set; } = default!;
    public virtual DbSet<UniqueIdentifier> UniqueIdentifiers { get; set; } = default!;
    public virtual DbSet<UseCase> UseCases { get; set; } = default!;
    public virtual DbSet<UseCaseDescription> UseCaseDescriptions { get; set; } = default!;
    public virtual DbSet<UserRole> UserRoles { get; set; } = default!;
    public virtual DbSet<UserRoleAssignedCollection> UserRoleAssignedCollections { get; set; } = default!;
    public virtual DbSet<UserRoleCollection> UserRoleCollections { get; set; } = default!;
    public virtual DbSet<UserRoleCollectionDescription> UserRoleCollectionDescriptions { get; set; } = default!;
    public virtual DbSet<UserRoleDescription> UserRoleDescriptions { get; set; } = default!;
    public virtual DbSet<VerifiedCredentialExternalType> VerifiedCredentialExternalTypes { get; set; } = default!;
    public virtual DbSet<VerifiedCredentialExternalTypeUseCaseDetailVersion> VerifiedCredentialExternalTypeUseCaseDetailVersions { get; set; } = default!;
    public virtual DbSet<VerifiedCredentialType> VerifiedCredentialTypes { get; set; } = default!;
    public virtual DbSet<VerifiedCredentialTypeAssignedExternalType> VerifiedCredentialTypeAssignedExternalTypes { get; set; } = default!;
    public virtual DbSet<VerifiedCredentialTypeKind> VerifiedCredentialTypeKinds { get; set; } = default!;
    public virtual DbSet<VerifiedCredentialTypeAssignedKind> VerifiedCredentialTypeAssignedKinds { get; set; } = default!;
    public virtual DbSet<VerifiedCredentialTypeAssignedUseCase> VerifiedCredentialTypeAssignedUseCases { get; set; } = default!;
    public virtual DbSet<CompaniesLinkedServiceAccount> CompanyLinkedServiceAccounts { get; set; } = default!;

    public virtual DbSet<OfferSubscriptionView> OfferSubscriptionView { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "en_US.utf8");
        modelBuilder.HasDefaultSchema("portal");

        modelBuilder.Entity<Agreement>(entity =>
        {
            entity.HasOne(d => d.AgreementCategory)
                .WithMany(p => p!.Agreements)
                .HasForeignKey(d => d.AgreementCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IssuerCompany)
                .WithMany(p => p!.Agreements)
                .HasForeignKey(d => d.IssuerCompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AgreementAssignedCompanyRole>(entity =>
        {
            entity.HasKey(e => new { e.AgreementId, e.CompanyRoleId });

            entity.HasOne(d => d.Agreement)
                .WithMany(p => p!.AgreementAssignedCompanyRoles)
                .HasForeignKey(d => d.AgreementId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.CompanyRole)
                .WithMany(p => p!.AgreementAssignedCompanyRoles!)
                .HasForeignKey(d => d.CompanyRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AgreementAssignedOffer>(entity =>
        {
            entity.HasKey(e => new { e.AgreementId, e.OfferId });

            entity.HasOne(d => d.Agreement)
                .WithMany(p => p!.AgreementAssignedOffers)
                .HasForeignKey(d => d.AgreementId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Offer)
                .WithMany(p => p!.AgreementAssignedOffers!)
                .HasForeignKey(d => d.OfferId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AgreementAssignedOfferType>(entity =>
        {
            entity.HasKey(e => new { e.AgreementId, e.OfferTypeId });

            entity.HasOne(d => d.Agreement)
                .WithMany(p => p!.AgreementAssignedOfferTypes)
                .HasForeignKey(d => d.AgreementId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.OfferType)
                .WithMany(p => p.AgreementAssignedOfferTypes)
                .HasForeignKey(d => d.OfferTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AgreementCategory>()
            .HasData(
                Enum.GetValues(typeof(AgreementCategoryId))
                    .Cast<AgreementCategoryId>()
                    .Select(e => new AgreementCategory(e))
            );

        modelBuilder.Entity<ConsentAssignedOffer>(entity =>
        {
            entity.HasKey(e => new { e.ConsentId, e.OfferId });

            entity.HasOne(d => d.Consent)
                .WithMany(p => p!.ConsentAssignedOffers)
                .HasForeignKey(d => d.ConsentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Offer)
                .WithMany(p => p!.ConsentAssignedOffers!)
                .HasForeignKey(d => d.OfferId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Offer>(entity =>
        {
            entity.HasOne(d => d.ProviderCompany)
                .WithMany(p => p!.ProvidedOffers);

            entity.HasOne(x => x.SalesManager)
                .WithMany(x => x!.SalesManagerOfOffers)
                .HasForeignKey(x => x.SalesManagerId);

            entity.HasOne(x => x.OfferType)
                .WithMany(x => x!.Offers)
                .HasForeignKey(x => x.OfferTypeId);

            entity.HasMany(p => p.Companies)
                .WithMany(p => p.BoughtOffers)
                .UsingEntity<OfferSubscription>(
                    j => j
                        .HasOne(d => d.Company!)
                        .WithMany()
                        .HasForeignKey(d => d.CompanyId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.Offer!)
                        .WithMany()
                        .HasForeignKey(d => d.OfferId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => e.Id);
                        j.HasOne(e => e.OfferSubscriptionStatus)
                            .WithMany(e => e.OfferSubscriptions)
                            .HasForeignKey(e => e.OfferSubscriptionStatusId)
                            .OnDelete(DeleteBehavior.ClientSetNull);
                        j.HasOne(e => e.Requester)
                            .WithMany(e => e.RequestedSubscriptions)
                            .HasForeignKey(e => e.RequesterId)
                            .OnDelete(DeleteBehavior.ClientSetNull);
                        j.Property(e => e.OfferSubscriptionStatusId)
                            .HasDefaultValue(OfferSubscriptionStatusId.PENDING);

                        j.HasAuditV1Triggers<OfferSubscription, AuditOfferSubscription20230317>();
                    }
                );

            entity.HasMany(a => a.SupportedLanguages)
                .WithMany(l => l.SupportingApps)
                .UsingEntity<AppLanguage>(
                    j => j
                        .HasOne(d => d.Language!)
                        .WithMany()
                        .HasForeignKey(d => d.LanguageShortName)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.App!)
                        .WithMany()
                        .HasForeignKey(d => d.AppId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { e.AppId, e.LanguageShortName });
                    }
                );

            entity.HasMany(p => p.OfferLicenses)
                .WithMany(p => p.Offers)
                .UsingEntity<OfferAssignedLicense>(
                    j => j
                        .HasOne(d => d.OfferLicense!)
                        .WithMany()
                        .HasForeignKey(d => d.OfferLicenseId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.Offer!)
                        .WithMany()
                        .HasForeignKey(d => d.OfferId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { AppId = e.OfferId, AppLicenseId = e.OfferLicenseId });
                    });

            entity.HasMany(p => p.UseCases)
                .WithMany(p => p.Apps)
                .UsingEntity<AppAssignedUseCase>(
                    j => j
                        .HasOne(d => d.UseCase!)
                        .WithMany()
                        .HasForeignKey(d => d.UseCaseId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.App!)
                        .WithMany()
                        .HasForeignKey(d => d.AppId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { e.AppId, e.UseCaseId });
                    });

            entity.HasMany(p => p.Documents)
                .WithMany(p => p.Offers)
                .UsingEntity<OfferAssignedDocument>(
                    j => j
                        .HasOne(d => d.Document!)
                        .WithMany()
                        .HasForeignKey(d => d.DocumentId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.Offer!)
                        .WithMany()
                        .HasForeignKey(d => d.OfferId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { e.OfferId, e.DocumentId });
                    });

            entity.HasMany(p => p.OfferSubscriptions)
                .WithOne(d => d.Offer)
                .HasForeignKey(d => d.OfferId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<Offer, AuditOffer20230406>();
        });

        modelBuilder.Entity<OfferSubscriptionProcessData>(entity =>
        {
            entity.HasKey(x => x.OfferSubscriptionId);

            entity.HasOne(x => x.OfferSubscription)
                .WithOne(x => x.OfferSubscriptionProcessData)
                .HasForeignKey<OfferSubscriptionProcessData>(x => x.OfferSubscriptionId);
        });

        modelBuilder.Entity<AppSubscriptionDetail>(entity =>
        {
            entity.HasOne(e => e.AppInstance)
                .WithMany(e => e.AppSubscriptionDetails)
                .HasForeignKey(e => e.AppInstanceId);

            entity.HasOne(e => e.OfferSubscription)
                .WithOne(e => e.AppSubscriptionDetail)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<AppSubscriptionDetail, AuditAppSubscriptionDetail20221118>();
        });

        modelBuilder.Entity<OfferType>()
            .HasData(
                Enum.GetValues(typeof(OfferTypeId))
                    .Cast<OfferTypeId>()
                    .Select(e => new OfferType(e))
            );

        modelBuilder.Entity<ServiceType>()
            .HasData(
                Enum.GetValues(typeof(ServiceTypeId))
                    .Cast<ServiceTypeId>()
                    .Select(e => new ServiceType(e))
            );

        modelBuilder.Entity<AppInstance>(entity =>
        {
            entity.HasOne(x => x.App)
                .WithMany(x => x.AppInstances)
                .HasForeignKey(x => x.AppId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.IamClient)
                .WithMany(x => x.AppInstances)
                .HasForeignKey(x => x.IamClientId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AppInstanceAssignedCompanyServiceAccount>(entity =>
        {
            entity.HasKey(x => new { x.AppInstanceId, x.CompanyServiceAccountId });
            entity.HasOne(x => x.AppInstance)
                .WithMany(x => x.ServiceAccounts)
                .HasForeignKey(x => x.AppInstanceId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(x => x.CompanyServiceAccount)
                .WithMany(x => x.AppInstances)
                .HasForeignKey(x => x.CompanyServiceAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AppInstanceSetup>(entity =>
        {
            entity.HasOne(x => x.App)
                .WithOne(x => x.AppInstanceSetup)
                .HasForeignKey<AppInstanceSetup>(x => x.AppId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<OfferDescription>(entity =>
        {
            entity.HasKey(e => new { AppId = e.OfferId, e.LanguageShortName });

            entity.HasOne(d => d.Offer)
                .WithMany(p => p!.OfferDescriptions)
                .HasForeignKey(d => d.OfferId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Language)
                .WithMany(p => p!.AppDescriptions)
                .HasForeignKey(d => d.LanguageShortName)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<OfferStatus>()
            .HasData(
                Enum.GetValues(typeof(OfferStatusId))
                    .Cast<OfferStatusId>()
                    .Select(e => new OfferStatus(e))
            );

        modelBuilder.Entity<OfferTag>(entity =>
        {
            entity.HasKey(e => new { AppId = e.OfferId, e.Name });

            entity.HasOne(d => d.Offer)
                .WithMany(p => p!.Tags)
                .HasForeignKey(d => d.OfferId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AuditOperation>()
            .HasData(
                Enum.GetValues(typeof(AuditOperationId))
                    .Cast<AuditOperationId>()
                    .Select(e => new AuditOperation(e))
            );

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasMany(p => p.OfferSubscriptions)
                .WithOne(d => d.Company)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(p => p.SelfDescriptionDocument)
                .WithMany(d => d.Companies)
                .HasForeignKey(d => d.SelfDescriptionDocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasMany(p => p.IdentityProviders)
                .WithMany(p => p.Companies)
                .UsingEntity<CompanyIdentityProvider>(
                    j => j
                        .HasOne(pt => pt.IdentityProvider!)
                        .WithMany()
                        .HasForeignKey(pt => pt.IdentityProviderId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(pt => pt.Company!)
                        .WithMany()
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasForeignKey(pt => pt.CompanyId),
                    j =>
                    {
                        j.HasKey(e => new { e.CompanyId, e.IdentityProviderId });
                    }
                );
        });

        modelBuilder.Entity<CompanyAssignedUseCase>(entity =>
        {
            entity.HasKey(e => new { e.CompanyId, e.UseCaseId });

            entity.HasOne(d => d.Company)
                .WithMany(p => p.CompanyAssignedUseCase)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UseCase)
                .WithMany(p => p.CompanyAssignedUseCase)
                .HasForeignKey(d => d.UseCaseId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ProviderCompanyDetail>(entity =>
        {
            entity.HasOne(e => e.Company)
                .WithOne(e => e.ProviderCompanyDetail)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<ProviderCompanyDetail, AuditProviderCompanyDetail20230614>();
        });

        modelBuilder.Entity<CompanyApplication>(entity =>
        {
            entity.HasOne(d => d.Company)
                .WithMany(p => p!.CompanyApplications)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<CompanyApplication, AuditCompanyApplication20230214>();
        });

        modelBuilder.Entity<CompanyApplicationStatus>()
            .HasData(
                Enum.GetValues(typeof(CompanyApplicationStatusId))
                    .Cast<CompanyApplicationStatusId>()
                    .Select(e => new CompanyApplicationStatus(e))
            );

        modelBuilder.Entity<ApplicationChecklistEntry>(entity =>
        {
            entity.HasKey(x => new { x.ApplicationId, ChecklistEntryTypeId = x.ApplicationChecklistEntryTypeId });

            entity.HasOne(ace => ace.Application)
                .WithMany(a => a.ApplicationChecklistEntries)
                .HasForeignKey(ace => ace.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ApplicationChecklistEntryStatus>()
            .HasData(
                Enum.GetValues(typeof(ApplicationChecklistEntryStatusId))
                    .Cast<ApplicationChecklistEntryStatusId>()
                    .Select(e => new ApplicationChecklistEntryStatus(e))
            );

        modelBuilder.Entity<ApplicationChecklistEntryType>()
            .HasData(
                Enum.GetValues(typeof(ApplicationChecklistEntryTypeId))
                    .Cast<ApplicationChecklistEntryTypeId>()
                    .Select(e => new ApplicationChecklistEntryType(e))
            );

        modelBuilder.Entity<CompanyIdentityProvider>()
            .HasOne(d => d.IdentityProvider)
            .WithMany(p => p.CompanyIdentityProviders)
            .HasForeignKey(d => d.IdentityProviderId);

        modelBuilder.Entity<CompanyRole>()
            .HasData(
                Enum.GetValues(typeof(CompanyRoleId))
                    .Cast<CompanyRoleId>()
                    .Select(e => new CompanyRole(e))
            );

        modelBuilder.Entity<CompanyRoleAssignedRoleCollection>();

        modelBuilder.Entity<CompanyRoleDescription>(entity =>
        {
            entity.HasKey(e => new { e.CompanyRoleId, e.LanguageShortName });
        });

        modelBuilder.Entity<CompanyRoleRegistrationData>();

        modelBuilder.Entity<CompanyAssignedRole>(entity =>
        {
            entity.HasKey(e => new { e.CompanyId, e.CompanyRoleId });
            entity.HasAuditV1Triggers<CompanyAssignedRole, AuditCompanyAssignedRole2023316>();

            entity.HasOne(d => d.Company!)
                .WithMany(p => p.CompanyAssignedRoles)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.CompanyRole!)
                .WithMany(p => p.CompanyAssignedRoles)
                .HasForeignKey(d => d.CompanyRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<UserRole>()
            .HasAuditV1Triggers<UserRole, AuditUserRole20221017>();

        modelBuilder.Entity<UserRoleCollection>(entity =>
        {
            entity.HasMany(p => p.UserRoles)
                .WithMany(p => p.UserRoleCollections)
                .UsingEntity<UserRoleAssignedCollection>(
                    j => j
                        .HasOne(d => d.UserRole)
                        .WithMany()
                        .HasForeignKey(d => d.UserRoleId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.UserRoleCollection)
                        .WithMany()
                        .HasForeignKey(d => d.UserRoleCollectionId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { e.UserRoleId, e.UserRoleCollectionId });
                    });
        });

        modelBuilder.Entity<UserRoleCollectionDescription>(entity =>
        {
            entity.HasKey(e => new { e.UserRoleCollectionId, e.LanguageShortName });
        });

        modelBuilder.Entity<UserRoleDescription>().HasKey(e => new { e.UserRoleId, e.LanguageShortName });

        modelBuilder.Entity<CompanyStatus>()
            .HasData(
                Enum.GetValues(typeof(CompanyStatusId))
                    .Cast<CompanyStatusId>()
                    .Select(e => new CompanyStatus(e))
            );

        modelBuilder.Entity<CompanyServiceAccountType>()
            .HasData(
                Enum.GetValues(typeof(CompanyServiceAccountTypeId))
                    .Cast<CompanyServiceAccountTypeId>()
                    .Select(e => new CompanyServiceAccountType(e))
            );

        modelBuilder.Entity<IdentityType>()
            .HasData(
                Enum.GetValues(typeof(IdentityTypeId))
                    .Cast<IdentityTypeId>()
                    .Select(e => new IdentityType(e))
            );

        modelBuilder.Entity<Identity>(entity =>
        {
            entity.HasIndex(e => e.UserEntityId)
                .IsUnique();

            entity.HasOne(d => d.Company)
                .WithMany(p => p.Identities)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.IdentityStatus)
                .WithMany(e => e.Identities)
                .HasForeignKey(e => e.UserStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.IdentityType)
                .WithMany(e => e.Identities)
                .HasForeignKey(e => e.IdentityTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<Identity, AuditIdentity20230526>();
        });

        modelBuilder.Entity<CompanyUser>(entity =>
        {
            entity.HasOne(x => x.Identity)
                .WithOne(x => x.CompanyUser)
                .HasForeignKey<CompanyUser>(x => x.Id);

            entity.HasMany(p => p.Offers)
                .WithMany(p => p.CompanyUsers)
                .UsingEntity<CompanyUserAssignedAppFavourite>(
                    j => j
                        .HasOne(d => d.App)
                        .WithMany()
                        .HasForeignKey(d => d.AppId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.CompanyUser)
                        .WithMany()
                        .HasForeignKey(d => d.CompanyUserId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { e.CompanyUserId, e.AppId });
                    });
            entity.HasMany(p => p.CompanyUserAssignedBusinessPartners)
                .WithOne(d => d.CompanyUser);

            entity.HasAuditV1Triggers<CompanyUser, AuditCompanyUser20230522>();
            entity.ToTable("company_users");
        });

        modelBuilder.Entity<CompanyServiceAccount>(entity =>
        {
            entity.HasOne(x => x.Identity)
                .WithOne(x => x.CompanyServiceAccount)
                .HasForeignKey<CompanyServiceAccount>(x => x.Id);

            entity.HasOne(d => d.CompanyServiceAccountType)
                .WithMany(p => p.CompanyServiceAccounts)
                .HasForeignKey(d => d.CompanyServiceAccountTypeId);

            entity.HasOne(d => d.OfferSubscription)
                .WithMany(p => p.CompanyServiceAccounts)
                .HasForeignKey(d => d.OfferSubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasIndex(e => e.ClientClientId)
                .IsUnique();

            entity.ToTable("company_service_accounts");
        });

        modelBuilder.Entity<IdentityAssignedRole>(entity =>
        {
            entity.HasKey(e => new { IdentityId = e.IdentityId, e.UserRoleId });

            entity
                .HasOne(d => d.UserRole)
                .WithMany(e => e.IdentityAssignedRoles)
                .HasForeignKey(d => d.UserRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity
                .HasOne(d => d.Identity)
                .WithMany(e => e.IdentityAssignedRoles)
                .HasForeignKey(d => d.IdentityId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasAuditV1Triggers<IdentityAssignedRole, AuditIdentityAssignedRole20230522>();
        });

        modelBuilder.Entity<CompanyUserAssignedBusinessPartner>()
            .HasKey(e => new { e.CompanyUserId, e.BusinessPartnerNumber });

        modelBuilder.Entity<IdentityUserStatus>()
            .HasData(
                Enum.GetValues(typeof(UserStatusId))
                    .Cast<UserStatusId>()
                    .Select(e => new IdentityUserStatus(e))
            );

        modelBuilder.Entity<Consent>(entity =>
        {
            entity.HasOne(d => d.Agreement)
                .WithMany(p => p!.Consents)
                .HasForeignKey(d => d.AgreementId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Company)
                .WithMany(p => p!.Consents)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.CompanyUser)
                .WithMany(p => p!.Consents)
                .HasForeignKey(d => d.CompanyUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ConsentStatus)
                .WithMany(p => p!.Consents)
                .HasForeignKey(d => d.ConsentStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<Consent, AuditConsent20230412>();
        });

        modelBuilder.Entity<ConsentAssignedOfferSubscription>(entity =>
        {
            entity.HasKey(e => new { e.ConsentId, e.OfferSubscriptionId });

            entity.HasOne(d => d.Consent)
                .WithMany(p => p!.ConsentAssignedOfferSubscriptions)
                .HasForeignKey(d => d.ConsentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.OfferSubscription)
                .WithMany(p => p!.ConsentAssignedOfferSubscriptions)
                .HasForeignKey(d => d.OfferSubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ConsentStatus>()
            .HasData(
                Enum.GetValues(typeof(ConsentStatusId))
                    .Cast<ConsentStatusId>()
                    .Select(e => new ConsentStatus(e))
            );

        modelBuilder.Entity<Country>(entity =>
        {
            entity.Property(e => e.Alpha2Code)
                .IsFixedLength();

            entity.Property(e => e.Alpha3Code)
                .IsFixedLength();
        });

        modelBuilder.Entity<CountryAssignedIdentifier>(entity =>
        {
            entity.HasKey(e => new { e.CountryAlpha2Code, e.UniqueIdentifierId });

            entity.HasOne(d => d.Country)
                .WithMany(p => p!.CountryAssignedIdentifiers)
                .HasForeignKey(d => d.CountryAlpha2Code)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UniqueIdentifier)
                .WithMany(p => p!.CountryAssignedIdentifiers)
                .HasForeignKey(d => d.UniqueIdentifierId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<DocumentType>()
            .HasData(
                Enum.GetValues(typeof(DocumentTypeId))
                    .Cast<DocumentTypeId>()
                    .Select(e => new DocumentType(e))
            );

        modelBuilder.Entity<MediaType>()
            .HasData(
                Enum.GetValues(typeof(MediaTypeId))
                    .Cast<MediaTypeId>()
                    .Select(e => new MediaType(e))
            );

        modelBuilder.Entity<DocumentStatus>()
            .HasData(
                Enum.GetValues(typeof(DocumentStatusId))
                    .Cast<DocumentStatusId>()
                    .Select(e => new DocumentStatus(e))
            );

        modelBuilder.Entity<IamClient>().HasIndex(e => e.ClientClientId).IsUnique();

        modelBuilder.Entity<IamIdentityProvider>()
            .HasOne(d => d.IdentityProvider)
                .WithOne(p => p!.IamIdentityProvider!)
                .HasForeignKey<IamIdentityProvider>(d => d.IdentityProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<IdentityProvider>()
            .HasOne(d => d.IdentityProviderCategory)
                .WithMany(p => p!.IdentityProviders)
                .HasForeignKey(d => d.IdentityProviderCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<IdentityProviderCategory>()
            .HasData(
                Enum.GetValues(typeof(IdentityProviderCategoryId))
                    .Cast<IdentityProviderCategoryId>()
                    .Select(e => new IdentityProviderCategory(e))
            );

        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasOne(d => d.CompanyUser)
                .WithMany(p => p!.Invitations)
                .HasForeignKey(d => d.CompanyUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.InvitationStatus)
                .WithMany(p => p!.Invitations)
                .HasForeignKey(d => d.InvitationStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<InvitationStatus>()
            .HasData(
                Enum.GetValues(typeof(InvitationStatusId))
                    .Cast<InvitationStatusId>()
                    .Select(e => new InvitationStatus(e))
            );

        modelBuilder.Entity<Connector>(entity =>
        {
            entity.HasOne(d => d.SelfDescriptionDocument)
                .WithOne(p => p.Connector!)
                .HasForeignKey<Connector>(d => d.SelfDescriptionDocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Status)
                .WithMany(p => p.Connectors)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Type)
                .WithMany(p => p.Connectors)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Provider)
                .WithMany(p => p.ProvidedConnectors);

            entity.HasOne(d => d.Host)
                .WithMany(p => p.HostedConnectors);

            entity.HasOne(d => d.CompanyServiceAccount)
                .WithOne(p => p.Connector)
                .HasForeignKey<Connector>(d => d.CompanyServiceAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<Connector, AuditConnector20230503>();
        });

        modelBuilder.Entity<ConnectorClientDetail>(entity =>
        {
            entity.HasKey(x => x.ConnectorId);

            entity.HasOne(c => c.Connector)
                .WithOne(c => c.ClientDetails)
                .HasForeignKey<ConnectorClientDetail>(c => c.ConnectorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ConnectorStatus>()
            .HasData(
                Enum.GetValues(typeof(ConnectorStatusId))
                    .Cast<ConnectorStatusId>()
                    .Select(e => new ConnectorStatus(e))
            );

        modelBuilder.Entity<ConnectorType>()
            .HasData(
                Enum.GetValues(typeof(ConnectorTypeId))
                    .Cast<ConnectorTypeId>()
                    .Select(e => new ConnectorType(e))
            );

        modelBuilder.Entity<Language>(entity =>
        {
            entity.Property(e => e.ShortName)
                .IsFixedLength();
        });

        modelBuilder.Entity<LanguageLongName>(entity =>
        {
            entity.HasKey(e => new { e.ShortName, e.LanguageShortName });
            entity.Property(e => e.ShortName)
                .IsFixedLength();
            entity.Property(e => e.LanguageShortName)
                .IsFixedLength();
            entity.HasOne(e => e.Language)
                .WithMany(e => e.LanguageLongNames)
                .HasForeignKey(e => e.ShortName)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(e => e.LongNameLanguage)
                .WithMany(e => e.LanguageLongNameLanguages)
                .HasForeignKey(e => e.LanguageShortName)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(x => x.DueDate)
                .IsRequired(false);

            entity.HasOne(d => d.Receiver)
                .WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ReceiverUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Creator)
                .WithMany(p => p.CreatedNotifications)
                .HasForeignKey(d => d.CreatorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.NotificationType)
                .WithMany(p => p.Notifications)
                .HasForeignKey(d => d.NotificationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<NotificationTopic>()
            .HasData(
                Enum.GetValues(typeof(NotificationTopicId))
                    .Cast<NotificationTopicId>()
                    .Select(e => new NotificationTopic(e))
            );

        modelBuilder.Entity<NotificationType>()
            .HasData(
                Enum.GetValues(typeof(NotificationTypeId))
                    .Cast<NotificationTypeId>()
                    .Select(e => new NotificationType(e))
            );

        modelBuilder.Entity<NotificationTypeAssignedTopic>(entity =>
        {
            entity.HasKey(e => new { e.NotificationTypeId, e.NotificationTopicId });

            entity.HasOne(d => d.NotificationTopic)
                .WithMany(x => x.NotificationTypeAssignedTopics)
                .HasForeignKey(d => d.NotificationTopicId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.NotificationType)
                .WithOne(x => x.NotificationTypeAssignedTopic)
                .HasForeignKey<NotificationTypeAssignedTopic>(d => d.NotificationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<UseCase>();

        modelBuilder.Entity<UniqueIdentifier>()
            .HasData(
                Enum.GetValues(typeof(UniqueIdentifierId))
                    .Cast<UniqueIdentifierId>()
                    .Select(e => new UniqueIdentifier(e))
            );

        modelBuilder.Entity<OfferSubscriptionStatus>()
            .HasData(
                Enum.GetValues(typeof(OfferSubscriptionStatusId))
                    .Cast<OfferSubscriptionStatusId>()
                    .Select(e => new OfferSubscriptionStatus(e))
            );

        modelBuilder.Entity<CompanyIdentifier>(entity =>
        {
            entity.HasKey(e => new { e.CompanyId, e.UniqueIdentifierId });

            entity.HasOne(d => d.Company)
                .WithMany(p => p!.CompanyIdentifiers)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UniqueIdentifier)
                .WithMany(p => p!.CompanyIdentifiers)
                .HasForeignKey(d => d.UniqueIdentifierId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<BpdmIdentifier>()
            .HasData(
                Enum.GetValues(typeof(BpdmIdentifierId))
                    .Cast<BpdmIdentifierId>()
                    .Select(e => new BpdmIdentifier(e))
            );

        modelBuilder.Entity<Process>()
            .HasOne(d => d.ProcessType)
            .WithMany(p => p!.Processes)
            .HasForeignKey(d => d.ProcessTypeId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<ProcessStep>()
            .HasOne(d => d.Process)
            .WithMany(p => p!.ProcessSteps)
            .HasForeignKey(d => d.ProcessId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<ProcessType>()
            .HasData(
                Enum.GetValues(typeof(ProcessTypeId))
                    .Cast<ProcessTypeId>()
                    .Select(e => new ProcessType(e))
            );

        modelBuilder.Entity<ProcessStepStatus>()
            .HasData(
                Enum.GetValues(typeof(ProcessStepStatusId))
                    .Cast<ProcessStepStatusId>()
                    .Select(e => new ProcessStepStatus(e))
            );

        modelBuilder.Entity<ProcessStepType>()
            .HasData(
                Enum.GetValues(typeof(ProcessStepTypeId))
                    .Cast<ProcessStepTypeId>()
                    .Select(e => new ProcessStepType(e))
            );

        modelBuilder.Entity<PrivacyPolicy>()
            .HasData(
                Enum.GetValues(typeof(PrivacyPolicyId))
                    .Cast<PrivacyPolicyId>()
                    .Select(e => new PrivacyPolicy(e))
            );

        modelBuilder.Entity<OfferAssignedPrivacyPolicy>(entity =>
        {
            entity.HasKey(e => new { e.OfferId, e.PrivacyPolicyId });

            entity.HasOne(d => d.Offer)
                .WithMany(p => p.OfferAssignedPrivacyPolicies)
                .HasForeignKey(d => d.OfferId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.PrivacyPolicy)
                .WithMany(p => p.OfferAssignedPrivacyPolicies)
                .HasForeignKey(d => d.PrivacyPolicyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<LicenseType>()
            .HasData(
                Enum.GetValues(typeof(LicenseTypeId))
                    .Cast<LicenseTypeId>()
                    .Select(e => new LicenseType(e))
            );

        modelBuilder.Entity<ServiceDetail>(entity =>
        {
            entity.HasKey(e => new { e.ServiceId, e.ServiceTypeId });

            entity.HasOne(e => e.ServiceType)
                .WithMany(e => e.ServiceDetails)
                .HasForeignKey(e => e.ServiceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.Service)
                .WithMany(e => e.ServiceDetails)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TechnicalUserProfile>(entity =>
        {
            entity.HasOne(t => t.Offer)
                .WithMany(o => o.TechnicalUserProfiles)
                .HasForeignKey(t => t.OfferId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TechnicalUserProfileAssignedUserRole>(entity =>
        {
            entity.HasKey(e => new { e.TechnicalUserProfileId, e.UserRoleId });

            entity.HasOne(d => d.TechnicalUserProfile)
                .WithMany(p => p!.TechnicalUserProfileAssignedUserRoles)
                .HasForeignKey(d => d.TechnicalUserProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UserRole)
                .WithMany(p => p!.TechnicalUserProfileAssignedUserRole)
                .HasForeignKey(d => d.UserRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<VerifiedCredentialType>()
            .HasData(
                Enum.GetValues(typeof(VerifiedCredentialTypeId))
                    .Cast<VerifiedCredentialTypeId>()
                    .Select(e => new VerifiedCredentialType(e))
            );

        modelBuilder.Entity<VerifiedCredentialTypeKind>()
            .HasData(
                Enum.GetValues(typeof(VerifiedCredentialTypeKindId))
                    .Cast<VerifiedCredentialTypeKindId>()
                    .Select(e => new VerifiedCredentialTypeKind(e))
            );

        modelBuilder.Entity<CompanySsiDetailStatus>()
            .HasData(
                Enum.GetValues(typeof(CompanySsiDetailStatusId))
                    .Cast<CompanySsiDetailStatusId>()
                    .Select(e => new CompanySsiDetailStatus(e))
            );

        modelBuilder.Entity<VerifiedCredentialExternalType>()
            .HasData(
                Enum.GetValues(typeof(VerifiedCredentialExternalTypeId))
                    .Cast<VerifiedCredentialExternalTypeId>()
                    .Select(e => new VerifiedCredentialExternalType(e))
            );

        modelBuilder.Entity<UseCaseDescription>(entity =>
        {
            entity.HasKey(e => new { e.UseCaseId, e.LanguageShortName });

            entity.HasOne(d => d.UseCase)
                .WithMany(p => p.UseCaseDescriptions)
                .HasForeignKey(d => d.UseCaseId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Language)
                .WithMany(p => p.UseCases)
                .HasForeignKey(d => d.LanguageShortName)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<CompanySsiDetail>(entity =>
        {
            entity.HasOne(c => c.Company)
                .WithMany(c => c.CompanySsiDetails)
                .HasForeignKey(t => t.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(c => c.VerifiedCredentialType)
                .WithMany(c => c.CompanySsiDetails)
                .HasForeignKey(t => t.VerifiedCredentialTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(c => c.CompanySsiDetailStatus)
                .WithMany(c => c.CompanySsiDetails)
                .HasForeignKey(t => t.CompanySsiDetailStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(c => c.VerifiedCredentialExternalTypeUseCaseDetailVersion)
                .WithMany(c => c.CompanySsiDetails)
                .HasForeignKey(t => t.VerifiedCredentialExternalTypeUseCaseDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(c => c.CreatorUser)
                .WithMany(c => c.CompanySsiDetails)
                .HasForeignKey(t => t.CreatorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(t => t.Document)
                .WithOne(o => o.CompanySsiDetail)
                .HasForeignKey<CompanySsiDetail>(t => t.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<CompanySsiDetail, AuditCompanySsiDetail20230621>();
        });

        modelBuilder.Entity<VerifiedCredentialTypeAssignedUseCase>(entity =>
        {
            entity.HasKey(x => new { x.VerifiedCredentialTypeId, x.UseCaseId });

            entity.HasOne(c => c.VerifiedCredentialType)
                .WithOne(c => c.VerifiedCredentialTypeAssignedUseCase)
                .HasForeignKey<VerifiedCredentialTypeAssignedUseCase>(c => c.VerifiedCredentialTypeId);

            entity.HasOne(c => c.UseCase)
                .WithOne(c => c.VerifiedCredentialAssignedUseCase)
                .HasForeignKey<VerifiedCredentialTypeAssignedUseCase>(c => c.UseCaseId);
        });

        modelBuilder.Entity<VerifiedCredentialTypeAssignedKind>(entity =>
        {
            entity.HasKey(e => new { e.VerifiedCredentialTypeId, e.VerifiedCredentialTypeKindId });

            entity.HasOne(d => d.VerifiedCredentialTypeKind)
                .WithMany(x => x.VerifiedCredentialTypeAssignedKinds)
                .HasForeignKey(d => d.VerifiedCredentialTypeKindId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.VerifiedCredentialType)
                .WithOne(x => x.VerifiedCredentialTypeAssignedKind)
                .HasForeignKey<VerifiedCredentialTypeAssignedKind>(d => d.VerifiedCredentialTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasIndex(x => x.VerifiedCredentialTypeId)
                .IsUnique(false);
        });

        modelBuilder.Entity<VerifiedCredentialTypeAssignedExternalType>(entity =>
        {
            entity.HasKey(e => new { e.VerifiedCredentialTypeId, e.VerifiedCredentialExternalTypeId });

            entity.HasOne(d => d.VerifiedCredentialType)
                .WithOne(x => x.VerifiedCredentialTypeAssignedExternalType)
                .HasForeignKey<VerifiedCredentialTypeAssignedExternalType>(d => d.VerifiedCredentialTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.VerifiedCredentialExternalType)
                .WithMany(x => x.VerifiedCredentialTypeAssignedExternalTypes)
                .HasForeignKey(d => d.VerifiedCredentialExternalTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<VerifiedCredentialExternalTypeUseCaseDetailVersion>(entity =>
        {
            entity.HasOne(d => d.VerifiedCredentialExternalType)
                .WithMany(x => x.VerifiedCredentialExternalTypeUseCaseDetailVersions)
                .HasForeignKey(d => d.VerifiedCredentialExternalTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasIndex(e => new { e.VerifiedCredentialExternalTypeId, e.Version })
                .IsUnique();
        });
        modelBuilder.Entity<CompaniesLinkedServiceAccount>()
             .ToView("company_linked_service_accounts", "portal")
             .HasKey(t => t.ServiceAccountId);

        modelBuilder.Entity<ConnectorAssignedOfferSubscription>(entity =>
        {
            entity.HasKey(e => new { e.ConnectorId, e.OfferSubscriptionId });

            entity.HasOne(d => d.Connector)
                .WithMany(x => x.ConnectorAssignedOfferSubscriptions)
                .HasForeignKey(d => d.ConnectorId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.OfferSubscription)
                .WithMany(x => x.ConnectorAssignedOfferSubscriptions)
                .HasForeignKey(d => d.OfferSubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<OfferSubscriptionView>()
            .ToView("offer_subscription_view", "portal")
            .HasNoKey();
    }
}
