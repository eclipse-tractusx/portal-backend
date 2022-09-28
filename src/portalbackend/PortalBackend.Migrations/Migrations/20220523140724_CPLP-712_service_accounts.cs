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
    public partial class CPLP712_service_accounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "company_service_account_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_service_account_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_service_accounts",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    company_service_account_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_service_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_service_accounts_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_service_accounts_company_service_account_statuses_c",
                        column: x => x.company_service_account_status_id,
                        principalSchema: "portal",
                        principalTable: "company_service_account_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "company_service_account_assigned_roles",
                schema: "portal",
                columns: table => new
                {
                    company_service_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_service_account_assigned_roles", x => new { x.company_service_account_id, x.user_role_id });
                    table.ForeignKey(
                        name: "fk_company_service_account_assigned_roles_company_service_acco",
                        column: x => x.company_service_account_id,
                        principalSchema: "portal",
                        principalTable: "company_service_accounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_service_account_assigned_roles_user_roles_user_role",
                        column: x => x.user_role_id,
                        principalSchema: "portal",
                        principalTable: "user_roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "iam_service_accounts",
                schema: "portal",
                columns: table => new
                {
                    client_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    client_client_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_entity_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    company_service_account_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iam_service_accounts", x => x.client_id);
                    table.ForeignKey(
                        name: "fk_iam_service_accounts_company_service_accounts_company_servi",
                        column: x => x.company_service_account_id,
                        principalSchema: "portal",
                        principalTable: "company_service_accounts",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_service_account_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_service_account_assigned_roles_user_role_id",
                schema: "portal",
                table: "company_service_account_assigned_roles",
                column: "user_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_company_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_company_service_account_status_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "company_service_account_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_iam_service_accounts_client_client_id",
                schema: "portal",
                table: "iam_service_accounts",
                column: "client_client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_iam_service_accounts_company_service_account_id",
                schema: "portal",
                table: "iam_service_accounts",
                column: "company_service_account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_iam_service_accounts_user_entity_id",
                schema: "portal",
                table: "iam_service_accounts",
                column: "user_entity_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_service_account_assigned_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "iam_service_accounts",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_service_accounts",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_service_account_statuses",
                schema: "portal");
        }
    }
}
