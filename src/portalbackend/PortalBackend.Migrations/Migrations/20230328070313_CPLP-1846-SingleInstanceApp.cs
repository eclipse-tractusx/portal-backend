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
            migrationBuilder.AddColumn<Guid>(
                name: "service_account_id",
                schema: "portal",
                table: "app_instances",
                type: "uuid",
                nullable: true);

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
                name: "ix_app_instances_service_account_id",
                schema: "portal",
                table: "app_instances",
                column: "service_account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_app_instance_setups_app_id",
                schema: "portal",
                table: "app_instance_setups",
                column: "app_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_app_instances_company_service_accounts_service_account_id",
                schema: "portal",
                table: "app_instances",
                column: "service_account_id",
                principalSchema: "portal",
                principalTable: "company_service_accounts",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_app_instances_company_service_accounts_service_account_id",
                schema: "portal",
                table: "app_instances");

            migrationBuilder.DropTable(
                name: "app_instance_setups",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_app_instances_service_account_id",
                schema: "portal",
                table: "app_instances");

            migrationBuilder.DropColumn(
                name: "service_account_id",
                schema: "portal",
                table: "app_instances");
        }
    }
}
