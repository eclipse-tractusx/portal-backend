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
    public partial class CPLP1248appnotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "requester_id",
                schema: "portal",
                table: "company_assigned_apps",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"));

            migrationBuilder.AlterColumn<Guid>(
                name: "requester_id",
                schema: "portal",
                table: "company_assigned_apps",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "sales_manager_id",
                schema: "portal",
                table: "apps",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_apps_sales_manager_id",
                schema: "portal",
                table: "apps",
                column: "sales_manager_id");

            migrationBuilder.AddForeignKey(
                name: "fk_apps_company_users_sales_manager_id",
                schema: "portal",
                table: "apps",
                column: "sales_manager_id",
                principalSchema: "portal",
                principalTable: "company_users",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_apps_company_users_sales_manager_id",
                schema: "portal",
                table: "apps");

            migrationBuilder.DropIndex(
                name: "ix_apps_sales_manager_id",
                schema: "portal",
                table: "apps");

            migrationBuilder.DropColumn(
                name: "requester_id",
                schema: "portal",
                table: "company_assigned_apps");

            migrationBuilder.DropColumn(
                name: "sales_manager_id",
                schema: "portal",
                table: "apps");
        }
    }
}
