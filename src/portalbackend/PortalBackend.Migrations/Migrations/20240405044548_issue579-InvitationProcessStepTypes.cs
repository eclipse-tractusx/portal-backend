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
    public partial class issue579InvitationProcessStepTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 420);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 421);

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 402,
                column: "label",
                value: "INVITATION_ADD_REALM_ROLE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 403,
                column: "label",
                value: "INVITATION_CREATE_SHARED_REALM");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 405,
                column: "label",
                value: "INVITATION_UPDATE_CENTRAL_IDP_URLS");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 410,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_CENTRAL_IDP");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 411,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 412,
                column: "label",
                value: "RETRIGGER_INVITATION_ADD_REALM_ROLE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 413,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_SHARED_REALM");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 414,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 415,
                column: "label",
                value: "RETRIGGER_INVITATION_UPDATE_CENTRAL_IDP_URLS");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 416,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_SHARED_CLIENT");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 417,
                column: "label",
                value: "RETRIGGER_INVITATION_ENABLE_CENTRAL_IDP");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 418,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_USER");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 419,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_DATABASE_IDP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 402,
                column: "label",
                value: "INVITATION_UPDATE_CENTRAL_IDP_URLS");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 403,
                column: "label",
                value: "INVITATION_ADD_REALM_ROLE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 405,
                column: "label",
                value: "INVITATION_CREATE_SHARED_REALM");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 410,
                column: "label",
                value: "INVITATION_SEND_MAIL");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 411,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_CENTRAL_IDP");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 412,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 413,
                column: "label",
                value: "RETRIGGER_INVITATION_UPDATE_CENTRAL_IDP_URLS");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 414,
                column: "label",
                value: "RETRIGGER_INVITATION_ADD_REALM_ROLE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 415,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 416,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_SHARED_REALM");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 417,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_SHARED_CLIENT");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 418,
                column: "label",
                value: "RETRIGGER_INVITATION_ENABLE_CENTRAL_IDP");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 419,
                column: "label",
                value: "RETRIGGER_INVITATION_CREATE_USER");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 420, "RETRIGGER_INVITATION_CREATE_DATABASE_IDP" },
                    { 421, "RETRIGGER_INVITATION_SEND_MAIL" }
                });
        }
    }
}
