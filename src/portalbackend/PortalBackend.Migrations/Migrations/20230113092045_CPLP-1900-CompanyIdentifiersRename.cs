/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
    public partial class CPLP1900CompanyIdentifiersRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_country_assigned_identifier_countries_country_alpha2code",
                schema: "portal",
                table: "country_assigned_identifier");

            migrationBuilder.DropForeignKey(
                name: "fk_country_assigned_identifier_unique_identifiers_unique_ident",
                schema: "portal",
                table: "country_assigned_identifier");

            migrationBuilder.DropPrimaryKey(
                name: "pk_country_assigned_identifier",
                schema: "portal",
                table: "country_assigned_identifier");

            migrationBuilder.RenameTable(
                name: "country_assigned_identifier",
                schema: "portal",
                newName: "country_assigned_identifiers",
                newSchema: "portal");

            migrationBuilder.RenameIndex(
                name: "ix_country_assigned_identifier_unique_identifier_id",
                schema: "portal",
                table: "country_assigned_identifiers",
                newName: "ix_country_assigned_identifiers_unique_identifier_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_country_assigned_identifiers",
                schema: "portal",
                table: "country_assigned_identifiers",
                columns: new[] { "country_alpha2code", "unique_identifier_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_country_assigned_identifiers_countries_country_alpha2code",
                schema: "portal",
                table: "country_assigned_identifiers",
                column: "country_alpha2code",
                principalSchema: "portal",
                principalTable: "countries",
                principalColumn: "alpha2code");

            migrationBuilder.AddForeignKey(
                name: "fk_country_assigned_identifiers_unique_identifiers_unique_iden",
                schema: "portal",
                table: "country_assigned_identifiers",
                column: "unique_identifier_id",
                principalSchema: "portal",
                principalTable: "unique_identifiers",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_country_assigned_identifiers_countries_country_alpha2code",
                schema: "portal",
                table: "country_assigned_identifiers");

            migrationBuilder.DropForeignKey(
                name: "fk_country_assigned_identifiers_unique_identifiers_unique_iden",
                schema: "portal",
                table: "country_assigned_identifiers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_country_assigned_identifiers",
                schema: "portal",
                table: "country_assigned_identifiers");

            migrationBuilder.RenameTable(
                name: "country_assigned_identifiers",
                schema: "portal",
                newName: "country_assigned_identifier",
                newSchema: "portal");

            migrationBuilder.RenameIndex(
                name: "ix_country_assigned_identifiers_unique_identifier_id",
                schema: "portal",
                table: "country_assigned_identifier",
                newName: "ix_country_assigned_identifier_unique_identifier_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_country_assigned_identifier",
                schema: "portal",
                table: "country_assigned_identifier",
                columns: new[] { "country_alpha2code", "unique_identifier_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_country_assigned_identifier_countries_country_alpha2code",
                schema: "portal",
                table: "country_assigned_identifier",
                column: "country_alpha2code",
                principalSchema: "portal",
                principalTable: "countries",
                principalColumn: "alpha2code");

            migrationBuilder.AddForeignKey(
                name: "fk_country_assigned_identifier_unique_identifiers_unique_ident",
                schema: "portal",
                table: "country_assigned_identifier",
                column: "unique_identifier_id",
                principalSchema: "portal",
                principalTable: "unique_identifiers",
                principalColumn: "id");
        }
    }
}
