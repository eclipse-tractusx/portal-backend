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
    public partial class CPLP1845AddServiceDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "service_details",
                schema: "portal",
                columns: table => new
                {
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_type_id = table.Column<int>(type: "integer", nullable: false),
                    technical_user_needed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_details", x => new { x.service_id, x.service_type_id });
                    table.ForeignKey(
                        name: "fk_service_details_offers_service_id",
                        column: x => x.service_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_service_details_service_types_service_type_id",
                        column: x => x.service_type_id,
                        principalSchema: "portal",
                        principalTable: "service_types",
                        principalColumn: "id");
                });

            migrationBuilder.Sql("INSERT INTO portal.service_details (service_id, service_type_id, technical_user_needed) SELECT service_id, service_type_id, service_type_id = 1 FROM portal.service_assigned_service_types");
            
            migrationBuilder.DropTable(
                name: "service_assigned_service_types",
                schema: "portal");
            
            migrationBuilder.CreateIndex(
                name: "ix_service_details_service_type_id",
                schema: "portal",
                table: "service_details",
                column: "service_type_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.Sql("INSERT INTO portal.service_assigned_service_types (service_id, service_type_id) SELECT service_id, service_type_id FROM portal.service_details");

            migrationBuilder.DropTable(
                name: "service_details",
                schema: "portal");

            migrationBuilder.CreateIndex(
                name: "ix_service_assigned_service_types_service_type_id",
                schema: "portal",
                table: "service_assigned_service_types",
                column: "service_type_id");
        }
    }
}
