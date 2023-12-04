/********************************************************************************
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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
    public partial class CPLP3523UpdateCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "template",
                schema: "portal",
                table: "verified_credential_external_type_use_case_detail_versions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "verified_credential_external_types",
                columns: new[] { "id", "label" },
                values: new object[] { 6, "Quality_Credential" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "verified_credential_types",
                columns: new[] { "id", "label" },
                values: new object[] { 6, "FRAMEWORK_AGREEMENT_QUALITY" });

            migrationBuilder.Sql("UPDATE portal.verified_credential_type_assigned_use_cases SET use_case_id = 'b3948771-3372-4568-9e0e-acca4e674098' WHERE verified_credential_type_id = 3 and use_case_id = 'c065a349-f649-47f8-94d5-1a504a855419'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE portal.verified_credential_type_assigned_use_cases SET use_case_id = 'c065a349-f649-47f8-94d5-1a504a855419' WHERE verified_credential_type_id = 3 and use_case_id = 'b3948771-3372-4568-9e0e-acca4e674098'");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "verified_credential_external_types",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "verified_credential_types",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.AlterColumn<string>(
                name: "template",
                schema: "portal",
                table: "verified_credential_external_type_use_case_detail_versions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
