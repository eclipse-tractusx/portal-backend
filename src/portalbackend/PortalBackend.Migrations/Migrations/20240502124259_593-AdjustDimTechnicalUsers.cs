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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _593AdjustDimTechnicalUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_USERROLE\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_USERROLE\"() CASCADE;");

            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.AddColumn<int>(
                name: "provider_information_id",
                schema: "portal",
                table: "user_roles",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "company_service_account_kind_id",
                schema: "portal",
                table: "company_service_accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "audit_user_role20240502",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role = table.Column<string>(type: "text", nullable: true),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    provider_information_id = table.Column<int>(type: "integer", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_user_role20240502", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "provider_informations",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    auth_url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provider_informations", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "provider_informations",
                columns: new[] { "id", "auth_url", "name" },
                values: new object[] { 1, "https://example.org/oauth/token", "Keycloak" });

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_provider_information_id",
                schema: "portal",
                table: "user_roles",
                column: "provider_information_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "client_client_id",
                filter: "client_client_id is not null AND company_service_account_kind_id = 1");

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_provider_informations_provider_information_id",
                schema: "portal",
                table: "user_roles",
                column: "provider_information_id",
                principalSchema: "portal",
                principalTable: "provider_informations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_USERROLE\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_USERROLE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_user_role20240502\" (\"id\", \"user_role\", \"offer_id\", \"provider_information_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"user_role\", \r\n  NEW.\"offer_id\", \r\n  NEW.\"provider_information_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_USERROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_USERROLE AFTER INSERT\r\nON \"portal\".\"user_roles\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_USERROLE\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_USERROLE\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_USERROLE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_user_role20240502\" (\"id\", \"user_role\", \"offer_id\", \"provider_information_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"user_role\", \r\n  NEW.\"offer_id\", \r\n  NEW.\"provider_information_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_USERROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_USERROLE AFTER UPDATE\r\nON \"portal\".\"user_roles\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_USERROLE\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_USERROLE\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_USERROLE\"() CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_provider_informations_provider_information_id",
                schema: "portal",
                table: "user_roles");

            migrationBuilder.DropTable(
                name: "audit_user_role20240502",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "provider_informations",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_user_roles_provider_information_id",
                schema: "portal",
                table: "user_roles");

            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "provider_information_id",
                schema: "portal",
                table: "user_roles");

            migrationBuilder.DropColumn(
                name: "company_service_account_kind_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "client_client_id",
                unique: true);

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_USERROLE\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_USERROLE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_user_role20231115\" (\"id\", \"user_role\", \"offer_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"user_role\", \r\n  NEW.\"offer_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_USERROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_USERROLE AFTER INSERT\r\nON \"portal\".\"user_roles\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_USERROLE\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_USERROLE\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_USERROLE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_user_role20231115\" (\"id\", \"user_role\", \"offer_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"user_role\", \r\n  NEW.\"offer_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_USERROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_USERROLE AFTER UPDATE\r\nON \"portal\".\"user_roles\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_USERROLE\"();");
        }
    }
}
