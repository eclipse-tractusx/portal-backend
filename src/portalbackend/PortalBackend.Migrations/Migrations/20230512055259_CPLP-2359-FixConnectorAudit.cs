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
    public partial class CPLP2359FixConnectorAudit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR() CASCADE;");

            migrationBuilder.DropPrimaryKey(
                name: "pk_audit_connector20230425",
                schema: "portal",
                table: "audit_connector20230425");

            migrationBuilder.RenameTable(
                name: "audit_connector20230425",
                schema: "portal",
                newName: "audit_connector20230503",
                newSchema: "portal");

            migrationBuilder.AddPrimaryKey(
                name: "pk_audit_connector20230503",
                schema: "portal",
                table: "audit_connector20230503",
                column: "audit_v1id");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230503 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"daps_registration_successful\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.name, \r\n  OLD.connector_url, \r\n  OLD.type_id, \r\n  OLD.status_id, \r\n  OLD.provider_id, \r\n  OLD.host_id, \r\n  OLD.self_description_document_id, \r\n  OLD.location_id, \r\n  OLD.daps_registration_successful, \r\n  OLD.self_description_message, \r\n  OLD.date_last_changed, \r\n  OLD.company_service_account_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_CONNECTOR AFTER DELETE\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230503 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"daps_registration_successful\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.connector_url, \r\n  NEW.type_id, \r\n  NEW.status_id, \r\n  NEW.provider_id, \r\n  NEW.host_id, \r\n  NEW.self_description_document_id, \r\n  NEW.location_id, \r\n  NEW.daps_registration_successful, \r\n  NEW.self_description_message, \r\n  NEW.date_last_changed, \r\n  NEW.company_service_account_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230503 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"daps_registration_successful\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.connector_url, \r\n  NEW.type_id, \r\n  NEW.status_id, \r\n  NEW.provider_id, \r\n  NEW.host_id, \r\n  NEW.self_description_document_id, \r\n  NEW.location_id, \r\n  NEW.daps_registration_successful, \r\n  NEW.self_description_message, \r\n  NEW.date_last_changed, \r\n  NEW.company_service_account_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR();");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR() CASCADE;");

            migrationBuilder.DropPrimaryKey(
                name: "pk_audit_connector20230503",
                schema: "portal",
                table: "audit_connector20230503");

            migrationBuilder.RenameTable(
                name: "audit_connector20230503",
                schema: "portal",
                newName: "audit_connector20230425",
                newSchema: "portal");

            migrationBuilder.AddPrimaryKey(
                name: "pk_audit_connector20230425",
                schema: "portal",
                table: "audit_connector20230425",
                column: "audit_v1id");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230425 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"daps_registration_successful\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.name, \r\n  OLD.connector_url, \r\n  OLD.type_id, \r\n  OLD.status_id, \r\n  OLD.provider_id, \r\n  OLD.host_id, \r\n  OLD.self_description_document_id, \r\n  OLD.location_id, \r\n  OLD.daps_registration_successful, \r\n  OLD.self_description_message, \r\n  OLD.date_last_changed, \r\n  OLD.company_service_account_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_CONNECTOR AFTER DELETE\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_CONNECTOR();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230425 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"daps_registration_successful\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.connector_url, \r\n  NEW.type_id, \r\n  NEW.status_id, \r\n  NEW.provider_id, \r\n  NEW.host_id, \r\n  NEW.self_description_document_id, \r\n  NEW.location_id, \r\n  NEW.daps_registration_successful, \r\n  NEW.self_description_message, \r\n  NEW.date_last_changed, \r\n  NEW.company_service_account_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_CONNECTOR();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO portal.audit_connector20230425 (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"daps_registration_successful\", \"self_description_message\", \"date_last_changed\", \"company_service_account_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.connector_url, \r\n  NEW.type_id, \r\n  NEW.status_id, \r\n  NEW.provider_id, \r\n  NEW.host_id, \r\n  NEW.self_description_document_id, \r\n  NEW.location_id, \r\n  NEW.daps_registration_successful, \r\n  NEW.self_description_message, \r\n  NEW.date_last_changed, \r\n  NEW.company_service_account_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON portal.connectors\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_CONNECTOR();");
        }
    }
}
