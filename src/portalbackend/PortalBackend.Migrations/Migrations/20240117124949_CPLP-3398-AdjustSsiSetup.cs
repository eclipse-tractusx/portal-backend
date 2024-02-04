/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class CPLP3398AdjustSsiSetup : Migration
    {
        /// <inheritdoc />
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"() CASCADE;");

            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS ct_is_external_type_use_case ON portal.company_ssi_details;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS portal.tr_is_external_type_use_case;");

            migrationBuilder.DropForeignKey(
                name: "fk_company_ssi_details_verified_credential_external_type_use_c",
                schema: "portal",
                table: "company_ssi_details");

            migrationBuilder.Sql("UPDATE portal.company_ssi_details SET expiry_date = date_created + INTERVAL '12 months' WHERE expiry_date IS NULL");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "expiry",
                schema: "portal",
                table: "verified_credential_external_type_use_case_detail_versions",
                type: "timestamp with timezone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldNullable: true,
                oldType: "timestamp with timezone");

            migrationBuilder.AlterColumn<string>(
                name: "version",
                schema: "portal",
                table: "verified_credential_external_type_use_case_detail_versions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: false,
                oldType: "text");

            migrationBuilder.RenameTable(
                name: "verified_credential_external_type_use_case_detail_versions",
                schema: "portal",
                newName: "verified_credential_external_type_detail_versions",
                newSchema: "portal");

            migrationBuilder.RenameColumn(
                name: "verified_credential_external_type_use_case_detail_id",
                schema: "portal",
                table: "company_ssi_details",
                newName: "verified_credential_external_type_detail_version_id");

            migrationBuilder.RenameIndex(
                name: "ix_company_ssi_details_verified_credential_external_type_use_c",
                schema: "portal",
                table: "company_ssi_details",
                newName: "ix_company_ssi_details_verified_credential_external_type_detai");

            migrationBuilder.Sql(@"CREATE FUNCTION portal.tr_is_external_type_use_case()
                RETURNS trigger
                VOLATILE
                COST 100
                AS $$
                BEGIN
                IF NEW.version IS NOT NULL
                    THEN RETURN NEW;
                END IF;
                IF EXISTS (
                    SELECT 1
                        FROM portal.verified_credential_type_assigned_external_types
                        WHERE 
                            verified_credential_external_type_id = NEW.verified_credential_external_type_id AND
                            verified_credential_type_id IN (
                                SELECT verified_credential_type_id
                                FROM portal.verified_credential_type_assigned_kinds
                                WHERE verified_credential_type_kind_id = '2'
                            )
                )
                THEN RETURN NEW;
                END IF;
                RAISE EXCEPTION 'the version % must be set for use cases', NEW.id;
                END
                $$ LANGUAGE plpgsql;");

            migrationBuilder.Sql(@"CREATE CONSTRAINT TRIGGER ct_is_external_type_use_case
                AFTER INSERT
                ON portal.verified_credential_external_type_detail_versions
                INITIALLY DEFERRED
                FOR EACH ROW
                EXECUTE PROCEDURE portal.tr_is_external_type_use_case();");

            migrationBuilder.AddColumn<int>(
                name: "expiry_check_type_id",
                schema: "portal",
                table: "company_ssi_details",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_company_ssi_detail20240104",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    verified_credential_type_id = table.Column<int>(type: "integer", nullable: true),
                    company_ssi_detail_status_id = table.Column<int>(type: "integer", nullable: true),
                    document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    creator_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expiry_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    verified_credential_external_type_detail_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expiry_check_type_id = table.Column<int>(type: "integer", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_ssi_detail20240104", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "expiry_check_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expiry_check_types", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "expiry_check_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "ONE_MONTH" },
                    { 2, "TWO_WEEKS" },
                    { 3, "ONE_DAY" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_type",
                columns: new[] { "id", "label" },
                values: new object[] { 26, "CREDENTIAL_EXPIRY" });

            migrationBuilder.RenameIndex(
                table: "verified_credential_external_type_detail_versions",
                schema: "portal",
                name: "ix_verified_credential_external_type_use_case_detail_versions_",
                newName: "ix_verified_credential_external_type_detail_versions_verified_");

            migrationBuilder.RenameIndex(
                table: "verified_credential_external_type_detail_versions",
                schema: "portal",
                name: "pk_verified_credential_external_type_use_case_detail_versions",
                newName: "pk_verified_credential_external_type_detail_versions");

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_expiry_check_type_id",
                schema: "portal",
                table: "company_ssi_details",
                column: "expiry_check_type_id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_ssi_details_expiry_check_types_expiry_check_type_id",
                schema: "portal",
                table: "company_ssi_details",
                column: "expiry_check_type_id",
                principalSchema: "portal",
                principalTable: "expiry_check_types",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_ssi_details_verified_credential_external_type_detai",
                schema: "portal",
                table: "company_ssi_details",
                column: "verified_credential_external_type_detail_version_id",
                principalSchema: "portal",
                principalTable: "verified_credential_external_type_detail_versions",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_company_ssi_detail20240104\" (\"id\", \"company_id\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"document_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_detail_version_id\", \"expiry_check_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"verified_credential_type_id\", \r\n  NEW.\"company_ssi_detail_status_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"creator_user_id\", \r\n  NEW.\"expiry_date\", \r\n  NEW.\"verified_credential_external_type_detail_version_id\", \r\n  NEW.\"expiry_check_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL AFTER INSERT\r\nON \"portal\".\"company_ssi_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_company_ssi_detail20240104\" (\"id\", \"company_id\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"document_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_detail_version_id\", \"expiry_check_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"verified_credential_type_id\", \r\n  NEW.\"company_ssi_detail_status_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"creator_user_id\", \r\n  NEW.\"expiry_date\", \r\n  NEW.\"verified_credential_external_type_detail_version_id\", \r\n  NEW.\"expiry_check_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL AFTER UPDATE\r\nON \"portal\".\"company_ssi_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"() CASCADE;");

            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS ct_is_external_type_use_case ON portal.company_ssi_details;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS portal.tr_is_external_type_use_case;");

            migrationBuilder.DropForeignKey(
                name: "fk_company_ssi_details_expiry_check_types_expiry_check_type_id",
                schema: "portal",
                table: "company_ssi_details");

            migrationBuilder.DropForeignKey(
                name: "fk_company_ssi_details_verified_credential_external_type_detai",
                schema: "portal",
                table: "company_ssi_details");

            migrationBuilder.DropTable(
                name: "audit_company_ssi_detail20240104",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "expiry_check_types",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_company_ssi_details_expiry_check_type_id",
                schema: "portal",
                table: "company_ssi_details");

            migrationBuilder.DropColumn(
                name: "expiry_check_type_id",
                schema: "portal",
                table: "company_ssi_details");

            migrationBuilder.RenameTable(
                name: "verified_credential_external_type_detail_versions",
                schema: "portal",
                newName: "verified_credential_external_type_use_case_detail_versions",
                newSchema: "portal");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type",
                keyColumn: "id",
                keyValue: 26);

            migrationBuilder.DropColumn(
                name: "expiry_check_type_id",
                schema: "portal",
                table: "company_ssi_details");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "expiry",
                schema: "portal",
                table: "verified_credential_external_type_use_case_detail_versions",
                type: "timestamp with timezone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset?),
                oldNullable: false,
                oldType: "timestamp with timezone");

            migrationBuilder.AlterColumn<string>(
                name: "version",
                schema: "portal",
                table: "verified_credential_external_type_use_case_detail_versions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true,
                oldType: "text");

            migrationBuilder.RenameColumn(
                name: "verified_credential_external_type_detail_version_id",
                schema: "portal",
                table: "company_ssi_details",
                newName: "verified_credential_external_type_use_case_detail_id");

            migrationBuilder.RenameIndex(
                table: "verified_credential_external_type_use_case_detail_versions",
                schema: "portal",
                name: "ix_verified_credential_external_type_detail_versions_verified_",
                newName: "ix_verified_credential_external_type_use_case_detail_versions_"
            );

            migrationBuilder.RenameIndex(
                table: "verified_credential_external_type_use_case_detail_versions",
                schema: "portal",
                name: "pk_verified_credential_external_type_detail_versions",
                newName: "pk_verified_credential_external_type_use_case_detail_versions");

            migrationBuilder.Sql(@"CREATE FUNCTION portal.tr_is_external_type_use_case()
                RETURNS trigger
                VOLATILE
                COST 100
                AS $$
                BEGIN
                IF NEW.verified_credential_external_type_use_case_detail_id IS NULL
                    THEN RETURN NEW;
                END IF;
                IF EXISTS (
                    SELECT 1
                            FROM portal.verified_credential_external_type_use_case_detail_versions
                            WHERE Id = NEW.verified_credential_external_type_use_case_detail_id
                                AND verified_credential_external_type_id IN (
                                SELECT verified_credential_external_type_id
                                FROM portal.verified_credential_type_assigned_external_types
                                WHERE verified_credential_type_id IN (
                                    SELECT verified_credential_type_id
                                    FROM portal.verified_credential_type_assigned_kinds
                                    WHERE verified_credential_type_kind_id = '1'
                                )
                            )
                )
                THEN RETURN NEW;
                END IF;
                RAISE EXCEPTION 'the detail % is not an use case', NEW.verified_credential_external_type_use_case_detail_id;
                END
                $$ LANGUAGE plpgsql;");

            migrationBuilder.Sql(@"CREATE CONSTRAINT TRIGGER ct_is_external_type_use_case
                AFTER INSERT
                ON portal.company_ssi_details
                INITIALLY DEFERRED
                FOR EACH ROW
                EXECUTE PROCEDURE portal.tr_is_external_type_use_case();");

            migrationBuilder.AddForeignKey(
                name: "fk_company_ssi_details_verified_credential_external_type_use_c",
                schema: "portal",
                table: "company_ssi_details",
                column: "verified_credential_external_type_use_case_detail_id",
                principalSchema: "portal",
                principalTable: "verified_credential_external_type_use_case_detail_versions",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_company_ssi_detail20231115\" (\"id\", \"company_id\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"document_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_use_case_detail_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"verified_credential_type_id\", \r\n  NEW.\"company_ssi_detail_status_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"creator_user_id\", \r\n  NEW.\"expiry_date\", \r\n  NEW.\"verified_credential_external_type_use_case_detail_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL AFTER INSERT\r\nON \"portal\".\"company_ssi_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_company_ssi_detail20231115\" (\"id\", \"company_id\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"document_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_use_case_detail_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"verified_credential_type_id\", \r\n  NEW.\"company_ssi_detail_status_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"creator_user_id\", \r\n  NEW.\"expiry_date\", \r\n  NEW.\"verified_credential_external_type_use_case_detail_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL AFTER UPDATE\r\nON \"portal\".\"company_ssi_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"();");
        }
    }
}
