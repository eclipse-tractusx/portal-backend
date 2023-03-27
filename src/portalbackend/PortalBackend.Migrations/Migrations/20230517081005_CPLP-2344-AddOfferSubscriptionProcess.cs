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
    public partial class CPLP2344AddOfferSubscriptionProcess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION() CASCADE;");

            migrationBuilder.AddColumn<string>(
                name: "auto_setup_callback_url",
                schema: "portal",
                table: "provider_company_details",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "process_id",
                schema: "portal",
                table: "offer_subscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "app_instance_setup_id",
                schema: "portal",
                table: "iam_service_accounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "disabled",
                schema: "portal",
                table: "iam_clients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "audit_offer_subscription20230317",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_subscription_status_id = table.Column<int>(type: "integer", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    process_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_offer_subscription20230317", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "offer_subscriptions_process_datas",
                schema: "portal",
                columns: table => new
                {
                    offer_subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_subscriptions_process_datas", x => x.offer_subscription_id);
                    table.ForeignKey(
                        name: "fk_offer_subscriptions_process_datas_offer_subscriptions_offer",
                        column: x => x.offer_subscription_id,
                        principalSchema: "portal",
                        principalTable: "offer_subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 100, "TRIGGER_PROVIDER" },
                    { 101, "START_AUTOSETUP" },
                    { 102, "OFFERSUBSCRIPTION_CLIENT_CREATION" },
                    { 103, "SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION" },
                    { 104, "OFFERSUBSCRIPTION_TECHNICALUSER_CREATION" },
                    { 105, "ACTIVATE_SUBSCRIPTION" },
                    { 106, "TRIGGER_PROVIDER_CALLBACK" },
                    { 107, "RETRIGGER_PROVIDER" },
                    { 108, "RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION" },
                    { 109, "RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION" },
                    { 110, "RETRIGGER_PROVIDER_CALLBACK" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 3, "OFFER_SUBSCRIPTION" });

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_process_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "process_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_iam_service_accounts_app_instance_setup_id",
                schema: "portal",
                table: "iam_service_accounts",
                column: "app_instance_setup_id");

            migrationBuilder.AddForeignKey(
                name: "fk_iam_service_accounts_app_instance_setups_app_instance_setup",
                schema: "portal",
                table: "iam_service_accounts",
                column: "app_instance_setup_id",
                principalSchema: "portal",
                principalTable: "app_instance_setups",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_offer_subscriptions_processes_process_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "process_id",
                principalSchema: "portal",
                principalTable: "processes",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription20230317 (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"process_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.company_id, \r\n  OLD.offer_id, \r\n  OLD.offer_subscription_status_id, \r\n  OLD.display_name, \r\n  OLD.description, \r\n  OLD.requester_id, \r\n  OLD.last_editor_id, \r\n  OLD.process_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION AFTER DELETE\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription20230317 (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"process_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.company_id, \r\n  NEW.offer_id, \r\n  NEW.offer_subscription_status_id, \r\n  NEW.display_name, \r\n  NEW.description, \r\n  NEW.requester_id, \r\n  NEW.last_editor_id, \r\n  NEW.process_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION AFTER INSERT\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription20230317 (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"process_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.company_id, \r\n  NEW.offer_id, \r\n  NEW.offer_subscription_status_id, \r\n  NEW.display_name, \r\n  NEW.description, \r\n  NEW.requester_id, \r\n  NEW.last_editor_id, \r\n  NEW.process_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION AFTER UPDATE\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION();");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION() CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_iam_service_accounts_app_instance_setups_app_instance_setup",
                schema: "portal",
                table: "iam_service_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_offer_subscriptions_processes_process_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropTable(
                name: "audit_offer_subscription20230317",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_subscriptions_process_datas",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_offer_subscriptions_process_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropIndex(
                name: "ix_iam_service_accounts_app_instance_setup_id",
                schema: "portal",
                table: "iam_service_accounts");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 110);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_types",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "auto_setup_callback_url",
                schema: "portal",
                table: "provider_company_details");

            migrationBuilder.DropColumn(
                name: "process_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropColumn(
                name: "app_instance_setup_id",
                schema: "portal",
                table: "iam_service_accounts");

            migrationBuilder.DropColumn(
                name: "disabled",
                schema: "portal",
                table: "iam_clients");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription20221005 (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.company_id, \r\n  OLD.offer_id, \r\n  OLD.offer_subscription_status_id, \r\n  OLD.display_name, \r\n  OLD.description, \r\n  OLD.requester_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION AFTER DELETE\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription20221005 (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.company_id, \r\n  NEW.offer_id, \r\n  NEW.offer_subscription_status_id, \r\n  NEW.display_name, \r\n  NEW.description, \r\n  NEW.requester_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION AFTER INSERT\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription20221005 (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.company_id, \r\n  NEW.offer_id, \r\n  NEW.offer_subscription_status_id, \r\n  NEW.display_name, \r\n  NEW.description, \r\n  NEW.requester_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION AFTER UPDATE\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION();");
        }
    }
}
