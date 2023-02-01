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
    public partial class CPLP1903BpdmIdentifier : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "bpdm_identifier_id",
                schema: "portal",
                table: "country_assigned_identifiers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "bpdm_identifiers",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bpdm_identifiers", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "bpdm_identifiers",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "EU_VAT_ID_DE" },
                    { 2, "CH_UID" },
                    { 3, "EU_VAT_ID_FR" },
                    { 4, "FR_SIREN" },
                    { 5, "EU_VAT_ID_AT" },
                    { 6, "DE_BNUM" },
                    { 7, "CZ_ICO" },
                    { 8, "EU_VAT_ID_CZ" },
                    { 9, "EU_VAT_ID_PL" },
                    { 10, "EU_VAT_ID_BE" },
                    { 11, "EU_VAT_ID_CH" },
                    { 12, "EU_VAT_ID_DK" },
                    { 13, "EU_VAT_ID_ES" },
                    { 14, "EU_VAT_ID_GB" },
                    { 15, "EU_VAT_ID_NO" },
                    { 16, "BE_ENT_NO" },
                    { 17, "CVR_DK" },
                    { 18, "ID_CRN" },
                    { 19, "NO_ORGID" },
                    { 20, "LEI_ID" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_country_assigned_identifiers_bpdm_identifier_id",
                schema: "portal",
                table: "country_assigned_identifiers",
                column: "bpdm_identifier_id");

            migrationBuilder.AddForeignKey(
                name: "fk_country_assigned_identifiers_bpdm_identifiers_bpdm_identifi",
                schema: "portal",
                table: "country_assigned_identifiers",
                column: "bpdm_identifier_id",
                principalSchema: "portal",
                principalTable: "bpdm_identifiers",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_country_assigned_identifiers_bpdm_identifiers_bpdm_identifi",
                schema: "portal",
                table: "country_assigned_identifiers");

            migrationBuilder.DropTable(
                name: "bpdm_identifiers",
                schema: "portal");

            migrationBuilder.DropIndex(
                name: "ix_country_assigned_identifiers_bpdm_identifier_id",
                schema: "portal",
                table: "country_assigned_identifiers");

            migrationBuilder.DropColumn(
                name: "bpdm_identifier_id",
                schema: "portal",
                table: "country_assigned_identifiers");
        }
    }
}
