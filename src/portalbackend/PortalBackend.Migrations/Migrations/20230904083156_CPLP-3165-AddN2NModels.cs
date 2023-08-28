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

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class CPLP3165AddN2NModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION() CASCADE;");

            migrationBuilder.AddColumn<int>(
                name: "identity_provider_type_id",
                schema: "portal",
                table: "identity_providers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "owner_id",
                schema: "portal",
                table: "identity_providers",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<int>(
                name: "company_application_type_id",
                schema: "portal",
                table: "company_applications",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "audit_company_application20230824",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    application_status_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    checklist_process_id = table.Column<Guid>(type: "uuid", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    company_application_type_id = table.Column<int>(type: "integer", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_application20230824", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "company_application_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_application_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "identity_provider_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_provider_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "onboarding_service_provider_details",
                schema: "portal",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    callback_url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_onboarding_service_provider_details", x => x.company_id);
                    table.ForeignKey(
                        name: "fk_onboarding_service_provider_details_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_application_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "INTERNAL" },
                    { 2, "EXTERNAL" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_roles",
                columns: new[] { "id", "label" },
                values: new object[] { 5, "ONBOARDING_SERVICE_PROVIDER" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "identity_provider_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "OWN" },
                    { 2, "MANAGED" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_identity_providers_identity_provider_type_id",
                schema: "portal",
                table: "identity_providers",
                column: "identity_provider_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_providers_owner_id",
                schema: "portal",
                table: "identity_providers",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_applications_company_application_type_id",
                schema: "portal",
                table: "company_applications",
                column: "company_application_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_applications_onboarding_service_provider_id",
                schema: "portal",
                table: "company_applications",
                column: "onboarding_service_provider_id");

            migrationBuilder.Sql("UPDATE portal.company_applications SET company_application_type_id = 1");
            migrationBuilder.Sql("UPDATE portal.identity_providers SET identity_provider_type_id = 1 WHERE identity_provider_category_id = 1");
            migrationBuilder.Sql("UPDATE portal.identity_providers SET identity_provider_type_id = 2 WHERE identity_provider_category_id != 1");
            migrationBuilder.Sql("UPDATE portal.identity_providers SET owner_id = '2dc4249f-b5ca-4d42-bef1-7a7a950a4f87'");
            migrationBuilder.Sql("UPDATE portal.identity_providers AS idp SET owner_id = (SELECT cip.company_id FROM portal.company_identity_providers AS cip WHERE idp.id = cip.identity_provider_id) WHERE idp.id IN (SELECT cip.identity_provider_id FROM portal.company_identity_providers AS cip);");

            migrationBuilder.AddForeignKey(
                name: "fk_company_applications_companies_onboarding_service_provider_",
                schema: "portal",
                table: "company_applications",
                column: "onboarding_service_provider_id",
                principalSchema: "portal",
                principalTable: "companies",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_applications_company_application_types_company_appl",
                schema: "portal",
                table: "company_applications",
                column: "company_application_type_id",
                principalSchema: "portal",
                principalTable: "company_application_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_identity_providers_companies_owner_id",
                schema: "portal",
                table: "identity_providers",
                column: "owner_id",
                principalSchema: "portal",
                principalTable: "companies",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_identity_providers_identity_provider_types_identity_provide",
                schema: "portal",
                table: "identity_providers",
                column: "identity_provider_type_id",
                principalSchema: "portal",
                principalTable: "identity_provider_types",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_company_application20230824\" (\"company_id\", \"company_application_type_id\", \"checklist_process_id\", \"date_last_changed\", \"last_editor_id\", \"application_status_id\", \"date_created\", \"id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"company_id\", \r\n  NEW.\"company_application_type_id\", \r\n  NEW.\"checklist_process_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  NEW.\"application_status_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION AFTER INSERT\r\nON \"portal\".\"company_applications\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_company_application20230824\" (\"company_id\", \"company_application_type_id\", \"checklist_process_id\", \"date_last_changed\", \"last_editor_id\", \"application_status_id\", \"date_created\", \"id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"company_id\", \r\n  NEW.\"company_application_type_id\", \r\n  NEW.\"checklist_process_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  NEW.\"application_status_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION AFTER UPDATE\r\nON \"portal\".\"company_applications\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION\"() CASCADE;");

            migrationBuilder.Sql("DELETE FROM portal.agreement_assigned_company_roles where company_role_id = 5");

            migrationBuilder.DropForeignKey(
                name: "fk_company_applications_company_application_types_company_appl",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.DropForeignKey(
                name: "fk_identity_providers_companies_owner_id",
                schema: "portal",
                table: "identity_providers");

            migrationBuilder.DropForeignKey(
                name: "fk_identity_providers_identity_provider_types_identity_provide",
                schema: "portal",
                table: "identity_providers");

            migrationBuilder.DropTable(
                name: "audit_company_application20230824",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_application_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identity_provider_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "onboarding_service_provider_details",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_identity_providers_identity_provider_type_id",
                schema: "portal",
                table: "identity_providers");

            migrationBuilder.DropIndex(
                name: "ix_identity_providers_owner_id",
                schema: "portal",
                table: "identity_providers");

            migrationBuilder.DropIndex(
                name: "ix_company_applications_company_application_type_id",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_roles",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "identity_provider_type_id",
                schema: "portal",
                table: "identity_providers");

            migrationBuilder.DropColumn(
                name: "owner_id",
                schema: "portal",
                table: "identity_providers");

            migrationBuilder.DropColumn(
                name: "company_application_type_id",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_company_application20230214\" (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"checklist_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"application_status_id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"checklist_process_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION AFTER INSERT\r\nON \"portal\".\"company_applications\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_company_application20230214\" (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"checklist_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"application_status_id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"checklist_process_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION AFTER UPDATE\r\nON \"portal\".\"company_applications\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION\"();");
        }
    }
}
