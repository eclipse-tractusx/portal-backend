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
    public partial class CPLP1353MultiTenantApp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM portal.company_service_account_assigned_roles");
            migrationBuilder.Sql("DELETE FROM portal.company_user_assigned_roles");
            migrationBuilder.Sql("DELETE FROM portal.user_roles");
            
            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_iam_clients_iam_client_id",
                schema: "portal",
                table: "user_roles");

            migrationBuilder.DropTable(
                name: "app_assigned_clients",
                schema: "portal");

            migrationBuilder.DropColumn(
                name: "app_url",
                schema: "portal",
                table: "apps");

            migrationBuilder.RenameColumn(
                name: "iam_client_id",
                schema: "portal",
                table: "user_roles",
                newName: "app_id");

            migrationBuilder.RenameIndex(
                name: "ix_user_roles_iam_client_id",
                schema: "portal",
                table: "user_roles",
                newName: "ix_user_roles_app_id");

            migrationBuilder.AddColumn<Guid>(
                name: "app_instance_id",
                schema: "portal",
                table: "company_assigned_apps",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "app_url",
                schema: "portal",
                table: "company_assigned_apps",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "app_instance_id",
                schema: "portal",
                table: "audit_company_assigned_apps_cplp_1254_db_audit",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "app_url",
                schema: "portal",
                table: "audit_company_assigned_apps_cplp_1254_db_audit",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_core_component",
                schema: "portal",
                table: "apps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "app_instances",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    iam_client_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_instances", x => x.id);
                    table.ForeignKey(
                        name: "fk_app_instances_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_app_instances_iam_clients_iam_client_id",
                        column: x => x.iam_client_id,
                        principalSchema: "portal",
                        principalTable: "iam_clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_apps_app_instance_id",
                schema: "portal",
                table: "company_assigned_apps",
                column: "app_instance_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_instances_app_id",
                schema: "portal",
                table: "app_instances",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_instances_iam_client_id",
                schema: "portal",
                table: "app_instances",
                column: "iam_client_id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_assigned_apps_app_instances_app_instance_id",
                schema: "portal",
                table: "company_assigned_apps",
                column: "app_instance_id",
                principalSchema: "portal",
                principalTable: "app_instances",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_apps_app_id",
                schema: "portal",
                table: "user_roles",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "apps",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_company_assigned_apps_app_instances_app_instance_id",
                schema: "portal",
                table: "company_assigned_apps");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_apps_app_id",
                schema: "portal",
                table: "user_roles");

            migrationBuilder.DropTable(
                name: "app_instances",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_company_assigned_apps_app_instance_id",
                schema: "portal",
                table: "company_assigned_apps");

            migrationBuilder.DropColumn(
                name: "app_instance_id",
                schema: "portal",
                table: "company_assigned_apps");

            migrationBuilder.DropColumn(
                name: "app_url",
                schema: "portal",
                table: "company_assigned_apps");

            migrationBuilder.DropColumn(
                name: "app_instance_id",
                schema: "portal",
                table: "audit_company_assigned_apps_cplp_1254_db_audit");

            migrationBuilder.DropColumn(
                name: "app_url",
                schema: "portal",
                table: "audit_company_assigned_apps_cplp_1254_db_audit");

            migrationBuilder.DropColumn(
                name: "is_core_component",
                schema: "portal",
                table: "apps");

            migrationBuilder.RenameColumn(
                name: "app_id",
                schema: "portal",
                table: "user_roles",
                newName: "iam_client_id");

            migrationBuilder.RenameIndex(
                name: "ix_user_roles_app_id",
                schema: "portal",
                table: "user_roles",
                newName: "ix_user_roles_iam_client_id");

            migrationBuilder.AddColumn<string>(
                name: "app_url",
                schema: "portal",
                table: "apps",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "app_assigned_clients",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    iam_client_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_assigned_clients", x => new { x.app_id, x.iam_client_id });
                    table.ForeignKey(
                        name: "fk_app_assigned_clients_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_app_assigned_clients_iam_clients_iam_client_id",
                        column: x => x.iam_client_id,
                        principalSchema: "portal",
                        principalTable: "iam_clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_app_assigned_clients_iam_client_id",
                schema: "portal",
                table: "app_assigned_clients",
                column: "iam_client_id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_iam_clients_iam_client_id",
                schema: "portal",
                table: "user_roles",
                column: "iam_client_id",
                principalSchema: "portal",
                principalTable: "iam_clients",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
