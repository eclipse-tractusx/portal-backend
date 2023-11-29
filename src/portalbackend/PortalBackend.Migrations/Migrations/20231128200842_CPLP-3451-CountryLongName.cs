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
    public partial class CPLP3451CountryLongName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "country_name_de",
                schema: "portal",
                table: "countries");

            migrationBuilder.DropColumn(
                name: "country_name_en",
                schema: "portal",
                table: "countries");

            migrationBuilder.CreateTable(
                name: "country_long_names",
                schema: "portal",
                columns: table => new
                {
                    alpha2code = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    long_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_country_long_names", x => new { x.alpha2code, x.short_name });
                    table.ForeignKey(
                        name: "fk_country_long_names_countries_country_alpha2code",
                        column: x => x.alpha2code,
                        principalSchema: "portal",
                        principalTable: "countries",
                        principalColumn: "alpha2code");
                    table.ForeignKey(
                        name: "fk_country_long_names_languages_language_temp_id2",
                        column: x => x.short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name");
                });

            migrationBuilder.CreateIndex(
                name: "ix_country_long_names_short_name",
                schema: "portal",
                table: "country_long_names",
                column: "short_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "country_long_names",
                schema: "portal");

            migrationBuilder.AddColumn<string>(
                name: "country_name_de",
                schema: "portal",
                table: "countries",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "country_name_en",
                schema: "portal",
                table: "countries",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
