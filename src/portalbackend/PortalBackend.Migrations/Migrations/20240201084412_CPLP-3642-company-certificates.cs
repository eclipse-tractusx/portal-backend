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

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class CPLP3642companycertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_company_role_descriptions_languages_language_temp_id1",
                schema: "portal",
                table: "company_role_descriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_country_long_names_languages_language_temp_id2",
                schema: "portal",
                table: "country_long_names");

            migrationBuilder.CreateTable(
                name: "company_certificate_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_certificate_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_certificate_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_certificate_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_certificate_type_descriptions",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    company_certificate_type_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_certificate_type_descriptions", x => new { x.id, x.short_name });
                    table.ForeignKey(
                        name: "fk_company_certificate_type_descriptions_company_certificate_t",
                        column: x => x.company_certificate_type_id,
                        principalSchema: "portal",
                        principalTable: "company_certificate_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_certificate_type_descriptions_languages_language_te",
                        column: x => x.short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "company_certificates",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    valid_till = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    company_certificate_type_id = table.Column<int>(type: "integer", nullable: false),
                    company_certificate_status_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_certificates", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_certificates_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_certificates_company_certificate_statuses_company_c",
                        column: x => x.company_certificate_status_id,
                        principalSchema: "portal",
                        principalTable: "company_certificate_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_company_certificates_company_certificate_types_company_cert",
                        column: x => x.company_certificate_type_id,
                        principalSchema: "portal",
                        principalTable: "company_certificate_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_company_certificates_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "portal",
                        principalTable: "documents",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_certificate_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "IN_REVIEW" },
                    { 2, "ACTIVE" },
                    { 3, "INACTVIE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_certificate_types",
                columns: new[] { "id", "label", "status" },
                values: new object[,]
                {
                    { 1, "AEO_CTPAT_Security_Declaration", 2 },
                    { 2, "ISO_9001", 2 },
                    { 3, "IATF_16949", 2 },
                    { 4, "ISO_14001_EMAS_or_national_certification", 2 },
                    { 5, "ISO_45001_OHSAS_18001_or_national_certification", 2 },
                    { 6, "ISO_IEC_27001", 2 },
                    { 7, "ISO_50001_or_national_certification", 2 },
                    { 8, "ISO_IEC_17025", 2 },
                    { 9, "ISO_15504_SPICE", 2 },
                    { 10, "B_BBEE_Certificate_of_South_Africa", 2 },
                    { 11, "IATF", 2 },
                    { 12, "TISAX", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_certificate_type_descriptions_company_certificate_t",
                schema: "portal",
                table: "company_certificate_type_descriptions",
                column: "company_certificate_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_certificate_type_descriptions_short_name",
                schema: "portal",
                table: "company_certificate_type_descriptions",
                column: "short_name");

            migrationBuilder.CreateIndex(
                name: "ix_company_certificates_company_certificate_status_id",
                schema: "portal",
                table: "company_certificates",
                column: "company_certificate_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_certificates_company_certificate_type_id",
                schema: "portal",
                table: "company_certificates",
                column: "company_certificate_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_certificates_company_id",
                schema: "portal",
                table: "company_certificates",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_certificates_document_id",
                schema: "portal",
                table: "company_certificates",
                column: "document_id");

            migrationBuilder.AddForeignKey(
                name: "fk_company_role_descriptions_languages_language_temp_id2",
                schema: "portal",
                table: "company_role_descriptions",
                column: "language_short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_country_long_names_languages_language_temp_id3",
                schema: "portal",
                table: "country_long_names",
                column: "short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_company_role_descriptions_languages_language_temp_id2",
                schema: "portal",
                table: "company_role_descriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_country_long_names_languages_language_temp_id3",
                schema: "portal",
                table: "country_long_names");

            migrationBuilder.DropTable(
                name: "company_certificate_type_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_certificates",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_certificate_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_certificate_types",
                schema: "portal");

            migrationBuilder.AddForeignKey(
                name: "fk_company_role_descriptions_languages_language_temp_id1",
                schema: "portal",
                table: "company_role_descriptions",
                column: "language_short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_country_long_names_languages_language_temp_id2",
                schema: "portal",
                table: "country_long_names",
                column: "short_name",
                principalSchema: "portal",
                principalTable: "languages",
                principalColumn: "short_name");
        }
    }
}
