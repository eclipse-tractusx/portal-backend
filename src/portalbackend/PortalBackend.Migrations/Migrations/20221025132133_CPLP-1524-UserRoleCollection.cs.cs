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
    public partial class CPLP1524UserRoleCollectioncs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "company_role_registration_data",
                schema: "portal",
                columns: table => new
                {
                    company_role_id = table.Column<int>(type: "integer", nullable: false),
                    is_registration_role = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_role_registration_data", x => x.company_role_id);
                    table.ForeignKey(
                        name: "fk_company_role_registration_data_company_roles_company_role_id",
                        column: x => x.company_role_id,
                        principalSchema: "portal",
                        principalTable: "company_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role_collections",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role_collections", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_role_assigned_role_collections",
                schema: "portal",
                columns: table => new
                {
                    company_role_id = table.Column<int>(type: "integer", nullable: false),
                    user_role_collection_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_role_assigned_role_collections", x => x.company_role_id);
                    table.ForeignKey(
                        name: "fk_company_role_assigned_role_collections_company_roles_compan",
                        column: x => x.company_role_id,
                        principalSchema: "portal",
                        principalTable: "company_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_company_role_assigned_role_collections_user_role_collection",
                        column: x => x.user_role_collection_id,
                        principalSchema: "portal",
                        principalTable: "user_role_collections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role_assigned_collections",
                schema: "portal",
                columns: table => new
                {
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_collection_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role_assigned_collections", x => new { x.user_role_id, x.user_role_collection_id });
                    table.ForeignKey(
                        name: "fk_user_role_assigned_collections_user_role_collections_user_r",
                        column: x => x.user_role_collection_id,
                        principalSchema: "portal",
                        principalTable: "user_role_collections",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_user_role_assigned_collections_user_roles_user_role_id",
                        column: x => x.user_role_id,
                        principalSchema: "portal",
                        principalTable: "user_roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_role_collection_descriptions",
                schema: "portal",
                columns: table => new
                {
                    user_role_collection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role_collection_descriptions", x => new { x.user_role_collection_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_user_role_collection_descriptions_languages_language_short_",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_role_collection_descriptions_user_role_collections_use",
                        column: x => x.user_role_collection_id,
                        principalSchema: "portal",
                        principalTable: "user_role_collections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_role_registration_data",
                columns: new[] { "company_role_id", "is_registration_role" },
                values: new object[,]
                {
                    { 1, true },
                    { 2, true },
                    { 3, true }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_roles",
                columns: new[] { "id", "label" },
                values: new object[] { 4, "OPERATOR" });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "user_role_collections",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { new Guid("1a24eca5-901f-4191-84a7-4ef09a894575"), "Operator" },
                    { new Guid("8cb12ea2-aed4-4d75-b041-ba297df3d2f2"), "CX Participant" },
                    { new Guid("a5b8b1de-7759-4620-9c87-6b6d74fb4fbc"), "Service Provider" },
                    { new Guid("ec428950-8b64-4646-b336-28af869b5d73"), "App Provider" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_role_assigned_role_collections",
                columns: new[] { "company_role_id", "user_role_collection_id" },
                values: new object[,]
                {
                    { 1, new Guid("8cb12ea2-aed4-4d75-b041-ba297df3d2f2") },
                    { 2, new Guid("ec428950-8b64-4646-b336-28af869b5d73") },
                    { 3, new Guid("a5b8b1de-7759-4620-9c87-6b6d74fb4fbc") },
                    { 4, new Guid("1a24eca5-901f-4191-84a7-4ef09a894575") }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_role_descriptions",
                columns: new[] { "company_role_id", "language_short_name", "description" },
                values: new object[,]
                {
                    { 4, "de", "Betreiber" },
                    { 4, "en", "Operator" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_role_registration_data",
                columns: new[] { "company_role_id", "is_registration_role" },
                values: new object[] { 4, false });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "user_role_collection_descriptions",
                columns: new[] { "language_short_name", "user_role_collection_id", "description" },
                values: new object[,]
                {
                    { "de", new Guid("1a24eca5-901f-4191-84a7-4ef09a894575"), "Betreiber" },
                    { "en", new Guid("1a24eca5-901f-4191-84a7-4ef09a894575"), "Operator" },
                    { "de", new Guid("8cb12ea2-aed4-4d75-b041-ba297df3d2f2"), "CX Netzwerkteilnehmer" },
                    { "en", new Guid("8cb12ea2-aed4-4d75-b041-ba297df3d2f2"), "CX Participant" },
                    { "de", new Guid("a5b8b1de-7759-4620-9c87-6b6d74fb4fbc"), "Dienstanbieter" },
                    { "en", new Guid("a5b8b1de-7759-4620-9c87-6b6d74fb4fbc"), "Service Provider" },
                    { "de", new Guid("ec428950-8b64-4646-b336-28af869b5d73"), "Softwareanbieter" },
                    { "en", new Guid("ec428950-8b64-4646-b336-28af869b5d73"), "App Provider" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_role_assigned_role_collections_user_role_collection",
                schema: "portal",
                table: "company_role_assigned_role_collections",
                column: "user_role_collection_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_role_assigned_collections_user_role_collection_id",
                schema: "portal",
                table: "user_role_assigned_collections",
                column: "user_role_collection_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_collection_descriptions_language_short_name",
                schema: "portal",
                table: "user_role_collection_descriptions",
                column: "language_short_name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_role_assigned_role_collections",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_role_registration_data",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "user_role_assigned_collections",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "user_role_collection_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "user_role_collections",
                schema: "portal");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_role_descriptions",
                keyColumns: new[] { "company_role_id", "language_short_name" },
                keyValues: new object[] { 4, "de" });

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_role_descriptions",
                keyColumns: new[] { "company_role_id", "language_short_name" },
                keyValues: new object[] { 4, "en" });

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_roles",
                keyColumn: "id",
                keyValue: 4);
        }
    }
}
