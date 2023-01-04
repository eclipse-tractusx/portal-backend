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
    public partial class CPLP1963AddChecklist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "checklist_entry_status",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_checklist_entry_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "checklist_entry_type",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_checklist_entry_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "application_checklist",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    application_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    checklist_entry_type_id = table.Column<int>(type: "integer", nullable: false),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_checklist", x => x.id);
                    table.ForeignKey(
                        name: "fk_application_checklist_checklist_entry_status_status_id",
                        column: x => x.status_id,
                        principalSchema: "portal",
                        principalTable: "checklist_entry_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_application_checklist_checklist_entry_type_checklist_entry_",
                        column: x => x.checklist_entry_type_id,
                        principalSchema: "portal",
                        principalTable: "checklist_entry_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_application_checklist_company_applications_application_id",
                        column: x => x.application_id,
                        principalSchema: "portal",
                        principalTable: "company_applications",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "checklist_entry_status",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "TO_DO" },
                    { 2, "IN_PROGRESS" },
                    { 3, "DONE" },
                    { 4, "FAILED" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "checklist_entry_type",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "REGISTRATION_VERIFICATION" },
                    { 2, "BUSINESS_PARTNER_NUMBER" },
                    { 3, "IDENTITY_WALLET" },
                    { 4, "CLEARING_HOUSE" },
                    { 5, "SELF_DESCRIPTION_LP" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_application_checklist_application_id",
                schema: "portal",
                table: "application_checklist",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "ix_application_checklist_checklist_entry_type_id",
                schema: "portal",
                table: "application_checklist",
                column: "checklist_entry_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_application_checklist_status_id",
                schema: "portal",
                table: "application_checklist",
                column: "status_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "application_checklist",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "checklist_entry_status",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "checklist_entry_type",
                schema: "portal");
        }
    }
}
