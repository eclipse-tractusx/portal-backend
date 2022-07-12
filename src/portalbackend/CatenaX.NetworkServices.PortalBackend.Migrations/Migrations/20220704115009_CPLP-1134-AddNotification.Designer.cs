﻿/********************************************************************************
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

using System;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    [DbContext(typeof(PortalDbContext))]
    [Migration("20220704115009_CPLP-1134-AddNotification")]
    partial class CPLP1134AddNotification
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("portal")
                .UseCollation("en_US.utf8")
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Address", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("city");

                    b.Property<string>("CountryAlpha2Code")
                        .IsRequired()
                        .HasMaxLength(2)
                        .HasColumnType("character(2)")
                        .HasColumnName("country_alpha2code");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<DateTimeOffset?>("DateLastChanged")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_last_changed");

                    b.Property<string>("Region")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("region");

                    b.Property<string>("Streetadditional")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("streetadditional");

                    b.Property<string>("Streetname")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("streetname");

                    b.Property<string>("Streetnumber")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("streetnumber");

                    b.Property<string>("Zipcode")
                        .HasMaxLength(12)
                        .HasColumnType("character varying(12)")
                        .HasColumnName("zipcode");

                    b.HasKey("Id")
                        .HasName("pk_addresses");

                    b.HasIndex("CountryAlpha2Code")
                        .HasDatabaseName("ix_addresses_country_alpha2code");

                    b.ToTable("addresses", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Agreement", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<int>("AgreementCategoryId")
                        .HasColumnType("integer")
                        .HasColumnName("agreement_category_id");

                    b.Property<string>("AgreementType")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("agreement_type");

                    b.Property<Guid?>("AppId")
                        .HasColumnType("uuid")
                        .HasColumnName("app_id");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<DateTimeOffset?>("DateLastChanged")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_last_changed");

                    b.Property<Guid>("IssuerCompanyId")
                        .HasColumnType("uuid")
                        .HasColumnName("issuer_company_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("name");

                    b.Property<Guid?>("UseCaseId")
                        .HasColumnType("uuid")
                        .HasColumnName("use_case_id");

                    b.HasKey("Id")
                        .HasName("pk_agreements");

                    b.HasIndex("AgreementCategoryId")
                        .HasDatabaseName("ix_agreements_agreement_category_id");

                    b.HasIndex("AppId")
                        .HasDatabaseName("ix_agreements_app_id");

                    b.HasIndex("IssuerCompanyId")
                        .HasDatabaseName("ix_agreements_issuer_company_id");

                    b.HasIndex("UseCaseId")
                        .HasDatabaseName("ix_agreements_use_case_id");

                    b.ToTable("agreements", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AgreementAssignedCompanyRole", b =>
                {
                    b.Property<Guid>("AgreementId")
                        .HasColumnType("uuid")
                        .HasColumnName("agreement_id");

                    b.Property<int>("CompanyRoleId")
                        .HasColumnType("integer")
                        .HasColumnName("company_role_id");

                    b.HasKey("AgreementId", "CompanyRoleId")
                        .HasName("pk_agreement_assigned_company_roles");

                    b.HasIndex("CompanyRoleId")
                        .HasDatabaseName("ix_agreement_assigned_company_roles_company_role_id");

                    b.ToTable("agreement_assigned_company_roles", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AgreementAssignedDocumentTemplate", b =>
                {
                    b.Property<Guid>("AgreementId")
                        .HasColumnType("uuid")
                        .HasColumnName("agreement_id");

                    b.Property<Guid>("DocumentTemplateId")
                        .HasColumnType("uuid")
                        .HasColumnName("document_template_id");

                    b.HasKey("AgreementId", "DocumentTemplateId")
                        .HasName("pk_agreement_assigned_document_templates");

                    b.HasIndex("DocumentTemplateId")
                        .IsUnique()
                        .HasDatabaseName("ix_agreement_assigned_document_templates_document_template_id");

                    b.ToTable("agreement_assigned_document_templates", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AgreementCategory", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_agreement_categories");

                    b.ToTable("agreement_categories", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "CX_FRAME_CONTRACT"
                        },
                        new
                        {
                            Id = 2,
                            Label = "APP_CONTRACT"
                        },
                        new
                        {
                            Id = 3,
                            Label = "DATA_CONTRACT"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.App", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<int>("AppStatusId")
                        .HasColumnType("integer")
                        .HasColumnName("app_status_id");

                    b.Property<string>("AppUrl")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("app_url");

                    b.Property<string>("ContactEmail")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("contact_email");

                    b.Property<string>("ContactNumber")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("contact_number");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<DateTimeOffset?>("DateReleased")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_released");

                    b.Property<string>("MarketingUrl")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("marketing_url");

                    b.Property<string>("Name")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("name");

                    b.Property<string>("Provider")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("provider");

                    b.Property<Guid?>("ProviderCompanyId")
                        .HasColumnType("uuid")
                        .HasColumnName("provider_company_id");

                    b.Property<string>("ThumbnailUrl")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("thumbnail_url");

                    b.HasKey("Id")
                        .HasName("pk_apps");

                    b.HasIndex("AppStatusId")
                        .HasDatabaseName("ix_apps_app_status_id");

                    b.HasIndex("ProviderCompanyId")
                        .HasDatabaseName("ix_apps_provider_company_id");

                    b.ToTable("apps", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppAssignedClient", b =>
                {
                    b.Property<Guid>("AppId")
                        .HasColumnType("uuid")
                        .HasColumnName("app_id");

                    b.Property<Guid>("IamClientId")
                        .HasColumnType("uuid")
                        .HasColumnName("iam_client_id");

                    b.HasKey("AppId", "IamClientId")
                        .HasName("pk_app_assigned_clients");

                    b.HasIndex("IamClientId")
                        .HasDatabaseName("ix_app_assigned_clients_iam_client_id");

                    b.ToTable("app_assigned_clients", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppAssignedLicense", b =>
                {
                    b.Property<Guid>("AppId")
                        .HasColumnType("uuid")
                        .HasColumnName("app_id");

                    b.Property<Guid>("AppLicenseId")
                        .HasColumnType("uuid")
                        .HasColumnName("app_license_id");

                    b.HasKey("AppId", "AppLicenseId")
                        .HasName("pk_app_assigned_licenses");

                    b.HasIndex("AppLicenseId")
                        .HasDatabaseName("ix_app_assigned_licenses_app_license_id");

                    b.ToTable("app_assigned_licenses", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppAssignedUseCase", b =>
                {
                    b.Property<Guid>("AppId")
                        .HasColumnType("uuid")
                        .HasColumnName("app_id");

                    b.Property<Guid>("UseCaseId")
                        .HasColumnType("uuid")
                        .HasColumnName("use_case_id");

                    b.HasKey("AppId", "UseCaseId")
                        .HasName("pk_app_assigned_use_cases");

                    b.HasIndex("UseCaseId")
                        .HasDatabaseName("ix_app_assigned_use_cases_use_case_id");

                    b.ToTable("app_assigned_use_cases", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppDescription", b =>
                {
                    b.Property<Guid>("AppId")
                        .HasColumnType("uuid")
                        .HasColumnName("app_id");

                    b.Property<string>("LanguageShortName")
                        .HasMaxLength(2)
                        .HasColumnType("character(2)")
                        .HasColumnName("language_short_name");

                    b.Property<string>("DescriptionLong")
                        .IsRequired()
                        .HasMaxLength(4096)
                        .HasColumnType("character varying(4096)")
                        .HasColumnName("description_long");

                    b.Property<string>("DescriptionShort")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("description_short");

                    b.HasKey("AppId", "LanguageShortName")
                        .HasName("pk_app_descriptions");

                    b.HasIndex("LanguageShortName")
                        .HasDatabaseName("ix_app_descriptions_language_short_name");

                    b.ToTable("app_descriptions", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppDetailImage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("AppId")
                        .HasColumnType("uuid")
                        .HasColumnName("app_id");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("image_url");

                    b.HasKey("Id")
                        .HasName("pk_app_detail_images");

                    b.HasIndex("AppId")
                        .HasDatabaseName("ix_app_detail_images_app_id");

                    b.ToTable("app_detail_images", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppLanguage", b =>
                {
                    b.Property<Guid>("AppId")
                        .HasColumnType("uuid")
                        .HasColumnName("app_id");

                    b.Property<string>("LanguageShortName")
                        .HasMaxLength(2)
                        .HasColumnType("character(2)")
                        .HasColumnName("language_short_name");

                    b.HasKey("AppId", "LanguageShortName")
                        .HasName("pk_app_languages");

                    b.HasIndex("LanguageShortName")
                        .HasDatabaseName("ix_app_languages_language_short_name");

                    b.ToTable("app_languages", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppLicense", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Licensetext")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("licensetext");

                    b.HasKey("Id")
                        .HasName("pk_app_licenses");

                    b.ToTable("app_licenses", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_app_statuses");

                    b.ToTable("app_statuses", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "CREATED"
                        },
                        new
                        {
                            Id = 2,
                            Label = "IN_REVIEW"
                        },
                        new
                        {
                            Id = 3,
                            Label = "ACTIVE"
                        },
                        new
                        {
                            Id = 4,
                            Label = "INACTIVE"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppSubscriptionStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_app_subscription_statuses");

                    b.ToTable("app_subscription_statuses", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "PENDING"
                        },
                        new
                        {
                            Id = 2,
                            Label = "ACTIVE"
                        },
                        new
                        {
                            Id = 3,
                            Label = "INACTIVE"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppTag", b =>
                {
                    b.Property<Guid>("AppId")
                        .HasColumnType("uuid")
                        .HasColumnName("app_id");

                    b.Property<string>("Name")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("tag_name");

                    b.HasKey("AppId", "Name")
                        .HasName("pk_app_tags");

                    b.ToTable("app_tags", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Company", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid?>("AddressId")
                        .HasColumnType("uuid")
                        .HasColumnName("address_id");

                    b.Property<string>("BusinessPartnerNumber")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("business_partner_number");

                    b.Property<int>("CompanyStatusId")
                        .HasColumnType("integer")
                        .HasColumnName("company_status_id");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("name");

                    b.Property<string>("Shortname")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("shortname");

                    b.Property<string>("TaxId")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("tax_id");

                    b.HasKey("Id")
                        .HasName("pk_companies");

                    b.HasIndex("AddressId")
                        .HasDatabaseName("ix_companies_address_id");

                    b.HasIndex("CompanyStatusId")
                        .HasDatabaseName("ix_companies_company_status_id");

                    b.ToTable("companies", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyApplication", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<int>("ApplicationStatusId")
                        .HasColumnType("integer")
                        .HasColumnName("application_status_id");

                    b.Property<Guid>("CompanyId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_id");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<DateTimeOffset?>("DateLastChanged")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_last_changed");

                    b.HasKey("Id")
                        .HasName("pk_company_applications");

                    b.HasIndex("ApplicationStatusId")
                        .HasDatabaseName("ix_company_applications_application_status_id");

                    b.HasIndex("CompanyId")
                        .HasDatabaseName("ix_company_applications_company_id");

                    b.ToTable("company_applications", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyApplicationStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_company_application_statuses");

                    b.ToTable("company_application_statuses", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "CREATED"
                        },
                        new
                        {
                            Id = 2,
                            Label = "ADD_COMPANY_DATA"
                        },
                        new
                        {
                            Id = 3,
                            Label = "INVITE_USER"
                        },
                        new
                        {
                            Id = 4,
                            Label = "SELECT_COMPANY_ROLE"
                        },
                        new
                        {
                            Id = 5,
                            Label = "UPLOAD_DOCUMENTS"
                        },
                        new
                        {
                            Id = 6,
                            Label = "VERIFY"
                        },
                        new
                        {
                            Id = 7,
                            Label = "SUBMITTED"
                        },
                        new
                        {
                            Id = 8,
                            Label = "CONFIRMED"
                        },
                        new
                        {
                            Id = 9,
                            Label = "DECLINED"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyAssignedApp", b =>
                {
                    b.Property<Guid>("CompanyId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_id");

                    b.Property<Guid>("AppId")
                        .HasColumnType("uuid")
                        .HasColumnName("app_id");

                    b.Property<int>("AppSubscriptionStatusId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(1)
                        .HasColumnName("app_subscription_status_id");

                    b.HasKey("CompanyId", "AppId")
                        .HasName("pk_company_assigned_apps");

                    b.HasIndex("AppId")
                        .HasDatabaseName("ix_company_assigned_apps_app_id");

                    b.HasIndex("AppSubscriptionStatusId")
                        .HasDatabaseName("ix_company_assigned_apps_app_subscription_status_id");

                    b.ToTable("company_assigned_apps", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyAssignedRole", b =>
                {
                    b.Property<Guid>("CompanyId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_id");

                    b.Property<int>("CompanyRoleId")
                        .HasColumnType("integer")
                        .HasColumnName("company_role_id");

                    b.HasKey("CompanyId", "CompanyRoleId")
                        .HasName("pk_company_assigned_roles");

                    b.HasIndex("CompanyRoleId")
                        .HasDatabaseName("ix_company_assigned_roles_company_role_id");

                    b.ToTable("company_assigned_roles", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyAssignedUseCase", b =>
                {
                    b.Property<Guid>("CompanyId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_id");

                    b.Property<Guid>("UseCaseId")
                        .HasColumnType("uuid")
                        .HasColumnName("use_case_id");

                    b.HasKey("CompanyId", "UseCaseId")
                        .HasName("pk_company_assigned_use_cases");

                    b.HasIndex("UseCaseId")
                        .HasDatabaseName("ix_company_assigned_use_cases_use_case_id");

                    b.ToTable("company_assigned_use_cases", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyIdentityProvider", b =>
                {
                    b.Property<Guid>("CompanyId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_id");

                    b.Property<Guid>("IdentityProviderId")
                        .HasColumnType("uuid")
                        .HasColumnName("identity_provider_id");

                    b.HasKey("CompanyId", "IdentityProviderId")
                        .HasName("pk_company_identity_providers");

                    b.HasIndex("IdentityProviderId")
                        .HasDatabaseName("ix_company_identity_providers_identity_provider_id");

                    b.ToTable("company_identity_providers", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyRole", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_company_roles");

                    b.ToTable("company_roles", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "ACTIVE_PARTICIPANT"
                        },
                        new
                        {
                            Id = 2,
                            Label = "APP_PROVIDER"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyRoleDescription", b =>
                {
                    b.Property<int>("CompanyRoleId")
                        .HasColumnType("integer")
                        .HasColumnName("company_role_id");

                    b.Property<string>("LanguageShortName")
                        .HasMaxLength(2)
                        .HasColumnType("character(2)")
                        .HasColumnName("language_short_name");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("description");

                    b.HasKey("CompanyRoleId", "LanguageShortName")
                        .HasName("pk_company_role_descriptions");

                    b.HasIndex("LanguageShortName")
                        .HasDatabaseName("ix_company_role_descriptions_language_short_name");

                    b.ToTable("company_role_descriptions", "portal");

                    b.HasData(
                        new
                        {
                            CompanyRoleId = 1,
                            LanguageShortName = "de",
                            Description = "Netzwerkteilnehmer"
                        },
                        new
                        {
                            CompanyRoleId = 1,
                            LanguageShortName = "en",
                            Description = "Participant"
                        },
                        new
                        {
                            CompanyRoleId = 2,
                            LanguageShortName = "de",
                            Description = "Softwareanbieter"
                        },
                        new
                        {
                            CompanyRoleId = 2,
                            LanguageShortName = "en",
                            Description = "Application Provider"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyServiceAccount", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("CompanyId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_id");

                    b.Property<int>("CompanyServiceAccountStatusId")
                        .HasColumnType("integer")
                        .HasColumnName("company_service_account_status_id");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_company_service_accounts");

                    b.HasIndex("CompanyId")
                        .HasDatabaseName("ix_company_service_accounts_company_id");

                    b.HasIndex("CompanyServiceAccountStatusId")
                        .HasDatabaseName("ix_company_service_accounts_company_service_account_status_id");

                    b.ToTable("company_service_accounts", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyServiceAccountAssignedRole", b =>
                {
                    b.Property<Guid>("CompanyServiceAccountId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_service_account_id");

                    b.Property<Guid>("UserRoleId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_role_id");

                    b.HasKey("CompanyServiceAccountId", "UserRoleId")
                        .HasName("pk_company_service_account_assigned_roles");

                    b.HasIndex("UserRoleId")
                        .HasDatabaseName("ix_company_service_account_assigned_roles_user_role_id");

                    b.ToTable("company_service_account_assigned_roles", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyServiceAccountStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_company_service_account_statuses");

                    b.ToTable("company_service_account_statuses", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "ACTIVE"
                        },
                        new
                        {
                            Id = 2,
                            Label = "INACTIVE"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_company_statuses");

                    b.ToTable("company_statuses", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "PENDING"
                        },
                        new
                        {
                            Id = 2,
                            Label = "ACTIVE"
                        },
                        new
                        {
                            Id = 3,
                            Label = "REJECTED"
                        },
                        new
                        {
                            Id = 4,
                            Label = "INACTIVE"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("CompanyId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_id");

                    b.Property<int>("CompanyUserStatusId")
                        .HasColumnType("integer")
                        .HasColumnName("company_user_status_id");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<DateTimeOffset?>("DateLastChanged")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_last_changed");

                    b.Property<string>("Email")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("email");

                    b.Property<string>("Firstname")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("firstname");

                    b.Property<byte[]>("Lastlogin")
                        .HasColumnType("bytea")
                        .HasColumnName("lastlogin");

                    b.Property<string>("Lastname")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("lastname");

                    b.HasKey("Id")
                        .HasName("pk_company_users");

                    b.HasIndex("CompanyId")
                        .HasDatabaseName("ix_company_users_company_id");

                    b.HasIndex("CompanyUserStatusId")
                        .HasDatabaseName("ix_company_users_company_user_status_id");

                    b.ToTable("company_users", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUserAssignedAppFavourite", b =>
                {
                    b.Property<Guid>("CompanyUserId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_user_id");

                    b.Property<Guid>("AppId")
                        .HasColumnType("uuid")
                        .HasColumnName("app_id");

                    b.HasKey("CompanyUserId", "AppId")
                        .HasName("pk_company_user_assigned_app_favourites");

                    b.HasIndex("AppId")
                        .HasDatabaseName("ix_company_user_assigned_app_favourites_app_id");

                    b.ToTable("company_user_assigned_app_favourites", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUserAssignedBusinessPartner", b =>
                {
                    b.Property<Guid>("CompanyUserId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_user_id");

                    b.Property<string>("BusinessPartnerNumber")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("business_partner_number");

                    b.HasKey("CompanyUserId", "BusinessPartnerNumber")
                        .HasName("pk_company_user_assigned_business_partners");

                    b.ToTable("company_user_assigned_business_partners", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUserAssignedRole", b =>
                {
                    b.Property<Guid>("CompanyUserId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_user_id");

                    b.Property<Guid>("UserRoleId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_role_id");

                    b.HasKey("CompanyUserId", "UserRoleId")
                        .HasName("pk_company_user_assigned_roles");

                    b.HasIndex("UserRoleId")
                        .HasDatabaseName("ix_company_user_assigned_roles_user_role_id");

                    b.ToTable("company_user_assigned_roles", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUserStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_company_user_statuses");

                    b.ToTable("company_user_statuses", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "ACTIVE"
                        },
                        new
                        {
                            Id = 2,
                            Label = "INACTIVE"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Connector", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("ConnectorUrl")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("connector_url");

                    b.Property<Guid?>("HostId")
                        .HasColumnType("uuid")
                        .HasColumnName("host_id");

                    b.Property<string>("LocationId")
                        .IsRequired()
                        .HasMaxLength(2)
                        .HasColumnType("character(2)")
                        .HasColumnName("location_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("name");

                    b.Property<Guid>("ProviderId")
                        .HasColumnType("uuid")
                        .HasColumnName("provider_id");

                    b.Property<int>("StatusId")
                        .HasColumnType("integer")
                        .HasColumnName("status_id");

                    b.Property<int>("TypeId")
                        .HasColumnType("integer")
                        .HasColumnName("type_id");

                    b.HasKey("Id")
                        .HasName("pk_connectors");

                    b.HasIndex("HostId")
                        .HasDatabaseName("ix_connectors_host_id");

                    b.HasIndex("LocationId")
                        .HasDatabaseName("ix_connectors_location_id");

                    b.HasIndex("ProviderId")
                        .HasDatabaseName("ix_connectors_provider_id");

                    b.HasIndex("StatusId")
                        .HasDatabaseName("ix_connectors_status_id");

                    b.HasIndex("TypeId")
                        .HasDatabaseName("ix_connectors_type_id");

                    b.ToTable("connectors", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.ConnectorStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_connector_statuses");

                    b.ToTable("connector_statuses", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "PENDING"
                        },
                        new
                        {
                            Id = 2,
                            Label = "ACTIVE"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.ConnectorType", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_connector_types");

                    b.ToTable("connector_types", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "COMPANY_CONNECTOR"
                        },
                        new
                        {
                            Id = 2,
                            Label = "CONNECTOR_AS_A_SERVICE"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Consent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("AgreementId")
                        .HasColumnType("uuid")
                        .HasColumnName("agreement_id");

                    b.Property<string>("Comment")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("comment");

                    b.Property<Guid>("CompanyId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_id");

                    b.Property<Guid>("CompanyUserId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_user_id");

                    b.Property<int>("ConsentStatusId")
                        .HasColumnType("integer")
                        .HasColumnName("consent_status_id");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<Guid?>("DocumentId")
                        .HasColumnType("uuid")
                        .HasColumnName("document_id");

                    b.Property<string>("Target")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("target");

                    b.HasKey("Id")
                        .HasName("pk_consents");

                    b.HasIndex("AgreementId")
                        .HasDatabaseName("ix_consents_agreement_id");

                    b.HasIndex("CompanyId")
                        .HasDatabaseName("ix_consents_company_id");

                    b.HasIndex("CompanyUserId")
                        .HasDatabaseName("ix_consents_company_user_id");

                    b.HasIndex("ConsentStatusId")
                        .HasDatabaseName("ix_consents_consent_status_id");

                    b.HasIndex("DocumentId")
                        .HasDatabaseName("ix_consents_document_id");

                    b.ToTable("consents", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.ConsentStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_consent_statuses");

                    b.ToTable("consent_statuses", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "ACTIVE"
                        },
                        new
                        {
                            Id = 2,
                            Label = "INACTIVE"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Country", b =>
                {
                    b.Property<string>("Alpha2Code")
                        .HasMaxLength(2)
                        .HasColumnType("character(2)")
                        .HasColumnName("alpha2code")
                        .IsFixedLength();

                    b.Property<string>("Alpha3Code")
                        .HasMaxLength(3)
                        .HasColumnType("character(3)")
                        .HasColumnName("alpha3code")
                        .IsFixedLength();

                    b.Property<string>("CountryNameDe")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("country_name_de");

                    b.Property<string>("CountryNameEn")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("country_name_en");

                    b.HasKey("Alpha2Code")
                        .HasName("pk_countries");

                    b.ToTable("countries", "portal");

                    b.HasData(
                        new
                        {
                            Alpha2Code = "DE",
                            Alpha3Code = "DEU",
                            CountryNameDe = "Deutschland",
                            CountryNameEn = "Germany"
                        },
                        new
                        {
                            Alpha2Code = "GB",
                            Alpha3Code = "GBR",
                            CountryNameDe = "United Kingdom of Great Britain and Northern Ireland (the)",
                            CountryNameEn = "United Kingdom of Great Britain and Northern Ireland (the)"
                        },
                        new
                        {
                            Alpha2Code = "AF",
                            Alpha3Code = "AFG",
                            CountryNameDe = "Afghanistan",
                            CountryNameEn = "Afghanistan"
                        },
                        new
                        {
                            Alpha2Code = "AL",
                            Alpha3Code = "ALB",
                            CountryNameDe = "Albania",
                            CountryNameEn = "Albania"
                        },
                        new
                        {
                            Alpha2Code = "DZ",
                            Alpha3Code = "DZA",
                            CountryNameDe = "Algeria",
                            CountryNameEn = "Algeria"
                        },
                        new
                        {
                            Alpha2Code = "AS",
                            Alpha3Code = "ASM",
                            CountryNameDe = "American Samoa",
                            CountryNameEn = "American Samoa"
                        },
                        new
                        {
                            Alpha2Code = "AD",
                            Alpha3Code = "AND",
                            CountryNameDe = "Andorra",
                            CountryNameEn = "Andorra"
                        },
                        new
                        {
                            Alpha2Code = "AO",
                            Alpha3Code = "AGO",
                            CountryNameDe = "Angola",
                            CountryNameEn = "Angola"
                        },
                        new
                        {
                            Alpha2Code = "AI",
                            Alpha3Code = "AIA",
                            CountryNameDe = "Anguilla",
                            CountryNameEn = "Anguilla"
                        },
                        new
                        {
                            Alpha2Code = "AQ",
                            Alpha3Code = "ATA",
                            CountryNameDe = "Antarctica",
                            CountryNameEn = "Antarctica"
                        },
                        new
                        {
                            Alpha2Code = "AG",
                            Alpha3Code = "ATG",
                            CountryNameDe = "Antigua and Barbuda",
                            CountryNameEn = "Antigua and Barbuda"
                        },
                        new
                        {
                            Alpha2Code = "AR",
                            Alpha3Code = "ARG",
                            CountryNameDe = "Argentina",
                            CountryNameEn = "Argentina"
                        },
                        new
                        {
                            Alpha2Code = "AM",
                            Alpha3Code = "ARM",
                            CountryNameDe = "Armenia",
                            CountryNameEn = "Armenia"
                        },
                        new
                        {
                            Alpha2Code = "AW",
                            Alpha3Code = "ABW",
                            CountryNameDe = "Aruba",
                            CountryNameEn = "Aruba"
                        },
                        new
                        {
                            Alpha2Code = "AU",
                            Alpha3Code = "AUS",
                            CountryNameDe = "Australia",
                            CountryNameEn = "Australia"
                        },
                        new
                        {
                            Alpha2Code = "AT",
                            Alpha3Code = "AUT",
                            CountryNameDe = "Austria",
                            CountryNameEn = "Austria"
                        },
                        new
                        {
                            Alpha2Code = "AZ",
                            Alpha3Code = "AZE",
                            CountryNameDe = "Azerbaijan",
                            CountryNameEn = "Azerbaijan"
                        },
                        new
                        {
                            Alpha2Code = "BS",
                            Alpha3Code = "BHS",
                            CountryNameDe = "Bahamas",
                            CountryNameEn = "Bahamas (the)"
                        },
                        new
                        {
                            Alpha2Code = "BH",
                            Alpha3Code = "BHR",
                            CountryNameDe = "Bahrain",
                            CountryNameEn = "Bahrain"
                        },
                        new
                        {
                            Alpha2Code = "BD",
                            Alpha3Code = "BGD",
                            CountryNameDe = "Bangladesh",
                            CountryNameEn = "Bangladesh"
                        },
                        new
                        {
                            Alpha2Code = "BB",
                            Alpha3Code = "BRB",
                            CountryNameDe = "Barbados",
                            CountryNameEn = "Barbados"
                        },
                        new
                        {
                            Alpha2Code = "BY",
                            Alpha3Code = "BLR",
                            CountryNameDe = "Belarus",
                            CountryNameEn = "Belarus"
                        },
                        new
                        {
                            Alpha2Code = "BE",
                            Alpha3Code = "BEL",
                            CountryNameDe = "Belgium",
                            CountryNameEn = "Belgium"
                        },
                        new
                        {
                            Alpha2Code = "BZ",
                            Alpha3Code = "BLZ",
                            CountryNameDe = "Belize",
                            CountryNameEn = "Belize"
                        },
                        new
                        {
                            Alpha2Code = "BJ",
                            Alpha3Code = "BEN",
                            CountryNameDe = "Benin",
                            CountryNameEn = "Benin"
                        },
                        new
                        {
                            Alpha2Code = "BM",
                            Alpha3Code = "BMU",
                            CountryNameDe = "Bermuda",
                            CountryNameEn = "Bermuda"
                        },
                        new
                        {
                            Alpha2Code = "AX",
                            Alpha3Code = "ALA",
                            CountryNameDe = "Åland Islands",
                            CountryNameEn = "Åland Islands"
                        },
                        new
                        {
                            Alpha2Code = "BT",
                            Alpha3Code = "BTN",
                            CountryNameDe = "Bhutan",
                            CountryNameEn = "Bhutan"
                        },
                        new
                        {
                            Alpha2Code = "BO",
                            Alpha3Code = "BOL",
                            CountryNameDe = "Bolivien",
                            CountryNameEn = "Bolivia (Plurinational State of)"
                        },
                        new
                        {
                            Alpha2Code = "BQ",
                            Alpha3Code = "BES",
                            CountryNameDe = "Bonaire, Sint Eustatius and Saba",
                            CountryNameEn = "Bonaire, Sint Eustatius and Saba"
                        },
                        new
                        {
                            Alpha2Code = "BA",
                            Alpha3Code = "BIH",
                            CountryNameDe = "Bosnien and Herzegovenien",
                            CountryNameEn = "Bosnia and Herzegovina"
                        },
                        new
                        {
                            Alpha2Code = "BW",
                            Alpha3Code = "BWA",
                            CountryNameDe = "Botswana",
                            CountryNameEn = "Botswana"
                        },
                        new
                        {
                            Alpha2Code = "BV",
                            Alpha3Code = "BVT",
                            CountryNameDe = "Bouvet Island",
                            CountryNameEn = "Bouvet Island"
                        },
                        new
                        {
                            Alpha2Code = "BR",
                            Alpha3Code = "BRA",
                            CountryNameDe = "Brasilien",
                            CountryNameEn = "Brazil"
                        },
                        new
                        {
                            Alpha2Code = "IO",
                            Alpha3Code = "IOT",
                            CountryNameDe = "British Indian Ocean Territory",
                            CountryNameEn = "British Indian Ocean Territory (the)"
                        },
                        new
                        {
                            Alpha2Code = "BN",
                            Alpha3Code = "BRN",
                            CountryNameDe = "Brunei Darussalam",
                            CountryNameEn = "Brunei Darussalam"
                        },
                        new
                        {
                            Alpha2Code = "BG",
                            Alpha3Code = "BGR",
                            CountryNameDe = "Bulgarien",
                            CountryNameEn = "Bulgaria"
                        },
                        new
                        {
                            Alpha2Code = "BF",
                            Alpha3Code = "BFA",
                            CountryNameDe = "Burkina Faso",
                            CountryNameEn = "Burkina Faso"
                        },
                        new
                        {
                            Alpha2Code = "BI",
                            Alpha3Code = "BDI",
                            CountryNameDe = "Burundi",
                            CountryNameEn = "Burundi"
                        },
                        new
                        {
                            Alpha2Code = "CV",
                            Alpha3Code = "CPV",
                            CountryNameDe = "Cabo Verde",
                            CountryNameEn = "Cabo Verde"
                        },
                        new
                        {
                            Alpha2Code = "KH",
                            Alpha3Code = "KHM",
                            CountryNameDe = "Cambodia",
                            CountryNameEn = "Cambodia"
                        },
                        new
                        {
                            Alpha2Code = "CM",
                            Alpha3Code = "CMR",
                            CountryNameDe = "Cameroon",
                            CountryNameEn = "Cameroon"
                        },
                        new
                        {
                            Alpha2Code = "CA",
                            Alpha3Code = "CAN",
                            CountryNameDe = "Kanada",
                            CountryNameEn = "Canada"
                        },
                        new
                        {
                            Alpha2Code = "KY",
                            Alpha3Code = "CYM",
                            CountryNameDe = "Cayman Islands (the)",
                            CountryNameEn = "Cayman Islands (the)"
                        },
                        new
                        {
                            Alpha2Code = "CF",
                            Alpha3Code = "CAF",
                            CountryNameDe = "Central African Republic (the)",
                            CountryNameEn = "Central African Republic (the)"
                        },
                        new
                        {
                            Alpha2Code = "TD",
                            Alpha3Code = "TCD",
                            CountryNameDe = "Chad",
                            CountryNameEn = "Chad"
                        },
                        new
                        {
                            Alpha2Code = "CL",
                            Alpha3Code = "CHL",
                            CountryNameDe = "Chile",
                            CountryNameEn = "Chile"
                        },
                        new
                        {
                            Alpha2Code = "CN",
                            Alpha3Code = "CHN",
                            CountryNameDe = "China",
                            CountryNameEn = "China"
                        },
                        new
                        {
                            Alpha2Code = "CX",
                            Alpha3Code = "CXR",
                            CountryNameDe = "Weihnachtsinseln",
                            CountryNameEn = "Christmas Island"
                        },
                        new
                        {
                            Alpha2Code = "CC",
                            Alpha3Code = "CCK",
                            CountryNameDe = "Cocos (Keeling) Islands (the)",
                            CountryNameEn = "Cocos (Keeling) Islands (the)"
                        },
                        new
                        {
                            Alpha2Code = "CO",
                            Alpha3Code = "COL",
                            CountryNameDe = "Kolumbien",
                            CountryNameEn = "Colombia"
                        },
                        new
                        {
                            Alpha2Code = "KM",
                            Alpha3Code = "COM",
                            CountryNameDe = "Comoros",
                            CountryNameEn = "Comoros (the)"
                        },
                        new
                        {
                            Alpha2Code = "CD",
                            Alpha3Code = "COD",
                            CountryNameDe = "Kongo",
                            CountryNameEn = "Congo (the Democratic Republic of the)"
                        },
                        new
                        {
                            Alpha2Code = "CK",
                            Alpha3Code = "COK",
                            CountryNameDe = "Cook Islands",
                            CountryNameEn = "Cook Islands (the)"
                        },
                        new
                        {
                            Alpha2Code = "CR",
                            Alpha3Code = "CRI",
                            CountryNameDe = "Costa Rica",
                            CountryNameEn = "Costa Rica"
                        },
                        new
                        {
                            Alpha2Code = "HR",
                            Alpha3Code = "HRV",
                            CountryNameDe = "Kroatien",
                            CountryNameEn = "Croatia"
                        },
                        new
                        {
                            Alpha2Code = "CU",
                            Alpha3Code = "CUB",
                            CountryNameDe = "Kuba",
                            CountryNameEn = "Cuba"
                        },
                        new
                        {
                            Alpha2Code = "CW",
                            Alpha3Code = "CUW",
                            CountryNameDe = "Curaçao",
                            CountryNameEn = "Curaçao"
                        },
                        new
                        {
                            Alpha2Code = "CY",
                            Alpha3Code = "CYP",
                            CountryNameDe = "Zypern",
                            CountryNameEn = "Cyprus"
                        },
                        new
                        {
                            Alpha2Code = "CZ",
                            Alpha3Code = "CZE",
                            CountryNameDe = "Tschechien",
                            CountryNameEn = "Czechia"
                        },
                        new
                        {
                            Alpha2Code = "CI",
                            Alpha3Code = "CIV",
                            CountryNameDe = "Côte d'Ivoire",
                            CountryNameEn = "Côte d'Ivoire"
                        },
                        new
                        {
                            Alpha2Code = "DK",
                            Alpha3Code = "DNK",
                            CountryNameDe = "Dänemark",
                            CountryNameEn = "Denmark"
                        },
                        new
                        {
                            Alpha2Code = "DJ",
                            Alpha3Code = "DJI",
                            CountryNameDe = "Djibouti",
                            CountryNameEn = "Djibouti"
                        },
                        new
                        {
                            Alpha2Code = "DM",
                            Alpha3Code = "DMA",
                            CountryNameDe = "Dominica",
                            CountryNameEn = "Dominica"
                        },
                        new
                        {
                            Alpha2Code = "DO",
                            Alpha3Code = "DOM",
                            CountryNameDe = "Dominikanische Republik",
                            CountryNameEn = "Dominican Republic (the)"
                        },
                        new
                        {
                            Alpha2Code = "EC",
                            Alpha3Code = "ECU",
                            CountryNameDe = "Ecuador",
                            CountryNameEn = "Ecuador"
                        },
                        new
                        {
                            Alpha2Code = "EG",
                            Alpha3Code = "EGY",
                            CountryNameDe = "Ägypten",
                            CountryNameEn = "Egypt"
                        },
                        new
                        {
                            Alpha2Code = "SV",
                            Alpha3Code = "SLV",
                            CountryNameDe = "El Salvador",
                            CountryNameEn = "El Salvador"
                        },
                        new
                        {
                            Alpha2Code = "GQ",
                            Alpha3Code = "GNQ",
                            CountryNameDe = "Equatorial Guinea",
                            CountryNameEn = "Equatorial Guinea"
                        },
                        new
                        {
                            Alpha2Code = "ER",
                            Alpha3Code = "ERI",
                            CountryNameDe = "Eritrea",
                            CountryNameEn = "Eritrea"
                        },
                        new
                        {
                            Alpha2Code = "EE",
                            Alpha3Code = "EST",
                            CountryNameDe = "Estonia",
                            CountryNameEn = "Estonia"
                        },
                        new
                        {
                            Alpha2Code = "SZ",
                            Alpha3Code = "SWZ",
                            CountryNameDe = "Eswatini",
                            CountryNameEn = "Eswatini"
                        },
                        new
                        {
                            Alpha2Code = "ET",
                            Alpha3Code = "ETH",
                            CountryNameDe = "Ethiopia",
                            CountryNameEn = "Ethiopia"
                        },
                        new
                        {
                            Alpha2Code = "FK",
                            Alpha3Code = "FLK",
                            CountryNameDe = "Falkland Islands (the) [Malvinas]",
                            CountryNameEn = "Falkland Islands (the) [Malvinas]"
                        },
                        new
                        {
                            Alpha2Code = "FO",
                            Alpha3Code = "FRO",
                            CountryNameDe = "Faroe Islands (the)",
                            CountryNameEn = "Faroe Islands (the)"
                        },
                        new
                        {
                            Alpha2Code = "FJ",
                            Alpha3Code = "FJI",
                            CountryNameDe = "Fiji",
                            CountryNameEn = "Fiji"
                        },
                        new
                        {
                            Alpha2Code = "FI",
                            Alpha3Code = "FIN",
                            CountryNameDe = "Finland",
                            CountryNameEn = "Finland"
                        },
                        new
                        {
                            Alpha2Code = "FR",
                            Alpha3Code = "FRA",
                            CountryNameDe = "Frankreich",
                            CountryNameEn = "France"
                        },
                        new
                        {
                            Alpha2Code = "GF",
                            Alpha3Code = "GUF",
                            CountryNameDe = "French Guiana",
                            CountryNameEn = "French Guiana"
                        },
                        new
                        {
                            Alpha2Code = "PF",
                            Alpha3Code = "PYF",
                            CountryNameDe = "French Polynesia",
                            CountryNameEn = "French Polynesia"
                        },
                        new
                        {
                            Alpha2Code = "TF",
                            Alpha3Code = "ATF",
                            CountryNameDe = "French Southern Territories (the)",
                            CountryNameEn = "French Southern Territories (the)"
                        },
                        new
                        {
                            Alpha2Code = "GA",
                            Alpha3Code = "GAB",
                            CountryNameDe = "Gabon",
                            CountryNameEn = "Gabon"
                        },
                        new
                        {
                            Alpha2Code = "GM",
                            Alpha3Code = "GMB",
                            CountryNameDe = "Gambia (the)",
                            CountryNameEn = "Gambia (the)"
                        },
                        new
                        {
                            Alpha2Code = "GE",
                            Alpha3Code = "GEO",
                            CountryNameDe = "Georgia",
                            CountryNameEn = "Georgia"
                        },
                        new
                        {
                            Alpha2Code = "GH",
                            Alpha3Code = "GHA",
                            CountryNameDe = "Ghana",
                            CountryNameEn = "Ghana"
                        },
                        new
                        {
                            Alpha2Code = "GI",
                            Alpha3Code = "GIB",
                            CountryNameDe = "Gibraltar",
                            CountryNameEn = "Gibraltar"
                        },
                        new
                        {
                            Alpha2Code = "GR",
                            Alpha3Code = "GRC",
                            CountryNameDe = "Greece",
                            CountryNameEn = "Greece"
                        },
                        new
                        {
                            Alpha2Code = "GL",
                            Alpha3Code = "GRL",
                            CountryNameDe = "Greenland",
                            CountryNameEn = "Greenland"
                        },
                        new
                        {
                            Alpha2Code = "GD",
                            Alpha3Code = "GRD",
                            CountryNameDe = "Grenada",
                            CountryNameEn = "Grenada"
                        },
                        new
                        {
                            Alpha2Code = "GP",
                            Alpha3Code = "GLP",
                            CountryNameDe = "Guadeloupe",
                            CountryNameEn = "Guadeloupe"
                        },
                        new
                        {
                            Alpha2Code = "GU",
                            Alpha3Code = "GUM",
                            CountryNameDe = "Guam",
                            CountryNameEn = "Guam"
                        },
                        new
                        {
                            Alpha2Code = "GT",
                            Alpha3Code = "GTM",
                            CountryNameDe = "Guatemala",
                            CountryNameEn = "Guatemala"
                        },
                        new
                        {
                            Alpha2Code = "GG",
                            Alpha3Code = "GGY",
                            CountryNameDe = "Guernsey",
                            CountryNameEn = "Guernsey"
                        },
                        new
                        {
                            Alpha2Code = "GN",
                            Alpha3Code = "GIN",
                            CountryNameDe = "Guinea",
                            CountryNameEn = "Guinea"
                        },
                        new
                        {
                            Alpha2Code = "GW",
                            Alpha3Code = "GNB",
                            CountryNameDe = "Guinea-Bissau",
                            CountryNameEn = "Guinea-Bissau"
                        },
                        new
                        {
                            Alpha2Code = "GY",
                            Alpha3Code = "GUY",
                            CountryNameDe = "Guyana",
                            CountryNameEn = "Guyana"
                        },
                        new
                        {
                            Alpha2Code = "HT",
                            Alpha3Code = "HTI",
                            CountryNameDe = "Haiti",
                            CountryNameEn = "Haiti"
                        },
                        new
                        {
                            Alpha2Code = "HM",
                            Alpha3Code = "HMD",
                            CountryNameDe = "Heard Island and McDonald Islands",
                            CountryNameEn = "Heard Island and McDonald Islands"
                        },
                        new
                        {
                            Alpha2Code = "VA",
                            Alpha3Code = "VAT",
                            CountryNameDe = "Holy See (the)",
                            CountryNameEn = "Holy See (the)"
                        },
                        new
                        {
                            Alpha2Code = "HN",
                            Alpha3Code = "HND",
                            CountryNameDe = "Honduras",
                            CountryNameEn = "Honduras"
                        },
                        new
                        {
                            Alpha2Code = "HK",
                            Alpha3Code = "HKG",
                            CountryNameDe = "Hong Kong",
                            CountryNameEn = "Hong Kong"
                        },
                        new
                        {
                            Alpha2Code = "HU",
                            Alpha3Code = "HUN",
                            CountryNameDe = "Hungary",
                            CountryNameEn = "Hungary"
                        },
                        new
                        {
                            Alpha2Code = "IS",
                            Alpha3Code = "ISL",
                            CountryNameDe = "Iceland",
                            CountryNameEn = "Iceland"
                        },
                        new
                        {
                            Alpha2Code = "IN",
                            Alpha3Code = "IND",
                            CountryNameDe = "India",
                            CountryNameEn = "India"
                        },
                        new
                        {
                            Alpha2Code = "ID",
                            Alpha3Code = "IDN",
                            CountryNameDe = "Indonesia",
                            CountryNameEn = "Indonesia"
                        },
                        new
                        {
                            Alpha2Code = "IR",
                            Alpha3Code = "IRN",
                            CountryNameDe = "Iran (Islamic Republic of)",
                            CountryNameEn = "Iran (Islamic Republic of)"
                        },
                        new
                        {
                            Alpha2Code = "IQ",
                            Alpha3Code = "IRQ",
                            CountryNameDe = "Iraq",
                            CountryNameEn = "Iraq"
                        },
                        new
                        {
                            Alpha2Code = "IE",
                            Alpha3Code = "IRL",
                            CountryNameDe = "Ireland",
                            CountryNameEn = "Ireland"
                        },
                        new
                        {
                            Alpha2Code = "IM",
                            Alpha3Code = "IMN",
                            CountryNameDe = "Isle of Man",
                            CountryNameEn = "Isle of Man"
                        },
                        new
                        {
                            Alpha2Code = "IL",
                            Alpha3Code = "ISR",
                            CountryNameDe = "Israel",
                            CountryNameEn = "Israel"
                        },
                        new
                        {
                            Alpha2Code = "IT",
                            Alpha3Code = "ITA",
                            CountryNameDe = "Italy",
                            CountryNameEn = "Italy"
                        },
                        new
                        {
                            Alpha2Code = "JM",
                            Alpha3Code = "JAM",
                            CountryNameDe = "Jamaica",
                            CountryNameEn = "Jamaica"
                        },
                        new
                        {
                            Alpha2Code = "JP",
                            Alpha3Code = "JPN",
                            CountryNameDe = "Japan",
                            CountryNameEn = "Japan"
                        },
                        new
                        {
                            Alpha2Code = "JE",
                            Alpha3Code = "JEY",
                            CountryNameDe = "Jersey",
                            CountryNameEn = "Jersey"
                        },
                        new
                        {
                            Alpha2Code = "JO",
                            Alpha3Code = "JOR",
                            CountryNameDe = "Jordan",
                            CountryNameEn = "Jordan"
                        },
                        new
                        {
                            Alpha2Code = "KZ",
                            Alpha3Code = "KAZ",
                            CountryNameDe = "Kazakhstan",
                            CountryNameEn = "Kazakhstan"
                        },
                        new
                        {
                            Alpha2Code = "KE",
                            Alpha3Code = "KEN",
                            CountryNameDe = "Kenya",
                            CountryNameEn = "Kenya"
                        },
                        new
                        {
                            Alpha2Code = "KI",
                            Alpha3Code = "KIR",
                            CountryNameDe = "Kiribati",
                            CountryNameEn = "Kiribati"
                        },
                        new
                        {
                            Alpha2Code = "KP",
                            Alpha3Code = "PRK",
                            CountryNameDe = "Korea (the Democratic People's Republic of)",
                            CountryNameEn = "Korea (the Democratic People's Republic of)"
                        },
                        new
                        {
                            Alpha2Code = "KR",
                            Alpha3Code = "KOR",
                            CountryNameDe = "Korea (the Republic of)",
                            CountryNameEn = "Korea (the Republic of)"
                        },
                        new
                        {
                            Alpha2Code = "KW",
                            Alpha3Code = "KWT",
                            CountryNameDe = "Kuwait",
                            CountryNameEn = "Kuwait"
                        },
                        new
                        {
                            Alpha2Code = "KG",
                            Alpha3Code = "KGZ",
                            CountryNameDe = "Kyrgyzstan",
                            CountryNameEn = "Kyrgyzstan"
                        },
                        new
                        {
                            Alpha2Code = "LA",
                            Alpha3Code = "LAO",
                            CountryNameDe = "Lao People's Democratic Republic (the)",
                            CountryNameEn = "Lao People's Democratic Republic (the)"
                        },
                        new
                        {
                            Alpha2Code = "LV",
                            Alpha3Code = "LVA",
                            CountryNameDe = "Latvia",
                            CountryNameEn = "Latvia"
                        },
                        new
                        {
                            Alpha2Code = "LB",
                            Alpha3Code = "LBN",
                            CountryNameDe = "Lebanon",
                            CountryNameEn = "Lebanon"
                        },
                        new
                        {
                            Alpha2Code = "LS",
                            Alpha3Code = "LSO",
                            CountryNameDe = "Lesotho",
                            CountryNameEn = "Lesotho"
                        },
                        new
                        {
                            Alpha2Code = "LR",
                            Alpha3Code = "LBR",
                            CountryNameDe = "Liberia",
                            CountryNameEn = "Liberia"
                        },
                        new
                        {
                            Alpha2Code = "LY",
                            Alpha3Code = "LBY",
                            CountryNameDe = "Libya",
                            CountryNameEn = "Libya"
                        },
                        new
                        {
                            Alpha2Code = "LI",
                            Alpha3Code = "LIE",
                            CountryNameDe = "Liechtenstein",
                            CountryNameEn = "Liechtenstein"
                        },
                        new
                        {
                            Alpha2Code = "LT",
                            Alpha3Code = "LTU",
                            CountryNameDe = "Lithuania",
                            CountryNameEn = "Lithuania"
                        },
                        new
                        {
                            Alpha2Code = "LU",
                            Alpha3Code = "LUX",
                            CountryNameDe = "Luxembourg",
                            CountryNameEn = "Luxembourg"
                        },
                        new
                        {
                            Alpha2Code = "MO",
                            Alpha3Code = "MAC",
                            CountryNameDe = "Macao",
                            CountryNameEn = "Macao"
                        },
                        new
                        {
                            Alpha2Code = "MG",
                            Alpha3Code = "MDG",
                            CountryNameDe = "Madagascar",
                            CountryNameEn = "Madagascar"
                        },
                        new
                        {
                            Alpha2Code = "MW",
                            Alpha3Code = "MWI",
                            CountryNameDe = "Malawi",
                            CountryNameEn = "Malawi"
                        },
                        new
                        {
                            Alpha2Code = "MY",
                            Alpha3Code = "MYS",
                            CountryNameDe = "Malaysia",
                            CountryNameEn = "Malaysia"
                        },
                        new
                        {
                            Alpha2Code = "MV",
                            Alpha3Code = "MDV",
                            CountryNameDe = "Maldives",
                            CountryNameEn = "Maldives"
                        },
                        new
                        {
                            Alpha2Code = "ML",
                            Alpha3Code = "MLI",
                            CountryNameDe = "Mali",
                            CountryNameEn = "Mali"
                        },
                        new
                        {
                            Alpha2Code = "MT",
                            Alpha3Code = "MLT",
                            CountryNameDe = "Malta",
                            CountryNameEn = "Malta"
                        },
                        new
                        {
                            Alpha2Code = "MH",
                            Alpha3Code = "MHL",
                            CountryNameDe = "Marshall Islands (the)",
                            CountryNameEn = "Marshall Islands (the)"
                        },
                        new
                        {
                            Alpha2Code = "MQ",
                            Alpha3Code = "MTQ",
                            CountryNameDe = "Martinique",
                            CountryNameEn = "Martinique"
                        },
                        new
                        {
                            Alpha2Code = "MR",
                            Alpha3Code = "MRT",
                            CountryNameDe = "Mauritania",
                            CountryNameEn = "Mauritania"
                        },
                        new
                        {
                            Alpha2Code = "MU",
                            Alpha3Code = "MUS",
                            CountryNameDe = "Mauritius",
                            CountryNameEn = "Mauritius"
                        },
                        new
                        {
                            Alpha2Code = "YT",
                            Alpha3Code = "MYT",
                            CountryNameDe = "Mayotte",
                            CountryNameEn = "Mayotte"
                        },
                        new
                        {
                            Alpha2Code = "MX",
                            Alpha3Code = "MEX",
                            CountryNameDe = "Mexico",
                            CountryNameEn = "Mexico"
                        },
                        new
                        {
                            Alpha2Code = "FM",
                            Alpha3Code = "FSM",
                            CountryNameDe = "Micronesia (Federated States of)",
                            CountryNameEn = "Micronesia (Federated States of)"
                        },
                        new
                        {
                            Alpha2Code = "MD",
                            Alpha3Code = "MDA",
                            CountryNameDe = "Moldova (the Republic of)",
                            CountryNameEn = "Moldova (the Republic of)"
                        },
                        new
                        {
                            Alpha2Code = "MC",
                            Alpha3Code = "MCO",
                            CountryNameDe = "Monaco",
                            CountryNameEn = "Monaco"
                        },
                        new
                        {
                            Alpha2Code = "MN",
                            Alpha3Code = "MNG",
                            CountryNameDe = "Mongolia",
                            CountryNameEn = "Mongolia"
                        },
                        new
                        {
                            Alpha2Code = "ME",
                            Alpha3Code = "MNE",
                            CountryNameDe = "Montenegro",
                            CountryNameEn = "Montenegro"
                        },
                        new
                        {
                            Alpha2Code = "MS",
                            Alpha3Code = "MSR",
                            CountryNameDe = "Montserrat",
                            CountryNameEn = "Montserrat"
                        },
                        new
                        {
                            Alpha2Code = "MA",
                            Alpha3Code = "MAR",
                            CountryNameDe = "Morocco",
                            CountryNameEn = "Morocco"
                        },
                        new
                        {
                            Alpha2Code = "MZ",
                            Alpha3Code = "MOZ",
                            CountryNameDe = "Mozambique",
                            CountryNameEn = "Mozambique"
                        },
                        new
                        {
                            Alpha2Code = "MM",
                            Alpha3Code = "MMR",
                            CountryNameDe = "Myanmar",
                            CountryNameEn = "Myanmar"
                        },
                        new
                        {
                            Alpha2Code = "NA",
                            Alpha3Code = "NAM",
                            CountryNameDe = "Namibia",
                            CountryNameEn = "Namibia"
                        },
                        new
                        {
                            Alpha2Code = "NR",
                            Alpha3Code = "NRU",
                            CountryNameDe = "Nauru",
                            CountryNameEn = "Nauru"
                        },
                        new
                        {
                            Alpha2Code = "NP",
                            Alpha3Code = "NPL",
                            CountryNameDe = "Nepal",
                            CountryNameEn = "Nepal"
                        },
                        new
                        {
                            Alpha2Code = "NL",
                            Alpha3Code = "NLD",
                            CountryNameDe = "Netherlands (the)",
                            CountryNameEn = "Netherlands (the)"
                        },
                        new
                        {
                            Alpha2Code = "NC",
                            Alpha3Code = "NCL",
                            CountryNameDe = "New Caledonia",
                            CountryNameEn = "New Caledonia"
                        },
                        new
                        {
                            Alpha2Code = "NZ",
                            Alpha3Code = "NZL",
                            CountryNameDe = "New Zealand",
                            CountryNameEn = "New Zealand"
                        },
                        new
                        {
                            Alpha2Code = "NI",
                            Alpha3Code = "NIC",
                            CountryNameDe = "Nicaragua",
                            CountryNameEn = "Nicaragua"
                        },
                        new
                        {
                            Alpha2Code = "NE",
                            Alpha3Code = "NER",
                            CountryNameDe = "Niger (the)",
                            CountryNameEn = "Niger (the)"
                        },
                        new
                        {
                            Alpha2Code = "NG",
                            Alpha3Code = "NGA",
                            CountryNameDe = "Nigeria",
                            CountryNameEn = "Nigeria"
                        },
                        new
                        {
                            Alpha2Code = "NU",
                            Alpha3Code = "NIU",
                            CountryNameDe = "Niue",
                            CountryNameEn = "Niue"
                        },
                        new
                        {
                            Alpha2Code = "NF",
                            Alpha3Code = "NFK",
                            CountryNameDe = "Norfolk Island",
                            CountryNameEn = "Norfolk Island"
                        },
                        new
                        {
                            Alpha2Code = "MK",
                            Alpha3Code = "MKD",
                            CountryNameDe = "North Macedonia",
                            CountryNameEn = "North Macedonia"
                        },
                        new
                        {
                            Alpha2Code = "MP",
                            Alpha3Code = "MNP",
                            CountryNameDe = "Northern Mariana Islands (the)",
                            CountryNameEn = "Northern Mariana Islands (the)"
                        },
                        new
                        {
                            Alpha2Code = "NO",
                            Alpha3Code = "NOR",
                            CountryNameDe = "Norway",
                            CountryNameEn = "Norway"
                        },
                        new
                        {
                            Alpha2Code = "OM",
                            Alpha3Code = "OMN",
                            CountryNameDe = "Oman",
                            CountryNameEn = "Oman"
                        },
                        new
                        {
                            Alpha2Code = "PK",
                            Alpha3Code = "PAK",
                            CountryNameDe = "Pakistan",
                            CountryNameEn = "Pakistan"
                        },
                        new
                        {
                            Alpha2Code = "PW",
                            Alpha3Code = "PLW",
                            CountryNameDe = "Palau",
                            CountryNameEn = "Palau"
                        },
                        new
                        {
                            Alpha2Code = "PS",
                            Alpha3Code = "PSE",
                            CountryNameDe = "Palestine, State of",
                            CountryNameEn = "Palestine, State of"
                        },
                        new
                        {
                            Alpha2Code = "PA",
                            Alpha3Code = "PAN",
                            CountryNameDe = "Panama",
                            CountryNameEn = "Panama"
                        },
                        new
                        {
                            Alpha2Code = "PG",
                            Alpha3Code = "PNG",
                            CountryNameDe = "Papua New Guinea",
                            CountryNameEn = "Papua New Guinea"
                        },
                        new
                        {
                            Alpha2Code = "PY",
                            Alpha3Code = "PRY",
                            CountryNameDe = "Paraguay",
                            CountryNameEn = "Paraguay"
                        },
                        new
                        {
                            Alpha2Code = "PE",
                            Alpha3Code = "PER",
                            CountryNameDe = "Peru",
                            CountryNameEn = "Peru"
                        },
                        new
                        {
                            Alpha2Code = "PH",
                            Alpha3Code = "PHL",
                            CountryNameDe = "Philippines (the)",
                            CountryNameEn = "Philippines (the)"
                        },
                        new
                        {
                            Alpha2Code = "PN",
                            Alpha3Code = "PCN",
                            CountryNameDe = "Pitcairn",
                            CountryNameEn = "Pitcairn"
                        },
                        new
                        {
                            Alpha2Code = "PL",
                            Alpha3Code = "POL",
                            CountryNameDe = "Poland",
                            CountryNameEn = "Poland"
                        },
                        new
                        {
                            Alpha2Code = "PT",
                            Alpha3Code = "PRT",
                            CountryNameDe = "Portugal",
                            CountryNameEn = "Portugal"
                        },
                        new
                        {
                            Alpha2Code = "PR",
                            Alpha3Code = "PRI",
                            CountryNameDe = "Puerto Rico",
                            CountryNameEn = "Puerto Rico"
                        },
                        new
                        {
                            Alpha2Code = "QA",
                            Alpha3Code = "QAT",
                            CountryNameDe = "Qatar",
                            CountryNameEn = "Qatar"
                        },
                        new
                        {
                            Alpha2Code = "RO",
                            Alpha3Code = "ROU",
                            CountryNameDe = "Romania",
                            CountryNameEn = "Romania"
                        },
                        new
                        {
                            Alpha2Code = "RU",
                            Alpha3Code = "RUS",
                            CountryNameDe = "Russian Federation (the)",
                            CountryNameEn = "Russian Federation (the)"
                        },
                        new
                        {
                            Alpha2Code = "RW",
                            Alpha3Code = "RWA",
                            CountryNameDe = "Rwanda",
                            CountryNameEn = "Rwanda"
                        },
                        new
                        {
                            Alpha2Code = "RE",
                            Alpha3Code = "REU",
                            CountryNameDe = "Réunion",
                            CountryNameEn = "Réunion"
                        },
                        new
                        {
                            Alpha2Code = "BL",
                            Alpha3Code = "BLM",
                            CountryNameDe = "Saint Barthélemy",
                            CountryNameEn = "Saint Barthélemy"
                        },
                        new
                        {
                            Alpha2Code = "SH",
                            Alpha3Code = "SHN",
                            CountryNameDe = "Saint Helena, Ascension and Tristan da Cunha",
                            CountryNameEn = "Saint Helena, Ascension and Tristan da Cunha"
                        },
                        new
                        {
                            Alpha2Code = "KN",
                            Alpha3Code = "KNA",
                            CountryNameDe = "Saint Kitts and Nevis",
                            CountryNameEn = "Saint Kitts and Nevis"
                        },
                        new
                        {
                            Alpha2Code = "LC",
                            Alpha3Code = "LCA",
                            CountryNameDe = "Saint Lucia",
                            CountryNameEn = "Saint Lucia"
                        },
                        new
                        {
                            Alpha2Code = "MF",
                            Alpha3Code = "MAF",
                            CountryNameDe = "Saint Martin (French part)",
                            CountryNameEn = "Saint Martin (French part)"
                        },
                        new
                        {
                            Alpha2Code = "PM",
                            Alpha3Code = "SPM",
                            CountryNameDe = "Saint Pierre and Miquelon",
                            CountryNameEn = "Saint Pierre and Miquelon"
                        },
                        new
                        {
                            Alpha2Code = "VC",
                            Alpha3Code = "VCT",
                            CountryNameDe = "Saint Vincent and the Grenadines",
                            CountryNameEn = "Saint Vincent and the Grenadines"
                        },
                        new
                        {
                            Alpha2Code = "WS",
                            Alpha3Code = "WSM",
                            CountryNameDe = "Samoa",
                            CountryNameEn = "Samoa"
                        },
                        new
                        {
                            Alpha2Code = "SM",
                            Alpha3Code = "SMR",
                            CountryNameDe = "San Marino",
                            CountryNameEn = "San Marino"
                        },
                        new
                        {
                            Alpha2Code = "ST",
                            Alpha3Code = "STP",
                            CountryNameDe = "Sao Tome and Principe",
                            CountryNameEn = "Sao Tome and Principe"
                        },
                        new
                        {
                            Alpha2Code = "SA",
                            Alpha3Code = "SAU",
                            CountryNameDe = "Saudi Arabia",
                            CountryNameEn = "Saudi Arabia"
                        },
                        new
                        {
                            Alpha2Code = "SN",
                            Alpha3Code = "SEN",
                            CountryNameDe = "Senegal",
                            CountryNameEn = "Senegal"
                        },
                        new
                        {
                            Alpha2Code = "RS",
                            Alpha3Code = "SRB",
                            CountryNameDe = "Serbia",
                            CountryNameEn = "Serbia"
                        },
                        new
                        {
                            Alpha2Code = "SC",
                            Alpha3Code = "SYC",
                            CountryNameDe = "Seychelles",
                            CountryNameEn = "Seychelles"
                        },
                        new
                        {
                            Alpha2Code = "SL",
                            Alpha3Code = "SLE",
                            CountryNameDe = "Sierra Leone",
                            CountryNameEn = "Sierra Leone"
                        },
                        new
                        {
                            Alpha2Code = "SG",
                            Alpha3Code = "SGP",
                            CountryNameDe = "Singapore",
                            CountryNameEn = "Singapore"
                        },
                        new
                        {
                            Alpha2Code = "SX",
                            Alpha3Code = "SXM",
                            CountryNameDe = "Sint Maarten (Dutch part)",
                            CountryNameEn = "Sint Maarten (Dutch part)"
                        },
                        new
                        {
                            Alpha2Code = "SK",
                            Alpha3Code = "SVK",
                            CountryNameDe = "Slovakia",
                            CountryNameEn = "Slovakia"
                        },
                        new
                        {
                            Alpha2Code = "SI",
                            Alpha3Code = "SVN",
                            CountryNameDe = "Slovenia",
                            CountryNameEn = "Slovenia"
                        },
                        new
                        {
                            Alpha2Code = "SB",
                            Alpha3Code = "SLB",
                            CountryNameDe = "Solomon Islands",
                            CountryNameEn = "Solomon Islands"
                        },
                        new
                        {
                            Alpha2Code = "SO",
                            Alpha3Code = "SOM",
                            CountryNameDe = "Somalia",
                            CountryNameEn = "Somalia"
                        },
                        new
                        {
                            Alpha2Code = "ZA",
                            Alpha3Code = "ZAF",
                            CountryNameDe = "South Africa",
                            CountryNameEn = "South Africa"
                        },
                        new
                        {
                            Alpha2Code = "GS",
                            Alpha3Code = "SGS",
                            CountryNameDe = "South Georgia and the South Sandwich Islands",
                            CountryNameEn = "South Georgia and the South Sandwich Islands"
                        },
                        new
                        {
                            Alpha2Code = "SS",
                            Alpha3Code = "SSD",
                            CountryNameDe = "South Sudan",
                            CountryNameEn = "South Sudan"
                        },
                        new
                        {
                            Alpha2Code = "ES",
                            Alpha3Code = "ESP",
                            CountryNameDe = "Spain",
                            CountryNameEn = "Spain"
                        },
                        new
                        {
                            Alpha2Code = "LK",
                            Alpha3Code = "LKA",
                            CountryNameDe = "Sri Lanka",
                            CountryNameEn = "Sri Lanka"
                        },
                        new
                        {
                            Alpha2Code = "SD",
                            Alpha3Code = "SDN",
                            CountryNameDe = "Sudan (the)",
                            CountryNameEn = "Sudan (the)"
                        },
                        new
                        {
                            Alpha2Code = "SR",
                            Alpha3Code = "SUR",
                            CountryNameDe = "Suriname",
                            CountryNameEn = "Suriname"
                        },
                        new
                        {
                            Alpha2Code = "SJ",
                            Alpha3Code = "SJM",
                            CountryNameDe = "Svalbard and Jan Mayen",
                            CountryNameEn = "Svalbard and Jan Mayen"
                        },
                        new
                        {
                            Alpha2Code = "SE",
                            Alpha3Code = "SWE",
                            CountryNameDe = "Sweden",
                            CountryNameEn = "Sweden"
                        },
                        new
                        {
                            Alpha2Code = "CH",
                            Alpha3Code = "CHE",
                            CountryNameDe = "Switzerland",
                            CountryNameEn = "Switzerland"
                        },
                        new
                        {
                            Alpha2Code = "SY",
                            Alpha3Code = "SYR",
                            CountryNameDe = "Syrian Arab Republic (the)",
                            CountryNameEn = "Syrian Arab Republic (the)"
                        },
                        new
                        {
                            Alpha2Code = "TW",
                            Alpha3Code = "TWN",
                            CountryNameDe = "Taiwan (Province of China)",
                            CountryNameEn = "Taiwan (Province of China)"
                        },
                        new
                        {
                            Alpha2Code = "TJ",
                            Alpha3Code = "TJK",
                            CountryNameDe = "Tajikistan",
                            CountryNameEn = "Tajikistan"
                        },
                        new
                        {
                            Alpha2Code = "TZ",
                            Alpha3Code = "TZA",
                            CountryNameDe = "Tanzania, the United Republic of",
                            CountryNameEn = "Tanzania, the United Republic of"
                        },
                        new
                        {
                            Alpha2Code = "TH",
                            Alpha3Code = "THA",
                            CountryNameDe = "Thailand",
                            CountryNameEn = "Thailand"
                        },
                        new
                        {
                            Alpha2Code = "TL",
                            Alpha3Code = "TLS",
                            CountryNameDe = "Timor-Leste",
                            CountryNameEn = "Timor-Leste"
                        },
                        new
                        {
                            Alpha2Code = "TG",
                            Alpha3Code = "TGO",
                            CountryNameDe = "Togo",
                            CountryNameEn = "Togo"
                        },
                        new
                        {
                            Alpha2Code = "TK",
                            Alpha3Code = "TKL",
                            CountryNameDe = "Tokelau",
                            CountryNameEn = "Tokelau"
                        },
                        new
                        {
                            Alpha2Code = "TO",
                            Alpha3Code = "TON",
                            CountryNameDe = "Tonga",
                            CountryNameEn = "Tonga"
                        },
                        new
                        {
                            Alpha2Code = "TT",
                            Alpha3Code = "TTO",
                            CountryNameDe = "Trinidad and Tobago",
                            CountryNameEn = "Trinidad and Tobago"
                        },
                        new
                        {
                            Alpha2Code = "TN",
                            Alpha3Code = "TUN",
                            CountryNameDe = "Tunisia",
                            CountryNameEn = "Tunisia"
                        },
                        new
                        {
                            Alpha2Code = "TR",
                            Alpha3Code = "TUR",
                            CountryNameDe = "Turkey",
                            CountryNameEn = "Turkey"
                        },
                        new
                        {
                            Alpha2Code = "TM",
                            Alpha3Code = "TKM",
                            CountryNameDe = "Turkmenistan",
                            CountryNameEn = "Turkmenistan"
                        },
                        new
                        {
                            Alpha2Code = "TC",
                            Alpha3Code = "TCA",
                            CountryNameDe = "Turks and Caicos Islands (the)",
                            CountryNameEn = "Turks and Caicos Islands (the)"
                        },
                        new
                        {
                            Alpha2Code = "TV",
                            Alpha3Code = "TUV",
                            CountryNameDe = "Tuvalu",
                            CountryNameEn = "Tuvalu"
                        },
                        new
                        {
                            Alpha2Code = "UG",
                            Alpha3Code = "UGA",
                            CountryNameDe = "Uganda",
                            CountryNameEn = "Uganda"
                        },
                        new
                        {
                            Alpha2Code = "UA",
                            Alpha3Code = "UKR",
                            CountryNameDe = "Ukraine",
                            CountryNameEn = "Ukraine"
                        },
                        new
                        {
                            Alpha2Code = "AE",
                            Alpha3Code = "ARE",
                            CountryNameDe = "United Arab Emirates (the)",
                            CountryNameEn = "United Arab Emirates (the)"
                        },
                        new
                        {
                            Alpha2Code = "UM",
                            Alpha3Code = "UMI",
                            CountryNameDe = "United States Minor Outlying Islands (the)",
                            CountryNameEn = "United States Minor Outlying Islands (the)"
                        },
                        new
                        {
                            Alpha2Code = "US",
                            Alpha3Code = "USA",
                            CountryNameDe = "United States of America (the)",
                            CountryNameEn = "United States of America (the)"
                        },
                        new
                        {
                            Alpha2Code = "UY",
                            Alpha3Code = "URY",
                            CountryNameDe = "Uruguay",
                            CountryNameEn = "Uruguay"
                        },
                        new
                        {
                            Alpha2Code = "UZ",
                            Alpha3Code = "UZB",
                            CountryNameDe = "Uzbekistan",
                            CountryNameEn = "Uzbekistan"
                        },
                        new
                        {
                            Alpha2Code = "VU",
                            Alpha3Code = "VUT",
                            CountryNameDe = "Vanuatu",
                            CountryNameEn = "Vanuatu"
                        },
                        new
                        {
                            Alpha2Code = "VE",
                            Alpha3Code = "VEN",
                            CountryNameDe = "Venezuela (Bolivarian Republic of)",
                            CountryNameEn = "Venezuela (Bolivarian Republic of)"
                        },
                        new
                        {
                            Alpha2Code = "VN",
                            Alpha3Code = "VNM",
                            CountryNameDe = "Viet Nam",
                            CountryNameEn = "Viet Nam"
                        },
                        new
                        {
                            Alpha2Code = "VG",
                            Alpha3Code = "VGB",
                            CountryNameDe = "Virgin Islands (British)",
                            CountryNameEn = "Virgin Islands (British)"
                        },
                        new
                        {
                            Alpha2Code = "VI",
                            Alpha3Code = "VIR",
                            CountryNameDe = "Virgin Islands (U.S.)",
                            CountryNameEn = "Virgin Islands (U.S.)"
                        },
                        new
                        {
                            Alpha2Code = "WF",
                            Alpha3Code = "WLF",
                            CountryNameDe = "Wallis and Futuna",
                            CountryNameEn = "Wallis and Futuna"
                        },
                        new
                        {
                            Alpha2Code = "EH",
                            Alpha3Code = "ESH",
                            CountryNameDe = "Western Sahara*",
                            CountryNameEn = "Western Sahara*"
                        },
                        new
                        {
                            Alpha2Code = "YE",
                            Alpha3Code = "YEM",
                            CountryNameDe = "Yemen",
                            CountryNameEn = "Yemen"
                        },
                        new
                        {
                            Alpha2Code = "ZM",
                            Alpha3Code = "ZMB",
                            CountryNameDe = "Zambia",
                            CountryNameEn = "Zambia"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Document", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid?>("CompanyUserId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_user_id");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<byte[]>("DocumentContent")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("document_content");

                    b.Property<byte[]>("DocumentHash")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("document_hash");

                    b.Property<string>("DocumentName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("document_name");

                    b.Property<int>("DocumentStatusId")
                        .HasColumnType("integer")
                        .HasColumnName("document_status_id");

                    b.Property<int?>("DocumentTypeId")
                        .HasColumnType("integer")
                        .HasColumnName("document_type_id");

                    b.HasKey("Id")
                        .HasName("pk_documents");

                    b.HasIndex("CompanyUserId")
                        .HasDatabaseName("ix_documents_company_user_id");

                    b.HasIndex("DocumentStatusId")
                        .HasDatabaseName("ix_documents_document_status_id");

                    b.HasIndex("DocumentTypeId")
                        .HasDatabaseName("ix_documents_document_type_id");

                    b.ToTable("documents", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.DocumentStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_document_status");

                    b.ToTable("document_status", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "PENDING"
                        },
                        new
                        {
                            Id = 2,
                            Label = "LOCKED"
                        },
                        new
                        {
                            Id = 3,
                            Label = "INACTIVE"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.DocumentTemplate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<DateTimeOffset?>("DateLastChanged")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_last_changed");

                    b.Property<string>("Documenttemplatename")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("documenttemplatename");

                    b.Property<string>("Documenttemplateversion")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("documenttemplateversion");

                    b.HasKey("Id")
                        .HasName("pk_document_templates");

                    b.ToTable("document_templates", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.DocumentType", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_document_types");

                    b.ToTable("document_types", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "CX_FRAME_CONTRACT"
                        },
                        new
                        {
                            Id = 2,
                            Label = "COMMERCIAL_REGISTER_EXTRACT"
                        },
                        new
                        {
                            Id = 3,
                            Label = "APP_CONTRACT"
                        },
                        new
                        {
                            Id = 4,
                            Label = "DATA_CONTRACT"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IamClient", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("ClientClientId")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("client_client_id");

                    b.HasKey("Id")
                        .HasName("pk_iam_clients");

                    b.HasIndex("ClientClientId")
                        .IsUnique()
                        .HasDatabaseName("ix_iam_clients_client_client_id");

                    b.ToTable("iam_clients", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IamIdentityProvider", b =>
                {
                    b.Property<string>("IamIdpAlias")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("iam_idp_alias");

                    b.Property<Guid>("IdentityProviderId")
                        .HasColumnType("uuid")
                        .HasColumnName("identity_provider_id");

                    b.HasKey("IamIdpAlias")
                        .HasName("pk_iam_identity_providers");

                    b.HasIndex("IdentityProviderId")
                        .IsUnique()
                        .HasDatabaseName("ix_iam_identity_providers_identity_provider_id");

                    b.ToTable("iam_identity_providers", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IamServiceAccount", b =>
                {
                    b.Property<string>("ClientId")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("client_id");

                    b.Property<string>("ClientClientId")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("client_client_id");

                    b.Property<Guid>("CompanyServiceAccountId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_service_account_id");

                    b.Property<string>("UserEntityId")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("user_entity_id");

                    b.HasKey("ClientId")
                        .HasName("pk_iam_service_accounts");

                    b.HasIndex("ClientClientId")
                        .IsUnique()
                        .HasDatabaseName("ix_iam_service_accounts_client_client_id");

                    b.HasIndex("CompanyServiceAccountId")
                        .IsUnique()
                        .HasDatabaseName("ix_iam_service_accounts_company_service_account_id");

                    b.HasIndex("UserEntityId")
                        .IsUnique()
                        .HasDatabaseName("ix_iam_service_accounts_user_entity_id");

                    b.ToTable("iam_service_accounts", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IamUser", b =>
                {
                    b.Property<string>("UserEntityId")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("user_entity_id");

                    b.Property<Guid>("CompanyUserId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_user_id");

                    b.HasKey("UserEntityId")
                        .HasName("pk_iam_users");

                    b.HasIndex("CompanyUserId")
                        .IsUnique()
                        .HasDatabaseName("ix_iam_users_company_user_id");

                    b.ToTable("iam_users", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IdentityProvider", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<int>("IdentityProviderCategoryId")
                        .HasColumnType("integer")
                        .HasColumnName("identity_provider_category_id");

                    b.HasKey("Id")
                        .HasName("pk_identity_providers");

                    b.HasIndex("IdentityProviderCategoryId")
                        .HasDatabaseName("ix_identity_providers_identity_provider_category_id");

                    b.ToTable("identity_providers", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IdentityProviderCategory", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_identity_provider_categories");

                    b.ToTable("identity_provider_categories", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "KEYCLOAK_SHARED"
                        },
                        new
                        {
                            Id = 2,
                            Label = "KEYCLOAK_OIDC"
                        },
                        new
                        {
                            Id = 3,
                            Label = "KEYCLOAK_SAML"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Invitation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("CompanyApplicationId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_application_id");

                    b.Property<Guid>("CompanyUserId")
                        .HasColumnType("uuid")
                        .HasColumnName("company_user_id");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<int>("InvitationStatusId")
                        .HasColumnType("integer")
                        .HasColumnName("invitation_status_id");

                    b.HasKey("Id")
                        .HasName("pk_invitations");

                    b.HasIndex("CompanyApplicationId")
                        .HasDatabaseName("ix_invitations_company_application_id");

                    b.HasIndex("CompanyUserId")
                        .HasDatabaseName("ix_invitations_company_user_id");

                    b.HasIndex("InvitationStatusId")
                        .HasDatabaseName("ix_invitations_invitation_status_id");

                    b.ToTable("invitations", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.InvitationStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_invitation_statuses");

                    b.ToTable("invitation_statuses", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "CREATED"
                        },
                        new
                        {
                            Id = 2,
                            Label = "PENDING"
                        },
                        new
                        {
                            Id = 3,
                            Label = "ACCEPTED"
                        },
                        new
                        {
                            Id = 4,
                            Label = "DECLINED"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Language", b =>
                {
                    b.Property<string>("ShortName")
                        .HasMaxLength(2)
                        .HasColumnType("character(2)")
                        .HasColumnName("short_name")
                        .IsFixedLength();

                    b.Property<string>("LongNameDe")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("long_name_de");

                    b.Property<string>("LongNameEn")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("long_name_en");

                    b.HasKey("ShortName")
                        .HasName("pk_languages");

                    b.ToTable("languages", "portal");

                    b.HasData(
                        new
                        {
                            ShortName = "de",
                            LongNameDe = "deutsch",
                            LongNameEn = "german"
                        },
                        new
                        {
                            ShortName = "en",
                            LongNameDe = "englisch",
                            LongNameEn = "english"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Notification", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<Guid?>("CreatorUserId")
                        .HasColumnType("uuid")
                        .HasColumnName("creator_user_id");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_created");

                    b.Property<DateTimeOffset?>("DueDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("due_date");

                    b.Property<int>("NotificationTypeId")
                        .HasColumnType("integer")
                        .HasColumnName("notification_type_id");

                    b.Property<int>("ReadStatusId")
                        .HasColumnType("integer")
                        .HasColumnName("read_status_id");

                    b.Property<Guid>("ReceiverUserId")
                        .HasColumnType("uuid")
                        .HasColumnName("receiver_user_id");

                    b.HasKey("Id")
                        .HasName("pk_notifications");

                    b.HasIndex("CreatorUserId")
                        .HasDatabaseName("ix_notifications_creator_user_id");

                    b.HasIndex("NotificationTypeId")
                        .HasDatabaseName("ix_notifications_notification_type_id");

                    b.HasIndex("ReadStatusId")
                        .HasDatabaseName("ix_notifications_read_status_id");

                    b.HasIndex("ReceiverUserId")
                        .HasDatabaseName("ix_notifications_receiver_user_id");

                    b.ToTable("notifications", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.NotificationStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_notification_status");

                    b.ToTable("notification_status", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "UNREAD"
                        },
                        new
                        {
                            Id = 2,
                            Label = "READ"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.NotificationType", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("label");

                    b.HasKey("Id")
                        .HasName("pk_notification_type");

                    b.ToTable("notification_type", "portal");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Label = "INFO"
                        },
                        new
                        {
                            Id = 2,
                            Label = "ACTION"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.UseCase", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("name");

                    b.Property<string>("Shortname")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("shortname");

                    b.HasKey("Id")
                        .HasName("pk_use_cases");

                    b.ToTable("use_cases", "portal");

                    b.HasData(
                        new
                        {
                            Id = new Guid("1aacde78-35ec-4df3-ba1e-f988cddcbbd9"),
                            Name = "None",
                            Shortname = "None"
                        },
                        new
                        {
                            Id = new Guid("1aacde78-35ec-4df3-ba1e-f988cddcbbd8"),
                            Name = "Circular Economy",
                            Shortname = "CE"
                        },
                        new
                        {
                            Id = new Guid("41e4a4c0-aae4-41c0-97c9-ebafde410de4"),
                            Name = "Demand and Capacity Management",
                            Shortname = "DCM"
                        },
                        new
                        {
                            Id = new Guid("c065a349-f649-47f8-94d5-1a504a855419"),
                            Name = "Quality Management",
                            Shortname = "QM"
                        },
                        new
                        {
                            Id = new Guid("6909ccc7-37c8-4088-99ab-790f20702460"),
                            Name = "Business Partner Management",
                            Shortname = "BPDM"
                        },
                        new
                        {
                            Id = new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b86"),
                            Name = "Traceability",
                            Shortname = "T"
                        },
                        new
                        {
                            Id = new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b87"),
                            Name = "Sustainability & CO2-Footprint",
                            Shortname = "CO2"
                        },
                        new
                        {
                            Id = new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b88"),
                            Name = "Manufacturing as a Service",
                            Shortname = "MaaS"
                        },
                        new
                        {
                            Id = new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b89"),
                            Name = "Real-Time Control",
                            Shortname = "RTC"
                        },
                        new
                        {
                            Id = new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b90"),
                            Name = "Modular Production",
                            Shortname = "MP"
                        });
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.UserRole", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("IamClientId")
                        .HasColumnType("uuid")
                        .HasColumnName("iam_client_id");

                    b.Property<string>("UserRoleText")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("user_role");

                    b.HasKey("Id")
                        .HasName("pk_user_roles");

                    b.HasIndex("IamClientId")
                        .HasDatabaseName("ix_user_roles_iam_client_id");

                    b.ToTable("user_roles", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.UserRoleDescription", b =>
                {
                    b.Property<Guid>("UserRoleId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_role_id");

                    b.Property<string>("LanguageShortName")
                        .HasMaxLength(2)
                        .HasColumnType("character(2)")
                        .HasColumnName("language_short_name");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("description");

                    b.HasKey("UserRoleId", "LanguageShortName")
                        .HasName("pk_user_role_descriptions");

                    b.HasIndex("LanguageShortName")
                        .HasDatabaseName("ix_user_role_descriptions_language_short_name");

                    b.ToTable("user_role_descriptions", "portal");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Address", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Country", "Country")
                        .WithMany("Addresses")
                        .HasForeignKey("CountryAlpha2Code")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_addresses_countries_country_temp_id");

                    b.Navigation("Country");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Agreement", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AgreementCategory", "AgreementCategory")
                        .WithMany("Agreements")
                        .HasForeignKey("AgreementCategoryId")
                        .IsRequired()
                        .HasConstraintName("fk_agreements_agreement_categories_agreement_category_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.App", "App")
                        .WithMany("Agreements")
                        .HasForeignKey("AppId")
                        .HasConstraintName("fk_agreements_apps_app_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Company", "IssuerCompany")
                        .WithMany("Agreements")
                        .HasForeignKey("IssuerCompanyId")
                        .IsRequired()
                        .HasConstraintName("fk_agreements_companies_issuer_company_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.UseCase", "UseCase")
                        .WithMany("Agreements")
                        .HasForeignKey("UseCaseId")
                        .HasConstraintName("fk_agreements_use_cases_use_case_id");

                    b.Navigation("AgreementCategory");

                    b.Navigation("App");

                    b.Navigation("IssuerCompany");

                    b.Navigation("UseCase");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AgreementAssignedCompanyRole", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Agreement", "Agreement")
                        .WithMany("AgreementAssignedCompanyRoles")
                        .HasForeignKey("AgreementId")
                        .IsRequired()
                        .HasConstraintName("fk_agreement_assigned_company_roles_agreements_agreement_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyRole", "CompanyRole")
                        .WithMany("AgreementAssignedCompanyRoles")
                        .HasForeignKey("CompanyRoleId")
                        .IsRequired()
                        .HasConstraintName("fk_agreement_assigned_company_roles_company_roles_company_role");

                    b.Navigation("Agreement");

                    b.Navigation("CompanyRole");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AgreementAssignedDocumentTemplate", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Agreement", "Agreement")
                        .WithMany("AgreementAssignedDocumentTemplates")
                        .HasForeignKey("AgreementId")
                        .IsRequired()
                        .HasConstraintName("fk_agreement_assigned_document_templates_agreements_agreement_");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.DocumentTemplate", "DocumentTemplate")
                        .WithOne("AgreementAssignedDocumentTemplate")
                        .HasForeignKey("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AgreementAssignedDocumentTemplate", "DocumentTemplateId")
                        .IsRequired()
                        .HasConstraintName("fk_agreement_assigned_document_templates_document_templates_do");

                    b.Navigation("Agreement");

                    b.Navigation("DocumentTemplate");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.App", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppStatus", "AppStatus")
                        .WithMany("Apps")
                        .HasForeignKey("AppStatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_apps_app_statuses_app_status_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Company", "ProviderCompany")
                        .WithMany("ProvidedApps")
                        .HasForeignKey("ProviderCompanyId")
                        .HasConstraintName("fk_apps_companies_provider_company_id");

                    b.Navigation("AppStatus");

                    b.Navigation("ProviderCompany");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppAssignedClient", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.App", "App")
                        .WithMany()
                        .HasForeignKey("AppId")
                        .IsRequired()
                        .HasConstraintName("fk_app_assigned_clients_apps_app_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IamClient", "IamClient")
                        .WithMany()
                        .HasForeignKey("IamClientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_app_assigned_clients_iam_clients_iam_client_id");

                    b.Navigation("App");

                    b.Navigation("IamClient");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppAssignedLicense", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.App", "App")
                        .WithMany()
                        .HasForeignKey("AppId")
                        .IsRequired()
                        .HasConstraintName("fk_app_assigned_licenses_apps_app_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppLicense", "AppLicense")
                        .WithMany()
                        .HasForeignKey("AppLicenseId")
                        .IsRequired()
                        .HasConstraintName("fk_app_assigned_licenses_app_licenses_app_license_id");

                    b.Navigation("App");

                    b.Navigation("AppLicense");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppAssignedUseCase", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.App", "App")
                        .WithMany()
                        .HasForeignKey("AppId")
                        .IsRequired()
                        .HasConstraintName("fk_app_assigned_use_cases_apps_app_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.UseCase", "UseCase")
                        .WithMany()
                        .HasForeignKey("UseCaseId")
                        .IsRequired()
                        .HasConstraintName("fk_app_assigned_use_cases_use_cases_use_case_id");

                    b.Navigation("App");

                    b.Navigation("UseCase");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppDescription", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.App", "App")
                        .WithMany("AppDescriptions")
                        .HasForeignKey("AppId")
                        .IsRequired()
                        .HasConstraintName("fk_app_descriptions_apps_app_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Language", "Language")
                        .WithMany("AppDescriptions")
                        .HasForeignKey("LanguageShortName")
                        .IsRequired()
                        .HasConstraintName("fk_app_descriptions_languages_language_temp_id");

                    b.Navigation("App");

                    b.Navigation("Language");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppDetailImage", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.App", "App")
                        .WithMany("AppDetailImages")
                        .HasForeignKey("AppId")
                        .IsRequired()
                        .HasConstraintName("fk_app_detail_images_apps_app_id");

                    b.Navigation("App");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppLanguage", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.App", "App")
                        .WithMany()
                        .HasForeignKey("AppId")
                        .IsRequired()
                        .HasConstraintName("fk_app_languages_apps_app_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Language", "Language")
                        .WithMany()
                        .HasForeignKey("LanguageShortName")
                        .IsRequired()
                        .HasConstraintName("fk_app_languages_languages_language_temp_id1");

                    b.Navigation("App");

                    b.Navigation("Language");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppTag", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.App", "App")
                        .WithMany("Tags")
                        .HasForeignKey("AppId")
                        .IsRequired()
                        .HasConstraintName("fk_app_tags_apps_app_id");

                    b.Navigation("App");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Company", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Address", "Address")
                        .WithMany("Companies")
                        .HasForeignKey("AddressId")
                        .HasConstraintName("fk_companies_addresses_address_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyStatus", "CompanyStatus")
                        .WithMany("Companies")
                        .HasForeignKey("CompanyStatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_companies_company_statuses_company_status_id");

                    b.Navigation("Address");

                    b.Navigation("CompanyStatus");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyApplication", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyApplicationStatus", "ApplicationStatus")
                        .WithMany("CompanyApplications")
                        .HasForeignKey("ApplicationStatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_company_applications_company_application_statuses_applicati");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Company", "Company")
                        .WithMany("CompanyApplications")
                        .HasForeignKey("CompanyId")
                        .IsRequired()
                        .HasConstraintName("fk_company_applications_companies_company_id");

                    b.Navigation("ApplicationStatus");

                    b.Navigation("Company");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyAssignedApp", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.App", "App")
                        .WithMany()
                        .HasForeignKey("AppId")
                        .IsRequired()
                        .HasConstraintName("fk_company_assigned_apps_apps_app_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppSubscriptionStatus", "AppSubscriptionStatus")
                        .WithMany("AppSubscriptions")
                        .HasForeignKey("AppSubscriptionStatusId")
                        .IsRequired()
                        .HasConstraintName("fk_company_assigned_apps_app_subscription_statuses_app_subscri");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Company", "Company")
                        .WithMany()
                        .HasForeignKey("CompanyId")
                        .IsRequired()
                        .HasConstraintName("fk_company_assigned_apps_companies_company_id");

                    b.Navigation("App");

                    b.Navigation("AppSubscriptionStatus");

                    b.Navigation("Company");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyAssignedRole", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Company", "Company")
                        .WithMany("CompanyAssignedRoles")
                        .HasForeignKey("CompanyId")
                        .IsRequired()
                        .HasConstraintName("fk_company_assigned_roles_companies_company_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyRole", "CompanyRole")
                        .WithMany()
                        .HasForeignKey("CompanyRoleId")
                        .IsRequired()
                        .HasConstraintName("fk_company_assigned_roles_company_roles_company_role_id");

                    b.Navigation("Company");

                    b.Navigation("CompanyRole");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyAssignedUseCase", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Company", "Company")
                        .WithMany()
                        .HasForeignKey("CompanyId")
                        .IsRequired()
                        .HasConstraintName("fk_company_assigned_use_cases_companies_company_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.UseCase", "UseCase")
                        .WithMany()
                        .HasForeignKey("UseCaseId")
                        .IsRequired()
                        .HasConstraintName("fk_company_assigned_use_cases_use_cases_use_case_id");

                    b.Navigation("Company");

                    b.Navigation("UseCase");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyIdentityProvider", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Company", "Company")
                        .WithMany()
                        .HasForeignKey("CompanyId")
                        .IsRequired()
                        .HasConstraintName("fk_company_identity_providers_companies_company_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IdentityProvider", "IdentityProvider")
                        .WithMany()
                        .HasForeignKey("IdentityProviderId")
                        .IsRequired()
                        .HasConstraintName("fk_company_identity_providers_identity_providers_identity_prov");

                    b.Navigation("Company");

                    b.Navigation("IdentityProvider");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyRoleDescription", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyRole", "CompanyRole")
                        .WithMany("CompanyRoleDescriptions")
                        .HasForeignKey("CompanyRoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_company_role_descriptions_company_roles_company_role_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Language", "Language")
                        .WithMany("CompanyRoleDescriptions")
                        .HasForeignKey("LanguageShortName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_company_role_descriptions_languages_language_temp_id2");

                    b.Navigation("CompanyRole");

                    b.Navigation("Language");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyServiceAccount", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Company", "Company")
                        .WithMany("CompanyServiceAccounts")
                        .HasForeignKey("CompanyId")
                        .IsRequired()
                        .HasConstraintName("fk_company_service_accounts_companies_company_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyServiceAccountStatus", "CompanyServiceAccountStatus")
                        .WithMany("CompanyServiceAccounts")
                        .HasForeignKey("CompanyServiceAccountStatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_company_service_accounts_company_service_account_statuses_c");

                    b.Navigation("Company");

                    b.Navigation("CompanyServiceAccountStatus");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyServiceAccountAssignedRole", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyServiceAccount", "CompanyServiceAccount")
                        .WithMany("CompanyServiceAccountAssignedRoles")
                        .HasForeignKey("CompanyServiceAccountId")
                        .IsRequired()
                        .HasConstraintName("fk_company_service_account_assigned_roles_company_service_acco");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.UserRole", "UserRole")
                        .WithMany()
                        .HasForeignKey("UserRoleId")
                        .IsRequired()
                        .HasConstraintName("fk_company_service_account_assigned_roles_user_roles_user_role");

                    b.Navigation("CompanyServiceAccount");

                    b.Navigation("UserRole");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUser", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Company", "Company")
                        .WithMany("CompanyUsers")
                        .HasForeignKey("CompanyId")
                        .IsRequired()
                        .HasConstraintName("fk_company_users_companies_company_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUserStatus", "CompanyUserStatus")
                        .WithMany("CompanyUsers")
                        .HasForeignKey("CompanyUserStatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_company_users_company_user_statuses_company_user_status_id");

                    b.Navigation("Company");

                    b.Navigation("CompanyUserStatus");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUserAssignedAppFavourite", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.App", "App")
                        .WithMany()
                        .HasForeignKey("AppId")
                        .IsRequired()
                        .HasConstraintName("fk_company_user_assigned_app_favourites_apps_app_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUser", "CompanyUser")
                        .WithMany()
                        .HasForeignKey("CompanyUserId")
                        .IsRequired()
                        .HasConstraintName("fk_company_user_assigned_app_favourites_company_users_company_");

                    b.Navigation("App");

                    b.Navigation("CompanyUser");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUserAssignedBusinessPartner", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUser", "CompanyUser")
                        .WithMany("CompanyUserAssignedBusinessPartners")
                        .HasForeignKey("CompanyUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_company_user_assigned_business_partners_company_users_compa");

                    b.Navigation("CompanyUser");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUserAssignedRole", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUser", "CompanyUser")
                        .WithMany("CompanyUserAssignedRoles")
                        .HasForeignKey("CompanyUserId")
                        .IsRequired()
                        .HasConstraintName("fk_company_user_assigned_roles_company_users_company_user_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.UserRole", "UserRole")
                        .WithMany()
                        .HasForeignKey("UserRoleId")
                        .IsRequired()
                        .HasConstraintName("fk_company_user_assigned_roles_user_roles_user_role_id");

                    b.Navigation("CompanyUser");

                    b.Navigation("UserRole");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Connector", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Company", "Host")
                        .WithMany("HostedConnectors")
                        .HasForeignKey("HostId")
                        .HasConstraintName("fk_connectors_companies_host_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Country", "Location")
                        .WithMany("Connectors")
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_connectors_countries_location_temp_id1");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Company", "Provider")
                        .WithMany("ProvidedConnectors")
                        .HasForeignKey("ProviderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_connectors_companies_provider_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.ConnectorStatus", "Status")
                        .WithMany("Connectors")
                        .HasForeignKey("StatusId")
                        .IsRequired()
                        .HasConstraintName("fk_connectors_connector_statuses_status_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.ConnectorType", "Type")
                        .WithMany("Connectors")
                        .HasForeignKey("TypeId")
                        .IsRequired()
                        .HasConstraintName("fk_connectors_connector_types_type_id");

                    b.Navigation("Host");

                    b.Navigation("Location");

                    b.Navigation("Provider");

                    b.Navigation("Status");

                    b.Navigation("Type");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Consent", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Agreement", "Agreement")
                        .WithMany("Consents")
                        .HasForeignKey("AgreementId")
                        .IsRequired()
                        .HasConstraintName("fk_consents_agreements_agreement_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Company", "Company")
                        .WithMany("Consents")
                        .HasForeignKey("CompanyId")
                        .IsRequired()
                        .HasConstraintName("fk_consents_companies_company_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUser", "CompanyUser")
                        .WithMany("Consents")
                        .HasForeignKey("CompanyUserId")
                        .IsRequired()
                        .HasConstraintName("fk_consents_company_users_company_user_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.ConsentStatus", "ConsentStatus")
                        .WithMany("Consents")
                        .HasForeignKey("ConsentStatusId")
                        .IsRequired()
                        .HasConstraintName("fk_consents_consent_statuses_consent_status_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Document", "Document")
                        .WithMany("Consents")
                        .HasForeignKey("DocumentId")
                        .HasConstraintName("fk_consents_documents_document_id");

                    b.Navigation("Agreement");

                    b.Navigation("Company");

                    b.Navigation("CompanyUser");

                    b.Navigation("ConsentStatus");

                    b.Navigation("Document");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Document", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUser", "CompanyUser")
                        .WithMany("Documents")
                        .HasForeignKey("CompanyUserId")
                        .HasConstraintName("fk_documents_company_users_company_user_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.DocumentStatus", "DocumentStatus")
                        .WithMany("Documents")
                        .HasForeignKey("DocumentStatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_documents_document_status_document_status_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.DocumentType", "DocumentType")
                        .WithMany("Documents")
                        .HasForeignKey("DocumentTypeId")
                        .HasConstraintName("fk_documents_document_types_document_type_id");

                    b.Navigation("CompanyUser");

                    b.Navigation("DocumentStatus");

                    b.Navigation("DocumentType");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IamIdentityProvider", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IdentityProvider", "IdentityProvider")
                        .WithOne("IamIdentityProvider")
                        .HasForeignKey("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IamIdentityProvider", "IdentityProviderId")
                        .IsRequired()
                        .HasConstraintName("fk_iam_identity_providers_identity_providers_identity_provider");

                    b.Navigation("IdentityProvider");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IamServiceAccount", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyServiceAccount", "CompanyServiceAccount")
                        .WithOne("IamServiceAccount")
                        .HasForeignKey("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IamServiceAccount", "CompanyServiceAccountId")
                        .IsRequired()
                        .HasConstraintName("fk_iam_service_accounts_company_service_accounts_company_servi");

                    b.Navigation("CompanyServiceAccount");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IamUser", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUser", "CompanyUser")
                        .WithOne("IamUser")
                        .HasForeignKey("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IamUser", "CompanyUserId")
                        .IsRequired()
                        .HasConstraintName("fk_iam_users_company_users_company_user_id");

                    b.Navigation("CompanyUser");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IdentityProvider", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IdentityProviderCategory", "IdentityProviderCategory")
                        .WithMany("IdentityProviders")
                        .HasForeignKey("IdentityProviderCategoryId")
                        .IsRequired()
                        .HasConstraintName("fk_identity_providers_identity_provider_categories_identity_pr");

                    b.Navigation("IdentityProviderCategory");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Invitation", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyApplication", "CompanyApplication")
                        .WithMany("Invitations")
                        .HasForeignKey("CompanyApplicationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_invitations_company_applications_company_application_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUser", "CompanyUser")
                        .WithMany("Invitations")
                        .HasForeignKey("CompanyUserId")
                        .IsRequired()
                        .HasConstraintName("fk_invitations_company_users_company_user_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.InvitationStatus", "InvitationStatus")
                        .WithMany("Invitations")
                        .HasForeignKey("InvitationStatusId")
                        .IsRequired()
                        .HasConstraintName("fk_invitations_invitation_statuses_invitation_status_id");

                    b.Navigation("CompanyApplication");

                    b.Navigation("CompanyUser");

                    b.Navigation("InvitationStatus");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Notification", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUser", "Creator")
                        .WithMany("CreatedNotifications")
                        .HasForeignKey("CreatorUserId")
                        .HasConstraintName("fk_notifications_company_users_creator_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.NotificationType", "NotificationType")
                        .WithMany("Notifications")
                        .HasForeignKey("NotificationTypeId")
                        .IsRequired()
                        .HasConstraintName("fk_notifications_notification_type_notification_type_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.NotificationStatus", "ReadStatus")
                        .WithMany("Notifications")
                        .HasForeignKey("ReadStatusId")
                        .IsRequired()
                        .HasConstraintName("fk_notifications_notification_status_read_status_id");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUser", "Receiver")
                        .WithMany("Notifications")
                        .HasForeignKey("ReceiverUserId")
                        .IsRequired()
                        .HasConstraintName("fk_notifications_company_users_receiver_id");

                    b.Navigation("Creator");

                    b.Navigation("NotificationType");

                    b.Navigation("ReadStatus");

                    b.Navigation("Receiver");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.UserRole", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IamClient", "IamClient")
                        .WithMany("UserRoles")
                        .HasForeignKey("IamClientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_roles_iam_clients_iam_client_id");

                    b.Navigation("IamClient");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.UserRoleDescription", b =>
                {
                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Language", "Language")
                        .WithMany("UserRoleDescriptions")
                        .HasForeignKey("LanguageShortName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_role_descriptions_languages_language_short_name");

                    b.HasOne("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.UserRole", "UserRole")
                        .WithMany("UserRoleDescriptions")
                        .HasForeignKey("UserRoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_role_descriptions_user_roles_user_role_id");

                    b.Navigation("Language");

                    b.Navigation("UserRole");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Address", b =>
                {
                    b.Navigation("Companies");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Agreement", b =>
                {
                    b.Navigation("AgreementAssignedCompanyRoles");

                    b.Navigation("AgreementAssignedDocumentTemplates");

                    b.Navigation("Consents");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AgreementCategory", b =>
                {
                    b.Navigation("Agreements");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.App", b =>
                {
                    b.Navigation("Agreements");

                    b.Navigation("AppDescriptions");

                    b.Navigation("AppDetailImages");

                    b.Navigation("Tags");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppStatus", b =>
                {
                    b.Navigation("Apps");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.AppSubscriptionStatus", b =>
                {
                    b.Navigation("AppSubscriptions");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Company", b =>
                {
                    b.Navigation("Agreements");

                    b.Navigation("CompanyApplications");

                    b.Navigation("CompanyAssignedRoles");

                    b.Navigation("CompanyServiceAccounts");

                    b.Navigation("CompanyUsers");

                    b.Navigation("Consents");

                    b.Navigation("HostedConnectors");

                    b.Navigation("ProvidedApps");

                    b.Navigation("ProvidedConnectors");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyApplication", b =>
                {
                    b.Navigation("Invitations");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyApplicationStatus", b =>
                {
                    b.Navigation("CompanyApplications");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyRole", b =>
                {
                    b.Navigation("AgreementAssignedCompanyRoles");

                    b.Navigation("CompanyRoleDescriptions");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyServiceAccount", b =>
                {
                    b.Navigation("CompanyServiceAccountAssignedRoles");

                    b.Navigation("IamServiceAccount");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyServiceAccountStatus", b =>
                {
                    b.Navigation("CompanyServiceAccounts");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyStatus", b =>
                {
                    b.Navigation("Companies");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUser", b =>
                {
                    b.Navigation("CompanyUserAssignedBusinessPartners");

                    b.Navigation("CompanyUserAssignedRoles");

                    b.Navigation("Consents");

                    b.Navigation("CreatedNotifications");

                    b.Navigation("Documents");

                    b.Navigation("IamUser");

                    b.Navigation("Invitations");

                    b.Navigation("Notifications");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.CompanyUserStatus", b =>
                {
                    b.Navigation("CompanyUsers");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.ConnectorStatus", b =>
                {
                    b.Navigation("Connectors");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.ConnectorType", b =>
                {
                    b.Navigation("Connectors");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.ConsentStatus", b =>
                {
                    b.Navigation("Consents");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Country", b =>
                {
                    b.Navigation("Addresses");

                    b.Navigation("Connectors");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Document", b =>
                {
                    b.Navigation("Consents");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.DocumentStatus", b =>
                {
                    b.Navigation("Documents");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.DocumentTemplate", b =>
                {
                    b.Navigation("AgreementAssignedDocumentTemplate");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.DocumentType", b =>
                {
                    b.Navigation("Documents");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IamClient", b =>
                {
                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IdentityProvider", b =>
                {
                    b.Navigation("IamIdentityProvider");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.IdentityProviderCategory", b =>
                {
                    b.Navigation("IdentityProviders");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.InvitationStatus", b =>
                {
                    b.Navigation("Invitations");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.Language", b =>
                {
                    b.Navigation("AppDescriptions");

                    b.Navigation("CompanyRoleDescriptions");

                    b.Navigation("UserRoleDescriptions");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.NotificationStatus", b =>
                {
                    b.Navigation("Notifications");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.NotificationType", b =>
                {
                    b.Navigation("Notifications");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.UseCase", b =>
                {
                    b.Navigation("Agreements");
                });

            modelBuilder.Entity("CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities.UserRole", b =>
                {
                    b.Navigation("UserRoleDescriptions");
                });
#pragma warning restore 612, 618
        }
    }
}
