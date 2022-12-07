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

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP748AddAppSubscriptionStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "app_subscription_status_id",
                schema: "portal",
                table: "company_assigned_apps",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "app_subscription_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_subscription_statuses", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "app_subscription_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "ACTIVE" },
                    { 3, "INACTIVE" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_apps_app_subscription_status_id",
                schema: "portal",
                table: "company_assigned_apps",
                column: "app_subscription_status_id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_assigned_apps_app_subscription_statuses_app_subscri",
                schema: "portal",
                table: "company_assigned_apps",
                column: "app_subscription_status_id",
                principalSchema: "portal",
                principalTable: "app_subscription_statuses",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_company_assigned_apps_app_subscription_statuses_app_subscri",
                schema: "portal",
                table: "company_assigned_apps");

            migrationBuilder.DropTable(
                name: "app_subscription_statuses",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_company_assigned_apps_app_subscription_status_id",
                schema: "portal",
                table: "company_assigned_apps");

            migrationBuilder.DropColumn(
                name: "app_subscription_status_id",
                schema: "portal",
                table: "company_assigned_apps");
        }
    }
}
