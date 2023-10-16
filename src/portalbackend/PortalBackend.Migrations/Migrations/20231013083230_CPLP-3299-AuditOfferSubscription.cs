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
    public partial class CPLP3299AuditOfferSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION\"() CASCADE;");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "date_created",
                schema: "portal",
                table: "offer_subscriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: "1970-01-01 12:00:00");

            migrationBuilder.CreateTable(
                name: "audit_offer_subscription20231013",
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
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_offer_subscription20231013", x => x.audit_v1id);
                });

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer_subscription20231013\" (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"process_id\", \"date_created\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"offer_id\", \r\n  NEW.\"offer_subscription_status_id\", \r\n  NEW.\"display_name\", \r\n  NEW.\"description\", \r\n  NEW.\"requester_id\", \r\n  NEW.\"last_editor_id\", \r\n  NEW.\"process_id\", \r\n  NEW.\"date_created\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION AFTER INSERT\r\nON \"portal\".\"offer_subscriptions\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer_subscription20231013\" (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"process_id\", \"date_created\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"offer_id\", \r\n  NEW.\"offer_subscription_status_id\", \r\n  NEW.\"display_name\", \r\n  NEW.\"description\", \r\n  NEW.\"requester_id\", \r\n  NEW.\"last_editor_id\", \r\n  NEW.\"process_id\", \r\n  NEW.\"date_created\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION AFTER UPDATE\r\nON \"portal\".\"offer_subscriptions\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION\"() CASCADE;");

            migrationBuilder.DropTable(
                name: "audit_offer_subscription20231013",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "date_created",
                schema: "portal",
                table: "offer_subscriptions");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer_subscription20230317\" (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"process_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"offer_id\", \r\n  NEW.\"offer_subscription_status_id\", \r\n  NEW.\"display_name\", \r\n  NEW.\"description\", \r\n  NEW.\"requester_id\", \r\n  NEW.\"last_editor_id\", \r\n  NEW.\"process_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION AFTER INSERT\r\nON \"portal\".\"offer_subscriptions\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO \"portal\".\"audit_offer_subscription20230317\" (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"process_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"company_id\", \r\n  NEW.\"offer_id\", \r\n  NEW.\"offer_subscription_status_id\", \r\n  NEW.\"display_name\", \r\n  NEW.\"description\", \r\n  NEW.\"requester_id\", \r\n  NEW.\"last_editor_id\", \r\n  NEW.\"process_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION AFTER UPDATE\r\nON \"portal\".\"offer_subscriptions\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"portal\".\"LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION\"();");
        }
    }
}
