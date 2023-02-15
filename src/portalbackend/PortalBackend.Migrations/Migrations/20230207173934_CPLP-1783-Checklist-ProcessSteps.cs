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

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1783ChecklistProcessSteps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "process_step_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_step_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "process_step_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_step_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "process_steps",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_step_type_id = table.Column<int>(type: "integer", nullable: false),
                    process_step_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_steps", x => x.id);
                    table.ForeignKey(
                        name: "fk_process_steps_process_step_statuses_process_step_status_id",
                        column: x => x.process_step_status_id,
                        principalSchema: "portal",
                        principalTable: "process_step_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_process_steps_process_step_types_process_step_type_id",
                        column: x => x.process_step_type_id,
                        principalSchema: "portal",
                        principalTable: "process_step_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.InsertData(
                schema: "portal",
                table: "application_checklist_types",
                columns: new[] { "id", "label" },
                values: new object[] { 6, "APPLICATION_ACTIVATION" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "TODO" },
                    { 2, "DONE" },
                    { 3, "SKIPPED" },
                    { 4, "FAILED" },
                    { 5, "DUPLICATE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "VERIFY_REGISTRATION" },
                    { 2, "CREATE_BUSINESS_PARTNER_NUMBER_PUSH" },
                    { 3, "CREATE_BUSINESS_PARTNER_NUMBER_PULL" },
                    { 4, "CREATE_BUSINESS_PARTNER_NUMBER_MANUAL" },
                    { 5, "CREATE_IDENTITY_WALLET" },
                    { 6, "RETRIGGER_IDENTITY_WALLET" },
                    { 7, "START_CLEARING_HOUSE" },
                    { 8, "RETRIGGER_CLEARING_HOUSE" },
                    { 9, "END_CLEARING_HOUSE" },
                    { 10, "CREATE_SELF_DESCRIPTION_LP" },
                    { 11, "RETRIGGER_SELF_DESCRIPTION_LP" },
                    { 12, "ACTIVATE_APPLICATION" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_application_assigned_process_steps_process_step_id",
                schema: "portal",
                table: "application_assigned_process_steps",
                column: "process_step_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_process_steps_process_step_status_id",
                schema: "portal",
                table: "process_steps",
                column: "process_step_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_process_steps_process_step_type_id",
                schema: "portal",
                table: "process_steps",
                column: "process_step_type_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "application_assigned_process_steps",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "process_steps",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "process_step_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "process_step_types",
                schema: "portal");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 6);
        }
    }
}
