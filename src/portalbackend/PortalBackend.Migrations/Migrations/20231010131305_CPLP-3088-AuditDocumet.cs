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
    /// <inheritdoc />
    public partial class CPLP3088AuditDocumet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "date_last_changed",
                schema: "portal",
                table: "documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "last_editor_id",
                schema: "portal",
                table: "documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_document20231010",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    document_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    document_content = table.Column<byte[]>(type: "bytea", nullable: false),
                    document_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    media_type_id = table.Column<int>(type: "integer", nullable: false),
                    document_type_id = table.Column<int>(type: "integer", nullable: false),
                    document_status_id = table.Column<int>(type: "integer", nullable: false),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_document20231010", x => x.audit_v1id);
                });

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_DOCUMENT\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_DOCUMENT$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_document20231010\" (\"id\", \"date_created\", \"document_hash\", \"document_content\", \"document_name\", \"media_type_id\", \"document_type_id\", \"document_status_id\", \"company_user_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"document_hash\", \r\n  NEW.\"document_content\", \r\n  NEW.\"document_name\", \r\n  NEW.\"media_type_id\", \r\n  NEW.\"document_type_id\", \r\n  NEW.\"document_status_id\", \r\n  NEW.\"company_user_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_DOCUMENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_DOCUMENT AFTER INSERT\r\nON \"portal\".\"documents\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_DOCUMENT\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_DOCUMENT\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_DOCUMENT$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_document20231010\" (\"id\", \"date_created\", \"document_hash\", \"document_content\", \"document_name\", \"media_type_id\", \"document_type_id\", \"document_status_id\", \"company_user_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"document_hash\", \r\n  NEW.\"document_content\", \r\n  NEW.\"document_name\", \r\n  NEW.\"media_type_id\", \r\n  NEW.\"document_type_id\", \r\n  NEW.\"document_status_id\", \r\n  NEW.\"company_user_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_DOCUMENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_DOCUMENT AFTER UPDATE\r\nON \"portal\".\"documents\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_DOCUMENT\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_DOCUMENT\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_DOCUMENT\"() CASCADE;");

            migrationBuilder.DropTable(
                name: "audit_document20231010",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "date_last_changed",
                schema: "portal",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "last_editor_id",
                schema: "portal",
                table: "documents");
        }
    }
}
