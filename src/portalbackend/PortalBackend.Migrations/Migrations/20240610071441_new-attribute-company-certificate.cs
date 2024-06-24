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

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class newattributecompanycertificate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE\"() CASCADE;");

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
                name: "companies_certificate_assigned_sites",
                schema: "portal",
                columns: table => new
                {
                    company_certificate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sites = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companies_certificate_assigned_sites", x => new { x.company_certificate_id, x.sites });
                    table.ForeignKey(
                        name: "fk_companies_certificate_assigned_sites_company_certificates_c",
                        column: x => x.company_certificate_id,
                        principalSchema: "portal",
                        principalTable: "company_certificates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_certificate_management20240416\" (\"id\", \"valid_from\", \"valid_till\", \"company_certificate_type_id\", \"company_certificate_status_id\", \"company_id\", \"document_id\", \"external_certificate_number\", \"issuer\", \"trust_level\", \"validator\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"valid_from\", \r\n  NEW.\"valid_till\", \r\n  NEW.\"company_certificate_type_id\", \r\n  NEW.\"company_certificate_status_id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"external_certificate_number\", \r\n  NEW.\"issuer\", \r\n  NEW.\"trust_level\", \r\n  NEW.\"validator\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE AFTER INSERT\r\nON \"portal\".\"company_certificates\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_certificate_management20240416\" (\"id\", \"valid_from\", \"valid_till\", \"company_certificate_type_id\", \"company_certificate_status_id\", \"company_id\", \"document_id\", \"external_certificate_number\", \"issuer\", \"trust_level\", \"validator\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"valid_from\", \r\n  NEW.\"valid_till\", \r\n  NEW.\"company_certificate_type_id\", \r\n  NEW.\"company_certificate_status_id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"external_certificate_number\", \r\n  NEW.\"issuer\", \r\n  NEW.\"trust_level\", \r\n  NEW.\"validator\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE AFTER UPDATE\r\nON \"portal\".\"company_certificates\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE\"() CASCADE;");

            migrationBuilder.DropTable(
                name: "companies_certificate_assigned_sites",
                schema: "portal");

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

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_certificate_management20240416\" (\"id\", \"valid_from\", \"valid_till\", \"company_certificate_type_id\", \"company_certificate_status_id\", \"company_id\", \"document_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"valid_from\", \r\n  NEW.\"valid_till\", \r\n  NEW.\"company_certificate_type_id\", \r\n  NEW.\"company_certificate_status_id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE AFTER INSERT\r\nON \"portal\".\"company_certificates\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYCERTIFICATE\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_certificate_management20240416\" (\"id\", \"valid_from\", \"valid_till\", \"company_certificate_type_id\", \"company_certificate_status_id\", \"company_id\", \"document_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"valid_from\", \r\n  NEW.\"valid_till\", \r\n  NEW.\"company_certificate_type_id\", \r\n  NEW.\"company_certificate_status_id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"document_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE AFTER UPDATE\r\nON \"portal\".\"company_certificates\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYCERTIFICATE\"();");
        }
    }
}
