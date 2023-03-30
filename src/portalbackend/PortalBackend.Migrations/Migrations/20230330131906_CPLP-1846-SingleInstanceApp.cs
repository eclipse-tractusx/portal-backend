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
    public partial class CPLP1846SingleInstanceApp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_instance_assigned_service_accounts",
                schema: "portal",
                columns: table => new
                {
                    app_instance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_service_account_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_instance_assigned_service_accounts", x => new { x.app_instance_id, x.company_service_account_id });
                    table.ForeignKey(
                        name: "fk_app_instance_assigned_service_accounts_app_instances_app_in",
                        column: x => x.app_instance_id,
                        principalSchema: "portal",
                        principalTable: "app_instances",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_app_instance_assigned_service_accounts_company_service_acco",
                        column: x => x.company_service_account_id,
                        principalSchema: "portal",
                        principalTable: "company_service_accounts",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "app_instance_setups",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_single_instance = table.Column<bool>(type: "boolean", nullable: false),
                    instance_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_instance_setups", x => x.id);
                    table.ForeignKey(
                        name: "fk_app_instance_setups_offers_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_app_instance_assigned_service_accounts_company_service_acco",
                schema: "portal",
                table: "app_instance_assigned_service_accounts",
                column: "company_service_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_instance_setups_app_id",
                schema: "portal",
                table: "app_instance_setups",
                column: "app_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_instance_assigned_service_accounts",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_instance_setups",
                schema: "portal");
        }
    }
}
