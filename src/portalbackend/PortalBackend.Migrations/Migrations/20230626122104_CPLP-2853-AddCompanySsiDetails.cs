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
    public partial class CPLP2853AddCompanySsiDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                columns: new[] { "verified_credential_external_type_id", "version" },
                unique: true);

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
        }
    }
}
