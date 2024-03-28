/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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
            migrationBuilder.AddColumn<Guid>(
                name: "process_id",
                schema: "portal",
                table: "identity_providers",
                type: "uuid",
                nullable: true);

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 500, "TRIGGER_DELETE_IDP_SHARED_REALM" },
                    { 501, "TRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT" },
                    { 502, "TRIGGER_DELETE_IDENTITY_LINKED_USERS" },
                    { 503, "TRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER" },
                    { 504, "TRIGGER_DELETE_IDENTITY_PROVIDER" },
                    { 505, "RETRIGGER_DELETE_IDP_SHARED_REALM" },
                    { 506, "RETRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT" },
                    { 507, "RETRIGGER_DELETE_IDENTITY_LINKED_USERS" },
                    { 508, "RETRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER" },
                    { 509, "RETRIGGER_DELETE_IDENTITY_PROVIDER" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 7, "IDP_DELETION" });

            migrationBuilder.CreateIndex(
                name: "ix_identity_providers_process_id",
                schema: "portal",
                table: "identity_providers",
                column: "process_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_identity_providers_processes_process_id",
                schema: "portal",
                table: "identity_providers",
                column: "process_id",
                principalSchema: "portal",
                principalTable: "processes",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_identity_providers_processes_process_id",
                schema: "portal",
                table: "identity_providers");

            migrationBuilder.DropIndex(
                name: "ix_identity_providers_process_id",
                schema: "portal",
                table: "identity_providers");

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

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 507);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 508);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 509);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_types",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "process_id",
                schema: "portal",
                table: "identity_providers");
        }
    }
}
