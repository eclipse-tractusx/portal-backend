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
    public partial class _564AddNewChecklistEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "did_document_location",
                schema: "portal",
                table: "companies",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 4,
                column: "label",
                value: "BPNL_CREDENTIAL");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 5,
                column: "label",
                value: "MEMBERSHIP_CREDENTIAL");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 6,
                column: "label",
                value: "CLEARING_HOUSE");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "application_checklist_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 7, "SELF_DESCRIPTION_LP" },
                    { 8, "APPLICATION_ACTIVATION" }
                });

            migrationBuilder.Sql("UPDATE portal.application_checklist SET application_checklist_entry_type_id = 8 WHERE application_checklist_entry_type_id = 6");
            migrationBuilder.Sql("UPDATE portal.application_checklist SET application_checklist_entry_type_id = 7 WHERE application_checklist_entry_type_id = 5");
            migrationBuilder.Sql("UPDATE portal.application_checklist SET application_checklist_entry_type_id = 6 WHERE application_checklist_entry_type_id = 4");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 25, "REQUEST_BPN_CREDENTIAL" },
                    { 26, "STORED_BPN_CREDENTIAL" },
                    { 27, "REQUEST_MEMBERSHIP_CREDENTIAL" },
                    { 28, "STORED_MEMBERSHIP_CREDENTIAL" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE from portal.application_checklist WHERE application_checklist_entry_type_id = 4");
            migrationBuilder.Sql("DELETE from portal.application_checklist WHERE application_checklist_entry_type_id = 5");
            migrationBuilder.Sql("UPDATE portal.application_checklist SET application_checklist_entry_type_id = 4 WHERE application_checklist_entry_type_id = 6");
            migrationBuilder.Sql("UPDATE portal.application_checklist SET application_checklist_entry_type_id = 5 WHERE application_checklist_entry_type_id = 7");
            migrationBuilder.Sql("UPDATE portal.application_checklist SET application_checklist_entry_type_id = 6 WHERE application_checklist_entry_type_id = 8");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 28);

            migrationBuilder.DropColumn(
                name: "did_document_location",
                schema: "portal",
                table: "companies");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 4,
                column: "label",
                value: "CLEARING_HOUSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 5,
                column: "label",
                value: "SELF_DESCRIPTION_LP");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "application_checklist_types",
                keyColumn: "id",
                keyValue: 6,
                column: "label",
                value: "APPLICATION_ACTIVATION");
        }
    }
}
