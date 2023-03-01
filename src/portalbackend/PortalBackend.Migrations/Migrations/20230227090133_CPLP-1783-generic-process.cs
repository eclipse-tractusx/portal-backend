﻿/********************************************************************************
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

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1783genericprocess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION() CASCADE;");

            migrationBuilder.AddColumn<Guid>(
                name: "process_id",
                schema: "portal",
                table: "process_steps",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "checklist_process_id",
                schema: "portal",
                table: "company_applications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_company_application20230214",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    application_status_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    checklist_process_id = table.Column<Guid>(type: "uuid", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_application20230214", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "process_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "processes",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_type_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processes", x => x.id);
                    table.ForeignKey(
                        name: "fk_processes_process_types_process_type_id",
                        column: x => x.process_type_id,
                        principalSchema: "portal",
                        principalTable: "process_types",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 1, "APPLICATION_CHECKLIST" });

            migrationBuilder.Sql("UPDATE portal.company_applications AS applications SET checklist_process_id = gen_random_uuid() FROM ( SELECT DISTINCT company_application_id FROM portal.application_assigned_process_steps) AS subquery WHERE applications.id = subquery.company_application_id;");
            migrationBuilder.Sql("INSERT INTO portal.processes (id, process_type_id) SELECT checklist_process_id, 1 FROM portal.company_applications WHERE checklist_process_id IS NOT NULL;");
            migrationBuilder.Sql("UPDATE portal.process_steps AS steps SET process_id = subquery.checklist_process_id FROM ( SELECT applications.checklist_process_id, assigned.process_step_id FROM portal.company_applications AS applications JOIN portal.application_assigned_process_steps AS assigned ON applications.id = assigned.company_application_id WHERE applications.checklist_process_id IS NOT NULL) AS subquery WHERE steps.id = subquery.process_step_id;");

            migrationBuilder.DropTable(
                name: "application_assigned_process_steps",
                schema: "portal");

            migrationBuilder.CreateIndex(
                name: "ix_process_steps_process_id",
                schema: "portal",
                table: "process_steps",
                column: "process_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_applications_checklist_process_id",
                schema: "portal",
                table: "company_applications",
                column: "checklist_process_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_processes_process_type_id",
                schema: "portal",
                table: "processes",
                column: "process_type_id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_applications_processes_checklist_process_id",
                schema: "portal",
                table: "company_applications",
                column: "checklist_process_id",
                principalSchema: "portal",
                principalTable: "processes",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_process_steps_processes_process_id",
                schema: "portal",
                table: "process_steps",
                column: "process_id",
                principalSchema: "portal",
                principalTable: "processes",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application20230214 (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"checklist_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.date_last_changed, \r\n  OLD.application_status_id, \r\n  OLD.company_id, \r\n  OLD.checklist_process_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION AFTER DELETE\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application20230214 (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"checklist_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.date_last_changed, \r\n  NEW.application_status_id, \r\n  NEW.company_id, \r\n  NEW.checklist_process_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION AFTER INSERT\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application20230214 (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"checklist_process_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.date_last_changed, \r\n  NEW.application_status_id, \r\n  NEW.company_id, \r\n  NEW.checklist_process_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION AFTER UPDATE\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION();");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION() CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_company_applications_processes_checklist_process_id",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.DropForeignKey(
                name: "fk_process_steps_processes_process_id",
                schema: "portal",
                table: "process_steps");

            migrationBuilder.DropTable(
                name: "audit_company_application20230214",
                schema: "portal");

            migrationBuilder.CreateTable(
                name: "application_assigned_process_steps",
                schema: "portal",
                columns: table => new
                {
                    company_application_id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_step_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_assigned_process_steps", x => new { x.company_application_id, x.process_step_id });
                    table.ForeignKey(
                        name: "fk_application_assigned_process_steps_company_applications_com",
                        column: x => x.company_application_id,
                        principalSchema: "portal",
                        principalTable: "company_applications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_application_assigned_process_steps_process_steps_process_st",
                        column: x => x.process_step_id,
                        principalSchema: "portal",
                        principalTable: "process_steps",
                        principalColumn: "id");
                });

            migrationBuilder.Sql("INSERT INTO portal.application_assigned_process_steps (company_application_id, process_step_id) SELECT applications.id,steps.id FROM portal.company_applications AS applications JOIN portal.process_steps AS steps ON applications.checklist_process_id = steps.process_id;");

            migrationBuilder.DropTable(
                name: "processes",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "process_types",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_process_steps_process_id",
                schema: "portal",
                table: "process_steps");

            migrationBuilder.DropIndex(
                name: "ix_company_applications_checklist_process_id",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.DropColumn(
                name: "process_id",
                schema: "portal",
                table: "process_steps");

            migrationBuilder.DropColumn(
                name: "checklist_process_id",
                schema: "portal",
                table: "company_applications");

            migrationBuilder.CreateIndex(
                name: "ix_application_assigned_process_steps_process_step_id",
                schema: "portal",
                table: "application_assigned_process_steps",
                column: "process_step_id",
                unique: true);

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application20221005 (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.date_last_changed, \r\n  OLD.application_status_id, \r\n  OLD.company_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION AFTER DELETE\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application20221005 (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.date_last_changed, \r\n  NEW.application_status_id, \r\n  NEW.company_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION AFTER INSERT\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application20221005 (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.date_last_changed, \r\n  NEW.application_status_id, \r\n  NEW.company_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION AFTER UPDATE\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION();");
        }
    }
}
