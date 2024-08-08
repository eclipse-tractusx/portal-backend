/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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
    public partial class _220rc1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE\"() CASCADE;");

            migrationBuilder.AddColumn<Guid>(
                name: "version",
                schema: "portal",
                table: "company_service_accounts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "valid_from",
                schema: "portal",
                table: "company_certificates",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "external_certificate_number",
                schema: "portal",
                table: "company_certificates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "issuer",
                schema: "portal",
                table: "company_certificates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trust_level",
                schema: "portal",
                table: "company_certificates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "validator",
                schema: "portal",
                table: "company_certificates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_certificate_number",
                schema: "portal",
                table: "audit_certificate_management20240416",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "issuer",
                schema: "portal",
                table: "audit_certificate_management20240416",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trust_level",
                schema: "portal",
                table: "audit_certificate_management20240416",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "validator",
                schema: "portal",
                table: "audit_certificate_management20240416",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "company_certificate_assigned_sites",
                schema: "portal",
                columns: table => new
                {
                    company_certificate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    site = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_certificate_assigned_sites", x => new { x.company_certificate_id, x.site });
                    table.ForeignKey(
                        name: "fk_company_certificate_assigned_sites_company_certificates_com",
                        column: x => x.company_certificate_id,
                        principalSchema: "portal",
                        principalTable: "company_certificates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "application_checklist_statuses",
                columns: new[] { "id", "label" },
                values: new object[] { 5, "SKIPPED" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "identity_user_statuses",
                columns: new[] { "id", "label" },
                values: new object[] { 5, "PENDING_DELETION" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 502, "AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE" },
                    { 503, "RETRIGGER_AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE" },
                    { 504, "DELETE_DIM_TECHNICAL_USER" },
                    { 505, "AWAIT_DELETE_DIM_TECHNICAL_USER" },
                    { 506, "RETRIGGER_DELETE_DIM_TECHNICAL_USER" },
                    { 800, "SELF_DESCRIPTION_CONNECTOR_CREATION" },
                    { 801, "SELF_DESCRIPTION_COMPANY_CREATION" },
                    { 802, "RETRIGGER_SELF_DESCRIPTION_CONNECTOR_CREATION" },
                    { 803, "RETRIGGER_SELF_DESCRIPTION_COMPANY_CREATION" }
                });

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 502 WHERE process_step_type_id = 114");
            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 503 WHERE process_step_type_id = 115");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 114);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 115);

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 10, "SELF_DESCRIPTION_CREATION" });

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_certificate_management20240416\" (\"id\", \"valid_from\", \"valid_till\", \"company_certificate_type_id\", \"company_certificate_status_id\", \"company_id\", \"document_id\", \"external_certificate_number\", \"issuer\", \"trust_level\", \"validator\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"valid_from\", \r\n  NEW.\"valid_till\", \r\n  NEW.\"company_certificate_type_id\", \r\n  NEW.\"company_certificate_status_id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"external_certificate_number\", \r\n  NEW.\"issuer\", \r\n  NEW.\"trust_level\", \r\n  NEW.\"validator\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE AFTER INSERT\r\nON \"portal\".\"company_certificates\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_certificate_management20240416\" (\"id\", \"valid_from\", \"valid_till\", \"company_certificate_type_id\", \"company_certificate_status_id\", \"company_id\", \"document_id\", \"external_certificate_number\", \"issuer\", \"trust_level\", \"validator\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"valid_from\", \r\n  NEW.\"valid_till\", \r\n  NEW.\"company_certificate_type_id\", \r\n  NEW.\"company_certificate_status_id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"external_certificate_number\", \r\n  NEW.\"issuer\", \r\n  NEW.\"trust_level\", \r\n  NEW.\"validator\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE AFTER UPDATE\r\nON \"portal\".\"company_certificates\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE\"() CASCADE;");

            migrationBuilder.DropTable(
                name: "company_certificate_assigned_sites",
                schema: "portal");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "application_checklist_statuses",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "identity_user_statuses",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_types",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DropColumn(
                name: "version",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "external_certificate_number",
                schema: "portal",
                table: "company_certificates");

            migrationBuilder.DropColumn(
                name: "issuer",
                schema: "portal",
                table: "company_certificates");

            migrationBuilder.DropColumn(
                name: "trust_level",
                schema: "portal",
                table: "company_certificates");

            migrationBuilder.DropColumn(
                name: "validator",
                schema: "portal",
                table: "company_certificates");

            migrationBuilder.DropColumn(
                name: "external_certificate_number",
                schema: "portal",
                table: "audit_certificate_management20240416");

            migrationBuilder.DropColumn(
                name: "issuer",
                schema: "portal",
                table: "audit_certificate_management20240416");

            migrationBuilder.DropColumn(
                name: "trust_level",
                schema: "portal",
                table: "audit_certificate_management20240416");

            migrationBuilder.DropColumn(
                name: "validator",
                schema: "portal",
                table: "audit_certificate_management20240416");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "valid_from",
                schema: "portal",
                table: "company_certificates",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 114, "AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE" },
                    { 115, "RETRIGGER_AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE" }
                });

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 114 WHERE process_step_type_id = 502");
            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 115 WHERE process_step_type_id = 503");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 502);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 503);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 504);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 505);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 506);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 800);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 801);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 802);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 803);

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_certificate_management20240416\" (\"id\", \"valid_from\", \"valid_till\", \"company_certificate_type_id\", \"company_certificate_status_id\", \"company_id\", \"document_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"valid_from\", \r\n  NEW.\"valid_till\", \r\n  NEW.\"company_certificate_type_id\", \r\n  NEW.\"company_certificate_status_id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE AFTER INSERT\r\nON \"portal\".\"company_certificates\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_certificate_management20240416\" (\"id\", \"valid_from\", \"valid_till\", \"company_certificate_type_id\", \"company_certificate_status_id\", \"company_id\", \"document_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"valid_from\", \r\n  NEW.\"valid_till\", \r\n  NEW.\"company_certificate_type_id\", \r\n  NEW.\"company_certificate_status_id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE AFTER UPDATE\r\nON \"portal\".\"company_certificates\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE\"();");
        }
    }
}
