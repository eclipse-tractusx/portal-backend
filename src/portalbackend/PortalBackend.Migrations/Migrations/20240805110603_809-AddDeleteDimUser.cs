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
    public partial class _809AddDeleteDimUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "version",
                schema: "portal",
                table: "company_service_accounts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.InsertData(
                schema: "portal",
                table: "identity_user_statuses",
                columns: new[] { "id", "label" },
                values: new object[] { 5, "PENDING_DELETION" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 502, "AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE" },
                    { 503, "RETRIGGER_AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE" },
                    { 504, "DELETE_DIM_TECHNICAL_USER" },
                    { 505, "AWAIT_DELETE_DIM_TECHNICAL_USER" },
                    { 506, "RETRIGGER_DELETE_DIM_TECHNICAL_USER" }
                });

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 502 WHERE process_step_type_id = 114");
            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 503 WHERE process_step_type_id = 115");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "portal",
                table: "identity_user_statuses",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "version",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 114, "AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE" },
                    { 115, "RETRIGGER_AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE" }
                });

            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 114 WHERE process_step_type_id = 502");
            migrationBuilder.Sql("UPDATE portal.process_steps SET process_step_type_id = 115 WHERE process_step_type_id = 503");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 502);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 503);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 504);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 505);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 506);
        }
    }
}
