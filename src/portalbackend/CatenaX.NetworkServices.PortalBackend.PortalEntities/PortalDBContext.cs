using System;
using System.Linq;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities
{
    public class PortalDBContext : DbContext
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PortalDBContext()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PortalDBContext(DbContextOptions<PortalDBContext> options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            : base(options)
        {
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<Agreement> Agreements { get; set; }
        public DbSet<AgreementAssignedCompanyRole> AgreementAssignedCompanyRoles { get; set; }
        public DbSet<AgreementAssignedDocumentTemplate> AgreementAssignedDocumentTemplates { get; set; }
        public DbSet<AgreementCategory> AgreementCategories { get; set; }
        public DbSet<App> Apps { get; set; }
        public DbSet<AppAssignedCompanyUserRole> AppAssignedCompanyUserRoles { get; set; }
        public DbSet<AppAssignedLicense> AppAssignedLicenses { get; set; }
        public DbSet<AppAssignedUseCase> AppAssignedUseCases { get; set; }
        public DbSet<AppDescription> AppDescriptions { get; set; }
        public DbSet<AppLicense> AppLicenses { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyApplication> CompanyApplications { get; set; }
        public DbSet<CompanyApplicationStatus> CompanyApplicationStatuses { get; set; }
        // public DbSet<CompanyAssignedApp> CompanyAssignedApps { get; set; }
        // public DbSet<CompanyAssignedRole> CompanyAssignedRoles { get; set; }
        // public DbSet<CompanyAssignedUseCase> CompanyAssignedUseCases { get; set; }
        // public DbSet<CompanyIdentityProvider> CompanyIdentityProviders { get; set; }
        public DbSet<CompanyRole> CompanyRoles { get; set; }
        public DbSet<CompanyStatus> CompanyStatuses { get; set; }
        public DbSet<CompanyUser> CompanyUsers { get; set; }
        public DbSet<CompanyUserAssignedAppFavourite> CompanyUserAssignedAppFavourites { get; set; }
        public DbSet<CompanyUserAssignedRole> CompanyUserAssignedRoles { get; set; }
        public DbSet<CompanyUserRole> CompanyUserRoles { get; set; }
        public DbSet<Consent> Consents { get; set; }
        public DbSet<ConsentStatus> ConsentStatuses { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentTemplate> DocumentTemplates { get; set; }
        public DbSet<IamIdentityProvider> IamIdentityProviders { get; set; }
        public DbSet<IamUser> IamUsers { get; set; }
        public DbSet<IdentityProvider> IdentityProviders { get; set; }
        public DbSet<IdentityProviderCategory> IdentityProviderCategories { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<InvitationStatus> InvitationStatuses { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<UseCase> UseCases { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "en_US.utf8");

            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("addresses", "portal");

                entity.Property(e => e.City)
                    .HasColumnName("city");

                entity.Property(e => e.CountryAlpha2Code)
                    .HasColumnName("country_alpha_2_code")
                    .IsFixedLength(true);

                entity.Property(e => e.Region)
                    .HasColumnName("region");

                entity.Property(e => e.Streetadditional)
                    .HasColumnName("streetadditional");

                entity.Property(e => e.Streetname)
                    .HasColumnName("streetname");

                entity.Property(e => e.Streetnumber)
                    .HasColumnName("streetnumber");

                entity.Property(e => e.Zipcode)
                    .HasPrecision(19, 2)
                    .HasColumnName("zipcode");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p!.Addresses)
                    .HasForeignKey(d => d.CountryAlpha2Code)
                    .HasConstraintName("fk_6jg6itw07d2qww62deuyk0kh");
            });

            modelBuilder.Entity<Agreement>(entity =>
            {
                entity.ToTable("agreements", "portal");

                entity.Property(e => e.AgreementCategoryId).HasColumnName("agreement_category_id");

                entity.Property(e => e.AgreementType)
                    .HasColumnName("agreement_type");

                entity.Property(e => e.AppId).HasColumnName("app_id");

                entity.Property(e => e.IssuerCompanyId).HasColumnName("issuer_company_id");

                entity.Property(e => e.Name)
                    .HasColumnName("name");

                entity.Property(e => e.UseCaseId).HasColumnName("use_case_id");

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

                entity.HasIndex(e => e.CompanyRoleId, "uk_6df9o1r7dy987w1pt9qnkopc")
                    .IsUnique();

                entity.Property(e => e.AgreementId).HasColumnName("agreement_id");

                entity.Property(e => e.CompanyRoleId).HasColumnName("company_role_id");

                entity.HasOne(d => d.Agreement)
                    .WithMany(p => p.AgreementAssignedCompanyRoles)
                    .HasForeignKey(d => d.AgreementId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ljol11mdo76f4kv7fwqn1qc6");

                entity.HasOne(d => d.CompanyRole)
                    .WithOne(p => p.AgreementAssignedCompanyRole!)
                    .HasForeignKey<AgreementAssignedCompanyRole>(d => d.CompanyRoleId)
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

                entity.Property(e => e.AgreementId).HasColumnName("agreement_id");

                entity.Property(e => e.DocumentTemplateId).HasColumnName("document_template_id");

                entity.HasOne(d => d.Agreement)
                    .WithMany(p => p.AgreementAssignedDocumentTemplates)
                    .HasForeignKey(d => d.AgreementId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_fvcwoptsuer9p23m055osose");

                entity.HasOne(d => d.DocumentTemplate)
                    .WithOne(p => p.AgreementAssignedDocumentTemplate!)
                    .HasForeignKey<AgreementAssignedDocumentTemplate>(d => d.DocumentTemplateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_bvrvs5aktrcn4t6565pnj3ur");
            });

            modelBuilder.Entity<AgreementCategory>(entity =>
            {
                entity.ToTable("agreement_categories", "portal");

                entity.Property(e => e.AgreementCategoryId)
                    .ValueGeneratedNever()
                    .HasColumnName("agreement_category_id");

                entity.Property(e => e.Label)
                    .HasColumnName("label");
            });

            modelBuilder.Entity<App>(entity =>
            {
                entity.ToTable("apps", "portal");

                entity.Property(e => e.DateReleased).HasColumnName("date_released");

                entity.Property(e => e.Name)
                    .HasColumnName("name");

                entity.Property(e => e.ThumbnailUrl)
                    .HasColumnName("thumbnail_url");

                entity.Property(e => e.AppUrl)
                    .HasColumnName("app_url");

                entity.Property(e => e.MarketingUrl)
                    .HasColumnName("marketing_url");

                entity.Property(e => e.VendorCompanyId).HasColumnName("vendor_company_id");

                entity.HasOne(d => d.VendorCompany)
                    .WithMany(p => p!.ProvidedApps)
                    .HasForeignKey(d => d.VendorCompanyId)
                    .HasConstraintName("fk_68a9joedhyf43smfx2xc4rgm");

                entity.HasMany(p => p.Companies)
                    .WithMany(p => p.BoughtApps)
                    .UsingEntity<CompanyAssignedApp>(
                        j => j
                            .HasOne(d => d.Company)
                            .WithMany()
                            .HasForeignKey(d => d.CompanyId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_k1dqlv81463yes0k8f2giyaf"),
                        j => j
                            .HasOne(d => d.App)
                            .WithMany()
                            .HasForeignKey(d => d.AppId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_t365qpfvehuq40om25dyrnn5"),
                        j =>
                        {
                            j.HasKey(e => new { e.CompanyId, e.AppId }).HasName("pk_company_assigned_apps");
                            j.ToTable("company_assigned_apps", "portal");
                            j.Property(e => e.AppId).HasColumnName("app_id");
                            j.Property(e => e.CompanyId).HasColumnName("company_id");
                        }
                    );

                entity.HasMany(p => p.CompanyUserRoles)
                    .WithMany(p => p.Apps)
                    .UsingEntity<AppAssignedCompanyUserRole>(
                        j => j
                            .HasOne(d => d.CompanyUserRole)
                            .WithMany()
                            .HasForeignKey(d => d.CompanyUserRoleId)
                            .HasConstraintName("fk_4m022ek8gffepnqlnuxwyxp8"),
                        j => j
                            .HasOne(d => d.App)
                            .WithMany()
                            .HasForeignKey(d => d.AppId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_oayyvy590ngh5705yspep0up"),
                        j =>
                        {
                            j.HasKey(e => new{ e.AppId, e.CompanyUserRoleId }).HasName("pk_app_assg_comp_user_roles");
                            j.ToTable("app_assigned_company_user_roles", "portal");
                            j.Property(e => e.AppId).HasColumnName("app_id");
                            j.Property(e => e.CompanyUserRoleId).HasColumnName("company_user_role_id");
                        });
                
                entity.HasMany(p => p.AppLicenses)
                    .WithMany(p => p.Apps)
                    .UsingEntity<AppAssignedLicense>(
                        j => j
                            .HasOne(d => d.AppLicense)
                            .WithMany()
                            .HasForeignKey(d => d.AppLicenseId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_mes2xm3i1wotryfc88be4dkf"),
                        j => j
                            .HasOne(d => d.App)
                            .WithMany()
                            .HasForeignKey(d => d.AppId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_3of613iyw1jx8gcj5i46jc1h"),
                        j =>
                        {
                            j.HasKey(e => new{ e.AppId, e.AppLicenseId }).HasName("pk_app_assigned_licenses");
                            j.ToTable("app_assigned_licenses", "portal");
                            j.Property(e => e.AppId).HasColumnName("app_id");
                            j.Property(e => e.AppLicenseId).HasColumnName("app_license_id");
                        });
                
                entity.HasMany(p => p.UseCases)
                    .WithMany(p => p.Apps)
                    .UsingEntity<AppAssignedUseCase>(
                        j => j
                            .HasOne(d => d.UseCase)
                            .WithMany()
                            .HasForeignKey(d => d.UseCaseId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_sjyfs49ma0kxaqfknjbaye0i"),
                        j => j
                            .HasOne(d => d.App)
                            .WithMany()
                            .HasForeignKey(d => d.AppId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_qi320sp8lxy7drw6kt4vheka"),
                        j =>
                        {
                            j.HasKey(e => new { e.AppId, e.UseCaseId }).HasName("pk_app_assigned_use_cases");
                            j.ToTable("app_assigned_use_cases", "portal");
                            j.Property(e => e.AppId).HasColumnName("app_id");
                            j.Property(e => e.UseCaseId).HasColumnName("use_case_id");
                        });
            });

            modelBuilder.Entity<AppDescription>(entity =>
            {
                entity.ToTable("app_descriptions", "portal");

                entity.HasKey(e => new { e.AppId, e.LanguageShortName })
                    .HasName("app_descriptions_pkey");

                entity.Property(e => e.AppId).HasColumnName("app_id");

                entity.Property(e => e.LanguageShortName)
                    .HasColumnName("language_short_name")
                    .IsFixedLength(true);

                entity.Property(e => e.DateCreated).HasColumnName("date_created");

                entity.Property(e => e.DateLastChanged).HasColumnName("date_last_changed");

                entity.Property(e => e.DescriptionLong)
                    .HasColumnName("description_long");

                entity.Property(e => e.DescriptionShort)
                    .HasColumnName("description_short");

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

            modelBuilder.Entity<AppLicense>(entity =>
            {
                entity.ToTable("app_licenses", "portal");

                entity.Property(e => e.Licensetext)
                    .HasColumnName("licensetext");
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("companies", "portal");

                entity.Property(e => e.AddressId).HasColumnName("address_id");

                entity.Property(e => e.Bpn)
                    .HasColumnName("bpn");

                entity.Property(e => e.CompanyStatusId)
                    .HasColumnName("company_status_id")
                    .HasConversion<int>();

                entity.Property(e => e.Name)
                    .HasColumnName("name");

                entity.Property(e => e.Parent)
                    .HasColumnName("parent");

                entity.Property(e => e.Shortname)
                    .HasColumnName("shortname");

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
                            .HasOne(d => d.CompanyRole)
                            .WithMany()
                            .HasForeignKey(d => d.CompanyRoleId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_my2p7jlqrjf0tq1f8rhk0i0a"),
                        j => j
                            .HasOne(d => d.Company)
                            .WithMany()
                            .HasForeignKey(d => d.CompanyId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_4db4hgj3yvqlkn9h6q8m4e0j"),
                        j =>
                        {
                            j.HasKey(e => new { e.CompanyId, e.CompanyRoleId }).HasName("pk_company_assigned_roles");
                            j.ToTable("company_assigned_roles", "portal");
                            j.Property(e => e.CompanyId).HasColumnName("company_id");
                            j.Property(e => e.CompanyRoleId).HasColumnName("company_role_id");
                        }
                    );

                entity.HasMany(p => p.UseCases)
                    .WithMany(p => p.Companies)
                    .UsingEntity<CompanyAssignedUseCase>(
                        j => j
                            .HasOne(d => d.UseCase)
                            .WithMany()
                            .HasForeignKey(d => d.UseCaseId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_m5eyaohrl0g9ju52byxsouqk"),
                        j => j
                            .HasOne(d => d.Company)
                            .WithMany()
                            .HasForeignKey(d => d.CompanyId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_u65fkdrxnbkp8n0s7mate01v"),
                        j =>
                        {
                            j.HasKey(e => new { e.CompanyId, e.UseCaseId }).HasName("pk_company_assigned_use_cas");
                            j.ToTable("company_assigned_use_cases", "portal");
                            j.Property(e => e.CompanyId).HasColumnName("company_id");
                            j.Property(e => e.UseCaseId).HasColumnName("use_case_id");
                        }
                    );

                entity.HasMany(p => p.IdentityProviders)
                    .WithMany(p => p.Companies)
                    .UsingEntity<CompanyIdentityProvider>(
                        j => j
                            .HasOne(pt => pt.IdentityProvider)
                            .WithMany()
                            .HasForeignKey(pt => pt.IdentityProviderId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_iwzehadf8whjd8asjdfuwefhs"),
                        j => j
                            .HasOne(pt => pt.Company)
                            .WithMany()
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasForeignKey(pt => pt.CompanyId)
                            .HasConstraintName("fk_haad983jkald89wlkejidk234"),
                        j => 
                        {
                            j.HasKey(e => new { e.CompanyId, e.IdentityProviderId })
                             .HasName("pk_company_identity_provider");
                            j.ToTable("company_identity_provider", "portal");
                            j.Property(e => e.CompanyId).HasColumnName("company_id");
                            j.Property(e => e.IdentityProviderId).HasColumnName("identity_provider_id");
                        }
                    );
            });

            modelBuilder.Entity<CompanyApplication>(entity =>
            {
                entity.ToTable("company_applications", "portal");

                entity.Property(e => e.ApplicationStatusId)
                    .HasColumnName("application_status_id")
                    .HasConversion<int>();

                entity.Property(e => e.CompanyId).HasColumnName("company_id");

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
                    .ValueGeneratedNever()
                    .HasColumnName("application_status_id")
                    .HasConversion<int>();

                entity.Property(e => e.Label)
                    .HasColumnName("label");

                entity.HasData(
                    Enum.GetValues(typeof(CompanyApplicationStatusId))
                        .Cast<CompanyApplicationStatusId>()
                        .Select(e => new CompanyApplicationStatus()
                        {
                            ApplicationStatusId = e,
                            Label = e.ToString()
                        }));
            });

            modelBuilder.Entity<CompanyRole>(entity =>
            {
                entity.ToTable("company_roles", "portal");

                entity.Property(e => e.CompanyRoleText)
                    .HasColumnName("company_role");

                entity.Property(e => e.NameDe)
                    .HasColumnName("name_de");

                entity.Property(e => e.NameEn)
                    .HasColumnName("name_en");
            });

            modelBuilder.Entity<CompanyStatus>(entity =>
            {
                entity.ToTable("company_status", "portal");

                entity.Property(e => e.CompanyStatusId)
                    .ValueGeneratedNever()
                    .HasColumnName("company_status_id")
                    .HasConversion<int>();

                entity.Property(e => e.Label)
                    .HasColumnName("label");

                entity.HasData(
                    Enum.GetValues(typeof(CompanyStatusId))
                        .Cast<CompanyStatusId>()
                        .Select(e => new CompanyStatus()
                        {
                            CompanyStatusId = e,
                            Label = e.ToString()
                        }));
            });

            modelBuilder.Entity<CompanyUser>(entity =>
            {
                entity.ToTable("company_users", "portal");

                entity.Property(e => e.CompanyId).HasColumnName("company_id");

                entity.Property(e => e.Email)
                    .HasColumnName("email");

                entity.Property(e => e.Firstname)
                    .HasColumnName("firstname");

                entity.Property(e => e.Lastlogin).HasColumnName("lastlogin");

                entity.Property(e => e.Lastname)
                    .HasColumnName("lastname");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p!.CompanyUsers)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ku01366dbcqk8h32lh8k5sx1");
                
                entity.HasMany(p => p.Apps)
                    .WithMany(p => p.CompanyUsers)
                    .UsingEntity<CompanyUserAssignedAppFavourite>(
                        j => j
                            .HasOne(d => d.App)
                            .WithMany()
                            .HasForeignKey(d => d.AppId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_eip97mygnbglivrtmkagesjh"),
                        j => j
                            .HasOne(d => d.CompanyUser)
                            .WithMany()
                            .HasForeignKey(d => d.CompanyUserId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_wva553r3xiew3ngbdkvafk85"),
                        j =>
                        {
                            j.HasKey(e => new { e.CompanyUserId, e.AppId }).HasName("pk_comp_user_ass_app_favour");
                            j.ToTable("company_user_assigned_app_favourites", "portal");
                            j.Property(e => e.AppId).HasColumnName("app_id");
                            j.Property(e => e.CompanyUserId).HasColumnName("company_user_id");
                        });
                
                entity.HasMany(p => p.CompanyUserRoles)
                    .WithMany(p => p.CompanyUsers)
                    .UsingEntity<CompanyUserAssignedRole>(
                        j => j
                            .HasOne(d => d.UserRole)
                            .WithMany()
                            .HasForeignKey(d => d.UserRoleId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_bw1yhel67uhrxfk7mevovq5p"),
                        j => j
                            .HasOne(d => d.CompanyUser)
                            .WithMany()
                            .HasForeignKey(d => d.CompanyUserId)
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_0c9rjjf9gm3l0n6reb4o0f1s"),
                        j =>
                        {
                            j.HasKey(e => new { e.CompanyUserId, e.UserRoleId }).HasName("pk_comp_user_assigned_roles");
                            j.ToTable("company_user_assigned_roles", "portal");
                            j.Property(e => e.CompanyUserId).HasColumnName("company_user_id");
                            j.Property(e => e.UserRoleId).HasColumnName("user_role_id");
                        });
            });

            modelBuilder.Entity<CompanyUserRole>(entity =>
            {
                entity.ToTable("company_user_roles", "portal");

                entity.Property(e => e.CompanyUserRoleText)
                    .HasColumnName("company_user_role");

                entity.Property(e => e.Namede)
                    .HasColumnName("namede");

                entity.Property(e => e.Nameen)
                    .HasColumnName("nameen");
            });

            modelBuilder.Entity<Consent>(entity =>
            {
                entity.ToTable("consents", "portal");

                entity.Property(e => e.AgreementId).HasColumnName("agreement_id");

                entity.Property(e => e.Comment)
                    .HasColumnName("comment");

                entity.Property(e => e.CompanyId).HasColumnName("company_id");

                entity.Property(e => e.CompanyUserId).HasColumnName("company_user_id");

                entity.Property(e => e.ConsentStatusId).HasColumnName("consent_status_id");

                entity.Property(e => e.DocumentsId).HasColumnName("documents_id");

                entity.Property(e => e.Target)
                    .HasColumnName("target");

                entity.Property(e => e.Timestamp)
                    .HasColumnName("timestamp");

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

                entity.HasOne(d => d.Documents)
                    .WithMany(p => p!.Consents)
                    .HasForeignKey(d => d.DocumentsId)
                    .HasConstraintName("fk_36j22f34lgi2444n4tynxamh");
            });

            modelBuilder.Entity<ConsentStatus>(entity =>
            {
                entity.ToTable("consent_status", "portal");

                entity.Property(e => e.ConsentStatusId)
                    .ValueGeneratedNever()
                    .HasColumnName("consent_status_id");

                entity.Property(e => e.Label)
                    .HasColumnName("label");
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("countries", "portal");

                entity.HasKey(e => e.Alpha2Code)
                    .HasName("countries_pkey");

                entity.Property(e => e.Alpha2Code)
                    .HasColumnName("alpha_2_code")
                    .IsFixedLength(true);

                entity.Property(e => e.Alpha3Code)
                    .HasColumnName("alpha_3_code")
                    .IsFixedLength(true);

                entity.Property(e => e.CountryNameDe)
                    .HasColumnName("country_name_de");

                entity.Property(e => e.CountryNameEn)
                    .HasColumnName("country_name_en");
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("documents", "portal");

                entity.Property(e => e.CompanyUserId).HasColumnName("company_user_id");

                entity.Property(e => e.DocumentOid)
                    .HasColumnType("oid")
                    .HasColumnName("document");

                entity.Property(e => e.Documenthash)
                    .HasColumnName("documenthash");

                entity.Property(e => e.Documentname)
                    .HasColumnName("documentname");

                entity.Property(e => e.Documentuploaddate)
                    .HasColumnName("documentuploaddate");

                entity.Property(e => e.Documentversion)
                    .HasColumnName("documentversion");

                entity.HasOne(d => d.CompanyUser)
                    .WithMany(p => p!.Documents)
                    .HasForeignKey(d => d.CompanyUserId)
                    .HasConstraintName("fk_xcgobngn7vk56k8nfkuaysvn");
            });

            modelBuilder.Entity<DocumentTemplate>(entity =>
            {
                entity.ToTable("document_templates", "portal");

                entity.Property(e => e.Documenttemplatename)
                    .HasColumnName("documenttemplatename");

                entity.Property(e => e.Documenttemplateversion)
                    .HasColumnName("documenttemplateversion");
            });

            modelBuilder.Entity<IamIdentityProvider>(entity =>
            {
                entity.ToTable("iam_identity_providers", "portal");

                entity.HasKey(e => e.IamIdpAlias)
                    .HasName("iam_identity_providers_pkey");

                entity.HasIndex(e => e.IdentityProviderId, "uk_aiehoat94wlhasdfiwlkefisi")
                    .IsUnique();

                entity.Property(e => e.IamIdpAlias)
                    .HasColumnName("iam_idp_alias");

                entity.Property(e => e.IdentityProviderId).HasColumnName("identity_provider_id");

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

                entity.Property(e => e.CompanyUserId).HasColumnName("company_user_id");

                entity.HasOne(d => d.CompanyUser)
                    .WithOne(p => p!.IamUser!)
                    .HasForeignKey<IamUser>(d => d.CompanyUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_iweorqwaeilskjeijekkalwo");
            });

            modelBuilder.Entity<IdentityProvider>(entity =>
            {
                entity.ToTable("identity_providers", "portal");

                entity.Property(e => e.IdentityProviderCategoryId)
                    .HasColumnName("identity_provider_category_id")
                    .HasConversion<int>();

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
                    .ValueGeneratedNever()
                    .HasColumnName("identity_provider_category_id")
                    .HasConversion<int>();

                entity.Property(e => e.Label)
                    .HasColumnName("label");

                entity.HasData(
                    Enum.GetValues(typeof(IdentityProviderCategoryId))
                        .Cast<IdentityProviderCategoryId>()
                        .Select(e => new IdentityProviderCategory()
                        {
                            IdentityProviderCategoryId = e,
                            Label = e.ToString()
                        }));
            });

            modelBuilder.Entity<Invitation>(entity =>
            {
                entity.ToTable("invitations", "portal");

                entity.Property(e => e.CompanyApplicationId).HasColumnName("company_application_id");

                entity.Property(e => e.CompanyUserId).HasColumnName("company_user_id");

                entity.Property(e => e.InvitationStatusId)
                    .HasColumnName("invitation_status_id")
                    .HasConversion<int>();

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
                    .ValueGeneratedNever()
                    .HasColumnName("invitation_status_id")
                    .HasConversion<int>();

                entity.Property(e => e.Label)
                    .HasColumnName("label");

                entity.HasData(
                    Enum.GetValues(typeof(InvitationStatusId))
                        .Cast<InvitationStatusId>()
                        .Select(e => new InvitationStatus()
                        {
                            InvitationStatusId = e,
                            Label = e.ToString()
                        }));
            });

            modelBuilder.Entity<Language>(entity =>
            {
                entity.ToTable("languages", "portal");

                entity.HasKey(e => e.LanguageShortName)
                    .HasName("languages_pkey");

                entity.Property(e => e.LanguageShortName)
                    .HasColumnName("language_short_name")
                    .IsFixedLength(true);

                entity.Property(e => e.LongNameDe)
                    .HasColumnName("long_name_de");

                entity.Property(e => e.LongNameEn)
                    .HasColumnName("long_name_en");
            });

            modelBuilder.Entity<UseCase>(entity =>
            {
                entity.ToTable("use_cases", "portal");

                entity.Property(e => e.Name)
                    .HasColumnName("name");

                entity.Property(e => e.Shortname)
                    .HasColumnName("shortname");
            });
        }
    }
}
