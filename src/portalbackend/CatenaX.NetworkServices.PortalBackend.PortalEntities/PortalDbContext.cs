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

using CatenaX.NetworkServices.PortalBackend.PortalEntities.AuditEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Laraue.EfCoreTriggers.Common.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities;

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
    public virtual DbSet<AgreementAssignedDocumentTemplate> AgreementAssignedDocumentTemplates { get; set; } = default!;
    public virtual DbSet<AgreementAssignedOffer> AgreementAssignedOffers { get; set; } = default!;
    public virtual DbSet<AgreementCategory> AgreementCategories { get; set; } = default!;
    public virtual DbSet<AppInstance> AppInstances { get; set; } = default!;
    public virtual DbSet<AppAssignedUseCase> AppAssignedUseCases { get; set; } = default!;
    public virtual DbSet<AppLanguage> AppLanguages { get; set; } = default!;
    public virtual DbSet<AppSubscriptionDetail> AppSubscriptionDetails { get; set; } = default!;
    public virtual DbSet<Company> Companies { get; set; } = default!;
    public virtual DbSet<CompanyApplication> CompanyApplications { get; set; } = default!;
    public virtual DbSet<CompanyApplicationStatus> CompanyApplicationStatuses { get; set; } = default!;
    public virtual DbSet<CompanyAssignedRole> CompanyAssignedRoles { get; set; } = default!;
    public virtual DbSet<CompanyAssignedUseCase> CompanyAssignedUseCases { get; set; } = default!;
    public virtual DbSet<CompanyIdentityProvider> CompanyIdentityProviders { get; set; } = default!;
    public virtual DbSet<CompanyRole> CompanyRoles { get; set; } = default!;
    public virtual DbSet<CompanyRoleDescription> CompanyRoleDescriptions { get; set; } = default!;
    public virtual DbSet<CompanyServiceAccount> CompanyServiceAccounts { get; set; } = default!;
    public virtual DbSet<CompanyServiceAccountAssignedRole> CompanyServiceAccountAssignedRoles { get; set; } = default!;
    public virtual DbSet<CompanyServiceAccountStatus> CompanyServiceAccountStatuses { get; set; } = default!;
    public virtual DbSet<CompanyStatus> CompanyStatuses { get; set; } = default!;
    public virtual DbSet<CompanyUser> CompanyUsers { get; set; } = default!;
    public virtual DbSet<CompanyUserAssignedAppFavourite> CompanyUserAssignedAppFavourites { get; set; } = default!;
    public virtual DbSet<CompanyUserAssignedBusinessPartner> CompanyUserAssignedBusinessPartners { get; set; } = default!;
    public virtual DbSet<CompanyUserAssignedRole> CompanyUserAssignedRoles { get; set; } = default!;
    public virtual DbSet<Connector> Connectors { get; set; } = default!;
    public virtual DbSet<ConnectorStatus> ConnectorStatuses { get; set; } = default!;
    public virtual DbSet<ConnectorType> ConnectorTypes { get; set; } = default!;
    public virtual DbSet<UserRole> UserRoles { get; set; } = default!;
    public virtual DbSet<UserRoleDescription> UserRoleDescriptions { get; set; } = default!;
    public virtual DbSet<CompanyUserStatus> CompanyUserStatuses { get; set; } = default!;
    public virtual DbSet<Consent> Consents { get; set; } = default!;
    public virtual DbSet<ConsentStatus> ConsentStatuses { get; set; } = default!;
    public virtual DbSet<ConsentAssignedOfferSubscription> ConsentAssignedOfferSubscriptions { get; set; } = default!;
    public virtual DbSet<Country> Countries { get; set; } = default!;
    public virtual DbSet<Document> Documents { get; set; } = default!;
    public virtual DbSet<DocumentTemplate> DocumentTemplates { get; set; } = default!;
    public virtual DbSet<DocumentType> DocumentTypes { get; set; } = default!;
    public virtual DbSet<DocumentStatus> DocumentStatus { get; set; } = default!;
    public virtual DbSet<IamClient> IamClients { get; set; } = default!;
    public virtual DbSet<IamIdentityProvider> IamIdentityProviders { get; set; } = default!;
    public virtual DbSet<IamServiceAccount> IamServiceAccounts { get; set; } = default!;
    public virtual DbSet<IamUser> IamUsers { get; set; } = default!;
    public virtual DbSet<IdentityProvider> IdentityProviders { get; set; } = default!;
    public virtual DbSet<IdentityProviderCategory> IdentityProviderCategories { get; set; } = default!;
    public virtual DbSet<Invitation> Invitations { get; set; } = default!;
    public virtual DbSet<InvitationStatus> InvitationStatuses { get; set; } = default!;
    public virtual DbSet<Language> Languages { get; set; } = default!;
    public virtual DbSet<Notification> Notifications { get; set; } = default!;
    public virtual DbSet<UseCase> UseCases { get; set; } = default!;
    public virtual DbSet<Offer> Offers { get; set; } = default!;
    public virtual DbSet<OfferAssignedDocument> OfferAssignedDocuments { get; set; } = default!;
    public virtual DbSet<OfferAssignedLicense> OfferAssignedLicenses { get; set; } = default!;
    public virtual DbSet<OfferDescription> OfferDescriptions { get; set; } = default!;
    public virtual DbSet<OfferDetailImage> OfferDetailImages { get; set; } = default!;
    public virtual DbSet<OfferLicense> OfferLicenses { get; set; } = default!;
    public virtual DbSet<OfferStatus> OfferStatuses { get; set; } = default!;
    public virtual DbSet<OfferTag> OfferTags { get; set; } = default!;
    public virtual DbSet<OfferType> OfferTypes { get; set; } = default!;
    public virtual DbSet<OfferSubscription> OfferSubscriptions { get; set; } = default!;
    public virtual DbSet<OfferSubscriptionStatus> OfferSubscriptionStatuses { get; set; } = default!;
    
    public virtual DbSet<AuditOfferSubscriptionCplp1440DbAuditing> AuditOfferSubscriptionCplp1440DbAuditing { get; set; } = default!;
    public virtual DbSet<AuditCompanyApplicationCplp1440DbAuditing> AuditCompanyApplicationCplp1440DbAuditing { get; set; } = default!;
    public virtual DbSet<AuditCompanyUserCplp1440DbAuditing> AuditCompanyUserCplp1440DbAuditing { get; set; } = default!;
    public virtual DbSet<AuditCompanyUserAssignedRoleCplp1440DbAuditing> AuditCompanyUserAssignedRoleCplp1440DbAuditing { get; set; } = default!;
    
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

        modelBuilder.Entity<AgreementAssignedDocumentTemplate>(entity =>
        {
            entity.HasKey(e => new { e.AgreementId, e.DocumentTemplateId });

            entity.HasOne(d => d.Agreement)
                .WithMany(p => p!.AgreementAssignedDocumentTemplates)
                .HasForeignKey(d => d.AgreementId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.DocumentTemplate)
                .WithOne(p => p!.AgreementAssignedDocumentTemplate!)
                .HasForeignKey<AgreementAssignedDocumentTemplate>(d => d.DocumentTemplateId)
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

        modelBuilder.Entity<AgreementCategory>()
            .HasData(
                Enum.GetValues(typeof(AgreementCategoryId))
                    .Cast<AgreementCategoryId>()
                    .Select(e => new AgreementCategory(e))
            );

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
                        j.Property(e => e.OfferSubscriptionStatusId)
                            .HasDefaultValue(OfferSubscriptionStatusId.PENDING);
                        j.AfterInsert(trigger => trigger
                            .Action(action => action
                                .Insert(newValue => new AuditOfferSubscriptionCplp1440DbAuditing
                                {
                                    Id = new Guid(),
                                    AuditId = newValue.Id,
                                    AuditOperationId = AuditOperationId.INSERT,
                                    OfferSubscriptionStatusId = newValue.OfferSubscriptionStatusId,
                                    DisplayName = newValue.DisplayName,
                                    Description = newValue.Description,
                                    RequesterId = newValue.RequesterId,
                                    LastEditorId = newValue.LastEditorId,
                                    DateLastChanged = new DateTimeOffset()
                                })))
                        .AfterUpdate(trigger => trigger
                            .Action(action => action
                                .Insert((oldEntity, newEntity) => new AuditOfferSubscriptionCplp1440DbAuditing
                                {
                                    Id = new Guid(),
                                    AuditId = oldEntity.Id,
                                    AuditOperationId = AuditOperationId.UPDATE,
                                    OfferSubscriptionStatusId = oldEntity.OfferSubscriptionStatusId,
                                    DisplayName = oldEntity.DisplayName,
                                    Description = oldEntity.Description,
                                    RequesterId = oldEntity.RequesterId,
                                    LastEditorId = oldEntity.LastEditorId,
                                    DateLastChanged = new DateTimeOffset()
                                })))
                        .AfterDelete(trigger => trigger
                            .Action(action => action
                                .Insert(deletedEntity => new AuditOfferSubscriptionCplp1440DbAuditing
                                {
                                    Id = new Guid(),
                                    AuditId = deletedEntity.Id,
                                    AuditOperationId = AuditOperationId.DELETE,
                                    OfferSubscriptionStatusId = deletedEntity.OfferSubscriptionStatusId,
                                    DisplayName = deletedEntity.DisplayName,
                                    Description = deletedEntity.Description,
                                    RequesterId = deletedEntity.RequesterId,
                                    LastEditorId = deletedEntity.LastEditorId,
                                    DateLastChanged = new DateTimeOffset()
                                })));
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
                .WithMany(p => p.Apps)
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
                        j.HasKey(e => new {AppId = e.OfferId, AppLicenseId = e.OfferLicenseId });
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
        });

        modelBuilder.Entity<AppSubscriptionDetail>(entity =>
        {
            entity.HasOne(e => e.AppInstance)
                .WithMany(e => e.AppSubscriptionDetails)
                .HasForeignKey(e => e.AppInstanceId);
            entity.HasOne(e => e.OfferSubscription)
                .WithOne(e => e.AppSubscriptionDetail)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });
        
        modelBuilder.Entity<OfferType>()
            .HasData(
                Enum.GetValues(typeof(OfferTypeId))
                    .Cast<OfferTypeId>()
                    .Select(e => new OfferType(e))
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
        
        modelBuilder.Entity<OfferDescription>(entity =>
        {
            entity.HasKey(e => new {AppId = e.OfferId, e.LanguageShortName });

            entity.HasOne(d => d.Offer)
                .WithMany(p => p!.OfferDescriptions)
                .HasForeignKey(d => d.OfferId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Language)
                .WithMany(p => p!.AppDescriptions)
                .HasForeignKey(d => d.LanguageShortName)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<OfferDetailImage>()
            .HasOne(d => d.Offer)
                .WithMany(p => p!.OfferDetailImages)
                .HasForeignKey(d => d.OfferId)
                .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<OfferStatus>()
            .HasData(
                Enum.GetValues(typeof(OfferStatusId))
                    .Cast<OfferStatusId>()
                    .Select(e => new OfferStatus(e))
            );

        modelBuilder.Entity<OfferTag>(entity =>
        {
            entity.HasKey(e => new {AppId = e.OfferId, e.Name });

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
            entity.HasMany(p => p.CompanyRoles)
                .WithMany(p => p.Companies)
                .UsingEntity<CompanyAssignedRole>(
                    j => j
                        .HasOne(d => d.CompanyRole!)
                        .WithMany()
                        .HasForeignKey(d => d.CompanyRoleId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.Company!)
                        .WithMany()
                        .HasForeignKey(d => d.CompanyId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { e.CompanyId, e.CompanyRoleId });
                    }
                );

            entity.HasMany(p => p.CompanyAssignedRoles)
                .WithOne(d => d.Company!)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasMany(p => p.OfferSubscriptions)
                .WithOne(d => d.Company)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasMany(p => p.UseCases)
                .WithMany(p => p.Companies)
                .UsingEntity<CompanyAssignedUseCase>(
                    j => j
                        .HasOne(d => d.UseCase!)
                        .WithMany()
                        .HasForeignKey(d => d.UseCaseId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.Company!)
                        .WithMany()
                        .HasForeignKey(d => d.CompanyId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { e.CompanyId, e.UseCaseId });
                    }
                );

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

        modelBuilder.Entity<CompanyApplication>(entity =>
        {
            entity.HasOne(d => d.Company)
                .WithMany(p => p!.CompanyApplications)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.AfterInsert(trigger => trigger
                .Action(action => action
                    .Insert(newValue => new AuditCompanyApplicationCplp1440DbAuditing
                    {
                        Id = new Guid(),
                        AuditId = newValue.Id,
                        AuditOperationId = AuditOperationId.INSERT,
                        ApplicationStatusId = newValue.ApplicationStatusId,
                        DateCreated = newValue.DateCreated,
                        CompanyId = newValue.CompanyId,
                        LastEditorId = newValue.LastEditorId,
                        DateLastChanged = new DateTimeOffset()
                    })));
            entity.AfterUpdate(trigger => trigger
                .Action(action => action
                    .Insert((oldEntity, newEntity) => new AuditCompanyApplicationCplp1440DbAuditing
                    {
                        Id = new Guid(),
                        AuditId = oldEntity.Id,
                        AuditOperationId = AuditOperationId.UPDATE,
                        ApplicationStatusId = oldEntity.ApplicationStatusId,
                        DateCreated = oldEntity.DateCreated,
                        CompanyId = oldEntity.CompanyId,
                        LastEditorId = oldEntity.LastEditorId,
                        DateLastChanged = new DateTimeOffset()
                    })));
            entity.AfterDelete(trigger => trigger
                .Action(action => action
                    .Insert(deletedEntity => new AuditCompanyApplicationCplp1440DbAuditing
                    {
                        Id = new Guid(),
                        AuditId = deletedEntity.Id,
                        AuditOperationId = AuditOperationId.DELETE,
                        ApplicationStatusId = deletedEntity.ApplicationStatusId,
                        DateCreated = deletedEntity.DateCreated,
                        CompanyId = deletedEntity.CompanyId,
                        LastEditorId = deletedEntity.LastEditorId,
                        DateLastChanged = new DateTimeOffset()
                    })));
        });

        modelBuilder.Entity<CompanyApplicationStatus>()
            .HasData(
                Enum.GetValues(typeof(CompanyApplicationStatusId))
                    .Cast<CompanyApplicationStatusId>()
                    .Select(e => new CompanyApplicationStatus(e))
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

        modelBuilder.Entity<CompanyRoleDescription>(entity =>
        {
            entity.HasKey(e => new { e.CompanyRoleId, e.LanguageShortName });

            entity.HasData(StaticPortalData.CompanyRoleDescriptions);
        });

        modelBuilder.Entity<CompanyServiceAccount>(entity =>
        {
            entity.HasOne(d => d.Company)
                .WithMany(p => p!.CompanyServiceAccounts)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.CompanyServiceAccountStatus)
                .WithMany(p => p!.CompanyServiceAccounts)
                .HasForeignKey(d => d.CompanyServiceAccountStatusId);
                
            entity.HasMany(p => p.UserRoles)
                .WithMany(p => p.CompanyServiceAccounts)
                .UsingEntity<CompanyServiceAccountAssignedRole>(
                    j => j
                        .HasOne(d => d.UserRole!)
                        .WithMany()
                        .HasForeignKey(d => d.UserRoleId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.CompanyServiceAccount!)
                        .WithMany()
                        .HasForeignKey(d => d.CompanyServiceAccountId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { e.CompanyServiceAccountId, e.UserRoleId });
                    });

            entity.HasMany(p => p.CompanyServiceAccountAssignedRoles)
                .WithOne(d => d.CompanyServiceAccount!);
        });

        modelBuilder.Entity<CompanyServiceAccountStatus>()
            .HasData(
                Enum.GetValues(typeof(CompanyServiceAccountStatusId))
                    .Cast<CompanyServiceAccountStatusId>()
                    .Select(e => new CompanyServiceAccountStatus(e))
            );

        modelBuilder.Entity<CompanyStatus>()
            .HasData(
                Enum.GetValues(typeof(CompanyStatusId))
                    .Cast<CompanyStatusId>()
                    .Select(e => new CompanyStatus(e))
            );
        
        modelBuilder.Entity<CompanyUser>(entity =>
        {
            entity.HasOne(d => d.Company)
                .WithMany(p => p!.CompanyUsers)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
                
            entity.HasMany(p => p.Offers)
                .WithMany(p => p.CompanyUsers)
                .UsingEntity<CompanyUserAssignedAppFavourite>(
                    j => j
                        .HasOne(d => d.App!)
                        .WithMany()
                        .HasForeignKey(d => d.AppId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.CompanyUser!)
                        .WithMany()
                        .HasForeignKey(d => d.CompanyUserId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { e.CompanyUserId, e.AppId });
                    });

            entity.HasMany(p => p.UserRoles)
                .WithMany(p => p.CompanyUsers)
                .UsingEntity<CompanyUserAssignedRole>(
                    j => j
                        .HasOne(d => d.UserRole!)
                        .WithMany()
                        .HasForeignKey(d => d.UserRoleId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.CompanyUser!)
                        .WithMany()
                        .HasForeignKey(d => d.CompanyUserId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                        j =>
                        {
                            j.HasKey(e => new { e.CompanyUserId, e.UserRoleId });
                            
                            j.AfterInsert(trigger => trigger
                                .Action(action => action
                                    .Insert(newValue => new AuditCompanyUserAssignedRoleCplp1440DbAuditing
                                    {
                                        Id = new Guid(),
                                        AuditId = newValue.Id,
                                        AuditOperationId = AuditOperationId.INSERT,
                                        UserRoleId = newValue.UserRoleId,
                                        CompanyUserId = newValue.CompanyUserId,
                                        LastEditorId = newValue.LastEditorId,
                                        DateLastChanged = new DateTimeOffset()
                                    })));
                            j.AfterUpdate(trigger => trigger
                                .Action(action => action
                                    .Insert((oldEntity, newEntity) => new AuditCompanyUserAssignedRoleCplp1440DbAuditing
                                    {
                                        Id = new Guid(),
                                        AuditId = oldEntity.Id,
                                        AuditOperationId = AuditOperationId.UPDATE,
                                        UserRoleId = oldEntity.UserRoleId,
                                        CompanyUserId = oldEntity.CompanyUserId,
                                        LastEditorId = oldEntity.LastEditorId,
                                        DateLastChanged = new DateTimeOffset()
                                    })));
                            j.AfterDelete(trigger => trigger
                                .Action(action => action
                                    .Insert(deletedEntity => new AuditCompanyUserAssignedRoleCplp1440DbAuditing
                                    {
                                        Id = new Guid(),
                                        AuditId = deletedEntity.Id,
                                        AuditOperationId = AuditOperationId.DELETE,
                                        UserRoleId = deletedEntity.UserRoleId,
                                        CompanyUserId = deletedEntity.CompanyUserId,
                                        LastEditorId = deletedEntity.LastEditorId,
                                        DateLastChanged = new DateTimeOffset()
                                    })));
                        });

            entity.HasMany(p => p.CompanyUserAssignedRoles)
                .WithOne(d => d.CompanyUser!);

            entity.HasMany(p => p.CompanyUserAssignedBusinessPartners)
                .WithOne(d => d.CompanyUser);
            
            entity.AfterInsert(trigger => trigger
                .Action(action => action
                    .Insert(newValue => new AuditCompanyUserCplp1440DbAuditing
                    {
                        Id = new Guid(),
                        AuditId = newValue.Id,
                        AuditOperationId = AuditOperationId.INSERT,
                        CompanyId = newValue.CompanyId,
                        CompanyUserStatusId = newValue.CompanyUserStatusId,
                        Email = newValue.Email,
                        Firstname = newValue.Firstname,
                        Lastlogin = newValue.Lastlogin,
                        Lastname = newValue.Lastname,
                        LastEditorId = newValue.LastEditorId,
                        DateLastChanged = new DateTimeOffset()
                    })));
            entity.AfterUpdate(trigger => trigger
                .Action(action => action
                    .Insert((oldEntity, newEntity) => new AuditCompanyUserCplp1440DbAuditing
                    {
                        Id = new Guid(),
                        AuditId = oldEntity.Id,
                        AuditOperationId = AuditOperationId.UPDATE,
                        CompanyId = oldEntity.CompanyId,
                        CompanyUserStatusId = oldEntity.CompanyUserStatusId,
                        Email = oldEntity.Email,
                        Firstname = oldEntity.Firstname,
                        Lastlogin = oldEntity.Lastlogin,
                        Lastname = oldEntity.Lastname,
                        LastEditorId = oldEntity.LastEditorId,
                        DateLastChanged = new DateTimeOffset()
                    })));
            entity.AfterDelete(trigger => trigger
                .Action(action => action
                    .Insert(deletedEntity => new AuditCompanyUserCplp1440DbAuditing
                    {
                        Id = new Guid(),
                        AuditId = deletedEntity.Id,
                        AuditOperationId = AuditOperationId.DELETE,
                        CompanyId = deletedEntity.CompanyId,
                        CompanyUserStatusId = deletedEntity.CompanyUserStatusId,
                        Email = deletedEntity.Email,
                        Firstname = deletedEntity.Firstname,
                        Lastlogin = deletedEntity.Lastlogin,
                        Lastname = deletedEntity.Lastname,
                        LastEditorId = deletedEntity.LastEditorId,
                        DateLastChanged = new DateTimeOffset()
                    })));
        });

        modelBuilder.Entity<CompanyUserAssignedBusinessPartner>()
            .HasKey(e => new { e.CompanyUserId, e.BusinessPartnerNumber });

        modelBuilder.Entity<UserRoleDescription>().HasKey(e => new { e.UserRoleId, e.LanguageShortName });

        modelBuilder.Entity<CompanyUserStatus>()
            .HasData(
                Enum.GetValues(typeof(CompanyUserStatusId))
                    .Cast<CompanyUserStatusId>()
                    .Select(e => new CompanyUserStatus(e))
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

            entity.HasData(StaticPortalData.Countries);
        });

        modelBuilder.Entity<DocumentType>()
            .HasData(
                Enum.GetValues(typeof(DocumentTypeId))
                    .Cast<DocumentTypeId>()
                    .Select(e => new DocumentType(e))
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

        modelBuilder.Entity<IamServiceAccount>(entity =>
        {
            entity.HasIndex(e => e.ClientClientId)
                .IsUnique();

            entity.HasIndex(e => e.UserEntityId)
                .IsUnique();

            entity.HasOne(d => d.CompanyServiceAccount)
                .WithOne(p => p!.IamServiceAccount!)
                .HasForeignKey<IamServiceAccount>(d => d.CompanyServiceAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<IamUser>()
            .HasOne(d => d.CompanyUser)
                .WithOne(p => p!.IamUser!)
                .HasForeignKey<IamUser>(d => d.CompanyUserId)
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

            entity.HasData(StaticPortalData.Languages);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(x => x.DueDate)
                .IsRequired(false);

            entity.HasOne(d => d.Receiver)
                .WithMany(p => p!.Notifications)
                .HasForeignKey(d => d.ReceiverUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Creator)
                .WithMany(p => p!.CreatedNotifications)
                .HasForeignKey(d => d.CreatorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.NotificationType)
                .WithMany(p => p!.Notifications)
                .HasForeignKey(d => d.NotificationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<NotificationType>()
            .HasData(
                Enum.GetValues(typeof(NotificationTypeId))
                    .Cast<NotificationTypeId>()
                    .Select(e => new NotificationType(e))
            );

        modelBuilder.Entity<UseCase>().HasData(StaticPortalData.UseCases);

        modelBuilder.Entity<OfferSubscriptionStatus>()
            .HasData(
                Enum.GetValues(typeof(OfferSubscriptionStatusId))
                    .Cast<OfferSubscriptionStatusId>()
                    .Select(e => new OfferSubscriptionStatus(e))
            );
    }
}
