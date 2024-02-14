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
    public partial class _180rc6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE portal.company_certificates SET company_certificate_status_id = 1 where company_certificate_status_id = 2");
            migrationBuilder.Sql("UPDATE portal.company_certificates SET company_certificate_status_id = 2 where company_certificate_status_id = 3");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_certificate_statuses",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "company_certificate_statuses",
                keyColumn: "id",
                keyValue: 1,
                column: "label",
                value: "ACTIVE");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "company_certificate_statuses",
                keyColumn: "id",
                keyValue: 2,
                column: "label",
                value: "INACTVIE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "portal",
                table: "company_certificate_statuses",
                keyColumn: "id",
                keyValue: 1,
                column: "label",
                value: "IN_REVIEW");

            migrationBuilder.UpdateData(
                schema: "portal",
                table: "company_certificate_statuses",
                keyColumn: "id",
                keyValue: 2,
                column: "label",
                value: "ACTIVE");

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_certificate_statuses",
                columns: new[] { "id", "label" },
                values: new object[] { 3, "INACTVIE" });

            migrationBuilder.Sql("UPDATE portal.company_certificates SET company_certificate_status_id = 3 where company_certificate_status_id = 2");
            migrationBuilder.Sql("UPDATE portal.company_certificates SET company_certificate_status_id = 2 where company_certificate_status_id = 1");
        }
    }
}
