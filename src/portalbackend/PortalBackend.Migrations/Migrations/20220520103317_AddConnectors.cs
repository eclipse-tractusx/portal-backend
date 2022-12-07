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
    public partial class AddConnectors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "connector_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connector_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "connector_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connector_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "connectors",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    connector_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    host_id = table.Column<Guid>(type: "uuid", nullable: true),
                    location_id = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connectors", x => x.id);
                    table.ForeignKey(
                        name: "fk_connectors_companies_host_id",
                        column: x => x.host_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_connectors_companies_provider_id",
                        column: x => x.provider_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_connectors_connector_statuses_status_id",
                        column: x => x.status_id,
                        principalSchema: "portal",
                        principalTable: "connector_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_connectors_connector_types_type_id",
                        column: x => x.type_id,
                        principalSchema: "portal",
                        principalTable: "connector_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_connectors_countries_location_temp_id1",
                        column: x => x.location_id,
                        principalSchema: "portal",
                        principalTable: "countries",
                        principalColumn: "alpha2code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "connector_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "ACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "connector_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "COMPANY_CONNECTOR" },
                    { 2, "CONNECTOR_AS_A_SERVICE" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_connectors_host_id",
                schema: "portal",
                table: "connectors",
                column: "host_id");

            migrationBuilder.CreateIndex(
                name: "ix_connectors_location_id",
                schema: "portal",
                table: "connectors",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_connectors_provider_id",
                schema: "portal",
                table: "connectors",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_connectors_status_id",
                schema: "portal",
                table: "connectors",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ix_connectors_type_id",
                schema: "portal",
                table: "connectors",
                column: "type_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "connectors",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "connector_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "connector_types",
                schema: "portal");
        }
    }
}
