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
    public partial class _593AddNewDimProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dim_company_service_accounts",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    authentication_service_url = table.Column<string>(type: "text", nullable: false),
                    client_secret = table.Column<byte[]>(type: "bytea", nullable: false),
                    initialization_vector = table.Column<byte[]>(type: "bytea", nullable: true),
                    encryption_mode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dim_company_service_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_dim_company_service_accounts_company_service_accounts_id",
                        column: x => x.id,
                        principalSchema: "portal",
                        principalTable: "company_service_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dim_user_creation_data",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dim_user_creation_data", x => x.id);
                    table.ForeignKey(
                        name: "fk_dim_user_creation_data_company_service_accounts_service_acc",
                        column: x => x.service_account_id,
                        principalSchema: "portal",
                        principalTable: "company_service_accounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_dim_user_creation_data_processes_process_id",
                        column: x => x.process_id,
                        principalSchema: "portal",
                        principalTable: "processes",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 112, "OFFERSUBSCRIPTION_CREATE_DIM_TECHNICAL_USER" },
                    { 113, "RETRIGGER_OFFERSUBSCRIPTION_CREATE_DIM_TECHNICAL_USER" },
                    { 114, "AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE" },
                    { 115, "RETRIGGER_AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE" },
                    { 500, "CREATE_DIM_TECHNICAL_USER" },
                    { 501, "RETRIGGER_CREATE_DIM_TECHNICAL_USER" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 7, "DIM_TECHNICAL_USER" });

            migrationBuilder.CreateIndex(
                name: "ix_dim_user_creation_data_process_id",
                schema: "portal",
                table: "dim_user_creation_data",
                column: "process_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dim_user_creation_data_service_account_id",
                schema: "portal",
                table: "dim_user_creation_data",
                column: "service_account_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dim_company_service_accounts",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "dim_user_creation_data",
                schema: "portal");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 113);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 114);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 115);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 500);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 501);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_types",
                keyColumn: "id",
                keyValue: 7);
        }
    }
}
