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
    public partial class _912AdjustRetrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE from portal.process_steps WHERE process_step_type_id = 15");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 15);

            migrationBuilder.Sql("DELETE from portal.process_steps WHERE process_step_type_id = 503");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 503);

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 1,
                column: "label",
                value: "MANUAL_VERIFY_REGISTRATION");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 9,
                column: "label",
                value: "AWAIT_CLEARING_HOUSE_RESPONSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 16,
                column: "label",
                value: "MANUAL_TRIGGER_OVERRIDE_CLEARING_HOUSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 18,
                column: "label",
                value: "AWAIT_SELF_DESCRIPTION_LP_RESPONSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 19,
                column: "label",
                value: "MANUAL_DECLINE_APPLICATION");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 21,
                column: "label",
                value: "AWAIT_DIM_RESPONSE_RESPONSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 26,
                column: "label",
                value: "AWAIT_BPN_CREDENTIAL_RESPONSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 28,
                column: "label",
                value: "AWAIT_MEMBERSHIP_CREDENTIAL_RESPONSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 101,
                column: "label",
                value: "AWAIT_START_AUTOSETUP");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 111,
                column: "label",
                value: "MANUAL_TRIGGER_ACTIVATE_SUBSCRIPTION");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 505,
                column: "label",
                value: "AWAIT_DELETE_DIM_TECHNICAL_USER_RESPONSE");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 31, "RETRIGGER_REQUEST_BPN_CREDENTIAL" },
                    { 32, "RETRIGGER_REQUEST_MEMBERSHIP_CREDENTIAL" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE from portal.process_steps WHERE process_step_type_id = 31");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 31);

            migrationBuilder.Sql("DELETE from portal.process_steps WHERE process_step_type_id = 32");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 32);

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 1,
                column: "label",
                value: "VERIFY_REGISTRATION");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 9,
                column: "label",
                value: "END_CLEARING_HOUSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 16,
                column: "label",
                value: "TRIGGER_OVERRIDE_CLEARING_HOUSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 18,
                column: "label",
                value: "FINISH_SELF_DESCRIPTION_LP");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 19,
                column: "label",
                value: "DECLINE_APPLICATION");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 21,
                column: "label",
                value: "AWAIT_DIM_RESPONSE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 26,
                column: "label",
                value: "STORED_BPN_CREDENTIAL");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 28,
                column: "label",
                value: "STORED_MEMBERSHIP_CREDENTIAL");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 101,
                column: "label",
                value: "START_AUTOSETUP");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 111,
                column: "label",
                value: "TRIGGER_ACTIVATE_SUBSCRIPTION");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 505,
                column: "label",
                value: "AWAIT_DELETE_DIM_TECHNICAL_USER");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 15, "OVERRIDE_BUSINESS_PARTNER_NUMBER" },
                    { 503, "RETRIGGER_AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE" }
                });
        }
    }
}
