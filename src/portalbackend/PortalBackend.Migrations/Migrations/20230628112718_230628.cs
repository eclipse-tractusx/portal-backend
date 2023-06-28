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

using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class _230628 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION() CASCADE;");

            migrationBuilder.AddColumn<string>(
                name: "client_client_id",
                schema: "portal",
                table: "company_service_accounts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "client_id",
                schema: "portal",
                table: "company_service_accounts",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_company_user20230523",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    firstname = table.Column<string>(type: "text", nullable: true),
                    lastlogin = table.Column<byte[]>(type: "bytea", nullable: true),
                    lastname = table.Column<string>(type: "text", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_user20230523", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "audit_identity_assigned_role20230522",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_identity_assigned_role20230522", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "audit_identity20230526",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_status_id = table.Column<int>(type: "integer", nullable: false),
                    user_entity_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    identity_type_id = table.Column<int>(type: "integer", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_identity20230526", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "identity_type",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "identities",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_status_id = table.Column<int>(type: "integer", nullable: false),
                    user_entity_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    identity_type_id = table.Column<int>(type: "integer", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identities", x => x.id);
                    table.ForeignKey(
                        name: "fk_identities_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_identities_identity_type_identity_type_id",
                        column: x => x.identity_type_id,
                        principalSchema: "portal",
                        principalTable: "identity_type",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_identities_identity_user_statuses_identity_status_id",
                        column: x => x.user_status_id,
                        principalSchema: "portal",
                        principalTable: "identity_user_statuses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "identity_assigned_roles",
                schema: "portal",
                columns: table => new
                {
                    identity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_assigned_roles", x => new { x.identity_id, x.user_role_id });
                    table.ForeignKey(
                        name: "fk_identity_assigned_roles_identities_identity_id",
                        column: x => x.identity_id,
                        principalSchema: "portal",
                        principalTable: "identities",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_identity_assigned_roles_user_roles_user_role_id",
                        column: x => x.user_role_id,
                        principalSchema: "portal",
                        principalTable: "user_roles",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "identity_type",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "COMPANY_USER" },
                    { 2, "COMPANY_SERVICE_ACCOUNT" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "identity_user_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" },
                    { 3, "DELETED" }
                });

            migrationBuilder.Sql("INSERT INTO portal.identities (id, date_created, company_id, user_status_id, user_entity_id, identity_type_id) SELECT cu.id, cu.date_created, cu.company_id, cu.company_user_status_id, i.user_entity_id, 1 FROM portal.company_users as cu LEFT JOIN portal.iam_users as i ON cu.id = i.company_user_id WHERE NOT EXISTS (SELECT 1 FROM portal.identities AS id WHERE id.id = cu.id);");
            migrationBuilder.Sql("INSERT INTO portal.identities (id, date_created, company_id, user_status_id, user_entity_id, identity_type_id) SELECT cu.id, cu.date_created, cu.service_account_owner_id, cu.company_service_account_status_id, i.user_entity_id, 2 FROM portal.company_service_accounts as cu LEFT JOIN portal.iam_service_accounts as i ON cu.id = i.company_service_account_id WHERE NOT EXISTS (SELECT 1 FROM portal.identities AS id WHERE id.id = cu.id);");
            migrationBuilder.Sql("UPDATE portal.company_service_accounts as sa SET client_id = i.client_id, client_client_id = i.client_client_id FROM ( SELECT company_service_account_id, client_id, client_client_id FROM portal.iam_service_accounts) AS i WHERE i.company_service_account_id = sa.id");
            migrationBuilder.Sql("INSERT INTO portal.identity_assigned_roles (identity_id, user_role_id, last_editor_id) SELECT company_user_id, user_role_id, last_editor_id FROM portal.company_user_assigned_roles;");
            migrationBuilder.Sql("INSERT INTO portal.identity_assigned_roles (identity_id, user_role_id, last_editor_id) SELECT company_service_account_id, user_role_id, null FROM portal.company_service_accounts as sa JOIN portal.company_service_account_assigned_roles as sar ON sa.id = sar.company_service_account_id;");

            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_companies_service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_company_service_account_statuses_c",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_company_users_companies_company_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropForeignKey(
                name: "fk_company_users_company_user_statuses_company_user_status_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropTable(
                name: "company_service_account_assigned_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_service_account_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_user_assigned_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_user_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "iam_service_accounts",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "iam_users",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_company_users_company_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropIndex(
                name: "ix_company_users_company_user_status_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_company_service_account_status_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "company_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropColumn(
                name: "company_user_status_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropColumn(
                name: "date_created",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropColumn(
                name: "company_service_account_status_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "date_created",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "client_client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identities_company_id",
                schema: "portal",
                table: "identities",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_identities_identity_type_id",
                schema: "portal",
                table: "identities",
                column: "identity_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_identities_user_entity_id",
                schema: "portal",
                table: "identities",
                column: "user_entity_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identities_user_status_id",
                schema: "portal",
                table: "identities",
                column: "user_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_assigned_roles_user_role_id",
                schema: "portal",
                table: "identity_assigned_roles",
                column: "user_role_id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_accounts_identities_identity_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_users_identities_identity_id",
                schema: "portal",
                table: "company_users",
                column: "id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "date_last_changed",
                schema: "portal",
                table: "provider_company_details",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "last_editor_id",
                schema: "portal",
                table: "provider_company_details",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_provider_company_detail20230614",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    auto_setup_url = table.Column<string>(type: "text", nullable: false),
                    auto_setup_callback_url = table.Column<string>(type: "text", nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_provider_company_detail20230614", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "language_long_names",
                schema: "portal",
                columns: table => new
                {
                    short_name = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    long_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_language_long_names", x => new { x.short_name, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_language_long_names_languages_language_short_name",
                        column: x => x.short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name");
                    table.ForeignKey(
                        name: "fk_language_long_names_languages_long_name_language_short_name",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name");
                });

            migrationBuilder.CreateIndex(
                name: "ix_language_long_names_language_short_name",
                schema: "portal",
                table: "language_long_names",
                column: "language_short_name");

            migrationBuilder.Sql("INSERT INTO portal.language_long_names (short_name,long_name, language_short_name) SELECT short_name, long_name_de, 'de' FROM portal.languages");
            migrationBuilder.Sql("INSERT INTO portal.language_long_names (short_name,long_name, language_short_name) SELECT short_name, long_name_en, 'en' FROM portal.languages");

            migrationBuilder.DropColumn(
                name: "long_name_de",
                schema: "portal",
                table: "languages");

            migrationBuilder.DropColumn(
                name: "long_name_en",
                schema: "portal",
                table: "languages");

            migrationBuilder.CreateTable(
                name: "audit_company_ssi_detail20230621",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    verified_credential_type_id = table.Column<int>(type: "integer", nullable: false),
                    company_ssi_detail_status_id = table.Column<int>(type: "integer", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creator_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expiry_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    verified_credential_external_type_use_case_detail_id = table.Column<Guid>(type: "uuid", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_ssi_detail20230621", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "company_ssi_detail_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_ssi_detail_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "use_case_descriptions",
                schema: "portal",
                columns: table => new
                {
                    use_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_use_case_descriptions", x => new { x.use_case_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_use_case_descriptions_languages_language_short_name",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name");
                    table.ForeignKey(
                        name: "fk_use_case_descriptions_use_cases_use_case_id",
                        column: x => x.use_case_id,
                        principalSchema: "portal",
                        principalTable: "use_cases",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_external_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_external_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_type_kinds",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_type_kinds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_external_type_use_case_detail_versions",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    verified_credential_external_type_id = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    template = table.Column<string>(type: "text", nullable: false),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expiry = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_external_type_use_case_detail_versions", x => x.id);
                    table.ForeignKey(
                        name: "fk_verified_credential_external_type_use_case_detail_versions_",
                        column: x => x.verified_credential_external_type_id,
                        principalSchema: "portal",
                        principalTable: "verified_credential_external_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_type_assigned_external_types",
                schema: "portal",
                columns: table => new
                {
                    verified_credential_type_id = table.Column<int>(type: "integer", nullable: false),
                    verified_credential_external_type_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_type_assigned_external_types", x => new { x.verified_credential_type_id, x.verified_credential_external_type_id });
                    table.ForeignKey(
                        name: "fk_verified_credential_type_assigned_external_types_verified_c",
                        column: x => x.verified_credential_external_type_id,
                        principalSchema: "portal",
                        principalTable: "verified_credential_external_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_verified_credential_type_assigned_external_types_verified_c1",
                        column: x => x.verified_credential_type_id,
                        principalSchema: "portal",
                        principalTable: "verified_credential_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_type_assigned_kinds",
                schema: "portal",
                columns: table => new
                {
                    verified_credential_type_id = table.Column<int>(type: "integer", nullable: false),
                    verified_credential_type_kind_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_type_assigned_kinds", x => new { x.verified_credential_type_id, x.verified_credential_type_kind_id });
                    table.ForeignKey(
                        name: "fk_verified_credential_type_assigned_kinds_verified_credential",
                        column: x => x.verified_credential_type_id,
                        principalSchema: "portal",
                        principalTable: "verified_credential_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_verified_credential_type_assigned_kinds_verified_credential1",
                        column: x => x.verified_credential_type_kind_id,
                        principalSchema: "portal",
                        principalTable: "verified_credential_type_kinds",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_type_assigned_use_cases",
                schema: "portal",
                columns: table => new
                {
                    verified_credential_type_id = table.Column<int>(type: "integer", nullable: false),
                    use_case_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_type_assigned_use_cases", x => new { x.verified_credential_type_id, x.use_case_id });
                    table.ForeignKey(
                        name: "fk_verified_credential_type_assigned_use_cases_use_cases_use_c",
                        column: x => x.use_case_id,
                        principalSchema: "portal",
                        principalTable: "use_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_verified_credential_type_assigned_use_cases_verified_creden",
                        column: x => x.verified_credential_type_id,
                        principalSchema: "portal",
                        principalTable: "verified_credential_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "company_ssi_details",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    verified_credential_type_id = table.Column<int>(type: "integer", nullable: false),
                    company_ssi_detail_status_id = table.Column<int>(type: "integer", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creator_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expiry_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    verified_credential_external_type_use_case_detail_id = table.Column<Guid>(type: "uuid", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_ssi_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_ssi_details_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_ssi_details_company_ssi_detail_statuses_company_ssi",
                        column: x => x.company_ssi_detail_status_id,
                        principalSchema: "portal",
                        principalTable: "company_ssi_detail_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_ssi_details_company_users_creator_user_id",
                        column: x => x.creator_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_ssi_details_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "portal",
                        principalTable: "documents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_ssi_details_verified_credential_external_type_use_c",
                        column: x => x.verified_credential_external_type_use_case_detail_id,
                        principalSchema: "portal",
                        principalTable: "verified_credential_external_type_use_case_detail_versions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_ssi_details_verified_credential_types_verified_cred",
                        column: x => x.verified_credential_type_id,
                        principalSchema: "portal",
                        principalTable: "verified_credential_types",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_ssi_detail_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "ACTIVE" },
                    { 3, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "document_types",
                columns: new[] { "id", "label" },
                values: new object[] { 14, "PRESENTATION" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_type",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 21, "ROLE_UPDATE_CORE_OFFER" },
                    { 22, "ROLE_UPDATE_APP_OFFER" },
                    { 23, "SUBSCRIPTION_URL_UPDATE" },
                    { 24, "CREDENTIAL_APPROVAL" },
                    { 25, "CREDENTIAL_REJECTED" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "verified_credential_external_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "TRACEABILITY_CREDENTIAL" },
                    { 2, "PCF_CREDENTIAL" },
                    { 3, "BEHAVIOR_TWIN_CREDENTIAL" },
                    { 4, "VEHICLE_DISMANTLE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "verified_credential_type_kinds",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "USE_CASE" },
                    { 2, "CERTIFICATE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "verified_credential_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "TRACEABILITY_FRAMEWORK" },
                    { 2, "PCF_FRAMEWORK" },
                    { 3, "BEHAVIOR_TWIN_FRAMEWORK" },
                    { 4, "DISMANTLER_CERTIFICATE" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_company_id",
                schema: "portal",
                table: "company_ssi_details",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_company_ssi_detail_status_id",
                schema: "portal",
                table: "company_ssi_details",
                column: "company_ssi_detail_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_creator_user_id",
                schema: "portal",
                table: "company_ssi_details",
                column: "creator_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_document_id",
                schema: "portal",
                table: "company_ssi_details",
                column: "document_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_verified_credential_external_type_use_c",
                schema: "portal",
                table: "company_ssi_details",
                column: "verified_credential_external_type_use_case_detail_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_verified_credential_type_id",
                schema: "portal",
                table: "company_ssi_details",
                column: "verified_credential_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_use_case_descriptions_language_short_name",
                schema: "portal",
                table: "use_case_descriptions",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_external_type_use_case_detail_versions_",
                schema: "portal",
                table: "verified_credential_external_type_use_case_detail_versions",
                columns: new[] { "verified_credential_external_type_id", "version" });

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_type_assigned_external_types_verified_c",
                schema: "portal",
                table: "verified_credential_type_assigned_external_types",
                column: "verified_credential_external_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_type_assigned_external_types_verified_c1",
                schema: "portal",
                table: "verified_credential_type_assigned_external_types",
                column: "verified_credential_type_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_type_assigned_kinds_verified_credential",
                schema: "portal",
                table: "verified_credential_type_assigned_kinds",
                column: "verified_credential_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_type_assigned_kinds_verified_credential1",
                schema: "portal",
                table: "verified_credential_type_assigned_kinds",
                column: "verified_credential_type_kind_id");

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_type_assigned_use_cases_use_case_id",
                schema: "portal",
                table: "verified_credential_type_assigned_use_cases",
                column: "use_case_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_type_assigned_use_cases_verified_creden",
                schema: "portal",
                table: "verified_credential_type_assigned_use_cases",
                column: "verified_credential_type_id",
                unique: true);

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_IDENTITY() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_IDENTITY$\r\nBEGIN\r\n  INSERT INTO portal.audit_identity20230526 (\"id\", \"date_created\", \"company_id\", \"user_status_id\", \"user_entity_id\", \"identity_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.company_id, \r\n  OLD.user_status_id, \r\n  OLD.user_entity_id, \r\n  OLD.identity_type_id, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_IDENTITY$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_IDENTITY AFTER DELETE\r\nON portal.identities\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_IDENTITY();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_IDENTITY() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_IDENTITY$\r\nBEGIN\r\n  INSERT INTO portal.audit_identity20230526 (\"id\", \"date_created\", \"company_id\", \"user_status_id\", \"user_entity_id\", \"identity_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.company_id, \r\n  NEW.user_status_id, \r\n  NEW.user_entity_id, \r\n  NEW.identity_type_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_IDENTITY$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_IDENTITY AFTER INSERT\r\nON portal.identities\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_IDENTITY();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_IDENTITY() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_IDENTITY$\r\nBEGIN\r\n  INSERT INTO portal.audit_identity20230526 (\"id\", \"date_created\", \"company_id\", \"user_status_id\", \"user_entity_id\", \"identity_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.company_id, \r\n  NEW.user_status_id, \r\n  NEW.user_entity_id, \r\n  NEW.identity_type_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_IDENTITY$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_IDENTITY AFTER UPDATE\r\nON portal.identities\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_IDENTITY();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_identity_assigned_role20230522 (\"identity_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.identity_id, \r\n  OLD.user_role_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE AFTER DELETE\r\nON portal.identity_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_identity_assigned_role20230522 (\"identity_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.identity_id, \r\n  NEW.user_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE AFTER INSERT\r\nON portal.identity_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_identity_assigned_role20230522 (\"identity_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.identity_id, \r\n  NEW.user_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE AFTER UPDATE\r\nON portal.identity_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20230523 (\"id\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.email, \r\n  OLD.firstname, \r\n  OLD.lastlogin, \r\n  OLD.lastname, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYUSER AFTER DELETE\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20230523 (\"id\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.email, \r\n  NEW.firstname, \r\n  NEW.lastlogin, \r\n  NEW.lastname, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYUSER AFTER INSERT\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20230523 (\"id\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.email, \r\n  NEW.firstname, \r\n  NEW.lastlogin, \r\n  NEW.lastname, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYUSER AFTER UPDATE\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_provider_company_detail20230614 (\"id\", \"date_created\", \"auto_setup_url\", \"auto_setup_callback_url\", \"company_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.auto_setup_url, \r\n  OLD.auto_setup_callback_url, \r\n  OLD.company_id, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL AFTER DELETE\r\nON portal.provider_company_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_provider_company_detail20230614 (\"id\", \"date_created\", \"auto_setup_url\", \"auto_setup_callback_url\", \"company_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.auto_setup_url, \r\n  NEW.auto_setup_callback_url, \r\n  NEW.company_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL AFTER INSERT\r\nON portal.provider_company_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_provider_company_detail20230614 (\"id\", \"date_created\", \"auto_setup_url\", \"auto_setup_callback_url\", \"company_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.auto_setup_url, \r\n  NEW.auto_setup_callback_url, \r\n  NEW.company_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL AFTER UPDATE\r\nON portal.provider_company_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYSSIDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_ssi_detail20230621 (\"id\", \"company_id\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"document_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_use_case_detail_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.company_id, \r\n  OLD.verified_credential_type_id, \r\n  OLD.company_ssi_detail_status_id, \r\n  OLD.document_id, \r\n  OLD.date_created, \r\n  OLD.creator_user_id, \r\n  OLD.expiry_date, \r\n  OLD.verified_credential_external_type_use_case_detail_id, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYSSIDETAIL AFTER DELETE\r\nON portal.company_ssi_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYSSIDETAIL();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_ssi_detail20230621 (\"id\", \"company_id\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"document_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_use_case_detail_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.company_id, \r\n  NEW.verified_credential_type_id, \r\n  NEW.company_ssi_detail_status_id, \r\n  NEW.document_id, \r\n  NEW.date_created, \r\n  NEW.creator_user_id, \r\n  NEW.expiry_date, \r\n  NEW.verified_credential_external_type_use_case_detail_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL AFTER INSERT\r\nON portal.company_ssi_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_ssi_detail20230621 (\"id\", \"company_id\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"document_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_use_case_detail_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.company_id, \r\n  NEW.verified_credential_type_id, \r\n  NEW.company_ssi_detail_status_id, \r\n  NEW.document_id, \r\n  NEW.date_created, \r\n  NEW.creator_user_id, \r\n  NEW.expiry_date, \r\n  NEW.verified_credential_external_type_use_case_detail_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL AFTER UPDATE\r\nON portal.company_ssi_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL();");

            migrationBuilder.Sql(@"CREATE FUNCTION portal.is_credential_type_use_case(vc_type_id integer)
                RETURNS BOOLEAN
                LANGUAGE plpgsql
                AS
                $$
                BEGIN
                    RETURN EXISTS (
                        SELECT 1
                        FROM portal.verified_credential_types
                        WHERE Id = vc_type_id
                            AND vc_type_id IN (
                                SELECT verified_credential_type_id
                                FROM portal.verified_credential_type_assigned_kinds
                                WHERE verified_credential_type_kind_id = '1'
                            )
                    );
                END;
                $$");

            migrationBuilder.Sql(@"
                ALTER TABLE portal.verified_credential_type_assigned_use_cases
                ADD CONSTRAINT CK_VCTypeAssignedUseCase_VerifiedCredentialType_UseCase 
                    CHECK (portal.is_credential_type_use_case(verified_credential_type_id))");

            migrationBuilder.Sql(@"CREATE FUNCTION portal.is_external_type_use_case(verified_credential_external_type_use_case_detail_id UUID)
                RETURNS BOOLEAN
                LANGUAGE plpgsql
                AS
                $$
                BEGIN
                    IF verified_credential_external_type_use_case_detail_id IS NULL THEN
                        RETURN TRUE;
                    END IF;
                    RETURN EXISTS (
                        SELECT 1
                            FROM portal.verified_credential_external_type_use_case_detail_versions
                            WHERE Id = verified_credential_external_type_use_case_detail_id
                              AND verified_credential_external_type_id IN (
                                SELECT verified_credential_external_type_id
                                FROM portal.verified_credential_type_assigned_external_types
                                WHERE verified_credential_type_id IN (
                                    SELECT verified_credential_type_id
                                    FROM portal.verified_credential_type_assigned_kinds
                                    WHERE verified_credential_type_kind_id = '1'
                                )
                            )
                    );
                END;
                $$");

            migrationBuilder.Sql(@"
                ALTER TABLE portal.company_ssi_details
                ADD CONSTRAINT CK_VC_ExternalType_DetailId_UseCase 
                    CHECK (portal.is_external_type_use_case(verified_credential_external_type_use_case_detail_id))");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE portal.verified_credential_type_assigned_use_cases DROP CONSTRAINT IF EXISTS CK_VCTypeAssignedUseCase_VerifiedCredentialType_UseCase;");
            migrationBuilder.Sql("ALTER TABLE portal.company_ssi_details DROP CONSTRAINT IF EXISTS CK_VC_ExternalType_DetailId_UseCase;");

            migrationBuilder.Sql("DROP FUNCTION portal.is_credential_type_use_case;");
            migrationBuilder.Sql("DROP FUNCTION portal.is_external_type_use_case;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYSSIDETAIL() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_IDENTITY() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_IDENTITY() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_IDENTITY() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_IDENTITYASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER() CASCADE;");

            migrationBuilder.DropTable(
                name: "audit_company_ssi_detail20230621",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_ssi_details",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "use_case_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "verified_credential_type_assigned_external_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "verified_credential_type_assigned_kinds",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "verified_credential_type_assigned_use_cases",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_ssi_detail_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "verified_credential_external_type_use_case_detail_versions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "verified_credential_type_kinds",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "verified_credential_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "verified_credential_external_types",
                schema: "portal");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "document_types",
                keyColumn: "id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type",
                keyColumn: "id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type",
                keyColumn: "id",
                keyValue: 25);
            migrationBuilder.AddColumn<string>(
                name: "long_name_de",
                schema: "portal",
                table: "languages",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "long_name_en",
                schema: "portal",
                table: "languages",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE portal.languages AS languages SET long_name_de=subquery.long_name FROM (SELECT short_name, long_name FROM portal.language_long_names WHERE language_short_name='de') AS subquery WHERE languages.short_name = subquery.short_name");
            migrationBuilder.Sql("UPDATE portal.languages AS languages SET long_name_en=subquery.long_name FROM (SELECT short_name, long_name FROM portal.language_long_names WHERE language_short_name='en') AS subquery WHERE languages.short_name = subquery.short_name");

            migrationBuilder.DropTable(
                name: "language_long_names",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_provider_company_detail20230614",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "date_last_changed",
                schema: "portal",
                table: "provider_company_details");

            migrationBuilder.DropColumn(
                name: "last_editor_id",
                schema: "portal",
                table: "provider_company_details");

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                schema: "portal",
                table: "company_users",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<int>(
                name: "company_user_status_id",
                schema: "portal",
                table: "company_users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "date_created",
                schema: "portal",
                table: "company_users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "company_service_account_status_id",
                schema: "portal",
                table: "company_service_accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "date_created",
                schema: "portal",
                table: "company_service_accounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.CreateTable(
                name: "company_service_account_assigned_roles",
                schema: "portal",
                columns: table => new
                {
                    company_service_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_service_account_assigned_roles", x => new { x.company_service_account_id, x.user_role_id });
                    table.ForeignKey(
                        name: "fk_company_service_account_assigned_roles_company_service_acco",
                        column: x => x.company_service_account_id,
                        principalSchema: "portal",
                        principalTable: "company_service_accounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_service_account_assigned_roles_user_roles_user_role",
                        column: x => x.user_role_id,
                        principalSchema: "portal",
                        principalTable: "user_roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_service_account_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_service_account_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_user_assigned_roles",
                schema: "portal",
                columns: table => new
                {
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
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
                name: "company_user_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_user_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "iam_service_accounts",
                schema: "portal",
                columns: table => new
                {
                    client_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    company_service_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_client_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_entity_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iam_service_accounts", x => x.client_id);
                    table.ForeignKey(
                        name: "fk_iam_service_accounts_company_service_accounts_company_servi",
                        column: x => x.company_service_account_id,
                        principalSchema: "portal",
                        principalTable: "company_service_accounts",
                        principalColumn: "id");
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

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_service_account_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_user_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" },
                    { 3, "DELETED" }
                });

            migrationBuilder.Sql("INSERT INTO portal.iam_users (user_entity_id, company_user_id) SELECT i.user_entity_id, cu.id FROM portal.identities AS i JOIN portal.company_users AS cu ON i.id = cu.id WHERE i.identity_type_id = 1 AND i.user_entity_id IS NOT NULL;");
            migrationBuilder.Sql("INSERT INTO portal.iam_service_accounts (client_id, company_service_account_id, client_client_id, user_entity_id) SELECT sa.client_id, i.id, sa.client_client_id, i.user_entity_id FROM portal.identities as i RIGHT JOIN portal.company_service_accounts as sa ON sa.id = i.Id WHERE sa.client_id != '' AND sa.client_client_id != '' AND i.user_entity_id is not null;");

            migrationBuilder.Sql("UPDATE portal.company_users as cu SET date_created = i.date_created, company_id = i.company_id, company_user_status_id = i.user_status_id FROM (SELECT id, date_created, company_id, user_status_id, identity_type_id FROM portal.identities) AS i WHERE i.id = cu.id");
            migrationBuilder.Sql("UPDATE portal.company_service_accounts SET date_created = i.date_created, service_account_owner_id = i.company_id, company_service_account_status_id = i.user_status_id FROM portal.identities AS i JOIN portal.company_service_accounts AS cu ON cu.id = i.id;");

            migrationBuilder.Sql("INSERT INTO portal.company_user_assigned_roles (company_user_id, user_role_id) SELECT cu.id, ir.user_role_id FROM portal.identity_assigned_roles AS ir INNER JOIN portal.identities AS i ON i.id = ir.identity_id INNER JOIN portal.company_users AS cu ON cu.id = i.id WHERE i.identity_type_id = 1;");
            migrationBuilder.Sql("INSERT INTO portal.company_service_account_assigned_roles (company_service_account_id, user_role_id) SELECT ir.identity_id, ir.user_role_id FROM portal.identity_assigned_roles as ir INNER JOIN portal.identities as i ON i.id = ir.identity_id WHERE i.identity_type_id = 2;");

            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_identities_identity_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_company_users_identities_identity_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropTable(
                name: "audit_company_user20230523",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_identity_assigned_role20230522",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_identity20230526",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identity_assigned_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identities",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identity_type",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identity_user_statuses",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "client_client_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "client_id",
                schema: "portal",
                table: "company_service_accounts");

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
                name: "ix_company_service_accounts_company_service_account_status_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "company_service_account_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "service_account_owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_account_assigned_roles_user_role_id",
                schema: "portal",
                table: "company_service_account_assigned_roles",
                column: "user_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_user_assigned_roles_user_role_id",
                schema: "portal",
                table: "company_user_assigned_roles",
                column: "user_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_iam_service_accounts_client_client_id",
                schema: "portal",
                table: "iam_service_accounts",
                column: "client_client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_iam_service_accounts_company_service_account_id",
                schema: "portal",
                table: "iam_service_accounts",
                column: "company_service_account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_iam_service_accounts_user_entity_id",
                schema: "portal",
                table: "iam_service_accounts",
                column: "user_entity_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_iam_users_company_user_id",
                schema: "portal",
                table: "iam_users",
                column: "company_user_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_accounts_companies_service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "service_account_owner_id",
                principalSchema: "portal",
                principalTable: "companies",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_accounts_company_service_account_statuses_c",
                schema: "portal",
                table: "company_service_accounts",
                column: "company_service_account_status_id",
                principalSchema: "portal",
                principalTable: "company_service_account_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_users_companies_company_id",
                schema: "portal",
                table: "company_users",
                column: "company_id",
                principalSchema: "portal",
                principalTable: "companies",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_users_company_user_statuses_company_user_status_id",
                schema: "portal",
                table: "company_users",
                column: "company_user_status_id",
                principalSchema: "portal",
                principalTable: "company_user_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_assigned_role20221018 (\"company_user_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.company_user_id, \r\n  OLD.user_role_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE AFTER DELETE\r\nON portal.company_user_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_assigned_role20221018 (\"company_user_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.company_user_id, \r\n  NEW.user_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE AFTER INSERT\r\nON portal.company_user_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_assigned_role20221018 (\"company_user_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.company_user_id, \r\n  NEW.user_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE AFTER UPDATE\r\nON portal.company_user_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20221005 (\"id\", \"date_created\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"company_id\", \"company_user_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.email, \r\n  OLD.firstname, \r\n  OLD.lastlogin, \r\n  OLD.lastname, \r\n  OLD.company_id, \r\n  OLD.company_user_status_id, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYUSER AFTER DELETE\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20221005 (\"id\", \"date_created\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"company_id\", \"company_user_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.email, \r\n  NEW.firstname, \r\n  NEW.lastlogin, \r\n  NEW.lastname, \r\n  NEW.company_id, \r\n  NEW.company_user_status_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYUSER AFTER INSERT\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20221005 (\"id\", \"date_created\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"company_id\", \"company_user_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.email, \r\n  NEW.firstname, \r\n  NEW.lastlogin, \r\n  NEW.lastname, \r\n  NEW.company_id, \r\n  NEW.company_user_status_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYUSER AFTER UPDATE\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER();");

        }
    }
}
