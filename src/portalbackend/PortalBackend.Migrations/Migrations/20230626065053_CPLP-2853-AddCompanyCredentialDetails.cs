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

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class CPLP2853AddCompanyCredentialDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "use_case_descriptions",
                schema: "portal",
                columns: table => new
                {
                    use_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_use_case_descriptions", x => new { x.use_case_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_use_case_descriptions_languages_language_short_name",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name");
                    table.ForeignKey(
                        name: "fk_use_case_descriptions_use_cases_use_case_id",
                        column: x => x.use_case_id,
                        principalSchema: "portal",
                        principalTable: "use_cases",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "use_case_participation_status",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_use_case_participation_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_type_kinds",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_type_kinds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_ssi_details",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    verified_credential_type_id = table.Column<int>(type: "integer", nullable: false),
                    company_ssi_detail_status_id = table.Column<int>(type: "integer", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expiry_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_ssi_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_ssi_details_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_ssi_details_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "portal",
                        principalTable: "documents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_ssi_details_use_case_participation_status_use_case_pa",
                        column: x => x.company_ssi_detail_status_id,
                        principalSchema: "portal",
                        principalTable: "use_case_participation_status",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_ssi_details_verified_credential_types_verified_cred",
                        column: x => x.verified_credential_type_id,
                        principalSchema: "portal",
                        principalTable: "verified_credential_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_type_assigned_kinds",
                schema: "portal",
                columns: table => new
                {
                    verified_credential_type_id = table.Column<int>(type: "integer", nullable: false),
                    verified_credential_type_kind_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_type_assigned_kinds", x => new { x.verified_credential_type_id, x.verified_credential_type_kind_id });
                    table.ForeignKey(
                        name: "fk_verified_credential_type_assigned_kinds_verified_credential",
                        column: x => x.verified_credential_type_id,
                        principalSchema: "portal",
                        principalTable: "verified_credential_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_verified_credential_type_assigned_kinds_verified_credential1",
                        column: x => x.verified_credential_type_kind_id,
                        principalSchema: "portal",
                        principalTable: "verified_credential_type_kinds",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_type_assigned_use_cases",
                schema: "portal",
                columns: table => new
                {
                    verified_credential_type_id = table.Column<int>(type: "integer", nullable: false),
                    use_case_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_type_assigned_use_cases", x => new { x.verified_credential_type_id, x.use_case_id });
                    table.ForeignKey(
                        name: "fk_verified_credential_type_assigned_use_cases_use_cases_use_c",
                        column: x => x.use_case_id,
                        principalSchema: "portal",
                        principalTable: "use_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_verified_credential_type_assigned_use_cases_verified_creden",
                        column: x => x.verified_credential_type_id,
                        principalSchema: "portal",
                        principalTable: "verified_credential_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "document_types",
                columns: new[] { "id", "label" },
                values: new object[] { 14, "PRESENTATION" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "use_case_participation_status",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "ACTIVE" },
                    { 3, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "verified_credential_type_kinds",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "USE_CASE" },
                    { 2, "CERTIFICATE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "verified_credential_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "Traceability Framework" },
                    { 2, "Sustainability Framework" },
                    { 3, "Dismantler Certificate" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_company_id",
                schema: "portal",
                table: "company_ssi_details",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_company_ssi_detail_status_id",
                schema: "portal",
                table: "company_ssi_details",
                column: "company_ssi_detail_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_document_id",
                schema: "portal",
                table: "company_ssi_details",
                column: "document_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_verified_credential_type_id",
                schema: "portal",
                table: "company_ssi_details",
                column: "verified_credential_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_use_case_descriptions_language_short_name",
                schema: "portal",
                table: "use_case_descriptions",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_type_assigned_kinds_verified_credential",
                schema: "portal",
                table: "verified_credential_type_assigned_kinds",
                column: "verified_credential_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_type_assigned_kinds_verified_credential1",
                schema: "portal",
                table: "verified_credential_type_assigned_kinds",
                column: "verified_credential_type_kind_id");

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_type_assigned_use_cases_use_case_id",
                schema: "portal",
                table: "verified_credential_type_assigned_use_cases",
                column: "use_case_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_type_assigned_use_cases_verified_creden",
                schema: "portal",
                table: "verified_credential_type_assigned_use_cases",
                column: "verified_credential_type_id",
                unique: true);
           
            migrationBuilder.Sql(@"CREATE FUNCTION portal.is_company_credential_detail_use_case(company_credential_detail_id UUID)
                RETURNS BOOLEAN
                LANGUAGE plpgsql
                AS
                $$
                BEGIN
                    RETURN EXISTS (
                        SELECT 1
                        FROM portal.company_credential_details
                        WHERE Id = company_credential_detail_id
                            AND credential_type_id IN (
                                SELECT credential_type_id
                                FROM portal.credential_type_assigned_kinds
                                WHERE credential_type_kind_id = '1'
                            )
                    );
                END;
                $$");

            migrationBuilder.Sql(@"
                ALTER TABLE portal.credential_assigned_use_cases
                ADD CONSTRAINT CK_CredentialAssignedUseCase_CompanyCredentialDetail_UseCase 
                    CHECK (portal.is_company_credential_detail_use_case(company_credential_detail_id))");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE portal.credential_assigned_use_cases DROP CONSTRAINT IF EXISTS CK_CredentialAssignedUseCase_CompanyCredentialDetail_UseCase;");

            migrationBuilder.Sql("DROP FUNCTION portal.is_company_credential_detail_use_case;");

            migrationBuilder.DropTable(
                name: "company_ssi_details",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "use_case_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "verified_credential_type_assigned_kinds",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "verified_credential_type_assigned_use_cases",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "use_case_participation_status",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "verified_credential_type_kinds",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "verified_credential_types",
                schema: "portal");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "document_types",
                keyColumn: "id",
                keyValue: 14);
        }
    }
}
