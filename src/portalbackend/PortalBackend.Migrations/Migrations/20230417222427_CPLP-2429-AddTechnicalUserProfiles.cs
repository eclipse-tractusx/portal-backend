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
    public partial class CPLP2429AddTechnicalUserProfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "technical_user_needed",
                schema: "portal",
                table: "service_details");

            migrationBuilder.CreateTable(
                name: "technical_user_profiles",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_technical_user_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_technical_user_profiles_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "technical_user_profile_assigned_user_roles",
                schema: "portal",
                columns: table => new
                {
                    technical_user_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_technical_user_profile_assigned_user_roles", x => new { x.technical_user_profile_id, x.user_role_id });
                    table.ForeignKey(
                        name: "fk_technical_user_profile_assigned_user_roles_technical_user_p",
                        column: x => x.technical_user_profile_id,
                        principalSchema: "portal",
                        principalTable: "technical_user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_technical_user_profile_assigned_user_roles_user_roles_user_r",
                        column: x => x.user_role_id,
                        principalSchema: "portal",
                        principalTable: "user_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_technical_user_profile_assigned_user_roles_user_role_id",
                schema: "portal",
                table: "technical_user_profile_assigned_user_roles",
                column: "user_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_technical_user_profiles_offer_id",
                schema: "portal",
                table: "technical_user_profiles",
                column: "offer_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "technical_user_profile_assigned_user_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "technical_user_profiles",
                schema: "portal");

            migrationBuilder.AddColumn<bool>(
                name: "technical_user_needed",
                schema: "portal",
                table: "service_details",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
