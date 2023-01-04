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

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1899AddUniqueIdentifierTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "unique_identifiers",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unique_identifiers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_identifiers",
                schema: "portal",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unique_identifier_id = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_identifiers", x => new { x.company_id, x.unique_identifier_id });
                    table.ForeignKey(
                        name: "fk_company_identifiers_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_identifiers_unique_identifiers_unique_identifier_id",
                        column: x => x.unique_identifier_id,
                        principalSchema: "portal",
                        principalTable: "unique_identifiers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "country_assigned_identifier",
                schema: "portal",
                columns: table => new
                {
                    country_alpha2code = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    unique_identifier_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_country_assigned_identifier", x => new { x.country_alpha2code, x.unique_identifier_id });
                    table.ForeignKey(
                        name: "fk_country_assigned_identifier_countries_country_alpha2code",
                        column: x => x.country_alpha2code,
                        principalSchema: "portal",
                        principalTable: "countries",
                        principalColumn: "alpha2code");
                    table.ForeignKey(
                        name: "fk_country_assigned_identifier_unique_identifiers_unique_ident",
                        column: x => x.unique_identifier_id,
                        principalSchema: "portal",
                        principalTable: "unique_identifiers",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "unique_identifiers",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "COMMERCIAL_REG_NUMBER" },
                    { 2, "VAT_ID" },
                    { 3, "LEI_CODE" },
                    { 4, "VIES" },
                    { 5, "EORI" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_identifiers_unique_identifier_id",
                schema: "portal",
                table: "company_identifiers",
                column: "unique_identifier_id");

            migrationBuilder.CreateIndex(
                name: "ix_country_assigned_identifier_unique_identifier_id",
                schema: "portal",
                table: "country_assigned_identifier",
                column: "unique_identifier_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_identifiers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "country_assigned_identifier",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "unique_identifiers",
                schema: "portal");
        }
    }
}
