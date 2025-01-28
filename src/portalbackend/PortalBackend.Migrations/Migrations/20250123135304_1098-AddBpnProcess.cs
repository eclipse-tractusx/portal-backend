/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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
    public partial class _1098AddBpnProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "process_id",
                schema: "portal",
                table: "company_user_assigned_business_partners",
                type: "uuid",
                nullable: true);

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 900, "DELETE_BPN_FROM_CENTRAL_USER" },
                    { 901, "DELETE_BPN_FROM_IDENTITY" },
                    { 902, "RETRIGGER_DELETE_BPN_FROM_CENTRAL_USER" },
                    { 904, "CHECK_LEGAL_ENTITY_DATA" },
                    { 905, "ADD_BPN_TO_IDENTITY" },
                    { 906, "CLEANUP_USER_BPN" },
                    { 907, "RETRIGGER_CHECK_LEGAL_ENTITY_DATA" },
                    { 908, "RETRIGGER_ADD_BPN_TO_IDENTITY" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 11, "USER_BPN" });

            migrationBuilder.CreateIndex(
                name: "ix_company_user_assigned_business_partners_process_id",
                schema: "portal",
                table: "company_user_assigned_business_partners",
                column: "process_id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_user_assigned_business_partners_processes_process_id",
                schema: "portal",
                table: "company_user_assigned_business_partners",
                column: "process_id",
                principalSchema: "portal",
                principalTable: "processes",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_company_user_assigned_business_partners_processes_process_id",
                schema: "portal",
                table: "company_user_assigned_business_partners");

            migrationBuilder.DropIndex(
                name: "ix_company_user_assigned_business_partners_process_id",
                schema: "portal",
                table: "company_user_assigned_business_partners");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 900);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 901);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 902);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 904);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 905);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 906);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 907);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 908);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_types",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DropColumn(
                name: "process_id",
                schema: "portal",
                table: "company_user_assigned_business_partners");
        }
    }
}
