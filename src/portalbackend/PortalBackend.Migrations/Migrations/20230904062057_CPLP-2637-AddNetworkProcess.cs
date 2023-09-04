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
using System;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP2637AddNetworkProcess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "company_user_assigned_identity_providers",
                schema: "portal",
                columns: table => new
                {
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<string>(type: "text", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: false),
                    process_step_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_user_assigned_identity_providers", x => new { x.company_user_id, x.identity_provider_id });
                    table.ForeignKey(
                        name: "fk_company_user_assigned_identity_providers_company_users_comp",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_user_assigned_identity_providers_identity_providers",
                        column: x => x.identity_provider_id,
                        principalSchema: "portal",
                        principalTable: "identity_providers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_user_assigned_identity_providers_process_steps_proc",
                        column: x => x.process_step_id,
                        principalSchema: "portal",
                        principalTable: "process_steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "network_registrations",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    external_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_network_registrations", x => x.id);
                    table.ForeignKey(
                        name: "fk_network_registrations_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_network_registrations_processes_process_id",
                        column: x => x.process_id,
                        principalSchema: "portal",
                        principalTable: "processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "identity_user_statuses",
                columns: new[] { "id", "label" },
                values: new object[] { 4, "PENDING" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 200, "SYNCHRONIZE_USER" },
                    { 201, "RETRIGGER_SYNCHRONIZE_USER" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 4, "PARTNER_REGISTRATION" });

            migrationBuilder.CreateIndex(
                name: "ix_company_user_assigned_identity_providers_identity_provider_",
                schema: "portal",
                table: "company_user_assigned_identity_providers",
                column: "identity_provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_user_assigned_identity_providers_process_step_id",
                schema: "portal",
                table: "company_user_assigned_identity_providers",
                column: "process_step_id");

            migrationBuilder.CreateIndex(
                name: "ix_network_registrations_company_id",
                schema: "portal",
                table: "network_registrations",
                column: "company_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_network_registrations_external_id",
                schema: "portal",
                table: "network_registrations",
                column: "external_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_network_registrations_process_id",
                schema: "portal",
                table: "network_registrations",
                column: "process_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_user_assigned_identity_providers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "network_registrations",
                schema: "portal");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "identity_user_statuses",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 200);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 201);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_types",
                keyColumn: "id",
                keyValue: 4);
        }
    }
}
