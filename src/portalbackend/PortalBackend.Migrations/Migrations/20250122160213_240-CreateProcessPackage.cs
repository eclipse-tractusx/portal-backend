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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _240CreateProcessPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_process_steps_process_step_statuses_process_step_status_id",
                schema: "portal",
                table: "process_steps");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "identity_type",
                keyColumn: "id",
                keyValue: 2,
                column: "label",
                value: "TECHNICAL_USER");

            migrationBuilder.AddForeignKey(
                name: "fk_process_steps_process_step_statuses_process_step_status_id",
                schema: "portal",
                table: "process_steps",
                column: "process_step_status_id",
                principalSchema: "portal",
                principalTable: "process_step_statuses",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_process_steps_process_step_statuses_process_step_status_id",
                schema: "portal",
                table: "process_steps");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "identity_type",
                keyColumn: "id",
                keyValue: 2,
                column: "label",
                value: "COMPANY_SERVICE_ACCOUNT");

            migrationBuilder.AddForeignKey(
                name: "fk_process_steps_process_step_statuses_process_step_status_id",
                schema: "portal",
                table: "process_steps",
                column: "process_step_status_id",
                principalSchema: "portal",
                principalTable: "process_step_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
