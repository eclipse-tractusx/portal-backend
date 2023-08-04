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

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class _160rc5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR() CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_company_users_creator_id",
                schema: "portal",
                table: "notifications");

            migrationBuilder.DropTable(
                name: "connector_client_details",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "daps_registration_successful",
                schema: "portal",
                table: "connectors");

            migrationBuilder.CreateTable(
                name: "audit_connector20230803",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    connector_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    host_id = table.Column<Guid>(type: "uuid", nullable: true),
                    self_description_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    location_id = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    self_description_message = table.Column<string>(type: "text", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    company_service_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_connector20230803", x => x.audit_v1id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_last_editor_id",
                schema: "portal",
                table: "user_roles",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_provider_company_details_last_editor_id",
                schema: "portal",
                table: "provider_company_details",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_offers_last_editor_id",
                schema: "portal",
                table: "offers",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_last_editor_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_assigned_roles_last_editor_id",
                schema: "portal",
                table: "identity_assigned_roles",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_identities_last_editor_id",
                schema: "portal",
                table: "identities",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_consents_last_editor_id",
                schema: "portal",
                table: "consents",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_users_last_editor_id",
                schema: "portal",
                table: "company_users",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_last_editor_id",
                schema: "portal",
                table: "company_ssi_details",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_roles_last_editor_id",
                schema: "portal",
                table: "company_assigned_roles",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_applications_last_editor_id",
                schema: "portal",
                table: "company_applications",
                column: "last_editor_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_subscription_details_last_editor_id",
                schema: "portal",
                table: "app_subscription_details",
                column: "last_editor_id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_subscription_details_identities_last_editor_id",
                schema: "portal",
                table: "app_subscription_details",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_applications_identities_last_editor_id",
                schema: "portal",
                table: "company_applications",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_assigned_roles_identities_last_editor_id",
                schema: "portal",
                table: "company_assigned_roles",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_ssi_details_identities_last_editor_id",
                schema: "portal",
                table: "company_ssi_details",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_users_identities_last_editor_id",
                schema: "portal",
                table: "company_users",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_consents_identities_last_editor_id",
                schema: "portal",
                table: "consents",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_identities_identities_last_editor_id",
                schema: "portal",
                table: "identities",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_identity_assigned_roles_identities_last_editor_id",
                schema: "portal",
                table: "identity_assigned_roles",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_identities_creator_id",
                schema: "portal",
                table: "notifications",
                column: "creator_user_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_offer_subscriptions_identities_last_editor_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_offers_identities_last_editor_id",
                schema: "portal",
                table: "offers",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_provider_company_details_identities_last_editor_id",
                schema: "portal",
                table: "provider_company_details",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_identities_last_editor_id",
                schema: "portal",
                table: "user_roles",
                column: "last_editor_id",
                principalSchema: "portal",
                principalTable: "identities",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230803 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.name, \r\n  OLD.connector_url, \r\n  OLD.type_id, \r\n  OLD.status_id, \r\n  OLD.provider_id, \r\n  OLD.host_id, \r\n  OLD.self_description_document_id, \r\n  OLD.location_id, \r\n  OLD.self_description_message, \r\n  OLD.date_last_changed, \r\n  OLD.company_service_account_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_CONNECTOR AFTER DELETE\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230803 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.connector_url, \r\n  NEW.type_id, \r\n  NEW.status_id, \r\n  NEW.provider_id, \r\n  NEW.host_id, \r\n  NEW.self_description_document_id, \r\n  NEW.location_id, \r\n  NEW.self_description_message, \r\n  NEW.date_last_changed, \r\n  NEW.company_service_account_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230803 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.connector_url, \r\n  NEW.type_id, \r\n  NEW.status_id, \r\n  NEW.provider_id, \r\n  NEW.host_id, \r\n  NEW.self_description_document_id, \r\n  NEW.location_id, \r\n  NEW.self_description_message, \r\n  NEW.date_last_changed, \r\n  NEW.company_service_account_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR();");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR() CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_app_subscription_details_identities_last_editor_id",
                schema: "portal",
                table: "app_subscription_details");

            migrationBuilder.DropForeignKey(
                name: "fk_company_applications_identities_last_editor_id",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.DropForeignKey(
                name: "fk_company_assigned_roles_identities_last_editor_id",
                schema: "portal",
                table: "company_assigned_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_company_ssi_details_identities_last_editor_id",
                schema: "portal",
                table: "company_ssi_details");

            migrationBuilder.DropForeignKey(
                name: "fk_company_users_identities_last_editor_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropForeignKey(
                name: "fk_consents_identities_last_editor_id",
                schema: "portal",
                table: "consents");

            migrationBuilder.DropForeignKey(
                name: "fk_identities_identities_last_editor_id",
                schema: "portal",
                table: "identities");

            migrationBuilder.DropForeignKey(
                name: "fk_identity_assigned_roles_identities_last_editor_id",
                schema: "portal",
                table: "identity_assigned_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_identities_creator_id",
                schema: "portal",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_offer_subscriptions_identities_last_editor_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_offers_identities_last_editor_id",
                schema: "portal",
                table: "offers");

            migrationBuilder.DropForeignKey(
                name: "fk_provider_company_details_identities_last_editor_id",
                schema: "portal",
                table: "provider_company_details");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_identities_last_editor_id",
                schema: "portal",
                table: "user_roles");

            migrationBuilder.DropTable(
                name: "audit_connector20230803",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_user_roles_last_editor_id",
                schema: "portal",
                table: "user_roles");

            migrationBuilder.DropIndex(
                name: "ix_provider_company_details_last_editor_id",
                schema: "portal",
                table: "provider_company_details");

            migrationBuilder.DropIndex(
                name: "ix_offers_last_editor_id",
                schema: "portal",
                table: "offers");

            migrationBuilder.DropIndex(
                name: "ix_offer_subscriptions_last_editor_id",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.DropIndex(
                name: "ix_identity_assigned_roles_last_editor_id",
                schema: "portal",
                table: "identity_assigned_roles");

            migrationBuilder.DropIndex(
                name: "ix_identities_last_editor_id",
                schema: "portal",
                table: "identities");

            migrationBuilder.DropIndex(
                name: "ix_consents_last_editor_id",
                schema: "portal",
                table: "consents");

            migrationBuilder.DropIndex(
                name: "ix_company_users_last_editor_id",
                schema: "portal",
                table: "company_users");

            migrationBuilder.DropIndex(
                name: "ix_company_ssi_details_last_editor_id",
                schema: "portal",
                table: "company_ssi_details");

            migrationBuilder.DropIndex(
                name: "ix_company_assigned_roles_last_editor_id",
                schema: "portal",
                table: "company_assigned_roles");

            migrationBuilder.DropIndex(
                name: "ix_company_applications_last_editor_id",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.DropIndex(
                name: "ix_app_subscription_details_last_editor_id",
                schema: "portal",
                table: "app_subscription_details");

            migrationBuilder.AddColumn<bool>(
                name: "daps_registration_successful",
                schema: "portal",
                table: "connectors",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "connector_client_details",
                schema: "portal",
                columns: table => new
                {
                    connector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connector_client_details", x => x.connector_id);
                    table.ForeignKey(
                        name: "fk_connector_client_details_connectors_connector_id",
                        column: x => x.connector_id,
                        principalSchema: "portal",
                        principalTable: "connectors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_company_users_creator_id",
                schema: "portal",
                table: "notifications",
                column: "creator_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230503 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"daps_registration_successful\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.name, \r\n  OLD.connector_url, \r\n  OLD.type_id, \r\n  OLD.status_id, \r\n  OLD.provider_id, \r\n  OLD.host_id, \r\n  OLD.self_description_document_id, \r\n  OLD.location_id, \r\n  OLD.daps_registration_successful, \r\n  OLD.self_description_message, \r\n  OLD.date_last_changed, \r\n  OLD.company_service_account_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_CONNECTOR AFTER DELETE\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230503 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"daps_registration_successful\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.connector_url, \r\n  NEW.type_id, \r\n  NEW.status_id, \r\n  NEW.provider_id, \r\n  NEW.host_id, \r\n  NEW.self_description_document_id, \r\n  NEW.location_id, \r\n  NEW.daps_registration_successful, \r\n  NEW.self_description_message, \r\n  NEW.date_last_changed, \r\n  NEW.company_service_account_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230503 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"daps_registration_successful\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.connector_url, \r\n  NEW.type_id, \r\n  NEW.status_id, \r\n  NEW.provider_id, \r\n  NEW.host_id, \r\n  NEW.self_description_document_id, \r\n  NEW.location_id, \r\n  NEW.daps_registration_successful, \r\n  NEW.self_description_message, \r\n  NEW.date_last_changed, \r\n  NEW.company_service_account_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR();");
        }
    }
}
