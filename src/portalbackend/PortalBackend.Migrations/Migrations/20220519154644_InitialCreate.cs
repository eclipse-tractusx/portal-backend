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

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "portal");

            migrationBuilder.CreateTable(
                name: "agreement_categories",
                schema: "portal",
                columns: table => new
                {
                    agreement_category_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_categories", x => x.agreement_category_id);
                });

            migrationBuilder.CreateTable(
                name: "app_licenses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    licensetext = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_licenses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "app_statuses",
                schema: "portal",
                columns: table => new
                {
                    app_status_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_statuses", x => x.app_status_id);
                });

            migrationBuilder.CreateTable(
                name: "company_application_statuses",
                schema: "portal",
                columns: table => new
                {
                    application_status_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_application_statuses", x => x.application_status_id);
                });

            migrationBuilder.CreateTable(
                name: "company_roles",
                schema: "portal",
                columns: table => new
                {
                    company_role_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_roles", x => x.company_role_id);
                });

            migrationBuilder.CreateTable(
                name: "company_statuses",
                schema: "portal",
                columns: table => new
                {
                    company_status_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_statuses", x => x.company_status_id);
                });

            migrationBuilder.CreateTable(
                name: "company_user_statuses",
                schema: "portal",
                columns: table => new
                {
                    company_user_status_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_user_statuses", x => x.company_user_status_id);
                });

            migrationBuilder.CreateTable(
                name: "consent_statuses",
                schema: "portal",
                columns: table => new
                {
                    consent_status_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consent_statuses", x => x.consent_status_id);
                });

            migrationBuilder.CreateTable(
                name: "countries",
                schema: "portal",
                columns: table => new
                {
                    alpha2code = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    alpha3code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: true),
                    country_name_de = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    country_name_en = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_countries", x => x.alpha2code);
                });

            migrationBuilder.CreateTable(
                name: "document_templates",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    documenttemplatename = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    documenttemplateversion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_types",
                schema: "portal",
                columns: table => new
                {
                    document_type_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_types", x => x.document_type_id);
                });

            migrationBuilder.CreateTable(
                name: "iam_clients",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_client_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iam_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "identity_provider_categories",
                schema: "portal",
                columns: table => new
                {
                    identity_provider_category_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_provider_categories", x => x.identity_provider_category_id);
                });

            migrationBuilder.CreateTable(
                name: "invitation_statuses",
                schema: "portal",
                columns: table => new
                {
                    invitation_status_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invitation_statuses", x => x.invitation_status_id);
                });

            migrationBuilder.CreateTable(
                name: "languages",
                schema: "portal",
                columns: table => new
                {
                    short_name = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    long_name_de = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    long_name_en = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_languages", x => x.short_name);
                });

            migrationBuilder.CreateTable(
                name: "use_cases",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    shortname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_use_cases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "addresses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    city = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    region = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    streetadditional = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    streetname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    streetnumber = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    zipcode = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    country_alpha2code = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_addresses_countries_country_temp_id",
                        column: x => x.country_alpha2code,
                        principalSchema: "portal",
                        principalTable: "countries",
                        principalColumn: "alpha2code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    iam_client_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_roles_iam_clients_iam_client_id",
                        column: x => x.iam_client_id,
                        principalSchema: "portal",
                        principalTable: "iam_clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_providers",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    identity_provider_category_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_providers", x => x.id);
                    table.ForeignKey(
                        name: "fk_identity_providers_identity_provider_categories_identity_pr",
                        column: x => x.identity_provider_category_id,
                        principalSchema: "portal",
                        principalTable: "identity_provider_categories",
                        principalColumn: "identity_provider_category_id");
                });

            migrationBuilder.CreateTable(
                name: "company_role_descriptions",
                schema: "portal",
                columns: table => new
                {
                    company_role_id = table.Column<int>(type: "integer", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_role_descriptions", x => new { x.company_role_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_company_role_descriptions_company_roles_company_role_id",
                        column: x => x.company_role_id,
                        principalSchema: "portal",
                        principalTable: "company_roles",
                        principalColumn: "company_role_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_company_role_descriptions_languages_language_temp_id2",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    bpn = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    tax_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    parent = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    shortname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    company_status_id = table.Column<int>(type: "integer", nullable: false),
                    address_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companies", x => x.id);
                    table.ForeignKey(
                        name: "fk_companies_addresses_address_id",
                        column: x => x.address_id,
                        principalSchema: "portal",
                        principalTable: "addresses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_companies_company_statuses_company_status_id",
                        column: x => x.company_status_id,
                        principalSchema: "portal",
                        principalTable: "company_statuses",
                        principalColumn: "company_status_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role_descriptions",
                schema: "portal",
                columns: table => new
                {
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role_descriptions", x => new { x.user_role_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_user_role_descriptions_languages_language_short_name",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_role_descriptions_user_roles_user_role_id",
                        column: x => x.user_role_id,
                        principalSchema: "portal",
                        principalTable: "user_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "iam_identity_providers",
                schema: "portal",
                columns: table => new
                {
                    iam_idp_alias = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    identity_provider_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iam_identity_providers", x => x.iam_idp_alias);
                    table.ForeignKey(
                        name: "fk_iam_identity_providers_identity_providers_identity_provider",
                        column: x => x.identity_provider_id,
                        principalSchema: "portal",
                        principalTable: "identity_providers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "apps",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_released = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    thumbnail_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    app_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    marketing_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    contact_number = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    provider = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    provider_company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    app_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_apps", x => x.id);
                    table.ForeignKey(
                        name: "fk_apps_app_statuses_app_status_id",
                        column: x => x.app_status_id,
                        principalSchema: "portal",
                        principalTable: "app_statuses",
                        principalColumn: "app_status_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_apps_companies_provider_company_id",
                        column: x => x.provider_company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_applications",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    application_status_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_applications", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_applications_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_applications_company_application_statuses_applicati",
                        column: x => x.application_status_id,
                        principalSchema: "portal",
                        principalTable: "company_application_statuses",
                        principalColumn: "application_status_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "company_assigned_roles",
                schema: "portal",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_assigned_roles", x => new { x.company_id, x.company_role_id });
                    table.ForeignKey(
                        name: "fk_company_assigned_roles_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_assigned_roles_company_roles_company_role_id",
                        column: x => x.company_role_id,
                        principalSchema: "portal",
                        principalTable: "company_roles",
                        principalColumn: "company_role_id");
                });

            migrationBuilder.CreateTable(
                name: "company_assigned_use_cases",
                schema: "portal",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    use_case_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_assigned_use_cases", x => new { x.company_id, x.use_case_id });
                    table.ForeignKey(
                        name: "fk_company_assigned_use_cases_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_assigned_use_cases_use_cases_use_case_id",
                        column: x => x.use_case_id,
                        principalSchema: "portal",
                        principalTable: "use_cases",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_identity_providers",
                schema: "portal",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_provider_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_identity_providers", x => new { x.company_id, x.identity_provider_id });
                    table.ForeignKey(
                        name: "fk_company_identity_providers_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_identity_providers_identity_providers_identity_prov",
                        column: x => x.identity_provider_id,
                        principalSchema: "portal",
                        principalTable: "identity_providers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_users",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    firstname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    lastlogin = table.Column<byte[]>(type: "bytea", nullable: true),
                    lastname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_user_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_users_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_users_company_user_statuses_company_user_status_id",
                        column: x => x.company_user_status_id,
                        principalSchema: "portal",
                        principalTable: "company_user_statuses",
                        principalColumn: "company_user_status_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agreements",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agreement_category_id = table.Column<int>(type: "integer", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    agreement_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: true),
                    issuer_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    use_case_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreements", x => x.id);
                    table.ForeignKey(
                        name: "fk_agreements_agreement_categories_agreement_category_id",
                        column: x => x.agreement_category_id,
                        principalSchema: "portal",
                        principalTable: "agreement_categories",
                        principalColumn: "agreement_category_id");
                    table.ForeignKey(
                        name: "fk_agreements_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_agreements_companies_issuer_company_id",
                        column: x => x.issuer_company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_agreements_use_cases_use_case_id",
                        column: x => x.use_case_id,
                        principalSchema: "portal",
                        principalTable: "use_cases",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "app_assigned_clients",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    iam_client_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_assigned_clients", x => new { x.app_id, x.iam_client_id });
                    table.ForeignKey(
                        name: "fk_app_assigned_clients_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_app_assigned_clients_iam_clients_iam_client_id",
                        column: x => x.iam_client_id,
                        principalSchema: "portal",
                        principalTable: "iam_clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "app_assigned_licenses",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_license_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_assigned_licenses", x => new { x.app_id, x.app_license_id });
                    table.ForeignKey(
                        name: "fk_app_assigned_licenses_app_licenses_app_license_id",
                        column: x => x.app_license_id,
                        principalSchema: "portal",
                        principalTable: "app_licenses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_app_assigned_licenses_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "app_assigned_use_cases",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    use_case_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_assigned_use_cases", x => new { x.app_id, x.use_case_id });
                    table.ForeignKey(
                        name: "fk_app_assigned_use_cases_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_app_assigned_use_cases_use_cases_use_case_id",
                        column: x => x.use_case_id,
                        principalSchema: "portal",
                        principalTable: "use_cases",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "app_descriptions",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    description_long = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    description_short = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_descriptions", x => new { x.app_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_app_descriptions_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_app_descriptions_languages_language_temp_id",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name");
                });

            migrationBuilder.CreateTable(
                name: "app_detail_images",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_detail_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_app_detail_images_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "app_languages",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_languages", x => new { x.app_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_app_languages_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_app_languages_languages_language_temp_id1",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name");
                });

            migrationBuilder.CreateTable(
                name: "app_tags",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_tags", x => new { x.app_id, x.tag_name });
                    table.ForeignKey(
                        name: "fk_app_tags_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_assigned_apps",
                schema: "portal",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_assigned_apps", x => new { x.company_id, x.app_id });
                    table.ForeignKey(
                        name: "fk_company_assigned_apps_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_assigned_apps_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_user_assigned_app_favourites",
                schema: "portal",
                columns: table => new
                {
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_user_assigned_app_favourites", x => new { x.company_user_id, x.app_id });
                    table.ForeignKey(
                        name: "fk_company_user_assigned_app_favourites_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_user_assigned_app_favourites_company_users_company_",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_user_assigned_roles",
                schema: "portal",
                columns: table => new
                {
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_user_assigned_roles", x => new { x.company_user_id, x.user_role_id });
                    table.ForeignKey(
                        name: "fk_company_user_assigned_roles_company_users_company_user_id",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_user_assigned_roles_user_roles_user_role_id",
                        column: x => x.user_role_id,
                        principalSchema: "portal",
                        principalTable: "user_roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "documents",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    document = table.Column<uint>(type: "oid", nullable: false),
                    documenthash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    documentname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    document_type_id = table.Column<int>(type: "integer", nullable: true),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_documents_company_users_company_user_id",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_documents_document_types_document_type_id",
                        column: x => x.document_type_id,
                        principalSchema: "portal",
                        principalTable: "document_types",
                        principalColumn: "document_type_id");
                });

            migrationBuilder.CreateTable(
                name: "iam_users",
                schema: "portal",
                columns: table => new
                {
                    user_entity_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iam_users", x => x.user_entity_id);
                    table.ForeignKey(
                        name: "fk_iam_users_company_users_company_user_id",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "invitations",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    invitation_status_id = table.Column<int>(type: "integer", nullable: false),
                    company_application_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invitations", x => x.id);
                    table.ForeignKey(
                        name: "fk_invitations_company_applications_company_application_id",
                        column: x => x.company_application_id,
                        principalSchema: "portal",
                        principalTable: "company_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_invitations_company_users_company_user_id",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_invitations_invitation_statuses_invitation_status_id",
                        column: x => x.invitation_status_id,
                        principalSchema: "portal",
                        principalTable: "invitation_statuses",
                        principalColumn: "invitation_status_id");
                });

            migrationBuilder.CreateTable(
                name: "agreement_assigned_company_roles",
                schema: "portal",
                columns: table => new
                {
                    agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_assigned_company_roles", x => new { x.agreement_id, x.company_role_id });
                    table.ForeignKey(
                        name: "fk_agreement_assigned_company_roles_agreements_agreement_id",
                        column: x => x.agreement_id,
                        principalSchema: "portal",
                        principalTable: "agreements",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_agreement_assigned_company_roles_company_roles_company_role",
                        column: x => x.company_role_id,
                        principalSchema: "portal",
                        principalTable: "company_roles",
                        principalColumn: "company_role_id");
                });

            migrationBuilder.CreateTable(
                name: "agreement_assigned_document_templates",
                schema: "portal",
                columns: table => new
                {
                    agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_template_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_assigned_document_templates", x => new { x.agreement_id, x.document_template_id });
                    table.ForeignKey(
                        name: "fk_agreement_assigned_document_templates_agreements_agreement_",
                        column: x => x.agreement_id,
                        principalSchema: "portal",
                        principalTable: "agreements",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_agreement_assigned_document_templates_document_templates_do",
                        column: x => x.document_template_id,
                        principalSchema: "portal",
                        principalTable: "document_templates",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "consents",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    comment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    consent_status_id = table.Column<int>(type: "integer", nullable: false),
                    target = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consents", x => x.id);
                    table.ForeignKey(
                        name: "fk_consents_agreements_agreement_id",
                        column: x => x.agreement_id,
                        principalSchema: "portal",
                        principalTable: "agreements",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_consents_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_consents_company_users_company_user_id",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_consents_consent_statuses_consent_status_id",
                        column: x => x.consent_status_id,
                        principalSchema: "portal",
                        principalTable: "consent_statuses",
                        principalColumn: "consent_status_id");
                    table.ForeignKey(
                        name: "fk_consents_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "portal",
                        principalTable: "documents",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "agreement_categories",
                columns: new[] { "agreement_category_id", "label" },
                values: new object[,]
                {
                    { 1, "CX_FRAME_CONTRACT" },
                    { 2, "APP_CONTRACT" },
                    { 3, "DATA_CONTRACT" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "app_statuses",
                columns: new[] { "app_status_id", "label" },
                values: new object[,]
                {
                    { 1, "CREATED" },
                    { 2, "IN_REVIEW" },
                    { 3, "ACTIVE" },
                    { 4, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_application_statuses",
                columns: new[] { "application_status_id", "label" },
                values: new object[,]
                {
                    { 1, "CREATED" },
                    { 2, "ADD_COMPANY_DATA" },
                    { 3, "INVITE_USER" },
                    { 4, "SELECT_COMPANY_ROLE" },
                    { 5, "UPLOAD_DOCUMENTS" },
                    { 6, "VERIFY" },
                    { 7, "SUBMITTED" },
                    { 8, "CONFIRMED" },
                    { 9, "DECLINED" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_roles",
                columns: new[] { "company_role_id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE_PARTICIPANT" },
                    { 2, "APP_PROVIDER" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_statuses",
                columns: new[] { "company_status_id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "ACTIVE" },
                    { 3, "REJECTED" },
                    { 4, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_user_statuses",
                columns: new[] { "company_user_status_id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "consent_statuses",
                columns: new[] { "consent_status_id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "countries",
                columns: new[] { "alpha2code", "alpha3code", "country_name_de", "country_name_en" },
                values: new object[,]
                {
                    { "AD", "AND", "Andorra", "Andorra" },
                    { "AE", "ARE", "United Arab Emirates (the)", "United Arab Emirates (the)" },
                    { "AF", "AFG", "Afghanistan", "Afghanistan" },
                    { "AG", "ATG", "Antigua and Barbuda", "Antigua and Barbuda" },
                    { "AI", "AIA", "Anguilla", "Anguilla" },
                    { "AL", "ALB", "Albania", "Albania" },
                    { "AM", "ARM", "Armenia", "Armenia" },
                    { "AO", "AGO", "Angola", "Angola" },
                    { "AQ", "ATA", "Antarctica", "Antarctica" },
                    { "AR", "ARG", "Argentina", "Argentina" },
                    { "AS", "ASM", "American Samoa", "American Samoa" },
                    { "AT", "AUT", "Austria", "Austria" },
                    { "AU", "AUS", "Australia", "Australia" },
                    { "AW", "ABW", "Aruba", "Aruba" },
                    { "AX", "ALA", "Åland Islands", "Åland Islands" },
                    { "AZ", "AZE", "Azerbaijan", "Azerbaijan" },
                    { "BA", "BIH", "Bosnien and Herzegovenien", "Bosnia and Herzegovina" },
                    { "BB", "BRB", "Barbados", "Barbados" },
                    { "BD", "BGD", "Bangladesh", "Bangladesh" },
                    { "BE", "BEL", "Belgium", "Belgium" },
                    { "BF", "BFA", "Burkina Faso", "Burkina Faso" },
                    { "BG", "BGR", "Bulgarien", "Bulgaria" },
                    { "BH", "BHR", "Bahrain", "Bahrain" },
                    { "BI", "BDI", "Burundi", "Burundi" },
                    { "BJ", "BEN", "Benin", "Benin" },
                    { "BL", "BLM", "Saint Barthélemy", "Saint Barthélemy" },
                    { "BM", "BMU", "Bermuda", "Bermuda" },
                    { "BN", "BRN", "Brunei Darussalam", "Brunei Darussalam" },
                    { "BO", "BOL", "Bolivien", "Bolivia (Plurinational State of)" },
                    { "BQ", "BES", "Bonaire, Sint Eustatius and Saba", "Bonaire, Sint Eustatius and Saba" },
                    { "BR", "BRA", "Brasilien", "Brazil" },
                    { "BS", "BHS", "Bahamas", "Bahamas (the)" },
                    { "BT", "BTN", "Bhutan", "Bhutan" },
                    { "BV", "BVT", "Bouvet Island", "Bouvet Island" },
                    { "BW", "BWA", "Botswana", "Botswana" },
                    { "BY", "BLR", "Belarus", "Belarus" },
                    { "BZ", "BLZ", "Belize", "Belize" },
                    { "CA", "CAN", "Kanada", "Canada" },
                    { "CC", "CCK", "Cocos (Keeling) Islands (the)", "Cocos (Keeling) Islands (the)" },
                    { "CD", "COD", "Kongo", "Congo (the Democratic Republic of the)" },
                    { "CF", "CAF", "Central African Republic (the)", "Central African Republic (the)" },
                    { "CH", "CHE", "Switzerland", "Switzerland" },
                    { "CI", "CIV", "Côte d'Ivoire", "Côte d'Ivoire" },
                    { "CK", "COK", "Cook Islands", "Cook Islands (the)" },
                    { "CL", "CHL", "Chile", "Chile" },
                    { "CM", "CMR", "Cameroon", "Cameroon" },
                    { "CN", "CHN", "China", "China" },
                    { "CO", "COL", "Kolumbien", "Colombia" },
                    { "CR", "CRI", "Costa Rica", "Costa Rica" },
                    { "CU", "CUB", "Kuba", "Cuba" },
                    { "CV", "CPV", "Cabo Verde", "Cabo Verde" },
                    { "CW", "CUW", "Curaçao", "Curaçao" },
                    { "CX", "CXR", "Weihnachtsinseln", "Christmas Island" },
                    { "CY", "CYP", "Zypern", "Cyprus" },
                    { "CZ", "CZE", "Tschechien", "Czechia" },
                    { "DE", "DEU", "Deutschland", "Germany" },
                    { "DJ", "DJI", "Djibouti", "Djibouti" },
                    { "DK", "DNK", "Dänemark", "Denmark" },
                    { "DM", "DMA", "Dominica", "Dominica" },
                    { "DO", "DOM", "Dominikanische Republik", "Dominican Republic (the)" },
                    { "DZ", "DZA", "Algeria", "Algeria" },
                    { "EC", "ECU", "Ecuador", "Ecuador" },
                    { "EE", "EST", "Estonia", "Estonia" },
                    { "EG", "EGY", "Ägypten", "Egypt" },
                    { "EH", "ESH", "Western Sahara*", "Western Sahara*" },
                    { "ER", "ERI", "Eritrea", "Eritrea" },
                    { "ES", "ESP", "Spain", "Spain" },
                    { "ET", "ETH", "Ethiopia", "Ethiopia" },
                    { "FI", "FIN", "Finland", "Finland" },
                    { "FJ", "FJI", "Fiji", "Fiji" },
                    { "FK", "FLK", "Falkland Islands (the) [Malvinas]", "Falkland Islands (the) [Malvinas]" },
                    { "FM", "FSM", "Micronesia (Federated States of)", "Micronesia (Federated States of)" },
                    { "FO", "FRO", "Faroe Islands (the)", "Faroe Islands (the)" },
                    { "FR", "FRA", "Frankreich", "France" },
                    { "GA", "GAB", "Gabon", "Gabon" },
                    { "GB", "GBR", "United Kingdom of Great Britain and Northern Ireland (the)", "United Kingdom of Great Britain and Northern Ireland (the)" },
                    { "GD", "GRD", "Grenada", "Grenada" },
                    { "GE", "GEO", "Georgia", "Georgia" },
                    { "GF", "GUF", "French Guiana", "French Guiana" },
                    { "GG", "GGY", "Guernsey", "Guernsey" },
                    { "GH", "GHA", "Ghana", "Ghana" },
                    { "GI", "GIB", "Gibraltar", "Gibraltar" },
                    { "GL", "GRL", "Greenland", "Greenland" },
                    { "GM", "GMB", "Gambia (the)", "Gambia (the)" },
                    { "GN", "GIN", "Guinea", "Guinea" },
                    { "GP", "GLP", "Guadeloupe", "Guadeloupe" },
                    { "GQ", "GNQ", "Equatorial Guinea", "Equatorial Guinea" },
                    { "GR", "GRC", "Greece", "Greece" },
                    { "GS", "SGS", "South Georgia and the South Sandwich Islands", "South Georgia and the South Sandwich Islands" },
                    { "GT", "GTM", "Guatemala", "Guatemala" },
                    { "GU", "GUM", "Guam", "Guam" },
                    { "GW", "GNB", "Guinea-Bissau", "Guinea-Bissau" },
                    { "GY", "GUY", "Guyana", "Guyana" },
                    { "HK", "HKG", "Hong Kong", "Hong Kong" },
                    { "HM", "HMD", "Heard Island and McDonald Islands", "Heard Island and McDonald Islands" },
                    { "HN", "HND", "Honduras", "Honduras" },
                    { "HR", "HRV", "Kroatien", "Croatia" },
                    { "HT", "HTI", "Haiti", "Haiti" },
                    { "HU", "HUN", "Hungary", "Hungary" },
                    { "ID", "IDN", "Indonesia", "Indonesia" },
                    { "IE", "IRL", "Ireland", "Ireland" },
                    { "IL", "ISR", "Israel", "Israel" },
                    { "IM", "IMN", "Isle of Man", "Isle of Man" },
                    { "IN", "IND", "India", "India" },
                    { "IO", "IOT", "British Indian Ocean Territory", "British Indian Ocean Territory (the)" },
                    { "IQ", "IRQ", "Iraq", "Iraq" },
                    { "IR", "IRN", "Iran (Islamic Republic of)", "Iran (Islamic Republic of)" },
                    { "IS", "ISL", "Iceland", "Iceland" },
                    { "IT", "ITA", "Italy", "Italy" },
                    { "JE", "JEY", "Jersey", "Jersey" },
                    { "JM", "JAM", "Jamaica", "Jamaica" },
                    { "JO", "JOR", "Jordan", "Jordan" },
                    { "JP", "JPN", "Japan", "Japan" },
                    { "KE", "KEN", "Kenya", "Kenya" },
                    { "KG", "KGZ", "Kyrgyzstan", "Kyrgyzstan" },
                    { "KH", "KHM", "Cambodia", "Cambodia" },
                    { "KI", "KIR", "Kiribati", "Kiribati" },
                    { "KM", "COM", "Comoros", "Comoros (the)" },
                    { "KN", "KNA", "Saint Kitts and Nevis", "Saint Kitts and Nevis" },
                    { "KP", "PRK", "Korea (the Democratic People's Republic of)", "Korea (the Democratic People's Republic of)" },
                    { "KR", "KOR", "Korea (the Republic of)", "Korea (the Republic of)" },
                    { "KW", "KWT", "Kuwait", "Kuwait" },
                    { "KY", "CYM", "Cayman Islands (the)", "Cayman Islands (the)" },
                    { "KZ", "KAZ", "Kazakhstan", "Kazakhstan" },
                    { "LA", "LAO", "Lao People's Democratic Republic (the)", "Lao People's Democratic Republic (the)" },
                    { "LB", "LBN", "Lebanon", "Lebanon" },
                    { "LC", "LCA", "Saint Lucia", "Saint Lucia" },
                    { "LI", "LIE", "Liechtenstein", "Liechtenstein" },
                    { "LK", "LKA", "Sri Lanka", "Sri Lanka" },
                    { "LR", "LBR", "Liberia", "Liberia" },
                    { "LS", "LSO", "Lesotho", "Lesotho" },
                    { "LT", "LTU", "Lithuania", "Lithuania" },
                    { "LU", "LUX", "Luxembourg", "Luxembourg" },
                    { "LV", "LVA", "Latvia", "Latvia" },
                    { "LY", "LBY", "Libya", "Libya" },
                    { "MA", "MAR", "Morocco", "Morocco" },
                    { "MC", "MCO", "Monaco", "Monaco" },
                    { "MD", "MDA", "Moldova (the Republic of)", "Moldova (the Republic of)" },
                    { "ME", "MNE", "Montenegro", "Montenegro" },
                    { "MF", "MAF", "Saint Martin (French part)", "Saint Martin (French part)" },
                    { "MG", "MDG", "Madagascar", "Madagascar" },
                    { "MH", "MHL", "Marshall Islands (the)", "Marshall Islands (the)" },
                    { "MK", "MKD", "North Macedonia", "North Macedonia" },
                    { "ML", "MLI", "Mali", "Mali" },
                    { "MM", "MMR", "Myanmar", "Myanmar" },
                    { "MN", "MNG", "Mongolia", "Mongolia" },
                    { "MO", "MAC", "Macao", "Macao" },
                    { "MP", "MNP", "Northern Mariana Islands (the)", "Northern Mariana Islands (the)" },
                    { "MQ", "MTQ", "Martinique", "Martinique" },
                    { "MR", "MRT", "Mauritania", "Mauritania" },
                    { "MS", "MSR", "Montserrat", "Montserrat" },
                    { "MT", "MLT", "Malta", "Malta" },
                    { "MU", "MUS", "Mauritius", "Mauritius" },
                    { "MV", "MDV", "Maldives", "Maldives" },
                    { "MW", "MWI", "Malawi", "Malawi" },
                    { "MX", "MEX", "Mexico", "Mexico" },
                    { "MY", "MYS", "Malaysia", "Malaysia" },
                    { "MZ", "MOZ", "Mozambique", "Mozambique" },
                    { "NA", "NAM", "Namibia", "Namibia" },
                    { "NC", "NCL", "New Caledonia", "New Caledonia" },
                    { "NE", "NER", "Niger (the)", "Niger (the)" },
                    { "NF", "NFK", "Norfolk Island", "Norfolk Island" },
                    { "NG", "NGA", "Nigeria", "Nigeria" },
                    { "NI", "NIC", "Nicaragua", "Nicaragua" },
                    { "NL", "NLD", "Netherlands (the)", "Netherlands (the)" },
                    { "NO", "NOR", "Norway", "Norway" },
                    { "NP", "NPL", "Nepal", "Nepal" },
                    { "NR", "NRU", "Nauru", "Nauru" },
                    { "NU", "NIU", "Niue", "Niue" },
                    { "NZ", "NZL", "New Zealand", "New Zealand" },
                    { "OM", "OMN", "Oman", "Oman" },
                    { "PA", "PAN", "Panama", "Panama" },
                    { "PE", "PER", "Peru", "Peru" },
                    { "PF", "PYF", "French Polynesia", "French Polynesia" },
                    { "PG", "PNG", "Papua New Guinea", "Papua New Guinea" },
                    { "PH", "PHL", "Philippines (the)", "Philippines (the)" },
                    { "PK", "PAK", "Pakistan", "Pakistan" },
                    { "PL", "POL", "Poland", "Poland" },
                    { "PM", "SPM", "Saint Pierre and Miquelon", "Saint Pierre and Miquelon" },
                    { "PN", "PCN", "Pitcairn", "Pitcairn" },
                    { "PR", "PRI", "Puerto Rico", "Puerto Rico" },
                    { "PS", "PSE", "Palestine, State of", "Palestine, State of" },
                    { "PT", "PRT", "Portugal", "Portugal" },
                    { "PW", "PLW", "Palau", "Palau" },
                    { "PY", "PRY", "Paraguay", "Paraguay" },
                    { "QA", "QAT", "Qatar", "Qatar" },
                    { "RE", "REU", "Réunion", "Réunion" },
                    { "RO", "ROU", "Romania", "Romania" },
                    { "RS", "SRB", "Serbia", "Serbia" },
                    { "RU", "RUS", "Russian Federation (the)", "Russian Federation (the)" },
                    { "RW", "RWA", "Rwanda", "Rwanda" },
                    { "SA", "SAU", "Saudi Arabia", "Saudi Arabia" },
                    { "SB", "SLB", "Solomon Islands", "Solomon Islands" },
                    { "SC", "SYC", "Seychelles", "Seychelles" },
                    { "SD", "SDN", "Sudan (the)", "Sudan (the)" },
                    { "SE", "SWE", "Sweden", "Sweden" },
                    { "SG", "SGP", "Singapore", "Singapore" },
                    { "SH", "SHN", "Saint Helena, Ascension and Tristan da Cunha", "Saint Helena, Ascension and Tristan da Cunha" },
                    { "SI", "SVN", "Slovenia", "Slovenia" },
                    { "SJ", "SJM", "Svalbard and Jan Mayen", "Svalbard and Jan Mayen" },
                    { "SK", "SVK", "Slovakia", "Slovakia" },
                    { "SL", "SLE", "Sierra Leone", "Sierra Leone" },
                    { "SM", "SMR", "San Marino", "San Marino" },
                    { "SN", "SEN", "Senegal", "Senegal" },
                    { "SO", "SOM", "Somalia", "Somalia" },
                    { "SR", "SUR", "Suriname", "Suriname" },
                    { "SS", "SSD", "South Sudan", "South Sudan" },
                    { "ST", "STP", "Sao Tome and Principe", "Sao Tome and Principe" },
                    { "SV", "SLV", "El Salvador", "El Salvador" },
                    { "SX", "SXM", "Sint Maarten (Dutch part)", "Sint Maarten (Dutch part)" },
                    { "SY", "SYR", "Syrian Arab Republic (the)", "Syrian Arab Republic (the)" },
                    { "SZ", "SWZ", "Eswatini", "Eswatini" },
                    { "TC", "TCA", "Turks and Caicos Islands (the)", "Turks and Caicos Islands (the)" },
                    { "TD", "TCD", "Chad", "Chad" },
                    { "TF", "ATF", "French Southern Territories (the)", "French Southern Territories (the)" },
                    { "TG", "TGO", "Togo", "Togo" },
                    { "TH", "THA", "Thailand", "Thailand" },
                    { "TJ", "TJK", "Tajikistan", "Tajikistan" },
                    { "TK", "TKL", "Tokelau", "Tokelau" },
                    { "TL", "TLS", "Timor-Leste", "Timor-Leste" },
                    { "TM", "TKM", "Turkmenistan", "Turkmenistan" },
                    { "TN", "TUN", "Tunisia", "Tunisia" },
                    { "TO", "TON", "Tonga", "Tonga" },
                    { "TR", "TUR", "Turkey", "Turkey" },
                    { "TT", "TTO", "Trinidad and Tobago", "Trinidad and Tobago" },
                    { "TV", "TUV", "Tuvalu", "Tuvalu" },
                    { "TW", "TWN", "Taiwan (Province of China)", "Taiwan (Province of China)" },
                    { "TZ", "TZA", "Tanzania, the United Republic of", "Tanzania, the United Republic of" },
                    { "UA", "UKR", "Ukraine", "Ukraine" },
                    { "UG", "UGA", "Uganda", "Uganda" },
                    { "UM", "UMI", "United States Minor Outlying Islands (the)", "United States Minor Outlying Islands (the)" },
                    { "US", "USA", "United States of America (the)", "United States of America (the)" },
                    { "UY", "URY", "Uruguay", "Uruguay" },
                    { "UZ", "UZB", "Uzbekistan", "Uzbekistan" },
                    { "VA", "VAT", "Holy See (the)", "Holy See (the)" },
                    { "VC", "VCT", "Saint Vincent and the Grenadines", "Saint Vincent and the Grenadines" },
                    { "VE", "VEN", "Venezuela (Bolivarian Republic of)", "Venezuela (Bolivarian Republic of)" },
                    { "VG", "VGB", "Virgin Islands (British)", "Virgin Islands (British)" },
                    { "VI", "VIR", "Virgin Islands (U.S.)", "Virgin Islands (U.S.)" },
                    { "VN", "VNM", "Viet Nam", "Viet Nam" },
                    { "VU", "VUT", "Vanuatu", "Vanuatu" },
                    { "WF", "WLF", "Wallis and Futuna", "Wallis and Futuna" },
                    { "WS", "WSM", "Samoa", "Samoa" },
                    { "YE", "YEM", "Yemen", "Yemen" },
                    { "YT", "MYT", "Mayotte", "Mayotte" },
                    { "ZA", "ZAF", "South Africa", "South Africa" },
                    { "ZM", "ZMB", "Zambia", "Zambia" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "document_types",
                columns: new[] { "document_type_id", "label" },
                values: new object[,]
                {
                    { 1, "CX_FRAME_CONTRACT" },
                    { 2, "COMMERCIAL_REGISTER_EXTRACT" },
                    { 3, "APP_CONTRACT" },
                    { 4, "DATA_CONTRACT" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "identity_provider_categories",
                columns: new[] { "identity_provider_category_id", "label" },
                values: new object[,]
                {
                    { 1, "KEYCLOAK_SHARED" },
                    { 2, "KEYCLOAK_OIDC" },
                    { 3, "KEYCLOAK_SAML" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "invitation_statuses",
                columns: new[] { "invitation_status_id", "label" },
                values: new object[,]
                {
                    { 1, "CREATED" },
                    { 2, "PENDING" },
                    { 3, "ACCEPTED" },
                    { 4, "DECLINED" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "languages",
                columns: new[] { "short_name", "long_name_de", "long_name_en" },
                values: new object[,]
                {
                    { "de", "deutsch", "german" },
                    { "en", "englisch", "english" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "use_cases",
                columns: new[] { "id", "name", "shortname" },
                values: new object[,]
                {
                    { new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b86"), "Traceability", "T" },
                    { new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b87"), "Sustainability & CO2-Footprint", "CO2" },
                    { new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b88"), "Manufacturing as a Service", "MaaS" },
                    { new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b89"), "Real-Time Control", "RTC" },
                    { new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b90"), "Modular Production", "MP" },
                    { new Guid("1aacde78-35ec-4df3-ba1e-f988cddcbbd8"), "Circular Economy", "CE" },
                    { new Guid("1aacde78-35ec-4df3-ba1e-f988cddcbbd9"), "None", "None" },
                    { new Guid("41e4a4c0-aae4-41c0-97c9-ebafde410de4"), "Demand and Capacity Management", "DCM" },
                    { new Guid("6909ccc7-37c8-4088-99ab-790f20702460"), "Business Partner Management", "BPDM" },
                    { new Guid("c065a349-f649-47f8-94d5-1a504a855419"), "Quality Management", "QM" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_role_descriptions",
                columns: new[] { "company_role_id", "language_short_name", "description" },
                values: new object[,]
                {
                    { 1, "de", "Netzwerkteilnehmer" },
                    { 1, "en", "Participant" },
                    { 2, "de", "Softwareanbieter" },
                    { 2, "en", "Application Provider" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_addresses_country_alpha2code",
                schema: "portal",
                table: "addresses",
                column: "country_alpha2code");

            migrationBuilder.CreateIndex(
                name: "ix_agreement_assigned_company_roles_company_role_id",
                schema: "portal",
                table: "agreement_assigned_company_roles",
                column: "company_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_agreement_assigned_document_templates_document_template_id",
                schema: "portal",
                table: "agreement_assigned_document_templates",
                column: "document_template_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agreements_agreement_category_id",
                schema: "portal",
                table: "agreements",
                column: "agreement_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_agreements_app_id",
                schema: "portal",
                table: "agreements",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "ix_agreements_issuer_company_id",
                schema: "portal",
                table: "agreements",
                column: "issuer_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_agreements_use_case_id",
                schema: "portal",
                table: "agreements",
                column: "use_case_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_assigned_clients_iam_client_id",
                schema: "portal",
                table: "app_assigned_clients",
                column: "iam_client_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_assigned_licenses_app_license_id",
                schema: "portal",
                table: "app_assigned_licenses",
                column: "app_license_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_assigned_use_cases_use_case_id",
                schema: "portal",
                table: "app_assigned_use_cases",
                column: "use_case_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_descriptions_language_short_name",
                schema: "portal",
                table: "app_descriptions",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_app_detail_images_app_id",
                schema: "portal",
                table: "app_detail_images",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_languages_language_short_name",
                schema: "portal",
                table: "app_languages",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_apps_app_status_id",
                schema: "portal",
                table: "apps",
                column: "app_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_apps_provider_company_id",
                schema: "portal",
                table: "apps",
                column: "provider_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_companies_address_id",
                schema: "portal",
                table: "companies",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "ix_companies_company_status_id",
                schema: "portal",
                table: "companies",
                column: "company_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_applications_application_status_id",
                schema: "portal",
                table: "company_applications",
                column: "application_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_applications_company_id",
                schema: "portal",
                table: "company_applications",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_apps_app_id",
                schema: "portal",
                table: "company_assigned_apps",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_roles_company_role_id",
                schema: "portal",
                table: "company_assigned_roles",
                column: "company_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_use_cases_use_case_id",
                schema: "portal",
                table: "company_assigned_use_cases",
                column: "use_case_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_identity_providers_identity_provider_id",
                schema: "portal",
                table: "company_identity_providers",
                column: "identity_provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_role_descriptions_language_short_name",
                schema: "portal",
                table: "company_role_descriptions",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_company_user_assigned_app_favourites_app_id",
                schema: "portal",
                table: "company_user_assigned_app_favourites",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_user_assigned_roles_user_role_id",
                schema: "portal",
                table: "company_user_assigned_roles",
                column: "user_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_users_company_id",
                schema: "portal",
                table: "company_users",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_users_company_user_status_id",
                schema: "portal",
                table: "company_users",
                column: "company_user_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_consents_agreement_id",
                schema: "portal",
                table: "consents",
                column: "agreement_id");

            migrationBuilder.CreateIndex(
                name: "ix_consents_company_id",
                schema: "portal",
                table: "consents",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_consents_company_user_id",
                schema: "portal",
                table: "consents",
                column: "company_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_consents_consent_status_id",
                schema: "portal",
                table: "consents",
                column: "consent_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_consents_document_id",
                schema: "portal",
                table: "consents",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_company_user_id",
                schema: "portal",
                table: "documents",
                column: "company_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_document_type_id",
                schema: "portal",
                table: "documents",
                column: "document_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_iam_clients_client_client_id",
                schema: "portal",
                table: "iam_clients",
                column: "client_client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_iam_identity_providers_identity_provider_id",
                schema: "portal",
                table: "iam_identity_providers",
                column: "identity_provider_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_iam_users_company_user_id",
                schema: "portal",
                table: "iam_users",
                column: "company_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identity_providers_identity_provider_category_id",
                schema: "portal",
                table: "identity_providers",
                column: "identity_provider_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_company_application_id",
                schema: "portal",
                table: "invitations",
                column: "company_application_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_company_user_id",
                schema: "portal",
                table: "invitations",
                column: "company_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_invitation_status_id",
                schema: "portal",
                table: "invitations",
                column: "invitation_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_descriptions_language_short_name",
                schema: "portal",
                table: "user_role_descriptions",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_iam_client_id",
                schema: "portal",
                table: "user_roles",
                column: "iam_client_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agreement_assigned_company_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "agreement_assigned_document_templates",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_assigned_clients",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_assigned_licenses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_assigned_use_cases",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_detail_images",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_languages",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_tags",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_assigned_apps",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_assigned_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_assigned_use_cases",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_identity_providers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_role_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_user_assigned_app_favourites",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_user_assigned_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "consents",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "iam_identity_providers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "iam_users",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "invitations",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "user_role_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "document_templates",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_licenses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "agreements",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "consent_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identity_providers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_applications",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "invitation_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "languages",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "agreement_categories",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "apps",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "use_cases",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_users",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "document_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identity_provider_categories",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_application_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "iam_clients",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "companies",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_user_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "addresses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "countries",
                schema: "portal");
        }
    }
}
