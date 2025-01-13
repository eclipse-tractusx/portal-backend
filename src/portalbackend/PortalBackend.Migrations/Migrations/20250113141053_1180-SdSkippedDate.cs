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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _1180SdSkippedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() CASCADE;");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "sd_skipped_date",
                schema: "portal",
                table: "connectors",
                type: "timestamp with time zone",
                nullable: true);

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

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20250113\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"technical_user_id\", \"sd_creation_process_id\", \"sd_skipped_date\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"technical_user_id\", \r\n  NEW.\"sd_creation_process_id\", \r\n  NEW.\"sd_skipped_date\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20250113\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"technical_user_id\", \"sd_creation_process_id\", \"sd_skipped_date\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"technical_user_id\", \r\n  NEW.\"sd_creation_process_id\", \r\n  NEW.\"sd_skipped_date\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() CASCADE;");

            migrationBuilder.DropTable(
                name: "audit_connector20250113",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "sd_skipped_date",
                schema: "portal",
                table: "connectors");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20241008\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"technical_user_id\", \"sd_creation_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"technical_user_id\", \r\n  NEW.\"sd_creation_process_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_CONNECTOR AFTER INSERT\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_CONNECTOR\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_CONNECTOR$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_connector20241008\" (\"id\", \"name\", \"connector_url\", \"type_id\", \"status_id\", \"provider_id\", \"host_id\", \"self_description_document_id\", \"location_id\", \"self_description_message\", \"date_last_changed\", \"technical_user_id\", \"sd_creation_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"name\", \r\n  NEW.\"connector_url\", \r\n  NEW.\"type_id\", \r\n  NEW.\"status_id\", \r\n  NEW.\"provider_id\", \r\n  NEW.\"host_id\", \r\n  NEW.\"self_description_document_id\", \r\n  NEW.\"location_id\", \r\n  NEW.\"self_description_message\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"technical_user_id\", \r\n  NEW.\"sd_creation_process_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_CONNECTOR$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_CONNECTOR AFTER UPDATE\r\nON \"portal\".\"connectors\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_CONNECTOR\"();");
        }
    }
}
