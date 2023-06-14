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
    public partial class CPLP2805AuditProviderCompanyDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "date_last_changed",
                schema: "portal",
                table: "provider_company_details",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "last_editor_id",
                schema: "portal",
                table: "provider_company_details",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_provider_company_detail20230614",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    auto_setup_url = table.Column<string>(type: "text", nullable: false),
                    auto_setup_callback_url = table.Column<string>(type: "text", nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_provider_company_detail20230614", x => x.audit_v1id);
                });

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_provider_company_detail20230614 (\"id\", \"date_created\", \"auto_setup_url\", \"auto_setup_callback_url\", \"company_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.auto_setup_url, \r\n  OLD.auto_setup_callback_url, \r\n  OLD.company_id, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL AFTER DELETE\r\nON portal.provider_company_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_provider_company_detail20230614 (\"id\", \"date_created\", \"auto_setup_url\", \"auto_setup_callback_url\", \"company_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.auto_setup_url, \r\n  NEW.auto_setup_callback_url, \r\n  NEW.company_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL AFTER INSERT\r\nON portal.provider_company_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_provider_company_detail20230614 (\"id\", \"date_created\", \"auto_setup_url\", \"auto_setup_callback_url\", \"company_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.auto_setup_url, \r\n  NEW.auto_setup_callback_url, \r\n  NEW.company_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL AFTER UPDATE\r\nON portal.provider_company_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL();");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_PROVIDERCOMPANYDETAIL() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_PROVIDERCOMPANYDETAIL() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_PROVIDERCOMPANYDETAIL() CASCADE;");

            migrationBuilder.DropTable(
                name: "audit_provider_company_detail20230614",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "date_last_changed",
                schema: "portal",
                table: "provider_company_details");

            migrationBuilder.DropColumn(
                name: "last_editor_id",
                schema: "portal",
                table: "provider_company_details");
        }
    }
}
