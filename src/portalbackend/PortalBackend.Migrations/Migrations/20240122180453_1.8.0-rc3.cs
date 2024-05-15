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
    public partial class _180rc3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_application_statuses",
                columns: new[] { "id", "label" },
                values: new object[] { 10, "CANCELLED_BY_CUSTOMER" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 208, "MANUAL_DECLINE_OSP" },
                    { 209, "REMOVE_KEYCLOAK_USERS" },
                    { 210, "RETRIGGER_REMOVE_KEYCLOAK_USERS" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_application_statuses",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 208);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 209);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 210);
        }
    }
}
