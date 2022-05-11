using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities
{
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
        public virtual DbSet<AgreementCategory> AgreementCategories { get; set; } = default!;
        public virtual DbSet<App> Apps { get; set; } = default!;
        public virtual DbSet<AppAssignedClient> AppAssignedClients { get; set; } = default!;
        public virtual DbSet<AppAssignedLicense> AppAssignedLicenses { get; set; } = default!;
        public virtual DbSet<AppAssignedUseCase> AppAssignedUseCases { get; set; } = default!;
        public virtual DbSet<AppDescription> AppDescriptions { get; set; } = default!;
        public virtual DbSet<AppDetailImage> AppDetailImages { get; set; } = default!;
        public virtual DbSet<AppLanguage> AppLanguages { get; set; } = default!;
        public virtual DbSet<AppLicense> AppLicenses { get; set; } = default!;
        public virtual DbSet<AppStatus> AppStatuses { get; set; } = default!;
        public virtual DbSet<AppTag> AppTags { get; set; } = default!;
        public virtual DbSet<Company> Companies { get; set; } = default!;
        public virtual DbSet<CompanyApplication> CompanyApplications { get; set; } = default!;
        public virtual DbSet<CompanyApplicationStatus> CompanyApplicationStatuses { get; set; } = default!;
        public virtual DbSet<CompanyAssignedApp> CompanyAssignedApps { get; set; } = default!;
        public virtual DbSet<CompanyAssignedRole> CompanyAssignedRoles { get; set; } = default!;
        public virtual DbSet<CompanyAssignedUseCase> CompanyAssignedUseCases { get; set; } = default!;
        public virtual DbSet<CompanyIdentityProvider> CompanyIdentityProviders { get; set; } = default!;
        public virtual DbSet<CompanyRole> CompanyRoles { get; set; } = default!;
        public virtual DbSet<CompanyStatus> CompanyStatuses { get; set; } = default!;
        public virtual DbSet<CompanyUser> CompanyUsers { get; set; } = default!;
        public virtual DbSet<CompanyUserAssignedAppFavourite> CompanyUserAssignedAppFavourites { get; set; } = default!;
        public virtual DbSet<CompanyUserAssignedRole> CompanyUserAssignedRoles { get; set; } = default!;
        public virtual DbSet<UserRole> UserRoles { get; set; } = default!;
        public virtual DbSet<CompanyUserStatus> CompanyUserStatuses { get; set; } = default!;
        public virtual DbSet<Consent> Consents { get; set; } = default!;
        public virtual DbSet<ConsentStatus> ConsentStatuses { get; set; } = default!;
        public virtual DbSet<Country> Countries { get; set; } = default!;
        public virtual DbSet<Document> Documents { get; set; } = default!;
        public virtual DbSet<DocumentTemplate> DocumentTemplates { get; set; } = default!;
        public virtual DbSet<DocumentType> DocumentTypes { get; set; } = default!;
        public virtual DbSet<IamClient> IamClients { get; set; } = default!;
        public virtual DbSet<IamIdentityProvider> IamIdentityProviders { get; set; } = default!;
        public virtual DbSet<IamUser> IamUsers { get; set; } = default!;
        public virtual DbSet<IdentityProvider> IdentityProviders { get; set; } = default!;
        public virtual DbSet<IdentityProviderCategory> IdentityProviderCategories { get; set; } = default!;
        public virtual DbSet<Invitation> Invitations { get; set; } = default!;
        public virtual DbSet<InvitationStatus> InvitationStatuses { get; set; } = default!;
        public virtual DbSet<Language> Languages { get; set; } = default!;
        public virtual DbSet<UseCase> UseCases { get; set; } = default!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "en_US.utf8");

            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("addresses", "portal");

                entity.Property(e => e.CountryAlpha2Code)
                    .IsFixedLength(true);

                entity.Property(e => e.Zipcode)
                    .HasPrecision(19, 2);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p!.Addresses)
                    .HasForeignKey(d => d.CountryAlpha2Code)
                    .HasConstraintName("fk_6jg6itw07d2qww62deuyk0kh");
            });

            modelBuilder.Entity<Agreement>(entity =>
            {
                entity.ToTable("agreements", "portal");

                entity.HasOne(d => d.AgreementCategory)
                    .WithMany(p => p!.Agreements)
                    .HasForeignKey(d => d.AgreementCategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_owqie84qkle78dasifljiwer");

                entity.HasOne(d => d.App)
                    .WithMany(p => p!.Agreements)
                    .HasForeignKey(d => d.AppId)
                    .HasConstraintName("fk_ooy9ydkah696jxh4lq7pn0xe");

                entity.HasOne(d => d.IssuerCompany)
                    .WithMany(p => p!.Agreements)
                    .HasForeignKey(d => d.IssuerCompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_n4nnf5bn8i3i9ijrf7kchfvc");

                entity.HasOne(d => d.UseCase)
                    .WithMany(p => p!.Agreements)
                    .HasForeignKey(d => d.UseCaseId)
                    .HasConstraintName("fk_whby66dika71srejhja6g75a");

            });

            modelBuilder.Entity<AgreementAssignedCompanyRole>(entity =>
            {
                entity.ToTable("agreement_assigned_company_roles", "portal");

                entity.HasKey(e => new { e.AgreementId, e.CompanyRoleId })
                    .HasName("pk_agreement_ass_comp_roles");

                entity.HasOne(d => d.Agreement)
                    .WithMany(p => p!.AgreementAssignedCompanyRoles)
                    .HasForeignKey(d => d.AgreementId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ljol11mdo76f4kv7fwqn1qc6");

                entity.HasOne(d => d.CompanyRole)
                    .WithMany(p => p!.AgreementAssignedCompanyRoles!)
                    .HasForeignKey(d => d.CompanyRoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_qh1hby9qcrr3gmy1cvi7nd3h");
            });

            modelBuilder.Entity<AgreementAssignedDocumentTemplate>(entity =>
            {
                entity.ToTable("agreement_assigned_document_templates", "portal");

                entity.HasKey(e => new { e.AgreementId, e.DocumentTemplateId })
                    .HasName("pk_agreement_ass_doc_templa");

                entity.HasIndex(e => e.DocumentTemplateId, "uk_9ib7xuc1vke96s9rvlyhxbtu")
                    .IsUnique();

                entity.HasOne(d => d.Agreement)
                    .WithMany(p => p!.AgreementAssignedDocumentTemplates)
                    .HasForeignKey(d => d.AgreementId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_fvcwoptsuer9p23m055osose");

                entity.HasOne(d => d.DocumentTemplate)
                    .WithOne(p => p!.AgreementAssignedDocumentTemplate!)
                    .HasForeignKey<AgreementAssignedDocumentTemplate>(d => d.DocumentTemplateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_bvrvs5aktrcn4t6565pnj3ur");
            });

            modelBuilder.Entity<AgreementCategory>(entity =>
            {
                entity.ToTable("agreement_categories", "portal");

                entity.Property(e => e.AgreementCategoryId)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<App>(entity =>
            {
                entity.ToTable("apps", "portal");

                entity.HasOne(d => d.ProviderCompany)
                    .WithMany(p => p!.ProvidedApps)
                    .HasForeignKey(d => d.ProviderCompanyId)
                    .HasConstraintName("fk_68a9joedhyf43smfx2xc4rgm");

                entity.HasOne(d => d.AppStatus)
                    .WithMany(p => p!.Apps)
                    .HasForeignKey(d => d.AppStatusId)
                    .HasConstraintName("fk_owihadhfweilwefhaf111aaa");

                entity.HasMany(p => p.Companies)
                    .WithMany(p => p.BoughtApps)
                    .UsingEntity<CompanyAssignedApp>(
                        j => j
                            .HasOne(d => d.Company!)
                            .WithMany()
                            .HasForeignKey(d => d.CompanyId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_k1dqlv81463yes0k8f2giyaf"),
                        j => j
                            .HasOne(d => d.App!)
                            .WithMany()
                            .HasForeignKey(d => d.AppId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_t365qpfvehuq40om25dyrnn5"),
                        j =>
                        {
                            j.HasKey(e => new { e.CompanyId, e.AppId }).HasName("pk_company_assigned_apps");
                            j.ToTable("company_assigned_apps", "portal");
                        }
                    );

                entity.HasMany(p => p.IamClients)
                    .WithMany(p => p.Apps)
                    .UsingEntity<AppAssignedClient>(
                        j => j
                            .HasOne(d => d.IamClient!)
                            .WithMany()
                            .HasForeignKey(d => d.IamClientId)
                            .HasConstraintName("fk_4m022ek8gffepnqlnuxwyxp8"),
                        j => j
                            .HasOne(d => d.App!)
                            .WithMany()
                            .HasForeignKey(d => d.AppId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_oayyvy590ngh5705yspep0up"),
                        j =>
                        {
                            j.HasKey(e => new{ e.AppId, e.IamClientId }).HasName("pk_app_assigned_clients");
                            j.ToTable("app_assigned_clients", "portal");
                        });

                entity.HasMany(a => a.SupportedLanguages)
                    .WithMany(l => l.SupportingApps)
                    .UsingEntity<AppLanguage>(
                        j => j
                            .HasOne(d => d.Language!)
                            .WithMany()
                            .HasForeignKey(d => d.LanguageShortName)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_oayyvy590ngh5705yspep102"),
                        j => j
                            .HasOne(d => d.App!)
                            .WithMany()
                            .HasForeignKey(d => d.AppId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_oayyvy590ngh5705yspep101"),
                        j =>
                        {
                            j.HasKey(e => new { e.AppId, e.LanguageShortName }).HasName("pk_app_language");
                            j.ToTable("app_languages", "portal");
                        }
                    );
                
                entity.HasMany(p => p.AppLicenses)
                    .WithMany(p => p.Apps)
                    .UsingEntity<AppAssignedLicense>(
                        j => j
                            .HasOne(d => d.AppLicense!)
                            .WithMany()
                            .HasForeignKey(d => d.AppLicenseId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_mes2xm3i1wotryfc88be4dkf"),
                        j => j
                            .HasOne(d => d.App!)
                            .WithMany()
                            .HasForeignKey(d => d.AppId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_3of613iyw1jx8gcj5i46jc1h"),
                        j =>
                        {
                            j.HasKey(e => new{ e.AppId, e.AppLicenseId }).HasName("pk_app_assigned_licenses");
                            j.ToTable("app_assigned_licenses", "portal");
                        });
                
                entity.HasMany(p => p.UseCases)
                    .WithMany(p => p.Apps)
                    .UsingEntity<AppAssignedUseCase>(
                        j => j
                            .HasOne(d => d.UseCase!)
                            .WithMany()
                            .HasForeignKey(d => d.UseCaseId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_sjyfs49ma0kxaqfknjbaye0i"),
                        j => j
                            .HasOne(d => d.App!)
                            .WithMany()
                            .HasForeignKey(d => d.AppId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_qi320sp8lxy7drw6kt4vheka"),
                        j =>
                        {
                            j.HasKey(e => new { e.AppId, e.UseCaseId }).HasName("pk_app_assigned_use_cases");
                            j.ToTable("app_assigned_use_cases", "portal");
                        });
            });

            modelBuilder.Entity<AppDescription>(entity =>
            {
                entity.ToTable("app_descriptions", "portal");

                entity.HasKey(e => new { e.AppId, e.LanguageShortName })
                    .HasName("app_descriptions_pkey");

                entity.Property(e => e.LanguageShortName)
                    .IsFixedLength(true);
                entity.HasOne(d => d.App)
                    .WithMany(p => p!.AppDescriptions)
                    .HasForeignKey(d => d.AppId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_qamy6j7s3klebrf2s69v9k0i");

                entity.HasOne(d => d.Language)
                    .WithMany(p => p!.AppDescriptions)
                    .HasForeignKey(d => d.LanguageShortName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_vrom2pjij9x6stgovhaqkfxf");
            });

            modelBuilder.Entity<AppDetailImage>(entity =>
            {
                entity.ToTable("app_detail_images", "portal");

                entity.HasOne(d => d.App)
                    .WithMany(p => p!.AppDetailImages)
                    .HasForeignKey(d => d.AppId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_oayyvy590ngh5705yspep12a");
            });

            modelBuilder.Entity<AppLicense>(entity =>
            {
                entity.ToTable("app_licenses", "portal");
            });

            modelBuilder.Entity<AppStatus>(entity =>
            {
                entity.ToTable("app_status", "portal");

                entity.Property(e => e.AppStatusId)
                    .ValueGeneratedNever();

                entity.HasData(
                    Enum.GetValues(typeof(AppStatusId))
                        .Cast<AppStatusId>()
                        .Select(e => new AppStatus(e)));
            });

            modelBuilder.Entity<AppTag>(entity =>
            {
                entity.ToTable("app_tags", "portal");

                entity.HasKey(e => new { e.AppId, e.Name })
                    .HasName("pk_app_tags");

                entity.HasOne(d => d.App)
                    .WithMany(p => p!.Tags)
                    .HasForeignKey(d => d.AppId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_qi320sp8lxy7drw6kt4vheka");
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("companies", "portal");

                entity.HasOne(d => d.Address)
                    .WithMany(p => p!.Companies)
                    .HasForeignKey(d => d.AddressId)
                    .HasConstraintName("fk_w70yf6urddd0ky7ev90okenf");

                entity.HasOne(d => d.CompanyStatus)
                    .WithMany(p => p!.Companies)
                    .HasForeignKey(d => d.CompanyStatusId)
                    .HasConstraintName("fk_owihadhfweilwefhaf682khj");

                entity.HasMany(p => p.CompanyRoles)
                    .WithMany(p => p.Companies)
                    .UsingEntity<CompanyAssignedRole>(
                        j => j
                            .HasOne(d => d.CompanyRole!)
                            .WithMany()
                            .HasForeignKey(d => d.CompanyRoleId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_my2p7jlqrjf0tq1f8rhk0i0a"),
                        j => j
                            .HasOne(d => d.Company!)
                            .WithMany()
                            .HasForeignKey(d => d.CompanyId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_4db4hgj3yvqlkn9h6q8m4e0j"),
                        j =>
                        {
                            j.HasKey(e => new { e.CompanyId, e.CompanyRoleId }).HasName("pk_company_assigned_roles");
                            j.ToTable("company_assigned_roles", "portal");
                        }
                    );

                entity.HasMany(p => p.CompanyAssignedRoles)
                    .WithOne(d => d.Company!)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_4db4hgj3yvqlkn9h6q8m4e0j");

                entity.HasMany(p => p.UseCases)
                    .WithMany(p => p.Companies)
                    .UsingEntity<CompanyAssignedUseCase>(
                        j => j
                            .HasOne(d => d.UseCase!)
                            .WithMany()
                            .HasForeignKey(d => d.UseCaseId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_m5eyaohrl0g9ju52byxsouqk"),
                        j => j
                            .HasOne(d => d.Company!)
                            .WithMany()
                            .HasForeignKey(d => d.CompanyId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_u65fkdrxnbkp8n0s7mate01v"),
                        j =>
                        {
                            j.HasKey(e => new { e.CompanyId, e.UseCaseId }).HasName("pk_company_assigned_use_cas");
                            j.ToTable("company_assigned_use_cases", "portal");
                        }
                    );

                entity.HasMany(p => p.IdentityProviders)
                    .WithMany(p => p.Companies)
                    .UsingEntity<CompanyIdentityProvider>(
                        j => j
                            .HasOne(pt => pt.IdentityProvider!)
                            .WithMany()
                            .HasForeignKey(pt => pt.IdentityProviderId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_iwzehadf8whjd8asjdfuwefhs"),
                        j => j
                            .HasOne(pt => pt.Company!)
                            .WithMany()
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasForeignKey(pt => pt.CompanyId)
                            .HasConstraintName("fk_haad983jkald89wlkejidk234"),
                        j => 
                        {
                            j.HasKey(e => new { e.CompanyId, e.IdentityProviderId })
                             .HasName("pk_company_identity_provider");
                            j.ToTable("company_identity_provider", "portal");
                        }
                    );
            });

            modelBuilder.Entity<CompanyApplication>(entity =>
            {
                entity.ToTable("company_applications", "portal");;

                entity.HasOne(d => d.ApplicationStatus)
                    .WithMany(p => p!.CompanyApplications)
                    .HasForeignKey(d => d.ApplicationStatusId)
                    .HasConstraintName("fk_akuwiehfiadf8928fhefhuda");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p!.CompanyApplications)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_3prv5i3o84vwvh7v0hh3sav7");
            });

            modelBuilder.Entity<CompanyApplicationStatus>(entity =>
            {
                entity.HasKey(e => e.ApplicationStatusId)
                    .HasName("company_application_status_pkey");

                entity.ToTable("company_application_status", "portal");

                entity.Property(e => e.ApplicationStatusId)
                    .ValueGeneratedNever();

                entity.HasData(
                    Enum.GetValues(typeof(CompanyApplicationStatusId))
                        .Cast<CompanyApplicationStatusId>()
                        .Select(e => new CompanyApplicationStatus(e)));
            });

            modelBuilder.Entity<CompanyRole>(entity =>
            {
                entity.ToTable("company_roles", "portal");

                entity.Property(e => e.CompanyRoleId)
                    .ValueGeneratedNever();

                entity.HasData(
                    Enum.GetValues(typeof(CompanyRoleId))
                        .Cast<CompanyRoleId>()
                        .Select(e => new CompanyRole(e)));
            });

            modelBuilder.Entity<CompanyRoleDescription>(entity =>
            {
                entity.ToTable("company_role_descriptions", "portal");

                entity.HasKey(e => new { e.CompanyRoleId, e.LanguageShortName })
                    .HasName("pk_company_role_descriptions");

                entity.HasOne(d => d.CompanyRole)
                    .WithMany(p => p!.CompanyRoleDescriptions)
                    .HasForeignKey(d => d.CompanyRoleId);
                
                entity.HasOne(d => d.Language)
                    .WithMany(p => p!.CompanyRoleDescriptions)
                    .HasForeignKey(d => d.LanguageShortName);
            });

            modelBuilder.Entity<CompanyStatus>(entity =>
            {
                entity.ToTable("company_status", "portal");

                entity.Property(e => e.CompanyStatusId)
                    .ValueGeneratedNever();

                entity.HasData(
                    Enum.GetValues(typeof(CompanyStatusId))
                        .Cast<CompanyStatusId>()
                        .Select(e => new CompanyStatus(e)));
            });

            modelBuilder.Entity<CompanyUser>(entity =>
            {
                entity.ToTable("company_users", "portal");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p!.CompanyUsers)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ku01366dbcqk8h32lh8k5sx1");

                entity.HasOne(d => d.CompanyUserStatus)
                    .WithMany(p => p!.CompanyUsers)
                    .HasForeignKey(d => d.CompanyUserStatusId)
                    .HasConstraintName("fk_company_users_company_user_status_id");
                
                entity.HasMany(p => p.Apps)
                    .WithMany(p => p.CompanyUsers)
                    .UsingEntity<CompanyUserAssignedAppFavourite>(
                        j => j
                            .HasOne(d => d.App!)
                            .WithMany()
                            .HasForeignKey(d => d.AppId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_eip97mygnbglivrtmkagesjh"),
                        j => j
                            .HasOne(d => d.CompanyUser!)
                            .WithMany()
                            .HasForeignKey(d => d.CompanyUserId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_wva553r3xiew3ngbdkvafk85"),
                        j =>
                        {
                            j.HasKey(e => new { e.CompanyUserId, e.AppId }).HasName("pk_comp_user_ass_app_favour");
                            j.ToTable("company_user_assigned_app_favourites", "portal");
                        });
                
                entity.HasMany(p => p.UserRoles)
                    .WithMany(p => p.CompanyUsers)
                    .UsingEntity<CompanyUserAssignedRole>(
                        j => j
                            .HasOne(d => d.UserRole!)
                            .WithMany()
                            .HasForeignKey(d => d.UserRoleId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_bw1yhel67uhrxfk7mevovq5p"),
                        j => j
                            .HasOne(d => d.CompanyUser!)
                            .WithMany()
                            .HasForeignKey(d => d.CompanyUserId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_0c9rjjf9gm3l0n6reb4o0f1s"),
                        j =>
                        {
                            j.HasKey(e => new { e.CompanyUserId, e.UserRoleId }).HasName("pk_comp_user_assigned_roles");
                            j.ToTable("company_user_assigned_roles", "portal");
                        });
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("user_roles", "portal");

                entity.HasOne(d => d.IamClient)
                    .WithMany(p => p!.UserRoles)
                    .HasForeignKey(d => d.IamClientId);
            });

            modelBuilder.Entity<UserRoleDescription>(entity =>
            {
                entity.ToTable("user_role_descriptions", "portal");

                entity.HasKey(e => new { e.UserRoleId, e.LanguageShortName })
                    .HasName("pk_company_role_descriptions");

                entity.HasOne(d => d.UserRole)
                    .WithMany(p => p!.UserRoleDescriptions)
                    .HasForeignKey(d => d.UserRoleId);
                
                entity.HasOne(d => d.Language)
                    .WithMany(p => p!.UserRoleDescriptions)
                    .HasForeignKey(d => d.LanguageShortName);
            });

            modelBuilder.Entity<CompanyUserStatus>(entity =>
            {
                entity.ToTable("company_user_status", "portal");

                entity.Property(e => e.CompanyUserStatusId)
                    .ValueGeneratedNever();

                entity.HasData(
                    Enum.GetValues(typeof(CompanyUserStatusId))
                        .Cast<CompanyUserStatusId>()
                        .Select(e => new CompanyUserStatus(e)));
            });

            modelBuilder.Entity<Consent>(entity =>
            {
                entity.ToTable("consents", "portal");

                entity.HasOne(d => d.Agreement)
                    .WithMany(p => p!.Consents)
                    .HasForeignKey(d => d.AgreementId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_39a5cbiv35v59ysgfon5oole");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p!.Consents)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_asqxie2r7yr06cdrw9ifaex8");

                entity.HasOne(d => d.CompanyUser)
                    .WithMany(p => p!.Consents)
                    .HasForeignKey(d => d.CompanyUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_cnrtafckouq96m0fw2qtpwbs");

                entity.HasOne(d => d.ConsentStatus)
                    .WithMany(p => p!.Consents)
                    .HasForeignKey(d => d.ConsentStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_aiodhuwehw8wee20adskdfo2");

                entity.HasOne(d => d.Document)
                    .WithMany(p => p!.Consents)
                    .HasForeignKey(d => d.DocumentId)
                    .HasConstraintName("fk_36j22f34lgi2444n4tynxamh");
            });

            modelBuilder.Entity<ConsentStatus>(entity =>
            {
                entity.ToTable("consent_status", "portal");

                entity.Property(e => e.ConsentStatusId)
                    .ValueGeneratedNever();

                entity.HasData(
                    Enum.GetValues(typeof(ConsentStatusId))
                        .Cast<ConsentStatusId>()
                        .Select(e => new ConsentStatus(e)));
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("countries", "portal");

                entity.Property(e => e.Alpha2Code)
                    .IsFixedLength(true);

                entity.Property(e => e.Alpha3Code)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("documents", "portal");

                entity.HasOne(d => d.DocumentType)
                    .WithMany(p => p!.Documents)
                    .HasForeignKey(d => d.DocumentTypeId)
                    .HasConstraintName("fk_xcgobngn7vk56k8nfkualsvn");

                entity.HasOne(d => d.CompanyUser)
                    .WithMany(p => p!.Documents)
                    .HasForeignKey(d => d.CompanyUserId)
                    .HasConstraintName("fk_xcgobngn7vk56k8nfkuaysvn");
            });

            modelBuilder.Entity<DocumentTemplate>(entity =>
            {
                entity.ToTable("document_templates", "portal");
            });

            modelBuilder.Entity<DocumentType>(entity =>
            {
                entity.ToTable("document_types", "portal");

                entity.Property(e => e.DocumentTypeId)
                    .ValueGeneratedNever();

                entity.HasData(
                    Enum.GetValues(typeof(DocumentTypeId))
                        .Cast<DocumentTypeId>()
                        .Select(e => new DocumentType(e)));
            });

            modelBuilder.Entity<IamClient>(entity =>
            {
                entity.ToTable("iam_clients", "portal");

                entity.HasIndex(e => e.ClientClientId, "uk_iam_client_client_client_id")
                    .IsUnique();
            });

            modelBuilder.Entity<IamIdentityProvider>(entity =>
            {
                entity.ToTable("iam_identity_providers", "portal");

                entity.HasIndex(e => e.IdentityProviderId, "uk_aiehoat94wlhasdfiwlkefisi")
                    .IsUnique();

                entity.HasOne(d => d.IdentityProvider)
                    .WithOne(p => p!.IamIdentityProvider!)
                    .HasForeignKey<IamIdentityProvider>(d => d.IdentityProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_9balkda89j2498dkj2lkjd9s3");
            });

            modelBuilder.Entity<IamUser>(entity =>
            {
                entity.ToTable("iam_users", "portal");

                entity.HasIndex(e => e.CompanyUserId, "uk_wiodwiowhdfo84f0sd9afsd2")
                    .IsUnique();

                entity.HasOne(d => d.CompanyUser)
                    .WithOne(p => p!.IamUser!)
                    .HasForeignKey<IamUser>(d => d.CompanyUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_iweorqwaeilskjeijekkalwo");
            });

            modelBuilder.Entity<IdentityProvider>(entity =>
            {
                entity.ToTable("identity_providers", "portal");

                entity.HasOne(d => d.IdentityProviderCategory)
                    .WithMany(p => p!.IdentityProviders)
                    .HasForeignKey(d => d.IdentityProviderCategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_iwohgwi9342adf9asdnfuie28");
            });

            modelBuilder.Entity<IdentityProviderCategory>(entity =>
            {
                entity.ToTable("identity_provider_categories", "portal");

                entity.Property(e => e.IdentityProviderCategoryId)
                    .ValueGeneratedNever();

                entity.HasData(
                    Enum.GetValues(typeof(IdentityProviderCategoryId))
                        .Cast<IdentityProviderCategoryId>()
                        .Select(e => new IdentityProviderCategory(e)));
            });

            modelBuilder.Entity<Invitation>(entity =>
            {
                entity.ToTable("invitations", "portal");

                entity.HasOne(d => d.CompanyApplication)
                    .WithMany(p => p!.Invitations)
                    .HasForeignKey(d => d.CompanyApplicationId)
                    .HasConstraintName("fk_dlrst4ju9d0wcgkh4w1nnoj3");

                entity.HasOne(d => d.CompanyUser)
                    .WithMany(p => p!.Invitations)
                    .HasForeignKey(d => d.CompanyUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_9tgenb7p09hr5c24haxjw259");

                entity.HasOne(d => d.InvitationStatus)
                    .WithMany(p => p!.Invitations)
                    .HasForeignKey(d => d.InvitationStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_woihaodhawoeir72alfidosd");
            });

            modelBuilder.Entity<InvitationStatus>(entity =>
            {
                entity.ToTable("invitation_status", "portal");

                entity.Property(e => e.InvitationStatusId)
                    .ValueGeneratedNever();

                entity.HasData(
                    Enum.GetValues(typeof(InvitationStatusId))
                        .Cast<InvitationStatusId>()
                        .Select(e => new InvitationStatus(e)));
            });

            modelBuilder.Entity<Language>(entity =>
            {
                entity.ToTable("languages", "portal");

                entity.Property(e => e.ShortName)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<UseCase>(entity =>
            {
                entity.ToTable("use_cases", "portal");
            });
        }
    }
}
