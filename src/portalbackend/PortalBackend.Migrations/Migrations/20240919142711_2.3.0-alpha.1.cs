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
    public partial class _230alpha1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"() CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_offers_companies_provider_company_id",
                schema: "portal",
                table: "offers");

            migrationBuilder.Sql("DELETE from portal.process_steps WHERE process_step_type_id = 15");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 15);

            migrationBuilder.Sql("DELETE from portal.process_steps WHERE process_step_type_id = 503");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 503);

            migrationBuilder.DropColumn(
                name: "provider",
                schema: "portal",
                table: "offers");

            migrationBuilder.DropColumn(
                name: "organisation_name",
                schema: "portal",
                table: "company_invitations");

            migrationBuilder.AlterColumn<Guid>(
                name: "provider_company_id",
                schema: "portal",
                table: "offers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "application_id",
                schema: "portal",
                table: "company_invitations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "audit_offer20240911",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    date_released = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    marketing_url = table.Column<string>(type: "text", nullable: true),
                    contact_email = table.Column<string>(type: "text", nullable: true),
                    contact_number = table.Column<string>(type: "text", nullable: true),
                    offer_type_id = table.Column<int>(type: "integer", nullable: true),
                    sales_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    provider_company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    offer_status_id = table.Column<int>(type: "integer", nullable: true),
                    license_type_id = table.Column<int>(type: "integer", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_offer20240911", x => x.audit_v1id);
                });

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 1,
                column: "label",
                value: "MANUAL_VERIFY_REGISTRATION");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 9,
                column: "label",
                value: "AWAIT_CLEARING_HOUSE_RESPONSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 16,
                column: "label",
                value: "MANUAL_TRIGGER_OVERRIDE_CLEARING_HOUSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 18,
                column: "label",
                value: "AWAIT_SELF_DESCRIPTION_LP_RESPONSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 19,
                column: "label",
                value: "MANUAL_DECLINE_APPLICATION");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 21,
                column: "label",
                value: "AWAIT_DIM_RESPONSE_RESPONSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 26,
                column: "label",
                value: "AWAIT_BPN_CREDENTIAL_RESPONSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 28,
                column: "label",
                value: "AWAIT_MEMBERSHIP_CREDENTIAL_RESPONSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 101,
                column: "label",
                value: "AWAIT_START_AUTOSETUP");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 111,
                column: "label",
                value: "MANUAL_TRIGGER_ACTIVATE_SUBSCRIPTION");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 505,
                column: "label",
                value: "AWAIT_DELETE_DIM_TECHNICAL_USER_RESPONSE");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 31, "RETRIGGER_REQUEST_BPN_CREDENTIAL" },
                    { 32, "RETRIGGER_REQUEST_MEMBERSHIP_CREDENTIAL" }
                });

            migrationBuilder.AddForeignKey(
                name: "fk_offers_companies_provider_company_id",
                schema: "portal",
                table: "offers",
                column: "provider_company_id",
                principalSchema: "portal",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFER$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer20240911\" (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_released\", \r\n  NEW.\"marketing_url\", \r\n  NEW.\"contact_email\", \r\n  NEW.\"contact_number\", \r\n  NEW.\"offer_type_id\", \r\n  NEW.\"sales_manager_id\", \r\n  NEW.\"provider_company_id\", \r\n  NEW.\"offer_status_id\", \r\n  NEW.\"license_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFER AFTER INSERT\r\nON \"portal\".\"offers\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFER$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer20240911\" (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_released\", \r\n  NEW.\"marketing_url\", \r\n  NEW.\"contact_email\", \r\n  NEW.\"contact_number\", \r\n  NEW.\"offer_type_id\", \r\n  NEW.\"sales_manager_id\", \r\n  NEW.\"provider_company_id\", \r\n  NEW.\"offer_status_id\", \r\n  NEW.\"license_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFER AFTER UPDATE\r\nON \"portal\".\"offers\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"() CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_offers_companies_provider_company_id",
                schema: "portal",
                table: "offers");

            migrationBuilder.DropTable(
                name: "audit_offer20240911",
                schema: "portal");

            migrationBuilder.Sql("DELETE from portal.process_steps WHERE process_step_type_id = 31");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 31);

            migrationBuilder.Sql("DELETE from portal.process_steps WHERE process_step_type_id = 32");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 32);

            migrationBuilder.AlterColumn<Guid>(
                name: "provider_company_id",
                schema: "portal",
                table: "offers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "provider",
                schema: "portal",
                table: "offers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "application_id",
                schema: "portal",
                table: "company_invitations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "organisation_name",
                schema: "portal",
                table: "company_invitations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 1,
                column: "label",
                value: "VERIFY_REGISTRATION");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 9,
                column: "label",
                value: "END_CLEARING_HOUSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 16,
                column: "label",
                value: "TRIGGER_OVERRIDE_CLEARING_HOUSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 18,
                column: "label",
                value: "FINISH_SELF_DESCRIPTION_LP");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 19,
                column: "label",
                value: "DECLINE_APPLICATION");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 21,
                column: "label",
                value: "AWAIT_DIM_RESPONSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 26,
                column: "label",
                value: "STORED_BPN_CREDENTIAL");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 28,
                column: "label",
                value: "STORED_MEMBERSHIP_CREDENTIAL");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 101,
                column: "label",
                value: "START_AUTOSETUP");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 111,
                column: "label",
                value: "TRIGGER_ACTIVATE_SUBSCRIPTION");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 505,
                column: "label",
                value: "AWAIT_DELETE_DIM_TECHNICAL_USER");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 15, "OVERRIDE_BUSINESS_PARTNER_NUMBER" },
                    { 503, "RETRIGGER_AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE" }
                });

            migrationBuilder.AddForeignKey(
                name: "fk_offers_companies_provider_company_id",
                schema: "portal",
                table: "offers",
                column: "provider_company_id",
                principalSchema: "portal",
                principalTable: "companies",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFER$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer20231115\" (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_released\", \r\n  NEW.\"marketing_url\", \r\n  NEW.\"contact_email\", \r\n  NEW.\"contact_number\", \r\n  NEW.\"provider\", \r\n  NEW.\"offer_type_id\", \r\n  NEW.\"sales_manager_id\", \r\n  NEW.\"provider_company_id\", \r\n  NEW.\"offer_status_id\", \r\n  NEW.\"license_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFER AFTER INSERT\r\nON \"portal\".\"offers\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFER$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer20231115\" (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_released\", \r\n  NEW.\"marketing_url\", \r\n  NEW.\"contact_email\", \r\n  NEW.\"contact_number\", \r\n  NEW.\"provider\", \r\n  NEW.\"offer_type_id\", \r\n  NEW.\"sales_manager_id\", \r\n  NEW.\"provider_company_id\", \r\n  NEW.\"offer_status_id\", \r\n  NEW.\"license_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFER AFTER UPDATE\r\nON \"portal\".\"offers\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"();");
        }
    }
}
