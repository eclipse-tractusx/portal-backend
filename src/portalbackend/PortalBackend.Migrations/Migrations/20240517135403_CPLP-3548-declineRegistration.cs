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
    public partial class CPLP3548declineRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_company_identity_providers_companies_company_id",
                schema: "portal",
                table: "company_identity_providers");

            migrationBuilder.DropForeignKey(
                name: "fk_company_identity_providers_identity_providers_identity_prov",
                schema: "portal",
                table: "company_identity_providers");

            migrationBuilder.DropForeignKey(
                name: "fk_company_user_assigned_identity_providers_company_users_comp",
                schema: "portal",
                table: "company_user_assigned_identity_providers");

            migrationBuilder.DropForeignKey(
                name: "fk_company_user_assigned_identity_providers_identity_providers",
                schema: "portal",
                table: "company_user_assigned_identity_providers");

            migrationBuilder.CreateTable(
                name: "company_user_assigned_processes",
                schema: "portal",
                columns: table => new
                {
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_user_assigned_processes", x => new { x.company_user_id, x.process_id });
                    table.ForeignKey(
                        name: "fk_company_user_assigned_processes_company_users_company_user_",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_company_user_assigned_processes_processes_process_id",
                        column: x => x.process_id,
                        principalSchema: "portal",
                        principalTable: "processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_provider_assigned_processes",
                schema: "portal",
                columns: table => new
                {
                    identity_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_provider_assigned_processes", x => new { x.identity_provider_id, x.process_id });
                    table.ForeignKey(
                        name: "fk_identity_provider_assigned_processes_identity_providers_ide",
                        column: x => x.identity_provider_id,
                        principalSchema: "portal",
                        principalTable: "identity_providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_identity_provider_assigned_processes_processes_process_id",
                        column: x => x.process_id,
                        principalSchema: "portal",
                        principalTable: "processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 600, "DELETE_CENTRAL_USER" },
                    { 601, "RETRIGGER_DELETE_CENTRAL_USER" },
                    { 602, "DELETE_COMPANYUSER_ASSIGNED_PROCESS" },
                    { 700, "DELETE_IDP_SHARED_REALM" },
                    { 701, "RETRIGGER_DELETE_IDP_SHARED_REALM" },
                    { 702, "DELETE_IDP_SHARED_SERVICEACCOUNT" },
                    { 703, "RETRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT" },
                    { 704, "DELETE_CENTRAL_IDENTITY_PROVIDER" },
                    { 705, "RETRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER" },
                    { 706, "DELETE_IDENTITY_PROVIDER" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 8, "USER_PROVISIONING" },
                    { 9, "IDENTITYPROVIDER_PROVISIONING" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_user_assigned_processes_company_user_id",
                schema: "portal",
                table: "company_user_assigned_processes",
                column: "company_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_user_assigned_processes_process_id",
                schema: "portal",
                table: "company_user_assigned_processes",
                column: "process_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identity_provider_assigned_processes_identity_provider_id",
                schema: "portal",
                table: "identity_provider_assigned_processes",
                column: "identity_provider_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identity_provider_assigned_processes_process_id",
                schema: "portal",
                table: "identity_provider_assigned_processes",
                column: "process_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_company_identity_providers_companies_company_id",
                schema: "portal",
                table: "company_identity_providers",
                column: "company_id",
                principalSchema: "portal",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_identity_providers_identity_providers_identity_prov",
                schema: "portal",
                table: "company_identity_providers",
                column: "identity_provider_id",
                principalSchema: "portal",
                principalTable: "identity_providers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_user_assigned_identity_providers_company_users_comp",
                schema: "portal",
                table: "company_user_assigned_identity_providers",
                column: "company_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_company_user_assigned_identity_providers_identity_providers",
                schema: "portal",
                table: "company_user_assigned_identity_providers",
                column: "identity_provider_id",
                principalSchema: "portal",
                principalTable: "identity_providers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_company_identity_providers_companies_company_id",
                schema: "portal",
                table: "company_identity_providers");

            migrationBuilder.DropForeignKey(
                name: "fk_company_identity_providers_identity_providers_identity_prov",
                schema: "portal",
                table: "company_identity_providers");

            migrationBuilder.DropForeignKey(
                name: "fk_company_user_assigned_identity_providers_company_users_comp",
                schema: "portal",
                table: "company_user_assigned_identity_providers");

            migrationBuilder.DropForeignKey(
                name: "fk_company_user_assigned_identity_providers_identity_providers",
                schema: "portal",
                table: "company_user_assigned_identity_providers");

            migrationBuilder.DropTable(
                name: "company_user_assigned_processes",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identity_provider_assigned_processes",
                schema: "portal");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 600);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 601);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 602);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 700);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 701);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 702);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 703);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 704);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 705);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 706);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_types",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_types",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.AddForeignKey(
                name: "fk_company_identity_providers_companies_company_id",
                schema: "portal",
                table: "company_identity_providers",
                column: "company_id",
                principalSchema: "portal",
                principalTable: "companies",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_identity_providers_identity_providers_identity_prov",
                schema: "portal",
                table: "company_identity_providers",
                column: "identity_provider_id",
                principalSchema: "portal",
                principalTable: "identity_providers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_user_assigned_identity_providers_company_users_comp",
                schema: "portal",
                table: "company_user_assigned_identity_providers",
                column: "company_user_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_user_assigned_identity_providers_identity_providers",
                schema: "portal",
                table: "company_user_assigned_identity_providers",
                column: "identity_provider_id",
                principalSchema: "portal",
                principalTable: "identity_providers",
                principalColumn: "id");
        }
    }
}
