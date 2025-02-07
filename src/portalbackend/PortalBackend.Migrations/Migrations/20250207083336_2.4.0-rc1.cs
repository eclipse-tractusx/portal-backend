/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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
    public partial class _240rc1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"() CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_process_steps_process_step_statuses_process_step_status_id",
                schema: "portal",
                table: "process_steps");

            migrationBuilder.DropColumn(
                name: "display_technical_user",
                schema: "portal",
                table: "offers");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "sd_skipped_date",
                schema: "portal",
                table: "connectors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "agreement_descriptions",
                schema: "portal",
                columns: table => new
                {
                    agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_descriptions", x => new { x.agreement_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_agreement_descriptions_agreements_agreement_id",
                        column: x => x.agreement_id,
                        principalSchema: "portal",
                        principalTable: "agreements",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_agreement_descriptions_languages_language_short_name",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name");
                });

            migrationBuilder.CreateTable(
                name: "audit_connector20250113",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    connector_url = table.Column<string>(type: "text", nullable: true),
                    type_id = table.Column<int>(type: "integer", nullable: true),
                    status_id = table.Column<int>(type: "integer", nullable: true),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: true),
                    host_id = table.Column<Guid>(type: "uuid", nullable: true),
                    self_description_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    location_id = table.Column<string>(type: "text", nullable: true),
                    self_description_message = table.Column<string>(type: "text", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    technical_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sd_creation_process_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sd_skipped_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_connector20250113", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "audit_offer20250121",
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
                    table.PrimaryKey("pk_audit_offer20250121", x => x.audit_v1id);
                });

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "identity_type",
                keyColumn: "id",
                keyValue: 2,
                column: "label",
                value: "TECHNICAL_USER");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_type",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 27, "APP_SUBSCRIPTION_DECLINE" },
                    { 28, "SERVICE_SUBSCRIPTION_DECLINE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 44, "SET_CX_MEMBERSHIP_IN_BPDM" },
                    { 45, "RETRIGGER_SET_CX_MEMBERSHIP_IN_BPDM" },
                    { 804, "AWAIT_SELF_DESCRIPTION_CONNECTOR_RESPONSE" },
                    { 805, "AWAIT_SELF_DESCRIPTION_COMPANY_RESPONSE" },
                    { 806, "RETRIGGER_AWAIT_SELF_DESCRIPTION_CONNECTOR_RESPONSE" },
                    { 807, "RETRIGGER_AWAIT_SELF_DESCRIPTION_COMPANY_RESPONSE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "technical_user_types",
                columns: new[] { "id", "label" },
                values: new object[] { 3, "PROVIDER_OWNED" });

            migrationBuilder.CreateIndex(
                name: "ix_agreement_descriptions_language_short_name",
                schema: "portal",
                table: "agreement_descriptions",
                column: "language_short_name");

            migrationBuilder.AddForeignKey(
                name: "fk_process_steps_process_step_statuses_process_step_status_id",
                schema: "portal",
                table: "process_steps",
                column: "process_step_status_id",
                principalSchema: "portal",
                principalTable: "process_step_statuses",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20250113\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"technical_user_id\", \"sd_creation_process_id\", \"sd_skipped_date\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"technical_user_id\", \r\n  NEW.\"sd_creation_process_id\", \r\n  NEW.\"sd_skipped_date\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20250113\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"technical_user_id\", \"sd_creation_process_id\", \"sd_skipped_date\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"technical_user_id\", \r\n  NEW.\"sd_creation_process_id\", \r\n  NEW.\"sd_skipped_date\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFER$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer20250121\" (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_released\", \r\n  NEW.\"marketing_url\", \r\n  NEW.\"contact_email\", \r\n  NEW.\"contact_number\", \r\n  NEW.\"offer_type_id\", \r\n  NEW.\"sales_manager_id\", \r\n  NEW.\"provider_company_id\", \r\n  NEW.\"offer_status_id\", \r\n  NEW.\"license_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFER AFTER INSERT\r\nON \"portal\".\"offers\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFER$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer20250121\" (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_released\", \r\n  NEW.\"marketing_url\", \r\n  NEW.\"contact_email\", \r\n  NEW.\"contact_number\", \r\n  NEW.\"offer_type_id\", \r\n  NEW.\"sales_manager_id\", \r\n  NEW.\"provider_company_id\", \r\n  NEW.\"offer_status_id\", \r\n  NEW.\"license_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFER AFTER UPDATE\r\nON \"portal\".\"offers\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"();");

            // Adjust CompaniesLinkedTechnicalUser
            migrationBuilder.Sql(@"CREATE OR REPLACE VIEW portal.company_linked_technical_users AS
                 SELECT
                     tu.id AS technical_user_id,
                     i.company_id AS owners,
                     CASE
                         WHEN tu.offer_subscription_id IS NOT NULL THEN o.provider_company_id
                         WHEN EXISTS (SELECT 1 FROM portal.connectors cs WHERE cs.technical_user_id = tu.id) THEN c.host_id
                         END AS provider
                 FROM portal.technical_users tu
                     JOIN portal.identities i ON tu.id = i.id
                     LEFT JOIN portal.offer_subscriptions os ON tu.offer_subscription_id = os.id
                     LEFT JOIN portal.offers o ON os.offer_id = o.id
                     LEFT JOIN portal.connectors c ON tu.id = c.technical_user_id
                 WHERE tu.technical_user_type_id = 1 AND i.identity_type_id = 2
                 UNION
                 SELECT
                     tu.id AS technical_user_id,
                     i.company_id AS owners,
                     null AS provider
                 FROM
                     portal.technical_users tu
                         JOIN portal.identities i ON tu.id = i.id
                 WHERE tu.technical_user_type_id = 2
                 UNION
                 SELECT
                     tu.id AS technical_user_id,
                     o.provider_company_id AS owners,
                     o.provider_company_id AS provider
                 FROM
                     portal.technical_users tu
                         JOIN portal.identities i ON tu.id = i.id
                         LEFT JOIN portal.offer_subscriptions os ON tu.offer_subscription_id = os.id
                         LEFT JOIN portal.offers o ON os.offer_id = o.id
                 WHERE tu.technical_user_type_id = 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"() CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_process_steps_process_step_statuses_process_step_status_id",
                schema: "portal",
                table: "process_steps");

            migrationBuilder.DropTable(
                name: "agreement_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_connector20250113",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_offer20250121",
                schema: "portal");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type",
                keyColumn: "id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "notification_type",
                keyColumn: "id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 804);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 805);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 806);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 807);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "technical_user_types",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "sd_skipped_date",
                schema: "portal",
                table: "connectors");

            migrationBuilder.AddColumn<bool>(
                name: "display_technical_user",
                schema: "portal",
                table: "offers",
                type: "boolean",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "identity_type",
                keyColumn: "id",
                keyValue: 2,
                column: "label",
                value: "COMPANY_SERVICE_ACCOUNT");

            migrationBuilder.AddForeignKey(
                name: "fk_process_steps_process_step_statuses_process_step_status_id",
                schema: "portal",
                table: "process_steps",
                column: "process_step_status_id",
                principalSchema: "portal",
                principalTable: "process_step_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20241008\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"technical_user_id\", \"sd_creation_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"technical_user_id\", \r\n  NEW.\"sd_creation_process_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20241008\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"technical_user_id\", \"sd_creation_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"technical_user_id\", \r\n  NEW.\"sd_creation_process_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFER$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer20241219\" (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"display_technical_user\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_released\", \r\n  NEW.\"marketing_url\", \r\n  NEW.\"contact_email\", \r\n  NEW.\"contact_number\", \r\n  NEW.\"display_technical_user\", \r\n  NEW.\"offer_type_id\", \r\n  NEW.\"sales_manager_id\", \r\n  NEW.\"provider_company_id\", \r\n  NEW.\"offer_status_id\", \r\n  NEW.\"license_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFER AFTER INSERT\r\nON \"portal\".\"offers\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFER\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFER$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer20241219\" (\"id\", \"name\", \"date_created\", \"date_released\", \"marketing_url\", \"contact_email\", \"contact_number\", \"display_technical_user\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"license_type_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"date_created\", \r\n  NEW.\"date_released\", \r\n  NEW.\"marketing_url\", \r\n  NEW.\"contact_email\", \r\n  NEW.\"contact_number\", \r\n  NEW.\"display_technical_user\", \r\n  NEW.\"offer_type_id\", \r\n  NEW.\"sales_manager_id\", \r\n  NEW.\"provider_company_id\", \r\n  NEW.\"offer_status_id\", \r\n  NEW.\"license_type_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFER AFTER UPDATE\r\nON \"portal\".\"offers\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFER\"();");

            // Revert adjusted changes in CompaniesLinkedTechnicalUser
            migrationBuilder.Sql(@"CREATE OR REPLACE VIEW portal.company_linked_technical_users AS
                 SELECT
                     tu.id AS technical_user_id,
                     i.company_id AS owners,
                     CASE
                         WHEN tu.offer_subscription_id IS NOT NULL THEN o.provider_company_id
                         WHEN EXISTS (SELECT 1 FROM portal.connectors cs WHERE cs.technical_user_id = tu.id) THEN c.host_id
                         END AS provider
                 FROM portal.technical_users tu
                     JOIN portal.identities i ON tu.id = i.id
                     LEFT JOIN portal.offer_subscriptions os ON tu.offer_subscription_id = os.id
                     LEFT JOIN portal.offers o ON os.offer_id = o.id
                     LEFT JOIN portal.connectors c ON tu.id = c.technical_user_id
                 WHERE tu.technical_user_type_id = 1 AND i.identity_type_id = 2
                 UNION
                 SELECT
                     tu.id AS technical_user_id,
                     i.company_id AS owners,
                     null AS provider
                 FROM
                     portal.technical_users tu
                         JOIN portal.identities i ON tu.id = i.id
                 WHERE tu.technical_user_type_id = 2
              ");
        }
    }
}
