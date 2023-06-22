/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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
    public partial class CPLP1302AddUpdateLanguages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "long_name_de",
                schema: "portal",
                table: "languages");

            migrationBuilder.DropColumn(
                name: "long_name_en",
                schema: "portal",
                table: "languages");

            migrationBuilder.CreateTable(
                name: "language_long_names",
                schema: "portal",
                columns: table => new
                {
                    short_name = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    long_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_language_long_names", x => new { x.short_name, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_language_long_names_languages_language_temp_id2",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name");
                });

            migrationBuilder.CreateIndex(
                name: "ix_language_long_names_language_short_name",
                schema: "portal",
                table: "language_long_names",
                column: "language_short_name");
            migrationBuilder.Sql("INSERT INTO portal.language_long_names (short_name,long_name, 'de') SELECT short_name, long_name_de FROM portal.languages");
            migrationBuilder.Sql("INSERT INTO portal.language_long_names (short_name,long_name, 'en') SELECT short_name, long_name_en FROM portal.languages");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO portal.languages (long_name_de, long_name_en) SELECT long_name_de, long_name FROM portal.language_long_names");
            migrationBuilder.DropTable(
                name: "language_long_names",
                schema: "portal");

            migrationBuilder.AddColumn<string>(
                name: "long_name_de",
                schema: "portal",
                table: "languages",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "long_name_en",
                schema: "portal",
                table: "languages",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
