/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1318AuditAppSubscription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "last_editor_id",
                schema: "portal",
                table: "app_subscription_details",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_app_subscription_detail20221118",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    app_subscription_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_app_subscription_detail20221118", x => x.audit_v1id);
                });

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_app_subscription_detail20221118 (\"id\", \"offer_subscription_id\", \"app_instance_id\", \"app_subscription_url\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.offer_subscription_id, \r\n  OLD.app_instance_id, \r\n  OLD.app_subscription_url, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL AFTER DELETE\r\nON portal.app_subscription_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_app_subscription_detail20221118 (\"id\", \"offer_subscription_id\", \"app_instance_id\", \"app_subscription_url\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.offer_subscription_id, \r\n  NEW.app_instance_id, \r\n  NEW.app_subscription_url, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL AFTER INSERT\r\nON portal.app_subscription_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_app_subscription_detail20221118 (\"id\", \"offer_subscription_id\", \"app_instance_id\", \"app_subscription_url\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.offer_subscription_id, \r\n  NEW.app_instance_id, \r\n  NEW.app_subscription_url, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL AFTER UPDATE\r\nON portal.app_subscription_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL();");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL() CASCADE;");

            migrationBuilder.DropTable(
                name: "audit_app_subscription_detail20221118",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "last_editor_id",
                schema: "portal",
                table: "app_subscription_details");
        }
    }
}
