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
    public partial class CPLP1803AddServiceTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_service_provider_company_details_company_id",
                schema: "portal",
                table: "service_provider_company_details");

            migrationBuilder.RenameTable(
                name: "service_provider_company_details",
                newName: "provider_company_details",
                schema: "portal");

            migrationBuilder.CreateIndex(
                name: "ix_provider_company_details_company_id",
                schema: "portal",
                table: "provider_company_details",
                column: "company_id",
                unique: true);

            migrationBuilder.CreateTable(
                name: "service_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service_assigned_service_types",
                schema: "portal",
                columns: table => new
                {
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_type_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_assigned_service_types", x => new { x.service_id, x.service_type_id });
                    table.ForeignKey(
                        name: "fk_service_assigned_service_types_offers_service_id",
                        column: x => x.service_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_service_assigned_service_types_service_types_service_type_id",
                        column: x => x.service_type_id,
                        principalSchema: "portal",
                        principalTable: "service_types",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "service_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "DATASPACE_SERVICE" },
                    { 2, "CONSULTANCE_SERVICE" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_service_assigned_service_types_service_type_id",
                schema: "portal",
                table: "service_assigned_service_types",
                column: "service_type_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "provider_company_details",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "service_assigned_service_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "service_types",
                schema: "portal");

            migrationBuilder.CreateTable(
                name: "service_provider_company_details",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    auto_setup_url = table.Column<string>(type: "text", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_provider_company_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_provider_company_details_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_service_provider_company_details_company_id",
                schema: "portal",
                table: "service_provider_company_details",
                column: "company_id",
                unique: true);
        }
    }
}
