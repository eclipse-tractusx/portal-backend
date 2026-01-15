/********************************************************************************
 * Copyright (c) 2026 Contributors to the Eclipse Foundation
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
    public partial class _1444revertappmultisubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE\"() CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_identity_assigned_roles_offer_subscriptions_offer_subscript",
                schema: "portal",
                table: "identity_assigned_roles");

            migrationBuilder.DropTable(
                name: "audit_identity_assigned_role20250929",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_identity_assigned_roles_offer_subscription_id",
                schema: "portal",
                table: "identity_assigned_roles");

            migrationBuilder.DropColumn(
                name: "offer_subscription_id",
                schema: "portal",
                table: "identity_assigned_roles");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_identity_assigned_role20230522\" (\"identity_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"identity_id\", \r\n  NEW.\"user_role_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE AFTER INSERT\r\nON \"portal\".\"identity_assigned_roles\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_identity_assigned_role20230522\" (\"identity_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"identity_id\", \r\n  NEW.\"user_role_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE AFTER UPDATE\r\nON \"portal\".\"identity_assigned_roles\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE\"() CASCADE;");

            migrationBuilder.AddColumn<Guid>(
                name: "offer_subscription_id",
                schema: "portal",
                table: "identity_assigned_roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_identity_assigned_role20250929",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    identity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    offer_subscription_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_identity_assigned_role20250929", x => x.audit_v1id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_identity_assigned_roles_offer_subscription_id",
                schema: "portal",
                table: "identity_assigned_roles",
                column: "offer_subscription_id");

            migrationBuilder.AddForeignKey(
                name: "fk_identity_assigned_roles_offer_subscriptions_offer_subscript",
                schema: "portal",
                table: "identity_assigned_roles",
                column: "offer_subscription_id",
                principalSchema: "portal",
                principalTable: "offer_subscriptions",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_identity_assigned_role20250929\" (\"identity_id\", \"user_role_id\", \"offer_subscription_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"identity_id\", \r\n  NEW.\"user_role_id\", \r\n  NEW.\"offer_subscription_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE AFTER INSERT\r\nON \"portal\".\"identity_assigned_roles\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_IDENTITYASSIGNEDROLE\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_identity_assigned_role20250929\" (\"identity_id\", \"user_role_id\", \"offer_subscription_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"identity_id\", \r\n  NEW.\"user_role_id\", \r\n  NEW.\"offer_subscription_id\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE AFTER UPDATE\r\nON \"portal\".\"identity_assigned_roles\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_IDENTITYASSIGNEDROLE\"();");
        }
    }
}
