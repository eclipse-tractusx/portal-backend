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
    public partial class CPLP3105RemoveDaps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR() CASCADE;");

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

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230803 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.name, \r\n  OLD.connector_url, \r\n  OLD.type_id, \r\n  OLD.status_id, \r\n  OLD.provider_id, \r\n  OLD.host_id, \r\n  OLD.self_description_document_id, \r\n  OLD.location_id, \r\n  OLD.self_description_message, \r\n  OLD.date_last_changed, \r\n  OLD.company_service_account_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_CONNECTOR AFTER DELETE\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230803 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.connector_url, \r\n  NEW.type_id, \r\n  NEW.status_id, \r\n  NEW.provider_id, \r\n  NEW.host_id, \r\n  NEW.self_description_document_id, \r\n  NEW.location_id, \r\n  NEW.self_description_message, \r\n  NEW.date_last_changed, \r\n  NEW.company_service_account_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230803 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.connector_url, \r\n  NEW.type_id, \r\n  NEW.status_id, \r\n  NEW.provider_id, \r\n  NEW.host_id, \r\n  NEW.self_description_document_id, \r\n  NEW.location_id, \r\n  NEW.self_description_message, \r\n  NEW.date_last_changed, \r\n  NEW.company_service_account_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR();");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR() CASCADE;");

            migrationBuilder.DropTable(
                name: "audit_connector20230803",
                schema: "portal");

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

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230503 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"daps_registration_successful\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.name, \r\n  OLD.connector_url, \r\n  OLD.type_id, \r\n  OLD.status_id, \r\n  OLD.provider_id, \r\n  OLD.host_id, \r\n  OLD.self_description_document_id, \r\n  OLD.location_id, \r\n  OLD.daps_registration_successful, \r\n  OLD.self_description_message, \r\n  OLD.date_last_changed, \r\n  OLD.company_service_account_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_CONNECTOR AFTER DELETE\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230503 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"daps_registration_successful\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.connector_url, \r\n  NEW.type_id, \r\n  NEW.status_id, \r\n  NEW.provider_id, \r\n  NEW.host_id, \r\n  NEW.self_description_document_id, \r\n  NEW.location_id, \r\n  NEW.daps_registration_successful, \r\n  NEW.self_description_message, \r\n  NEW.date_last_changed, \r\n  NEW.company_service_account_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230503 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"daps_registration_successful\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.connector_url, \r\n  NEW.type_id, \r\n  NEW.status_id, \r\n  NEW.provider_id, \r\n  NEW.host_id, \r\n  NEW.self_description_document_id, \r\n  NEW.location_id, \r\n  NEW.daps_registration_successful, \r\n  NEW.self_description_message, \r\n  NEW.date_last_changed, \r\n  NEW.company_service_account_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR();");
        }
    }
}
