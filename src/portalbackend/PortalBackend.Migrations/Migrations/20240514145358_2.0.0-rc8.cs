/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _200rc8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.AddColumn<int>(
                name: "company_service_account_kind_id",
                schema: "portal",
                table: "company_service_accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "company_service_account_kindes",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_service_account_kindes", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_service_account_kindes",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "INTERNAL" },
                    { 2, "EXTERNAL" }
                });

            migrationBuilder.Sql("UPDATE portal.company_service_accounts SET company_service_account_kind_id = 1");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "client_client_id",
                filter: "client_client_id is not null AND company_service_account_kind_id = 1");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_company_service_account_kind_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "company_service_account_kind_id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_service_accounts_company_service_account_kindes_com",
                schema: "portal",
                table: "company_service_accounts",
                column: "company_service_account_kind_id",
                principalSchema: "portal",
                principalTable: "company_service_account_kindes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_company_service_accounts_company_service_account_kindes_com",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropTable(
                name: "company_service_account_kindes",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_company_service_account_kind_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "company_service_account_kind_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "client_client_id",
                unique: true);
        }
    }
}
