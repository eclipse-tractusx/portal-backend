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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class CPLP3401AddIamDisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "display_name",
                schema: "portal",
                table: "iam_identity_providers",
                type: "text",
                nullable: true);

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[] { 300, "SYNCHRONIZE_IDP_DISPLAY_NAME" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 5, "IDP_DISPLAY_NAME_SYNC" });

            migrationBuilder.Sql(@"INSERT INTO portal.processes (id, process_type_id, lock_expiry_date, version)
                                 VALUES ('25b1eaea-99fa-4fb2-892f-e4be7486d448', 5, null, 'deadbeef-dead-beef-dead-beefdeadbeef');
                                 INSERT INTO portal.process_steps (id, process_step_type_id, process_step_status_id, date_created, date_last_changed, process_id, message)
                                 VALUES ('910215e4-9039-4884-b578-5e5345bffc62', 300, 1, CURRENT_TIMESTAMP, null, '25b1eaea-99fa-4fb2-892f-e4be7486d448', null)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM portal.process_steps where process_step_type_id = 300;
                                    DELETE FROM portal.processes where process_type_id = 5");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 300);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "process_types",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "display_name",
                schema: "portal",
                table: "iam_identity_providers");
        }
    }
}
