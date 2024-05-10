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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _593AdjustDimTechnicalUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.AddColumn<int>(
                name: "company_service_account_kind_id",
                schema: "portal",
                table: "company_service_accounts",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "client_client_id",
                filter: "client_client_id is not null AND company_service_account_kind_id = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.DropColumn(
                name: "company_service_account_kind_id",
                schema: "portal",
                table: "company_service_accounts");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_client_client_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "client_client_id",
                unique: true);
        }
    }
}
